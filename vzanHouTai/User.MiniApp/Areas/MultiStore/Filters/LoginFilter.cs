using BLL.MiniApp;
using BLL.MiniApp.Footbath;
using Entity.MiniApp;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace User.MiniApp.Areas.MultiStore.Filters
{
    /// <summary>
    /// 多门店权限验证
    /// </summary>
    public class LoginFilterAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 必须是模板拥有者才能通过验证
        /// (默认false,代表分店管理者也可通过此拦截器的验证)
        /// </summary>
        public bool certainlyAppMasterRole = false;
      
        
        

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            #region 验证用户是否登录
            string AccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();

            Guid _accountid = Guid.Empty;
            Guid.TryParse(AccountId, out _accountid);
            if (_accountid == Guid.Empty)
            {
                filterContext.Result = returnMsgByRquestType(filterContext, "授权验证：未登录，非法操作");
                return;
            }

            Account accountModel = AccountBLL.SingleModel.GetModel(_accountid);
            if (accountModel == null)
            {
                filterContext.Result = returnMsgByRquestType(filterContext, "授权验证：用户不存在！");
                return;
            }
            #endregion

            #region 验证用户权限
            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
            int storeId = Utility.IO.Context.GetRequestInt("storeId", 0);
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (xcx == null)
            {
                filterContext.Result = returnMsgByRquestType(filterContext, "没有开通此模板！");
            }

            //验证店铺是否为该小程序所有
            FootBath store = null;
            if (storeId == 0)
            {
                store = FootBathBLL.SingleModel.GetModel($" appId = {appId} and HomeId = 0 ");
            }
            else
            {
                store = FootBathBLL.SingleModel.GetModel(storeId);
            }
            if (store == null || store.appId != xcx.Id) 
            {
                filterContext.Result = returnMsgByRquestType(filterContext, $"授权验证：您不是该店铺的管理员");
                return;
            }

            //HomeId 不为 0 验证分店权限
            if (store.HomeId > 0)
            {
                List<UserRole> havingRoles = UserRoleBLL.SingleModel.GetCurrentUserRoles(accountModel.Id, appId, storeId);
                if (havingRoles == null || !havingRoles.Any())
                {
                    filterContext.Result = new RedirectResult("/base/PageError?type=4");
                    return;
                }
            }
            else //HomeId 为 0 验证总店权限
            {
                bool havingRoles = UserRoleBLL.SingleModel.HavingRole(accountModel.Id, RoleType.总店管理员, appId, 0);
                if (!havingRoles)
                {
                    filterContext.Result = new RedirectResult("/base/PageError?type=4");
                    return;
                }
            }
            #endregion
        }

        //根据不同类型请求,返回不同格式的响应
        public ActionResult returnMsgByRquestType(AuthorizationContext filterContext, string msg)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                return new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = msg }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

                //return new System.Web.Mvc.JsonResult() { Data = new { isok = -1, msg = msg }, ContentEncoding = System.Text.Encoding.UTF8, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "json" };
            }
            else
            {
                return new ContentResult() { Content = msg };
            }
        }
    }
}