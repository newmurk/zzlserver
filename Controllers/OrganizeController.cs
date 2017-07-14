using LitJson2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using ZeroStartAPI;
using ZeroStartAPI.Models;

namespace ZeroStartAPI.Controllers
{
    //requestSource="zzl|0x95e516cf|D279882E8B8198C9AB43A541B6B04BA1"
    public class OrganizeController : ApiController
    {
        utility ut = new utility();

        [HttpPost]
        [Route("RecordTelAdd")]
        public RetMessage<string> ogenroll(int dataID, string strTokenid)
        {
            RetMessage<string> rm = new RetMessage<string>();
            string strUserId = ut.getUserIDByToken_og(strTokenid);
            if (strUserId == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (organizeEntities db = new organizeEntities())
            {
                recordtel cc = new recordtel();
                
                cc.userid = strUserId;
                cc.dataid = dataID;
                string strNow = DateTime.Now.ToString("u");
                cc.teldate = strNow.Substring(0, 10);
                cc.teltime = strNow.Substring(10, 8); ;
                db.recordtel.Add(cc);
                db.SaveChanges();
                
            }

            List<string> ListRetArrys = new List<string>();
            ListRetArrys.Add("succed");
            rm.data = ListRetArrys;
            rm.RetCode = 0;
            return rm;
        }
        [HttpPost]
        [Route("ogenroll")]
        public RetMessage<string> ogenroll()
        {
            RetMessage<string> rm = new RetMessage<string>();
            string tokenId = "";
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];

            }
            catch
            {
                rm.RetCode = 98;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            
            string userid = ut.getUserIDByToken_og(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            HttpFileCollection files = HttpContext.Current.Request.Files;
            string fileurl = "";
            foreach (string f in files.AllKeys)
            {
                HttpPostedFile file = files[f];
                fileurl = file.FileName;
                
                if (string.IsNullOrEmpty(file.FileName) == false)
                    file.SaveAs(HttpContext.Current.Server.MapPath("~/App_Data/") + file.FileName);
            }
            int ogquestionnaireID = 0;
            using (organizeEntities db = new organizeEntities())
            {
                ogquestionnaire cc = new ogquestionnaire();
                cc.name = HttpContext.Current.Request["name"].ToString();
                cc.birthday = HttpContext.Current.Request["birthday"].ToString();
                cc.sex = HttpContext.Current.Request["sex"].ToString(); 
                cc.nativeplace = HttpContext.Current.Request["nativeplace"].ToString();
                cc.nations = HttpContext.Current.Request["nations"].ToString();
                cc.level = HttpContext.Current.Request["level"].ToString(); 
                cc.identitycard = HttpContext.Current.Request["identitycard"].ToString();
                cc.educated = HttpContext.Current.Request["educated"].ToString(); 
                cc.unit = HttpContext.Current.Request["unit"].ToString(); 
                cc.address = HttpContext.Current.Request["address"].ToString();
                cc.mobile = HttpContext.Current.Request["mobile"].ToString();
                cc.membersflag = int.Parse(HttpContext.Current.Request["membersflag"].ToString());
                cc.place = HttpContext.Current.Request["place"].ToString(); 
                cc.qqid = HttpContext.Current.Request["qqid"].ToString();
                cc.picurl =  fileurl;
                cc.userid = userid;
                db.ogquestionnaire.Add(cc);
                db.SaveChanges();
                ogquestionnaireID = cc.qid;
            }
           
            List<string> ListRetArrys = new List<string>();
            ListRetArrys.Add(ogquestionnaireID.ToString());
            rm.data = ListRetArrys;
            rm.RetCode = 0;
            return rm; 
        }
        [HttpGet]
        [Route("ogenrollQuery")]
        /*提交数据：token
返回：用户提交过的代报名信息和是否付款的信息
         */
        public RetMessage<ViewOgQuestionnaireModel> ogenrollQuery(string tokenId)
        {

            RetMessage<ViewOgQuestionnaireModel> rm = new RetMessage<ViewOgQuestionnaireModel>();
            List<ViewOgQuestionnaireModel> ListRetArrys = new List<ViewOgQuestionnaireModel>();

            string userid = ut.getUserIDByToken_og(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (organizeEntities db = new organizeEntities())
            {
                List<ogquestionnaire> rdArry = new List<ogquestionnaire>();
                rdArry = db.ogquestionnaire.Where(x => x.userid == userid).ToList();
                
                List<payinfo> payArray = new List<payinfo>();
                payArray = db.payinfo.Where(x => x.userid == userid  && x.transaction_id != null && x.transaction_id.Trim() != "").ToList();
                foreach (var xx in rdArry)
                {
                    ViewOgQuestionnaireModel vgo = new ViewOgQuestionnaireModel();
                    vgo.Buyed = -1;
                    vgo.qid = xx.qid;
                    vgo.name = xx.name;
                    foreach (var cc in payArray)
                    {
                        if (cc.dataid == xx.qid)
                        {
                            vgo.Buyed = 0;
                        }
                    }
                    ListRetArrys.Add(vgo);
                }
            }
           
            
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;
        }

        [Route("ogRetUserID")]
        public async Task<RetMessage<Token>> Getuseridbywxlogin(string js_code)
        {
            var l_js_code = "0311zLDk1E5afi09g9Ck14DXDk11zLD9";
            if (js_code != null)
                l_js_code = js_code;

            string APPID = ut.getAppSetting(11);
            string SECRET = ut.getAppSetting(12);
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
                    token.tokenMake(data["openid"].ToString(), data["session_key"].ToString(),2);
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
        //syd	首页培训机构列表	返回：图片、标题、简介、地区、id、培训机构介绍pdf的url

        [HttpGet]
        [Route("ogsyd")]
        public IEnumerable<datainfo> syd()
        {
            List<datainfo> ListRetArrys = new List<datainfo>();
            using (organizeEntities db = new organizeEntities())
            {

                var ups = from tv_up in db.datainfo
                          where tv_up.type != null
                          select tv_up;

                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }
            }
            return ListRetArrys;

        }
        [HttpGet]
        [Route("OgDataCatalogList")]
        public RetMessage<datacatalog> Getdatacatalog()
        {
            RetMessage<datacatalog> rm = new RetMessage<datacatalog>();
            List<datacatalog> ListRetArrys = new List<datacatalog>();
            using (organizeEntities db = new organizeEntities())
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
        [Route("OgDataInfoList")]
        public RetMessage<ViewDataInfoModel> GetDataInfobyID(int dataID, string strTokenid)
        {
            RetMessage<ViewDataInfoModel> rm = new RetMessage<ViewDataInfoModel>();
            int ldataID = dataID;
            string strUserId = ut.getUserIDByToken_og(strTokenid);
            if (strUserId == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            List<ViewDataInfoModel> vdmArray = new List<ViewDataInfoModel>();
            ViewDataInfoModel vdm = new ViewDataInfoModel();
            using (organizeEntities db = new organizeEntities())
            {
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
                    foreach (var gg in dc)
                    {
                        DataCatalogs dcs = new DataCatalogs();
                        dcs.catainfo = gg.catainfo;
                        dcs.catalogid = gg.catalogid;
                        dcs.sid = gg.sid;
                        dcss.Add(dcs);
                    }
                    vdm.DataCatalogArry = dcss;
                    vdm.Buyed = 0;
                    var ReadCount = 0;
                    /*
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
                    */

                    vdm.BuyedCount = int.Parse(ut.getAppSetting(9)) + dataID + ReadCount;
                    List<string> userArry = new List<string>();
                    vdm.UserPicUrl = userArry;
                }
            }
            rm.RetCode = 0;
            vdmArray.Add(vdm);
            rm.data = vdmArray;
            return rm;
        }
        [Route("OgDataInfoList")]
        public RetMessage<datainfo> GetDataInfo(int CatalogID)
        {
            RetMessage<datainfo> rm = new RetMessage<datainfo>();
            int lCatalogID = CatalogID;
            List<datainfo> ListRetArrys = new List<datainfo>();
            using (organizeEntities db = new organizeEntities())
            {
                var ups = from a in db.datainfo
                          join b in db.r_datacata on a.dataid equals b.dataid
                          join c in db.datacatalog on b.catalogid equals c.catalogid
                          where c.catalogid == lCatalogID
                          select a;

                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList<datainfo>();

                }
            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;
        }

        [HttpGet]
        [Route("OgMakeOrder")]
        public RetMessage<Order> makeOrder(string tokenId, int dataId)
        {
            RetMessage<Order> rm = new RetMessage<Order>();
            if (tokenId != null)
            {
                string useridandopenid = ut.getUserIDAndOpenIDByToken_og(tokenId);


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
                    using (organizeEntities db = new organizeEntities())
                    {
                        //取资料主档信息
                        ogquestionnaire cc = new  ogquestionnaire();
                        cc = db.ogquestionnaire.Find(dataId);
                        if (cc == null)
                        {
                            rm.RetCode = -6;
                            rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                            return rm;
                        }
                        string[] levelBuff = cc.level.Split(',');
                        strcode = "全国BIM等级考试缴费["+ cc.level + "]";
                        total_fee = (int.Parse(ut.getAppSetting(13)) * levelBuff.Count()*100).ToString();
                    }
                    string wx_appid = ut.getAppSetting(11);
                    string wx_mch_id = ut.getAppSetting(1);
                    string wx_nonce_str = ut.getRandomString(20);
                    byte[] buffer = Encoding.UTF8.GetBytes(strcode);
                    string wx_body = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    string wx_out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmss") + ut.getRandomString(10);
                    string wx_total_fee = ((int)(decimal.Parse(total_fee) )).ToString();//unit:fen
                    //string wx_total_fee = "1";//unit:fen
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
                        using (organizeEntities db = new organizeEntities())
                        {
                            string strNow = DateTime.Now.ToString("u");
                            payinfo pi = new payinfo();
                            pi.dataid = dataId;
                            pi.paydate = strNow.Substring(0, 10);
                            pi.paytime = strNow.Substring(10, 8);
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



        //查询结果内容
        [Route("OgDataInfoListQuery")]
        public RetMessage<datainfo> GetDataInfoByQuery(string queryKey, string strTokenid)
        {
            RetMessage<datainfo> rm = new RetMessage<datainfo>();
            List<datainfo> ListRetArrys = new List<datainfo>();
            using (organizeEntities db = new organizeEntities())
            {
                IQueryable<datainfo> ups;
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


                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList<datainfo>();
                }

                //插入查询log
                querylog qlog = new querylog();
                string strNow = DateTime.Now.ToString("u");
                qlog.userid = ut.getUserIDByToken_og(strTokenid);
                qlog.qdate = strNow.Substring(0, 10);
                qlog.qtime = strNow.Substring(11, 8);
                qlog.strkey = queryKey;
                db.querylog.Add(qlog);
                db.SaveChanges();

            }
            rm.RetCode = 0;
            rm.data = ListRetArrys;
            return rm;
        }
       
        [HttpGet]
        [Route("OgPurchasedByUserid")]
        public RetMessage<int> PurchasedByUserid(string strTokenid)
        {
            RetMessage<int> rm = new RetMessage<int>();
            strTokenid = ut.getUserIDByToken_og(strTokenid);
            if (strTokenid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            int iRetCode = -1;
            using (organizeEntities db = new organizeEntities())
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

        [Route("OgDataInfoListPurchased")]
        public RetMessage<ViewPayInfoModel> GetDataInfoOwn(string strTokenid)
        {
            RetMessage<ViewPayInfoModel> rm = new RetMessage<ViewPayInfoModel>();
            string l_strTokenid = "zb";
            if (strTokenid != null && strTokenid.Trim() != "")
            {
                l_strTokenid = strTokenid;
            }
            l_strTokenid = ut.getUserIDByToken_og(strTokenid);
            if (l_strTokenid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            //已购买的后台方法
            List<ViewPayInfoModel> ListRetArrys = new List<ViewPayInfoModel>();
            using (organizeEntities db = new organizeEntities())
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
        [Route("OgTransLog")]
        public RetMessage<tv_purchased> PutTransLog(string strTokenid)
        {
            RetMessage<tv_purchased> rm = new RetMessage<tv_purchased>();
            string l_strTokenid = "zb";
            if (strTokenid != null && strTokenid.Trim() != "")
            {
                l_strTokenid = strTokenid;
            }
            l_strTokenid = ut.getUserIDByToken_og(l_strTokenid);
            if (l_strTokenid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            List<tv_purchased> ListRetArrys = new List<tv_purchased>();
            using (organizeEntities db = new organizeEntities())
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
    }
}

