using BLL.MiniApp;
using BLL.MiniApp.User;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Filters;
using Utility;

namespace User.MiniApp.Controllers
{
    [RouteAuthCheck]
    public class SubAccountController : Controller
    {
        private ReturnMsg result = new ReturnMsg();

        
        

        [HttpGet]
        public ActionResult Index(AuthInfo authInfo = null, int? appId = null, int groupPageindex = 1, int rolePageIndex = 1)
        {
            List<AuthGroup> authGroup = AuthGroupBLL.SingleModel.GetListByAId(aId: appId.Value, pageIndex: groupPageindex, pageSize: 10);
            List<AuthRole> authRole = AuthRoleBLL.SingleModel.GetListByAId(aId: appId.Value, pageIndex: rolePageIndex, pageSize: 10);
            List<NavMenu> navmenu = authInfo.AllMenu;


            authGroup.ForEach(group =>
            {
                List<AuthMenu> authMenu = group.GetAuthMenu();
                group.NavMenu = string.Join("；", navmenu.FindAll(menu => authMenu.Exists(auth => auth.ItemId == menu.Id)).Select(menu => menu.Name));
            });

            authRole.ForEach(role =>
            {
                role.GroupName = AuthGroupBLL.SingleModel.GetModel(role.GroupId)?.Name;
                role.CreateUserName = role.CreateUserId > 0 ? AuthRoleBLL.SingleModel.GetModel(role.CreateUserId)?.Name : authInfo.AuthName;
            });

            int groupCount = AuthGroupBLL.SingleModel.GetCountByAId(aId: appId.Value);
            int roleCount = AuthRoleBLL.SingleModel.GetCountByAId(aId: appId.Value);

            return View(Tuple.Create(authGroup, authRole, groupCount, roleCount));
        }

        [HttpGet]
        public ActionResult Group(AuthInfo authInfo = null, int? aId = null, int? groupId = null)
        {
            AuthGroup group = null;
            if (groupId.HasValue)
            {
                group = AuthGroupBLL.SingleModel.GetByAId(aId.Value, groupId: groupId.Value);
            }
            List<NavMenu> menu = authInfo.AllMenu;
            return View(Tuple.Create(group, menu));
        }

