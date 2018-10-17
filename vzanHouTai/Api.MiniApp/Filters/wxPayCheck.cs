using Api.MiniApp.Models;
using BLL.MiniApp;
using System.Web.Mvc;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 微信支付（检查）
    /// </summary>
    public class wxPayCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var Request = filterContext.RequestContext.HttpContext.Request;
            //当前域名
            string c_host = Request.Url.Host;
            //必须使用微赞，并且当前域名不是VZAN
            if (!string.IsNullOrEmpty(c_host) && !c_host.Contains(WebConfigBLL.VZAN_DOMAIN))
            {
                filterContext.Result = new RedirectResult(Request.Url.ToString().Replace(c_host, WebConfigBLL.VZAN_DOMAIN));
                filterContext.RequestContext.HttpContext.Response.End();
                return;
            }
            //浏览器判断
            bool? reuslt = Request.UserAgent?.ToLower().Contains("micromessenger");
            if (!reuslt.HasValue || !reuslt.Value)
            {
                filterContext.Result = new JsonResult() { Data = new { result = false, message = "请在微信端打开！" } };
                return;
            }
        }
    }
}