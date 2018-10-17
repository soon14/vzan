using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class CheckLoginAttribute : ActionFilterAttribute
    {
        public CheckLoginAttribute()
        { }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string AccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
        }

        private void ErrorRedirect(ActionExecutingContext filterContext)
        {
            filterContext.Result = new RedirectResult(WebSiteConfig.WsqAdminUrl + "?backUrl=" + HttpContext.Current.Request.Url);
            return;
        }
    }

    public class CheckLoginMethodAttribute : AuthorizeAttribute
    {
        private string action;

        public CheckLoginMethodAttribute(string action)
        {
            this.action = action;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string u = httpContext.Request.ServerVariables["HTTP_USER_AGENT"];
            if (string.IsNullOrEmpty(u))
            {
                return false;
            }

            Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if ((b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
            {
                return false;
            }
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            string[] domianArry = WebSiteConfig.DzWebSiteDomain.Split(';');
            if (!domianArry.Contains(domian)) // if (domian != WebSiteConfig.DzWebSiteDomain && domian != WebSiteConfig.DzWebSiteDomain2)
            {
                filterContext.Result = new RedirectResult("/mobile/indexDl");
                return;
            }
            else
            {
                filterContext.Result = new RedirectResult("/mobile/mobileHome");
                return;
            }
        }
    }


    public class baseController : Controller
    {
        #region private

        /// <summary>
        /// 查询用户身份
        /// </summary>
        protected ViewModelMyWorkbench GetUserInfo(int aid = 0, int pageType = 0)
        {
            ViewModelMyWorkbench model = new ViewModelMyWorkbench();
            try
            {
                if (pageType == -1)
                {
                    model._Member = MemberBLL.SingleModel.GetModel(string.Format("AccountId='{0}'", agentuserId));
                    model._Account = AccountBLL.SingleModel.GetModel(agentuserId);
                }
                else
                {
                    model._Member = MemberBLL.SingleModel.GetModel(string.Format("AccountId='{0}'", dzuserId));
                    model._Account = AccountBLL.SingleModel.GetModel(dzuserId);
                }

                if (model._Member != null)
                {
                    if (model._Account != null)
                    {

                        //获取用户是否有设置自定义水印
                        int bottomlogocount = ConfParamBLL.SingleModel.GetCustomConfigCount(model._Account.Id.ToString(), "'agentcustomlogo'");
                        model.BottomLogoCount = bottomlogocount;
                        if (aid > 0)
                        {
                            List<ConfParam> confparamlist = ConfParamBLL.SingleModel.GetListByRId(aid, "'agentsystemlogo'");
                            model.CustomLogo = confparamlist == null || confparamlist.Count <= 0 ? "" : confparamlist[0].Value;//"https://gss0.bdstatic.com/5bVWsj_p_tVS5dKfpU_Y_D3/res/r/image/2017-09-27/297f5edb1e984613083a2d3cc0c5bb36.png";
                        }
                        //判断该用户是否是代理商开发的
                        //XcxAppAccountRelation xcxmodel = XcxAppAccountRelationBLL.SingleModel.CheckAgentOpenCustomer(model._Account.Id.ToString());
                        List<AgentCustomerRelation> agentCustomerRelation = AgentCustomerRelationBLL.SingleModel.GetListByAccountId(model._Account.Id.ToString());
                        model.IsAgentCustomer = agentCustomerRelation != null && agentCustomerRelation.Count > 0;
                        if (!model.IsAgentCustomer)
                        {
                            model.IsAgentCustomer = XcxAppAccountRelationBLL.SingleModel.EsitPlatChild(51, model._Account.Id.ToString());
                        }
                        model.IsDistributioner = agentCustomerRelation.Where(w => w.QrcodeId > 0 && w.OpenExtension >= 0)?.ToList().Count > 0;
                        //代理商信息
                        model.Agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(model._Account.Id.ToString());

                        if (model.Agentinfo != null)
                        {
                            model.Agentinfo.IsAgentDistribution = new AgentDistributionRelationBLL().GetCountByAgentId(model.Agentinfo.id) > 0;
                            if (agentinfo != null)
                            {
                                if (agentinfo.id != model.Agentinfo.id)
                                {
                                    model.Agentinfo = null;
                                }
                            }
                        }

                        //校验用户身份
                        //if (model._Account != null)
                        //{
                        //    //判断手机号是否加密，加密则更新为不加密
                        //    if (model._Account.ConsigneePhone.Length > 11)
                        //    {
                        //        model._Account.ConsigneePhone = DESEncryptTools.GetMd5Base32(model._Account.ConsigneePhone);
                        //        AccountBLL.SingleModel.Update(model._Account, "ConsigneePhone");
                        //    }

                        //    if (!string.IsNullOrEmpty(model._Account.ConsigneePhone))
                        //    {
                        //        ViewBag.Phone = model._Account.ConsigneePhone;
                        //    }
                        //}
                    }
                }
            }
            catch (Exception)
            {
            }

            return model;
        }

        /*登陆用户Id*/

        protected Guid agentuserId
        {
            get
            {
                Guid accountId = Guid.Empty;
                string agentCookie = CookieHelper.GetCookie("agent_UserCookieNew");

                if (!string.IsNullOrEmpty(agentCookie))
                    agentCookie = DESEncryptTools.DESDecrypt(agentCookie);

                if (!string.IsNullOrEmpty(agentCookie))
                {
                    List<string> kv = agentCookie.SplitStr(@"\r\n");
                    if (kv.Count == 2 && !string.IsNullOrEmpty(kv[0]) && !string.IsNullOrEmpty(kv[1]))
                    {
                        Agentinfo agentInfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(kv[0]);
                        Account account = AccountBLL.SingleModel.GetAccountByCache(kv[0]);
                        if (agentInfo != null && account.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss") == kv[1])
                        {
                            return Guid.Parse(kv[0]);
                        }
                    }
                }
                return Guid.Empty;
            }
        }

        protected Guid dzuserId
        {
            get
            {
                Guid accountId = Guid.Empty;
                string userCookie = CookieHelper.GetCookie("dz_UserCookieNew");
                if (!string.IsNullOrEmpty(userCookie))
                    userCookie = DESEncryptTools.DESDecrypt(userCookie);
                if (!string.IsNullOrEmpty(userCookie))
                {
                    List<string> kv = userCookie.SplitStr(@"\r\n");
                    if (kv.Count == 2 && !string.IsNullOrEmpty(kv[0]) && !string.IsNullOrEmpty(kv[1]))
                    {
                        Account account = AccountBLL.SingleModel.GetAccountByCache(kv[0]);
                        if (account != null && account.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss") == kv[1])
                        {
                            return Guid.Parse(kv[0]);
                        }
                        else
                        {
                            RedisUtil.Remove(kv[0]);
                        }
                    }
                }
                return Guid.Empty;
            }
        }

        /// <summary>
        /// 多门店登录UserId
        /// </summary>
        protected Int32 multiStoreUserId
        {
            get
            {
                Int32 multiStoreAccountId = 0;
                Int32.TryParse(CookieHelper.GetCookie("dz_MultiStoreUserCookie"), out multiStoreAccountId);

                return multiStoreAccountId;
            }
        }

        /// <summary>
        /// 多门店登录User的Account
        /// </summary>
        protected Guid multiStoreUserAccountId
        {
            get
            {
                if (!Guid.Empty.Equals(multiStoreAccount.AccountId))
                {
                    //当前登录分店用户的Account的Id
                    return multiStoreAccount.AccountId;
                }
                else
                {
                    string multiStoreUserAccountId = CookieHelper.GetCookie("dz_MultiStoreAccountUserCookie");
                    if (!string.IsNullOrEmpty(multiStoreUserAccountId))
                    {
                        return Guid.Parse(multiStoreUserAccountId);
                    }
                    else
                    {
                        return Guid.Empty;
                    }
                }
            }
        }

        protected bool CheckCityAuthor(int areaCode)
        {
            return true;
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {

            base.OnException(filterContext);
        }

        #endregion private

        public ActionResult PageError(int type)
        {
            Return_Msg result = new Return_Msg();
            switch (type)
            {
                case 1:
                    result.Msg = "授权验证：无登陆信息！";
                    result.code = "500";
                    if (Request.IsAjaxRequest())
                    {
                        return ReturnJson(isOk: result.isok, message: result.Msg, code: result.code);
                    }
                    return Redirect("/dzhome/login");
                case 2:
                    result.Msg = "授权验证：用户不存在！";
                    result.code = "500";
                    break;

                case 3:
                    result.Msg = "没有开通此模板！";
                    result.code = "500";
                    break;

                case 4:
                    result.Msg = "授权验证：您不是该店铺的管理员！";
                    result.code = "403";
                    break;

                case 5:
                    result.Msg = "授权验证：未开通本服务！";
                    result.code = "403";
                    break;

                case 6:
                    result.Msg = "找不到对应小程序模板！";
                    result.code = "500";
                    break;

                case 7:
                    result.Msg = "参数错误！";
                    result.code = "500";
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return ReturnJson(isOk: result.isok, message: result.Msg, code: result.code);
            }
            return View("pageerror", result);
        }

        public ActionResult PageErrorMsg(Return_Msg errorMsg)
        {
            return View("pageerror", errorMsg);
        }

        public string IsWeChat => CheckIsWx();

        public string CheckIsWx()
        {
            if (string.IsNullOrEmpty(Request.UserAgent))
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), "Request.UserAgent没值:" + Request.Url.ToString());
                return "1";
            }
            return Request.UserAgent.ToLower().Contains("micromessenger") ? "1" : "0";
        }

        public Account agentaccount
        {
            get
            {
                return AccountBLL.SingleModel.GetModel(agentuserId);
            }
        }

        public Account dzaccount
        {
            get
            {
                return AccountBLL.SingleModel.GetModel(dzuserId);
            }
        }

        /// <summary>
        /// 当前多门店用户
        /// </summary>
        public MultiStoreAccount multiStoreAccount
        {
            get
            {
                return MultiStoreAccountBLL.SingleModel.GetModel(multiStoreUserId);
            }
        }

        /// <summary>
        /// 当前多门店用户的Account信息
        /// </summary>
        public Account multiStore_Account
        {
            get
            {
                return AccountBLL.SingleModel.GetModel(multiStoreUserAccountId);
            }
        }

        public Agentinfo agentinfo
        {
            get
            {
                return AgentinfoBLL.SingleModel.GetModelByAccoundId(agentuserId.ToString());
            }
        }

        public void SetCookie(string name, string domain, string value)
        {
            string[] domains = domain.Split(';');
            if (domains != null && domains.Length > 0)
            {
                foreach (string item in domains)
                {
                    CookieHelper.SetCookie(name, value, item, 60 * 24);
                }
            }
        }

        public void InityyForm(int appId)
        {
            EntSetting page = EntSettingBLL.SingleModel.GetModel($"aid={appId} and pages like '%\"def_name\":\"产品预约\"%'");
            if (page == null)
            {
                page = EntSettingBLL.SingleModel.GetModel($"aid={appId}");
                if (page == null)
                {
                    page = new EntSetting();
                    string pagestr = "[{ \"skin\":1,\"name\":\"产品预约\",\"sel\":false,\"target\":\"_self\",\"coms\":[{\"type\":\"yyform\",\"name\":\"产品预约\",\"title\":\"\",\"items\":[],\"showname\":true,\"showphone\":true,\"showsex\":true,\"showage\":true,\"showremark\":true,\"showdress\":true,\"showmap\":false,\"showtime\":true,\"timecount\":10,\"timetype\":\"分钟\"}],\"selComIndex\":0,\"def_name\":\"产品预约\"}]";
                    page.pages = pagestr;
                    page.aid = appId;

                    EntSettingBLL.SingleModel.Add(page);
                }
                else
                {
                    string pagestr = ",{ \"skin\":1,\"name\":\"产品预约\",\"sel\":true,\"target\":\"_self\",\"coms\":[{\"type\":\"yyform\",\"name\":\"产品预约\",\"title\":\"\",\"items\":[],\"showname\":true,\"showphone\":true,\"showsex\":true,\"showage\":true,\"showremark\":true,\"showdress\":true,\"showmap\":false,\"showtime\":true,\"timecount\":10,\"timetype\":\"分钟\"}],\"selComIndex\":0,\"def_name\":\"产品预约\"}]";
                    if (string.IsNullOrEmpty(page.pages))
                    {
                        pagestr = "[{ \"skin\":1,\"name\":\"产品预约\",\"sel\":true,\"target\":\"_self\",\"coms\":[{\"type\":\"yyform\",\"name\":\"产品预约\",\"title\":\"\",\"items\":[],\"showname\":true,\"showphone\":true,\"showsex\":true,\"showage\":true,\"showremark\":true,\"showdress\":true,\"showmap\":false,\"showtime\":true,\"timecount\":10,\"timetype\":\"分钟\"}],\"selComIndex\":0,\"def_name\":\"产品预约\"}]";
                        page.pages = pagestr;
                    }
                    else
                    {
                        page.pages = page.pages.Substring(0, page.pages.Length - 1) + pagestr;
                    }
                    EntSettingBLL.SingleModel.Update(page);
                }
            }
        }

        /// <summary>
        /// 扫描微信二维码
        /// </summary>
        public string GetQrdCode()
        {
            //扫码登陆代码
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["qrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode wxkey = new LoginQrCode();
                wxkey.SessionId = sessonid;
                wxkey.IsLogin = false;
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, wxkey, TimeSpan.FromDays(1));
            }

            return sessonid;
        }

        public JsonResult ReturnJson(bool isOk = false, string message = null, string code = null, object data = null, bool allowGet = true)
        {
            Return_Msg jsonModel = new Return_Msg
            {
                Msg = message,
                code = code,
                dataObj = data,
                isok = isOk
            };
            return new JsonResultFormat() { Data = jsonModel, JsonRequestBehavior = allowGet ? JsonRequestBehavior.AllowGet : JsonRequestBehavior.DenyGet };
        }

#if DEBUG

        public ActionResult ClearRedis(string key)
        {
            bool isSuccess = CommonSettingBLL.RemoveRedis(key);
            return Json(new { isSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
        }

#endif

        public ActionResult IsHaveNewOrder()
        {
            int aid = Context.GetRequestInt("aid", 0);
            Return_Msg returndata = new Return_Msg();
            string voicepath = "";
            if (aid > 0)
            {
                string rediurl = "";
                int xcxtype = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(aid);

                int ordertype = 0;
                string content = "";
                //获取提示信息
                returndata.Msg = XcxAppAccountRelationBLL.SingleModel.IsHaveNewOrder(aid, ref content, ref ordertype);
                if (returndata.Msg.Length > 0)
                {
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                //获取跳转路径和提示音
                returndata.Msg = XcxAppAccountRelationBLL.SingleModel.CommandNewOrder(xcxtype, aid, ref rediurl, ref voicepath, ref ordertype);
                if (returndata.Msg.Length > 0)
                {
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                }
                returndata.isok = true;
                returndata.dataObj = new { voicepath = voicepath, redie_url = rediurl, content = content };
                //string havestr = Utils.IsHaveNewOrder(aid, 0);
                //if (!string.IsNullOrEmpty(havestr))
                //{
                //    string[] msgattr = havestr.Split('_');
                //    if(msgattr.Length>1)
                //    {
                //        switch (Convert.ToInt32(msgattr[1]))
                //        {
                //            case (int)EntGoodsType.普通产品: content = "您有一个新的订单，请及时处理"; break;
                //            case (int)EntGoodsType.预约商品: content = "您有一个新的产品预约单，请及时处理"; break;
                //        }
                //    }

                //    returndata.isok = true;
                //    returndata.dataObj =new { voicepath = voicepath ,redie_url= rediurl } ;
                //    Utils.RemoveIsHaveNewOrder(aid, 0, "");
                //}
                //else
                //{
                //    returndata.Msg = "没有新订单";
                //}
            }

            return Json(returndata,JsonRequestBehavior.AllowGet);
        }

        public ActionResult HaveNewOrder()
        {
            //string accountid = Context.GetRequest("accountid","");
            //Return_Msg returndata = new Return_Msg();

            //Utils.RemoveIsHaveNewOrder(accountid);
            //return Json(returndata);

            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                return Json(returndata);
            }

            Utils.RemoveIsHaveNewOrder(aid, 0);
            return Json(returndata);
        }

        /// <summary>
        /// 将百度地图的经纬度转换为腾讯地图经纬度
        /// 百度地图对应的 BD09 协议坐标，转到 中国正常坐标系GCJ02协议的坐标
        /// </summary>
        /// <param name="lat">维度</param>
        /// <param name="lng">经度</param>
        public ActionResult ConvertBaiduPointToTencent(string lnglat)
        {

            try
            {
                return Content(HttpHelper.GetData("http://api.map.baidu.com/geoconv/v1/", "coords=" + lnglat + "&from=5&to=3&ak=QGXVNnuXjEinccPzuAYPNq5wynQw3nT2"));
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                string[] arr = lnglat.Split(',');
                double lng = Convert.ToDouble(arr[0]);
                double lat = Convert.ToDouble(arr[1]);
                C_AreaBLL.Convert_BD09_To_GCJ02(ref lat, ref lng);
                return Content($"{{\"status\":0,\"result\":[{{\"x\":" + lng + ",\"y\":" + lat + "}}]}");

            }
        }

        
    }
}