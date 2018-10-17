using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Models;

namespace User.MiniApp.Areas.Shop.Filters
{
    /// <summary>
    /// 校验POST的Form
    /// </summary>
    public class FormValidate : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
            if (!modelState.IsValid)
            {
                string errorMessage = modelState.Values
                       .SelectMany(m => m.Errors)
                       .Select(m => m.ErrorMessage)
                       .First();
                //直接响应验证结果
                filterContext.Result = Common.SingleModel.ApiModel(message: errorMessage);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}