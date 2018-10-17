using Api.MiniApp.Models;
using BLL.MiniApp;
using Entity.MiniApp;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Filters
{
    public class AuthPXHFangCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string appid = filterContext.HttpContext.Request.QueryString["appid"];
            appid = string.IsNullOrEmpty(appid) ? filterContext.HttpContext.Request.Form["appid"] : appid;
            string openId = filterContext.HttpContext.Request.QueryString["openId"];
            openId = string.IsNullOrEmpty(openId) ? filterContext.HttpContext.Request.Form["openId"] : openId;
            string phone =filterContext.HttpContext.Request.QueryString["phone"];
            phone = string.IsNullOrEmpty(phone) ? filterContext.HttpContext.Request.Form["phone"] : phone;
            if (string.IsNullOrWhiteSpace(openId) || string.IsNullOrWhiteSpace(phone))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new BaseResult()
                    {
                        result = false,
                        msg = "参数：openId或者phone为空",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }

            C_UserInfo LoginUser = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openId);
            if (LoginUser == null)
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new BaseResult()
                    {
                        result = false,
                        msg = "登陆信息异常"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }
        }
    }
}