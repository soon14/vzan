using BLL.MiniApp;
using BLL.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Web.Mvc;
using Utility;

namespace User.MiniApp.Areas.Pin.Filters
{
    /// <summary>
    /// 拼享惠平台管理权限过滤器
    /// </summary>
    public class RouteAuthCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (WebSiteConfig.Environment != "dev")
            //{
            //    return;
            //}

            #region 登陆校验

            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            if (appId == 0)
                appId = Utility.IO.Context.GetRequestInt("aid", 0);

            string encryAccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
            Guid accountId = Guid.Empty;
            if (!Guid.TryParse(encryAccountId, out accountId))
            {
                filterContext.Result = new RedirectResult($"/SubAccount/Login?appId={appId}");
                return;
            }

            Account account = AccountBLL.SingleModel.GetModel(accountId);
            if(account == null)
            {
                filterContext.Result = new RedirectResult($"/SubAccount/Login?appId={appId}");
                return;
            }


            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, account.Id.ToString());
            if(xcx == null)
            {
                filterContext.Result = new RedirectResult($"/SubAccount/Login?appId={appId}");
                return;
            }
            #endregion

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(xcx.Id);
            string masterAuth = CookieHelper.GetCookie("masterAuth");
            string authToken = DESEncryptTools.DESDecrypt(masterAuth);
            if (authToken == account.Id.ToString())
            {
                //最高管理权限
                filterContext.ActionParameters["authInfo"] = AuthRoleBLL.SingleModel.GetMasterAuth(pageType: pageType, authName: account.LoginId, accessUrl: filterContext.HttpContext.Request.Url.ToString());
                return;
            }

            string adminAuth = CookieHelper.GetCookie("adminAuth");
            string adminAuthToken = DESEncryptTools.DESDecrypt(adminAuth);
            int roleId = 0;
            if (int.TryParse(adminAuthToken, out roleId) && roleId > 0)
            {
                AuthInfo authInfo = AuthRoleBLL.SingleModel.GetAuthMenuByRole(pageType: pageType, roleId: roleId, accessUrl: filterContext.HttpContext.Request.Url.ToString());
                authInfo.AuthName = account.LoginId;
                //子帐号权限
                filterContext.ActionParameters["authInfo"] = authInfo;
                return;
            }
            filterContext.Result = new RedirectResult($"/SubAccount/Login?appId={appId}");
            //base.OnActionExecuting(filterContext);
        }
    }
}