using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.MiniApp.User;
using Entity.MiniApp.Conf;
using Entity.MiniApp;

namespace Entity.MiniApp.User
{
    [Serializable]
    public class C_ApiUserInfo
    {
        public int userid { get; set; }
        public string sessionId { get; set; }
        public string openId { get; set; }
        public string nickName { get; set; }
        public string gender { get; set; }
        public string language { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string avatarUrl { get; set; }
        public string unionId { get; set; }
        public string tel { get; set; }
        public  int IsValidTelePhone { get; set; }
        public C_ApiWaterMark watermark { get; set; }
        public string openGId { get; set; }
        public int iscityowner { get; set; }
        public string phoneNumber { get; set; }
        public string purePhoneNumber { get; set; }
        public string countryCode { get; set; }
        /// <summary>
        /// 登陆验证码（缓存半天,Guid.NewGuid()）
        /// </summary>
        public string loginSessionKey { get; set; }
    }

    public class C_ApiWaterMark
    {
        public int timestamp { get; set; }
        public string appid { get; set; }
    }
}
