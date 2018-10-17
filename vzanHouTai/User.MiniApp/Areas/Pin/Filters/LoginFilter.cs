using BLL.MiniApp;
using BLL.MiniApp.Pin;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using Entity.MiniApp.User;
using System;
using System.Web.Mvc;
using Utility;

namespace User.MiniApp.Areas.Pin.Filters
{
    public class LoginFilterAttribute: AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;

            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId == 0)
                appId = Utility.IO.Context.GetRequestInt("aid", 0);

            string AccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
            Guid _accountid = Guid.Empty;
            Guid.TryParse(AccountId, out _accountid);
            if (appId==0|| _accountid == Guid.Empty)
            {
                filterContext.Result = new RedirectResult("/dzhome/login");
                return;
            }
            //用户是否存在
            Account accountModel = AccountBLL.SingleModel.GetModel(_accountid);
            if (accountModel == null)
            {
                filterContext.Result = new RedirectResult("/dzhome/login");
                return;
            }
            
            #region 验证用户权限
            


            //用户是否开通了这个模板
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, accountModel.Id.ToString());
            if (xcx == null)
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=5");
                return;
            }
            filterContext.RouteData.Values["xcx"] = xcx;

            var templatemodel = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (templatemodel == null)
            {
                return;
            }

            //拼享惠的授权登陆验证
            if (templatemodel.Type == (int)TmpType.拼享惠)
            {
                int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);
                //如果这个用户是门店管理员 
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

                    
                    PinStore pinStore = PinStoreBLL.SingleModel.GetModel(storeid);
                    //如果门店不存在
                    if (pinStore == null)
                    {
                        filterContext.Result = new RedirectResult("/PinAdmin/main/login");
                        return;
                    }
                    //filterContext.RouteData.Values["dishStore"] = dishStore;
                    return;
                }
            }

            #endregion
        }
    }
}