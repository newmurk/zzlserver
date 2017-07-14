using LitJson2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ZeroStartAPI;
using ZeroStartAPI.Models;

namespace ZeroStartAPI.Controllers
{
    public class MasterController : ApiController
    {
        
        utility ut = new utility();

        [HttpPost]
        [Route("PushUserPicMa")]
        public RetMessage<string> GetUserPicMa(string strTokenid, string nickname, string userPicUrl)
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
            using (masterEntities db = new masterEntities())
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
        [HttpGet]
        [Route("getMasterCard")]
        public async Task<RetMessage<string>> getMasterCard(string tokenId)
        {
            RetMessage<string> rm = new RetMessage<string>();
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;
            }
            
            List<string> retData = new List<string>();
            var userPicPath = ut.getUserPicByUserid(userid);
            if (userPicPath.succeed==0 )
            {
                string aa = "";
                string access_token = "";
                if (HttpContext.Current.Application["g_access_token_time"] != null)
                {
                    var tokenApplyTime = HttpContext.Current.Application["g_access_token_time"].ToString().Split('|')[0];
                    var tokenApplyed = HttpContext.Current.Application["g_access_token_time"].ToString().Split('|')[1];
                    string strNow = DateTime.Now.AddHours(-2).ToString("u");
                    if (tokenApplyTime.CompareTo(strNow) > 0)
                    {
                        //有效
                        access_token = tokenApplyed;
                    }
                }
                if (access_token == "")
                {
                    string APPID = ut.getAppSetting(16);
                    string SECRET = ut.getAppSetting(17);
                    string grant_type = ut.getAppSetting(18);
                    string strurl = @"https://api.weixin.qq.com/cgi-bin/token?appid=" + APPID + "&secret=" + SECRET + "&grant_type=" + grant_type;
                    HttpClient hc = new HttpClient();
                    HttpResponseMessage hrm = await hc.GetAsync(strurl);

                    if (hrm.IsSuccessStatusCode)
                    {
                        string jsonresult = await hrm.Content.ReadAsStringAsync();
                        if (jsonresult.Contains("errcode") == false)
                        {
                            JsonData data = JsonMapper.ToObject(jsonresult);
                            //access_token，
                            access_token = data["access_token"].ToString();

                            var tokenApplyTime = DateTime.Now.ToString("u");
                            var tokenApplyed = access_token;
                            HttpContext.Current.Application["g_access_token_time"] = tokenApplyTime + "|" + tokenApplyed;
                        }
                    }
                    else
                    {
                        access_token = "Error";
                    }
                }
                if (access_token.CompareTo("Error") != 0)
                {
                    aa = ut.PostMoths(access_token, userPicPath.picm, userPicPath.roomkey, userid, userPicPath.roomid);
                }
                else
                    aa = access_token;
                retData.Add(aa);
                rm.data = retData;
                rm.RetCode = 0;
            }
            else
            {
                rm.RetCode = -1;
            }
            // string aa = getUserQrCode(userPicPath).ToString();
           
           
            return rm;

        }

