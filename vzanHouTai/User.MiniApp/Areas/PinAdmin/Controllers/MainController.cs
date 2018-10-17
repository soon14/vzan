using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Pin;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Pin;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using User.MiniApp.Areas.PinAdmin.Filters;
using Utility;

namespace User.MiniApp.Areas.PinAdmin.Controllers
{
    [LoginFilter]
    public class MainController : BaseController
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string act = "", string loginName = "", string password = "", string verify = "")
        {
            if (!string.IsNullOrEmpty(act))
            {
                if (act == "login")
                {
                    if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(password))
                    {
                        result.msg = "用户名和密码不能为空";
                        return Json(result);
                    }
                    PinStore store = PinStoreBLL.SingleModel.GetAdminByLoginParams(loginName, password);
                    if (store != null)
                    {
                        result.code = 1;
                        result.obj = new
                        {
                            store.aId,
                            storeId = store.id,
                        };
                        result.msg = Utility.DESEncryptTools.DESEncrypt(store.id.ToString());
                        return Json(result);
                        //Utility.CookieHelper.SetCookie("dzDishAdmin", Utility.DESEncryptTools.DESEncrypt(store.id.ToString()), WebConfigBLL.CookieDomain);
                    }
                    else
                    {
                        result.code = 0;
                        result.msg = "用户名或密码错误";
                    }
                    return Json(result);
                }
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult GetAuthCode(string telphone = "", string act = "")
        {
            if (!Regex.IsMatch(telphone, @"^1[3-9]+\d{9}$"))
            {
                result.msg = "手机号码格式不正确！";
                return Json(result);
            }
            PinStore store = PinStoreBLL.SingleModel.GetStoreByPhone(telphone);
            if (store != null && act == "")
            {
                result.msg = "用户名已存在，请登陆";
                return Json(result);
            }
            SendMsgHelper sendMsgHelper = new SendMsgHelper();
            string authCode = RedisUtil.Get<string>(telphone);
            if (string.IsNullOrEmpty(authCode))
                authCode = EncodeHelper.CreateRandomCode(4);
            bool sendResult = sendMsgHelper.AliSend(telphone, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), 11) + "\"}", "小未科技", 401);
            if (sendResult)
            {
                RedisUtil.Set<string>(telphone, authCode, TimeSpan.FromMinutes(5));
                result.code = 1;
                result.msg = "验证码发送成功！";
            }
            else
            {
                result.msg = "验证码发送失败,请稍后再试！";
            }
            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult Reg(string act = "", string loginname = "", string password = "", string authCode = "", int aid = 0)
        {
            if (!string.IsNullOrEmpty(act))
            {
                if (aid <= 0 || loginname.Length == 0)
                {
                    result.msg = "非法请求";
                    return Json(result);
                }
                if (act == "reg")
                {
                    PinStore store = PinStoreBLL.SingleModel.GetStoreByPhone(loginname);
                    if (store != null)
                    {
                        result.msg = "用户名已存在，请登陆";
                        return Json(result);
                    }
                    string code = RedisUtil.Get<string>(loginname);
                    if (code == "" || authCode == "" || code != authCode)
                    {
                        result.msg = "验证码错误";
                        return Json(result);
                    }
                    PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
                    store = new PinStore
                    {
                        aId = aid,
                        rz = 1,
                        state = 1,
                        endDate = DateTime.Now.AddDays(platform.freeDays),
                        loginName = loginname,
                        password = Utility.DESEncryptTools.GetMd5Base32(password),
                        startDate = DateTime.Now,
                        userId = 0,//
                        phone = loginname,
                        logo = "",
                        storeName = "",
                        agentId = 0
                    };

                    object obj = PinStoreBLL.SingleModel.Add(store);
                    int storeId = 0;
                    if (!Convert.IsDBNull(obj))
                        storeId = Convert.ToInt32(obj);

                    if (storeId > 0)
                    {
                        //店铺开通成功，检查用户是否在小程序端授权过手机号码，如果有，进行关联
                        //拼享惠绑定的的appid为：
                        C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(store.phone, "wxbb2fe3080d04c9b2");
                        if (userInfo != null)
                        {
                            userInfo.StoreId = storeId;
                            C_UserInfoBLL.SingleModel.Update(userInfo, "StoreId");
                        }

                        result.code = 1;
                        result.obj = new
                        {
                            store.aId,
                            storeId = store.id,
                        };
                        result.msg = DESEncryptTools.DESEncrypt(store.id.ToString());
                    }
                    else
                    {
                        result.msg = "开通失败";
                        result.obj = 0;
                    }
                    return Json(result);
                }

            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPwd(string act = "", string loginname = "", string password = "", string authCode = "", int aid = 0)
        {
            if (!string.IsNullOrEmpty(act))
            {
                if (act == "reset")
                {
                    PinStore store = PinStoreBLL.SingleModel.GetStoreByPhone(loginname);
                    if (store == null)
                    {
                        result.msg = "店铺不存在，请检查手机号码是否正确";
                        return Json(result);
                    }
                    string code = RedisUtil.Get<string>(loginname);
                    if (code == "" || authCode == "" || code != authCode)
                    {
                        result.msg = "验证码错误";
                        return Json(result);
                    }
                    store.password = DESEncryptTools.GetMd5Base32(password);
                    if (PinStoreBLL.SingleModel.Update(store, "password"))
                        result.msg = "密码重置成功，请登陆";
                    else
                        result.msg = "重置失败，请重试";
                    return Json(result);
                }
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Utility.CookieHelper.Remove("dzPinAdmin");
            return Redirect("/PinAdmin/main/Login");
        }

        public ActionResult Index(int appId = 0, int storeId = 0, int aid = 0)
        {
            ViewBag.pinStore = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            bool isRoot = false;

            if (appId == 0)
                appId = aid;
            string AccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
            if (!string.IsNullOrEmpty(AccountId))
            {
                Guid _accountid = Guid.Empty;
                Guid.TryParse(AccountId, out _accountid);

                //用户是否存在
                Account accountModel = AccountBLL.SingleModel.GetModel(_accountid);
                if (accountModel != null)
                {
                    //用户是否开通了这个模板
                    XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, accountModel.Id.ToString());
                    if (xcx != null)
                    {
                        PinStore storeModel = PinStoreBLL.SingleModel.GetModelByAid_Id(xcx.Id, storeId);
                        if (storeModel != null)
                        {
                            isRoot = true;
                        }
                    }
                }
            }
            ViewBag.isRoot = isRoot;

            return View();
        }

        public ActionResult StoreSetting(int aid = 0, int storeId = 0, int Index = 0)
        {
            if (aid <= 0 || storeId <= 0)
            {
                return Content("参数错误");
            }
            PinStore pinStore = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (pinStore != null && !string.IsNullOrEmpty(pinStore.kfUserIds))
            {
                pinStore.kfUserInfo = C_UserInfoBLL.SingleModel.GetModel(Convert.ToInt32(pinStore.kfUserIds));
            }
            
            //获取门店自提地点
            ViewData["placeList"] = PickPlaceBLL.SingleModel.GetListByAid_StoreId(aid, storeId);
            ViewBag.Index = Index;
            return View(pinStore);
        }

        public ActionResult SearchUserInfo(int aid = 0, string nickName = "", int storeId = 0)
        {
            if (aid <= 0 || string.IsNullOrEmpty(nickName) || storeId <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcxRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result);
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (store == null)
            {
                result.code = 0;
                result.msg = "门店不存在";
                return Json(result);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByNickName(xcxRelation.AppId, nickName);
            result.code = 1;
            result.obj = userInfo;
            return Json(result);
        }
        [AllowAnonymous]
        public ActionResult SaveStoreInfo(PinStore store, SettingModel setting)
        {
            if (!ModelState.IsValid)
            {
                result.code = 0;
                result.msg = this.ErrorMsg();
                return Json(result);
            }
            if (store.aId <= 0 || store.id <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            PinStore model = PinStoreBLL.SingleModel.GetModelByAid_Id(store.aId, store.id);
            if (model == null)
            {
                result.code = 0;
                result.msg = "门店不存在";
                return Json(result);
            }
          
            model.kfUserIds = store.kfUserIds;
            SettingModel modelSetting = model.setting;
            modelSetting.autoWelcome = setting.autoWelcome;
            modelSetting.kfPhone = setting.kfPhone;
            modelSetting.openIm = setting.openIm;
            modelSetting.openKfPhone = setting.openKfPhone;
            modelSetting.voiceTips = setting.voiceTips;
            modelSetting.welcome = setting.welcome;
            modelSetting.openZq = setting.openZq;
            modelSetting.place = setting.place;
            modelSetting.name = setting.name;
            modelSetting.phone = setting.phone;
            model.settingJson = JsonConvert.SerializeObject(modelSetting);
            if (PinStoreBLL.SingleModel.Update(model, "settingJson,kfUserIds"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.code = 0;
                result.msg = "保存失败";
            }
            return Json(result);
        }

        public ActionResult updatePickPlaceState(int aid = 0, int storeId = 0, int id = 0, string act = "")
        {
            if (aid <= 0 || storeId <= 0 || id <= 0 || string.IsNullOrEmpty(act))
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            if (act == "del")
            {
                
                PickPlace place = PickPlaceBLL.SingleModel.GetModelByAid_StoreId_Id(aid, storeId, id);
                if (place == null)
                {
                    result.code = 0;
                    result.msg = "数据错误";
                    return Json(result);
                }
                place.state = -1;
                if (PickPlaceBLL.SingleModel.Update(place, "state"))
                {
                    result.code = 1;
                    result.msg = "操作成功";
                }
                else
                {
                    result.code = 0;
                    result.msg = "操作失败";
                }
                return Json(result);
            }
            else
            {
                result.code = 0;
                result.msg = "参数错误act error";
                return Json(result);
            }
        }

        /// <summary>
        /// 保存自提信息
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public ActionResult SavePlaceInfo(PickPlace place)
        {
            
            if (place == null || place.aid <= 0 || place.storeId <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            if (string.IsNullOrEmpty(place.name))
            {
                result.code = 0;
                result.msg = "请输入店铺名称";
                return Json(result);
            }
            if (string.IsNullOrEmpty(place.address))
            {
                result.code = 0;
                result.msg = "请选择店铺地址";
                return Json(result);
            }
            place.addtime = DateTime.Now;
            if (place.Id > 0)
            {
                PickPlace model = PickPlaceBLL.SingleModel.GetModelByAid_Id(place.aid, place.Id);
                if (model == null)
                {
                    result.code = 0;
                    result.msg = "数据错误";
                    return Json(result);
                }
                model.name = place.name;
                model.address = place.address;
                model.lat = place.lat;
                model.lng = place.lng;
                model.addtime = place.addtime;
                if (PickPlaceBLL.SingleModel.Update(model))
                {
                    result.code = 1;
                    result.obj = PickPlaceBLL.SingleModel.GetListByAid_StoreId(place.aid, place.storeId);
                    result.msg = "保存成功";
                }
                else
                {
                    result.code = 0;
                    result.msg = "保存失败";
                }
                return Json(result);
            }
            else
            {
                place.Id = Convert.ToInt32(PickPlaceBLL.SingleModel.Add(place));
                if (place.Id > 0)
                {
                    result.code = 1;
                    result.obj = PickPlaceBLL.SingleModel.GetListByAid_StoreId(place.aid, place.storeId);
                    result.msg = "保存成功";
                }
                else
                {
                    result.code = 0;
                    result.msg = "保存失败";
                }
                return Json(result);
            }
        }

        /// <summary>
        /// 我的桌面
        /// </summary>
        /// <returns></returns>
        public ActionResult Welcome()
        {
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            
            PinStore store = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            Dictionary<string, object> model = new Dictionary<string, object>();

            ////今日总收入 =所有订单金额-交易取消订单金额-交易失败订单金额
            int todayMoney = pinGoodsOrderBLL.GetEarningsByDate(store.id, startDate, endDate);
            model.Add("todayMoney", (todayMoney * 0.01).ToString("0.00"));

            ////今日订单==所有订单-交易取消订单-交易失败订单
            int saleCount = pinGoodsOrderBLL.GetSaleCountByDate(store.id, startDate, endDate);
            model.Add("saleCount", saleCount);

            ////总收入=订单总收入
            int onlineIncome = pinGoodsOrderBLL.GetEarningsByDate(store.id);
            model.Add("onlineIncome", (onlineIncome * 0.01).ToString("0.00"));

            ////总订单数=订单表总数+微信买单总数+余额消费记录总数-余额支付订单数
            int allOrderCount = pinGoodsOrderBLL.GetSaleCountByDate(store.id);
            model.Add("allOrderCount", allOrderCount);

            ////已上架商品数
            int goodsCount = PinGoodsBLL.SingleModel.GetCountByStoreId(store.id);
            model.Add("goodsCount", goodsCount);

            return View(model);
        }

        /// <summary>
        /// 账号设置，密码修改
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountEdit(string act = "", int aId = 0, int storeId = 0, string password = "", string repassword = "")
        {
            PinStore store = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            if (store == null)
                return Content("门店不存在！");

            if (string.IsNullOrEmpty(act))
            {
                if (aId <= 0)
                {
                    result.code = 500;
                    result.msg = "参数错误";
                    return View("PageError", result);
                }
                if (storeId <= 0)
                {
                    result.code = 500;
                    result.msg = "参数错误!";
                    return View("PageError", result);
                }
                return View(store);
            }
            else if (act == "save")
            {
                if (!ModelState.IsValid)
                {
                    result.msg = this.ErrorMsg();
                    return Json(result);
                }
                if (!string.IsNullOrEmpty(password))
                {
                    if (!password.Equals(repassword))
                    {
                        result.msg = "密码不一致";
                        return Json(result);
                    }
                    else
                    {
                        store.password = DESEncryptTools.GetMd5Base32(password);
                    }

                    bool isSuccess = PinStoreBLL.SingleModel.Update(store, "password");
                    if (isSuccess)
                    {
                        result.code = 1;
                        result.msg = "保存成功";
                    }
                    else
                    {
                        result.code = 0;
                        result.msg = "保存失败";
                    }
                }
                else
                {
                    result.msg = "请输入要修改的密码";
                }

            }
            return Json(result);
        }
        /// <summary>
        ///通知设置
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult NoticeSetting(int aId = 0, int storeId = 0)
        {
            
            if (aId <= 0 || storeId <= 0)
            {
                return Content("参数错误");
            }
            string appId= Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppId;//公众号appid
            PinStore pinStore = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            if (pinStore == null)
            {
                return Content("信息错误，请重新登录");
            }
            UserInfoJson userInfo = null;
            if (!string.IsNullOrEmpty(pinStore.wxOpenId))
            {
                userInfo = UserApi.Info(appId, pinStore.wxOpenId, Language.zh_CN);
            }
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["wxbindqrcodekey"] = sessonid;
            if (null == RedisUtil.Get<ScanModel>("wxbindSessionID:" + sessonid))
            {
                ScanModel model = new ScanModel();
                RedisUtil.Set<ScanModel>("wxbindSessionID:" + sessonid, model, TimeSpan.FromDays(1));
            }
            ViewBag.StoreId = storeId;
            return View(userInfo);
        }
        /// <summary>
        /// 扫码监听
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckScan(string wxkey="",int storeId=0)
        {
            PinStore pinStore = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            if (pinStore == null)
            {
                result.msg = "信息错误，请重新登录"+ storeId;
                return Json(result);
            }
            if (string.IsNullOrEmpty(wxkey))
            {
                result.msg = "wxkey为空";
                return Json(result);
            }
            ScanModel model = RedisUtil.Get<ScanModel>("wxbindSessionID:" + wxkey);
            if (model == null || model.userInfo == null)
            {
                result.msg = "未扫码";
                return Json(result);
            }

            result.obj = new { userInfo = model.userInfo };
            pinStore.wxOpenId = model.userInfo.openid;
            if(PinStoreBLL.SingleModel.Update(pinStore, "wxopenid")) 
            {
                result.msg = "绑定成功";
                result.code = 1;
            }
            else
            {
                result.msg = "绑定失败";
            }
            return Json(result);
        }
        /// <summary>
        /// 取消微信绑定
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult CancelBind(int storeId = 0)
        {
            PinStore pinStore = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            if (pinStore == null)
            {
                result.msg = "信息错误，请重新登录" + storeId;
                return Json(result);
            }
            pinStore.wxOpenId = string.Empty;
            if (PinStoreBLL.SingleModel.Update(pinStore, "wxopenid"))
            {
                result.msg = "取消成功";
                result.code = 1;
            }
            else
            {
                result.msg = "取消失败";
            }
            return Json(result);
        }
    }
}