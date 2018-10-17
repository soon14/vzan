using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
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

namespace User.MiniApp.Areas.Qiye.Filters
{
    public class LoginFilterAttribute : AuthorizeAttribute
    {
        

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;
            int aid = Context.GetRequestInt("appId", 0);
            if(aid<=0)
            {
                aid = Context.GetRequestInt("aid", 0);
            }
            string AccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
            Guid _accountid = Guid.Empty;
            Guid.TryParse(AccountId, out _accountid);
            if (aid == 0 || _accountid == Guid.Empty)
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=1");
                return;
            }
            
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(aid, _accountid.ToString());
            if(xcxrelation==null)
            {
                filterContext.Result = new RedirectResult("/base/PageError?type=2");
                return;
            }
           
        }
    }
}