using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.MiniApp.User;

/// <summary>
/// Member转移过来的
/// </summary>
namespace Entity.MiniApp
{
    public class LoginQrCode
    {
        public string OpenId { get; set; }
        public string SessionId { get; set; }
        public WeiXinUser WxUser { get; set; }
        public bool IsLogin { get; set; }
        public string unionid { get; set; }
    }
}
