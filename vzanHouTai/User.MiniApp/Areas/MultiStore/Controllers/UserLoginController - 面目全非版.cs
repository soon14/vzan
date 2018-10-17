//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using User.MiniApp.Controllers;
//using Utility.IO;
//using Entity.MiniApp.User;
//using Utility;
//using Entity.MiniApp;
//using DAL.Base;
//using BLL.MiniApp;
//using BLL.MiniApp.Footbath;
//using Entity.MiniApp.Footbath;
//using User.MiniApp.Areas.MultiStore.Filters;
//using MySql.Data.MySqlClient;

//namespace User.MiniApp.Areas.MultiStore.Controllers
//{
//    /// <summary>
//    /// 多门店登录
//    /// ps: 一个AppId下同个用户只能绑定一个分店账户
//    ///     一个分店账户可成为多个店铺的管理者
//    ///     考虑表耦合的关系,不可以解绑,只能删除.
//    /// </summary>
//    public class UserLoginController : baseController
//    {
//        private readonly MultiStoreAccountBLL _multiStoreAccountBLL;
//        private readonly FootBathBLL _footBathBLL;
//        private readonly PayCenterSettingBLL _payCenterSettingBLL;
//        private readonly UserRoleBLL _userRoleBLL;

//        private readonly int _appId = Context.GetRequestInt("appId", 0);
//        private readonly int _storeId = Context.GetRequestInt("storeId", 0);


//        public UserLoginController()
//        {
//            _multiStoreAccountBLL = new MultiStoreAccountBLL();
//            _footBathBLL = new FootBathBLL();
//            _payCenterSettingBLL = new PayCenterSettingBLL();
//            _userRoleBLL = new UserRoleBLL();
//        }
//        #region 绑定门店

//        /// <summary>
//        /// 门店管理者绑定微信
//        /// </summary>
//        /// <param name="wxkey"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult StoreManagerBindWx(string wxkey = "")
//        {
//            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
//            if (string.IsNullOrEmpty(wxkey))
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }
//            if (lcode == null)
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }
//            if (!lcode.IsLogin)
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }

//            if (lcode.WxUser != null)
//            {
//                //已扫描
//                Account accountmodel = null;
//                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
//                {
//                    UserBaseInfoBLL ubll = new UserBaseInfoBLL();
//                    log4net.LogHelper.WriteInfo(this.GetType(), $"MiniappUserBaseInfo ");
//                    UserBaseInfo userInfo = ubll.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
//                    if (userInfo == null)
//                    {
//                        userInfo = new UserBaseInfo();
//                        userInfo.openid = lcode.WxUser.openid;
//                        userInfo.nickname = lcode.WxUser.nickname;
//                        userInfo.headimgurl = lcode.WxUser.headimgurl;
//                        userInfo.sex = lcode.WxUser.sex;
//                        userInfo.country = lcode.WxUser.country;
//                        userInfo.city = lcode.WxUser.city;
//                        userInfo.province = lcode.WxUser.province;
//                        userInfo.unionid = lcode.WxUser.unionid;
//                        ubll.Add(userInfo);
//                    }
//                    accountmodel = _accountBLL.GetAccountByWeixinUser(lcode.WxUser);
//                    if (accountmodel == null)
//                    {
//                        return Json(new { success = true, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//                    }
//                    MemberBLL bllMember = new MemberBLL();
//                    Member member = bllMember.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
//                    member.LastModified = DateTime.Now;//记录登录时间
//                    bllMember.Update(member);
//                    RedisUtil.Remove("SessionID:" + wxkey);

//                    Int32 appId = Convert.ToInt32(Session["appId"] ?? "0");
//                    Int32 storeId = Convert.ToInt32(Session["storeId"] ?? "0");

//                    bool havingRole = _userRoleBLL.HavingRole(RoleType.分店管理员, appId, storeId);
//                    if (havingRole)
//                    {
//                        return Json(new { success = true, msg = "该店铺已经有管理者,请解绑后再重新绑定！" }, JsonRequestBehavior.AllowGet);
//                    }

