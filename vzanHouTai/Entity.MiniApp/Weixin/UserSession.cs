using Senparc.Weixin.MP.AdvancedAPIs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Weixin
{
    /// <summary>
    /// 小程序用户解密后的数据
    /// </summary>
    public class UserSession
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }
        public string openid { get; set; }
        public string unionid { get; set; }
        public string session_key { get; set; }
        public string code { get; set; }
        public string vector { get; set; }
        public bool verify()
        {
            return !(string.IsNullOrWhiteSpace(openid) || string.IsNullOrWhiteSpace(session_key) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(vector));
        }
        public string enData { get; set; }
        public string deData { get; set; }
        public string signature { get; set; }
    }
}
