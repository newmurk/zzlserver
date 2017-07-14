using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Routing;
using ZeroStartAPI.Controllers;
namespace ZeroStartAPI.App_Start
{

    public class SiteRouteCustomer : ActionFilterAttribute
    {
       
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var sc=HttpContext.Current.Request.Headers.Get("sc");
            //requestSource="zzl|0x119|sfsdfsdff"

            //requestSource = "zzl|0x95e516cf|D279882E8B8198C9AB43A541B6B04BA1"
            if (sc != null)
            {
                var typeName = actionContext.ControllerContext.GetType().FullName;
                string aa = actionContext.ActionDescriptor.ActionName;
                var c = HttpContext.Current.Request.RawUrl;
                string strKey = "";
                Regex rg = new Regex(strKey);
                string []buff = c.Split('?');
                sc = sc + buff[0];

                for (int ii = 1; ii < buff.Count() - 1; ii++)
                {
                    sc += buff[ii];
                }

                HttpContext.Current.Response.Redirect(sc);
                HttpContext.Current.Response.End();
                HttpContext.Current.Response.Flush();
            }
            /*
          using (dataserverEntities db = new dataserverEntities())
          {
              syslog cc = new syslog();
              var rs = RouteTable.Routes;

              //取资料主档信息
              cc = transaction_id;
              db.SaveChanges();
          }
          byte[] byts = new byte[HttpContext.Current.Request.InputStream.Length];
          System.Web.HttpContext.Current.Request.InputStream.Position = 0;
          HttpContext.Current.Request.InputStream.Read(byts, 0, byts.Length);
          string req = Encoding.UTF8.GetString(byts);
          var rs = RouteTable.Routes.ToString();
           string a = "统一处理";*/
            //var bb = 0;
        }
    }
}