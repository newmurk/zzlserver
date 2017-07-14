using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZeroStartAPI.Models
{
    public class Order
    {
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }
        public string package { get; set; }
        public string paySign { get; set; }
        public string wx_out_trade_no { get; set; }
    }
}