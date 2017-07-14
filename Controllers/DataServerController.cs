//using LitJson2;
using LitJson2;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using ZeroStartAPI.Models;



namespace ZeroStartAPI.Controllers
{
    public class DataServerController : ApiController
    {
        utility ut = new utility();
        private string GetWebClientIp(HttpRequestMessage request = null)
        {
            request = request ?? Request;

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }

        [HttpGet]
        [HttpPost]
        [Route("wxRetCallFunc")]
        public string wxRetCallFunc()
        {
            string strRetString = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg >< ![CDATA[OK]] ></ return_msg></xml>";
            byte[] byts = new byte[HttpContext.Current.Request.InputStream.Length];
            System.Web.HttpContext.Current.Request.InputStream.Position = 0;
            HttpContext.Current.Request.InputStream.Read(byts, 0, byts.Length);
            string req = Encoding.UTF8.GetString(byts);
            byte[] array = Encoding.UTF8.GetBytes(req);
            MemoryStream ms = new MemoryStream(array);
            StreamReader sr = new StreamReader(ms);
            string html = sr.ReadToEnd();
            var xml = new XmlDocument();
            xml.LoadXml(req);

            var root = xml.DocumentElement;
            StringReader stram = new StringReader(html);
            XmlTextReader reader = new XmlTextReader(stram);
            System.Data.DataSet ds = new System.Data.DataSet();
            ds.ReadXml(reader);

            if (ds.Tables.Count > 0)
            {
                string appid = ds.Tables[0].Rows[0]["appid"].ToString();
                string out_trade_no = ds.Tables[0].Rows[0]["out_trade_no"].ToString();
                if (appid.CompareTo(ut.getAppSetting(0)) == 0)
                {
                    using (dataserverEntities db = new dataserverEntities())
                    {
                        payinfo cc = new payinfo();
                        //取资料主档信息
                        cc = db.payinfo.Where(vv => vv.out_trade_no == out_trade_no).FirstOrDefault();
                        if (cc == null)
                        {
                            strRetString = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg >< ![CDATA[订单号错误]] ></ return_msg></xml>";
                            return strRetString;
                        }
                        else
                        {
                            if (cc.transaction_id != null && cc.transaction_id.Length > 0)
                            {
                                return strRetString;
                            }
                        }
                    }
                }
                if (appid.CompareTo(ut.getAppSetting(11)) == 0)
                {
                    using (organizeEntities db = new organizeEntities())
                    {
                        payinfo cc = new payinfo();
                        //取资料主档信息
                        cc = db.payinfo.Where(vv => vv.out_trade_no == out_trade_no).FirstOrDefault();
                        if (cc == null)
                        {
                            strRetString = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg >< ![CDATA[订单号错误]] ></ return_msg></xml>";
                            return strRetString;
                        }
                        else
                        {
                            if (cc.transaction_id != null && cc.transaction_id.Length > 0)
                            {
                                return strRetString;
                            }
                        }
                    }
                }

                var dic = new Dictionary<string, string> { };
                for (int ii = 0; ii <= ds.Tables[0].Columns.Count - 1; ii++)
                {
                    if (ds.Tables[0].Columns[ii].ToString() != "sign")
                        dic.Add(ds.Tables[0].Columns[ii].ToString(), ds.Tables[0].Rows[0].ItemArray[ii].ToString());
                }
                var sin = ut.GetSignString(dic);
                if (sin == ds.Tables[0].Rows[0]["sign"].ToString())
                {
                    string return_code = ds.Tables[0].Rows[0]["return_code"].ToString();
                    if (return_code.ToUpper() == "SUCCESS")
                    {
                        string result_code = ds.Tables[0].Rows[0]["result_code"].ToString();
                        string transaction_id = ds.Tables[0].Rows[0]["transaction_id"].ToString();
                        if (result_code.ToUpper() == "SUCCESS")
                        {
                            if (appid.CompareTo(ut.getAppSetting(0)) == 0)
                            {
                                using (dataserverEntities db = new dataserverEntities())
                                {
                                    payinfo cc = new payinfo();
                                    //取资料主档信息
                                    cc = db.payinfo.Where(vv => vv.out_trade_no == out_trade_no).FirstOrDefault();
                                    //取资料主档信息
                                    cc.transaction_id = transaction_id;
                                    db.SaveChanges();
                                }
                            }
                            if (appid.CompareTo(ut.getAppSetting(11)) == 0)
                            {
                                using (organizeEntities db = new organizeEntities())
                                {
                                    payinfo cc = new payinfo();
                                    //取资料主档信息
                                    cc = db.payinfo.Where(vv => vv.out_trade_no == out_trade_no).FirstOrDefault();
                                    //取资料主档信息
                                    cc.transaction_id = transaction_id;
                                    db.SaveChanges();
                                }
                            }

                        }
                    }
                }
                else
                {
                    strRetString = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg >< ![CDATA[签名失败]] ></ return_msg></xml>";
                }



            }
            else
            {
                strRetString = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg >< ![CDATA[数据格式错误]] ></ return_msg></xml>";
            }
            return strRetString;
        }
        [HttpPost]
        [Route("questionnaire")]
        public RetMessage<string> questionnaire()
        {
            //string tokenId, [System.Web.Mvc.Bind(Include = "name,age,company,position,mobile,email,area")] questionnaire qn
            byte[] byts = new byte[HttpContext.Current.Request.InputStream.Length];
            System.Web.HttpContext.Current.Request.InputStream.Position = 0;
            HttpContext.Current.Request.InputStream.Read(byts, 0, byts.Length);
            string req = Encoding.UTF8.GetString(byts);
            byte[] array = Encoding.UTF8.GetBytes(req);
            MemoryStream ms = new MemoryStream(array);
            StreamReader sr = new StreamReader(ms);
            string html = sr.ReadToEnd();

            JsonData data = JsonMapper.ToObject(html);
            string tokenId = data["tokenId"].ToString();
            RetMessage<string> rm = new RetMessage<string>();
            string userid = ut.getUserIDByToken(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (dataserverEntities db = new dataserverEntities())
            {
                syslog sl = new syslog();
                sl.logcontent = html;
                db.syslog.Add(sl);
                db.SaveChanges();
                int dataid = 0;
                if (data["dataid"] != null)
                    dataid = int.Parse(data["dataid"].ToString());
                questionnaire cc = new questionnaire();
                cc.name = data["name"].ToString();
                cc.age = data["age"].ToString();
                cc.company = data["company"].ToString();
                cc.position = data["position"].ToString();
                cc.mobile = data["mobile"].ToString();
                cc.email = data["email"].ToString();
                cc.area = data["area"].ToString();
                cc.userid = userid;
                cc.dataid = dataid;
                db.questionnaire.Add(cc);
                db.SaveChanges();
            }
            rm.RetCode = 0;
            return rm;
        }

        [Route("syDataCatalogList")]
        public RetMessage<datacatalog> GetSydatacatalog()
        {
            RetMessage<datacatalog> rm = new RetMessage<datacatalog>();
            List<datacatalog> ListRetArrys = new List<datacatalog>();
            string[] sycatalogs = ut.getAppSetting(10).Split('|');
            //string[] sycatalogs = ut.getAppSetting(14).Split('|');
            //string[] organizationids = ut.getAppSetting(15).Split('|');
            
            List<int> CatalogIDArry = new List<int>();
            foreach (var xx in sycatalogs)
            {
                if (xx != null && xx != "")
                    CatalogIDArry.Add(int.Parse(xx));
            }
           
            using (dataserverEntities db = new dataserverEntities())
            {
                var ups = db.datacatalog.Where(x => CatalogIDArry.Contains(x.sid)).ToList();
                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;

        }

        [HttpGet]
        [Route("makeOrder")]
        public RetMessage<Order> makeOrder(string tokenId, int dataId)
        {
            RetMessage<Order> rm = new RetMessage<Order>();
            if (tokenId != null)
            {
                string useridandopenid = ut.getUserIDAndOpenIDByToken(tokenId);


                if (useridandopenid == null)
                {
                    //错误返回;
                    rm.RetCode = -7;
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                }
                else
                {
                    string[] buff = useridandopenid.Split('|');
                    string userid = buff[0];
                    string openid = buff[1];
                    string strcode = null;
                    string total_fee = null;
                    using (dataserverEntities db = new dataserverEntities())
                    {
                        //取资料主档信息
                        datainfo cc = new datainfo();
                        cc = db.datainfo.Find(dataId);
                        if (cc == null)
                        {
                            rm.RetCode = -6;
                            rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                            return rm;
                        }
                        strcode = cc.title;
                        total_fee = cc.amount;
                    }
                    string wx_appid = ut.getAppSetting(0);
                    string wx_mch_id = ut.getAppSetting(1);
                    string wx_nonce_str = ut.getRandomString(20);
                    byte[] buffer = Encoding.UTF8.GetBytes(strcode);
                    string wx_body = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    string wx_out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmss") + ut.getRandomString(10);
                    string wx_total_fee = ((int)(decimal.Parse(total_fee) * 100)).ToString();//unit:fen
                                                                                             //string wx_spbill_create_ip = GetWebClientIp();
                                                                                             //string wx_spbill_create_ip = "59.110.164.24";
                    string wx_spbill_create_ip = ut.getAppSetting(4);
                    string wx_notify_url = ut.getAppSetting(3);
                    string wx_trade_type = ut.getAppSetting(5);
                    var dic = new Dictionary<string, string> {
                    {"appid",wx_appid},
                    {"body",wx_body},
                    {"mch_id",wx_mch_id},
                    {"nonce_str",wx_nonce_str },
                    {"notify_url",wx_notify_url },
                    {"openid",openid},
                    {"out_trade_no",wx_out_trade_no },
                    {"spbill_create_ip",wx_spbill_create_ip },
                    {"total_fee",wx_total_fee },
                    {"trade_type",wx_trade_type }
                };
                    dic.Add("sign", ut.GetSignString(dic));
                    var sb = new StringBuilder();
                    sb.Append("<xml>");
                    foreach (var d in dic)
                    {
                        sb.Append("<" + d.Key + ">" + d.Value + "</" + d.Key + ">");
                    }
                    sb.Append("</xml>");
                    string response = ut.CreatedPostHttpResponse("https://api.mch.weixin.qq.com/pay/unifiedorder", sb);
                    byte[] array = Encoding.UTF8.GetBytes(response);
                    MemoryStream ms = new MemoryStream(array);
                    StreamReader sr = new StreamReader(ms);
                    string html = sr.ReadToEnd();
                    var xml = new XmlDocument();
                    xml.LoadXml(response);
                    var root = xml.DocumentElement;
                    StringReader stram = new StringReader(html);
                    XmlTextReader reader = new XmlTextReader(stram);
                    System.Data.DataSet ds = new System.Data.DataSet();
                    ds.ReadXml(reader);
                    string return_code = ds.Tables[0].Rows[0]["return_code"].ToString();
                    if (return_code.ToUpper() == "SUCCESS")
                    {
                        using (dataserverEntities db = new dataserverEntities())
                        {
                            string strNow = DateTime.Now.ToString("u");
                            payinfo pi = new payinfo();
                            pi.dataid = dataId;
                            pi.paydate = strNow.Substring(0, 10);
                            pi.paytime = strNow.Substring(11, 8);
                            //(int)decimal.Parse(total_fee)*100
                            pi.paymount = total_fee;//total_fee
                            pi.out_trade_no = wx_out_trade_no;
                            pi.userid = userid;
                            db.payinfo.Add(pi);
                            db.SaveChanges();

                        }
                        //wx_out_trade_no
                        var res = new Dictionary<string, string> {
                    {"appID",wx_appid},
                    { "nonceStr",ds.Tables[0].Rows[0]["nonce_str"].ToString()},
                    { "package","prepay_id="+ ds.Tables[0].Rows[0]["prepay_id"].ToString()},
                    { "signType","MD5"},
                    { "timeStamp",ut.GetTimeStamp()},
                    };
                        res.Add("paySign", ut.GetSignString(res));
                        List<Order> orderArray = new List<Order>();
                        Order order = new Order();
                        order.timeStamp = res["timeStamp"];
                        order.nonceStr = res["nonceStr"];
                        order.package = res["package"];
                        order.paySign = res["paySign"];
                        order.wx_out_trade_no = wx_out_trade_no;
                        rm.RetCode = 0;
                        orderArray.Add(order);
                        rm.data = orderArray;
                    }
                    else
                    {
                        rm.RetCode = -2;
                        rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                    }
                }

            }
            else
            {
                rm.RetCode = -3;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
            }
            return rm;
        }
        [Route("getCurGift")]
        public RetMessage<string> getCurGift()
        {
            RetMessage<string> rm = new RetMessage<string>();

            return rm;
        }
        /*
         * 公益榜第一的昵称和头像，当前用户的分享数，以及当前用户前一名用户及后一名用户的昵称、头像、和当前用户的分享差距，当前一期公益榜的开始时间及结束时间
         */
        [Route("getCurCommonweal")]
        public RetMessage<ViewRankListModel> getCurCommonweal(string strTokenid)
        {
            RetMessage<ViewRankListModel> rm = new RetMessage<ViewRankListModel>();
            rm.RetCode = -1;
            string userid = ut.getUserIDByToken(strTokenid);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;
            }
            List<ViewRankListModel> ltvrlm = new List<ViewRankListModel>();
            using (dataserverEntities db = new dataserverEntities())
            {
                ViewRankListModel vrlm = new ViewRankListModel();
                var rc = db.rankinglist.Where(x => x.cur == 1).FirstOrDefault();
                if (rc != null)
                {
                    vrlm.begindate = rc.begindate;
                    vrlm.rankid = rc.rankid;
                    vrlm.enddate = rc.enddate+" "+"23:59:59";
                    vrlm.giftpicurl = rc.giftpicurl;
                    vrlm.gifttitle = rc.gifttitle;
                    vrlm.giftcontent = rc.giftcontent;
                    vrlm.cur = rc.cur;
                    var lCountArry = (from p in db.sharpcount
                                      where p.sharpdate.CompareTo (vrlm.begindate)>= 0 && p.sharpdate.CompareTo(vrlm.enddate) <= 0 
                                      group p by p.userid into g
                                      // orderby g.Sum(p => p.scount) descending
                                      
                                      select new KeyCountSharp()
                                      {
                                          Key = g.Key,
                                          Total = (int)g.Sum(p => p.scount)
                                      }).ToList();
                    lCountArry= lCountArry.OrderByDescending(p => p.Total).ToList();
                    if (lCountArry.Count > 0)
                    {
                        List<string> ltusers = new List<string>();
                        List<userinfo> ltUserinfo = new List<userinfo>();
                        List<rankingInfoList> ltRankingInfoList = new List<rankingInfoList>();
                        int ii = 0;
                        int iHavUSer = -1;
                        int curUserSharp = 0;
                        foreach(var aa in lCountArry)
                        {
                            ii += 1;
                            rankingInfoList rilWiner = new rankingInfoList();
                            rilWiner.NickName = aa.Key;
                            rilWiner.SharpCount = aa.Total;
                            rilWiner.Position = ii;
                            rilWiner.IFlag = 1;
                            if (aa.Key == userid)
                            {
                                iHavUSer = ii;
                                curUserSharp = aa.Total;
                            }
                            else
                            {
                                ltusers.Add(rilWiner.NickName);
                            }
                            if (ii <= 3)
                            {
                                ltRankingInfoList.Add(rilWiner);
                            }
                            
                        }
                        rankingInfoList curUser = new rankingInfoList();
                        curUser.NickName = userid;
                        curUser.Position = iHavUSer;
                        curUser.SharpCount = curUserSharp;
                        if (iHavUSer < 0)
                        {
                            curUser.SharpCount = 0;
                            curUser.Position = -1;
                        }
                        
                        curUser.IFlag = 0;
                        ltusers.Add(userid);
                        ltRankingInfoList.Add(curUser);

                        vrlm.ltRankingInfo = ltRankingInfoList;

                        var usersArry = from users in db.userinfo
                                        where ltusers.Contains(users.userid)
                                        select users;
                        if (usersArry != null && usersArry.ToList().Count > 0)
                        {
                            ltUserinfo = usersArry.ToList();
                        }
                        foreach (var dd in ltUserinfo)
                        {
                            foreach(var cc in vrlm.ltRankingInfo)
                            {
                                if (cc.NickName == dd.userid)
                                {
                                    cc.PicAddress = dd.address;
                                    cc.NickName = dd.username;
                                }
                            }
                        }
                    }


                }
                ltvrlm.Add(vrlm);
            }
            rm.RetCode = 0;
            /*
            if(ltvrlm.Count>0)
            {
                for(int cc=0;cc< ltvrlm[0].ltRankingInfo.Count; cc++)
                {
                    if(cc>2&&cc!= ltvrlm[0].ltRankingInfo.Count - 1)
                    {
                        
                    }
                }
            }*/
               
           
            rm.data = ltvrlm;
            return rm;
        }
        [Route("getMoreCommonweal")]
        public RetMessage<ViewRankListMoreModel> getMoreCommonweal()
        {
            RetMessage<ViewRankListMoreModel> rm = new RetMessage<ViewRankListMoreModel>();
            List<ViewRankListMoreModel> vrlmm = new List<ViewRankListMoreModel>();
            using (dataserverEntities db = new dataserverEntities())
            {
               
                var rc = db.rankinglist.Where(x => x.cur != 1).ToList();
                ViewRankListMoreModel vrlm = new ViewRankListMoreModel();
                foreach(var dd in rc)
                {
                    vrlm.begindate = dd.begindate;
                    vrlm.rankid = dd.rankid;
                    vrlm.enddate = dd.enddate;
                    vrlm.giftpicurl = dd.giftpicurl;
                    vrlm.gifttitle = dd.gifttitle;
                    vrlm.giftcontent = dd.giftcontent;
                    vrlm.cur = dd.cur;
                    vrlm.NikeName =dd.winner;
                    vrlmm.Add(vrlm);
                }
                List<userinfo> ltUserinfo = new List<userinfo>();
                var usersArry = from users in db.userinfo
                                where vrlm.NikeName.Contains(users.userid)
                                select users;
                if (usersArry != null && usersArry.ToList().Count > 0)
                {
                    ltUserinfo = usersArry.ToList();
                }
                foreach(var dd in ltUserinfo)
                {
                    foreach(var cc in vrlmm)
                    {
                        if (cc.NikeName == dd.userid)
                        {
                            cc.NikeName = dd.userid;
                            cc.UserPic = dd.address;
                        }
                    }
                }
               

            }
            rm.data = vrlmm;
            rm.RetCode = 0;
            return rm;
        }
        [HttpPost]
        [Route("putDataSharp")]
        public RetMessage<string> putDataSharp(string strTokenid, int  dataid)
        {
            RetMessage<string> rm = new RetMessage<string>();
            rm.RetCode = -1;
            string userid = ut.getUserIDByToken(strTokenid);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (dataserverEntities db = new dataserverEntities())
            {
                sharpcount rt = new sharpcount();
                rt.scount = 1;
                rt.dataid = dataid;
                rt.userid = userid;
                string strNow = DateTime.Now.ToString("u");
                rt.sharpdate = strNow.Substring(0, 10);
                rt.sharptime = strNow.Substring(11, 8);
                db.sharpcount.Add(rt);
                db.SaveChanges();
            }
            rm.RetCode = 0;
            return rm;
        }
        [Route("PushUserPic")]
        public RetMessage<string> GetUserPic(string strTokenid, string nickname ,string userPicUrl)
        {
            RetMessage<string> rm = new RetMessage<string>();
            rm.RetCode = 0;
            string userid = ut.getUserIDByToken(strTokenid);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (dataserverEntities db = new dataserverEntities())
            {
                userinfo cc = new userinfo();
                //取资料主档信息
                cc = db.userinfo.Where(vv => vv.userid == userid).FirstOrDefault();
                //取资料主档信息
                cc.address = userPicUrl;
                cc.username = nickname;
                db.SaveChanges();
                rm.RetCode = 0;
            }

            return rm;
        }

        /// <summary>
        /// 读取用户信息
        /// </summary>
        [Route("RetUserID")]
        public async Task<RetMessage<Token>> Getuseridbywxlogin(string js_code)
        {
            var l_js_code = "0311zLDk1E5afi09g9Ck14DXDk11zLD9";
            if (js_code != null)
                l_js_code = js_code;

            string APPID = ut.getAppSetting(0);
            string SECRET = ut.getAppSetting(6);
            string grant_type = ut.getAppSetting(7);
            string strurl = @" https://api.weixin.qq.com/sns/jscode2session?appid=" + APPID + "&secret=" + SECRET + "&js_code=" + l_js_code + "&grant_type=" + grant_type;
            HttpClient hc = new HttpClient();
            //Url httpUrl = new Url(strurl);
            HttpResponseMessage hrm = await hc.GetAsync(strurl);
            RetMessage<Token> rm = new RetMessage<Token>();
            if (hrm.IsSuccessStatusCode)
            {

                string jsonresult = await hrm.Content.ReadAsStringAsync();
                if (jsonresult.Contains("errcode"))
                {
                    rm.RetCode = -4;
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                }
                else
                {
                    JsonData data = JsonMapper.ToObject(jsonresult);


                    //取到openid，转换token
                    List<Token> tokenArray = new List<Token>();
                    Token token = new Token();
                    token.tokenMake(data["openid"].ToString(), data["session_key"].ToString(),1);
                    rm.RetCode = 0;
                    tokenArray.Add(token);
                    rm.data = tokenArray;

                }

            }
            else
            {
                rm.RetCode = -5;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
            }
            return rm;
        }
        [HttpGet]
        [Route("syc")]
        public IEnumerable<datacatalog> syc()
        {
            List<datacatalog> ListRetArrys = new List<datacatalog>();
            using (dataserverEntities db = new dataserverEntities())
            {

                var ups = from tv_up in db.datacatalog
                          where tv_up.sid > 0
                          select tv_up;

                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }
            }
            return ListRetArrys;

        }
        [Route("DataCatalogList")]
        public RetMessage<datacatalog> Getdatacatalog()
        {
            RetMessage<datacatalog> rm = new RetMessage<datacatalog>();
            List<datacatalog> ListRetArrys = new List<datacatalog>();
            using (dataserverEntities db = new dataserverEntities())
            {
                var ups = from tv_up in db.datacatalog
                              //  where tv_up.userid == strUserID
                          select tv_up;
                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;

        }
        [HttpGet]
        [Route("syd")]
        public List <ViewDataInfoModel> syd(int iflag,string queryKey,string strTokenid,int catalogid)
        {
            List<ViewDataInfoModel> ListRetArrys = new List<ViewDataInfoModel>();
            List<datainfo> listDataInfos = new List<datainfo>();
            List<r_datacata> listDataCatalogs = new List<r_datacata>();
            List<datacatalog> ltDataCatlogs = new List<datacatalog>();
            List<KeyCount> lCountArry;
            //取专家和机构的catalogID
            string[] expertids = ut.getAppSetting(14).Split('|');
            string[] organizationids = ut.getAppSetting(15).Split('|');

            List<int> LtExpertidsSid = new List<int>();
            List<int> LtOrganizationidsSid = new List<int>();
            List<int> LtAllSid = new List<int>();

            List<int> LtExpertidsCatalogid = new List<int>();
            List<int> LtOrganizationidsCatalogid = new List<int>();
            List<int> LtAllCatalogid = new List<int>();


            List<int> LtCataLogs = new List<int>();
            List<int> edataid = new List<int>();
            List<int> odataid = new List<int>();

            List<int> LtExpertidsDataID = new List<int>();
            List<int> LtOrganizationidsDataID = new List<int>();
            List<int> LtAllDataID = new List<int>();

            foreach (var aa in expertids)
            {
                if (aa != "")
                {
                    LtExpertidsSid.Add(int.Parse(aa));
                    LtAllSid.Add(int.Parse(aa));
                }
            }
            foreach (var aa in organizationids)
            {
                if (aa != "")
                {
                    LtOrganizationidsSid.Add(int.Parse(aa));
                    LtAllSid.Add(int.Parse(aa));
                }
            }
          
        
            using (dataserverEntities db = new dataserverEntities())
            {
                IQueryable<datainfo> ups;
                if (iflag == 1)
                {
                    if (queryKey != null)
                    {
                        ups = from a in db.datainfo
                              join b in db.r_datacata on a.dataid equals b.dataid
                              join c in db.datacatalog on b.catalogid equals c.catalogid
                              where (a.describe.Contains(queryKey) || a.title.Contains(queryKey) || c.catainfo.Contains(queryKey))
                              select a;
                    }
                    else
                    {
                        ups = from a in db.datainfo
                              join b in db.r_datacata on a.dataid equals b.dataid
                              join c in db.datacatalog on b.catalogid equals c.catalogid
                              select a;
                    }
                    querylog qlog = new querylog();
                    string strNow = DateTime.Now.ToString("u");
                    qlog.userid = ut.getUserIDByToken(strTokenid);
                    qlog.qdate = strNow.Substring(0, 10);
                    qlog.qtime = strNow.Substring(11, 8);
                    qlog.strkey = queryKey;
                    db.querylog.Add(qlog);
                    db.SaveChanges();

                }
                else
                {
                    if (iflag == 2)
                    {
                         ups = from a in db.datainfo
                                  join b in db.r_datacata on a.dataid equals b.dataid
                                  join c in db.datacatalog on b.catalogid equals c.catalogid
                                  where c.catalogid == catalogid
                               select a;
                    }
                    else
                    {
                        //取首页资料
                        ups = from tv_up in db.datainfo
                              where tv_up.type != null
                              select tv_up;
                    }
                }
               
                if (ups != null && ups.ToList().Count > 0)
                {
                    listDataInfos = ups.ToList();

                }
                //取特定所有目录
                var lcatalogsArry = from lcatalogs in db.datacatalog
                                    where LtAllSid.Contains(lcatalogs.sid)
                                    select lcatalogs;
                if (lcatalogsArry != null && lcatalogsArry.ToList().Count > 0)
                {
                    ltDataCatlogs = lcatalogsArry.ToList();
                }
                foreach (var aa in ltDataCatlogs)
                {
                    foreach (var a1 in LtExpertidsSid)
                    {
                        if (aa.sid == a1)
                        {
                            LtExpertidsCatalogid.Add(aa.catalogid);

                        }
                    }
                    foreach (var a2 in LtOrganizationidsSid)
                    {
                        if (aa.sid == a2)
                        {
                            LtOrganizationidsCatalogid.Add(aa.catalogid);

                        }
                    }
                    LtAllCatalogid.Add(aa.catalogid);
                }
               
                //取目录资料对应关系,根据所有特定目录
                var l_r_datacata = from r_datacata in db.r_datacata
                          where LtAllCatalogid.Contains(r_datacata.catalogid)
                          select r_datacata;


                if (l_r_datacata != null && l_r_datacata.ToList().Count > 0)
                {
                    listDataCatalogs = l_r_datacata.ToList();
                }
             
                foreach (var cc in listDataCatalogs)
                {
                    if(LtExpertidsCatalogid.Contains(cc.catalogid))
                        edataid.Add(cc.dataid);
                    if (LtOrganizationidsCatalogid.Contains(cc.catalogid))
                        odataid.Add(cc.dataid);
                }
                //取点击率，按dataid分组
                lCountArry = (from p in db.readcount
                              group p by p.dataid into g
                              //orderby g.Sum(p => p.rcount) descending
                              select new KeyCount()
                              {
                                  Key = (int)(g.Key),
                                  Total = (int)g.Sum(p => p.rcount)
                              }).ToList();
                lCountArry = lCountArry.OrderByDescending(p => p.Total).ToList();
            }
            List<int> strDataID = new List<int>();
            for (int ii = 0; ii <= 29 && ii <= lCountArry.Count(); ii++)
            {
                strDataID.Add(lCountArry[ii].Key);
            }
           
            foreach (var vdim in listDataInfos)
            {
                ViewDataInfoModel vdm = new ViewDataInfoModel();
                vdm.dataid = vdim.dataid;
                vdm.title = vdim.title;
                vdm.describe = vdim.describe;
                vdm.amount = vdim.amount;
                vdm.url = vdim.url;
                vdm.signature = vdim.signature;
                vdm.type = vdim.type;
                vdm.picurs = vdim.picurs;
                vdm.picurd = vdim.picurd;
                vdm.ddate = vdim.ddate;
                vdm.dtime = vdim.dtime;

                string IconFlag = "";
                DateTime dt = DateTime.Now.AddMonths(-1);
                string strNow = dt.ToString("u").Substring(0, 10);
                if (vdm.ddate.CompareTo(strNow) >= 0)
                {
                    IconFlag += "1" + "|";
                }

                if (strDataID.Contains(vdim.dataid) == true)
                {
                    IconFlag += "2" + "|";
                }

                if (edataid.Contains(vdim.dataid)==true)
                {
                    IconFlag += "3" + "|";
                }
                if (odataid.Contains(vdim.dataid) == true)
                {
                    IconFlag += "4" + "|";
                }
                vdm.IconFlag = IconFlag.Trim('|');
                ListRetArrys.Add(vdm);
            }

            return ListRetArrys;

        }

       
        //查询结果内容
        [Route("DataInfoListQuery")]
        public RetMessage<ViewDataInfoModel> GetDataInfoByQuery(string queryKey, string strTokenid)
        {
            RetMessage<ViewDataInfoModel> rm = new RetMessage<ViewDataInfoModel>();
            rm.data = syd(1, queryKey, strTokenid,0);
            rm.RetCode = 0;
            return rm;
        }
        [Route("DataInfoList")]
        public RetMessage<ViewDataInfoModel> GetDataInfo(int CatalogID)
        {
            RetMessage<ViewDataInfoModel> rm = new RetMessage<ViewDataInfoModel>();
            rm.data = syd(2, "", "", CatalogID);

            rm.RetCode = 0;
            return rm;
        }
        [Route("DataInfoList")]
        public RetMessage<ViewDataInfoModel> GetDataInfobyID(int dataID, string strTokenid)
        {
            RetMessage<ViewDataInfoModel> rm = new RetMessage<ViewDataInfoModel>();
            int ldataID = dataID;
            string strUserId = ut.getUserIDByToken(strTokenid);
            if (strUserId == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;
            }
            List<KeyCount> lCountArry;
            string IconFlag = "";
            List<ViewDataInfoModel> vdmArray = new List<ViewDataInfoModel>();
            ViewDataInfoModel vdm = new ViewDataInfoModel();
            DateTime dt = DateTime.Now.AddMonths(-1);
            string strNow = dt.ToString("u").Substring(0, 10);
            using (dataserverEntities db = new dataserverEntities())
            {
                int lcount = int.Parse(ut.getAppSetting(9));
                //取资料主档信息
                datainfo cc = new datainfo();
                cc = db.datainfo.Find(ldataID);
                if (cc != null && cc.dataid > 0)
                {
                    vdm.amount = cc.amount;
                    vdm.dataid = cc.dataid;
                    vdm.describe = cc.describe;
                    vdm.signature = cc.signature;
                    vdm.title = cc.title;
                    vdm.type = cc.type;
                    vdm.url = cc.url;
                    vdm.Buyed = -1;
                    vdm.picurd = cc.picurd;
                    vdm.picurs = cc.picurs;
                    vdm.ddate = cc.ddate;
                    vdm.dtime = cc.dtime;

                    //取点击率，按dataid分组
                    lCountArry = (from p in db.readcount
                                  group p by p.dataid into g
                                  orderby g.Sum(p => p.rcount) descending
                                  select new KeyCount()
                                  {
                                      Key = (int)(g.Key),
                                      Total = (int)g.Sum(p => p.rcount)
                                  }).ToList();
                    lCountArry = lCountArry.OrderByDescending(p => p.Total).ToList();
                    List<int> strDataID = new List<int>();
                    for (int ii = 0; ii <= 29 && ii <= lCountArry.Count(); ii++)
                    {
                        strDataID.Add(lCountArry[ii].Key);
                    }
                    if (vdm.ddate.CompareTo(strNow) >= 0)
                    {
                        IconFlag += "1" + "|";
                    }

                    if (strDataID.Contains(vdm.dataid) == true)
                    {
                        IconFlag += "2" + "|";
                    }


                    //取关系表
                    List<r_datacata> rdArry = new List<r_datacata>();
                    rdArry = db.r_datacata.Where(x => x.dataid == ldataID).ToList();
                    List<int> CatalogIDArry = new List<int>();
                    foreach (var xx in rdArry)
                    {
                        CatalogIDArry.Add(xx.catalogid);
                    }
                    //取标签
                    // dc = db.datacatalog.Where(x => CatalogIDArry.Contains(x.catalogid)&& x.sid == 99).ToList();
                    List<datacatalog> dc = new List<datacatalog>();
                    dc = db.datacatalog.Where(x => CatalogIDArry.Contains(x.catalogid)).ToList();
                    List<DataCatalogs> dcss = new List<DataCatalogs>();
                    string[] sycatalogs = ut.getAppSetting(14).Split('|');
                     string[] syOrg = ut.getAppSetting(15).Split('|');
                    foreach (var gg in dc)
                    {
                        DataCatalogs dcs = new DataCatalogs();
                        dcs.catainfo = gg.catainfo;
                        dcs.catalogid = gg.catalogid;
                        dcs.sid = gg.sid;
                        dcss.Add(dcs);
                        foreach (var sid in sycatalogs)
                        {
                            if (gg.sid == int.Parse(sid))
                            {
                               
                                    IconFlag += "3" + "|";
                                //专家
                                if (gg.sid == 99)
                                {
                                    lcount = 300;
                                    //检查黄
                                    questionnaire qn = new questionnaire();
                                    //取资料主档信息
                                    qn = db.questionnaire.Where(vv => vv.userid == strUserId).FirstOrDefault();
                                    if (qn == null || qn.qid <= 0)
                                    {
                                        rm.RetCode = -8;
                                        rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                                    }
                                }
                               
                            }
                        }
                        foreach (var sid in syOrg)
                        {
                            if (gg.sid == int.Parse(sid))
                            {
                                IconFlag += "4" + "|";
                            }
                        }
                    }
                    vdm.DataCatalogArry = dcss;

                    //if (vv.Count() > 0)
                    vdm.Buyed = 0;
                    /*closed by zb for payinfo
                     * var vv = db.payinfo.Where(x => x.userid == strUserId && x.dataid == dataID && x.transaction_id != null && x.transaction_id.Trim() != "");
                     var tmp= db.payinfo.Where(x=>  x.transaction_id != null && x.transaction_id.Trim() != "");
                    var BuyedCount = tmp.Where(x => x.dataid == dataID).Count();
                    */
                    var ReadCount = 0;
                    //ReadCount=db.syslog.Count();



                    var rc = db.readcount.Where(x => x.dataid == dataID).ToList();
                    if (rc != null)
                    {
                        ReadCount = (int)rc.Sum(x => x.rcount);
                        var rcs = rc.Where(x => x.userid == strUserId).FirstOrDefault();
                        if (rcs != null)
                        {
                            rcs.rcount++;
                            db.SaveChanges();
                        }
                        else
                        {
                            readcount rt = new readcount();
                            rt.rcount = 1;
                            rt.dataid = dataID;
                            rt.userid = strUserId;
                            db.readcount.Add(rt);
                            db.SaveChanges();
                        }

                    }
                    else
                    {
                        readcount rt = new readcount();
                        rt.rcount = 1;
                        rt.dataid = dataID;
                        rt.userid = strUserId;
                        db.readcount.Add(rt);
                        db.SaveChanges();
                        ReadCount = 1;
                    }
                    vdm.BuyedCount = lcount + dataID + ReadCount;
                    /*cloused by zb for userpic
                     * 取用户头像
                    var ursArry=tmp.Select(x => x.userid).Take(5);
                    List<string> userArry = new List<string>();
                    foreach(var aa in ursArry)
                    {
                        userArry.Add(aa);
                    }
                    vdm.UserPicUrl= userArry;*/
                    List<string> userArry = new List<string>();
                    vdm.UserPicUrl = userArry;
                    vdm.IconFlag = IconFlag.Trim('|');
                }
            }
            if (rm.RetCode != -8)
                rm.RetCode = 0;
            vdmArray.Add(vdm);
            rm.data = vdmArray;
            return rm;

        }
        
        // GET: api/PurchasedByUserid
        [HttpGet]
        [Route("PurchasedByUserid")]
        public RetMessage<int> PurchasedByUserid(string strTokenid)
        {
            RetMessage<int> rm = new RetMessage<int>();
            strTokenid = ut.getUserIDByToken(strTokenid);
            if (strTokenid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            int iRetCode = -1;
            using (dataserverEntities db = new dataserverEntities())
            {
                var ups = from a in db.payinfo
                          where a.userid == strTokenid
                          select a;
                if (ups != null && ups.ToList().Count > 0)
                {
                    iRetCode = 0;
                }
            }
            List<int> iRetArry = new List<int>();
            rm.RetCode = 0;
            iRetArry.Add(iRetCode);
            rm.data = iRetArry;
            return rm;

        }

        [Route("DataInfoListPurchased")]
        public RetMessage<ViewPayInfoModel> GetDataInfoOwn(string strTokenid)
        {
            RetMessage<ViewPayInfoModel> rm = new RetMessage<ViewPayInfoModel>();
            string l_strTokenid = "zb";
            if (strTokenid != null && strTokenid.Trim() != "")
            {
                l_strTokenid = strTokenid;
            }
            l_strTokenid = ut.getUserIDByToken(strTokenid);
            if (l_strTokenid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            //已购买的后台方法
            List<ViewPayInfoModel> ListRetArrys = new List<ViewPayInfoModel>();
            using (dataserverEntities db = new dataserverEntities())
            {
                List<payinfo> ups = new List<payinfo>();
                ups = db.payinfo.Where(x => x.userid == l_strTokenid && x.transaction_id != null).ToList();
                if (ups != null && ups.ToList().Count > 0)
                {
                    List<int> dataids = new List<int>();
                    foreach (var vps in ups)
                    {
                        dataids.Add((int)vps.dataid);
                    }

                    List<datainfo> datainfos = new List<datainfo>();
                    datainfos = db.datainfo.Where(x => dataids.Contains(x.dataid)).ToList();

                    foreach (var pi in ups)
                    {
                        ViewPayInfoModel vpm = new ViewPayInfoModel();
                        vpm.dataid = pi.dataid;
                        vpm.paydate = pi.paydate;
                        vpm.payid = pi.payid;
                        vpm.paymount = pi.paymount;
                        vpm.paytime = pi.paytime;
                        vpm.userid = null;
                        foreach (var cc in datainfos)
                        {
                            if (cc.dataid == vpm.dataid)
                            {
                                vpm.dataInfo = cc.title;
                                vpm.picurd = cc.picurd;
                                vpm.picurs = cc.picurs;
                                vpm.signature = cc.signature;
                                vpm.url = cc.url;
                            }

                        }
                        ListRetArrys.Add(vpm);
                    }
                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;
        }
        [Route("TransLog")]
        public RetMessage<tv_purchased> PutTransLog(string strTokenid)
        {
            RetMessage<tv_purchased> rm = new RetMessage<tv_purchased>();
            string l_strTokenid = "zb";
            if (strTokenid != null && strTokenid.Trim() != "")
            {
                l_strTokenid = strTokenid;
            }
            l_strTokenid = ut.getUserIDByToken(l_strTokenid);
            if (l_strTokenid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            List<tv_purchased> ListRetArrys = new List<tv_purchased>();
            using (dataserverEntities db = new dataserverEntities())
            {
                var ups = from a in db.tv_purchased
                          where a.userid == l_strTokenid
                          select a;
                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList<tv_purchased>();

                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;

        }
        /*
        [Route("QuestList")]//bim娘
        public RetMessage<questioninfo> GetQuestList()
        {
            RetMessage<questioninfo> rm = new RetMessage<questioninfo>();
            List<questioninfo> ListRetArrys = new List<questioninfo>();
            using (dataserverEntities db = new dataserverEntities())
            {

                var ups = from tv_up in db.questioninfo
                              //  where tv_up.userid == strUserID
                          select tv_up;

                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;

        }
        [Route("QuestList")]//bim娘
        public RetMessage<questioninfo> GetQuestList(string queryKey)
        {
            RetMessage<questioninfo> rm = new RetMessage<questioninfo>();
            List<questioninfo> ListRetArrys = new List<questioninfo>();
            using (dataserverEntities db = new dataserverEntities())
            {

                var ups = from tv_up in db.questioninfo
                          where tv_up.qtitle.Contains(queryKey) || tv_up.qcontent.Contains(queryKey)
                          select tv_up;

                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;
        }*/
    }
}
