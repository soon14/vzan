using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using BLL.MiniApp.Dish;
using Entity.MiniApp;
using Utility.IO;
using Entity.MiniApp.Dish;
using User.MiniApp.Model;
using Core.MiniApp;
using BLL.MiniApp;
using Newtonsoft.Json;
using User.MiniApp.Areas.DishAdmin.Filters;
using Utility;
using Entity.MiniApp.User;
using BLL.MiniApp.Helper;
using DAL.Base;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    [LoginFilter]
    public class MainController : Controller
    {
        protected readonly DishReturnMsg _result;
        public MainController()
        {
            _result = new DishReturnMsg();
            _result.msg = "";
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string act = "", string login_username = "", string login_userpass = "", string verify = "")
        {
            if (!string.IsNullOrEmpty(act))
            {
                if (act == "login")
                {
                    if (string.IsNullOrEmpty(login_username) || string.IsNullOrEmpty(login_userpass))
                    {
                        _result.msg = "用户名和密码不能为空";
                        return Json(_result);
                    }
                    //if (Session["authorCode"]==null|| verify.ToLower() != Session["authorCode"].ToString().ToLower())
                    //{
                    //    _result.msg = "验证码不正确";
                    //    return Json(_result);
                    //}
                    DishStore store = DishStoreBLL.SingleModel.GetAdminByLoginParams(login_username, login_userpass);
                    if (store != null)
                    {
                        _result.code = 1;
                        _result.obj = new
                        {
                            aId = store.aid,
                            storeId = store.id,
                        };
                        _result.msg = Utility.DESEncryptTools.DESEncrypt(store.id.ToString());
                        return Json(_result);
                        //Utility.CookieHelper.SetCookie("dzDishAdmin", Utility.DESEncryptTools.DESEncrypt(store.id.ToString()), WebConfigBLL.CookieDomain);
                    }
                    else
                    {
                        _result.code = 0;
                        _result.msg = "用户名或密码错误";
                    }
                    return Json(_result);
                }
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Utility.CookieHelper.Remove("dzDishAdmin");
            return Redirect("/DishAdmin/main/Login");
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetAuthCodeImg()
        {
            //验证码获取
            string authorCode = CreateRandomCode();
            Session["authorCode"] = authorCode;
            CreateImage(authorCode);
            Response.End();
            return View();
        }

        public ActionResult Index()
        {
            ViewBag.dishStore = (DishStore)Request.RequestContext.RouteData.Values["dishStore"];
            bool isRoot = false;

            int appId = Context.GetRequestInt("appId", 0);
            if (appId == 0)
                appId = Context.GetRequestInt("aid", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
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
                        DishStore storeModel = DishStoreBLL.SingleModel.GetModelByAid_Id(xcx.Id, storeId);
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

        /// <summary>
        /// 我的桌面
        /// </summary>
        /// <returns></returns>
        public ActionResult Welcome()
        {
            //DishStoreEarnings
            
            DishOrderBLL dishOrderBLL = DishOrderBLL.SingleModel;
            
            

            DishStore store = (DishStore)Request.RequestContext.RouteData.Values["dishStore"];
            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            Dictionary<string, object> model = new Dictionary<string, object>();

            //今日余额支付收入
            double todayAccountMoney = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.余额支付);
            model.Add("todayAccountMoney", todayAccountMoney);
            //今日微信支付收入
            double todayWxMoney = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.微信支付);
            model.Add("todayWxMoney", todayWxMoney);
            //今日现金支付收入
            double todayxjIncome = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.线下支付);
            model.Add("todayxjIncome", todayxjIncome);
            //今日货到付款收入
            double todayhdfkIncome = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.货到支付);
            model.Add("todayhdfkIncome", todayhdfkIncome);
            //今日手动确认收入
            double todaysdIncome = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, 0);
            model.Add("todaysdIncome", todaysdIncome);
            //今日微信买单收入
            double todayWxMdIncome = dishOrderBLL.GetWxMdIncomeByDate(store.id, startDate, endDate);
            model.Add("todayWxMdIncome", todayWxMdIncome);
            //今日余额买单收入
            double todayAccountMdIncome = DishCardAccountLogBLL.SingleModel.GetAccountSumByDate(store.id, startDate, endDate) - todayAccountMoney;
            todayAccountMdIncome = Math.Round(todayAccountMdIncome, 2);
            model.Add("todayAccountMdIncome", todayAccountMdIncome);
            //今日总收入
            double todayMoney = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate) + todayWxMdIncome + todayAccountMdIncome;
            model.Add("todayMoney", todayMoney);

            //今日余额支付订单数
            int saleAccountCount = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.余额支付);
            model.Add("saleAccountCount", saleAccountCount);
            //今日微信支付订单数
            int saleWxCount = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.微信支付);
            model.Add("saleWxCount", saleWxCount);
            //今日现金支付订单数
            int saleXjCount = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.线下支付);
            model.Add("saleXjCount", saleXjCount);
            //今日货到付款订单数
            int saleHdfkCount = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.货到支付);
            model.Add("saleHdfkCount", saleHdfkCount);
            //今日手动确认订单数
            int saleSdCount = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, 0);
            model.Add("saleSdCount", saleSdCount);
            //今日微信买单订单数
            int wxMdCount = dishOrderBLL.GetWxMdCountByDate(store.id, startDate, endDate);
            model.Add("wxMdCount", wxMdCount);
            //今日余额买单订单数
            int accountMdCount = DishCardAccountLogBLL.SingleModel.GetCountByDate(store.id, startDate, endDate) - saleAccountCount;
            model.Add("accountMdCount", accountMdCount);
            //今日订单=订单表总数+微信买单订单数+余额买单订单数
            int saleCount = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate) + wxMdCount + accountMdCount;
            model.Add("saleCount", saleCount);

            //余额支付总收入
            double accountIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.余额支付);
            model.Add("accountIncome", accountIncome);
            //微信支付总收入
            double wxIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.微信支付);
            model.Add("wxIncome", wxIncome);
            //现金支付总收入
            double xjIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.线下支付);
            model.Add("xjIncome", xjIncome);
            //货到付款总收入
            double hdfkIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.货到支付);
            model.Add("hdfkIncome", hdfkIncome);
            //手动确认总收入
            double sdIncome = dishOrderBLL.GetEarnings(store.id, 0);
            model.Add("sdIncome", sdIncome);
            //微信买单收入
            double wxMdIncome = dishOrderBLL.GetWxMdIncomeByDate(store.id, startDate, endDate, false);
            model.Add("wxMdIncome", wxMdIncome);
            //余额买单收入
            double accountMdIncome = DishCardAccountLogBLL.SingleModel.GetAccountSumByDate(store.id, startDate, endDate, false) - accountIncome;
            accountMdIncome = Math.Round(accountMdIncome, 2);
            model.Add("accountMdIncome", accountMdIncome);
            //总收入=订单总收入+微信买单收入+余额买单收入
            double onlineIncome = dishOrderBLL.GetEarnings(store.id) + wxMdIncome + accountMdIncome;
            model.Add("onlineIncome", onlineIncome);

            //总订单数=订单表总数+微信买单总数+余额消费记录总数-余额支付订单数
            int allOrderCount = dishOrderBLL.GetSaleCount(store.id) + dishOrderBLL.GetWxMdCountByDate(store.id, startDate, endDate, false) + DishCardAccountLogBLL.SingleModel.GetCountByDate(store.id, startDate, endDate) - dishOrderBLL.GetSaleCount(store.id, (int)DishEnums.PayMode.余额支付);
            model.Add("allOrderCount", allOrderCount);
            //商品数
            int goodsCount = DishGoodBLL.SingleModel.GetCountByStoreId(store.id);
            model.Add("goodsCount", goodsCount);
            //餐桌数
            int tablesCount = DishTableBLL.SingleModel.GetCountByStoreId(store.id);
            model.Add("tablesCount", tablesCount);
            //打印机数
            int printerCount = DishPrintBLL.SingleModel.GetCountByStoreId(store.id);
            model.Add("printerCount", printerCount);
            //用户数
            //int userCount = c_UserInfoBLL.GetCountByStoreId(store.id);
            //model.Add("userCount", userCount);

            return View(model);
        }

        /// <summary>
        /// 门店信息
        /// </summary>
        /// <returns></returns>
        public ActionResult DishInfo()
        {
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误";
                return View("PageError", _result);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId <= 0)
            {
                _result.code = 500;
                _result.msg = "参数错误!";
                return View("PageError", _result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (store == null)
            {
                _result.code = 500;
                _result.msg = "门店不存在";
                return View("PageError", _result);
            }
            //基础设置
            if (string.IsNullOrEmpty(store.baseConfigJson))
            {
                store.baseConfig = new DishBaseConfig();
                store.baseConfigJson = JsonConvert.SerializeObject(store.baseConfig);
            }
            else
            {
                store.baseConfig = JsonConvert.DeserializeObject<DishBaseConfig>(store.baseConfigJson);
            }

            //高级设置
            if (string.IsNullOrEmpty(store.gaojiConfigJson))
            {
                store.gaojiConfig = new DishGaojiConfig();
                store.gaojiConfigJson = JsonConvert.SerializeObject(store.gaojiConfig);
            }
            else
            {
                store.gaojiConfig = JsonConvert.DeserializeObject<DishGaojiConfig>(store.gaojiConfigJson);
            }
            //店内设置
            if (string.IsNullOrEmpty(store.dianneiConfigJson))
            {
                store.dianneiConfig = new DishDianneiConfig();
                store.dianneiConfigJson = JsonConvert.SerializeObject(store.dianneiConfig);
            }
            else
            {
                store.dianneiConfig = JsonConvert.DeserializeObject<DishDianneiConfig>(store.dianneiConfigJson);
                if (string.IsNullOrEmpty(store.dianneiConfig.dish_diannei_tips_one)) store.dianneiConfig.dish_diannei_tips_one = "下单付款后，订单才能下送后厨";
                if (string.IsNullOrEmpty(store.dianneiConfig.dish_diannei_tips_two)) store.dianneiConfig.dish_diannei_tips_two = "下单后，订单将下送到厨房";
            }
            //外卖设置
            if (string.IsNullOrEmpty(store.takeoutConfigJson))
            {
                store.takeoutConfig = new DishTakeoutConfig();
                store.takeoutConfigJson = JsonConvert.SerializeObject(store.takeoutConfig);
            }
            else
            {
                store.takeoutConfig = JsonConvert.DeserializeObject<DishTakeoutConfig>(store.takeoutConfigJson);
            }

            //跳转设置
            if (string.IsNullOrEmpty(store.tiaozhuanConfigJson))
            {
                store.tiaozhuanConfig = new DishTiaoZhuanConfig();
                store.tiaozhuanConfigJson = JsonConvert.SerializeObject(store.tiaozhuanConfig);
            }
            else
            {
                store.tiaozhuanConfig = JsonConvert.DeserializeObject<DishTiaoZhuanConfig>(store.tiaozhuanConfigJson);
            }

            return View(store);
        }

        public ActionResult saveDishInfo(DishBaseConfig baseConfig, DishGaojiConfig gaojiConfig, DishDianneiConfig dianneiConfig, DishTakeoutConfig takeoutConfig, DishTiaoZhuanConfig tiaozhuanConfig, List<string> dish_open_btime, List<string> dish_open_etime, List<string> dish_open_wm_btime, List<string> dish_open_wm_etime, List<string> dish_zq_btime, List<string> dish_zq_etime, List<string> ps_time_b, List<string> ps_time_e, List<double> ps_time_jiner, string dish_con_mobile, string dish_con_phone, double ws_lat, double ws_lng)
        {
            if (baseConfig == null || gaojiConfig == null || dianneiConfig == null || takeoutConfig == null || tiaozhuanConfig == null)
            {
                _result.msg = "数据错误！";
                return Json(_result);
            }
            int aid = Context.GetRequestInt("aId", 0);
            if (aid <= 0)
            {
                _result.msg = "参数错误";
                return View("PageError", _result);
            }
            int storeId = Context.GetRequestInt("storeId", 0);
            if (storeId <= 0)
            {
                _result.msg = "参数错误!";
                return View("PageError", _result);
            }
            DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (store == null)
            {
                _result.msg = "门店不存在";
                return Json(_result);
            }
            string dish_name = Context.GetRequest("dish_name", string.Empty);
            if (string.IsNullOrEmpty(dish_name))
            {
                _result.msg = "请输入店铺名称";
                return Json(_result);
            }
            int dish_cate_id = Context.GetRequestInt("dish_cate_id", 0);
            string dish_logo = Context.GetRequest("dish_logo", string.Empty);
            if (string.IsNullOrEmpty(dish_logo))
            {
                _result.msg = "请上传门店logo";
                return Json(_result);
            }
            if ((dish_open_btime == null || dish_open_btime.Count <= 0) && (dish_open_etime == null || dish_open_etime.Count <= 0))
            {
                _result.msg = "请选择店内营业时间";
                return Json(_result);
            }
            if ((dish_open_wm_btime == null || dish_open_wm_btime.Count <= 0) && (dish_open_wm_etime == null || dish_open_wm_etime.Count <= 0))
            {
                _result.msg = "请选择外卖营业时间";
                return Json(_result);
            }
            baseConfig.open_time = GetDishOpenTime(dish_open_btime, dish_open_etime);
            baseConfig.wm_time = GetDishOpenWmTime(dish_open_wm_btime, dish_open_wm_etime);
            dianneiConfig.zq_time = GetDishZqTime(dish_zq_btime, dish_zq_etime);
            takeoutConfig.ps_rule = GetDishPsRule(ps_time_b, ps_time_e, ps_time_jiner);
            store.dish_logo = dish_logo;
            store.dish_name = dish_name;
            store.dish_cate_id = dish_cate_id;
            store.baseConfigJson = JsonConvert.SerializeObject(baseConfig);
            store.gaojiConfigJson = JsonConvert.SerializeObject(gaojiConfig);
            store.dianneiConfigJson = JsonConvert.SerializeObject(dianneiConfig);
            store.takeoutConfigJson = JsonConvert.SerializeObject(takeoutConfig);
            store.tiaozhuanConfigJson = JsonConvert.SerializeObject(tiaozhuanConfig);
            store.dish_con_mobile = dish_con_mobile;
            store.dish_con_phone = dish_con_phone;
            store.ws_lat = ws_lat;
            store.ws_lng = ws_lng;
            bool isSuccess = DishStoreBLL.SingleModel.Update(store, "dish_con_mobile,dish_con_phone,dish_logo,dish_name,dish_cate_id,baseConfigJson,gaojiConfigJson,dianneiConfigJson,takeoutConfigJson,tiaozhuanConfigJson,ws_lat,ws_lng");
            if (isSuccess)
            {
                _result.code = 1;
                _result.msg = "保存成功";
            }
            else
            {
                _result.code = 0;
                _result.msg = "保存失败";
            }
            return Json(_result);
        }

        /// <summary>
        /// 拼接配送规则
        /// </summary>
        /// <param name="ps_time_b"></param>
        /// <param name="ps_time_e"></param>
        /// <param name="ps_time_jiner"></param>
        /// <returns></returns>
        private List<DishPsRule> GetDishPsRule(List<string> ps_time_b, List<string> ps_time_e, List<double> ps_time_jiner)
        {
            if (ps_time_b == null) ps_time_b = new List<string>();
            if (ps_time_e == null) ps_time_e = new List<string>();
            List<DishPsRule> list = new List<DishPsRule>();
            for (int i = 0; i < ps_time_b.Count; i++)
            {
                DishPsRule psRule = new DishPsRule();
                psRule.ps_time_b = ps_time_b[i];
                if (i < ps_time_e.Count)
                {
                    psRule.ps_time_e = ps_time_e[i];
                }
                if (i < ps_time_jiner.Count)
                {
                    psRule.ps_time_jiner = ps_time_jiner[i];
                }
                list.Add(psRule);
            }
            return list;
        }

        /// <summary>
        /// 拼接店内自取时间
        /// </summary>
        /// <param name="dish_zq_btime"></param>
        /// <param name="dish_zq_etime"></param>
        /// <returns></returns>
        private List<DishZqTime> GetDishZqTime(List<string> dish_zq_btime, List<string> dish_zq_etime)
        {
            if (dish_zq_btime == null) dish_zq_btime = new List<string>();
            if (dish_zq_etime == null) dish_zq_etime = new List<string>();
            List<DishZqTime> list = new List<DishZqTime>();
            if (dish_zq_btime.Count > dish_zq_btime.Count)
            {
                for (int i = 0; i < dish_zq_btime.Count; i++)
                {
                    DishZqTime zqTime = new DishZqTime();
                    zqTime.dish_zq_btime = dish_zq_btime[i];
                    if (i < dish_zq_etime.Count)
                    {
                        zqTime.dish_zq_etime = dish_zq_etime[i];
                    }
                    list.Add(zqTime);
                }
            }
            else
            {
                for (int i = 0; i < dish_zq_etime.Count; i++)
                {
                    DishZqTime openTime = new DishZqTime();
                    openTime.dish_zq_etime = dish_zq_etime[i];
                    if (i < dish_zq_btime.Count)
                    {
                        openTime.dish_zq_btime = dish_zq_btime[i];
                    }
                    list.Add(openTime);
                }
            }
            return list;
        }

        /// <summary>
        /// 拼装门店营业时间
        /// </summary>
        /// <param name="open_btime"></param>
        /// <param name="open_etime"></param>
        /// <returns></returns>
        private List<DishOpenTime> GetDishOpenTime(List<string> open_btime, List<string> open_etime)
        {
            if (open_btime == null) open_btime = new List<string>();
            if (open_etime == null) open_etime = new List<string>();
            List<DishOpenTime> list = new List<DishOpenTime>();
            if (open_btime.Count > open_etime.Count)
            {
                for (int i = 0; i < open_btime.Count; i++)
                {
                    DishOpenTime openTime = new DishOpenTime();
                    openTime.dish_open_btime = open_btime[i];
                    if (i < open_etime.Count)
                    {
                        openTime.dish_open_etime = open_etime[i];
                    }
                    list.Add(openTime);
                }
            }
            else
            {
                for (int i = 0; i < open_etime.Count; i++)
                {
                    DishOpenTime openTime = new DishOpenTime();
                    openTime.dish_open_etime = open_etime[i];
                    if (i < open_btime.Count)
                    {
                        openTime.dish_open_btime = open_btime[i];
                    }
                    list.Add(openTime);
                }
            }
            return list;
        }

        /// <summary>
        /// 拼装外卖营业时间
        /// </summary>
        /// <param name="open_btime"></param>
        /// <param name="open_etime"></param>
        /// <returns></returns>
        private List<DishOpenWmTime> GetDishOpenWmTime(List<string> open_btime, List<string> open_etime)
        {
            if (open_btime == null) open_btime = new List<string>();
            if (open_etime == null) open_etime = new List<string>();
            List<DishOpenWmTime> list = new List<DishOpenWmTime>();
            if (open_btime.Count > open_etime.Count)
            {
                for (int i = 0; i < open_btime.Count; i++)
                {
                    DishOpenWmTime openTime = new DishOpenWmTime();
                    openTime.dish_open_wm_btime = open_btime[i];
                    if (i < open_etime.Count)
                    {
                        openTime.dish_open_wm_etime = open_etime[i];
                    }
                    list.Add(openTime);
                }
            }
            else
            {
                for (int i = 0; i < open_etime.Count; i++)
                {
                    DishOpenWmTime openTime = new DishOpenWmTime();
                    openTime.dish_open_wm_etime = open_etime[i];
                    if (i < open_btime.Count)
                    {
                        openTime.dish_open_wm_btime = open_btime[i];
                    }
                    list.Add(openTime);
                }
            }
            return list;
        }

        /// <summary>
        /// 支付配置
        /// </summary>
        /// <returns></returns>
        public ActionResult PaySetting(string act = "", DishPaySetting paySetting = null)
        {
            int aid = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            if (act != "save")
            {
                if (aid <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误";
                    return View("PageError", _result);
                }
                if (storeId <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误!";
                    return View("PageError", _result);
                }
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.code = 500;
                    _result.msg = "门店不存在";
                    return View("PageError", _result);
                }
                if (string.IsNullOrEmpty(store.paySettingJson))
                {
                    store.paySetting = new DishPaySetting();
                }
                else
                {
                    store.paySetting = JsonConvert.DeserializeObject<DishPaySetting>(store.paySettingJson);
                }
                return View(store);
            }
            else
            {
                if (aid <= 0)
                {
                    _result.msg = "参数错误";
                    return Json(_result);
                }
                if (storeId <= 0)
                {
                    _result.msg = "参数错误!";
                    return Json(_result);
                }
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.msg = "门店不存在";
                    return Json(_result);
                }
                if (paySetting == null)
                {
                    _result.msg = "数据错误!";
                    return Json(_result);
                }
                if (paySetting.pay_yuer_isopen == 1)
                {
                    DishVipCardSetting card = DishVipCardSettingBLL.SingleModel.GetModelByStoreId(store.id);
                    if (card == null || card.card_open_status == 0)
                    {
                        _result.msg = "会员功能还未开启";
                        return Json(_result);
                    }
                }

                store.paySettingJson = JsonConvert.SerializeObject(paySetting);
                bool isSuccess = DishStoreBLL.SingleModel.Update(store, "paySettingJson");
                if (isSuccess)
                {
                    _result.code = 1;
                    _result.msg = "保存成功";
                }
                else
                {
                    _result.code = 0;
                    _result.msg = "保存失败";
                }
                return Json(_result);
            }
        }

        /// <summary>
        /// 短信配置
        /// </summary>
        /// <returns></returns>
        public ActionResult SmsSetting(string act = "", DishSmsSetting smsSetting = null)
        {
            int aid = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            if (act != "save")
            {
                if (aid <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误";
                    return View("PageError", _result);
                }
                if (storeId <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误!";
                    return View("PageError", _result);
                }
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.code = 500;
                    _result.msg = "门店不存在";
                    return View("PageError", _result);
                }
                if (string.IsNullOrEmpty(store.smsSettingJson))
                {
                    store.smsSetting = new DishSmsSetting();
                }
                else
                {
                    store.smsSetting = JsonConvert.DeserializeObject<DishSmsSetting>(store.smsSettingJson);
                }
                return View(store);
            }
            else
            {
                if (aid <= 0)
                {
                    _result.msg = "参数错误";
                    return Json(_result);
                }
                if (storeId <= 0)
                {
                    _result.msg = "参数错误!";
                    return Json(_result);
                }
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
                if (store == null)
                {
                    _result.msg = "门店不存在";
                    return Json(_result);
                }
                if (smsSetting == null)
                {
                    _result.msg = "数据错误!";
                    return Json(_result);
                }
                if (string.IsNullOrEmpty(smsSetting.msg_phone))
                {
                    _result.msg = "请输入手机号码";
                    return Json(_result);
                }
                if (smsSetting.msg_phone.Length != 11)
                {
                    _result.msg = "手机号码错误";
                    return Json(_result);
                }
                store.smsSettingJson = JsonConvert.SerializeObject(smsSetting);
                bool isSuccess = DishStoreBLL.SingleModel.Update(store, "smsSettingJson");
                if (isSuccess)
                {
                    _result.code = 1;
                    _result.msg = "保存成功";
                }
                else
                {
                    _result.code = 0;
                    _result.msg = "保存失败";
                }
                return Json(_result);
            }
        }

        /// <summary>
        /// 账号设置，密码修改
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountEdit(string act = "")
        {
            int aid = Context.GetRequestInt("aId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);

            if (act != "save")
            {
                if (aid <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误";
                    return View("PageError", _result);
                }
                if (storeId <= 0)
                {
                    _result.code = 500;
                    _result.msg = "参数错误!";
                    return View("PageError", _result);
                }
                DishStore store = (DishStore)Request.RequestContext.RouteData.Values["dishStore"];

                if (store == null)
                {
                    _result.code = 500;
                    _result.msg = "门店不存在";
                    return View("PageError", _result);
                }

                return View(store);
            }
            else
            {
                DishStore store = (DishStore)Request.RequestContext.RouteData.Values["dishStore"];
                if (store == null)
                {
                    _result.msg = "门店不存在";
                    return Json(_result);
                }
                string login_username = Context.GetRequest("login_username", string.Empty);
                string login_userpass = Context.GetRequest("login_userpass", string.Empty);
                string login_nuserpass = Context.GetRequest("login_nuserpass", string.Empty);
                string filedStr = "login_username";
                if (string.IsNullOrEmpty(login_username))
                {
                    _result.msg = "登录名不能为空";
                    return Json(_result);
                }
                store.login_username = login_username;
                if (!string.IsNullOrEmpty(login_userpass))
                {
                    if (!login_userpass.Equals(login_nuserpass))
                    {
                        _result.msg = "密码不一致";
                        return Json(_result);
                    }
                    else
                    {
                        store.login_userpass = DESEncryptTools.GetMd5Base32(login_userpass);
                        filedStr += ",login_userpass";
                    }
                }
                bool isSuccess = DishStoreBLL.SingleModel.Update(store, filedStr);
                if (isSuccess)
                {
                    _result.code = 1;
                    _result.msg = "保存成功";
                }
                else
                {
                    _result.code = 0;
                    _result.msg = "保存失败";
                }
                return Json(_result);
            }
        }

        /// <summary>
        /// 餐桌管理
        /// </summary>
        /// <returns></returns>
        public ActionResult DishTable(int aId = 0, int storeId = 0, string sortData = "", string act = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishTable> vm = new ViewModel<DishTable>();
                vm.DataList = DishTableBLL.SingleModel.GetTableByParams(aId, storeId, true, true);

                vm.DataModel = new DishTable();
                vm.DataModel.aId = aId;
                vm.DataModel.storeId = storeId;
                return View(vm);
            }
            else
            {
                //更新排序
                if (act == "sort")
                {
                    bool updateResult = DishTableBLL.SingleModel.UpdateSortBatch(sortData);
                    _result.code = updateResult ? 1 : 0;
                    _result.msg = updateResult ? "修改成功" : "修改失败";
                }

                //一键清台
                else if (act == "clear")
                {
                    bool isSuccess = DishTableBLL.SingleModel.ClearTable(aId, storeId);

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "清台成功" : "清台失败";
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 展开编辑桌台
        /// </summary>
        /// <param name="act">操作类型</param>
        /// <param name="editCols">更新的列</param>
        /// <returns></returns>
        public ActionResult DishTableEdit(DishTable model, string act = "", string editCols = "")
        {
            EditModel<DishTable> editModel = new EditModel<DishTable>();
            //进入编辑页面
            if (string.IsNullOrWhiteSpace(act))
            {
                if (model.id <= 0)
                {
                    editModel.DataModel = new DishTable();
                    editModel.DataModel.aId = model.aId;
                    editModel.DataModel.storeId = model.storeId;
                    editModel.DataModel.table_renshu = 2;
                    editModel.DataModel.table_sort = 99;
                    return View(editModel);
                }
                else
                {
                    editModel.DataModel = DishTableBLL.SingleModel.GetModel(model.id);
                    return View(editModel);
                }
            }

            //保存编辑内容
            else if (act == "save")
            {
                //检测是否存在同名桌台号
                if (DishTableBLL.SingleModel.CheckExistTableName(model.id, model.aId, model.storeId, model.table_name))
                {
                    _result.code = 0;
                    _result.msg = "保存失败,桌台名称重复";
                    return Json(_result);
                }
                if (model.id <= 0)
                {
                    Int32 id = Convert.ToInt32(DishTableBLL.SingleModel.Add(model));

                    _result.code = id > 0 ? 1 : 0;
                    _result.msg = id > 0 ? "保存成功" : "保存失败";
                }
                else
                {
                    bool isSuccess = false;
                    if (!string.IsNullOrWhiteSpace(editCols))
                    {
                        isSuccess = DishTableBLL.SingleModel.Update(model, editCols);
                    }
                    else
                    {
                        isSuccess = DishTableBLL.SingleModel.Update(model, "table_name,table_renshu,table_sort");//默认更新
                    }
                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "保存成功" : "保存失败";
                }
                return Json(_result);
            }

            //删除
            else if (act == "del")
            {
                if (model.id <= 0)
                {
                    _result.code = 0;
                    _result.msg = "参数错误";
                }
                else
                {
                    DishTable updateModel = DishTableBLL.SingleModel.GetModel(model.id);
                    if (updateModel != null)
                    {
                        updateModel.state = -1;
                        bool updateResult = DishTableBLL.SingleModel.Update(updateModel, "state");

                        _result.code = updateResult ? 1 : 0;
                        _result.msg = updateResult ? "删除成功" : "删除失败";
                    }
                    else
                    {
                        _result.code = 0;
                        _result.msg = "删除失败,桌台不存在";
                    }
                }
                return Json(_result);
            }

            return View(editModel);
        }

        /// <summary>
        /// 二维码管理
        /// </summary>
        /// <returns></returns>
        public ActionResult QrcodeList(int aId = 0, int storeId = 0, string act = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                Dictionary<string, string> qrcode_urls = new Dictionary<string, string>();
                string xcxapiurl = string.Empty;//获取token的路径

                qrcodeclass qrcode = null;
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(aId);
                //OpenAuthorizerConfig authorConfig = _openAuthorizerConfigBLL.GetModelByAppids(app.AppId);
                //if (authorConfig == null)
                //{
                //    _result.code = 500;
                //    _result.msg = "用户授权资料异常,请用小程序账号重新授权";
                //    return View("PageError", _result);
                //}
                //xcxapiurl = XcxApiBLL.SingleModel.GetOpenAuthodModel(authorConfig.user_name);

                string token = "";
                DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
                if (store != null)
                {
                    if (string.IsNullOrWhiteSpace(store.storeHome_qrcode))
                    {
                        if (XcxApiBLL.SingleModel.GetToken(app, ref token))
                        {
                            //店铺首页二维码
                            qrcode = CommondHelper.GetMiniAppQrcode_wxaapp(token, $"pages/restaurant/restaurant-home-info/index?aId={aId}&dish_id={storeId}");
                            if (qrcode.isok == 1)
                            {
                                store.storeHome_qrcode = qrcode.url;
                                DishStoreBLL.SingleModel.Update(store, "storeHome_qrcode");
                            }
                        }
                    }
                    if (string.IsNullOrWhiteSpace(store.payPage_qrcode))
                    {
                        if (XcxApiBLL.SingleModel.GetToken(app, ref token))
                        {
                            //店铺支付页二维码
                            qrcode = CommondHelper.GetMiniAppQrcode_wxaapp(token, $"pages/restaurant/pay/index?aId={aId}&dish_id={storeId}");
                            if (qrcode.isok == 1)
                            {
                                store.payPage_qrcode = qrcode.url;
                                DishStoreBLL.SingleModel.Update(store, "payPage_qrcode");
                            }
                        }
                    }

                    qrcode_urls.Add("店铺小程序二维码", store.storeHome_qrcode);
                    qrcode_urls.Add("支付小程序二维码", store.payPage_qrcode);
                }

                List<DishTable> dishTabls = DishTableBLL.SingleModel.GetTableByParams(aId, storeId, true, true);
                dishTabls?.ForEach(t =>
                {
                    if (string.IsNullOrWhiteSpace(t.table_qrcode))
                    {
                        if (XcxApiBLL.SingleModel.GetToken(app, ref token))
                        {
                            //店铺桌台二维码
                            qrcode = CommondHelper.GetMiniAppQrcode_wxaapp(token, $"pages/restaurant/restaurant-single/index?aId={aId}&dish_id={storeId}&table_id={t.id}");
                            if (qrcode.isok == 1)
                            {
                                t.table_qrcode = qrcode.url;
                                DishTableBLL.SingleModel.Update(t, "table_qrcode");
                            }
                        }
                    }

                    qrcode_urls.Add($"{t.id}||{t.table_name}({t.table_renshu}人桌)", t.table_qrcode);
                });

                return View(qrcode_urls);
            }
            else
            {
                //刷新二维码
                if (act == "refresh")
                {
                    Dictionary<string, string> qrcode_urls = new Dictionary<string, string>();

                    DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
                    if (store != null)
                    {
                        store.storeHome_qrcode = string.Empty;
                        store.payPage_qrcode = string.Empty;

                        DishStoreBLL.SingleModel.Update(store, "storeHome_qrcode,payPage_qrcode");
                    }

                    List<DishTable> dishTabls = DishTableBLL.SingleModel.GetTableByParams(aId, storeId, true, true);
                    dishTabls?.ForEach(t =>
                    {
                        t.table_qrcode = string.Empty;

                        DishTableBLL.SingleModel.Update(t, "table_qrcode");
                    });

                    _result.code = 1;
                    _result.msg = "刷新成功";
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 排队管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Queue(int aId = 0, int storeId = 0, int state = 999, string act = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishQueue> vm = new ViewModel<DishQueue>();
                vm.DataList = DishQueueBLL.SingleModel.GetQueuesByParams(aId, storeId, true, true) ?? new List<DishQueue>();

                vm.DataModel = new DishQueue();
                vm.DataModel.aId = aId;
                vm.DataModel.storeId = storeId;
                return View(vm);
            }
            else
            {
            }
            return Json(_result);
        }

        /// <summary>
        /// 排队队列编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult DishQueueEdit(DishQueue model, string act = "", string editCols = "")
        {
            EditModel<DishQueue> editModel = new EditModel<DishQueue>();

            //进入编辑页面
            if (string.IsNullOrWhiteSpace(act))
            {
                if (model.id <= 0)
                {
                    editModel.DataModel = new DishQueue();
                    editModel.DataModel.aId = model.aId;
                    editModel.DataModel.storeId = model.storeId;
                    editModel.DataModel.q_renshu = 2;
                    editModel.DataModel.q_order = 99;

                    return View(editModel);
                }
                else
                {
                    editModel.DataModel = DishQueueBLL.SingleModel.GetModel(model.id);
                    return View(editModel);
                }
            }

            //保存编辑内容
            else if (act == "save")
            {
                //检测是否存在同名桌台号
                if (DishQueueBLL.SingleModel.CheckExistQueueName(model.id, model.aId, model.storeId, model.q_name))
                {
                    _result.code = 0;
                    _result.msg = "保存失败,队列名称重复";
                    return Json(_result);
                }

                if (model.id <= 0)
                {
                    Int32 id = Convert.ToInt32(DishQueueBLL.SingleModel.Add(model));

                    _result.code = id > 0 ? 1 : 0;
                    _result.msg = id > 0 ? "保存成功" : "保存失败";
                }
                else
                {
                    bool isSuccess = false;
                    if (!string.IsNullOrWhiteSpace(editCols))
                    {
                        isSuccess = DishQueueBLL.SingleModel.Update(model, editCols);
                    }
                    else
                    {
                        isSuccess = DishQueueBLL.SingleModel.Update(model, "q_name,q_renshu,q_qianzhui,q_tongzhi_renshu,q_order,q_curnumber,state");//默认更新
                    }
                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "保存成功" : "保存失败";
                }
                return Json(_result);
            }

            //删除
            else if (act == "del")
            {
                if (model.id <= 0)
                {
                    _result.code = 0;
                    _result.msg = "参数错误";
                }
                else
                {
                    //队列删除判定里头有否用户排队
                    //if (DishQueueUpBLL.SingleModel.CheckExistQueueUps(model.aId, model.storeId, model.id))
                    //{
                    //    _result.code = 0;
                    //    _result.msg = "队列中存在用户,删除失败";
                    //    return Json(_result);
                    //}

                    DishQueue updateModel = DishQueueBLL.SingleModel.GetModel(model.id);
                    if (updateModel != null)
                    {
                        updateModel.state = -1;
                        bool updateResult = DishQueueBLL.SingleModel.Update(updateModel, "state");

                        _result.code = updateResult ? 1 : 0;
                        _result.msg = updateResult ? "删除成功" : "删除失败";
                    }
                    else
                    {
                        _result.code = 0;
                        _result.msg = "删除失败,队列不存在";
                    }
                }
                return Json(_result);
            }
            return View();
        }

        /// <summary>
        /// 客人队列
        /// </summary>
        /// <returns></returns>
        public ActionResult Customer(int aId = 0, int storeId = 0, int state = 999, string act = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishQueueUp> vm = new ViewModel<DishQueueUp>();
                vm.DataList = DishQueueUpBLL.SingleModel.GetQueueUpByParams(aId, storeId, state, true)?.OrderByDescending(q => q.id).ToList() ?? new List<DishQueueUp>();

                vm.DataModel = new DishQueueUp();
                vm.DataModel.aId = aId;
                vm.DataModel.storeId = storeId;

                ViewBag.DishQueueList = DishQueueBLL.SingleModel.GetQueuesByParams(aId, storeId, true, true) ?? new List<DishQueue>();
                return View(vm);
            }
            else
            {
            }

            return View();
        }

        /// <summary>
        /// 客人队列记录
        /// </summary>
        /// <returns></returns>
        public ActionResult DishQueueUpEdit(DishQueueUp model, string act = "", string editCols = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                EditModel<DishQueueUp> vm = new EditModel<DishQueueUp>();
                if (model.id > 0)
                {
                    vm.DataModel = DishQueueUpBLL.SingleModel.GetModel(model.id);
                }
                else
                {
                    vm.DataModel = new DishQueueUp();
                    vm.DataModel.aId = model.aId;
                    vm.DataModel.storeId = model.storeId;
                }
                return View(vm);
            }
            else
            {
                if (act == "save")
                {
                    if (model.id <= 0)
                    {
                        Int32 id = Convert.ToInt32(DishQueueUpBLL.SingleModel.Add(model));

                        _result.code = id > 0 ? 1 : 0;
                        _result.msg = id > 0 ? "保存成功" : "保存失败";
                    }
                    else
                    {
                        XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(model.aId);
                        if (xcxrelation == null)
                        {
                            _result.code = 0;
                            _result.msg = "小程序未绑定";
                            return Json(_result);
                        }
                        XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
                        if (xcxTemplate == null)
                        {
                            _result.msg = "未找到小程序的模板";
                            return Json(_result);
                        }

                        DishQueueUp editModel = DishQueueUpBLL.SingleModel.GetModel(model.id);

                        if (model == null)
                        {
                            _result.msg = "排队信息不存在";
                            return Json(_result);
                        }
                        editModel.state = model.state;
                        bool isSuccess = false;
                        if (!string.IsNullOrWhiteSpace(editCols))
                        {
                            isSuccess = DishQueueUpBLL.SingleModel.Update(editModel, editCols);
                        }
                        else
                        {
                            isSuccess = DishQueueUpBLL.SingleModel.Update(editModel, "state");//默认更新
                        }
                        _result.code = isSuccess ? 1 : 0;
                        _result.msg = isSuccess ? "保存成功" : "保存失败";

                        if (isSuccess && editModel.state == 1)
                        {
                            try
                            {
                                DishStore dishStore = (DishStore)Request.RequestContext.RouteData.Values["dishStore"];
                                object curSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.SortQueueTemplateMessageData(dishStore.dish_name, editModel, SendTemplateMessageTypeEnum.排队拿号排队到号通知);
                                //TODO:$"pages/restaurant/paidui/index?dish_id={model.storeId}&id={model.id}"
                                TemplateMsg_Miniapp.SendTemplateMessage(editModel.user_Id, SendTemplateMessageTypeEnum.排队拿号排队到号通知, xcxTemplate.Type, curSortQueue_TemplateMsgObj, "");//
                                editModel.q_is_tongzhi = 1;
                                DishQueueUpBLL.SingleModel.Update(editModel, "q_is_tongzhi");
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    return Json(_result);
                }

                //删除
                else if (act == "del")
                {
                    if (model.id <= 0)
                    {
                        _result.code = 0;
                        _result.msg = "参数错误";
                    }
                    else
                    {
                        DishQueueUp updateModel = DishQueueUpBLL.SingleModel.GetModel(model.id);
                        if (updateModel != null)
                        {
                            updateModel.state = (int)DishEnums.QueueUpEnums.已删除;
                            bool updateResult = DishQueueUpBLL.SingleModel.Update(updateModel, "state");

                            _result.code = updateResult ? 1 : 0;
                            _result.msg = updateResult ? "删除成功" : "删除失败";
                        }
                        else
                        {
                            _result.code = 0;
                            _result.msg = "删除失败,排队记录不存在";
                        }
                    }
                }

                //叫号
                else if (act == "call")
                {
                    if (model.id <= 0)
                    {
                        _result.code = 0;
                        _result.msg = "参数错误";
                    }
                    else
                    {
                        List<DishQueueUp> callModels = DishQueueUpBLL.SingleModel.GetCalls(model.q_catid, model.q_renshu);
                        if (callModels?.Count > 0)
                        {
                            callModels.ForEach(c =>
                            {
                                c.q_is_tongzhi = 1;
                                DishQueueUpBLL.SingleModel.Update(c, "q_is_tongzhi");

                                //TODO:abel读取模板消息通知客户
                            });
                            _result.code = 1;
                            _result.msg = "叫号成功";
                            return Json(_result);
                        }
                        else
                        {
                            _result.code = 0;
                            _result.msg = "叫号失败,队列无人";
                            return Json(_result);
                        }
                    }
                }
            }
            return Json(_result);
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="length">随机码个数,默认5个</param>
        /// <returns></returns>
        public string CreateRandomCode(int length = 5)
        {
            int rand;
            char code;
            string randomcode = String.Empty;

            //生成一定长度的验证码
            System.Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                rand = random.Next();

                if (rand % 3 == 0)
                {
                    code = (char)('A' + (char)(rand % 26));
                }
                else
                {
                    code = (char)('0' + (char)(rand % 10));
                }

                randomcode += code.ToString();
            }
            return randomcode;
        }

        /// <summary>
        /// 创建随机码图片
        /// </summary>
        /// <param name="randomcode">随机码</param>
        public void CreateImage(string randomcode)
        {
            int randAngle = 45; //随机转动角度
            int mapwidth = (int)(randomcode.Length * 16);
            Bitmap map = new Bitmap(mapwidth, 22);//创建图片背景
            Graphics graph = Graphics.FromImage(map);
            graph.Clear(Color.AliceBlue);//清除画面，填充背景
            graph.DrawRectangle(new Pen(Color.Black, 0), 0, 0, map.Width - 1, map.Height - 1);//画一个边框
                                                                                              //graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//模式

            Random rand = new Random();

            //背景噪点生成
            Pen blackPen = new Pen(Color.LightGray, 0);
            for (int i = 0; i < 50; i++)
            {
                int x = rand.Next(0, map.Width);
                int y = rand.Next(0, map.Height);
                graph.DrawRectangle(blackPen, x, y, 1, 1);
            }

            //验证码旋转，防止机器识别
            char[] chars = randomcode.ToCharArray();//拆散字符串成单字符数组

            //文字距中
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            //定义颜色
            Color[] c = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            //定义字体
            string[] font = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体" };
            int cindex = rand.Next(7);

            for (int i = 0; i < chars.Length; i++)
            {
                int findex = rand.Next(5);

                Font f = new System.Drawing.Font(font[findex], 14, System.Drawing.FontStyle.Bold);//字体样式(参数2为字体大小)
                Brush b = new System.Drawing.SolidBrush(c[cindex]);

                Point dot = new Point(14, 14);
                //graph.DrawString(dot.X.ToString(),fontstyle,new SolidBrush(Color.Black),10,150);//测试X坐标显示间距的
                float angle = rand.Next(-randAngle, randAngle);//转动的度数

                graph.TranslateTransform(dot.X, dot.Y);//移动光标到指定位置
                graph.RotateTransform(angle);
                graph.DrawString(chars[i].ToString(), f, b, 1, 1, format);
                //graph.DrawString(chars[i].ToString(),fontstyle,new SolidBrush(Color.Blue),1,1,format);
                graph.RotateTransform(-angle);//转回去
                graph.TranslateTransform(-2, -dot.Y);//移动光标到指定位置，每个字符紧凑显示，避免被软件识别
            }
            //生成图片
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            Response.Clear();
            Response.ContentType = "image/gif";
            Response.BinaryWrite(ms.ToArray());
            graph.Dispose();
            map.Dispose();
        }

        public ActionResult NotifySetting(int aId = 0, int storeId = 0)
        {
            if (aId <= 0 || storeId <= 0)
            {
                return Content("参数错误");
            }
            string appId = Senparc.Weixin.Config.SenparcWeixinSetting.WeixinAppId;//公众号appid
            DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
            if (store == null)
            {
                return Content("信息错误，请重新登录");
            }
            UserBaseInfo baseInfo = null;
            
            if (!string.IsNullOrEmpty(store.notifyOpenId))
            {
                baseInfo = UserBaseInfoBLL.SingleModel.GetModelByOpenId(store.notifyOpenId, WebSiteConfig.DZ_WxSerId);
            }
            string sessonid = new Random().Next(1, 3) + DateTime.Now.ToString("mmssfffff");
            Session["wxbindqrcodekey"] = sessonid;
            if (null == RedisUtil.Get<LoginQrCode>("SessionID:" + sessonid))
            {
                LoginQrCode model = new LoginQrCode();
                RedisUtil.Set<LoginQrCode>("SessionID:" + sessonid, model, TimeSpan.FromDays(1));
            }
            ViewBag.StoreId = storeId;
            return View(baseInfo);
        }
        /// <summary>
        /// 扫码监听
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckScan(string wxkey = "", int storeId = 0)
        {
            DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
            if (store == null)
            {
                _result.msg = "信息错误，请重新登录" + storeId;
                return Json(_result);
            }
            if (string.IsNullOrEmpty(wxkey))
            {
                _result.msg = "wxkey为空";
                return Json(_result);
            }
            LoginQrCode lcode = RedisUtil.Get<LoginQrCode>("SessionID:" + wxkey);
            if (lcode == null || lcode.WxUser == null)
            {
                _result.msg = "未扫码";
                return Json(_result);
            }

            _result.obj = new { userInfo = new { lcode.WxUser.headimgurl, lcode.WxUser.nickname } };
            store.notifyOpenId = lcode.WxUser.openid;
            if (DishStoreBLL.SingleModel.Update(store, "notifyopenid"))
            {
                _result.msg = "绑定成功";
                _result.code = 1;
            }
            else
            {
                _result.msg = "绑定失败";
            }
            return Json(_result);
        }
        /// <summary>
        /// 取消微信绑定
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult CancelBind(int storeId = 0)
        {
            DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
            if (store == null)
            {
                _result.msg = "信息错误，请重新登录" + storeId;
                return Json(_result);
            }
            store.notifyOpenId = string.Empty;
            if (DishStoreBLL.SingleModel.Update(store, "notifyopenid"))
            {
                _result.msg = "取消成功";
                _result.code = 1;
            }
            else
            {
                _result.msg = "取消失败";
            }
            return Json(_result);
        }

    }
}