//                    bool isSuccess = Convert.ToInt32(_userRoleBLL.Add(new UserRole() { RoleId = (int)RoleType.分店管理员, AppId = appId, StoreId = storeId, State = 1, UserId = accountmodel.Id, CreateDate = DateTime.Now, UpdateDate = DateTime.Now })) > 0;

//                    if (!isSuccess)
//                    {
//                        return Json(new { success = true, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//                    }


//                    return Json(new { success = true, msg = "绑定成功！" }, JsonRequestBehavior.AllowGet);
//                }
//            }
//            else
//            {
//                return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//            }
//            return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//        }
//        #endregion


//        #region 登录

//        // GET: MultiStore/UserLogin
//        public ActionResult Login()
//        {
//            //清除缓存
//            CookieHelper.Remove("agent_UserCookieNew");
//            CookieHelper.Remove("dz_UserCookieNew");
//            CookieHelper.Remove("regphoneuserid");

//            //扫码登陆代码
//            var sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
//            Session["qrcodekey"] = sessonid;
//            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
//            {
//                LoginQrCode wxkey = new LoginQrCode();
//                wxkey.SessionId = sessonid;
//                wxkey.IsLogin = false;
//                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
//            }

//            return View();
//        }


//        /// <summary>
//        /// 密码请求登录
//        /// </summary>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult LoginRequest()
//        {
//            //清除缓存
//            CookieHelper.Remove("agent_UserCookieNew");
//            CookieHelper.Remove("dz_UserCookieNew");
//            CookieHelper.Remove("regphoneuserid");

//            //设定请求编码格式
//            Response.ContentEncoding = System.Text.Encoding.UTF8;
//            string merNo = StringHelper.NoHtml(Context.GetRequest("MerNo", string.Empty));
//            string loginName = StringHelper.NoHtml(Context.GetRequest("LoginName", string.Empty));
//            string password = StringHelper.NoHtml(Context.GetRequest("Password", string.Empty));

//            //当前请求登录的用户
//            MultiStoreAccount curUser = _multiStoreAccountBLL.GetModelByParams(merNo, loginName, password);
//            if (curUser == null)
//            {
//                return Json(new { success = false,msg = "登录失败,请检查填写的登录信息是否正确" });
//            }
//            string cookiedomain = ".vzan.com";
//            Session["dz_MultiStoreAccountId"] = curUser.Id.ToString();
//            CookieHelper.SetCookie("dz_UserCookieNew", curUser.AccountId.ToString(), cookiedomain, 60 * 24 * 10);

//            return Json(new { success = true, msg = "登录成功！"});
//        }
        
        
//        /// <summary>
//        /// 检查用户是否已经使用二维码登录
//        /// </summary>
//        /// <param name="wxkey"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult WXLogin(string wxkey = "")
//        {
//            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
//            if (string.IsNullOrEmpty(wxkey))
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }
//            if (lcode == null)
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }
//            if (!lcode.IsLogin)
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }

