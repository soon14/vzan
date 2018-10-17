using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Controllers;
using Utility.IO;
using Entity.MiniApp.User;
using Entity.MiniApp;
using DAL.Base;
using BLL.MiniApp;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Footbath;
using User.MiniApp.Areas.MultiStore.Filters;
using BLL.MiniApp.Conf;
using Entity.MiniApp.Conf;

namespace User.MiniApp.Areas.MultiStore.Controllers
{
    /// <summary>
    /// 多门店登录
    ///   根据权限表UserRole里的权限查询出可以管理的店铺然后选择进入
    /// </summary>
    public class UserLoginController : baseController
    {
        
        
        

        private readonly int _appId = Context.GetRequestInt("appId", 0);
        private readonly int _storeId = Context.GetRequestInt("storeId", 0);


        public UserLoginController()
        {
            
            
        }

        /// <summary>
        /// 分店登录页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login()
        {
            //扫码登陆代码
            var sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }

            return View();
        }

        /// <summary>
        /// 检查用户是否已经使用二维码登录
        /// </summary>
        /// <param name="wxkey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WXLogin(string wxkey = "")
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            if (string.IsNullOrEmpty(wxkey))
            {
                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
            }
            if (lcode == null)
            {
                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
            }
            if (!lcode.IsLogin)
            {
                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
            }