        [Route("maRetUserID")]
        public async Task<RetMessage<Token>> Getuseridbywxlogin(string js_code)
        {
            var l_js_code = "0311zLDk1E5afi09g9Ck14DXDk11zLD9";
            if (js_code != null)
                l_js_code = js_code;

            string APPID = ut.getAppSetting(16);
            string SECRET = ut.getAppSetting(17);
            string grant_type = ut.getAppSetting(7);
            string strurl = @" https://api.weixin.qq.com/sns/jscode2session?appid=" + APPID + "&secret=" + SECRET + "&js_code=" + l_js_code + "&grant_type=" + grant_type;
            HttpClient hc = new HttpClient();
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
                    token.tokenMake(data["openid"].ToString(), data["session_key"].ToString(), 3);
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

        public RetMessage<ViewMastInfoModel> getAllMasterCode(string masterId,string tokenId, string queryKey, string serivceidArry,int iCase)
        {
            RetMessage<ViewMastInfoModel> rm = new RetMessage<ViewMastInfoModel>();
            List<ViewMastInfoModel> LRetDate = new List<ViewMastInfoModel>();
            List<masterinfo> ListRetArrys = new List<masterinfo>();
            List<mpayinfo> lmpayinfo = new List<mpayinfo>();
            List<mpayinfo> mpayinfos = new List<mpayinfo>();
            List<services> LServices = new List<services>();
            List<KeyCountSharp> readCountArry = new List<KeyCountSharp>();
            List<mroominfo> mroominfo = new List<mroominfo>();
            List<ViewmroominfoModel> vmroominfo = new List<ViewmroominfoModel>();



            string serivceFlag = "";
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            using (masterEntities db = new masterEntities())
            {

                IQueryable<services> servicesArry;
                if (serivceidArry != null && serivceidArry != "")
                {
                    List<int> ltServiceID = new List<int>();
                    string[] buff = serivceidArry.Split('|');
                    foreach (var ii in buff)
                    {
                        ltServiceID.Add(int.Parse(ii));
                    }
                    servicesArry = from services in db.services
                                   where ltServiceID.Contains(services.sid)
                                   select services;
                }
                else
                {
                    servicesArry = from services in db.services
                                   select services;
                }

                if (servicesArry != null && servicesArry.ToList().Count > 0)
                {
                    LServices = servicesArry.ToList();

                }
                IQueryable<masterinfo> ups;

                if (queryKey != null)
                {
                    foreach (var cc in LServices)
                    {
                        if (cc.describe.Contains(queryKey))
                        {
                            serivceFlag += cc.sid + "|";

                        }
                    }
                    serivceFlag = serivceFlag.Trim('|');
                    ups = from tv_up in db.masterinfo
                          where (tv_up.mastername.Contains(queryKey) || tv_up.profiles.Contains(queryKey)
                          || tv_up.serviceArry.CompareTo(serivceFlag) == 0)
                          select tv_up;
                }
                else
                {
                    if(masterId!=null&& masterId!= ""){
                        ups = from tv_up in db.masterinfo
                              where tv_up.masterid.CompareTo(masterId)==0
                              select tv_up;
                        //取评价，订单和阅读量
                        readCountArry = (from p in db.mreadcount
                                             where p.masterid.CompareTo(masterId) == 0
                                             group p by p.masterid into g
                                             select new KeyCountSharp()
                                             {
                                                 Key = g.Key,
                                                 Total = (int)g.Count()
                                             }).ToList();
                        //readCountArry = readCountArry.OrderByDescending(p => p.Total).ToList();
                       var tmpayinfo = from vmpayinfo in db.mpayinfo
                              where vmpayinfo.masterid.CompareTo(masterId) == 0
                              select vmpayinfo;
                        if (tmpayinfo != null && tmpayinfo.ToList().Count > 0)
                        {
                            lmpayinfo = tmpayinfo.ToList();
                        }

                    }
                    else
                    {
                        ups = from tv_up in db.masterinfo
                              select tv_up;

                    }
                   
                }

                if (ups != null && ups.ToList().Count > 0)
                {
                    ListRetArrys = ups.ToList();

                }

                if (queryKey != null)
                {
                    querylog qlog = new querylog();
                    string strNow = DateTime.Now.ToString("u");
                    qlog.userid = userid;
                    qlog.qdate = strNow.Substring(0, 10);
                    qlog.qtime = strNow.Substring(11, 8);
                    qlog.strkey = queryKey;
                    db.querylog.Add(qlog);
                    db.SaveChanges();
                }
                var payinfos= db.mpayinfo.Where(p => p.userid == userid);
               if(payinfos!=null&& payinfos.ToList().Count > 0)
                {
                    mpayinfos = payinfos.ToList();
                }
               if(iCase==2)//取最后一个师傅发布的信息内容
                {
                    foreach(var bb in ListRetArrys)
                    {
                        var tt = db.mroominfo.Where(p => p.masterid == bb.masterid).OrderByDescending(t => t.cdate).ThenByDescending(v => v.ctime);
                        
                        if(tt!=null&& tt.ToList().Count>0)
                        mroominfo.Add(tt.ToList().First());
                       
                    }

                }

            }
            foreach (var aa in ListRetArrys)
            {

                ViewMastInfoModel vmim = new ViewMastInfoModel();
                vmim.masterid = aa.masterid;
                vmim.mastername = aa.mastername;
                vmim.picurl = aa.picurl;
                vmim.profiles = aa.profiles;
                vmim.roomid = aa.roomid;
                vmim.address = aa.address.Replace('|', ',');
                vmim.mobile = aa.mobile;
                vmim.mail = aa.mail;
                vmim.wxid = aa.wxid;
                vmim.iRete = 100;
                vmim.iBuyed = 0;
                string[] buff = aa.serviceArry.Split('|');
                foreach (var bb in LServices)
                {
                    foreach (var cc in buff)
                    {
                        if (int.Parse(cc) == bb.sid)
                        {
                            vmim.serviceArry += bb.describe + "|";
                        }
                    }
                }
                vmim.serviceArry = vmim.serviceArry.Trim('|');

                ////取评价，订单和阅读量
                foreach(var vv in readCountArry)
                {
                    if (vv.Key == vmim.masterid)
                    {
                        vmim.iReadCount = vv.Total;
                    }

                }
                foreach (var vv in lmpayinfo)
                {
                    if (vv.masterid == vmim.masterid)
                    {
                        vmim.iServiceCount = lmpayinfo.Count();
                        vmim.iRete = vmim.iRete-lmpayinfo.Select(p => p.evaluate != 1).Count();
                    }
                }
                foreach (var vv in mpayinfos)
                {
                    if (vv.masterid == vmim.masterid)
                        vmim.iBuyed = 1;

                }
                if (iCase == 2)//取最后一个师傅发布的信息内容
                {
                    foreach(var cc in mroominfo)
                    {
                        List<ViewFileInfo> ltvFInfoA = new List<ViewFileInfo>();
                        List<ViewFileInfo> ltvFInfoV = new List<ViewFileInfo>();
                        List<ViewFileInfo> ltvFInfoI = new List<ViewFileInfo>();
                        string[] buffx = cc.fileurl.Split('|');
                        foreach (var xx in buffx)
                        {
                            ViewFileInfo fileinfo = new ViewFileInfo();
                            string[] tmp = xx.Split('=');
                            fileinfo.fileurl = tmp[0];
                            fileinfo.filetype = tmp[1];
                            //1-Img,2-Video,3-audio
                            if (fileinfo.filetype.CompareTo("1") == 0)
                                ltvFInfoI.Add(fileinfo);
                            if (fileinfo.filetype.CompareTo("2") == 0)
                                ltvFInfoV.Add(fileinfo);
                            if (fileinfo.filetype.CompareTo("3") == 0)
                                ltvFInfoA.Add(fileinfo);
                        }
                        ViewmroominfoModel retModel = new ViewmroominfoModel();
                        retModel.ltFilesAudio = ltvFInfoA;
                        retModel.ltFilesVideo = ltvFInfoV;
                        retModel.ltFilesImg = ltvFInfoI;
                        retModel.masterid = cc.masterid;
                        retModel.mrcontent = cc.mrcontent;
                        retModel.fileurl = cc.fileurl;
                        retModel.cdate = cc.cdate;
                        retModel.ctime = cc.ctime;
                        retModel.mrcid = cc.mrcid;
                        vmroominfo.Add(retModel);
                    }
                    vmim.lmroominfo = vmroominfo;
                }

                LRetDate.Add(vmim);
            }
            rm.RetCode = 0;
            rm.data = LRetDate.OrderByDescending(p=>p.iBuyed).ToList();
            return rm;
        }

        [HttpGet]
        [Route("MasterListAll")]
        public RetMessage<ViewMastInfoModel> getAllMaster(string tokenId, string queryKey,string serivceidArry)
        {
            RetMessage<ViewMastInfoModel> rm = new RetMessage<ViewMastInfoModel>();
            rm = getAllMasterCode(null,tokenId, queryKey, serivceidArry,1);
            return  rm;
        }
        [HttpGet]
        [Route("MasterListSignle")]
        public RetMessage<ViewMastInfoModel> getMasterbyID(string tokenId,string masterId)
        {
            RetMessage<ViewMastInfoModel> rm = new RetMessage<ViewMastInfoModel>();
            rm = getAllMasterCode(masterId,tokenId, null, null,2);
            return rm;
        }
       
        [HttpGet]
        [Route("MasterRoom")]
        public RetMessage<ViewRoomInfoModel> getMasterRoom(string tokenId, string masterId)
        {
            RetMessage<ViewRoomInfoModel> rm = new RetMessage<ViewRoomInfoModel>();
            rm.RetCode = 0;
            List<ViewRoomInfoModel> ltRetDatas = new List<ViewRoomInfoModel>();
            List<ViewmroominfoModel> ltRoomInfos = new List<ViewmroominfoModel>();
            ViewRoomInfoModel retData = new ViewRoomInfoModel();
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
 
            using (masterEntities db = new masterEntities())
            {
                var masterinfo = db.masterinfo.Find(masterId);
                if (masterinfo !=null)
                {
                    //取房间图片，介绍
                    List<String> userIDArry = new List<string>();
                    List<userinfo> ltUserinfos = new List<userinfo>();

                    retData.username = masterinfo.mastername;
                    retData.userpic = masterinfo.picurl;


                    retData.iOwner = -1;
                    var RoomInfos = db.mroominfo.Where(p => p.masterid == masterId).ToList();
                    foreach (var aa in RoomInfos)
                    {
                        ViewmroominfoModel vrim = new ViewmroominfoModel();
                        vrim.cdate = aa.cdate;
                        vrim.mrcid = aa.mrcid;
                        vrim.masterid = aa.masterid;
                        vrim.mrcontent = aa.mrcontent;
                        vrim.ctime = aa.ctime;
                        vrim.fileurl = aa.fileurl;

                        List<ViewFileInfo> ltvFInfoA = new List<ViewFileInfo>();
                        List<ViewFileInfo> ltvFInfoV = new List<ViewFileInfo>();
                        List<ViewFileInfo> ltvFInfoI = new List<ViewFileInfo>();
                        string[] buff = vrim.fileurl.Split('|');
                        foreach (var xx in buff)
                        {
                            ViewFileInfo fileinfo = new ViewFileInfo();
                            string[] tmp = xx.Split('=');
                            fileinfo.fileurl = tmp[0];
                            fileinfo.filetype = tmp[1];
                            //1-Img,2-Video,3-audio
                            if (fileinfo.filetype.CompareTo("1") == 0)
                                ltvFInfoI.Add(fileinfo);
                            if (fileinfo.filetype.CompareTo("2") == 0)
                                ltvFInfoV.Add(fileinfo);
                            if (fileinfo.filetype.CompareTo("3") == 0)
                                ltvFInfoA.Add(fileinfo);
                        }
                        vrim.ltFilesAudio = ltvFInfoA;
                        vrim.ltFilesVideo = ltvFInfoV;
                        vrim.ltFilesImg = ltvFInfoI;

                        var mrcrinfos = db.mrcrinfo.Where(p => p.mrcid == aa.mrcid).ToList();
                        List<ViewmrcrinfoModel> ltmrcrinfos = new List<ViewmrcrinfoModel>();
                        foreach (var bb in mrcrinfos)
                        {
                            if (bb.ruserid != null && bb.ruserid != "")
                                userIDArry.Add(bb.ruserid);
                            if (bb.suserid != null && bb.suserid != "")
                                userIDArry.Add(bb.suserid);

                        }
                        ltUserinfos = db.userinfo.Where(x => userIDArry.Contains(x.userid)).ToList();

                        foreach (var bb in mrcrinfos)
                        {
                            ViewmrcrinfoModel vmt = new ViewmrcrinfoModel();
                            vmt.mcontent = bb.mcontent;
                            vmt.mdate = bb.mdate;
                            vmt.mid = bb.mid;
                            vmt.mrcid = bb.mrcid;
                            vmt.mrcrid = bb.mrcrid;
                            vmt.mtime = bb.mtime;
                            vmt.suserid = bb.suserid;
                            vmt.ruserid = bb.ruserid;
                            vmt.IsMe = -1;

                            foreach (var cc in ltUserinfos)
                            {
                                if (bb.ruserid == cc.userid)
                                {
                                    vmt.rusername = cc.username;
                                }
                                if (bb.suserid == cc.userid)
                                {
                                    vmt.susername = cc.username;
                                }
                            }
                            if (vmt.suserid == userid)
                            {
                                vmt.IsMe = 0;
                            }

                            ltmrcrinfos.Add(vmt);

                        }
                        vrim.ltmrcrinfos = ltmrcrinfos;
                        ltRoomInfos.Add(vrim);
                    }
                    ltRoomInfos = ltRoomInfos.OrderByDescending(p => p.cdate).ThenByDescending(p => p.ctime).ToList();
                    retData.ltRoomInfos = ltRoomInfos;
                    //var masterinfo = db.masterinfo.Where(p => p.masterid == userid).Select(t => t.roomid).FirstOrDefault();
                    if (userid == masterId)
                    {
                        retData.iOwner = 0;
                    }
                    ltRetDatas.Add(retData);
                }
                else
                {
                    rm.RetCode = -13;
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);

                }


            }
            if(rm.RetCode==0)
            rm.data= ltRetDatas;
            return rm;
        }
        [HttpPost]
        [Route("MasterDelFiles")]
        public RetMessage<string> roomFilesDel()
        {
            RetMessage<string> rm = new RetMessage<string>();
            string tokenId = "";
            string filename = "";
            string masterid = "";
            string RetString = "";
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];
                masterid = HttpContext.Current.Request["masterid"].ToString();
                filename = HttpContext.Current.Request["filename"].ToString();
            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;
            }
            string FileSavedDirectory = HttpContext.Current.Server.MapPath("~/UploadedData/" + masterid);
            string fileFullname = FileSavedDirectory + "/" + filename;
            if (string.IsNullOrEmpty(filename) == false)
            {
                if (Directory.Exists(FileSavedDirectory) == false|| File.Exists(fileFullname) == false)//如果不存在就创建file文件夹
                {
                    rm.RetCode = -21;
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                }
                else
                {
                    try
                    {
                        File.Delete(fileFullname);

                    }
                    catch
                    {
                        rm.RetCode = -22;
                        rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                    }
                }
            }
            rm.RetCode = 0;
            List<string> ls = new List<string>();
            ls.Add(RetString);
            rm.data = ls;
            return rm;
        }
        [HttpPost]
        [Route("MasterPushFiles")]
        public RetMessage<string> roomFilesPush()
        {
            RetMessage<string> rm = new RetMessage<string>();
            string tokenId = "";
            string masterid = "";
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];
                masterid =HttpContext.Current.Request["masterid"].ToString();
            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;
            }
            HttpFileCollection files = HttpContext.Current.Request.Files;
            string fileurl = "";
            string FileSavedDirectory = HttpContext.Current.Server.MapPath("~/UploadedData/" + masterid);
            foreach (string f in files.AllKeys)
            {
                HttpPostedFile file = files[f];
                //string NowTime = DateTime.Now.ToString("u");
                fileurl +=f;
                if (string.IsNullOrEmpty(file.FileName) == false)
                {
                    if (Directory.Exists(FileSavedDirectory) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(FileSavedDirectory);
                    }
                    file.SaveAs(FileSavedDirectory+@"/"+ fileurl);

                }
            }
            rm.RetCode = 0;
            List<string> ls = new List<string>();
            ls.Add(fileurl);
            rm.data = ls;
            return rm;
        }
        [HttpPost]
        [Route("MasterDelContent")]
        public RetMessage<string> roomContentDel()
        {
            RetMessage<string> rm = new RetMessage<string>();
            rm.RetCode = -1;
            string tokenId = "";
            string mrcid = "";
            string masterid = "";
            int key = 0;
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];
                masterid = HttpContext.Current.Request["masterid"].ToString();
                mrcid = HttpContext.Current.Request["mrcid"].ToString();
            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;
            }
            key = int.Parse(mrcid);
            List<string> ltFileArry = new List<string>();
            using (masterEntities db = new masterEntities())
            {
                var mroominfo = db.mroominfo.Find(key);
                if (mroominfo != null)
                {
                    string[] buff = mroominfo.fileurl.Split('|');
                    foreach (var aa in buff)
                    {
                        string[] bufft = aa.Split('=');
                        ltFileArry.Add(bufft[0]) ;
                    }
                }
                db.mrcrinfo.RemoveRange(db.mrcrinfo.Where(x => x.mrcid == key));
                db.mroominfo.Remove(mroominfo);
                db.SaveChanges();

            }
            string FileSavedDirectory = HttpContext.Current.Server.MapPath("~/UploadedData/" + masterid);
            foreach (var aa in ltFileArry)
            {
                string fileFullname = FileSavedDirectory + "/" + aa;
                try
                {
                    if(File.Exists(fileFullname) == true)
                        File.Delete(fileFullname);

                }
                catch
                {
                    rm.RetCode = -22;
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                }
            }
            rm.RetCode = 0;
            return rm;
        }
        [HttpPost]
        [Route("MasterPushContent")]
        public RetMessage<ViewmroominfoModel> roomContentPush()
        {
            RetMessage<ViewmroominfoModel> rm = new RetMessage<ViewmroominfoModel>();
            string tokenId = "";
            string masterid = "";
            string fileArr = "";
            string mrcontent = "";
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];
                masterid = HttpContext.Current.Request["masterid"].ToString();
                fileArr = HttpContext.Current.Request["fileArr"].ToString();
                mrcontent = HttpContext.Current.Request["mrcontent"].ToString();
            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            List<ViewmroominfoModel> ListRetArrys = new List<ViewmroominfoModel>();
            using (masterEntities db = new masterEntities())
            {
                mroominfo cc = new mroominfo();
                string strNow = DateTime.Now.ToString("u");
                cc.cdate = strNow.Substring(0, 10);
                cc.ctime = strNow.Substring(11, 8);
                cc.fileurl = fileArr;
                cc.masterid = masterid;
                cc.mrcontent = mrcontent;
                db.mroominfo.Add(cc);
                db.SaveChanges();
                ViewmroominfoModel vm = new ViewmroominfoModel();
                vm.cdate = cc.cdate;
                vm.ctime = cc.ctime;
                vm.masterid = masterid;
                vm.mrcontent = mrcontent;
                vm.mrcid = cc.mrcid;
                List<ViewFileInfo> ltvFInfoA = new List<ViewFileInfo>();
                List<ViewFileInfo> ltvFInfoV = new List<ViewFileInfo>();
                List<ViewFileInfo> ltvFInfoI = new List<ViewFileInfo>();
                string[] buffx = cc.fileurl.Split('|');
                foreach (var xx in buffx)
                {
                    ViewFileInfo fileinfo = new ViewFileInfo();
                    string[] tmp = xx.Split('=');
                    fileinfo.fileurl = tmp[0];
                    fileinfo.filetype = tmp[1];
                    //1-Img,2-Video,3-audio
                    if (fileinfo.filetype.CompareTo("1") == 0)
                        ltvFInfoI.Add(fileinfo);
                    if (fileinfo.filetype.CompareTo("2") == 0)
                        ltvFInfoV.Add(fileinfo);
                    if (fileinfo.filetype.CompareTo("3") == 0)
                        ltvFInfoA.Add(fileinfo);
                }

                vm.ltFilesAudio = ltvFInfoA;
                vm.ltFilesVideo = ltvFInfoV;
                vm.ltFilesImg = ltvFInfoI;
                ListRetArrys.Add(vm);

            }

           
            rm.data = ListRetArrys;
            rm.RetCode = 0;
            return rm;
        }



        [HttpPost]
        [Route("SRPushContent")]
        public RetMessage<string> sroomContentPush()
        {
            RetMessage<string> rm = new RetMessage<string>();
            string tokenId = "";
            int roomid = 0;
            string mrcontent = "";
            int classtype = -1;
            string content = "";
            string filename = "";
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];
                roomid = int.Parse(HttpContext.Current.Request["sid"].ToString());
                if(HttpContext.Current.Request["ctype"].ToString()!=null&& HttpContext.Current.Request["ctype"].ToString().Trim()!="")
                classtype =int.Parse(HttpContext.Current.Request["ctype"].ToString()) ;
                switch (classtype)
                {
                    case 1://文字
                        content = HttpContext.Current.Request["content"];
                        break;
                    case 2://图片，视频,声音
                        HttpFileCollection files = HttpContext.Current.Request.Files;
                        string fileurl = "";
                        foreach (string f in files.AllKeys)
                        {
                            HttpPostedFile file = files[f];
                            fileurl += file.FileName;

                            if (string.IsNullOrEmpty(file.FileName) == false)
                                file.SaveAs(HttpContext.Current.Server.MapPath("~/UploadedData/") + roomid + file.FileName);
                        }
                        filename = fileurl;
                        break;
                }
            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            var userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            int retID = -1;
            using (masterEntities db = new masterEntities())
            {
               
               srcontent cc = new srcontent();
                cc.sid = roomid;
                string strNow = DateTime.Now.ToString("u");
                cc.cdate = strNow.Substring(0, 10);
                cc.ctime = strNow.Substring(11, 8);
                if (classtype == 1)
                {
                    cc.mrcontent = mrcontent;
                }
                if (classtype == 2)
                {
                    cc.fileurl = filename;
                }
                db.srcontent.Add(cc);
                db.SaveChanges();
                retID = cc.srcid;
            }

            List<string> ListRetArrys = new List<string>();
            ListRetArrys.Add(retID.ToString());
            rm.data = ListRetArrys;
            rm.RetCode = 0;
            return rm;
        }


        [HttpPost]
        [Route("PushComment")]
        public RetMessage<string> roomCommentPush()
        {
            RetMessage<string> rm = new RetMessage<string>();
            string tokenId = "";
            int contentid = 0;
            int commentid = 0;
            string mrcontent;
            try
            {
                //内容id、token、文字内容、评论id
                //评论id为0是对内容评论，有评论id是回复评论
                tokenId = HttpContext.Current.Request["tokenId"];
                if (HttpContext.Current.Request["commentid"] != null&& HttpContext.Current.Request["commentid"].ToString().Trim()!="")
                {
                    commentid = int.Parse(HttpContext.Current.Request["commentid"].ToString());
                }
                if (HttpContext.Current.Request["contentid"] != null && HttpContext.Current.Request["contentid"].ToString().Trim() != "")
                {
                    contentid = int.Parse(HttpContext.Current.Request["contentid"].ToString());
                }
                mrcontent = HttpContext.Current.Request["content"].ToString();

            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            var userinfo = ut.getUserInfoByToken(3,tokenId, commentid);
            if (userinfo == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            int rcontentid = 0;
            string username = "";
            using (masterEntities db = new masterEntities())
            {
                string strNow = DateTime.Now.ToString("u");

                mrcrinfo cc = new mrcrinfo();
                cc.mrcid = contentid;
                cc.mcontent = mrcontent;
                cc.mdate = strNow.Substring(0, 10);
                cc.mtime = strNow.Substring(11, 8);
                cc.suserid = userinfo[0].userid;
                username = userinfo[0].username;
                if (commentid != 0)
                {
                    cc.ruserid = userinfo[1].userid;
                    username += "=" + userinfo[1].username; ;
                    cc.mid = commentid;
                }
                
                db.mrcrinfo.Add(cc);
                db.SaveChanges();
                rcontentid = cc.mrcrid;
            }
            List<string> ListRetArrys = new List<string>();
            ListRetArrys.Add(rcontentid.ToString());
            rm.data = ListRetArrys;
            rm.RetCode = 0;
            return rm;
        }

        [HttpGet]
        [Route("EnterRoomCheck")]
        public RetMessage<string> rommEnterCheck(string tokenId, string roomid,string password)
        {
            RetMessage<string> rm = new RetMessage<string>();
            List<string> retString = new List<string>();
            var userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            int lroomid = -1;
            if (roomid != null)
            {
                lroomid = int.Parse(roomid);
            }
            using (masterEntities db = new masterEntities())
            {
                var mroominfo = db.masterinfo.Where(p => p.roomid == lroomid).FirstOrDefault();
                if (mroominfo != null&& mroominfo.key== password)
                {
                    rm.RetCode = 0;
                    retString.Add(mroominfo.masterid);
                }
                else
                {
                    if (mroominfo == null)
                    {
                        rm.RetCode = -10;
                    }
                    else
                    {
                        rm.RetCode = -11;
                    }
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                }
                rm.data = retString;
            }
            return rm;
        }

        [HttpGet]
        [Route("CreatSubRoom")]
        public RetMessage<string> CreatSubRoom(string tokenId,string roomname,string price, string begindate,string enddate,string userToken)
        {
            int sid = -1;
            List<string> sidAddy = new List<string>();
            RetMessage<string> rm = new RetMessage<string>();
            RetMessage<Order> lOrders = new RetMessage<Order>();
            masterinfo masterinfo = ut.getMasterByToken_ma(tokenId);
            if (masterinfo == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (masterEntities db = new masterEntities())
            {
                string strNow = DateTime.Now.ToString("u");
                sroominfo cc = new sroominfo();
                cc.masterid = masterinfo.masterid;
                cc.begindate = begindate;
                cc.enddate = enddate;
                cc.creatdate = strNow.Substring(0, 10);
                cc.creattime = strNow.Substring(11, 8);
                db.sroominfo.Add(cc);
                db.SaveChanges();
                sid = cc.sid;
            }
            if (userToken != null && userToken.Trim() != "" && price != null && price.Trim() != "")
            {
                //创建订单
                lOrders= ut.makeOrder(1, 100, "1", "shifuzhangsan", "openid", "userid");
                
            }
            sidAddy.Add(sid.ToString());
           
            if (lOrders.RetCode == 0)
            {
                sidAddy.Add("|");
                foreach (var aa in lOrders.data)
                {
                    sidAddy.Add("timeStamp="+ aa.timeStamp) ;
                    sidAddy.Add("nonceStr=" + aa.nonceStr);
                    sidAddy.Add("package=" + aa.package);
                    sidAddy.Add("paySign=" + aa.paySign);
                    sidAddy.Add("wx_out_trade_no=" + aa.wx_out_trade_no);
                }
            }
            rm.data = sidAddy;
            return rm;
        }

        [HttpGet]
        [Route("PayforRoom")]
        public RetMessage<string> PayForRoom(string tokenId, string masterid)
        {
            List<string> sidAddy = new List<string>();
            RetMessage<string> rm = new RetMessage<string>();
            RetMessage<Order> lOrders = new RetMessage<Order>();
            string useridandopenidandNikename = ut.getUserIDAndOpenIDAndNikeNameByToken_ma(tokenId);
            if (useridandopenidandNikename == null)
            {
                //错误返回;
                rm.RetCode = -7;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
            }
            else
            {
                string[] buff = useridandopenidandNikename.Split('|');
                string userid = buff[0];
                string openid = buff[1];
                string nikeName = buff[2];
                lOrders = ut.makeOrder(2, 100, masterid, "拜"+ nikeName+"为师", openid, userid);
                if (lOrders.RetCode == 0)
                {
                    foreach (var aa in lOrders.data)
                    {
                        sidAddy.Add("timeStamp=" + aa.timeStamp);
                        sidAddy.Add("nonceStr=" + aa.nonceStr);
                        sidAddy.Add("package=" + aa.package);
                        sidAddy.Add("paySign=" + aa.paySign);
                        sidAddy.Add("wx_out_trade_no=" + aa.wx_out_trade_no);
                    }
                }
            }
            rm.data = sidAddy;
            return rm;
        }


        private  RetMessage<ViewSRoomInfoModel> getSRoom(string tokenId)
        {
            RetMessage<ViewSRoomInfoModel> rm = new RetMessage<ViewSRoomInfoModel>();
            List<ViewSRoomInfoModel> lV = new List<ViewSRoomInfoModel>();
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            List<sroominfo> lsroominfo = new List<sroominfo>();
            using (masterEntities db = new masterEntities())
            {
                //小房间id、小房间名称、未读信息数、是否未进入过、小房间过期时间
                lsroominfo = db.sroominfo.ToList();

            }
            foreach (var aa in lsroominfo)
            {
                ViewSRoomInfoModel vsr = new ViewSRoomInfoModel();
                vsr.begindate = aa.begindate;
                vsr.enddate = aa.enddate;
                vsr.sid = aa.sid;
                vsr.title = aa.title;
                lV.Add(vsr);
            }
            rm.data = lV;
            rm.RetCode = 0;
            return rm;
        }
        [HttpGet]
        [Route("SRoomListAll")]
        public RetMessage<ViewSRoomInfoModel> getAllSRoom(string tokenId)
        {
            RetMessage<ViewSRoomInfoModel> rm = new RetMessage<ViewSRoomInfoModel>();
            rm = getSRoom(tokenId);
            return rm;
        }

        [HttpGet]
        [Route("SRContent")]
        public RetMessage<ViewSrcontenInfoModel> getSRContent(string tokenId,int sid)
        {
            List<srcontent> ltsrcontent = new List<srcontent>();
            List<r_scu> r_scus = new List<r_scu>();
            RetMessage<ViewSrcontenInfoModel> rm = new RetMessage<ViewSrcontenInfoModel>();
            rm.RetCode = -1;

            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (masterEntities db = new masterEntities())
            {
                var spayinfo = db.spayinfo.Where(p => p.userid == userid && p.sid == sid).ToList();
                if (spayinfo.Count > 0)
                {
                    r_scus = db.r_scu.Where(p => p.userid == userid).ToList();//已读信息
                    ltsrcontent = db.srcontent.Where(p =>p.sid==sid).ToList();
                }
                else
                {
                    rm.RetCode = 12;
                    rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                    return rm;
                }
            }
            foreach(var aa in ltsrcontent)
            {
                ViewSrcontenInfoModel vsm = new ViewSrcontenInfoModel();
                vsm.cdate = aa.cdate;
                vsm.ctime = aa.ctime;
                vsm.ctype = aa.ctype;
                vsm.fileurl = vsm.fileurl;
                vsm.mrcontent = aa.mrcontent;
                vsm.sid = aa.sid;
                vsm.srcid = aa.srcid;
                vsm.iHaveRead = -1;
                foreach (var bb in r_scus)
                {
                    if (bb.srcid == aa.srcid)
                    {
                        vsm.iHaveRead = 0;
                    }

                }

            }

            return rm;
        }
        [HttpGet]
        [Route("HaveNewMessage")]
        public RetMessage<int > getNewMessage(string tokenId)
        {
            RetMessage<int> rm = new RetMessage<int>();
            List<cmessageinfo> ltMessages = new List<cmessageinfo>();
            
            rm.RetCode = -1;
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (masterEntities db = new masterEntities())
            {
                ltMessages = db.cmessageinfo.Where(p => p.recipient == userid).ToList();
            }
            List<int> ri = new List<int>();
            ri.Add(ltMessages.Count);
            rm.RetCode = 0;
           return rm;
        }

        [HttpGet]
        [Route("MessageList")]
        public RetMessage<ViewMessageinfoModel> getAllMessage(string tokenId)
        {
            //私信id、发信人昵称、发信人token、发信人图像、发信时间、是否已回复
            RetMessage<ViewMessageinfoModel> rm = new RetMessage<ViewMessageinfoModel>();
            List<cmessageinfo> ltMessages = new List<cmessageinfo>();
            List<omessageinfo> ltOMessages = new List<omessageinfo>();
            List<dmessageinfo> ltDMessages = new List<dmessageinfo>();
            List<r_mu> ltr_mu = new List<r_mu>();
            //List<> ltMessages = new List<cmessageinfo>();
            //  List<cmessageinfo> ltMessages = new List<cmessageinfo>();
            rm.RetCode = -1;
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            using (masterEntities db = new masterEntities())
            {
                ltMessages = db.cmessageinfo.Where(p => p.recipient == userid).ToList();
                ltOMessages = db.omessageinfo.Where(p => p.recipient == userid).ToList();
                ltDMessages = db.dmessageinfo.Where(p => p.recipient == userid).ToList();
                ltr_mu=db.r_mu.Where(p=>p.userid==userid).ToList();


            }
            List<ViewMessageinfoModel> ri = new List<ViewMessageinfoModel>();
            foreach(var aa in ltMessages)
            {
                ViewMessageinfoModel vm = new ViewMessageinfoModel();
                vm.cdate = aa.cdate;
                vm.content = aa.content;
                vm.ctime = aa.ctime;
                vm.ctype = aa.ctype;
                vm.fileurl = aa.fileurl;
                vm.mid = aa.mid;
                vm.recipient = aa.recipient;
                vm.sender = aa.sender;
                vm.iHaveRead = -1;
                ri.Add(vm);
            }

            foreach (var aa in ltOMessages)
            {
                ViewMessageinfoModel vm = new ViewMessageinfoModel();
                vm.cdate = aa.cdate;
                vm.content = aa.content;
                vm.ctime = aa.ctime;
                vm.ctype = aa.ctype;
                vm.fileurl = aa.fileurl;
                vm.mid = aa.mid;
                vm.recipient = aa.recipient;
                vm.sender = aa.sender;
                vm.iHaveRead = -1;
                foreach(var bb in ltr_mu)
                {
                    if (aa.mid == bb.mid)
                    {
                        vm.iHaveRead = 0;
                    }

                }
                ri.Add(vm);
            }



            foreach (var aa in ltDMessages)
            {
                ViewMessageinfoModel vm = new ViewMessageinfoModel();
                vm.cdate = aa.cdate;
                vm.content = aa.content;
                vm.ctime = aa.ctime;
                vm.ctype = aa.ctype;
                vm.fileurl = aa.fileurl;
                vm.mid = aa.mid;
                vm.recipient = aa.recipient;
                vm.sender = aa.sender;
                vm.iHaveRead = -1;
                foreach (var bb in ltr_mu)
                {
                    if (aa.mid == bb.mid)
                    {
                        vm.iHaveRead = 0;
                    }

                }
                ri.Add(vm);
            }

            rm.data = ri;
            rm.RetCode = 0;
            return rm;
        }

        [HttpPost]
        [Route("PushMessage")]
        public RetMessage<string> MessagePush()
        {
            RetMessage<string> rm = new RetMessage<string>();
            string tokenId = "";
            string recipienter = "";
            string mrcontent = "";
            int classtype = -1;
            string content = "";
            string filename = "";
            try
            {
                tokenId = HttpContext.Current.Request["tokenId"];
                recipienter = HttpContext.Current.Request["recipienter"];
                if (HttpContext.Current.Request["ctype"].ToString() != null && HttpContext.Current.Request["ctype"].ToString().Trim() != "")
                    classtype = int.Parse(HttpContext.Current.Request["ctype"].ToString());
                switch (classtype)
                {
                    case 1://文字
                        content = HttpContext.Current.Request["content"];
                        break;
                    case 2://图片，视频,声音
                        HttpFileCollection files = HttpContext.Current.Request.Files;
                        string fileurl = "";
                        foreach (string f in files.AllKeys)
                        {
                            HttpPostedFile file = files[f];
                            fileurl += file.FileName;

                            if (string.IsNullOrEmpty(file.FileName) == false)
                                file.SaveAs(HttpContext.Current.Server.MapPath("~/UploadedData/")  + file.FileName);
                        }
                        filename = fileurl;
                        break;
                }
            }
            catch
            {
                rm.RetCode = 97;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            var userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            int retID = -1;
            using (masterEntities db = new masterEntities())
            {

                cmessageinfo cc = new cmessageinfo();
                cc.recipient = recipienter;
                string strNow = DateTime.Now.ToString("u");
                cc.cdate = strNow.Substring(0, 10);
                cc.cdate = strNow.Substring(11, 8);
                if (classtype == 1)
                {
                    cc.content = mrcontent;
                }
                if (classtype == 2)
                {
                    cc.fileurl = filename;
                }
                db.cmessageinfo.Add(cc);
                db.SaveChanges();
                retID = cc.mid;
            }

            List<string> ListRetArrys = new List<string>();
            ListRetArrys.Add(retID.ToString());
            rm.data = ListRetArrys;
            rm.RetCode = 0;
            return rm;
        }

        [HttpGet]
        [Route("MessageInfo")]
        public RetMessage<ViewMessageinfoModel> getMessage(string tokenId, int mid)
        {
            RetMessage<ViewMessageinfoModel> rm = new RetMessage<ViewMessageinfoModel>();
            ViewMessageinfoModel vm = new ViewMessageinfoModel();
            List<r_mu> ltr_mu = new List<r_mu>();

            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            using (masterEntities db = new masterEntities())
            {
                ltr_mu = db.r_mu.Where(p => p.userid == userid).ToList();
                var cmessageinfo = db.cmessageinfo.Find(mid);
                if (cmessageinfo != null)
                {
                    vm.cdate = cmessageinfo.cdate;
                    vm.content = cmessageinfo.content;
                    vm.ctime = cmessageinfo.ctime;
                    vm.ctype = cmessageinfo.ctype;
                    vm.fileurl = cmessageinfo.fileurl;
                    vm.mid = cmessageinfo.mid;
                    vm.recipient = cmessageinfo.recipient;
                    vm.sender = cmessageinfo.sender;
                    vm.iHaveRead = -1;
                }
                else
                {
                    var omessageinfo = db.omessageinfo.Find(mid);
                    if (omessageinfo != null)
                    {

                        vm.cdate = omessageinfo.cdate;
                        vm.content = omessageinfo.content;
                        vm.ctime = omessageinfo.ctime;
                        vm.ctype = omessageinfo.ctype;
                        vm.fileurl = omessageinfo.fileurl;
                        vm.mid = omessageinfo.mid;
                        vm.recipient = omessageinfo.recipient;
                        vm.sender = omessageinfo.sender;
                        vm.iHaveRead = -1;
                    }
                    else
                    {
                        var dmessageinfo = db.omessageinfo.Find(mid);
                        if (dmessageinfo != null)
                        {
                            vm.cdate = dmessageinfo.cdate;
                            vm.content = dmessageinfo.content;
                            vm.ctime = dmessageinfo.ctime;
                            vm.ctype = dmessageinfo.ctype;
                            vm.fileurl = dmessageinfo.fileurl;
                            vm.mid = dmessageinfo.mid;
                            vm.recipient = dmessageinfo.recipient;
                            vm.sender = dmessageinfo.sender;
                            vm.iHaveRead = -1;
                        }
                    }
                }
                foreach (var bb in ltr_mu)
                {
                    if (vm.mid == bb.mid)
                    {
                        vm.iHaveRead = 0;
                    }

                }
            }
            return rm;
        }

        [HttpGet]
        [Route("WalletInfo")]
        public RetMessage<ViewWallet> getViewWallet(string tokenId)
        {

            RetMessage<ViewWallet> rm = new RetMessage<ViewWallet>();
            List<ViewWallet> ltv = new List<ViewWallet>();
            List<wallet> wallets = new List<wallet>();
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }

            using (masterEntities db = new masterEntities())
            {
                 wallets = db.wallet.Where(p => p.userid == userid).ToList();
            }
            if(wallets!=null&& wallets.Count > 0)
            {
                int curAmount = 0;
                int getedAmount = 0;
                List<getAmount> ltgetamount = new List<getAmount>();
                foreach (var aa in wallets)
                {
                    //（提现申请时间、提现金额、提现状态）
                    getAmount getAmounts = new getAmount();
                    getAmounts.applytime = aa.wdate+aa.wtime;
                    getAmounts.applyamount = aa.amount;
                    getAmounts.applyed = 1;
                    ltgetamount.Add(getAmounts);
                    if (aa.subjects < 0)
                        curAmount += int.Parse(aa.amount);
                    else
                        getedAmount += int.Parse(aa.amount);
                }
                ViewWallet vw = new ViewWallet();
                vw.curAmount = curAmount;
                vw.applyAmount = curAmount - getedAmount;

                vw.getAmount = ltgetamount;
            }
           
                return rm;

        }

        [HttpGet]
        [Route("WalletGet")]
        public RetMessage<string> getWallet(string tokenId,string amount)
        {

            RetMessage<string > rm = new RetMessage<string>();
            List<string> lr = new List<string>();
            string userid = ut.getUserIDByToken_ma(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }
            int retID = -1;
            using (masterEntities db = new masterEntities())
            {
                wallet cc = new wallet();
                string strNow = DateTime.Now.ToString("u");
                cc.wdate = strNow.Substring(0, 10);
                cc.wtime = strNow.Substring(11, 8);
                cc.amount = amount;
                cc.subjects = 1;
                cc.userid = userid;
                db.wallet.Add(cc);
                db.SaveChanges();
                retID = cc.wid;
            }
            lr.Add(retID.ToString());
            rm.RetCode = 0;
            rm.data = lr;
            return rm;
        }


    }
}
