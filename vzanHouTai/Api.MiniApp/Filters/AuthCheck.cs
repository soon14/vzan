using Api.MiniApp.Models;
using BLL.MiniApp;
using DAL.Base;
using System.Web.Mvc;
using Utility.IO;
using Entity.MiniApp.Dish;
using BLL.MiniApp.Dish;
using Entity.MiniApp;
using System;
using Entity.MiniApp.Weixin;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 登陆验证
    /// </summary>
    public class AuthCheck : ActionFilterAttribute
    {
        public bool IsCheck { get; set; } = true;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IsCheck)
                return;
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                return;
            string utoken = Context.GetRequest("utoken", "");
            //HACK 用于调试
            if (utoken == "63168744-3fbe-42ba-8b5c-c59f34ff134d")
                return;
            Guid utoken_guid = Guid.Empty;
            if (!Guid.TryParse(utoken, out utoken_guid))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new DishApiReturnMsg
                    {
                        code = 0,
                        info = "非法请求！"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }
            int aid = Context.GetRequestInt("aid", 0);
            UserSession usersession = RedisUtil.Get<UserSession>(string.Format(CheckLoginClass._redis_loginSessionOpenIdKey, utoken));
            if (usersession == null)
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new DishApiReturnMsg
                    {
                        code = 2,
                        info = "请先登录！"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                return;
            }
            else
            {
                C_UserInfo userInfo = DishPublicBLL.SingleModel.GetUserInfo(utoken);
                if (userInfo == null)
                {
                    filterContext.Result = new JsonResult()
                    {
                        Data = new DishApiReturnMsg
                        {
                            code = 0,
                            info = "用户不存在！"
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    return;
                }
                filterContext.RouteData.Values.Add("userInfo", userInfo);
            }

        }
    }
}