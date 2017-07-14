using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ZeroStartAPI.Controllers
{
    public class ApiBaseController : ApiController
    {
        public interface iutility
        {
            string getErrMessage(int messageId);
        }
    }
}
