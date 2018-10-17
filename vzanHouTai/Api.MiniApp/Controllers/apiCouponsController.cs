using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Entity.MiniApp.Tools;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Conf;
using Api.MiniApp.Models;
using BLL.MiniApp.Tools;
using Utility.IO;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Ent;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using BLL.MiniApp.Footbath;
using Newtonsoft.Json;
using Entity.MiniApp.Fds;
using BLL.MiniApp.Fds;
using Api.MiniApp.Filters;
using Entity.MiniApp.Plat;
using BLL.MiniApp.Plat;
using Entity.MiniApp.Dish;
using BLL.MiniApp.Dish;

namespace Api.MiniApp.Controllers
{
    public class apiCouponsController : InheritController
    {
    }

    [ExceptionLog]
    public class apiMiniAppCouponsController : apiCouponsController
    {
        private static readonly object CutPriceLocker = new object();
        private static readonly object GroupLocker = new object();
        private static readonly object lockgetcoupon = new object();
        //立减金
        private static readonly object lockgetReductionCard = new object();
        /// <summary>
        /// 实例化对象
        /// </summary>
        public apiMiniAppCouponsController()
        {

        }

        /// <summary>
        /// 获取我的优惠券列表
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post"), ApiOAuthAppId]
        public ActionResult GetMyCouponList()
        {
            int userId = Context.GetRequestInt("userId", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            string goodsId = Context.GetRequest("goodsId", string.Empty);//商品ID，用于用户下单时获取可用优惠券，默认0获取我的优惠列表
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int state = Context.GetRequestInt("state", 0);//0：未使用，1：已使用，2：已过期,3：已失效，可使用：4
            int ticketType = Context.GetRequestInt("ticketType", 0);
            int storeId = Context.GetRequestInt("storeid", 0);
            int platStoreId = 0;
            if (storeId > 0)
            {
                platStoreId = storeId;
            }
            string goodsInfo = Context.GetRequest("goodsInfo", string.Empty);
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "未找到小程序授权资料" }, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxtemplat = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxtemplat == null)
            {
                return Json(new { isok = false, msg = "未找到模板" }, JsonRequestBehavior.AllowGet);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "未找到用户" }, JsonRequestBehavior.AllowGet);
            }

            string errMsg = string.Empty;
            string aids = "";
            string userids = userId.ToString();

            storeId = _xcxAppAccountRelationBLL.ReturnStoreIdByAId(xcxrelation.Id, ref errMsg, ref aids, ref userids, userInfo.TelePhone);

            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                return Json(new { isok = false, errMsg }, JsonRequestBehavior.AllowGet);
            }

            List<CouponLog> couponloglist = CouponLogBLL.SingleModel.GetListByApi(state, userids, storeId, aids, 10, pageIndex, "p.storeid desc,l.addtime desc", goodsId, goodsInfo);

            switch (xcxtemplat.Type)
            {
                case (int)TmpType.小未平台:
                    List<PlatCoupons> platpostdata = new List<PlatCoupons>();
                    string storeids = string.Join(",", couponloglist?.Select(s => s.StoreId).Distinct());
                    //下单时，获取优惠券应该根据storeId获取，避免获取别的店铺优惠券
                    if (platStoreId > 0)
                    {
                        storeids = platStoreId.ToString();
                    }
                    List<PlatStore> platstorelist = PlatStoreBLL.SingleModel.GetListByIds(storeids);
                    if (platstorelist != null && platstorelist.Count > 0)
                    {
                        foreach (PlatStore item in platstorelist)
                        {
                            PlatCoupons platcoupon = new PlatCoupons();
                            platcoupon.StoreId = item.Id;
                            platcoupon.AId = item.Aid;
                            platcoupon.StoreName = item.Name;

                            List<CouponLog> temploglist = couponloglist.Where(w => w.StoreId == item.Id).ToList();
                            platcoupon.couponloglist = temploglist;

                            platpostdata.Add(platcoupon);
                        }
                    }
                    return Json(new { isok = true, msg = "平台获取优惠券成功", postdata = platpostdata }, JsonRequestBehavior.AllowGet);
                default:
                    var postdata = couponloglist?.Select(s => new
                    {
                        s.Id,
                        s.CouponId,
                        s.CouponName,
                        EndUseTimeStr = s.EndUseTime.ToString("yyyy.MM.dd"),
                        StartUseTimeStr = s.StartUseTime.ToString("yyyy.MM.dd"),
                        State = state,
                        s.Money,
                        s.Money_fmt,
                        s.CouponWay,
                        s.LimitMoney,
                        s.LimitMoneyStr,
                        s.UserId,
                        s.ValType,
                        s.CanUse,
                        s.GoodsIdStr,
                        s.StoreId,
                        s.StoreName,
                        s.discountType
                    });

                    return Json(new { isok = true, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取店铺优惠券列表
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post"), ApiOAuthAppId]
        public ActionResult GetStoreCouponList()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int IsShowTip = Context.GetRequestInt("IsShowTip", 0);//是否首页弹窗显示
            int goodstype = Context.GetRequestInt("goodstype", -1);//是否指定商品有优惠
            int state = Context.GetRequestInt("state", 2);
            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "未找到小程序授权资料" }, JsonRequestBehavior.AllowGet);
            }

            string userids = "";
            string aids = "";
            string errMsg = string.Empty;
            //店铺ID
            int storeId = _xcxAppAccountRelationBLL.ReturnStoreIdByAId(xcxrelation.Id, ref errMsg, ref aids, ref userids);
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                return Json(new { isok = false, errMsg }, JsonRequestBehavior.AllowGet);
            }

            List<Coupons> couponlist = CouponsBLL.SingleModel.GetCouponList("", state, storeId, xcxrelation.Id, TicketType.优惠券, 10000, 1, "addtime desc", goodstype, IsShowTip);
            List<Coupons> showTipCouponlist = new List<Coupons>();
            if (couponlist?.Count > 0)
            {
                string couponids = string.Join(",", couponlist.Select(s => s.Id));
                List<CouponLog> userloglist = CouponLogBLL.SingleModel.GetListByIds(couponids);
                List<CouponLog> customerloglist = userloglist?.Where(w => w.UserId == userId).ToList();
                foreach (Coupons item in couponlist)
                {
                    List<CouponLog> tempuserlog = userloglist.Where(w => w.CouponId == item.Id).ToList();
                    if (tempuserlog?.Count > 0)
                    {
                        //库存
                        item.RemNum = item.CreateNum - tempuserlog.Count();
                    }
                    else
                    {
                        item.RemNum = item.CreateNum;
                    }
                
                    var tempuserlist = tempuserlog.GroupBy(g => g.UserId).ToList();
                    //多少人领取
                    item.PersonNum = tempuserlist != null && tempuserlist.Count > 0 ? tempuserlist.Count : 0;

                    //判断当前用户是否可以领取优惠券
                    List<CouponLog> tempcustomerlog = customerloglist.Where(w => w.CouponId == item.Id).ToList();
                    if ((item.LimitReceive > 0 && tempcustomerlog?.Count > 0 && tempcustomerlog?.Count >= item.LimitReceive) || item.RemNum <= 0)
                    {
                        item.CanGet = false;
                    }

                    if (item.CanGet)
                    {
                        showTipCouponlist.Add(item);
                    }
                }
            }

            if (IsShowTip == 1)
            {
                couponlist = showTipCouponlist;
            }

            var postdata = couponlist?.Select(s => new
            {
                s.Id,
                s.StoreId,
                s.CouponName,
                EndUseTimeStr = s.EndUseTime.ToString("yyyy.MM.dd"),
                StartUseTimeStr = s.StartUseTime.ToString("yyyy.MM.dd"),
                s.State,
                s.CreateNum,
                s.Desc,
                s.LimitMoney,
                s.LimitMoneyStr,
                s.LimitReceive,
                s.Money,
                Money_fmt = s.CouponWay == 0 ? (s.Money / 100).ToString() : (s.Money / 100.00).ToString(),
                s.RemNum,
                s.ValDay,
                s.ValType,
                VipLevel = s.VipLevel == 0 ? 1 : s.VipLevel,
                s.CouponWay,
                s.GoodsIdStr,
                s.CanGet,
                s.PersonNum
            });

            return Json(new { isok = true, msg = "成功", postdata = postdata }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("get", "post"), ApiOAuthAppId]
        public ActionResult GetCoupon()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            int couponId = Context.GetRequestInt("couponId", 0);
            int userId = Context.GetRequestInt("userId", 0);

            if (couponId <= 0)
            {
                return Json(new { isok = false, msg = "优惠券ID不能小于0" }, JsonRequestBehavior.AllowGet);
            }
            if (userId <= 0)
            {
                return Json(new { isok = false, msg = "用户ID不能小于0" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo usermodel = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (usermodel == null)
            {
                return Json(new { isok = false, msg = "找不到用户" }, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxrelation == null)
            {
                return Json(new { isok = false, msg = "未找到小程序授权资料" }, JsonRequestBehavior.AllowGet);
            }

            string userids = "";
            string aids = "";
            string errMsg = string.Empty;
            //店铺ID
            int storeId = _xcxAppAccountRelationBLL.ReturnStoreIdByAId(xcxrelation.Id, ref errMsg, ref aids, ref userids);
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                return Json(new { isok = false, errMsg }, JsonRequestBehavior.AllowGet);
            }

            lock (lockgetcoupon)
            {
                string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Coupons couponmodel = CouponsBLL.SingleModel.GetValidModel(couponId, nowtime);
                if (couponmodel == null)
                {
                    return Json(new { isok = false, msg = "找不到优惠券！" }, JsonRequestBehavior.AllowGet);
                }

                int couponlogcount = CouponLogBLL.SingleModel.GetCountByCouponId(couponId);
                if (couponlogcount >= couponmodel.CreateNum)
                {
                    return Json(new { isok = false, msg = "您下手太慢了，优惠券被抢光了！" }, JsonRequestBehavior.AllowGet);
                }

                //判断用户领取是否超过限制
                if (couponmodel.LimitReceive > 0)
                {
                    int usercouponlogcount = CouponLogBLL.SingleModel.GetCountByCouponIdAndUserId(couponId, userId);
                    if (usercouponlogcount >= couponmodel.LimitReceive)
                    {
                        return Json(new { isok = false, msg = "您已领取过了，快去使用！" }, JsonRequestBehavior.AllowGet);
                    }
                }

                //判断有会员等级限制
                if (couponmodel.VipLevel > 0)
                {
                    VipRelation viprelationmodel = VipRelationBLL.SingleModel.GetModelByRidAndUid(xcxrelation.AppId, userId);
                    if (viprelationmodel != null)
                    {
                        VipLevel viplevel = VipLevelBLL.SingleModel.GetModel(viprelationmodel.levelid);
                        if (viplevel == null || viplevel.level < couponmodel.VipLevel)
                        {
                            return Json(new { isok = false, msg = "您的会员等级不够，无法领取优惠券！" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                CouponLog couponlog = new CouponLog();
                couponlog.AddTime = DateTime.Now;
                couponlog.CouponId = couponmodel.Id;

                if (couponmodel.ValType == 0)
                {
                    //指定优惠券固定时间
                    couponlog.StartUseTime = couponmodel.StartUseTime;
                    couponlog.EndUseTime = couponmodel.EndUseTime;
                }
                else
                {
                    //领券时间
                    string getcouptime = DateTime.Now.ToShortDateString();
                    //1：领到券次日开始N天内有效，2：领到券当日开始N天内有效
                    couponlog.StartUseTime = DateTime.Parse(getcouptime + " 00:00:00").AddDays(couponmodel.ValType == 1 ? 1 : 0);
                    couponlog.EndUseTime = DateTime.Parse(getcouptime + " 23:59:59").AddDays(couponmodel.ValDay + (couponmodel.ValType == 1 ? 0 : -1));
                }

                couponlog.CouponName = couponmodel.CouponName;
                couponlog.UserId = userId;

                couponlog.Id = Convert.ToInt32(CouponLogBLL.SingleModel.Add(couponlog));

                return Json(new { isok = couponlog.Id > 0, msg = couponlog.Id > 0 ? "领取成功" : "领取失败" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 立减金-立即领取（不维护，用GetReductionCardV2）
        /// </summary>
        /// <returns></returns>
        [HttpPost, AuthLoginCheckXiaoChenXun]
        public ActionResult GetReductionCard()
        {
            Coupons model = null;
            int couponsId = Context.GetRequestInt("couponsId", 0);
            if (couponsId <= 0)
            {
                return Json(new { isok = false, msg = "我钥匙被弄丢了", coupon = model });
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "我是不存在的", coupon = model });
            }

            int aId = 0;
            Food store_Food = null;
            int orderType = Context.GetRequestInt("orderType", 0);//默认专业版,多门店版
            switch (orderType)
            {
                case 0://专业版,多门店版
                    EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
                    if (orderInfo == null)
                    {
                        return Json(new { isok = false, msg = "活动已过期", coupon = model });
                    }
                    aId = orderInfo.aId;
                    break;
                case 1://餐饮
                    FoodGoodsOrder orderInfo_Food = FoodGoodsOrderBLL.SingleModel.GetModel(orderId);
                    if (orderInfo_Food == null)
                    {
                        return Json(new { isok = false, msg = "活动已过期", coupon = model });
                    }
                    store_Food = FoodBLL.SingleModel.GetModel(orderInfo_Food.StoreId);
                    if (store_Food == null)
                    {
                        return Json(new { isok = false, msg = "活动已过期_", coupon = model });
                    }

                    aId = store_Food.appId;
                    break;
            }

            lock (lockgetReductionCard)
            {
                model = CouponsBLL.SingleModel.GetModelByIdAndAppId(couponsId, aId, (int)CouponState.已开启);
                if (model == null)
                {
                    return Json(new { isok = false, msg = "立减金已消失", coupon = model });
                }

                List<CouponLog> loglist = CouponLogBLL.SingleModel.GetList($"couponid in ({model.Id}) and state!=4");

                int userId = Context.GetRequestInt("userId", 0);
                if (userId <= 0)
                {
                    return Json(new { isok = false, msg = "id错误", coupon = model });
                }
                C_UserInfo user = C_UserInfoBLL.SingleModel.GetModel(userId);
                if (user == null)
                {
                    return Json(new { isok = false, msg = "用户不存在", coupon = model });
                }
                DateTime date = DateTime.Now;

                if ((DateTime.Compare(date, model.StartUseTime) > 0 && DateTime.Compare(date, model.EndUseTime) < 0 && model.ValType == 0) || model.ValType != 0)
                {
                    List<CouponLog> logList = CouponLogBLL.SingleModel.GetList($" CouponId={model.Id} and fromorderid={orderId} ") ?? new List<CouponLog>();
                    List<C_UserInfo> userList = new List<C_UserInfo>();
                    string userIds = string.Join(",", logList.Select(log => log.UserId).ToList());
                    if (logList.Count > 0)
                    {
                        userList = C_UserInfoBLL.SingleModel.GetList($"id in ( {userIds} )") ?? new List<C_UserInfo>();
                        model.StartUseTime = logList[0].StartUseTime;
                        model.EndUseTime = logList[0].EndUseTime;
                    }
                    List<CouponLog> userGetList = CouponLogBLL.SingleModel.GetList($"couponId={couponsId} and userid={user.Id}");

                    //已领取完
                    if (userList.Count >= model.SatisfyNum)
                    {
                        C_UserInfo userInfo = userList.Where(u => u.Id == user.Id).FirstOrDefault();
                        return Json(new { isok = true, coupon = model, userList = userList, userInfo = userInfo });
                    }


                    //未领取过这份立减金
                    if (logList.Where(log => log.UserId == user.Id).ToList().Count <= 0)
                    {
                        List<CouponLog> temploglist = loglist?.Where(w => w.CouponId == model.Id).ToList();
                        if (temploglist != null && temploglist.Count > 0)
                        {
                            //已领取份数
                            int orderCount = temploglist.GroupBy(g => g.FromOrderId).Count();
                            //库存是否足够
                            model.RemNum = model.CreateNum - orderCount;
                            if (model.RemNum <= 0)
                            {
                                return Json(new { isok = false, msg = "立减金已放送完毕,请关注下次的立减金优惠哦", coupon = model });
                            }
                        }
                        //超过领取限制
                        if (userGetList != null && userGetList.Count >= model.LimitReceive && model.LimitReceive > 0)
                        {
                            return Json(new { isok = false, msg = "你已超过领取限制", coupon = model });
                        }
                        bool isok = false;
                        CouponLog getCoupon = new CouponLog
                        {
                            CouponId = model.Id,
                            CouponName = model.CouponName,
                            FromOrderId = orderId,
                            UserId = user.Id,
                            State = 4,
                            StartUseTime = model.StartUseTime,
                            EndUseTime = model.EndUseTime,
                            AddTime = DateTime.Now,
                            StoreId = model.StoreId
                        };
                        getCoupon.Id = Convert.ToInt32(CouponLogBLL.SingleModel.Add(getCoupon));
                        isok = getCoupon.Id > 0;

                        if (isok)
                        {
                            logList.Add(getCoupon);
                            userList.Add(user);
                        }
                        else
                        {
                            return Json(new { isok = false, msg = "出了点小问题~", coupon = model });
                        }
                        //满足条件
                        if (userList.Count == model.SatisfyNum)
                        {

                            if (model.ValType == 1)
                            {
                                model.StartUseTime = Convert.ToDateTime(date.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));
                                model.EndUseTime = Convert.ToDateTime(date.AddDays(model.ValDay).ToString("yyyy-MM-dd 23:59:59"));

                            }
                            //领券当天有效
                            else if (model.ValType == 2)
                            {
                                model.StartUseTime = Convert.ToDateTime(date.ToString("yyyy-MM-dd 00:00:00"));
                                model.EndUseTime = Convert.ToDateTime(date.AddDays(model.ValDay - 1).ToString("yyyy-MM-dd 23:59:59"));
                            }
                            foreach (CouponLog log in logList)
                            {
                                log.StartUseTime = model.StartUseTime;
                                log.EndUseTime = model.EndUseTime;
                                log.State = 0;
                                CouponLogBLL.SingleModel.Update(log, "State,StartUseTime,EndUseTime");
                            }
                        }
                    }
                    return Json(new { isok = true, userList = userList, coupon = model, userInfo = user });
                }
            }

            return Json(new { isok = false, msg = "立减金Miss", coupon = model });
        }

        /// <summary>
        /// 获取未领取的立减金
        /// </summary>
        /// <returns></returns>
        public ActionResult GetReductionCardList()
        {
            int storeid = Context.GetRequestInt("storeId", 0);
            int userid = Context.GetRequestInt("userId", 0);
            int aid = Context.GetRequestInt("aid", 0);
            if (storeid <= 0)
            {
                return Json(new { isok = false, msg = "门店不存在" }, JsonRequestBehavior.AllowGet);
            }
            if (aid <= 0)
            {
                return Json(new { isok = false, msg = "门店错误" }, JsonRequestBehavior.AllowGet);
            }

            string userids = "";
            string aids = "";
            string errMsg = string.Empty;
            //店铺ID
            int storeId = _xcxAppAccountRelationBLL.ReturnStoreIdByAId(aid, ref errMsg, ref aids, ref userids);
            if (!string.IsNullOrWhiteSpace(errMsg))
            {
                return Json(new { isok = false, errMsg }, JsonRequestBehavior.AllowGet);
            }

            if (userid <= 0)
            {
                return Json(new { isok = false, msg = "用户id错误" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo user = C_UserInfoBLL.SingleModel.GetModel(userid);
            if (user == null)
            {
                return Json(new { isok = false, msg = "我找不到你的信息" }, JsonRequestBehavior.AllowGet);
            }
            List<Coupons> coupons = CouponsBLL.SingleModel.GetUnsatisfiedReductionCard(storeId, aid, userid);

            return Json(new { isok = true, coupons = coupons }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 立减金-立即领取
        /// </summary>
        /// <returns></returns>
        [HttpPost, AuthCheckLoginSessionKey]
        public ActionResult GetReductionCardV2()
        {
            returnObj = new Return_Msg_APP();
            int couponsId = Context.GetRequestInt("couponsId", 0);
            int orderId = Context.GetRequestInt("orderId", 0);
            int userId = Context.GetRequestInt("userId", 0);
            int aid = Context.GetRequestInt("aid", 0);
            returnObj.code = "0";

            if (couponsId <= 0)
            {
                returnObj.Msg = "无效优惠券参数";
                return Json(returnObj);
            }
            if (orderId <= 0)
            {
                returnObj.Msg = "无效订单参数";
                return Json(returnObj);
            }
            if (userId <= 0)
            {
                returnObj.Msg = "无效用户参数";
                return Json(returnObj);
            }
            C_UserInfo user = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (user == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            Coupons model = null;
            lock (lockgetReductionCard)
            {
                model = CouponsBLL.SingleModel.GetModelByIdAndAppId(couponsId, aid, (int)CouponState.已开启);
                if (model == null)
                {
                    returnObj.Msg = "立减金活动已过期";
                    return Json(returnObj);
                }

                List<CouponLog> allLogList = CouponLogBLL.SingleModel.GetListByIds(model.Id.ToString());
                DateTime date = DateTime.Now;
                if ((DateTime.Compare(date, model.StartUseTime) > 0 && DateTime.Compare(date, model.EndUseTime) < 0 && model.ValType == 0) || model.ValType != 0)
                {
                    List<CouponLog> logList = CouponLogBLL.SingleModel.GetListByOrderId(model.Id, orderId) ?? new List<CouponLog>();
                    if (logList.Count > 0)
                    {
                        model.StartUseTime = logList[0].StartUseTime;
                        model.EndUseTime = logList[0].EndUseTime;
                    }
                    else
                    {
                        if (allLogList != null)
                        {
                            //已领取份数
                            int orderCount = allLogList.GroupBy(g => g.FromOrderId).Count();
                            //库存是否足够
                            model.RemNum = model.CreateNum - orderCount;
                            if (model.RemNum <= 0)
                            {
                                returnObj.Msg = "立减金已放送完毕,请关注下次的立减金优惠哦";
                                return Json(returnObj);
                            }
                        }
                    }

                    string userIds = string.Join(",", logList.Select(log => log.UserId).ToList());
                    List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetListByIds(userIds) ?? new List<C_UserInfo>();
                    List<CouponLog> userGetList = CouponLogBLL.SingleModel.GetListByUserId(couponsId, user.Id);

                    //已领取完
                    if (userList.Count >= model.SatisfyNum)
                    {
                        C_UserInfo userInfo = userList.Where(u => u.Id == user.Id).FirstOrDefault();
                        returnObj.isok = true;
                        returnObj.dataObj = new { userList = userList, coupon = model, userInfo = user };
                        return Json(returnObj);
                    }

                    //未领取过这份立减金
                    if (logList.Where(log => log.UserId == user.Id).ToList().Count <= 0)
                    {
                        //超过领取限制
                        if (userGetList != null && userGetList.Count >= model.LimitReceive && model.LimitReceive > 0)
                        {
                            returnObj.Msg = "你已超过领取限制";
                            return Json(returnObj);
                        }
                        bool isok = false;
                        CouponLog getCoupon = new CouponLog
                        {
                            CouponId = model.Id,
                            CouponName = model.CouponName,
                            FromOrderId = orderId,
                            UserId = user.Id,
                            State = 4,
                            StartUseTime = model.StartUseTime,
                            EndUseTime = model.EndUseTime,
                            AddTime = DateTime.Now,
                            StoreId = model.StoreId,
                        };
                        getCoupon.Id = Convert.ToInt32(CouponLogBLL.SingleModel.Add(getCoupon));
                        isok = getCoupon.Id > 0;

                        if (isok)
                        {
                            logList.Add(getCoupon);
                            userList.Add(user);
                        }
                        else
                        {
                            returnObj.Msg = "请重新领取";
                            return Json(returnObj);
                        }
                        //满足条件
                        if (userList.Count == model.SatisfyNum)
                        {
                            if (model.ValType == 1)
                            {
                                model.StartUseTime = Convert.ToDateTime(date.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));
                                model.EndUseTime = Convert.ToDateTime(date.AddDays(model.ValDay).ToString("yyyy-MM-dd 23:59:59"));
                            }
                            //领券当天有效
                            else if (model.ValType == 2)
                            {
                                model.StartUseTime = Convert.ToDateTime(date.ToString("yyyy-MM-dd 00:00:00"));
                                model.EndUseTime = Convert.ToDateTime(date.AddDays(model.ValDay - 1).ToString("yyyy-MM-dd 23:59:59"));
                            }

                            string couponLogIds = string.Join(",", logList.Select(s => s.Id));
                            CouponLogBLL.SingleModel.UpdateCouponLogState(couponLogIds, 0, model.StartUseTime, model.EndUseTime);
                        }
                    }
                    returnObj.code = "1";
                    returnObj.isok = true;
                    returnObj.dataObj = new { userList = userList, coupon = model, userInfo = user };
                    return Json(returnObj);
                }
            }
            
            returnObj.Msg = "立减金丢失";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取未领取的立减金
        /// </summary>
        /// <returns></returns>
        [HttpPost, AuthCheckLoginSessionKey]
        public ActionResult GetReductionCardListV2()
        {
            returnObj = new Return_Msg_APP();
            int storeId = Context.GetRequestInt("storeId", 0);
            int userId = Context.GetRequestInt("userId", 0);
            int aid = Context.GetRequestInt("aid", 0);
            if (storeId <= 0)
            {
                returnObj.Msg = "无效店铺参数";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "无效模板参数";
                return Json(returnObj);
            }

            string errMsg = string.Empty;
            if (userId <= 0)
            {
                returnObj.Msg = "无效用户参数";
                return Json(returnObj);
            }
            C_UserInfo user = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (user == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }

            List<Coupons> coupons = CouponsBLL.SingleModel.GetUnsatisfiedReductionCard(storeId, aid, userId);

            returnObj.isok = true;
            returnObj.dataObj = coupons;
            return Json(returnObj);
        }
        /// <summary>
        /// 智慧餐厅-根据平台id(aid)获取所有门店未领取的立减金 只会查询开启的店铺的优惠券
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="state">0：未使用，1：已使用，2：已过期</param>
        /// <returns></returns>
        [HttpPost, AuthCheckLoginSessionKey]
        public ActionResult DishGetReductionCardList(int aid, int userId, int state = 0)
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "0";
            if (aid <= 0)
            {
                returnObj.Msg = "无效模板参数";
                return Json(returnObj);
            }

            string errMsg = string.Empty;
            if (userId <= 0)
            {
                returnObj.Msg = "无效用户参数";
                return Json(returnObj);
            }
            C_UserInfo user = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (user == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }
            List<PlatCoupons> list = new List<PlatCoupons>();
            List<int> storeIds = new List<int>();
            switch (state)
            {
                case 0://未使用
                    List<Coupons> coupons = CouponsBLL.SingleModel.GetUnsatisfiedReductionCard(aid, userId) ?? new List<Coupons>();//未领取的立减金

                    if (coupons.Count > 0)
                    {
                        storeIds.AddRange(coupons.Select(coupon => coupon.StoreId).ToList());
                    }
                    List<CouponLog> couponlogs = CouponLogBLL.SingleModel.GetCouponList(state, user.Id, -999, aid, 500, 1, "l.addtime desc", "", "") ?? new List<CouponLog>(); //已领取的未使用的优惠券
                    if (couponlogs.Count > 0)
                    {
                        storeIds.AddRange(couponlogs.Select(couponlog => couponlog.StoreId).ToList());
                    }
                    storeIds = storeIds.Distinct().ToList();
                    string storeIdsStr = string.Join(",",storeIds);
                    List<DishStore> dishStoreList = DishStoreBLL.SingleModel.GetListByIds(storeIdsStr);
                    if(dishStoreList!=null && dishStoreList.Count>0)
                    {
                        foreach (DishStore store in dishStoreList)
                        {
                            if (store.state > -1)
                            {
                                PlatCoupons coupon = new PlatCoupons()
                                {
                                    StoreLogo = store.dish_logo,
                                    StoreId = store.id,
                                    StoreName = store.dish_name,
                                    AId = store.aid,
                                    couponloglist = couponlogs.Where(couponlog => couponlog.StoreId == store.id).ToList(),
                                    couponList = coupons.Where(cou => cou.StoreId == store.id).ToList()
                                };
                                list.Add(coupon);
                            }
                        }
                    }
                    break;
                case 1://已使用
                case 2://已过期
                    couponlogs = CouponLogBLL.SingleModel.GetCouponList(state, user.Id, -999, aid, 500, 1, "l.addtime desc", "", "") ?? new List<CouponLog>(); //已领取的未使用的优惠券
                    if (couponlogs.Count > 0)
                    {
                        storeIds.AddRange(couponlogs.Select(couponlog => couponlog.StoreId).ToList());
                    }
                    storeIds = storeIds.Distinct().ToList();
                    foreach (var storeId in storeIds)
                    {
                        DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
                        if (store.state > -1)
                        {
                            PlatCoupons coupon = new PlatCoupons()
                            {
                                StoreLogo=store.dish_logo,
                                StoreId = store.id,
                                StoreName = store.dish_name,
                                AId = store.aid,
                                couponloglist = couponlogs.Where(couponlog => couponlog.StoreId == store.id).ToList(),
                            };
                            list.Add(coupon);
                        }
                    }
                    break;
                default:
                    returnObj.Msg = "参数错误 state error";
                    return Json(returnObj);
            }

            returnObj.code = "1";
            returnObj.isok = true;
            returnObj.dataObj = new { list };
            return Json(returnObj);
        }
    }
}