//            if (lcode.WxUser != null)
//            {
//                //已扫描
//                Account accountmodel = null;
//                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
//                {
//                    UserBaseInfoBLL ubll = new UserBaseInfoBLL();
//                    log4net.LogHelper.WriteInfo(this.GetType(), $"MiniappUserBaseInfo ");
//                    UserBaseInfo userInfo = ubll.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
//                    if (userInfo == null)
//                    {
//                        userInfo = new UserBaseInfo();
//                        userInfo.openid = lcode.WxUser.openid;
//                        userInfo.nickname = lcode.WxUser.nickname;
//                        userInfo.headimgurl = lcode.WxUser.headimgurl;
//                        userInfo.sex = lcode.WxUser.sex;
//                        userInfo.country = lcode.WxUser.country;
//                        userInfo.city = lcode.WxUser.city;
//                        userInfo.province = lcode.WxUser.province;
//                        userInfo.unionid = lcode.WxUser.unionid;
//                        ubll.Add(userInfo);
//                    }
//                    accountmodel = _accountBLL.GetAccountByWeixinUser(lcode.WxUser);
//                    if (accountmodel == null)
//                    {
//                        return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//                    }
//                    MemberBLL bllMember = new MemberBLL();
//                    Member member = bllMember.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
//                    member.LastModified = DateTime.Now;//记录登录时间
//                    bllMember.Update(member);
//                    RedisUtil.Remove("SessionID:" + wxkey);
//                    CookieHelper.Remove("agent_UserCookieNew");
//                    CookieHelper.Remove("dz_UserCookieNew");
//                    CookieHelper.Remove("regphoneuserid");
//                    Session["dz_MultiStoreAccountId"] = 0;

//                    string cookiedomain = ".vzan.com";
//                    Session["dz_UserCookieNew"] = accountmodel.Id.ToString();
//                    CookieHelper.SetCookie("dz_UserCookieNew", accountmodel.Id.ToString(), cookiedomain, 60 * 24 * 10);

//                    return Json(new { success = true, msg = "成功！" }, JsonRequestBehavior.AllowGet);
//                }
//            }
//            else
//            {
//                return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);
//            }
//            return Json(new { success = false, msg = "登录失败！" }, JsonRequestBehavior.AllowGet);

//        }


//        /// <summary>
//        /// 微信登录后选择使用的账号   --逻辑修改后不用
//        /// </summary>
//        /// <param name="wxkey"></param>
//        /// <returns></returns>
//        [HttpPost]
//        public ActionResult WXLogin_multiStoreUser()
//        {
//            //扫码登录选择账号
//            int multiStoreAccountId = Context.GetRequestInt("multiStoreAccountId", 0);
//            if (multiStoreAccountId == 0)
//            {
//                return Json(new { success = false, msg = "未检测到有效用户,请重新选择登录！" }, JsonRequestBehavior.AllowGet);
//            }

//            MultiStoreAccount multiStoreAccount = _multiStoreAccountBLL.GetModel(multiStoreAccountId);
//            if (multiStoreAccount == null)
//            {
//                return Json(new { success = false, msg = "用户不存在,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//            }

//            //验证是否选择分店登录账号与当前登录账号关联
//            if (!multiStoreAccount.AccountId.Equals(multiStore_Account.Id))
//            {
//                return Json(new { success = false, msg = "该用户账号与当前登录号不存在绑定关系！" }, JsonRequestBehavior.AllowGet);
//            }

//            CookieHelper.Remove("dz_MultiStoreUserCookie");
//            string cookiedomain = ".vzan.com";
//            Session["dz_MultiStoreUserCookie"] = multiStoreAccount.Id.ToString();
//            CookieHelper.SetCookie("dz_MultiStoreUserCookie", multiStoreAccount.Id.ToString(), cookiedomain, 60 * 24 * 10);
//            return Json(new { success = true, msg = "登录成功！" }, JsonRequestBehavior.AllowGet);
//        }


//        /// <summary>
//        /// 当前登陆者管理的店铺
//        /// </summary>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        [HttpGet]
//        public ActionResult GetManagerList()
//        {
//            //扫码登录账号
//            if (dzaccount == null)
//            {
//                return Json(new { success = false, msg = "未登录！" }, JsonRequestBehavior.AllowGet);
//            }

//            List<FootBath> storeMaterials = _footBathBLL.GetListByAccountId(dzaccount.Id); 
            
//            if (storeMaterials == null || !storeMaterials.Any())
//            {
//                return Json(new { success = false, msg = "当前账号未管理任何店铺！" }, JsonRequestBehavior.AllowGet);
//            }

//            var stores = storeMaterials.Select(x => new { storeName = x.StoreName, storeAddress = x.Address, url = $"/MultiStore/StoresManager/Index?appId={x.appId}&storeId={x.Id}" });

