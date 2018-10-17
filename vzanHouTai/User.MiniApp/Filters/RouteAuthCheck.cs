using BLL.MiniApp;
using BLL.MiniApp.Pin;
using Entity.MiniApp;
using Entity.MiniApp.User;
using Entity.MiniApp.Pin;
using System;
using System.Web;
using System.Web.Mvc;
using Utility;
using BLL.MiniApp.User;
using System.Collections.Generic;
using System.Collections;
using Utility.IO;
using Core.MiniApp;
namespace User.MiniApp.Filters
{
    /// <summary>
    /// 小未程序子帐号登陆权限过滤器
    /// </summary>
    public class RouteAuthCheck : ActionFilterAttribute
    {

        /// <summary>
        /// 使用子帐号权限管理的模板
        /// </summary>
        private readonly IList<int> EnabelVersion = new List<int> { (int)TmpType.小程序专业模板, (int)TmpType.拼享惠, (int)TmpType.小未平台 };

        /// <summary>
        /// 主账号登陆信息
        /// </summary>
        /// <returns></returns>
        private static Account GetLoginAccount()
        {
            //string encryAccountId = CookieHelper.GetCookie("dz_UserCookieNew");
            //Guid accountId = Guid.Empty;
            //if (!Guid.TryParse(encryAccountId, out accountId))
            //{
            //    return null;
            //}
            //return AccountBLL.SingleModel.GetModel(accountId);
            return AccountBLL.SingleModel.GetModel(Utils.GetBuildCookieId("dz_UserCookieNew"));
        }

        /// <summary>
        /// 子帐号权限
        /// </summary>
        /// <returns></returns>
        public static AuthRole GetAdminAuth()
        {
            string adminAuth = CookieHelper.GetCookie("adminAuth");
            string adminAuthToken = DESEncryptTools.DESDecrypt(adminAuth);
            int roleId = 0;
            if (int.TryParse(adminAuthToken, out roleId) && roleId > 0)
            {
                return AuthRoleBLL.SingleModel.GetModel(roleId);
            }
            return null;
        }

        /// <summary>
        /// 最高管理权限
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static bool IsMasterAuth(Account account)
        {
            string masterAuth = CookieHelper.GetCookie("masterAuth");
            string authToken = DESEncryptTools.DESDecrypt(masterAuth);
            return authToken == account.Id.ToString();
        }

        public static bool IsPinAdmin()
        {
            int storeId = Context.GetRequestInt("storeId", 0);
            string cookieStoreId = CookieHelper.GetCookie("dzPinAdmin");
            if (storeId == 0 || string.IsNullOrWhiteSpace(cookieStoreId))
            {
                return false;
            }
            int decrypStoreId = 0;
            int.TryParse(DESEncryptTools.DESDecrypt(cookieStoreId), out decrypStoreId);
            return storeId > 0 && decrypStoreId == storeId;
        }

        //public static bool IsManageMenu(int pageType, string route)
        //{
        //    return new NavMenuBLL().IsManageMenu(pageType: pageType, route: route);
        //}

        /// <summary>
        /// 获取当前路由，格式：/{controller}/{action} 或 /{area}/{controller}/{action}
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public static string GetControllerRoute(ActionExecutingContext filterContext)
        {
            string controllerName = filterContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.RouteData.Values["action"].ToString();

            object areaName = string.Empty;
            if (filterContext.RouteData.DataTokens.TryGetValue("area", out areaName))
            {
                return $"/{areaName}/{controllerName}/{actionName}";
            }
            return $"/{controllerName}/{actionName}";
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (WebSiteConfig.Environment != "dev")
            //{
            //    return;
            //}

            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                return;
            }

            //忽略POST请求，和ajax请求
            if(filterContext.HttpContext.Request.HttpMethod != "GET" || filterContext.HttpContext.Request.IsAjaxRequest())
            {
                return;
            }

            if (IsPinAdmin())
            {
                return;
            }

            #region 登陆校验
            Account account = GetLoginAccount();
            if (account == null)
            {
                filterContext.Result = new RedirectResult("/dzhome/login");
                filterContext.Result = new EmptyResult();
                return;
            }

            int appId = Context.GetRequestInt("appId", 0);
                appId = appId == 0 ? Context.GetRequestInt("aid", 0) : appId;

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, account.Id.ToString());
            if(xcx == null)
            {
                filterContext.Result = new RedirectResult("/dzhome/casetemplate");
                filterContext.Result = new EmptyResult();
                return;
            }
            #endregion

            int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(xcx.Id);
            if (!EnabelVersion.Contains(pageType))
            {
                //当前小程序版本没有接入子帐号管理
                return;
            }

            string route = GetControllerRoute(filterContext);

            AuthInfo authInfo = null;
            bool isMaster = IsMasterAuth(account);
            if (isMaster)
            {
                //最高管理权限
                authInfo = AuthRoleBLL.SingleModel.GetAppMasterAuth(pageType: pageType, authName: account.LoginId, accessUrl:route);
            }
            else
            {
                //子帐号权限
                AuthRole role = GetAdminAuth();
                authInfo = AuthRoleBLL.SingleModel.GetAppMenuByRole(role: role, pageType: pageType, accessUrl: route);
                authInfo.AuthName = account.LoginId;
            }
            if (authInfo != null)
            {
                filterContext.ActionParameters["authInfo"] = authInfo;
                filterContext.Controller.ViewBag.authInfo = authInfo;
                filterContext.Controller.ViewBag.versionId = xcx.VersionId;
            }

            //判断当前小程序访问权限
            if (authInfo != null && authInfo.AuthAdmin != null && authInfo.AuthAdmin.AId != xcx.Id)
            {
                string url = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(authInfo.AuthAdmin.AId) == (int)TmpType.拼享惠 ?
                            $"/pin/main?Id={authInfo.AuthAdmin.AId}&appId={authInfo.AuthAdmin.AId}" :
                            $"/SubAccount/Welcome?appId={authInfo.AuthAdmin.AId}&pageType={pageType}";
                filterContext.Result = new RedirectResult(url);
                filterContext.Result = new EmptyResult();
                return;
            }

            //判断当前路由访问权限
            bool? hasAccess = authInfo?.CheckRouteAccess();
            if(hasAccess.HasValue && !hasAccess.Value)
            {
                //拒绝访问，跳回欢迎页（无权限）
                filterContext.Result = new RedirectResult($"/SubAccount/Welcome?appId={xcx.Id}&pageType={pageType}");
                filterContext.Result = new EmptyResult();
                return;
            }
            if(hasAccess.HasValue && hasAccess.Value)
            {
                //允许访问（有权限）
                return;
            }

            //无权限凭证
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult { Data = new Return_Msg { code = "403", isok = false, Msg = "登陆授权过期" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet, };
                filterContext.Result = new EmptyResult();
            }
            else
            {
                filterContext.Result = new RedirectResult($"/SubAccount/Login?appId={xcx.Id}");
                filterContext.Result = new EmptyResult();
            }
        }
    }
}