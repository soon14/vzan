using BLL.MiniApp;
using BLL.MiniApp.Home;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.Home;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp.Conf;
using System.Configuration;
using Utility;
using System.Data;
using Utility.IO;
using MySql.Data.MySqlClient;
using Entity.MiniApp.Conf;
using System.Text;
using Newtonsoft.Json.Linq;
using BLL.MiniApp.User;
using System.Threading.Tasks;

namespace User.MiniApp.Controllers
{
    public partial class dzhomeController : baseController
    {
        //修改密码发送验证码缓存
        
        private Return_Msg _msg = new Return_Msg();
        
        //private CustomModelUserRelationBLL _customModelUserRelationBLL = new CustomModelUserRelationBLL();
        //记得要在配置文件配置，要不然会报异常
        //可以试用的小程序类型
        private string _testxcxtype = ConfigurationManager.AppSettings["testxcxtype"].ToString();
        //可以免费使用的小程序类型
        private string _freexcxtype = ConfigurationManager.AppSettings["freexcxtype"].ToString();

        [CheckLoginMethod("Home")]
        public ActionResult Home()
        {
            return Redirect("/dzhome/newHome");
        }

        public ActionResult PhoneReg()
        {
            return View();
        }

        [CheckLoginMethod("productCenter")]
        public ActionResult productCenter()
        {
            return View();
        }
        /// <summary>
        /// 小程序单页版
        /// </summary>
        /// <returns></returns>
        public ActionResult productPage()
        {
            List<XcxTemplate> xcxTemplateList = GetTemplateModel(4);
            return View(xcxTemplateList);
        }
        /// <summary>
        /// 专业版
        /// </summary>
        /// <returns></returns>
        public ActionResult productMajor()
        {
            List<XcxTemplate> xcxTemplateList = GetTemplateModel(37);
            return View(xcxTemplateList);

        }
        /// <summary>
        /// 电商版
        /// </summary>
        /// <returns></returns>
        public ActionResult productEc()
        {
            List<XcxTemplate> xcxTemplateList = GetTemplateModel(6);
            return View(xcxTemplateList);
        }
        /// <summary>
        /// 行业版
        /// </summary>
        /// <returns></returns>
        public ActionResult productBasis()
        {
            List<XcxTemplate> xcxTemplateList = GetTemplateModel(27);
            return View(xcxTemplateList);
        }
        /// <summary>
        /// 企业版
        /// </summary>
        /// <returns></returns>
        public ActionResult productFirm()
        {
            List<XcxTemplate> xcxTemplateList = GetTemplateModel(2);
            return View(xcxTemplateList);
        }

        /// <summary>
        /// 小程序商店
        /// </summary>
        /// <returns></returns>
        [CheckLoginMethod("solution")]
        public ActionResult miniAppShop()
        {
            return View();
        }
        /// <summary>
        /// 餐饮版
        /// </summary>
        /// <returns></returns>
        public ActionResult productCatering()
        {
            List<XcxTemplate> xcxTemplateList = GetTemplateModel(18);
            return View(xcxTemplateList);
        }
        public ActionResult more_detail(int id)
        {
            NewsGw model = NewsGwBLL.SingleModel.GetModel(id);
            if (model == null)
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            return View(model);
        }
        /// <summary>
        /// 小程序百科
        /// </summary>
        /// <returns></returns>
        public ActionResult programWiki()
        {
            int newsId = Context.GetRequestInt("newsId", 0);
            int menuId = Context.GetRequestInt("menuId", 0);
            string showDetail = Context.GetRequest("showDetail", "false");
            List<Homenews> hotNewsList =HomenewsBLL.SingleModel.GetList($"state=1 and type={(int)newsType.miniAPP} and ishot=1", 4, 1, "*", "sort desc,id desc");
            List<Homenews> commonNewsList = HomenewsBLL.SingleModel.GetList($"state=1 and type={(int)newsType.miniAPP} and iscommon=1", 3, 1, "*", "sort desc,id desc");
            List<Homebkmenu> menuList = HomebkmenuBLL.SingleModel.GetList($" type={(int)MenuType.miniapp} and parentId=0");
            //热门问题
            ViewBag.hotNewsList = hotNewsList;
            //常见问题
            ViewBag.commonNewsList = commonNewsList;
            //菜单栏
            ViewBag.menuList = menuList;
            //内容页or列表页
            ViewBag.showDetail = showDetail == "true";
            ViewBag.newsId = newsId;
            ViewBag.menuId = menuId;
            ViewBag.menuFirstName = "小程序百科";
            return View();
        }

        public ActionResult News()
        {
            return View();
        }
        public ActionResult productFeature()
        {
            return View();
        }

        [CheckLoginMethod("solution")]
        public ActionResult about()
        {
            return View();
        }

        #region 登陆注册
        public ActionResult WXReg()
        {
            #region 保存返回地址
            System.Web.HttpCookie BzBackUrl = new HttpCookie("backurl");
            BzBackUrl.Value = ViewBag.backurl;
            BzBackUrl.Expires = DateTime.Now.AddMinutes(5);
            System.Web.HttpContext.Current.Response.Cookies.Add(BzBackUrl);
            #endregion
            string whichpage = Context.GetRequest("wp", "");
            if (!string.IsNullOrEmpty(whichpage))
            {
                ViewBag.WhichPage = whichpage;
            }
            else
            {
                ViewBag.WhichPage = "IdxPage";
            }
            //扫码登陆代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }
            ViewBag.keyword =HomekeywordBLL.SingleModel.GetKeyWord();
            return View();
        }