//            return Json(new { success = true, postData = stores }, JsonRequestBehavior.AllowGet);
            
//        }
//        #endregion

//        #region 登录账号管理
//        /// <summary>
//        /// 登录账号管理页面
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter(certainlyAppMasterRole = true),HttpGet]
//        public ActionResult UserLoginManager()
//        {
//            string merNo = string.Empty;

//            ViewBag.appId = _appId;
//            XcxAppAccountRelation role = _xcxAppAccountRelationBLL.GetModel(_appId);
//            if (role != null)
//            {
//                PayCenterSetting userPaySetting = _payCenterSettingBLL.GetPayCenterSettingByappid(role.AppId);
//                if (userPaySetting != null)
//                {
//                    merNo = userPaySetting.Mch_id;
//                }
//            }
//            ViewBag.MerNo = merNo;//商户号

//            return View();
//        }

//        /// <summary>
//        /// 登录账号列表获取  --不用,会影响逻辑
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter, HttpGet]
//        public ActionResult GetUserAccountList()
//        {
//            List<MultiStoreAccount> multiStoreAccounts = _multiStoreAccountBLL.GetListByMasterAccountId(dzaccount.Id, _appId);
//            var postData = new
//            {
//                multiStoreAccounts = multiStoreAccounts,
//                recordCount = multiStoreAccounts == null ? 0 : multiStoreAccounts.Count
//            };

//            return Json(new { success = true, msg = "成功!", postData = postData }, JsonRequestBehavior.AllowGet);
//        }

//        /// <summary>
//        /// 登录账号 - 删除
//        /// </summary>
//        /// <returns></returns>
//        //[MasterStoreLoginFilter, HttpPost]
//        [LoginFilter(certainlyAppMasterRole = true)]
//        public ActionResult DeleteUserAccount()
//        {
//            int multiStoreAccountId = Context.GetRequestInt("Id", 0);
//            if (multiStoreAccountId <= 0)
//            {
//                return Json(new { success = false, msg = "删除失败!" });
//            }
//            MultiStoreAccount multiStoreAccount = _multiStoreAccountBLL.GetModel($" Id = {multiStoreAccountId} ");
//            if (multiStoreAccount == null)
//            {
//                return Json(new { success = false, msg = "删除失败!" });
//            }
//            multiStoreAccount.State = -1;//删除
//            var isSuccess = _multiStoreAccountBLL.Update(multiStoreAccount, "State");
//            if (isSuccess)
//            {
//                //删除用户在这个模板里的权限
//                var deleteRoleSql = $" update userRole set State = 0 Where Appid = {multiStoreAccount.AppId} and UserId = '{multiStoreAccount.AccountId}' and RoleId = {(int)RoleType.分店管理员} ";
//                SqlMySql.ExecuteNonQuery(_userRoleBLL.connName, System.Data.CommandType.Text, deleteRoleSql);
//            }

//            return Json(new { success = isSuccess, msg = isSuccess ?  "删除成功!" : "删除失败!"});
//        }

//        /// <summary>
//        /// 登录账号 - 添加
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter(certainlyAppMasterRole = true),HttpPost]
//        public ActionResult AddUserAccount(MultiStoreAccount multiStoreAccount)
//        {
//            if (multiStoreAccount.LoginName.Length < FilterSpecial(multiStoreAccount.LoginName).Length)
//            {
//                return Json(new { success = false, msg = "登录名含有特殊字符！" }, JsonRequestBehavior.AllowGet);
//            }

//            if (multiStoreAccount.Password.Length < FilterSpecial(multiStoreAccount.Password).Length)
//            {
//                return Json(new { success = false, msg = "密码仅能输入数字和英文！" }, JsonRequestBehavior.AllowGet);
//            }

            
//            if (multiStoreAccount.LoginName.Length > 20)
//            {
//                return Json(new { success = false, msg = "登录名长度不能超过20个字符！" }, JsonRequestBehavior.AllowGet);
//            }
//            if (multiStoreAccount.Password.Length < 6)            {
//                return Json(new { success = false, msg = "密码请输入6位以上数字和英文！" }, JsonRequestBehavior.AllowGet);
//            }

