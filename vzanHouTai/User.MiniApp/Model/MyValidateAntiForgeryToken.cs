using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Net;

namespace User.MiniApp.Model
{
    public class MyValidateAntiForgeryToken : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (request.HttpMethod == WebRequestMethods.Http.Post)
            {
                var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];
                var cookieValue = antiForgeryCookie != null
                 ? antiForgeryCookie.Value
                 : null;
                //从cookies 和 Headers 中 验证防伪标记
                //这里可以加try-catch
                try
                {
                    AntiForgery.Validate(cookieValue, request.Headers["__RequestVerificationToken"]);
                }
                catch (Exception )
                {
                    //log4net.LogHelper.WriteInfo(typeof(MyValidateAntiForgeryToken), ex.Message);
                }
            }
        }
    }
}