        public ActionResult Login()
        {
            int method = Context.GetRequestInt("method", 0);
            CookieHelper.Remove("agent_UserCookieNew");
            CookieHelper.Remove("dz_UserCookieNew");
            string accountid = Context.GetRequest("accountid", string.Empty);
            //总后台跳转过来进入
            if (!string.IsNullOrEmpty(accountid))
            {
                string masterAuth = DESEncryptTools.DESEncrypt(accountid);
                ViewBag.MasterAuth = masterAuth;
                Account tempAccount = AccountBLL.SingleModel.GetModelByStringId(accountid);
                if(tempAccount==null)
                {
                    return View("PageError", new Return_Msg() { Msg = "该用户不存在", code = "500" });
                }
                ViewBag.AccountId = Utils.BuildCookie(tempAccount.Id, tempAccount.UpdateTime); //accountid;
                return View();
            }

            HomeViewModel model = new HomeViewModel();

            model.config = HomeConfigBLL.SingleModel.GetModelByCache(1);
            model.config.attr = JsonConvert.DeserializeObject<ConfigAttr>(model.config.config);
            if (model.config.attr == null)
            {
                model.config.attr = new ConfigAttr();
            }
            model.keywords =HomekeywordBLL.SingleModel.GetKeyWord();
            //扫码登陆代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey);
            }
            ViewBag.Method = method;
            return View(model);
        }
        public ActionResult Login2()
        {
            string accountid = Context.GetRequest("accountid", string.Empty);
            Account account = new Account() { LoginId = accountid };
            return View(account);
        }

        public ActionResult Findpsw()
        {
            return View();
        }

