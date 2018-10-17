using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 错误信息
    /// </summary>
    [Serializable]
    public class Wx_ErrorMessage
    {
        public string errcode { get; set; } = "0";//错误码
        public string errmsg { get; set; } = string.Empty; //错误信息

    }
    [Serializable]
    public class WxAuthorize : Wx_ErrorMessage
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string openid { get; set; }
        public string scope { get; set; }

        public string unionid { get; set; }

    }
    [Serializable]
    public class GongZhongToken : Wx_ErrorMessage
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public DateTime AddTime { get; set; }
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string AppId { get; set; }
    }
    [Serializable]
    public class WxUser : Wx_ErrorMessage
    {
        public string openid { get; set; } = string.Empty;
        public string unionid { get; set; } = string.Empty;
        public string nickname { get; set; } = string.Empty;
        public int sex { get; set; } = 0;
        public string province { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string headimgurl { get; set; } = string.Empty;
        public string serverid { get; set; } = string.Empty;
    }
}
