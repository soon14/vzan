using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Areas.Shop.Filters
{
    /// <summary>
    /// HTML静态文件访问路由
    /// </summary>
    public class HTMLRoute: ActionFilterAttribute
    {
        public string GetControllerRoute(ActionExecutingContext filterContext)
        {
            string controllerName = filterContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.RouteData.Values["action"].ToString();

            object areaName = string.Empty;
            if (!filterContext.RouteData.DataTokens.TryGetValue("area", out areaName))
            {
                return "/Areas/Shop/Views/Login.html";
            }
            return $"/Areas/{areaName}/Views/{controllerName}/{actionName}.html";
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Result = new FilePathResult(GetControllerRoute(filterContext), "text/html");
        }
    }
}