//            if (multiStoreAccount.Remark.Length > 100)
//            {
//                return Json(new { success = false, msg = "备注请输入100字内的内容！" }, JsonRequestBehavior.AllowGet);
//            }

//            XcxAppAccountRelation role = _xcxAppAccountRelationBLL.GetModel(_appId);
//            if (role == null)
//            {
//                return Json(new { success = false, msg = "备注请输入100字内的内容！" }, JsonRequestBehavior.AllowGet);
//            }
//            PayCenterSetting userPaySetting = _payCenterSettingBLL.GetPayCenterSettingByappid(role.AppId);
//            if (userPaySetting == null)
//            {
//                return Json(new { success = false, msg = "商户号不能为空,请先绑定小程序商户号！" }, JsonRequestBehavior.AllowGet);
//            }

//            multiStoreAccount.AppId = _appId;
//            multiStoreAccount.MerNo = userPaySetting.Mch_id;//商户号
//            multiStoreAccount.MasterAccountId = dzaccount.Id;

//            var isRepetitionLoginName = _multiStoreAccountBLL.repetitionLoginName(multiStoreAccount);
//            if (isRepetitionLoginName)
//            {
//                return Json(new { success = false, msg = "登录名重复,请更换登录名！" }, JsonRequestBehavior.AllowGet);
//            }
//            try
//            {
//                multiStoreAccount.Id = Convert.ToInt32(_multiStoreAccountBLL.Add(multiStoreAccount));

//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
//            }

//            bool isSuccess = multiStoreAccount.Id > 0;
//            if (isSuccess)
//            {
//                return Json(new { success = true, msg = "添加成功" },JsonRequestBehavior.AllowGet);
//            }
//            else
//            {
//                return Json(new { success = false, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
//            }
//        }


//        /// <summary>
//        /// 登录账号 - 编辑
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter(certainlyAppMasterRole = true),HttpPost]
//        public ActionResult EditUserAccount(MultiStoreAccount multiStoreAccount)
//        {
//            if (multiStoreAccount.LoginName.Length < FilterSpecial(multiStoreAccount.LoginName).Length)
//            {
//                return Json(new { success = false, msg = "登录名含有特殊字符！" });
//            }

//            if (multiStoreAccount.Password.Length < FilterSpecial(multiStoreAccount.Password).Length)
//            {
//                return Json(new { success = false, msg = "密码仅能输入数字和英文！" });
//            }

//            if (multiStoreAccount.LoginName.Length > 20)
//            {
//                return Json(new { success = false, msg = "登录名长度不能超过20个字符！" });
//            }
//            if (multiStoreAccount.Password.Length < 6)
//            {
//                return Json(new { success = false, msg = "密码请输入6位以上数字和英文！" });
//            }

//            if (multiStoreAccount.Remark.Length > 100)
//            {
//                return Json(new { success = false, msg = "备注请输入100字内的内容！" });
//            }

//            if (multiStoreAccount.Id < 0)
//            {
//                return Json(new { success = false, msg = "记录错误！" });
//            }
//            MultiStoreAccount multiStoreAccount_Db = _multiStoreAccountBLL.GetModel(multiStoreAccount.Id);
//            if (multiStoreAccount == null)
//            {
//                return Json(new { success = false, msg = "记录错误！" });
//            }

//            multiStoreAccount_Db.LoginName = multiStoreAccount.LoginName;
//            multiStoreAccount_Db.Password = multiStoreAccount.Password;
//            multiStoreAccount_Db.Remark = multiStoreAccount.Remark;

