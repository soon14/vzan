using BLL.MiniApp;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Pin;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Pin;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace User.MiniApp.Areas.PinAdmin.Filters
{
    public class LoginFilterAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //TODO 开发阶段屏蔽登陆验证
            //return;
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;

            int storeId = Context.GetRequestInt("storeId", 0);

            if (storeId > 0)
            {
                string cookieSoreId = CookieHelper.GetCookie("dzPinAdmin");
                //如果没有找到登陆cookie
                if (string.IsNullOrEmpty(cookieSoreId))
                {
                    filterContext.Result = new RedirectResult("/PinAdmin/main/login");
                    return;
                }
                int storeid = 0;
                int.TryParse(DESEncryptTools.DESDecrypt(cookieSoreId), out storeid);
                //如果登陆cookie无法解密
                if (storeid <= 0)
                {
                    filterContext.Result = new RedirectResult("/PinAdmin/main/login");
                    return;
                }
                //只能进对应的管理后台
                if (storeid != storeId)
                {
                    filterContext.Result = new RedirectResult("/PinAdmin/main/login");
                    return;
                }
                
                PinStore pinStore = PinStoreBLL.SingleModel.GetModel(storeid);
                //如果门店不存在
                if (pinStore == null)
                {
                    filterContext.Result = new RedirectResult("/PinAdmin/main/login");
                    return;
                }

                filterContext.RouteData.Values["pinStore"] = pinStore;
            }
            else
            {
                filterContext.Result = new RedirectResult("/PinAdmin/main/login");
                return;
            }

        }
    }
}