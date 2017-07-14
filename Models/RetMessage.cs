using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZeroStartAPI.Models
{
    public class RetMessage<T>
    {
        public int RetCode { get; set; }
        public string ErrorMsg { get; set; }
        public List <T>  data { get; set; }
    }
}