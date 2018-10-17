using Api.MiniApp.Models;
using BLL.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 商家版小程序身份验证
    /// </summary>
    public class MasterCheck : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string useridStr = filterContext.HttpContext.Request.Form["userid"] ?? filterContext.HttpContext.Request.QueryString["userid"];
            int userid = 0;
            Int32.TryParse(useridStr, out userid);
            if (userid <= 0)
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new
                    {
                        isok = false,
                        Msg = "参数：userid 错误",
                        errcode = 500,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }
            string appid = filterContext.HttpContext.Request.Form["storeAppid"] ?? filterContext.HttpContext.Request.QueryString["storeAppid"];
            if (string.IsNullOrEmpty(appid))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new
                    {
                        isok = false,
                        Msg = "参数：storeAppid 错误",
                        errcode = 500,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userid);
            if (userInfo == null)
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new
                    {
                        isok = false,
                        Msg = "用户不存在",
                        errcode = 500,
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }

            //Account account = AccountBLL.SingleModel.GetModelByPhone(userInfo.TelePhone);
            //if (account == null)
            //{
            //    filterContext.Result = new JsonResult()
            //    {
            //        Data = new
            //        {
            //            isok = false,
            //            Msg = "用户信息错误",
            //            errcode = 500,
            //        },
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //    };
            //    base.OnActionExecuting(filterContext);
            //    return;
            //}
        }
    }
}