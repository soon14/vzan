using BLL.MiniApp;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace User.MiniApp.Filters
{
    public class LoginFilterAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 接收授权数据参数名
        /// </summary>
        private string authDataPara { get; set; }
        /// <summary>
        /// 授权登陆过滤器
        /// </summary>
        /// <param name="parseAuthDataTo">接收授权数据参数名</param>
        public LoginFilterAttribute(string parseAuthDataTo = null)
        {
            authDataPara = parseAuthDataTo;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;

            int appId = Utility.IO.Context.GetRequestInt("appId", 0);

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            //没有开通此模板
            if (xcx == null || xcx.Id<=0)
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=3");
                return;
            }

            if (!string.IsNullOrWhiteSpace(authDataPara))
            {
                filterContext.RouteData.Values[authDataPara] = xcx;
            }

            XcxTemplate templatemodel = XcxTemplateBLL.SingleModel.GetModel(xcx.TId);
            if (templatemodel.Type == (int)TmpType.拼享惠 && Utility.IO.Context.GetRequestInt("storeId", 0) > 0)
            {
                return;
            }

            Guid _accountid = Utils.GetBuildCookieId("dz_UserCookieNew");
            //没有登录
            if (_accountid == Guid.Empty)
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=1");
                return;
            }
            Account accountModel = AccountBLL.SingleModel.GetModel(_accountid);
            //登录用户不存在
            if (accountModel == null)
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=2");
                return;
            }

            #region 验证用户权限

            if (templatemodel != null)
            {

                if (templatemodel.Type == (int)TmpType.小程序多门店模板)
                {
                    int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);
                    //验证店铺是否为该小程序所有
                    FootBath store = null;
                    if (storeId == 0)
                    {
                        store = FootBathBLL.SingleModel.GetModel($" appId = {appId} and HomeId = 0  ");
                    }
                    else
                    {
                        store = FootBathBLL.SingleModel.GetModel(storeId);
                    }
                    ////验证店铺是否为该小程序所有
                    if (store == null || store.appId != xcx.Id)
                    {
                        filterContext.Result = new RedirectResult("/base/PageError?type=4");
                        return;
                    }

                    //若为分店 验证分店权限
                    if (store.HomeId > 0)
                    {
                        List<UserRole> havingRoles = UserRoleBLL.SingleModel.GetCurrentUserRoles(accountModel.Id, appId, storeId);
                        if (havingRoles == null || !havingRoles.Any())
                        {
                            filterContext.Result = new RedirectResult("/base/PageError?type=4");
                            return;
                        }
                    }
                    else //当前选择为总店 验证总店权限
                    {
                        bool havingRoles = UserRoleBLL.SingleModel.HavingRole(accountModel.Id, RoleType.总店管理员, appId);
                        if (!havingRoles)
                        {
                            filterContext.Result = new RedirectResult("/base/PageError?type=4");
                            return;
                        }
                    }
                }
                else if (templatemodel.Type == (int)TmpType.小程序餐饮多门店模板)
                {
                    int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);
                    //验证店铺是否为该小程序所有
                    Food store = null;
                    if (storeId == 0)
                    {
                        store = FoodBLL.SingleModel.GetModel($" appId = {appId} and masterStoreId = 0  ");
                    }
                    else
                    {
                        store = FoodBLL.SingleModel.GetModel(storeId);
                    }
                    ////验证店铺是否为该小程序所有
                    if (store == null || store.appId != xcx.Id)
                    {
                        filterContext.Result = new RedirectResult("/base/PageError?type=4");
                        return;
                    }

                    //若为分店 验证分店权限
                    if (store.masterStoreId > 0)
                    {
                        List<UserRole> havingRoles = UserRoleBLL.SingleModel.GetCurrentUserRoles(accountModel.Id, appId, storeId);
                        if (havingRoles == null || !havingRoles.Any())
                        {
                            filterContext.Result = new RedirectResult("/base/PageError?type=4");
                            return;
                        }
                    }
                    else //当前选择为总店 验证总店权限
                    {
                        bool havingRoles = UserRoleBLL.SingleModel.HavingRole(accountModel.Id, RoleType.总店管理员, appId);
                        if (!havingRoles)
                        {
                            filterContext.Result = new RedirectResult("/base/PageError?type=4");
                            return;
                        }
                    }
                }
                else
                {
                    int id = Utility.IO.Context.GetRequestInt("Id", 0);
                    if (appId <= 0)
                    {
                        appId = id;
                    }

                    XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, accountModel.Id.ToString());
                    if (role == null)
                    {
                        filterContext.Result = new RedirectResult("/base/PageError?type=5");
                        return;
                    }
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=6");
                return;
            }


            #endregion
            //base.OnAuthorization(filterContext);
        }
    }
}