            if (lcode.WxUser != null)
            {
                //已扫描
                Account accountmodel = null;
                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
                {
                    
                    log4net.LogHelper.WriteInfo(this.GetType(), $"MiniappUserBaseInfo ");
                    UserBaseInfo userInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
                    if (userInfo == null)
                    {
                        userInfo = new UserBaseInfo();
                        userInfo.openid = lcode.WxUser.openid;
                        userInfo.nickname = lcode.WxUser.nickname;
                        userInfo.headimgurl = lcode.WxUser.headimgurl;
                        userInfo.sex = lcode.WxUser.sex;
                        userInfo.country = lcode.WxUser.country;
                        userInfo.city = lcode.WxUser.city;
                        userInfo.province = lcode.WxUser.province;
                        userInfo.unionid = lcode.WxUser.unionid;
                        UserBaseInfoBLL.SingleModel.Add(userInfo);
                    }
                    accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser);
                    if (accountmodel == null)
                    {
                        return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
                    }
                    
                    Member member = MemberBLL.SingleModel.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
                    member.LastModified = DateTime.Now;//记录登录时间
                    MemberBLL.SingleModel.Update(member);
                    RedisUtil.Remove("SessionID:" + wxkey);
                    //CookieHelper.Remove("agent_UserCookieNew");
                    //CookieHelper.Remove("dz_UserCookieNew");
                    //CookieHelper.Remove("regphoneuserid");
                    //Session["dz_MultiStoreAccountId"] = 0;

                    //string cookiedomain = ".vzan.com";
                    //Session["dz_UserCookieNew"] = accountmodel.Id.ToString();
                    //CookieHelper.SetCookie("dz_UserCookieNew", accountmodel.Id.ToString(), cookiedomain, 60 * 24 * 10);

                    return Json(new { success = true, msg = accountmodel.Id.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 当前登陆者管理的店铺
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetManagerList()
        {
            //扫码登录账号
            if (dzaccount == null)
            {
                return Json(new { success = false, msg = "未登录！"+dzuserId.ToString() }, JsonRequestBehavior.AllowGet);
            }

            List<FootBath> storeMaterials = FootBathBLL.SingleModel.GetListByAccountId(dzaccount.Id); 
            
            if (storeMaterials == null || !storeMaterials.Any())
            {
                return Json(new { success = false, msg = "当前账号未管理任何店铺！" }, JsonRequestBehavior.AllowGet);
            }

            var stores = storeMaterials.Select(x => new { storeName = x.StoreName, storeAddress = x.Address, url = $"/MultiStore/StoresManager/Index?appId={x.appId}&storeId={x.Id}" });

            return Json(new { success = true, postData = stores }, JsonRequestBehavior.AllowGet);
            
        }
        

        /// <summary>
        /// 门店管理者绑定微信
        /// </summary>
        /// <param name="wxkey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StoreManagerBindWx(string wxkey = "")
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            if (string.IsNullOrEmpty(wxkey))
            {
                return Json(new { success = false, msg = "没有wxkey！" }, JsonRequestBehavior.AllowGet);
            }
            if (lcode == null)
            {
                return Json(new { success = false, msg = "lcode为NULL！" }, JsonRequestBehavior.AllowGet);
            }
            if (!lcode.IsLogin)
            {
                return Json(new { success = false, msg = "未登录！" }, JsonRequestBehavior.AllowGet);
            }

            if (lcode.WxUser != null)
            {
                //已扫描
                Account accountmodel = null;
                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
                {
                    
                    log4net.LogHelper.WriteInfo(this.GetType(), $"MiniappUserBaseInfo ");
                    UserBaseInfo userInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
                    if (userInfo == null)
                    {
                        userInfo = new UserBaseInfo();
                        userInfo.openid = lcode.WxUser.openid;
                        userInfo.nickname = lcode.WxUser.nickname;
                        userInfo.headimgurl = lcode.WxUser.headimgurl;
                        userInfo.sex = lcode.WxUser.sex;
                        userInfo.country = lcode.WxUser.country;
                        userInfo.city = lcode.WxUser.city;
                        userInfo.province = lcode.WxUser.province;
                        userInfo.unionid = lcode.WxUser.unionid;
                        UserBaseInfoBLL.SingleModel.Add(userInfo);
                    }
                    accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser);
                    if (accountmodel == null)
                    {
                        return Json(new { success = true, msg = "找不到绑定账号！" }, JsonRequestBehavior.AllowGet);
                    }
                    
                    Member member = MemberBLL.SingleModel.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
                    member.LastModified = DateTime.Now;//记录登录时间
                    MemberBLL.SingleModel.Update(member);
                    RedisUtil.Remove("SessionID:" + wxkey);

                    Int32 appId = Convert.ToInt32(Session["appId"] ?? "0");
                    Int32 storeId = Convert.ToInt32(Session["storeId"] ?? "0");

                    bool havingRole = UserRoleBLL.SingleModel.HavingRole(RoleType.分店管理员, appId, storeId);
                    if (havingRole)
                    {
                        return Json(new { success = true, msg = "该店铺已经有管理者,请解绑后再重新绑定！",isBind=true }, JsonRequestBehavior.AllowGet);
                    }

                    //添加当前扫码用户于当前店铺的管理权限
                    TransactionModel tran = new TransactionModel();
                    tran.Add(UserRoleBLL.SingleModel.BuildAddSql(new UserRole() { RoleId = (int)RoleType.分店管理员, AppId = appId, StoreId = storeId, State = 1, UserId = accountmodel.Id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now }));
                    FootBath store = FootBathBLL.SingleModel.GetModel(storeId);
                    store.ShopManagerName = lcode.WxUser.nickname;
                    store.ShopManager = accountmodel.LoginId;
                    tran.Add(FootBathBLL.SingleModel.BuildUpdateSql(store));
                    bool isSuccess = UserRoleBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);

                    if (!isSuccess)
                    {
                        return Json(new { success = true, msg = "加入数据库失败！" }, JsonRequestBehavior.AllowGet);
                    }


                    return Json(new { success = true, msg = "绑定成功！", ShopManagerName=store.ShopManagerName, ShopManager=store.ShopManager }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { success = false, msg = "找不到openid！" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
            }
           // return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 解除店铺权限
        /// </summary>
        /// <returns></returns>
        [LoginFilter, HttpPost]
        public ActionResult RemoveUserStoreManagerRole()
        {
            //删除用户在这个模板里的权限
            var deleteRoleSql = $" update userRole set State = 0 Where Appid = {_appId} and storeId = {_storeId} and RoleId = {(int)RoleType.分店管理员} and State = 1 ";

            TransactionModel tran = new TransactionModel();
            tran.Add(deleteRoleSql);
            FootBath store = FootBathBLL.SingleModel.GetModel(_storeId);
            store.ShopManagerName = string.Empty;
            store.ShopManager = string.Empty;
            tran.Add(FootBathBLL.SingleModel.BuildUpdateSql(store));
            bool isSuccess = UserRoleBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
            return Json(new { success = isSuccess, msg = isSuccess ? "解除权限成功!" : "解除权限失败!", ShopManagerName = store.ShopManagerName, ShopManager = store.ShopManager });
        }
    }
}