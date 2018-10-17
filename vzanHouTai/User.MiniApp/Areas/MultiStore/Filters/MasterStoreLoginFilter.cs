//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using BLL.MiniApp.Ent;
//using Entity.MiniApp.Ent;
//using BLL.MiniApp;
//using Core.MiniApp;
//using Entity.MiniApp;
//using Entity.MiniApp.User;
//using Newtonsoft.Json;
//using Utility;
//using Entity.MiniApp.Footbath;
//using BLL.MiniApp.Footbath;

//namespace User.MiniApp.Areas.MultiStore.Filters
//{
//    /// <summary>
//    /// 多门店总店管理权限验证
//    /// </summary>
//    public class MasterStoreLoginFilterAttribute : AuthorizeAttribute
//    {

//        public override void OnAuthorization(AuthorizationContext filterContext)
//        {
//            string AccountId = CookieHelper.GetCookie("dz_UserCookieNew");
//            Guid _accountid = Guid.Empty;
//            Guid.TryParse(AccountId, out _accountid);
//            if (_accountid == Guid.Empty)
//            {
//                filterContext.Result = new ContentResult() { Content = JsonConvert.SerializeObject(new { isok = false, msg = "授权验证：未登录，非法操作" }) };
//                return;
//            }
//            Account accountModel = accountBLL.GetModel(_accountid);
//            if (accountModel == null)
//            {
//                filterContext.Result = new ContentResult() { Content = JsonConvert.SerializeObject(new { isok = false, msg = "授权验证：用户不存在！" }) };
//                return;
//            }
//            int appId = Utility.IO.Context.GetRequestInt("appId", 0);
//            int id = Utility.IO.Context.GetRequestInt("Id", 0);
//            if (appId <= 0)
//            {
//                appId = id;
//            }
//            XcxAppAccountRelation role = relationBLL.GetModelByaccountidAndAppid(appId, accountModel.Id.ToString());
//            if (role == null)
//            {
//                filterContext.Result = new ContentResult() { Content = JsonConvert.SerializeObject(new { isok = false, msg = "授权验证：未开通本服务！" }) };
//                return;
//            }
//        }
//    }
//}