//            //验证登录名是否重复
//            var isRepetitionLoginName = _multiStoreAccountBLL.repetitionLoginName(multiStoreAccount_Db);
//            if (isRepetitionLoginName)
//            {
//                return Json(new { success = false, msg = "登录名重复,请更换登录名！" });
//            }

//            bool isSuccess = _multiStoreAccountBLL.Update(multiStoreAccount_Db, "LoginName,Password,Remark");
//            if (isSuccess)
//            {
//                return Json(new { success = true, msg = "更新成功" });
//            }
//            else
//            {
//                return Json(new { success = false, msg = "更新失败" });
//            }
//        }

//        /// <summary>
//        /// 登录账号 - 解除绑定   --不可用,会影响逻辑！！！
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter,HttpPost]
//        public ActionResult RemoveUserAccount()
//        {
//            int multiStoreAccountId = Context.GetRequestInt("Id", 0);
//            if (multiStoreAccountId <= 0)
//            {
//                return Json(new { success = false, msg = "解绑失败!" });
//            }
//            MultiStoreAccount multiStoreAccount = _multiStoreAccountBLL.GetModel($" Id = {multiStoreAccountId} ");
//            if (multiStoreAccount == null || multiStoreAccount.State < 0)
//            {
//                return Json(new { success = false, msg = "解绑失败!" });
//            }

//            multiStoreAccount.State = 0;//删除
//            multiStoreAccount.AccountId = Guid.Empty;
//            var isSuccess = _multiStoreAccountBLL.Update(multiStoreAccount, "State,AccountId");
//            return Json(new { success = isSuccess, msg = isSuccess ? "解绑成功!" : "解绑失败!" });
//        }


//        /// <summary>
//        /// 登录账号绑定弹框内容
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter(certainlyAppMasterRole = true), HttpGet]
//        public ActionResult _BindUserAccountBox(MultiStoreAccount multiStoreAccount)
//        {
//            //扫码登陆代码
//            var sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
//            Session["qrcodekey"] = sessonid;
//            Session["multiStoreAccountId"] = multiStoreAccount.Id;
//            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
//            {
//                LoginQrCode wxkey = new LoginQrCode();
//                wxkey.SessionId = sessonid;
//                wxkey.IsLogin = false;
//                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
//            }

//            return View("_BindWXToUserAccount");
//        }


//        /// <summary>
//        /// 登录账号 - 绑定
//        /// </summary>
//        /// <returns></returns>
//        [LoginFilter(certainlyAppMasterRole = true),HttpPost]
//        public ActionResult checkBindUserAccount(string wxkey = "")
//        {
//            string bug = "";
//            try
//            {
//                LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
//                if (string.IsNullOrEmpty(wxkey))
//                {
//                    return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//                }
//                if (lcode == null)
//                {
//                    return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//                }
//                if (!lcode.IsLogin)
//                {
//                    return Json(new { success = false, msg = "绑定失败！" }, JsonRequestBehavior.AllowGet);
//                }

//                if (lcode.WxUser != null)
//                {
//                    //已扫描
//                    Account accountmodel = null;
//                    if (!string.IsNullOrEmpty(lcode.WxUser.openid))
//                    {
//                        UserBaseInfoBLL ubll = new UserBaseInfoBLL();
//                        log4net.LogHelper.WriteInfo(this.GetType(), $"MiniappUserBaseInfo ");
//                        UserBaseInfo userInfo = ubll.GetModelByOpenId(lcode.WxUser.openid, lcode.WxUser.serverid);
//                        if (userInfo == null)
//                        {
//                            userInfo = new UserBaseInfo();
//                            userInfo.openid = lcode.WxUser.openid;
//                            userInfo.nickname = lcode.WxUser.nickname;
//                            userInfo.headimgurl = lcode.WxUser.headimgurl;
//                            userInfo.sex = lcode.WxUser.sex;
//                            userInfo.country = lcode.WxUser.country;
//                            userInfo.city = lcode.WxUser.city;
//                            userInfo.province = lcode.WxUser.province;
//                            userInfo.unionid = lcode.WxUser.unionid;
//                            ubll.Add(userInfo);
//                        }
//                        accountmodel = _accountBLL.GetAccountByWeixinUser(lcode.WxUser);
//                        if (accountmodel == null)
//                        {
//                            return Json(new { success = true, msg = "绑定失败,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        MemberBLL bllMember = new MemberBLL();
//                        Member member = bllMember.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
//                        member.LastModified = DateTime.Now;//记录登录时间
//                        bllMember.Update(member);
//                        RedisUtil.Remove("SessionID:" + wxkey);
//                        bug += "a";

