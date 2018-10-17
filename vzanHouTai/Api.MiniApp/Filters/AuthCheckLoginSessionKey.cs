using Api.MiniApp.Models;
using BLL.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Weixin;
using System.Web.Mvc;

namespace Api.MiniApp.Filters
{
    /// <summary>
    /// 登陆秘钥检查
    /// </summary>
    public class AuthCheckLoginSessionKey : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string sessionkey = filterContext.HttpContext.Request.Form["sessionkey"] ?? filterContext.HttpContext.Request.QueryString["sessionkey"];
            string appid = filterContext.HttpContext.Request.Form["appid"] ??filterContext.HttpContext.Request.QueryString["appid"];
            
            //兼容旧接口
            if(string.IsNullOrEmpty(sessionkey))
            {
                if (string.IsNullOrWhiteSpace(appid))
                {
                    filterContext.Result = new JsonResult()
                    {
                        Data = new
                        {
                            isok = false,
                            msg = "参数：appid，为空",
                            errcode = 500,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    base.OnActionExecuting(filterContext);
                    return;
                }
                string redis_loginSessionKey = CheckLoginClass.GetLoginSessionKey(appid);
                if (redis_loginSessionKey == null || redis_loginSessionKey.Length == 0)
                {
                    filterContext.Result = new JsonResult()
                    {
                        Data = new
                        {
                            isok = false,
                            msg = "登陆秘钥过期",
                            errcode = 200,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    base.OnActionExecuting(filterContext);
                    return;
                }
            }
            else
            {
                UserSession usersession = RedisUtil.Get<UserSession>(string.Format(CheckLoginClass._redis_loginSessionOpenIdKey, sessionkey));
                if (usersession == null)
                {
                    filterContext.Result = new JsonResult()
                    {
                        Data = new
                        {
                            isok = false,
                            msg = "登陆秘钥过期",
                            errcode = 200,
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                    base.OnActionExecuting(filterContext);
                    return;
                }
            }
            
        }
    }
}