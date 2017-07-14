using LitJson2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using ZeroStartAPI.Models;

namespace ZeroStartAPI.Controllers
{
    public class RobotController : ApiController
    {
        utility ut = new utility();
        [HttpPost]
        [Route("getRobot")]
        public RetMessage<string> getAnswers()
        {
            RetMessage<string> rm = new RetMessage<string>();
            /*
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
            
            string userid = ut.getUserIDByToken(tokenId);
            if (userid == null)
            {
                rm.RetCode = 99;
                rm.ErrorMsg = ut.getErrMessage(rm.RetCode);
                return rm;

            }*/
            /*
             *  var url = 'http://www.tuling123.com/openapi/api?key=eeec171e907553c15aa3131562f75903&info=' + info;
    wx.request( {
      url: url,
      data: {

      },
      header: {
        'Content-Type': 'application/json'
      },
      success: function( res ) {
        console.log( res.data )
        typeof cb == "function" && cb( res.data )
      }
             */
            /*
             * API地址:
http://www.tuling123.com/openapi/api
APIkey:
5efbdf4d27bf4b4a9cb709caf7c83a31
secret：
47526d089c54663e
ON

            
		//生成密钥
		String keyParam = secret+timestamp+apiKey;
		String key = Md5.MD5(keyParam);
             */
            /*
          string timestamp=ut.GetTimeStamp();
          const string secret= "47526d089c54663e";
         
          string keyParam = secret + timestamp + aPIkey;
          MD5 md5 = new MD5CryptoServiceProvider();
          byte[] result = Encoding.UTF8.GetBytes(keyParam);
          byte[] output = md5.ComputeHash(result);
          string str = BitConverter.ToString(output);
          Aes.Create(str);

          rebotUrlObject rbuo = new rebotUrlObject();
          rbuo.key = aPIkey;
          rbuo.timestamp = timestamp;
          rbuo.data = str;*/
            const string aPIkey = "5efbdf4d27bf4b4a9cb709caf7c83a31";
            var sb = new StringBuilder();
            var formVars = new Dictionary<string, string>();
            formVars.Add("key", aPIkey);
            formVars.Add("info", "bim是什么");
            foreach (var d in formVars)
            {
                sb.Append(d.Key + "：" + d.Value);
            }
            string response = ut.CreatedPostHttpResponse("http://www.tuling123.com/openapi/api", sb);


         
            JsonData jd = new JsonData();
            jd["key"] = aPIkey;
   //         jd["timestamp"] = timestamp;//**新添加一层关系时，需要再次 new** JsonData（）
            jd["info"] = "bim是什么";

           
            var content = new FormUrlEncodedContent(formVars);

            var client = new HttpClient();
            var postUrl = "http://www.tuling123.com/openapi/api";
            var task = client.PostAsync(postUrl, content).Result;  //参数未进行序列化
            var cc=task.RequestMessage.ToString();



            //http://www.tuling123.com/openapi/api?key=5efbdf4d27bf4b4a9cb709caf7c83a31&info=bim是什么
            var url = "http://www.tuling123.com/openapi/api";
            var httpClient = new HttpClient();
            // var responseJson = httpClient.PostAsJsonAsync(url, jd.ToString()).Result.Content.ReadAsStringAsync().Result;
            var responseJson = httpClient.PostAsJsonAsync(url,jd.ToString()).Result.Content.ReadAsStringAsync().Result;

            /*
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
            }*/
            rm.RetCode = 0;
            return rm;
        }

    }
}
