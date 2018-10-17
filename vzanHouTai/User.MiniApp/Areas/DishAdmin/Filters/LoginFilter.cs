using BLL.MiniApp.Dish;
using Entity.MiniApp.Dish;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace User.MiniApp.Areas.DishAdmin.Filters
{
    public class LoginFilterAttribute : AuthorizeAttribute
    {
        
        
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;

            int storeId = Context.GetRequestInt("storeId", 0);

            if (storeId > 0)
            {
                string cookieSoreId = CookieHelper.GetCookie("dzDishAdmin");
                //如果没有找到登陆cookie
                if (string.IsNullOrEmpty(cookieSoreId))
                {
                    filterContext.Result = new RedirectResult("/DishAdmin/main/login");
                    return;
                }
                int storeid = 0;
                int.TryParse(DESEncryptTools.DESDecrypt(cookieSoreId), out storeid);
                //如果登陆cookie无法解密
                if (storeid <= 0)
                {
                    filterContext.Result = new RedirectResult("/DishAdmin/main/login");
                    return;
                }
                //只能进对应的管理后台
                if (storeid != storeId)
                {
                    filterContext.Result = new RedirectResult("/DishAdmin/main/login");
                    return;
                }
                DishStore dishStore = DishStoreBLL.SingleModel.GetModel(storeid);
                //如果门店不存在
                if (dishStore == null)
                {
                    filterContext.Result = new RedirectResult("/DishAdmin/main/login");
                    return;
                }
                
                filterContext.RouteData.Values["dishStore"] = dishStore;
            }
            else
            {
                filterContext.Result = new RedirectResult("/DishAdmin/main/login");
                return;
            }

        }
    }
}