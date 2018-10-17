using BLL.MiniApp;
using BLL.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace User.MiniApp.Areas.Pin.Controllers
{
    public class LoginController : BaseController
    {
        [HttpGet, AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 拼享会平台账号登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="isKeep"></param>
        /// <param name="backurl"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public ActionResult Index(int? appId = null, string username = null, string password = null, bool isKeep = false, string backurl = null)
        {
            if(!appId.HasValue)
            {
                result.msg = "参数不能为空_appId";
                return Json(result);
            }

            //清除缓存
            CookieHelper.Remove("dz_UserCookieNew");
            Response.ContentEncoding = Encoding.UTF8;
            username = StringHelper.NoHtml(username.Trim());
            password = StringHelper.NoHtml(password);

            AuthRole admin = AuthRoleBLL.SingleModel.UserLogin(appId.Value, username, password);
            if (admin == null)
            {
                result.msg = "用户名或密码错误";
                return Json(result);
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(admin.AId);
            if (app == null)
            {
                result.msg = "小程序不存在";
                return Json(result);
            }
            Account account = AccountBLL.SingleModel.GetModel(app.AccountId);
            if (account == null)
            {
                result.msg = "授权账号不存在";
                return Json(result);
            }

            Session["userName"] = username;
            Session["passWord"] = password;
            Session["dzAccountId"] = account.Id.ToString();
            if (isKeep)//--保存本地用户名
            {
                CookieHelper.SetCookie("LoginUserName", HttpUtility.UrlEncode(username));
            }
            else
            {
                CookieHelper.Remove("LoginUserName");
            }

            Task.Factory.StartNew(() =>
            {
                AuthRoleBLL.SingleModel.UpdateLoginTime(admin);
            });

            result.code = 1;
            result.msg = "登陆成功";
            result.obj = new { loginToken = account.Id, authToken = DESEncryptTools.DESEncrypt(admin.Id.ToString()),  url = $"/pin/main?Id={app.Id}&appId={app.Id}" };
            return Json(result);
        }
    }
}