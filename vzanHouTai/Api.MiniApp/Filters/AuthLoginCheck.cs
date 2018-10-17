using Api.MiniApp.Models;
using BLL.MiniApp;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Filters
{

    /// <summary>
    /// 登陆状态检查，并保持到ViewData["LoginData"]
    /// </summary>
    public class AuthLoginCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            string unionid = filterContext.HttpContext.Request.Form["unionid"] ?? filterContext.HttpContext.Request.QueryString["unionid"];
            if (string.IsNullOrWhiteSpace(unionid))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new BaseResult()
                    {
                        result = false,
                        msg = "参数：unionid，为空",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }

            var LoginUser = C_UserInfoBLL.SingleModel.GetModelFromCacheByUnionid(unionid);
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