        //检查用户是否已经使用二维码登录
        [HttpPost]
        public ActionResult WXLogin(string wxkey)
        {
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            int usertype = Context.GetRequestInt("usertype", 0);
            string domain = WebConfigBLL.CookieDomain;

            if (string.IsNullOrEmpty(wxkey))
            {
                return Json(new { success = false, msg = "-1" });
            }
            if (lcode == null)
            {
                return Json(new { success = false, msg = "-2" });
            }
            if (!lcode.IsLogin)
            {
                return Json(new { success = false, msg = "-3" + wxkey + "_" + usertype });
            }

            Account accountmodel = null;
            if (lcode.WxUser != null)
            {
                if (!string.IsNullOrEmpty(lcode.WxUser.openid))
                {
                    
                    //  log4net.LogHelper.WriteInfo(this.GetType(), $"MiniappUserBaseInfo ");
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

                    accountmodel = AccountBLL.SingleModel.GetAccountByWeixinUser(lcode.WxUser, usertype);
                    if (accountmodel == null)
                    {
                        return Json(new { success = false, msg = "-4" });
                    }
                    //判断新普通用户是否已绑定手机号码
                    //if(!accountmodel.Status)
                    //{
                    //    //用来绑定手机号时查询该用户
                    //    //SetCookie("regphoneuserid",domain, accountmodel.Id.ToString());
                    //    //CookieHelper.SetCookie("regphoneuserid", accountmodel.Id.ToString(), domain, 60 * 24);
                    //    //return Content("-2");
                    //    return Json(new { success = true, msg = accountmodel.Id.ToString() ,code=-2});
                    //}

                    
                    Member member = MemberBLL.SingleModel.GetModel(string.Format("AccountId ='{0}'", accountmodel.Id.ToString()));
                    member.LastModified = DateTime.Now;//记录登录时间
                    MemberBLL.SingleModel.Update(member);
                    RedisUtil.Remove("SessionID:" + wxkey);
                    string val = accountmodel.Id.ToString();
                    CookieHelper.Remove("agent_UserCookieNew");
                    CookieHelper.Remove("dz_UserCookieNew");
                    CookieHelper.Remove("regphoneuserid");
                    //SetCookie("dz_UserCookieNew", domain, val);
                    //CookieHelper.SetCookie("dz_UserCookieNew", val, domain, 60 * 24 * 10);
                }
            }
            else
            {
                return Json(new { success = false, msg = "-5" });
            }

            string key = Utils.BuildCookie(accountmodel.Id, accountmodel.UpdateTime);

            return Json(new { success = true, msg = key, authToken = DESEncryptTools.DESEncrypt(accountmodel.Id.ToString()) });
            //if (dzaccount == null)
            //{
            //    return Content("/dzhome/login");
            //}
            //if (string.IsNullOrEmpty(dzaccount.ConsigneePhone))
            //{
            //    return Content("/dzhome/PhoneReg");
            //}
            //return Content("/member/myworkbench");
            //return Content("/dzhome/caseTemplate");
            //return Content("/article/index");
            //ret new_url = "/article/index";
            //return View("SaveCookieRedirectView", new_url);

        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="back"></param>
        /// <returns></returns>
        public ActionResult Logout(string back, string userAccountId = "")
        {
            //判断是不是代理
            if (!string.IsNullOrEmpty(userAccountId))
            {
                Agentinfo agentModel = AgentinfoBLL.SingleModel.GetModelByAccoundId(userAccountId);
                if (agentModel != null)
                {
                    AgentinfoBLL.SingleModel.RemoveOutTime(agentModel.id);
                }
            }

            CookieHelper.Remove("agent_UserCookieNew");
            CookieHelper.Remove("dz_UserCookieNew");

            if (!string.IsNullOrEmpty(back))
            {
                return Redirect(back);
            }
            else
            {
                return Redirect("/dzhome/login");
                //return Redirect(System.Configuration.ConfigurationManager.AppSettings["MiniappContent"]);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="isKeep"></param>
        /// <param name="backurl"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DZLoginAjax(string userName, string passWord, bool isKeep)
        {
            //清除缓存
            CookieHelper.Remove("agent_UserCookieNew");
            CookieHelper.Remove("dz_UserCookieNew");
             
            if(Response!= null)
            {
                Response.ContentEncoding = System.Text.Encoding.UTF8;
            }

            //string cookiedomain = System.Configuration.ConfigurationManager.AppSettings["cookiedomain"];
            userName = StringHelper.NoHtml(userName.Trim());
            passWord = StringHelper.NoHtml(passWord);

            //新的登录方法
            Account account = AccountBLL.SingleModel.LoginUserWhole(userName, passWord);
           
            //用户名不存在
            if (account == null)
            {
                return Json(new { success = false, code = 2, msg = "密码错误" });
            }

            if (isKeep)//--保存本地用户名
            {
                CookieHelper.SetCookie("LoginUserName", HttpUtility.UrlEncode(userName));
            }
            else
            {
                CookieHelper.Remove("LoginUserName");
            }
            
            Member member = MemberBLL.SingleModel.GetModel(string.Format(" AccountId ='{0}'", account.Id.ToString()));
            member.LastModified = DateTime.Now;//记录登录时间
            MemberBLL.SingleModel.Update(member);

            string cookiedomain = WebConfigBLL.CookieDomain;
            if (Session != null)
            {
                Session["userName"] = userName;
                Session["passWord"] = passWord;
                Session["dzAccountId"] = account.Id.ToString();
            }

            string MasterAuth = DESEncryptTools.DESEncrypt(account.Id.ToString());

            //SetCookie("dz_UserCookieNew", cookiedomain, account.Id.ToString());
            //CookieHelper.SetCookie("dz_UserCookieNew", account.Id.ToString(), cookiedomain, 60 * 24 * 10);

            //return RedirectToAction("dzhome", "casetemplate");
            //return Json(new { success = true, msg = account.Id.ToString(), authToken = MasterAuth });
            return Json(new { success = true, msg = Utils.BuildCookie(account.Id,account.UpdateTime), authToken = MasterAuth });
        }

        /// <summary>
        /// 代理商进入自己小程序后台处理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GoToAgentBackground()
        {
            string cookiedomain = WebConfigBLL.CookieDomain;
            Return_Msg msg = new Return_Msg();
            if (agentaccount == null)
            {
                msg.isok = false;
                msg.Msg = "登陆过期，请重新登陆！";
            }

            SetCookie("dz_UserCookieNew", cookiedomain, Utils.BuildCookie(agentaccount.Id, dzaccount.UpdateTime));
            //CookieHelper.SetCookie("dz_UserCookieNew", agentaccount.Id.ToString(), ".vzan.com", 60 * 24 * 10);
            msg.isok = true;

            return Json(msg);
        }

        /// <summary>
        /// 发送验证码修改密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetVaildCode()
        {
            string phoneNum = Context.GetRequest("phonenum", "");
            //修改密码 type=0  注册type=1
            int type = Context.GetRequestInt("type", 0);

            Account account = AccountBLL.SingleModel.GetModelByPhone(phoneNum);

            //代理分销，判断是否已开通过代理，开通过代理就不给他开通
            int agentqrcodeid = Context.GetRequestInt("agentqrcodeid", 0);
            if (agentqrcodeid > 0 && account != null)
            {
                Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(account.Id.ToString(), -1);
                if (agentmodel != null)
                {
                    _msg.Msg = "该手机号已经绑定了代理商账号";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }
            }

            _msg = CommondHelper.GetVaildCode(agentqrcodeid, phoneNum, account, type);

            return Json(_msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 检验验证码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckVaildCode()
        {
            string phoneNum = Context.GetRequest("phonenum", "");
            string code = Context.GetRequest("code", "");

            _msg = CommondHelper.CheckVaildCode(phoneNum, code);

            return Json(_msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改密码或绑定手机号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveUserInfo()
        {
            string domain = WebConfigBLL.CookieDomain;
            string password = Context.GetRequest("password", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string code = Context.GetRequest("code", string.Empty);
            string accountid = dzuserId.ToString();
            int checkphone = Context.GetRequestInt("checkphone", 0);//是否校验手机号,0检验，1不校验
            //修改密码 type=0  注册type=1
            int type = Context.GetRequestInt("type", 0);
            if (checkphone == 0)
            {
                if (string.IsNullOrEmpty(phone) && checkphone == 0)
                {
                    _msg.isok = false;
                    _msg.Msg = "手机号码不能为空！";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(code))
                {
                    _msg.isok = false;
                    _msg.Msg = "验证码不能为空！";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }

                //校验验证码
                _msg = CommondHelper.CheckVaildCode(phone, code);

                if (!_msg.isok)
                {
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }
            }

            Account accountModel = dzaccount;
            //判断是否是普通用户进来注册绑定手机号
            string reguserid = CookieHelper.GetCookie("regphoneuserid");
            if (!string.IsNullOrEmpty(reguserid))
            {
                accountModel = AccountBLL.SingleModel.GetModel($"id='{reguserid}'");
                accountid = accountModel != null ? accountModel.Id.ToString() : "";
            }

            //找回密码不用判断是否已登录
            if (accountModel == null && type != 0)
            {
                _msg.isok = false;
                _msg.Msg = "亲，您还没有账号，赶紧注册一个！";
                return Json(_msg, JsonRequestBehavior.AllowGet);
            }
            if (type == 0)
            {
                if (string.IsNullOrEmpty(password))
                {
                    _msg.isok = false;
                    _msg.Msg = "密码不能为空！";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }

                accountModel = AccountBLL.SingleModel.GetModelByPhone(phone);

                if (accountModel == null)
                {
                    _msg.isok = false;
                    _msg.Msg = "亲，您手机号码还没绑定用户，注册一个呗！";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }

                accountModel.Password = DESEncryptTools.GetMd5Base32(password);
                accountModel.SyncStatus = "U";
                accountModel.UpdateTime = DateTime.Now;
                _msg.isok = AccountBLL.SingleModel.Update(accountModel, "Password,UpdateTime");

                if (!_msg.isok)
                {
                    _msg.Msg = "密码修改失败";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }
                _msg.Msg = "密码修改成功";
            }
            else if (type == 1)//手机号，密码绑定
            {
                //手机号码绑定
                if (checkphone == 0)
                {
                    _msg = AccountBLL.SingleModel.ChangePhone(accountid, 2, phone);
                    if (!_msg.isok)
                    {
                        return Json(_msg, JsonRequestBehavior.AllowGet);
                    }
                }
                //修改密码
                accountModel.Password = DESEncryptTools.GetMd5Base32(password);
                accountModel.SyncStatus = "U";
                accountModel.Status = true;
                accountModel.UpdateTime = DateTime.Now;
                _msg.isok = AccountBLL.SingleModel.Update(accountModel, "Password,Status,UpdateTime");

                if (!_msg.isok)
                {
                    _msg.Msg = "手机号绑定失败";
                }

                _msg.Msg = "手机号绑定成功";
                if (checkphone == 1 && _msg.isok)
                {
                    _msg.Msg = "修改密码成功";
                }
            }
            else if (type > 1)//手机号绑定
            {
                _msg = AccountBLL.SingleModel.ChangePhone(accountid, type, phone);
                if (!_msg.isok)
                {
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }
                _msg.Msg = "手机号绑定成功";
            }

            //普通用户注册绑定手机号后保存用户ID，用来直接跳转到工作台
            if (!string.IsNullOrEmpty(reguserid))
            {
                CookieHelper.Remove("agent_UserCookieNew");
                CookieHelper.Remove("dz_UserCookieNew");
                CookieHelper.Remove("regphoneuserid");

                SetCookie("dz_UserCookieNew", domain, reguserid);

                //CookieHelper.SetCookie("dz_UserCookieNew", reguserid, domain, 60 * 24 * 10);
            }

            string key = string.Format(CommondHelper._resetPasswordkey, phone);
            RedisUtil.Remove(key);
            return Json(_msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取登陆用户基本信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserCookie(int pageType = 0)
        {
            int aid = Context.GetRequestInt("aid", 0);
            ViewModelMyWorkbench model = GetUserInfo(aid, pageType);
            _msg = new Return_Msg();
            _msg.dataObj = model;
            _msg.isok = true;
            return Json(_msg, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CommandCookie()
        {
            string key = Context.GetRequest("key", string.Empty);
            int agentcustomerid = Context.GetRequestInt("agentcustomerid", 0);
            string MasterAuth = string.Empty;
            string kvalue = "";
            string msg = "";
            string cookieName = "";

            Account account = AccountBLL.SingleModel.GetModel(dzuserId);
            bool isok = true;
            switch (key)
            {
                case "agent_UserCookieNew":
                    //判断是不是代理
                    Agentinfo cookieagentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(dzuserId.ToString());
                    if (cookieagentinfo != null)
                    {
                        if (agentinfo == null)
                        {
                            AgentinfoBLL.SingleModel.SavePassword(cookieagentinfo.id, "");
                        }

                        Session["AccountId"] = dzuserId.ToString();
                        Account agentAccountInfo = AccountBLL.SingleModel.GetAccountByCache(cookieagentinfo.useraccountid);
                        kvalue = Utils.BuildCookie(Guid.Parse(cookieagentinfo.useraccountid), agentAccountInfo.UpdateTime); //cookieagentinfo.useraccountid;
                        cookieName = "agent_UserCookieNew";
                    }
                    else
                    {
                        msg = "您还不是代理商";
                        isok = false;
                    }
                    break;
                case "dz_UserCookieNew":
                    if (agentinfo == null)
                    {
                        msg = "您还不是代理商";
                        isok = false;
                        return Json(new { msg, isok });
                    }
                    AgentCustomerRelation customer = AgentCustomerRelationBLL.SingleModel.GetModelByIdAndAgentId(agentcustomerid, agentinfo.id);
                    if (customer == null)
                    {
                        msg = "用户不存在或已停用";
                        isok = false;
                        return Json(new { msg, isok });
                    }
                    cookieName = "dz_UserCookieNew";
                    Account dzAccountInfo = AccountBLL.SingleModel.GetAccountByCache(customer.useraccountid);
                    kvalue = Utils.BuildCookie(Guid.Parse(customer.useraccountid), dzAccountInfo.UpdateTime); //customer.useraccountid;
                    MasterAuth = DESEncryptTools.DESEncrypt(customer.useraccountid);
                    break;
                case "agentdz_UserCookieNew":
                    if (agentinfo == null)
                    {
                        msg = "登陆过期，请重新登陆";
                        isok = false;
                        return Json(new { msg, isok });
                    }
                    cookieName = "dz_UserCookieNew";
                    Account agentdzAccountInfo = AccountBLL.SingleModel.GetAccountByCache(agentinfo.useraccountid);
                    kvalue = Utils.BuildCookie(Guid.Parse(agentinfo.useraccountid), agentdzAccountInfo.UpdateTime);//agentinfo.useraccountid ;
                    MasterAuth = DESEncryptTools.DESEncrypt(agentinfo.useraccountid);
                    break;
            }

            return Json(new { key = key, kvalue = kvalue, auth = MasterAuth,  msg = msg, isok = isok, cookiename = cookieName }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// 产品案例
        /// </summary>
        /// <returns></returns>
        [CheckLoginMethod("solution")]
        public ActionResult ProductCase()
        {

            List<XcxTemplate> xcxTemplates = XcxTemplateBLL.SingleModel.GetListByIdsandProjectType((int)ProjectType.小程序);

            return View(xcxTemplates);
        }

        /// <summary>
        /// 获取产品案例
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCaseList()
        {
            int tid = Context.GetRequestInt("id", 0);
            string tagIdstr = Context.GetRequest("tagId", "0");
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 8);
            string sqlwhere = $"type={(int)caseType.xcxcase} and state>0 ";
            string tagSqlwhere = $"state>=0 and agentid=0 ";
            if (tid > 0)
            {
                sqlwhere += $" and casetype ={tid}";
                tagSqlwhere += $" and tid={tid}";
            }

            if (tagIdstr != "0")
            {
                if (!string.IsNullOrEmpty(tagIdstr))
                {
                    List<string> tagIds = tagIdstr.SplitStr(",");
                    if (tagIds.Count > 0)
                    {
                        tagIds = tagIds.Select(p => p = "FIND_IN_SET('" + p + "',tagids)").ToList();
                        sqlwhere += $" and (" + string.Join(" or ", tagIds) + ")";
                    }
                }
            }
            List<Miniapptag> tagList = new List<Miniapptag>();
            List<Miniapptag> tags = MiniapptagBLL.SingleModel.GetList(tagSqlwhere);
            List<object> objList = new List<object>();
            if (tags != null && tags.Count > 0)
            {
                List<string> names = new List<string>();
                names = tags.Select(t => t.tagname).Distinct().ToList();
                foreach (string name in names)
                {
                    List<Miniapptag> taglist = tags.Where(t => t.tagname == name).ToList();
                    var obj = new
                    {
                        id = string.Join(",", taglist.Select(t => t.id)),
                        tagname = taglist[0].tagname
                    };
                    objList.Add(obj);
                }
            }
            List<Homecase> cases = HomecaseBLL.SingleModel.GetList(sqlwhere, pageSize, pageIndex, "*", "sort desc,id desc");
            if (cases != null && cases.Count > 0)
            {
                cases.ForEach(c =>
                {
                    c.coverPath = Utility.ImgHelper.ResizeImg(c.coverPath, 700, 700);
                    c.QrcodePath = Utility.ImgHelper.ResizeImg(c.QrcodePath, 700, 700);
                });
            }
            int recordCount = HomecaseBLL.SingleModel.GetCount(sqlwhere);

            return Json(new { isok = true, cases = cases, tags = objList, recordCount = recordCount }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 获取小程序模板信息
        /// </summary>
        /// <param name="id">模板id</param>
        /// <returns></returns>
        private List<XcxTemplate> GetTemplateModel(int id)
        {
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={id}");
            List<XcxTemplate> xcxTemplateList = null;
            if (xcxTemplate != null)
            {
                xcxTemplateList = new List<XcxTemplate>();
                xcxTemplateList.Add(xcxTemplate);
            }
            return xcxTemplateList;
        }

        /// <summary>
        /// 获取百科新闻
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBkNewsList()
        {
            int pageindex = Context.GetRequestInt("pageindex", 1);
            int pagesize = Context.GetRequestInt("pagesize", 10);
            int menuId = Context.GetRequestInt("menuId", 0);
            string title = Context.GetRequest("title", string.Empty);
            string sqlwhere = $"type ={ (int)newsType.miniAPP} and state = 1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (menuId > 0)
            {
                sqlwhere += $" and childnode={menuId}";
            }
            if (!string.IsNullOrEmpty(title))
            {
                sqlwhere += $" and title like @title";
                parameters.Add(new MySqlParameter("@title", $"%{title}%"));
            }
            List<Homenews> list = HomenewsBLL.SingleModel.GetListByParam(sqlwhere, parameters.ToArray(), pagesize, pageindex, "*", "sort desc,addtime desc");
            int recordCount = HomenewsBLL.SingleModel.GetCount(sqlwhere, parameters.ToArray());
            List<object> objlist = null;
            if (list != null && list.Count > 0)
            {
                objlist = new List<object>();
                foreach (Homenews news in list)
                {

                    string[] taglist;
                    if (!string.IsNullOrEmpty(news.tags))
                    {
                        taglist = news.tags.Split(',');
                    }
                    else
                    {
                        taglist = null;
                    }
                    object obj = new
                    {
                        taglist = taglist,
                        id = news.Id,
                        title = news.title,
                        imgUrl = news.ImgPath,
                        Description = news.Description
                    };
                    objlist.Add(obj);
                }
            }
            _msg.isok = true;
            _msg.dataObj = objlist;
            return Json(new { isok = true, newsList = objlist, recordCount = recordCount }, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 新闻资讯或者深度观点列表
        /// </summary>
        /// <param name="Type">0→资讯  1 →深度观点  2拼享惠资讯</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult NewsList(int Type = 0, int pageIndex = 1, int pageSize = 20)
        {
            Return_Msg returnData = new Return_Msg();

            int count = 0;
            List<NewsGw> list = NewsGwBLL.SingleModel.GetListByType(Type, pageSize, pageIndex, ref count);
            returnData.dataObj = new { list = list, count = count };
            returnData.isok = true;

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkNews()
        {
            int id = Context.GetRequestInt("newsId", 0);
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            Homenews news = HomenewsBLL.SingleModel.GetModel($"state=1 and id={id} and type ={ (int)newsType.miniAPP}");
            if (news == null)
            {
                return Json(new { isok = false, msg = "我是不存在的 →_→" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { isok = true, news = news }, JsonRequestBehavior.AllowGet);
        }
        
        ///// <summary>
        ///// 移动端：代理招商
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult mAgent()
        //{
        //    return View();
        //}
        ///// <summary>
        ///// 移动端：免费领取小程序
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult freeTrial()
        //{
        //    return View();
        //}   
        ///// <summary>
        ///// 移动端：首页
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult mHome()
        //{
        //    return View();
        //}

        /// <summary>
        /// 获取多个模板的产品案例
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCaseListByTemplate()
        {
            string tids = Context.GetRequest("tids", string.Empty);
            int pageSize = Context.GetRequestInt("pageSize", 3);
            string sqlstr = $"";
            string sql = string.Empty;
            List<Homecase> cases = null;
            if (string.IsNullOrEmpty(tids))
            {
                return Json(new { isok = true, cases = cases }, JsonRequestBehavior.AllowGet);
            }
            List<string> tidList = tids.Split(',').ToList();
            if (tidList == null || tidList.Count <= 0)
            {
                return Json(new { isok = true, cases = cases }, JsonRequestBehavior.AllowGet);
            }
            foreach (string tid in tidList)
            {
                sql += $"union (select * from homecase where type={(int)caseType.xcxcase} and state>0 and casetype={tid} LIMIT 0, {pageSize})";
            }
            sql = sql.TrimStart("union".ToArray());
            DataSet ds = SqlMySql.ExecuteDataSet(dbEnum.QLWL.ToString(), CommandType.Text, sql);
            if (ds.Tables.Count <= 0)
                return Json(new { isok = true, cases = cases }, JsonRequestBehavior.AllowGet);
            DataTable dt = ds.Tables[0]; if (dt == null || dt.Rows.Count <= 0)
                return Json(new { isok = true, cases = cases }, JsonRequestBehavior.AllowGet);
            cases = new List<Homecase>();
            foreach (DataRow row in dt.Rows)
            {
                Homecase homeCase = new Homecase();
                if (row["coverPath"] != DBNull.Value)
                {
                    homeCase.coverPath = ImgHelper.ResizeImg(row["coverPath"].ToString(), 700, 700);
                }
                if (row["qrcodepath"] != DBNull.Value)
                {
                    homeCase.QrcodePath = ImgHelper.ResizeImg(row["qrcodepath"].ToString(), 700, 700);
                }
                if (row["casetype"] != DBNull.Value)
                {
                    homeCase.casetype = Convert.ToInt32(row["casetype"]);
                }
                cases.Add(homeCase);
            }
            List<object> objList = new List<object>();
            foreach (string tid in tidList)
            {
                List<Homecase> clist = cases.Where(c => c.casetype == Convert.ToInt32(tid)).ToList();
                var obj = new
                {
                    tid = tid,
                    clist = clist
                };
                objList.Add(obj);
            }
            return Json(new { isok = true, list = objList }, JsonRequestBehavior.AllowGet);
        }

        [CheckLoginMethod("solution")]
        public ActionResult templateMarket()
        {
            List<XcxTemplate> xcxTemplates = XcxTemplateBLL.SingleModel.GetListByIdsandProjectType((int)ProjectType.小程序);
            return View(xcxTemplates);
        }

        [CheckLoginMethod("solution")]
        public ActionResult solution()
        {
            List<XcxTemplate> xcxTemplates = XcxTemplateBLL.SingleModel.GetListByIdsandProjectType((int)ProjectType.小程序);
            return View(xcxTemplates);
        }

        [CheckLoginMethod("solution")]
        public ActionResult newHome()
        {
            //扫码登陆代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            //  log4net.LogHelper.WriteInfo(this.GetType(), sessonid);
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey, TimeSpan.FromDays(1));
            }

            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            string[] domianArry = WebSiteConfig.DzWebSiteDomain.Split(';');
            if (domian == "jia.xiaochengxu.com.cn")
            {
                return Redirect("/dzhome/programWiki?menuId=0");
            }
            if (!domianArry.Contains(domian)) // if (domian != WebSiteConfig.DzWebSiteDomain && domian != WebSiteConfig.DzWebSiteDomain2) 
            {
                return Redirect("/dzhome/indexDl");
            }

            List<XcxTemplate> xcxTemplates = XcxTemplateBLL.SingleModel.GetListByIdsandProjectType((int)ProjectType.小程序);
            return View(xcxTemplates);
        }
        
        //代理商授权证书
        public ActionResult authorization()
        {
            List<XcxTemplate> xcxTemplates = XcxTemplateBLL.SingleModel.GetListByIdsandProjectType((int)ProjectType.小程序);
            return View(xcxTemplates);
        }

        #region 大树视频
        /// <summary>
        /// 输入密码验证是否可以观看加密视频
        /// </summary>
        /// <returns></returns>
        public ActionResult CanSeeVideo()
        {
            string password = Context.GetRequest("password", string.Empty);
            int videoid = Context.GetRequestInt("videoid", 0);
            int id = Context.GetRequestInt("id", 0);

            if (string.IsNullOrEmpty(password))
            {
                return Json(new { isok = false, msg = "请输入密码" }, JsonRequestBehavior.AllowGet);
            }

            if (id <= 0)
            {
                return Json(new { isok = false, msg = "请输入密码" }, JsonRequestBehavior.AllowGet);
            }

            #region Base64解密
            password = password.Replace(" ", "+");
            byte[] bytes = Convert.FromBase64String(password);
            password = Encoding.UTF8.GetString(bytes);
            #endregion

            VideoPlayList model =VideoPlaylistBLL.SingleModel.GetModel(id);

            if (model == null)
            {
                return Json(new { isok = false, msg = "密码错误" }, JsonRequestBehavior.AllowGet);
            }

            if (model.Password != password)
            {
                return Json(new { isok = false, msg = "密码错误" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = true, msg = "密码正确" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取播放视频数据集合
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVideoPlayList()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int typeid = Context.GetRequestInt("typeid", -1);
            int themeid = Context.GetRequestInt("themeid", 0);
            int orderbytype = Context.GetRequestInt("orderbytype", 0);//1：热门，2：时间
            int orderby = Context.GetRequestInt("orderby", -1);//0：升序，1：降序

            List<VideoPlayList> playlist =VideoPlaylistBLL.SingleModel.GetVideoPlaylist(pageIndex, pageSize, typeid, orderby, themeid, orderbytype);
            try
            {
                if (playlist != null && playlist.Count > 0)
                {
                    foreach (VideoPlayList item in playlist)
                    {
                        item.IsPassword = !string.IsNullOrEmpty(item.Password);
                        item.Password = "";
                    }

                    playlist = GetPlayCount(playlist, orderbytype);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }

            return Json(new { isok = true, msg = "成功", data = playlist }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取播放视频主题数据集合
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVideoPlayThemeList()
        {
            Return_Msg returnobj = new Return_Msg();
            List<VideoPlayTheme> themelist = VideoPlayThemeBLL.SingleModel.GetDataList();
            returnobj.dataObj = themelist;

            return Json(returnobj);
        }

        private List<VideoPlayList> GetPlayCount(List<VideoPlayList> playlist, int orderbytype)
        {
            if (playlist != null && playlist.Count > 0)
            {
                string tpids = string.Join(",", playlist.Where(w => w.zbid > 0)?.Select(s => s.zbid));
                object data = new { tpid = tpids };
                string url = $"{WebConfigBLL.ZBtopapi}?tpid={tpids}";
                string dataJson = JsonConvert.SerializeObject(data);
                string result = HttpHelper.DoPostJson(url, dataJson);//WxHelper.HttpGet(url);
                if (!string.IsNullOrEmpty(result))
                {
                    //log4net.LogHelper.WriteInfo(this.GetType(),result);

                    VideoParam rmodel = JsonConvert.DeserializeObject<VideoParam>(result);
                    if (rmodel.isok && rmodel.dataObj != null && rmodel.dataObj.Count > 0)
                    {
                        foreach (VideoPlayList oitem in playlist)
                        {
                            TpcData model = rmodel.dataObj.Where(w => w.tpid == oitem.zbid).FirstOrDefault();
                            if (model != null)
                            {
                                int viewcts = model.viewcts;
                                oitem.playcount += viewcts;
                            }
                        }
                    }
                }

                //按热门排序，因为播放量是从直播接口获取，所以不能通过sql语句排序
                if (orderbytype == 1)
                {
                    return playlist.OrderByDescending(o => o.playcount).ToList();
                }
            }

            return playlist;
        }

        //视频播放页
        public ActionResult VideoDetails()
        {
            int id = Context.GetRequestInt("id", 0);
            VideoPlayList playmodel =VideoPlaylistBLL.SingleModel.GetModel(id);
            if (playmodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "该视频不存在", code = "500" });
            }
            playmodel.playcount += 1;
            playmodel.IsPassword = !string.IsNullOrEmpty(playmodel.Password);
            playmodel.Password = "";
            VideoPlaylistBLL.SingleModel.Update(playmodel, "playcount");
            List<VideoPlayList> playlist = new List<VideoPlayList>() { playmodel };
            try
            {
                playlist = GetPlayCount(playlist, 0);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }


            return View(playlist[0]);
        }

        //视频列表页
        public ActionResult VideoList()
        {
            return View();
        }
        #endregion

        //代理商建站首页
        public ActionResult indexDl()
        {
            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            if (string.IsNullOrEmpty(domian))
            {
                return View("PageError", new Return_Msg() { Msg = "请求异常(域名不合法)!", code = "500" });
            }

            if (domian.Contains(WebSiteConfig.DzWebSiteDomainExt))
            {
                //表示二级域名
                domian = domian.Replace(WebSiteConfig.DzWebSiteDomainExt, "");
            }
            if (domian.Contains("xn--"))//表示中文域名,会进行punycode编码 例如:www.九美.com   System.Web.HttpContext.Current.Request.Url.Host;实际拿到的是 www.xn--sjq221j.com
            {
                System.Globalization.IdnMapping idn = new System.Globalization.IdnMapping();

                domian= idn.GetUnicode(domian);
            }


            AgentWebSiteInfo agentWebSiteInfo = AgentWebSiteInfoBLL.SingleModel.GetModelByDomian(domian);
            if (agentWebSiteInfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "请求异常(没有数据)!", code = "500" });
            }
            Agentinfo webAgentInfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(agentWebSiteInfo.userAccountId);
            if(webAgentInfo==null)
            {
                return View("PageError", new Return_Msg() { Msg = "无效代理!", code = "403" });
            }
            //if (webAgentInfo.OutTime<DateTime.Now)


            //{
            //    return View("PageError", new Return_Msg() { Msg = "代理合同已失效，请续签!", code = "403" });
            //}
            if (agentWebSiteInfo.webState == -1)
            {
                return View("PageError", new Return_Msg() { Msg = "网站已被停用(请联系客户)!", code = "403" });
            }

            if (!string.IsNullOrEmpty(agentWebSiteInfo.seoConfig))
            {
                agentWebSiteInfo.seoConfigModel = JsonConvert.DeserializeObject<SeoConfigModel>(agentWebSiteInfo.seoConfig);
            }
            if (!string.IsNullOrEmpty(agentWebSiteInfo.pageMsgConfig))
            {
                agentWebSiteInfo.pageMsgConfigModel = JsonConvert.DeserializeObject<PageMsgConfigModel>(agentWebSiteInfo.pageMsgConfig);
            }
            
            return View(agentWebSiteInfo);
        }
        //代理加盟
        public ActionResult agent()
        {
            return View();
        }
        //新零售足浴
        public ActionResult retail()
        {
            return View();
        }
        //独立域名
        public ActionResult independentDomain()
        {
            return View();
        }
        //新零售拼享惠
        public ActionResult pinxianghui()
        {
            return View();
        }

        #region 微信小程序关键词
        public ActionResult KeyWord()
        {
            return View();
        }
        /// <summary>
        /// 关键词类型
        /// </summary>
        /// <returns></returns>
        public ActionResult GetKeyWordTypeList()
        {
            Return_Msg returnData = new Return_Msg();

            List<KeyWordType> list = KeyWordTypeBLL.SingleModel.GetListByParentId(0,0);

            returnData.dataObj = list;
            returnData.isok = true;

            return Json(returnData);
        }

        /// <summary>
        /// 关键词集合
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetKeyWordList(int typeId=-999,int pageIndex=1,int pageSize=10,string name="")
        {
            Return_Msg returnData = new Return_Msg();

            int count = 0;
            List<KeyWord> list = KeyWordBLL.SingleModel.GetListByCache(typeId,pageIndex,pageSize,name,ref count);

            returnData.dataObj = new { list=list,count=count};
            returnData.isok = true;

            return Json(returnData);
        }

        /// <summary>
        /// 申购关键词
        /// </summary>
        /// <param name="id"></param>
        /// <param name="phone"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ApplyKeyWord(int id=0,string phone="",decimal price=0)
        {
            Return_Msg returnData = new Return_Msg();

            if(id<=0)
            {
                returnData.Msg = "参数错误";
                return Json(returnData);
            }
            if(price <= 0)
            {
                returnData.Msg = "无效价格";
                return Json(returnData);
            }
            if(string.IsNullOrEmpty(phone))
            {
                returnData.Msg = "请输入手机号";
                return Json(returnData);
            }

            ApplyKeyWord model = new ApplyKeyWord();
            model.Phone = phone;
            model.Price = price;
            model.KeyWordId = id;
            model.Id = Convert.ToInt32(ApplyKeyWordBLL.SingleModel.Add(model));

            returnData.isok = model.Id > 0;
            returnData.Msg = returnData.isok?"保存成功":"保存失败";

            return Json(returnData);
        }
        #endregion
    }
}