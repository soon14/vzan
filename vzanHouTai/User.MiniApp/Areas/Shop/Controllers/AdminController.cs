using BLL.MiniApp;
using BLL.MiniApp.Dish;
using Core.MiniApp;
using Core.MiniApp.DTO;
using Core.MiniApp.WeiXin;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;

namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop")]
    [RoutePrefix("Admin")]
    [Route("{action=Index}/{storeId?}")]
    //[RoutePrefix("Shop/Admin")]
    [LoginFilter(storePara: "store", accountPara: "account")]
    public class AdminController : BaseController
    {
        [HTMLRoute, AllowAnonymous]
        public void Index() { }

        [HttpPost, AllowAnonymous]
        public JsonResult MasterLogin(string username, string password, string wxToken = null)
        {
            bool wxLogin = !string.IsNullOrWhiteSpace(wxToken);
            bool hasInput = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
            if (!wxLogin && !hasInput)
            {
                return ApiModel(message: "用户名和密码不能为空");
            }

            Account account = null;
            C_UserInfo userInfo = null;
            if (wxLogin)
            {
                //微信授权登陆
                wxToken = HttpUtility.UrlDecode(wxToken);
                int userId;
                int.TryParse(DESEncryptTools.DESDecrypt(wxToken), out userId);
                userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
                account = AccountBLL.SingleModel.GetAccountByUnionId(userInfo.UnionId);
            }

            string loginToken = string.Empty;
            string authToken = string.Empty;
            if (account != null)
            {
                //登陆已绑定微信的账号
                authToken = DESEncryptTools.DESEncrypt(account.Id.ToString());
                loginToken = Utils.BuildCookie(account.Id, account.UpdateTime);
            }
            else if (hasInput)
            {
                bool success = false;
                //用账号密码登录账号
                account = AccountBLL.SingleModel.LoginUserWhole(username, password);
                if (account == null)
                {
                    return ApiModel(message: "用户名或密码错误");
                }
                if (userInfo != null)
                {
                    account.UnionId = userInfo.UnionId;
                    success = AccountBLL.SingleModel.Update(account, "UnionId");
                }
                if (userInfo != null && account != null && !success)
                {
                    return ApiModel(message: "账号绑定微信失败");
                }
                authToken = DESEncryptTools.DESEncrypt(account.Id.ToString());
                loginToken = Utils.BuildCookie(account.Id, account.UpdateTime);
            }
            else
            {
                return ApiModel(isok:true, message: "微信账号未绑定账号，请输入账号密码", data: "NewUser");
            }

            return ApiModel(isok: true, message: "登陆成功", data: new { loginToken, authToken });
        }

        [HttpPost, AllowAnonymous]
        public JsonResult Login(string username, string password, string wxToken = null)
        {
            bool hasInput = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
            //门店暂时不做微信登陆
            //bool wxLogin = !string.IsNullOrWhiteSpace(wxToken);

            //C_UserInfo userInfo = null;
            DishStore store = null;
            //if (!string.IsNullOrWhiteSpace(wxToken))
            //{
            //    int userId = 0;
            //    int.TryParse(DESEncryptTools.DESEncrypt(wxToken), out userId);
            //    userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            //    DishStoreBLL.SingleModel.GetStoreByUserId(userInfo.Id);
            //}

            string loginToken = string.Empty;
            if (store != null)
            {
                loginToken = DESEncryptTools.Encrypt(store.id.ToString());
            }
            else if (hasInput)
            {
                store = DishStoreBLL.SingleModel.GetAdminByLoginParams(username, password);
                if (store == null)
                {
                    return ApiModel(message: "用户名或密码错误");
                }
                loginToken = DESEncryptTools.DESEncrypt(store.id.ToString());
            }
            else
            {
                return ApiModel(message: "用户名和密码不能为空");
            }

            return ApiModel(isok: true, message: "登陆成功", data: new { loginToken, storeId = store.id });
        }

        [HttpGet, Route("WeChatLogin/{sessionGUID?}"), AllowAnonymous]
        public ActionResult WeChatLogin(string sessionGUID)
        {
            OAuthUserInfo oauthInfo = WxOAuth.SingleModel.GetOAuthInfo(sessionGUID);
            if (oauthInfo == null)
            {
                return Content("错误");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelFromCache(oauthInfo.openid);
            C_UserInfo updateUser = new C_UserInfo
            {
                NickName = oauthInfo.nickname,
                HeadImgUrl = oauthInfo.headimgurl,
                Address = $"{oauthInfo.country}{oauthInfo.province}{oauthInfo.city}",
                OpenId = oauthInfo.openid,
                UnionId = oauthInfo.unionid,
                Sex = oauthInfo.sex,
                Remark = "公众号移动端授权用户",
                AddTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };
            if (userInfo?.Id > 0)
            {
                C_UserInfoBLL.SingleModel.Update(updateUser, "NickName,HeadImgUrl,Address,Sex,UpdateTime");
                updateUser.Id = userInfo.Id;
            }
            else
            {
                int newId;
                if (!int.TryParse(C_UserInfoBLL.SingleModel.Add(updateUser)?.ToString(), out newId) || newId == 0)
                {
                    return Content("保存用户信息失败");
                }
                updateUser.Id = newId;
            }

            string loginType = string.Empty;
            Account account = AccountBLL.SingleModel.GetAccountByUnionId(updateUser.UnionId);
            if (!string.IsNullOrWhiteSpace(account?.Id.ToString()))
            {
                //已绑定小程序管理账号
                loginType = "master";
            }

            string loginToken = DESEncryptTools.DESEncrypt(updateUser.Id.ToString());
            return Redirect($"/Shop/Admin#/?token={HttpUtility.UrlEncode(loginToken)}&type={loginType}");
        }

        [HttpGet]
        public ActionResult GetStoreInfo(DishStore store)
        {
            return ApiModel(isok: true, message: "获取成功", data: new { Login = store.login_username });
        }

        [HttpPost]
        public ActionResult UpdateAccount(DishStore store, string login, string password)
        {
            if (string.IsNullOrEmpty(login))
            {
                return ApiModel(message: "登录名不能为空");
            }

            store.login_username = login;
            string updateField = "login_username";
            if (!string.IsNullOrWhiteSpace(password))
            {
                store.login_userpass = DESEncryptTools.GetMd5Base32(password);
                updateField += ",login_userpass";
            }

            bool success = DishStoreBLL.SingleModel.Update(store, updateField);
            return ApiModel(isok: success, message: success ? "操作成功" : "操作失败");
        }

        [HttpGet, MasterAuthOnly]
        public JsonResult App(Account account)
        {
            List<XcxAppAccountRelation> apps = XcxAppAccountRelationBLL.SingleModel.GetListByTemplateType(account.Id.ToString(), TmpType.智慧餐厅);
            if (apps == null || apps.Count == 0)
            {
                return ApiModel(message: "没有开通智慧餐厅");
            }

            List<XcxTemplate> appTemplates = XcxTemplateBLL.SingleModel.GetListByIds(string.Join(",", apps.Select(s => s.TId).Distinct()));
            List<OpenAuthorizerConfig> appConfigs = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(account.Id.ToString(), string.Join(",", apps.Select(item => item.Id)));

            string appIds = $"'{string.Join("','", apps.FindAll(item => !string.IsNullOrEmpty(item.AppId)).Select(s => s.AppId))}'";
            List<UserXcxTemplate> appPublish = UserXcxTemplateBLL.SingleModel.GetListByAppIds(appIds);

            object formatDTO = apps.Select(app =>
            {
                XcxTemplate template = appTemplates.Where(thisItem => thisItem.Id == app.TId).FirstOrDefault();
                if (app.AuthoAppType == 1)
                {
                    OpenAuthorizerConfig config = appConfigs.FirstOrDefault(thisItem => thisItem.RId == app.Id);
                    app.XcxName = config?.nick_name;
                    app.UploadStateName = XcxTypeEnum.未发布.ToString();
                    if (config != null && !string.IsNullOrWhiteSpace(app.AppId))
                    {
                        UserXcxTemplate publish = appPublish?.FirstOrDefault(item => item.AppId == app.AppId);
                        if (publish != null)
                        {
                            if (publish.AddTime.ToString("yyyy-MM-dd") == "0001-01-01")
                            {
                                publish.State = (int)XcxTypeEnum.未发布;
                                UserXcxTemplateBLL.SingleModel.Update(publish, "state");
                            }
                            app.UploadStateName = Enum.GetName(typeof(XcxTypeEnum), publish.State);
                        }
                    }
                }
                else
                {
                    app.XcxName = appPublish?.FirstOrDefault(f => f.AppId == app.AppId)?.Name;
                    app.UploadStateName = "手动发布";
                }

                if (string.IsNullOrWhiteSpace(app.XcxName))
                {
                    app.XcxName = "未绑定公众号小程序";
                }

                string expireDay = string.Empty;
                if (app.outtime <= DateTime.Now)
                {
                    expireDay = "0天";
                }
                else
                {
                    expireDay = Convert.ToInt32(Math.Round(app.outtime.Subtract(DateTime.Now).TotalDays)).ToString() + "天";
                }

                return new
                {
                    app.Id,
                    Template = template.TName,
                    AppTitle = app.XcxName,
                    Icon = template.TImgurl,
                    ExpireDay = expireDay,
                    StoreCount = app.SCount,
                    PublishState = app.UploadStateName,
                };
            });

            return ApiModel(isok: true, message: "获取成功", data: formatDTO);
        }

        [Route("{aId?}/Store")]
        [HttpGet]
        public JsonResult Store(Account account, int? aId, int pageIndex = 1, int pageSize = 10)
        {
            List<XcxAppAccountRelation> miniapplist;
            if (aId.HasValue)
            {
                miniapplist = new List<XcxAppAccountRelation> { XcxAppAccountRelationBLL.SingleModel.GetModel(aId.Value) };
            }
            else
            {
                miniapplist = XcxAppAccountRelationBLL.SingleModel.GetListByTemplateType(account.Id.ToString(), TmpType.智慧餐厅);
            }
            string aIds = string.Join(",", miniapplist.Select(item => item.Id));

            if (string.IsNullOrWhiteSpace(aIds))
            {
                return ApiModel(message: "无管理店铺");
            }

            List<DishStore> store = DishStoreBLL.SingleModel.GetListByAids(aIds, pageIndex, pageSize);
            int total = DishStoreBLL.SingleModel.GetCountByAids(aIds);
            object formatData = store.Select(item => new
            {
                Name = item.dish_name,
                Logo = item.dish_logo,
                Id = item.id,
                CellPhone = item.dish_con_mobile,
                Tel = item.dish_con_phone,
                Login = item.login_username,
                Begin = item.dish_begin_time.ToString("yyyy/MM/dd"),
                Expire = item.dish_end_time.ToString("yyyy/MM/dd"),
                IsMain = item.ismain == 1
            });

            return ApiModel(isok: true, message: "获取成功", data: new { page = formatData, total });
        }

        [HttpGet, MasterAuthOnly]
        public JsonResult GetShopAuth(Account account, DishStore store)
        {
            string loginToken = DESEncryptTools.DESEncrypt(store.id.ToString());
            return ApiModel(isok: true, message: "获取授权Token成功", data: new { loginToken });
        }

        [Route("{aId}/AddStore")]
        [HttpPost, MasterAuthOnly, FormValidate]
        public JsonResult AddStore(Account account, [System.Web.Http.FromBody]EditStore edit, int? aId)
        {
            if (!aId.HasValue)
            {
                return ApiModel(message: "参数不能为空_aId");
            }
            if (string.IsNullOrEmpty(edit.Password))
            {
                return ApiModel(message: "密码不能为空");
            }

            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(aId.Value, account.Id.ToString());

            int storeCount = DishStoreBLL.SingleModel.GetCount($"aid={app.Id} and state<>-1");
            if (storeCount >= app.SCount)
            {
                return ApiModel(message: $"门店数量已达到上限，您最多只能创建{app.SCount}个门店");
            }

            DishStore store = new DishStore
            {
                updateTime = DateTime.Now,
                dish_name = edit.Name,
                dish_logo = edit.Logo,
                dish_begin_time = edit.Begin.Value,
                dish_end_time = edit.Expire.Value,
                login_username = edit.Login,
                //如果只能创建一个门店，默认设置为主店
                ismain = app.SCount <= 1 && storeCount == 0 ? 1 : 0,
                login_userpass = DESEncryptTools.GetMd5Base32(edit.Password),
                aid = app.Id,
            };
            int newId = 0;
            bool success = int.TryParse(DishStoreBLL.SingleModel.Add(store)?.ToString(), out newId) && newId > 0;

            return ApiModel(isok: success, message: success ? "新增成功" : "新增失败");
        }

        [HttpPost, MasterAuthOnly, FormValidate]
        public JsonResult EditStore(Account account, DishStore store, [System.Web.Http.FromBody]EditStore edit)
        {
            if (DishStoreBLL.SingleModel.CheckExistLoginName(store.id, store.aid, edit.Login))
            {
                //请输入门店管理员账号
                return ApiModel(message: "存在同名的管理者账号,请修改！");
            }

            store.updateTime = DateTime.Now;
            store.dish_name = edit.Name;
            store.dish_logo = edit.Logo;
            store.dish_begin_time = edit.Begin.Value;
            store.dish_end_time = edit.Expire.Value;
            store.login_username = edit.Login;
            string updateColumns = "dish_name,dish_logo,dish_begin_time,dish_end_time,login_username,updateTime";
            if (!string.IsNullOrEmpty(edit.Password))
            {
                updateColumns += ",login_userpass";
                store.login_userpass = DESEncryptTools.GetMd5Base32(edit.Password);
            }
            bool success = DishStoreBLL.SingleModel.Update(store, updateColumns);

            return ApiModel(isok: success, message: success ? "更新成功" : "更新失败");
        }

        [Route("SetMainStore/{storeId}/{enable}")]
        [HttpPost, MasterAuthOnly]
        public ActionResult SetMainStore(bool? enable, Account account, DishStore store)
        {
            if (!enable.HasValue)
            {
                return ApiModel(message: "参数不合法");
            }

            int enableValue = enable.Value ? 1 : 0;
            bool success = DishStoreBLL.SingleModel.SetMainStore(enableValue, store.aid, store.id);

            return ApiModel(isok: success, message: success ? "设置成功" : "设置失败");
        }

        [HttpPost, MasterAuthOnly]
        public JsonResult DeleteShop(Account account, DishStore store)
        {
            bool success = DishStoreBLL.SingleModel.DelStore(store);
            return ApiModel(isok: success, message: success ? "删除成功" : "删除失败");
        }

#if DEBUG
        public JsonResult Test(string accountId)
        {
            Account account = AccountBLL.SingleModel.GetModel(Guid.Parse(accountId));
            return ApiModel(message: Utils.BuildCookie(account.Id, account.UpdateTime), code: DESEncryptTools.DESEncrypt(account.Id.ToString()));
        }
#endif
    }
}