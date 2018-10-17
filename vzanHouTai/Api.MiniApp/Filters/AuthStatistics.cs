using Api.MiniApp.Models;
using BLL.MiniApp;
using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Web.Mvc;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 小未平台小程序访问量统计
    /// </summary>
    public class AuthStatistics : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string aid= filterContext.HttpContext.Request.Form["aid"] ?? filterContext.HttpContext.Request.QueryString["aid"];
            string userid = filterContext.HttpContext.Request.Form["userid"] ?? filterContext.HttpContext.Request.QueryString["userid"];
            string appid = filterContext.HttpContext.Request.Form["appid"] ?? filterContext.HttpContext.Request.QueryString["appid"];

            if(!string.IsNullOrEmpty(aid) && !string.IsNullOrEmpty(appid))
            {
                int tempaid = 0;
                int tempuserid = 0;
                if (int.TryParse(aid, out tempaid)&& int.TryParse(userid, out tempuserid))
                {
                    PlatStatistics model = new PlatStatistics();
                    model.AddTime = DateTime.Now;
                    model.UserId = tempuserid;
                    model.AId = tempaid;
                    model.AppId = appid;
                    PlatStatisticsBLL.SingleModel.Add(model);
                }
            }
        }
    }
}