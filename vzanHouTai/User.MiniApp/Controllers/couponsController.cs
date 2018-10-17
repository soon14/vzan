using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp;
using Core.MiniApp;
using Newtonsoft.Json;
using User.MiniApp.Comment;
using User.MiniApp.Model;
using Entity.MiniApp;
using System.IO;
using System.Text;
using Utility.AliOss;
using System.Web.Script.Serialization;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using DAL.Base;
using User.MiniApp.Filters;
using MySql.Data.MySqlClient;
using BLL.MiniApp.Ent;
using Utility.IO;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Fds;
using BLL.MiniApp.Fds;
using Entity.MiniApp.Stores;
using Entity.MiniApp.FunctionList;
using BLL.MiniApp.FunList;
using Entity.MiniApp.Plat;
using BLL.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using BLL.MiniApp.PlatChild;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;

namespace User.MiniApp.Controllers
{
    public class couponsController : baseController
    {
        public couponsController()
        {
        }
        //优惠券列表管理
        [LoginFilter]
        [RouteAuthCheck]
        public ActionResult CouponsList(int appId, int PageType)
        {
            int couponstate = Context.GetRequestInt("couponstate", 0);
            //int appId =  Context.GetRequestInt("appId", 0);
            //int PageType =  Context.GetRequestInt("PageType", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string couponname = Context.GetRequest("couponname", string.Empty);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
           
            if (appId <= 0)
            {
                return Redirect("dzhome/login");
            }

            if (PageType <= 0)
            {
                return Redirect("dzhome/login");
            }

            #region 专业版 版本控制

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            int CouponsSwtich = 0;//优惠券开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                PageType = xcxTemplate.Type;
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }

                if (!string.IsNullOrEmpty(functionList.ComsConfig))
                {
                    ComsConfig comsConfig = JsonConvert.DeserializeObject<ComsConfig>(functionList.ComsConfig);
                    CouponsSwtich = comsConfig.Coupons;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.CouponsSwtich = CouponsSwtich;
            #endregion


            string erromsg = string.Empty;
            string userids = "";
            string aids = "";
            int storeid = XcxAppAccountRelationBLL.SingleModel.ReturnStoreIdByAId(appId, ref erromsg, ref aids, ref userids);
            if (erromsg != null && erromsg.Length > 0)
            {
                return View("PageError", new Return_Msg() { Msg = erromsg, code = "500" });
            }
            List<Coupons> list = new List<Coupons>();
            try
            {

                list = CouponsBLL.SingleModel.GetCouponList(couponname, couponstate, storeid, appId, TicketType.优惠券, pageSize, pageIndex, "addtime desc");
                if (list != null && list.Count > 0)
                {
                    string couponids = string.Join(",", list.Select(s => s.Id).Distinct());
                    List<CouponLog> loglist = CouponLogBLL.SingleModel.GetList($"couponid in ({couponids})");

                    foreach (Coupons item in list)
                    {
                        //领取记录
                        List<CouponLog> temploglist = loglist?.Where(w => w.CouponId == item.Id).ToList();
                        if (temploglist != null && temploglist.Count > 0)
                        {
                            //库存
                            item.RemNum = item.CreateNum - temploglist.Count;
                            item.CouponNum = temploglist.Count;
                            var tempuserlist = temploglist.GroupBy(g => g.UserId).ToList();
                            //多少人领取
                            item.PersonNum = tempuserlist != null && tempuserlist.Count > 0 ? tempuserlist.Count : 0;
                            //已使用
                            List<CouponLog> tempuselist = temploglist.Where(w => w.State == 1).ToList();
                            item.UseNum = tempuselist != null && tempuselist.Count > 0 ? tempuselist.Count : 0;
                        }
                        else
                        {
                            //库存
                            item.RemNum = item.CreateNum;
                        }
                    }
                }

                ViewBag.TotalCount = CouponsBLL.SingleModel.GetCouponListCount(couponname, couponstate, storeid, appId, TicketType.优惠券);
                ViewBag.PageType = PageType;
                ViewBag.appId = appId;
                ViewBag.StoreId = storeid;
                ViewBag.pageSize = pageSize;
                ViewBag.couponstate = couponstate;
                ViewBag.couponname = couponname;
                ViewBag.SouceFrom = souceFrom;
            }
            catch (Exception)
            {

            }

            return View(list);
        }

