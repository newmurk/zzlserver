using LitJson2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;
using ZeroStartAPI.Models;

namespace ZeroStartAPI.Controllers
{
    public class utility
    {
        public string getErrMessage(int messageId)
        {
            string errMessage = "";
            switch (messageId)
            {
                case 0: break;
                case -1: errMessage = "没找到openID"; break;
                case -2: errMessage = "统一下单失败"; break;
                case -3: errMessage = "tokenId为空"; break;
                case -4: errMessage = "取openid失败"; break;
                case -5: errMessage = "Https请求失败"; break;
                case -6: errMessage = "取主档信息失败"; break;
                case -7: errMessage = "取userid和openid失败"; break;
                case -8: errMessage = "没有调查问卷"; break;
                case -9: errMessage = "获取access_token失败"; break;
                case -10: errMessage = "获取房间错误"; break;
                case -11: errMessage = "口令不正确"; break;
                case -12: errMessage = "没有购买小房间服务"; break;
                case -13: errMessage = "取师傅错误"; break;
                case -21: errMessage = "文件或目录不存在"; break;
                case -22: errMessage = "删除文件失败"; break;
                case 97: errMessage = "缺少必要内容"; break;
                case 98: errMessage = "文件大小超过限制"; break;
                case 99: errMessage = "根据token取用户出错"; break;

            }
            return errMessage;
        }
        public string getAppSetting(int appSettingId)
        {
            string[] appBuff = { "wx_appid", "wx_mch_id", "wx_key",
                "wxpay_notifyurl", "wx_spbill_create_ip","wx_trade_type",
                "secret","grant_type","strcode",
                "baseReaderCount","sy_catalogid","wx_appid_zzl",
                "secret_zzl","total_fee_zzl","expertid",
                "organizationid","wx_appid_m","secret_m","grant_type_m"};
            string RetAppSetting = ConfigurationManager.AppSettings[appBuff[appSettingId]];
            return RetAppSetting;
        }
        public string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + getRandomString(10);
        }
        public masterinfo getMasterByToken_ma(string strToken)
        {
            string strUserid = null;
            masterinfo strMasterid = new masterinfo();
            if (strToken != null && strToken.Trim() != "")
            {
                using (masterEntities db = new masterEntities())
                {
                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                    strMasterid = db.masterinfo.Where(p => p.masterid == strUserid).FirstOrDefault();
                }
            }
            return strMasterid;
        }
        public string getUserIDByToken_ma(string strToken)
        {
            string strUserid = null;
            if (strToken != null && strToken.Trim() != "")
            {
                using (masterEntities db = new masterEntities())
                {
                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                }
            }
            return strUserid;
        }
        public string getUserIDByToken_og(string strToken)
        {
            string strUserid = null;
            if (strToken != null && strToken.Trim() != "")
            {
                using (organizeEntities db = new organizeEntities())
                {
                    var ups = from uts in db.uts
                              where uts.token == strToken
                              select uts;
                    List<uts> ua = ups.ToList<uts>();

                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                }
            }
            return strUserid;
        }
        public string getUserIDByToken(string strToken)
        {
            string strUserid = null;
            if (strToken != null && strToken.Trim() != "")
            {
                using (dataserverEntities db = new dataserverEntities())
                {
                    //strUserid = db.uts.FirstOrDefault(p => p.token == strToken).userid;
                    var ups = from uts in db.uts
                              where uts.token == strToken
                              select uts;
                    List<uts> ua = ups.ToList<uts>();

                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                }
            }
            return strUserid;
        }
        public List <userinfo> getUserInfoByToken(int flag,string strToken,int commentid)
        {
            List<userinfo> retUserinfos = new List<userinfo>();
            if (strToken != null && strToken.Trim() != "")
            {
                switch (flag)
                {
                    case 1:
                        using (dataserverEntities db = new dataserverEntities())
                        {
                            var strUserid = db.uts.FirstOrDefault(p => p.token == strToken).userid;
                            retUserinfos = db.userinfo.Where(p => p.userid == strUserid).ToList();
                        }
                        break;
                    case 2:
                        using (organizeEntities db = new organizeEntities())
                        {
                            var strUserid = db.uts.FirstOrDefault(p => p.token == strToken).userid;
                            retUserinfos = db.userinfo.Where(p => p.userid == strUserid).ToList();
                        }
                        break;
                    case 3:
                        using (masterEntities db = new masterEntities())
                        {
                            if (commentid > 0)
                            {
                                
                                var strUserid = db.uts.FirstOrDefault(p => p.token == strToken).userid;
                                List<String> userArry = new List<string>();
                                userArry.Add(strUserid);

                                var ltmrcrinfo = db.mrcrinfo.Where(p => p.mrcrid == commentid).ToList();
                                var userid = "";
                                if (ltmrcrinfo != null && ltmrcrinfo.Count > 0)
                                {
                                    userid = ltmrcrinfo[0].suserid.ToString();
                                    userArry.Add(userid);

                                }
                                foreach(var aa in userArry)
                                {
                                    var tt = db.userinfo.Where(p => p.userid == aa).FirstOrDefault();
                                    if (tt != null)
                                    {
                                        retUserinfos.Add(tt);
                                    }
                                }
                            }
                            else
                            {
                                var strUserid = db.uts.FirstOrDefault(p => p.token == strToken).userid;
                                retUserinfos = db.userinfo.Where(p => p.userid == strUserid).ToList();

                            }

                        }
                        break;
                }
               
            }
            return retUserinfos;
        }
        public string getAmoutByDataId(int dataId)
        {
            string amout = null;
            using (dataserverEntities db = new dataserverEntities())
            {
                var di = db.datainfo.Find(dataId);
                amout = (int.Parse(di.amount) * 100).ToString();

            }
            return amout;
        }
        public string getUserIDAndOpenIDByToken_og(string strToken)
        {
            string strUserid = "";
            string openID = "";
            if (strToken != null && strToken.Trim() != "")
            {
                using (organizeEntities db = new organizeEntities())
                {
                    // var uts = db.uts.Where(p => p.token == strToken);
                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                    openID = db.userinfo.Where(p => p.userid == strUserid).Select(t => t.openid.ToString()).FirstOrDefault();
                }
            }
            return strUserid + "|" + openID;
        }
        public string getUserIDAndOpenIDAndNikeNameByToken_ma(string strToken)
        {
            string strUserid = "";
            string openID = "";
            string nikeName = "";
            if (strToken != null && strToken.Trim() != "")
            {
                using (masterEntities db = new masterEntities())
                {
                    // var uts = db.uts.Where(p => p.token == strToken);
                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                    var userinfo = db.userinfo.Where(p => p.userid == strUserid).FirstOrDefault();
                    openID = userinfo.openid;
                    nikeName = userinfo.username;

                }
            }
            return strUserid + "|" + openID + "|"+ nikeName;
        }
        public string getUserIDAndOpenIDByToken(string strToken)
        {
            string strUserid = "";
            string openID = "";
            if (strToken != null && strToken.Trim() != "")
            {
                using (dataserverEntities db = new dataserverEntities())
                {
                    // var uts = db.uts.Where(p => p.token == strToken);
                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                    openID = db.userinfo.Where(p => p.userid == strUserid).Select(t => t.openid.ToString()).FirstOrDefault();
                }
            }
            return strUserid + "|" + openID;
        }
        public string getOpenIDByToken(string strToken)
        {
            string strUserid = strToken;
            string openID = "";
            if (strToken != null && strToken.Trim() != "")
            {
                using (dataserverEntities db = new dataserverEntities())
                {
                    strUserid = db.uts.Where(p => p.token == strToken).Select(t => t.userid.ToString()).FirstOrDefault();
                    openID = db.userinfo.Where(p => p.userid == strUserid).Select(t => t.openid.ToString()).FirstOrDefault();
                }
            }
            return openID;
        }
        public static HttpWebResponse CreateGetHttpResponse(string url, string userAgent, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = userAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }
        public string GetSignString(Dictionary<string, string> dic)
        {
            utility ut = new utility();
            string wx_key = ut.getAppSetting(2);
            string key = wx_key;
            dic = dic.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            var sign = dic.Aggregate("", (current, d) => current + (d.Key + "=" + d.Value + "&"));
            sign += "key=" + key;
            byte[] result = Encoding.UTF8.GetBytes(sign);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string str = BitConverter.ToString(output).Replace("-", "").ToUpper();
            sign = str;
            return sign;
        }
        public string CreatedPostHttpResponse(string url, StringBuilder sb)
        {
            var httpClient = new HttpClient();
            var responseJson = httpClient.PostAsJsonAsync(url, sb).Result.Content.ReadAsStringAsync().Result;

            return responseJson;
        }
        public string getRandomString(int length)
        {
            //const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Guid uGid = Guid.NewGuid();
            string str = uGid.ToString().Replace("-", "");

            Random random = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int number = random.Next(32);
                sb.Append(str[number]);
            }
            return sb.ToString();
        }

        public void MakeThumbnail(MemoryStream originalImagePath, MemoryStream thumbnailPath, int width, int height, string mode)
        {
            Image originalImage = Image.FromStream(originalImagePath);
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）   
                    break;
                case "W"://指定宽，高按比例   
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H"://指定高，宽按比例
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut"://指定高宽裁减（不变形）   
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }
            //新建一个bmp图片
            Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充
            g.Clear(Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
           new Rectangle(x, y, ow, oh),
           GraphicsUnit.Pixel);
            try
            {
                //以jpg格式保存缩略图
                bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }
        public Image CutEllipse(Image img, Rectangle rec, Size size)
        {
            Bitmap bitmap = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (TextureBrush br = new TextureBrush(img, System.Drawing.Drawing2D.WrapMode.Clamp, rec))
                {
                    br.ScaleTransform(bitmap.Width / (float)rec.Width, bitmap.Height / (float)rec.Height);

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.FillEllipse(br, new Rectangle(Point.Empty, size));
                }
            }
            return bitmap;
        }
        public string CombinImage(MemoryStream ms, MemoryStream mdestImg,string roomkey, string userid)
        {
            //string destImgN = destImg + @".png";
            Image imgBack = System.Drawing.Image.FromStream(ms);     //相框图片 
            MemoryStream msr = new MemoryStream();
            MakeThumbnail(mdestImg, msr, 150, 150, "HW");
            // MakeThumbnail(mdestImg, msr, 145, 145, "HW");
            Image img = Image.FromStream(msr);

            // Image newImage = CutEllipse(img, new Rectangle(0, 0, 145, 145), new Size(145, 145));
            Image newImage = CutEllipse(img, new Rectangle(0, 0, 150, 150), new Size(150, 150));
            Graphics g = Graphics.FromImage(imgBack);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.DrawImage(newImage, 145, 145, 145, 145);
            g.DrawImage(newImage, 128, 128, 175, 175);
            img.Dispose();
            //imgBack.Save( destImg+ @"-f.png");
            Image imgBigBack = Image.FromFile(HttpContext.Current.Server.MapPath("~/App_Data/") + "zsfn.jpg");
            Graphics gf = Graphics.FromImage(imgBigBack);
            gf.SmoothingMode = SmoothingMode.AntiAlias;
            gf.DrawImage(imgBack, 950, 30, 175, 175);
            string roomkeytext="";
            foreach(var aa in roomkey)
            {
                roomkeytext += aa + "  ";
            }
            gf.DrawString(roomkeytext, new Font("arial", 40, FontStyle.Regular), Brushes.White, new PointF(200, 340));
            //imgBigBack.Save(HttpContext.Current.Server.MapPath("~/UploadedData/aa.png"));
            
            string picSavedPath = userid + @".png";
            // imgBack.Save(HttpContext.Current.Server.MapPath("~/App_Data/") + picSavedPath);
            imgBigBack.Save(HttpContext.Current.Server.MapPath("~/UploadedData/") + picSavedPath);
            imgBack.Dispose();
            newImage.Dispose();
            imgBigBack.Dispose();
            GC.Collect();
            return picSavedPath;
        }
        public string PostMoths(string access_token, MemoryStream muserpic, string roomkey,string userid,int roomid)
        {
                         //  http://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=ACCESS_TOKEN
            string _url = "http://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token=" + access_token;
            //ttp://api.weixin.qq.com/wxa/getwxacodeunlimit?
            string strURL = _url;
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            JsonData _data = new JsonData();
            //_data["scene"] = "br_"+ userid;
            _data["scene"] = roomid;
            _data["width"] = "430";
           // _data["width"] = "153";
            string _jso = _data.ToJson();
            //string paraUrlCoded = param;
            byte[] payload;
            //payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
            payload = System.Text.Encoding.UTF8.GetBytes(_jso);
            request.ContentLength = payload.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();

            // string picpath=HttpContext.Current.Server.MapPath("~/App_Data/");
            byte[] tt = StreamToBytes(s);
            /*
            //将流保存在c盘test.png文件下
            string picpath=HttpContext.Current.Server.MapPath("~/App_Data/");
            string portrait = picpath + @"portrait.png";
            System.IO.File.WriteAllBytes(portrait, tt);*/
            MemoryStream ms = new MemoryStream(tt);
            ms.Seek(0, SeekOrigin.Begin);
            /*

            WebRequest myrequest = WebRequest.Create(userpic);
            WebResponse myresponse = myrequest.GetResponse();
            Stream imgstream = myresponse.GetResponseStream();
            System.Drawing.Image img = System.Drawing.Image.FromStream(imgstream);
            //img.Save(Server.MapPath("test.jpg"),System.Drawing.Imaging.ImageFormat.Jpeg);
            MemoryStream ms1 = new MemoryStream();
            img.Save(ms1, System.Drawing.Imaging.ImageFormat.Jpeg);
           */

            var aa = CombinImage(ms, muserpic, roomkey,userid);
            ms.Close();
            ms.Dispose();
            muserpic.Close();
            muserpic.Dispose();
            //  aa.Save(picpath+ @"userportrait.png");
            return aa;
        }
        ///将数据流转为byte[]
        public byte[] StreamToBytes(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            int temp = stream.ReadByte();
            while (temp != -1)
            {
                bytes.Add((byte)temp);
                temp = stream.ReadByte();
            }
            return bytes.ToArray();
        }
        public ImgMem getUserPicByUserid(string userid)
        {
            ImgMem retIm = new ImgMem();
            retIm.succeed = -1;
            string strFilePath = "";
            masterinfo master = new masterinfo();
            using (masterEntities db = new masterEntities())
            {
                master = db.masterinfo.Where(p => p.masterid == userid).FirstOrDefault();
                strFilePath = master.picurl;
            }
            if (strFilePath != null && strFilePath.Trim() != "")
            {
                WebClient client = new WebClient();
                byte[] bytes = client.DownloadData(new Uri(strFilePath));
                MemoryStream ms = new MemoryStream(bytes);
                ms.Seek(0, SeekOrigin.Begin);
                // ms.Close();
                retIm.succeed = 0;
                retIm.picm = ms;
                retIm.roomkey = master.key;
                retIm.roomid = master.roomid;
                /*
                string filename = HttpContext.Current.Server.MapPath("~/App_Data/") + userid;
                Stream sr = new FileStream(filename, FileMode.OpenOrCreate);
                strFilePath = filename;
                ms.WriteTo(sr);
                sr.Close();
                ms.Close();
                ms.Dispose();
                sr.Dispose();*/
            }
            return retIm;
        }

        public RetMessage<Order> makeOrder(int payflag, int price, string goodid, string title, string openid, string userid)
        {
            RetMessage<Order> rm = new RetMessage<Order>();
            string strcode = null;
            string total_fee = null;
            int appidclass = -1;
            switch (payflag)
            {
                case 1:
                    strcode = "AIB平台-[" + title + "]";
                    total_fee = (price * 100).ToString();
                    appidclass = 16;
                    break;
            }
            string wx_appid = getAppSetting(appidclass);
            string wx_mch_id = getAppSetting(1);
            string wx_nonce_str = getRandomString(20);
            byte[] buffer = Encoding.UTF8.GetBytes(strcode);
            string wx_body = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            string wx_out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmss") + getRandomString(10);
            string wx_total_fee = ((int)(decimal.Parse(total_fee))).ToString();//unit:fen
            string wx_spbill_create_ip = getAppSetting(4);
            string wx_notify_url = getAppSetting(3);
            string wx_trade_type = getAppSetting(5);
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
            dic.Add("sign", GetSignString(dic));
            var sb = new StringBuilder();
            sb.Append("<xml>");
            foreach (var d in dic)
            {
                sb.Append("<" + d.Key + ">" + d.Value + "</" + d.Key + ">");
            }
            sb.Append("</xml>");
            string response = CreatedPostHttpResponse("https://api.mch.weixin.qq.com/pay/unifiedorder", sb);
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
                if (payflag == 1)
                {
                    using (masterEntities db = new masterEntities())
                    {
                        string strNow = DateTime.Now.ToString("u");
                        mpayinfo pi = new mpayinfo();
                        //pi.dataid = dataId;
                        pi.paydate = strNow.Substring(0, 10);
                        pi.paytime = strNow.Substring(10, 8);
                        //(int)decimal.Parse(total_fee)*100
                        pi.paymount = total_fee;//total_fee
                        pi.out_trade_no = wx_out_trade_no;
                        pi.masterid = goodid;
                        pi.userid = userid;
                        db.mpayinfo.Add(pi);
                        db.SaveChanges();

                    }
                }


                //wx_out_trade_no
                var res = new Dictionary<string, string> {
                    {"appID",wx_appid},
                    { "nonceStr",ds.Tables[0].Rows[0]["nonce_str"].ToString()},
                    { "package","prepay_id="+ ds.Tables[0].Rows[0]["prepay_id"].ToString()},
                    { "signType","MD5"},
                    { "timeStamp",GetTimeStamp()},
                    };
                res.Add("paySign", GetSignString(res));
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
                rm.ErrorMsg = getErrMessage(rm.RetCode);
            }
            return rm;
        }
    }
}