        [HttpPost]
        public JsonResult UpdateGroup(int? aId = null, AuthGroup updateGroup = null)
        {
            if (updateGroup == null)
            {
                result.msg = "提交数据为空";
                return Json(result);
            }
            if (updateGroup.Id > 0 && updateGroup.Aid != aId.Value)
            {
                result.msg = "非法操作";
                return Json(result);
            }
            if (string.IsNullOrWhiteSpace(updateGroup.Name))
            {
                result.msg = "名称不能为空";
                return Json(result);
            }
            if (AuthGroupBLL.SingleModel.CheckGroupName(updateGroup.Name, aId.Value, updateGroup.Id))
            {
                result.msg = $"名称为[{updateGroup.Name}]的分组已存在，请选择其它名称";
                return Json(result);
            }
            bool isSuccess = false;
            if (updateGroup.Id > 0)
            {
                isSuccess = AuthGroupBLL.SingleModel.Update(updateGroup);
            }
            else
            {
                updateGroup.Aid = aId.Value;
                updateGroup.AddTime = DateTime.Now;
                isSuccess = int.Parse(AuthGroupBLL.SingleModel.Add(updateGroup).ToString()) > 0;
            }
            result.code = isSuccess ? 1 : 0;
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteGroup(int? aId = null, int? groupId = null)
        {
            if (!aId.HasValue || !groupId.HasValue)
            {
                result.msg = "参数异常";
            }
            result.code = AuthGroupBLL.SingleModel.DeleteGroup(aId: aId.Value, groupId: groupId.Value) ? 1 : 0;
            return Json(result);
        }

        [HttpGet]
        public ActionResult Account(int? aId = null, int? roleId = null)
        {
            AuthRole role = null;
            if (roleId.HasValue)
            {
                role = AuthRoleBLL.SingleModel.GetByAId(aId.Value, roleId: roleId.Value);
            }
            List<AuthGroup> group = AuthGroupBLL.SingleModel.GetListByAId(aId: aId.Value);
            return View(Tuple.Create(role, group));
        }


        [HttpPost]
        public JsonResult UpdateRole(AuthInfo authInfo = null, int? aId = null, AuthRole updateRole = null, bool isChangePwd = false)
        {
            if (updateRole == null)
            {
                result.msg = "提交数据为空";
                return Json(result);
            }
            if (updateRole.Id > 0 && updateRole.AId != aId.Value)
            {
                result.msg = "非法操作";
                return Json(result);
            }
            if (string.IsNullOrWhiteSpace(updateRole.Name))
            {
                result.msg = "名称不能为空";
                return Json(result);
            }
            if (updateRole.GroupId <= 0)
            {
                result.msg = "账号分组不能为空";
                return Json(result);
            }
            if (string.IsNullOrWhiteSpace(updateRole.LoginName))
            {
                result.msg = "登陆用户名不能为空";
                return Json(result);
            }
            if (updateRole.LoginName.Length < 6)
            {
                result.msg = "登录名长度至少6个字符";
                return Json(result);
            }
            if (!string.IsNullOrWhiteSpace(updateRole.Password) && updateRole.Password.Length < 6)
            {
                result.msg = "密码长度至少6个字符";
                return Json(result);
            }
            if (updateRole.Id == 0 && !AuthRoleBLL.SingleModel.CheckLoginName(aId.Value, updateRole.LoginName))
            {
                result.msg = "该登录用户名已使用，请重新选一个";
                return Json(result);
            }
            bool isSuccess = false;
            if (updateRole.Id > 0)
            {
                string pwdField = isChangePwd ? ",Password" : string.Empty;
                updateRole.Password = isChangePwd ? DESEncryptTools.GetMd5Base32(updateRole.Password) : string.Empty;
                isSuccess = AuthRoleBLL.SingleModel.Update(updateRole, $"Name,GroupId,Remark{pwdField}");
            }
            else
            {
                updateRole.Password = string.IsNullOrWhiteSpace(updateRole.Password) ? "123456" : updateRole.Password;
                updateRole.Password = DESEncryptTools.GetMd5Base32(updateRole.Password);
                updateRole.CreateUserId = authInfo.AuthAdmin != null ? authInfo.AuthAdmin.Id : 0;
                updateRole.AId = aId.Value;
                updateRole.AddTime = DateTime.Now;
                isSuccess = int.Parse(AuthRoleBLL.SingleModel.Add(updateRole).ToString()) > 0;
            }
            result.code = isSuccess ? 1 : 0;
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteRole(int? aId = null, int? roleId = null)
        {
            if (!aId.HasValue || !roleId.HasValue)
            {
                result.msg = "参数异常";
            }
            result.code = AuthRoleBLL.SingleModel.DeleteRole(aId: aId.Value, roleId: roleId.Value) ? 1 : 0;
            return Json(result);
        }

        /// <summary>
        /// 子帐号登陆入口
        /// </summary>
        [HttpGet, AllowAnonymous]
        public ActionResult Login(int? appId)
        {
            if (!appId.HasValue)
            {
                return new HttpStatusCodeResult(404);
            }
            return View(appId.Value);
        }

        /// <summary>
        /// 子帐号登陆接口
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="isKeep"></param>
        /// <param name="backurl"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public ActionResult Login(int? appId = null, string username = null, string password = null, bool isKeep = false, string backurl = null)
        {
            if (!appId.HasValue)
            {
                result.msg = "参数不能为空_appId";
                return Json(result);
            }

            //清除缓存
            Response.ContentEncoding = Encoding.UTF8;
            username = StringHelper.NoHtml(username.Trim());
            password = StringHelper.NoHtml(password);

            AuthRole admin = AuthRoleBLL.SingleModel.UserLogin(appId.Value, username, password);
            if (admin == null)
            {
                result.msg = "用户名或密码错误";
                return Json(result);
            }

            XcxAppAccountRelationBLL appBLL = XcxAppAccountRelationBLL.SingleModel;
            XcxAppAccountRelation app = appBLL.GetModel(admin.AId);
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

            int pageType = appBLL.GetXcxTemplateType(app.Id);
            Dictionary<int, string> getUrl = new Dictionary<int, string> {
                { (int)TmpType.拼享惠, $"/pin/main?Id={app.Id}&appId={app.Id}" },
                { (int)TmpType.小程序专业模板, $"/SubAccount/Welcome?appId={app.Id}&pagetype={pageType}" },
                { (int)TmpType.小未平台, $"/Plat/admin/Index?Id={appId}&appId={appId}" }
            };

            string url = getUrl[pageType];

            string loginToken = Utils.BuildCookie(account.Id, account.UpdateTime); //customer.useraccountid;
            string authToken = DESEncryptTools.DESEncrypt(admin.Id.ToString());

            result.code = 1;
            result.msg = "登陆成功";
            result.obj = new { loginToken, authToken, url };
            return Json(result);
        }

        [HttpGet]
        public ViewResult Welcome(AuthInfo authInfo, int? appId = null, int? pageType = null)
        {
            if(appId.HasValue)
            {
                ViewBag.appId = appId.Value;
            }
            if(pageType.HasValue)
            {
                ViewBag.pageType = pageType.Value;
            }
            return View(authInfo);
        }
    }
}