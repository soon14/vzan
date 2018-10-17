using Api.MiniApp.Models;
using BLL.MiniApp;
using System.Web.Mvc;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 登陆状态检查，并保持到ViewData["LoginData"]
    /// </summary>
    public class AuthLoginCheckXiaoChenXun : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            string openId = filterContext.HttpContext.Request.Form["openId"] ?? filterContext.HttpContext.Request.QueryString["openId"];
            if (string.IsNullOrWhiteSpace(openId))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new BaseResult()
                    {
                        result = false,
                        msg = "参数：openId，为空",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }

            var LoginUser = C_UserInfoBLL.SingleModel.GetModelFromCache(openId);
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
            //LoginData = LoginUser;
            filterContext.Controller.ViewData.Add("LoginData", LoginUser);
        }
    }
}