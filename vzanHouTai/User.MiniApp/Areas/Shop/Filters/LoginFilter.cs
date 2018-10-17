using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using BLL.MiniApp;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Plat;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using Utility;
using Entity.MiniApp.Footbath;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Fds;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Dish;
using Utility.IO;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;
using User.MiniApp.Areas.Shop.Models;
using Entity.MiniApp.Dish;

namespace User.MiniApp.Areas.Shop.Filters
{
    public class LoginFilterAttribute : AuthorizeAttribute
    {
        private string _StorePara { get; set; }

        private string _AccountPara { get; set; }

        private string _XcxPara { get; set; }

        private bool _GetAuthStore { get; set; }

        private Func<bool> _OnAuthorizationed { get; set; }

        public LoginFilterAttribute(string accountPara = null, string storePara = null, string xcxPara = null, bool getAuthStore = false )
        {
            _StorePara = storePara;
            _AccountPara = accountPara;
            _XcxPara = xcxPara;
            _GetAuthStore = getAuthStore;
        }

        private string GetControllerRoute(AuthorizationContext filterContext)
        {
            string controllerName = filterContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.RouteData.Values["action"].ToString();

            object areaName = string.Empty;
            if (!filterContext.RouteData.DataTokens.TryGetValue("area", out areaName))
            {
                return "/Areas/Shop/Views/Login.html";
            }
            return $"/Areas/{areaName}/Views/{controllerName}/{actionName}.html";
        }

        private DishStore GetStoreByAuth(AuthorizationContext filterContext)
        {
            int manageStoreId = 0;
            string loginToken = CookieHelper.GetCookie("dzDishAdmin");
            if(!int.TryParse(DESEncryptTools.DESDecrypt(loginToken), out manageStoreId) || manageStoreId <= 0)
            {
                return null;
            }
            return DishStoreBLL.SingleModel.GetModel(manageStoreId);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;

            //小程序管理员
            Guid accountId = Utils.GetBuildCookieId("dz_UserCookieNew");
            bool masterAuth = accountId != Guid.Empty && accountId != null;

            if (masterAuth && !string.IsNullOrWhiteSpace(_AccountPara))
            {
                Account account = AccountBLL.SingleModel.GetModel(accountId);
                filterContext.RouteData.Values[_AccountPara] = account;
            }

            //店铺管理员
            int storeAccessId = 0;
            object storePara;
            bool storeAccess = filterContext.RouteData.Values.TryGetValue("storeId", out storePara) && int.TryParse(storePara?.ToString(), out storeAccessId) && storeAccessId > 0;
            bool storeAdminAuth = false;

            DishStore manageStore = null;
            //访问管理店铺接口（检验该店铺管理权限）
            if (storeAccess)
            {
                manageStore = GetStoreByAuth(filterContext);
                storeAdminAuth = storeAccessId == manageStore?.id;
            }
            //控制器手动获取管理店铺
            if (_GetAuthStore)
            {
                manageStore = GetStoreByAuth(filterContext);
            }
            //保存店铺实体到路由字典
            if ((storeAdminAuth || _GetAuthStore) && !string.IsNullOrWhiteSpace(_StorePara))
            {
                filterContext.RouteData.Values[_StorePara] = manageStore;
            }
            //小程序管理可直接操作所属店铺
            if (masterAuth && storeAccess)
            {
                DishStore store = DishStoreBLL.SingleModel.GetModel(storeAccessId);
                XcxAppAccountRelation app = store?.id > 0 ? XcxAppAccountRelationBLL.SingleModel.GetModel(store.aid) : null;
                if (app == null || app.AccountId != app.AccountId)
                {
                    //无小程序管理员权限 或 操作他人账号的小程序
                    filterContext.Result = Common.SingleModel.ApiModel(message: "非法操作", code: "503");
                    return;
                }
                if(!string.IsNullOrWhiteSpace(_StorePara))
                {
                    filterContext.RouteData.Values[_StorePara] = store;
                }
                if (!string.IsNullOrWhiteSpace(_XcxPara))
                {
                    filterContext.RouteData.Values[_XcxPara] = app;
                }
            }

            //bool isAjax = filterContext.HttpContext.Request.IsAjaxRequest();
            if (!masterAuth && !storeAdminAuth)
            {
                filterContext.Result = Common.SingleModel.ApiModel(message: "登陆过期");
            }

            if(filterContext.ActionDescriptor.IsDefined(typeof(MasterAuthOnly), true) && !masterAuth)
            {
                filterContext.Result = Common.SingleModel.ApiModel(message: "权限不足");
            }
        }
    }

    /// <summary>
    /// 仅小程序管理员可访问
    /// </summary>
    public sealed class MasterAuthOnly : Attribute
    {

    }
}