//                        //绑定账号
//                        int multiStoreAccount_Id = Convert.ToInt32(Session["multiStoreAccountId"]);
//                        MultiStoreAccount multiStoreAccount_DB = _multiStoreAccountBLL.GetModel(multiStoreAccount_Id);
//                        if (multiStoreAccount_DB == null || !multiStoreAccount_DB.AccountId.Equals(Guid.Empty))
//                        {
//                            return Json(new { success = true, msg = "绑定失败,请刷新页面重试！" + multiStoreAccount_Id }, JsonRequestBehavior.AllowGet);
//                        }
//                        bug += "b";
//                        //一个AppId下同个用户只能绑定一个分店账户
//                        if (_multiStoreAccountBLL.Exists($" AppId = {multiStoreAccount_DB.AppId} And AccountId = '{accountmodel.Id}' And State = 1 "))
//                        {
//                            return Json(new { success = true, msg = "绑定失败,一个微信账号只能绑定一个分店登录账户,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//                        }
//                        bug = "c";
//                        multiStoreAccount_DB.AccountId = accountmodel.Id;
//                        multiStoreAccount_DB.State = 1;
//                        bool isSuccess = _multiStoreAccountBLL.Update(multiStoreAccount_DB, "AccountId,State");
//                        if (isSuccess)
//                        {
//                            return Json(new { success = true, msg = "绑定成功！" }, JsonRequestBehavior.AllowGet);
//                        }
//                    }
//                }
//                else
//                {
//                    return Json(new { success = true, msg = "绑定失败,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//                }
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = true, msg = bug + "_" + ex.Message }, JsonRequestBehavior.AllowGet);
//            }
            
//            return Json(new { success = true, msg = "绑定失败,请刷新页面重试！" }, JsonRequestBehavior.AllowGet);
//        }


//        // <summary>
//        /// 过滤特殊字符
//        /// 如果字符串为空，直接返回。
//        /// </summary>
//        /// <param name="str">需要过滤的字符串</param>
//        /// <returns>过滤好的字符串</returns>
//        public static string FilterSpecial(string str)
//        {
//            if (str == "")
//            {
//                return str;
//            }
//            else
//            {
//                str = str.Replace("'", "");
//                str = str.Replace("<", "");
//                str = str.Replace(">", "");
//                str = str.Replace("%", "");
//                str = str.Replace("'delete", "");
//                str = str.Replace("''", "");
//                str = str.Replace("\"\"", "");
//                str = str.Replace(",", "");
//                str = str.Replace(".", "");
//                str = str.Replace(">=", "");
//                str = str.Replace("=<", "");
//                str = str.Replace("-", "");
//                str = str.Replace("_", "");
//                str = str.Replace(";", "");
//                str = str.Replace("||", "");
//                str = str.Replace("[", "");
//                str = str.Replace("]", "");
//                str = str.Replace("&", "");
//                str = str.Replace("#", "");
//                str = str.Replace("/", "");
//                str = str.Replace("-", "");
//                str = str.Replace("|", "");
//                str = str.Replace("?", "");
//                str = str.Replace(">?", "");
//                str = str.Replace("?<", "");
//                str = str.Replace(" ", "");
//                return str;
//            }
//        }

//        #endregion

//    }
//}