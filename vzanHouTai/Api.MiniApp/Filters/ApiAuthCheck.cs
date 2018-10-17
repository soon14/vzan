using Api.MiniApp.Models;
using BLL.MiniApp.Dish;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Weixin;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Utility.IO;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 登陆验证
    /// </summary>
    public class ApiAuthCheck : ActionFilterAttribute
    {
        /// <summary>
        /// 验证接口请求是否合法
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (filterContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return;
            string utoken = Context.GetRequest("utoken", "");

            Guid utoken_guid = Guid.Empty;
            if (!Guid.TryParse(utoken, out utoken_guid))
            {
                filterContext.Response= filterContext.Request.CreateResponse(HttpStatusCode.OK, new ReturnMsg{
                    code=0,
                    msg= "非法请求!"
                });
                return;
            }
            int aid = Context.GetRequestInt("aid", 0);
            UserSession usersession = RedisUtil.Get<UserSession>(string.Format(CheckLoginClass._redis_loginSessionOpenIdKey, utoken));
            if (usersession == null)
            {
                filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.OK, new ReturnMsg
                {
                    code = 2,
                    msg = "请先登录!"
                });
                return;
            }
            else
            {
                C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
                if (userInfo == null)
                {
                    filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.OK, new ReturnMsg
                    {
                        code = 0,
                        msg = "用户不存在!"
                    });
                    return;
                }
                filterContext.RequestContext.RouteData.Values["userInfo"] = userInfo;
            }

        }
    }
}