        /// 使优惠券失效
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult DeleteCoupons()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "id不能小于0" });
            }

            Coupons model = CouponsBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                return Json(new { isok = false, msg = "找不到优惠券，请刷新重试" });
            }

            model.State = (int)CouponState.已失效;
            model.UpdateTime = DateTime.Now;
            if (CouponsBLL.SingleModel.Update(model, "state,updatetime"))
            {
                return Json(new { isok = true, msg = "保存成功" });
            }
            else
                return Json(new { isok = true, msg = "保存失败" });
        }

        //添加/编辑优惠券
        [LoginFilter]
        [RouteAuthCheck]
        public ActionResult AddOrEdit()
        {
            int appId = Context.GetRequestInt("appId", 0);


            if (appId == 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (xcx == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "403" });
            }

            int PageType = Context.GetRequestInt("PageType", 0);
            int couponid = Context.GetRequestInt("couponid", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);

            Coupons viewModel = null;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            if (couponid > 0)
            {
                viewModel = CouponsBLL.SingleModel.GetModel(couponid);
                if (null == viewModel)
                    return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
                viewModel.LimitMoneyType = viewModel.LimitMoney > 0 ? 1 : 0;
                if (!string.IsNullOrEmpty(viewModel.GoodsIdStr))
                {
                    viewModel.SelectGoods = new List<object>();
                    switch (PageType)
                    {
                        case (int)TmpType.小程序足浴模板:
                        case (int)TmpType.小程序专业模板:
                            List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetListByIds(viewModel.GoodsIdStr);
                            foreach (EntGoods item in goodslist)
                            {
                                viewModel.SelectGoods.Add(new
                                {
                                    Id = item.id,
                                    ImgUrl = item.img,
                                    GoodsName = item.name,
                                    showtime = item.addtime.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sel = false
                                });
                            }

                            break;
                        case (int)TmpType.小程序电商模板:
                            //默认返回电商版
                            List<StoreGoods> list = StoreGoodsBLL.SingleModel.GetListByIds(viewModel.GoodsIdStr);
                            foreach (StoreGoods item in list)
                            {
                                viewModel.SelectGoods.Add(new
                                {
                                    Id = item.Id,
                                    ImgUrl = item.ImgUrl,
                                    GoodsName = item.GoodsName,
                                    showtime = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sel = false
                                });
                            }
                            break;
                        case (int)TmpType.小程序餐饮模板:
                            //默认返回电商版
                            List<FoodGoods> foodlist = FoodGoodsBLL.SingleModel.GetListByIds(viewModel.GoodsIdStr);
                            foreach (FoodGoods item in foodlist)
                            {
                                viewModel.SelectGoods.Add(new
                                {
                                    Id = item.Id,
                                    ImgUrl = item.ImgUrl,
                                    GoodsName = item.GoodsName,
                                    showtime = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sel = false
                                });
                            }
                            break;

                    }
                }
            }
            else
            {
                viewModel = new Coupons() { StoreId = storeId, CreateNum = 20 };
            }
            ViewBag.SouceFrom = souceFrom;
            viewModel.appId = appId;
            viewModel.showTipCount = CouponsBLL.SingleModel.GetShowTipCount(appId);

            List<VipLevel> levelList = VipLevelBLL.SingleModel.GetList($"appid='{xcx.AppId}' and state>=0");
            ViewBag.levelList = levelList;

            return View(viewModel);
        }

        /// <summary>
        /// add coupon添加优惠券
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult addorupdatecoupon(Coupons coupon)
        {

            if (coupon == null || coupon.appId <= 0)
                return Json(new { isok = false, msg = "参数错误" });

            int appId = coupon.appId;
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (app == null)
                return Json(new { isok = false, msg = "没开通模板" });

            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return Json(new { isok = false, msg = "小程序模板不存在" });

            int showTipCount = CouponsBLL.SingleModel.GetShowTipCount(appId, coupon.Id);

            if (showTipCount >= 3 && coupon.IsShowTip == 1)
            {
                return Json(new { isok = false, msg = "只允许设置3张优惠券弹窗显示" });
            }

            #region 专业版 版本控制
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {

                FunctionList functionList = new FunctionList();
                int versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return Json(new { isok = false, msg = "此功能未开启" });
                }

                MarketingPlugin marketingPlugin = new MarketingPlugin();
                if (!string.IsNullOrEmpty(functionList.MarketingPlugin))
                {
                    marketingPlugin = JsonConvert.DeserializeObject<MarketingPlugin>(functionList.MarketingPlugin);
                }
                if (marketingPlugin.ReductionCard == 1)//表示关闭了立减金功能
                {
                    return Json(new { isok = false, msg = "请升级更高版本才能使用此功能！" });
                }

            }
            #endregion

            //表单验证
            if (coupon.CreateNum > 999 || coupon.CreateNum < 1)
            {
                return Json(new { isok = false, msg = "优惠券的生成数量为1-999！" }, JsonRequestBehavior.AllowGet);
            }
            //固定日期
            if (coupon.Id <= 0)
            {
                if (coupon.ValType == 0)
                {
                    if (coupon.EndUseTime < coupon.StartUseTime)
                    {
                        return Json(new { isok = false, msg = "优惠券过期日期必须大于生效日期！" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (coupon.ValDay <= 0)
                {
                    return Json(new { isok = false, msg = "优惠券生效日期不能为空！" }, JsonRequestBehavior.AllowGet);
                }
            }


            if (coupon.Money <= 0)
            {
                return Json(new { isok = false, msg = coupon.CouponWay == 0 ? "优惠金额必须0！" : "优惠折扣必须大于0" }, JsonRequestBehavior.AllowGet);
            }
            else if (coupon.CouponWay == 1 && coupon.Money > 99 * 100)
            {
                return Json(new { isok = false, msg = "优惠折扣不能超过9.9折" }, JsonRequestBehavior.AllowGet);
            }

            bool result = false;
            #region Base64解密
            try
            {
                if (!string.IsNullOrEmpty(coupon.Desc))
                {
                    string strDescription = coupon.Desc.Replace(" ", "+");
                    byte[] bytes = Convert.FromBase64String(strDescription);
                    coupon.Desc = System.Text.Encoding.UTF8.GetString(bytes);
                }

                coupon.UpdateTime = DateTime.Now;

                if (coupon.Id > 0)
                {
                    Coupons oldModel = CouponsBLL.SingleModel.GetModel(coupon.Id);
                    if (null == oldModel)
                        return Json(new { isok = false, msg = "优惠券出错，请刷新重试！" }, JsonRequestBehavior.AllowGet);

                    //已领取份数
                    int couponUserNum = 0;
                    List<CouponLog> loglist = CouponLogBLL.SingleModel.GetList("couponid=" + coupon.Id + "  and state!=4 ");
                    if (loglist != null && loglist.Any())
                    {
                        couponUserNum = loglist.GroupBy(g => g.FromOrderId).Count();
                    }

                    if (couponUserNum > coupon.CreateNum)
                    {
                        return Json(new { isok = false, msg = $"当前优惠券已被领取{couponUserNum}份 , 生成数量不能小于已领取数量！" }, JsonRequestBehavior.AllowGet);
                    }
                    // log4net.LogHelper.WriteInfo(this.GetType(),JsonConvert.SerializeObject(coupon));


                    string columnField = "couponname,CreateNum,UpdateTime,desc,IsShowTip,discountType,WxCouponsCardOpen";

                    //生成微信卡包对应的优惠券
                    if (string.IsNullOrEmpty(coupon.WxCouponsCardId) && coupon.WxCouponsCardOpen == 1)
                    {
                        CreateCardResult createCardResult = CouponsBLL.SingleModel.AddWxCoupons(coupon, app, dzaccount.Id.ToString());
                        if (createCardResult != null && createCardResult.errcode == 0)
                        {
                            columnField += ",WxCouponsCardId,WxCouponsCardOpenResult";
                            coupon.WxCouponsCardId = createCardResult.card_id;
                            coupon.WxCouponsCardOpenResult = $"同步成功微信优惠券ID:{createCardResult.card_id}";
                        }
                        else
                        {
                            columnField += ",WxCouponsCardOpenResult";
                            coupon.WxCouponsCardOpenResult = $"同步失败原因{createCardResult.errcode}:{createCardResult.errmsg}";
                        }
                    }

                    result = CouponsBLL.SingleModel.Update(coupon, columnField);
                }
                else
                {
                    coupon.AddTime = DateTime.Now;
                    //添加优惠券

                    if (coupon.WxCouponsCardOpen == 1)
                    {
                        CreateCardResult createCardResult = CouponsBLL.SingleModel.AddWxCoupons(coupon, app, dzaccount.Id.ToString());
                        if (createCardResult != null && createCardResult.errcode == 0)
                        {
                            coupon.WxCouponsCardId = createCardResult.card_id;
                            coupon.WxCouponsCardOpenResult = $"同步成功微信优惠券ID:{createCardResult.card_id}";
                        }
                        else
                        {
                            coupon.WxCouponsCardOpenResult = $"同步失败原因:{createCardResult.errmsg}";
                        }

                    }


                    int id = coupon.Id = Convert.ToInt32(CouponsBLL.SingleModel.Add(coupon));
                    result = id > 0;
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = "系统繁忙！" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            return Json(new { isok = result, msg = result ? "保存成功！" : "系统繁忙" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 社交立减金
        /// </summary>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult ReductionCardList(int appId, int PageType)
        {
            int couponstate = Context.GetRequestInt("couponstate", -1);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string couponname = Context.GetRequest("couponname", string.Empty);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            #region 专业版 版本控制
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            PageType = xcxTemplate.Type;
            int reductionCardSwtich = 0;//社交立减金开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.NewsMgr))
                {
                    MarketingPlugin marketingPlugin = JsonConvert.DeserializeObject<MarketingPlugin>(functionList.MarketingPlugin);
                    reductionCardSwtich = marketingPlugin.ReductionCard;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.reductionCardSwtich = reductionCardSwtich;
            #endregion

            int storeId = 0;
            switch (PageType)
            {
                case (int)TmpType.小程序餐饮模板:
                    Food store_Food = FoodBLL.SingleModel.GetModel($" appId = {appId} ");
                    if (store_Food == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
                    }
                    storeId = store_Food.Id;
                    break;
                case (int)TmpType.小程序多门店模板:
                    FootBath store_MultiStore = FootBathBLL.SingleModel.GetModel($" appId = {appId} and HomeId = 0 ");
                    if (store_MultiStore == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
                    }
                    storeId = store_MultiStore.Id;
                    break;
                case (int)TmpType.小程序专业模板:
                    Store store = StoreBLL.SingleModel.GetModelByRid(appId);
                    if (store == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
                    }
                    storeId = store.Id;
                    break;
                case (int)TmpType.小未平台子模版:
                    PlatStore platStore = PlatStoreBLL.SingleModel.GetModelByAId(appId);
                    if (platStore == null)
                    {
                        return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
                    }
                    storeId = platStore.Id;
                    break;
                default:
                    return View("PageError", new Return_Msg() { Msg = "找不到店铺!", code = "500" });
            }

            List<Coupons> list = new List<Coupons>();
            list = CouponsBLL.SingleModel.GetCouponList(couponname, couponstate, storeId, appId, TicketType.立减金, pageSize, pageIndex, "addtime desc");
            if (list != null && list.Count > 0)
            {
                string couponids = string.Join(",", list.Select(s => s.Id).Distinct());
                List<CouponLog> loglist = CouponLogBLL.SingleModel.GetList($"couponid in ({couponids}) and state!=4");

                foreach (Coupons item in list)
                {
                    item.StateStr = CouponsBLL.SingleModel.GetStateName(item);
                    //领取记录
                    List<CouponLog> temploglist = loglist?.Where(w => w.CouponId == item.Id).ToList();
                    if (temploglist != null && temploglist.Count > 0)
                    {
                        //已领取份数
                        int orderCount = temploglist.GroupBy(g => g.FromOrderId).Count();

                        //库存
                        item.RemNum = item.CreateNum - orderCount;
                        item.CouponNum = temploglist.Count;
                        var tempuserlist = temploglist.GroupBy(g => g.UserId).ToList();
                        //多少人领取
                        item.PersonNum = tempuserlist != null && tempuserlist.Count > 0 ? tempuserlist.Count : 0;
                        //已使用
                        List<CouponLog> tempuselist = temploglist.Where(w => w.State == 1).ToList();
                        item.UseNum = tempuselist != null && tempuselist.Count > 0 ? tempuselist.Count : 0;
                    }
                    else
                    {
                        //库存
                        item.RemNum = item.CreateNum;
                    }
                }
            }

            ViewBag.TotalCount = CouponsBLL.SingleModel.GetCouponListCount(couponname, couponstate, storeId, appId, TicketType.立减金);
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            ViewBag.StoreId = storeId;
            ViewBag.pageSize = pageSize;
            ViewBag.couponstate = couponstate;
            ViewBag.couponname = couponname;

            return View(list);
        }

        /// <summary>
        /// 添加/编辑 立减金
        /// </summary>
        /// <returns></returns>
        [RouteAuthCheck]
        public ActionResult AddOrEditReductionCard()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int PageType = Context.GetRequestInt("PageType", 0);
            int couponid = Context.GetRequestInt("couponid", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;

            #region 专业版 版本控制
            if (appId <= 0)
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });

            if (dzaccount == null)
                return Redirect("/dzhome/login");
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());

            if (app == null)
                return View("PageError", new Return_Msg() { Msg = "小程序未授权!", code = "403" });
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel($"id={app.TId}");
            if (xcxTemplate == null)
                return View("PageError", new Return_Msg() { Msg = "小程序模板不存在!", code = "500" });

            PageType = xcxTemplate.Type;
            int reductionCardSwtich = 0;//社交立减金开关
            int versionId = 0;
            if (xcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                FunctionList functionList = new FunctionList();
                versionId = app.VersionId;
                functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={xcxTemplate.Type} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "功能权限未设置!", code = "500" });
                }
                if (!string.IsNullOrEmpty(functionList.NewsMgr))
                {
                    MarketingPlugin marketingPlugin = JsonConvert.DeserializeObject<MarketingPlugin>(functionList.MarketingPlugin);
                    reductionCardSwtich = marketingPlugin.ReductionCard;
                }

            }
            ViewBag.versionId = versionId;
            ViewBag.reductionCardSwtich = reductionCardSwtich;
            #endregion

            Coupons viewModel = null;
            ViewBag.PageType = PageType;
            ViewBag.appId = appId;
            if (couponid > 0)
            {
                viewModel = CouponsBLL.SingleModel.GetModel(couponid);
                if (null == viewModel)
                    return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
                viewModel.LimitMoneyType = viewModel.LimitMoney > 0 ? 1 : 0;
                if (!string.IsNullOrEmpty(viewModel.GoodsIdStr))
                {
                    viewModel.SelectGoods = new List<object>();

                    int int_TryParseId = 0;
                    switch (PageType)
                    {
                        case (int)TmpType.小程序餐饮模板:
                            Food store_Food = FoodBLL.SingleModel.GetModelByAppId(appId);
                            if (store_Food != null && !string.IsNullOrWhiteSpace(viewModel.GoodsIdStr))
                            {
                                //处理字符串,取有效数据Id
                                List<string> idStrs = viewModel.GoodsIdStr.Split(',').Where(g => !string.IsNullOrWhiteSpace(g)
                                                                                && Int32.TryParse(g, out int_TryParseId))?.ToList();

                                if (idStrs != null && idStrs.Any())
                                {
                                    List<FoodGoods> goods_Food = FoodGoodsBLL.SingleModel.GetList($"id in ({string.Join(",", idStrs)})");
                                    foreach (FoodGoods item in goods_Food)
                                    {
                                        viewModel.SelectGoods.Add(new
                                        {
                                            Id = item.Id,
                                            ImgUrl = item.ImgUrl,
                                            GoodsName = item.GoodsName,
                                            showtime = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                            sel = false
                                        });
                                    }
                                }
                            }
                            break;

                        case (int)TmpType.小程序多门店模板:
                        case (int)TmpType.小程序专业模板:
                            List<EntGoods> goodslist = EntGoodsBLL.SingleModel.GetList($"id in ({viewModel.GoodsIdStr})");
                            foreach (EntGoods item in goodslist)
                            {
                                viewModel.SelectGoods.Add(new
                                {
                                    Id = item.id,
                                    ImgUrl = item.img,
                                    GoodsName = item.name,
                                    showtime = item.addtime.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sel = false
                                });
                            }
                            break;

                        case (int)TmpType.小未平台子模版:
                            List<PlatChildGoods> platChildGoodsList = PlatChildGoodsBLL.SingleModel.GetListByIds(viewModel.GoodsIdStr);
                            foreach (PlatChildGoods item in platChildGoodsList)
                            {
                                viewModel.SelectGoods.Add(new
                                {
                                    Id = item.Id,
                                    ImgUrl = item.Img,
                                    GoodsName = item.Name,
                                    showtime = item.Addtime.ToString("yyyy-MM-dd HH:mm:ss"),
                                    sel = false
                                });
                            }
                            break;
                    }
                }
            }
            else
            {
                viewModel = new Coupons() { StoreId = storeId, CreateNum = 20, TicketType = (int)TicketType.立减金 };
            }
            viewModel.appId = appId;
            return View(viewModel);
        }

        /// <summary>
        /// 开启/关闭立减金 
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenOrClose(int storeId = 0)
        {
            int appId = Context.GetRequestInt("appId", 0);
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "id不能小于0" });
            }

            var model = CouponsBLL.SingleModel.GetModelByIdAndAppId(id, appId);
            if (model == null)
            {
                return Json(new { isok = false, msg = "找不到优惠券，请刷新重试" });
            }
            int state = Context.GetRequestInt("state", 0);
            model.UpdateTime = DateTime.Now;
            switch (state)
            {
                case (int)CouponState.已关闭:
                    model.State = state;
                    break;
                case (int)CouponState.已开启:
                    model.State = state;
                    //立减金只能有一个开启
                    if (CouponsBLL.SingleModel.GetOpenedModelByState(appId, storeId) != null)
                    {
                        return Json(new { isok = false, msg = "已有开启的立减金，请先关闭该立减金" });
                    }
                    break;
                default:
                    return Json(new { isok = false, msg = "状态异常" });

            }
            if (CouponsBLL.SingleModel.Update(model, "state,updatetime"))
            {
                return Json(new { isok = true, msg = "修改成功" });
            }
            else
                return Json(new { isok = true, msg = "修改失败" });
        }
    }
}