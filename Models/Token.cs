using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZeroStartAPI.Models
{
    public class Token
    {
        public string openID{ get; set; }
        public string  token { get; set; }
      
        public void tokenMake(string openID,string session_key,int flag)
        {
            string strUserid = "";
            switch (flag)
            {
                case 1:
                    using (dataserverEntities db = new dataserverEntities())
                    {
                        userinfo lui = db.userinfo.FirstOrDefault(x => x.openid == openID);
                        using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                        {
                            int iHave = -1;
                            if (lui != null && lui.userid.Length > 0)
                            {
                                strUserid = lui.userid.ToString().Trim();
                                iHave = 0;
                            }
                            else
                            {
                                Guid uGid = Guid.NewGuid();
                                strUserid = uGid.ToString();
                                userinfo ui = new userinfo();
                                ui.openid = openID;
                                ui.userid = strUserid;
                                db.userinfo.Add(ui);
                                db.SaveChanges();

                            }

                            DateTime ExpireTime = DateTime.Now;
                            var hash = System.Security.Cryptography.MD5.Create();
                            var signStr = openID + strUserid + ExpireTime;

                            var sortStr = string.Concat(signStr.OrderBy(d => d));
                            var bytes = System.Text.Encoding.UTF8.GetBytes(sortStr);
                            //使用MD5加密
                            var md5Val = hash.ComputeHash(bytes);
                            //把二进制转化为大写的十六进制
                            System.Text.StringBuilder result = new System.Text.StringBuilder();
                            foreach (var c in md5Val)
                            {
                                result.Append(c.ToString("X2"));
                            }
                            token = result.ToString().ToUpper();
                            uts luts = new uts();
                            luts.token = token;
                            luts.userid = strUserid;
                            luts.session_key = session_key;
                            if (iHave == 0)
                            {
                                db.Entry(luts).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                db.uts.Add(luts);
                            }

                            db.SaveChanges();
                            scope.Complete();
                        }

                    }
                    break;
                case 2:
                    using (organizeEntities db = new organizeEntities())
                    {
                        userinfo lui = db.userinfo.FirstOrDefault(x => x.openid == openID);
                        using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                        {
                            int iHave = -1;
                            if (lui != null && lui.userid.Length > 0)
                            {
                                strUserid = lui.userid.ToString().Trim();
                                iHave = 0;
                            }
                            else
                            {
                                Guid uGid = Guid.NewGuid();
                                strUserid = uGid.ToString();
                                userinfo ui = new userinfo();
                                ui.openid = openID;
                                ui.userid = strUserid;
                                db.userinfo.Add(ui);
                                db.SaveChanges();

                            }

                            DateTime ExpireTime = DateTime.Now;
                            var hash = System.Security.Cryptography.MD5.Create();
                            var signStr = openID + strUserid + ExpireTime;

                            var sortStr = string.Concat(signStr.OrderBy(d => d));
                            var bytes = System.Text.Encoding.UTF8.GetBytes(sortStr);
                            //使用MD5加密
                            var md5Val = hash.ComputeHash(bytes);
                            //把二进制转化为大写的十六进制
                            System.Text.StringBuilder result = new System.Text.StringBuilder();
                            foreach (var c in md5Val)
                            {
                                result.Append(c.ToString("X2"));
                            }
                            token = result.ToString().ToUpper();
                            uts luts = new uts();
                            luts.token = token;
                            luts.userid = strUserid;
                            luts.session_key = session_key;
                            if (iHave == 0)
                            {
                                db.Entry(luts).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                db.uts.Add(luts);
                            }

                            db.SaveChanges();
                            scope.Complete();
                        }

                    }
                    break;
                case 3:
                    using (masterEntities db = new masterEntities())
                    {
                        userinfo lui = db.userinfo.FirstOrDefault(x => x.openid == openID);
                        using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                        {
                            int iHave = -1;
                            if (lui != null && lui.userid.Length > 0)
                            {
                                strUserid = lui.userid.ToString().Trim();
                                iHave = 0;
                            }
                            else
                            {
                                Guid uGid = Guid.NewGuid();
                                strUserid = uGid.ToString();
                                userinfo ui = new userinfo();
                                ui.openid = openID;
                                ui.userid = strUserid;
                                db.userinfo.Add(ui);
                                db.SaveChanges();

                            }

                            DateTime ExpireTime = DateTime.Now;
                            var hash = System.Security.Cryptography.MD5.Create();
                            var signStr = openID + strUserid + ExpireTime;

                            var sortStr = string.Concat(signStr.OrderBy(d => d));
                            var bytes = System.Text.Encoding.UTF8.GetBytes(sortStr);
                            //使用MD5加密
                            var md5Val = hash.ComputeHash(bytes);
                            //把二进制转化为大写的十六进制
                            System.Text.StringBuilder result = new System.Text.StringBuilder();
                            foreach (var c in md5Val)
                            {
                                result.Append(c.ToString("X2"));
                            }
                            token = result.ToString().ToUpper();
                            uts luts = new uts();
                            luts.token = token;
                            luts.userid = strUserid;
                            luts.session_key = session_key;
                            if (iHave == 0)
                            {
                                db.Entry(luts).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                db.uts.Add(luts);
                            }
                            db.SaveChanges();
                            scope.Complete();
                        }
                    }
                    break;
            }
        }
    }
}