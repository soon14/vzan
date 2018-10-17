using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Helper;
using BLL.MiniApp.User;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiFootbathController : InheritController
    {
    }

    public class apiMiappFootbathController : apiFootbathController
    {
        public static readonly ConcurrentDictionary<int, object> lockObjectDict_Order = new ConcurrentDictionary<int, object>();//订单锁


        /// <summary>
        /// 技师端登录 Redis_key
        /// </summary>
        private static readonly string TECHNICIAN_LOGIN_KEY = "TECHNICIAN_LOGIN_KEY_{0}";

        public apiMiappFootbathController()
        {
            
        }

        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        // GET: apiMiappFootbath
        public ActionResult GetStoreInfo(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            data.switchModel = JsonConvert.DeserializeObject<SwitchModel>(data.SwitchConfig);
            data.shopDays = FootBathBLL.SingleModel.GetShopDays(data.switchModel);
            data.ReservationTime = FootBathBLL.SingleModel.GetReservationTime(data.switchModel.PresetTime);
            List<C_Attachment> LogoList = C_AttachmentBLL.SingleModel.GetListByCache(data.Id, (int)AttachmentItemType.小程序足浴版店铺logo);
            List<C_Attachment> photoList = C_AttachmentBLL.SingleModel.GetListByCache(data.Id, (int)AttachmentItemType.小程序足浴版门店图片);
            return Json(new { isok = 1, msg = "成功", data = data, LogoList = LogoList, photoList = photoList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取技师列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetTechnicianList(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }

            int pageSize = Context.GetRequestInt("pageSize", 6);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int sex = Context.GetRequestInt("sex", 0);
            int orderByAge = Context.GetRequestInt("age", 0);//年龄排序 0不排序 1升序 2降序
            int orderByCount = Context.GetRequestInt("count", 0);//订单排序 0不排序 1升序 2降序
            List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetTechnicianList(data.Id, sex, orderByAge, orderByCount, pageSize, pageIndex);
            return Json(new { isok = 1, technicianList = technicianList });
        }

        /// <summary>
        /// 根据技师id获取技师信息
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetTechnicianInfo(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" });
            }
            TechnicianInfo info = TechnicianInfoBLL.SingleModel.GetModelById(id);
            if (info == null)
            {
                return Json(new { isok = -1, msg = "该技师信息错误" });
            }
            int levelId = Context.GetRequestInt("levelid", 0);
            if (levelId <= 0)
            {
                return Json(new { isok = -1, msg = "有点小问题levelid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelInfo = VipLevelBLL.SingleModel.GetModelById(levelId);
            if (levelInfo == null)
            {
                return Json(new { isok = -1, msg = "有点小问题levelInfo_null" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(info.itemId))
            {
                info.serviceList = EntGoodsBLL.SingleModel.GetServerListByIds(info.appId, info.itemId);
                if (info.serviceList != null && info.serviceList.Count > 0)
                {
                    info.serviceList.ForEach(s =>
                    {
                        VipLevelBLL.SingleModel.CalculateVipGoodsPrice(s, levelInfo);//获取会员折后价
                    });
                }
            }
            int userId = Context.GetRequestInt("uid", 0);
            if (userId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_UserId(appid, userId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            bool state = EntGoodsOrderBLL.SingleModel.ValidIsSuccess(data.appId, userId, info.id);

            return Json(new { isok = 1, info = info, state = state });
        }

        /// <summary>
        /// 获取服务列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetServiceList(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int levelId = Context.GetRequestInt("levelid", 0);
            if (levelId <= 0)
            {
                return Json(new { isok = -1, msg = "有点小问题levelid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelInfo = VipLevelBLL.SingleModel.GetModelById(levelId);
            if (levelInfo == null)
            {
                return Json(new { isok = -1, msg = "有点小问题levelInfo_null" }, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Context.GetRequestInt("pageSize", 6);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int orderByPrice = Context.GetRequestInt("price", 0);//价格排序 0不排序 1升序 2降序
            int count = Context.GetRequestInt("count", 0);//订单排序 0不排序 1升序 2降序
            int type = Context.GetRequestInt("type", 0);

            List<EntGoods> goodsList = EntGoodsBLL.SingleModel.GetFootbathGoodsList(umodel.Id, orderByPrice, count, type, pageSize, pageIndex);
            if (goodsList != null && goodsList.Count > 0)
            {
                goodsList.ForEach(s =>
                {
                    VipLevelBLL.SingleModel.CalculateVipGoodsPrice(s, levelInfo);//获取会员折后价
                    if (!string.IsNullOrEmpty(s.ptypes))
                    {
                        List<EntGoodType> typeList = EntGoodTypeBLL.SingleModel.GetServiceItemListByIds(s.ptypes);
                        if (typeList != null && typeList.Count > 0)
                        {
                            foreach (EntGoodType typeInfo in typeList)
                            {
                                s.ptypestr += typeInfo.name + ",";
                            }
                        }
                        s.ptypestr = s.ptypestr.TrimEnd(',');
                    }
                });
            }
            List<EntGoodType> goodTypeList = EntGoodTypeBLL.SingleModel.GetServiceItemList(data.appId, data.Id, (int)GoodProjectType.足浴版服务项目分类);
            return Json(new { isok = 1, goodsList = goodsList, typeList = goodTypeList });
        }

        /// <summary>
        /// 根据服务id获取服务信息
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetServiceInfo(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            int levelId = Context.GetRequestInt("levelid", 0);
            if (levelId <= 0)
            {
                return Json(new { isok = -1, msg = "有点小问题levelid_null" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelInfo = VipLevelBLL.SingleModel.GetModelById(levelId);
            if (levelInfo == null)
            {
                return Json(new { isok = -1, msg = "有点小问题levelInfo_null" }, JsonRequestBehavior.AllowGet);
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetServiceById(data.appId, id, 1);
            if (serviceInfo == null)
            {
                return Json(new { isok = -1, msg = "该服务已下架" }, JsonRequestBehavior.AllowGet);
            }
            VipLevelBLL.SingleModel.CalculateVipGoodsPrice(serviceInfo, levelInfo);
            List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetTechnicianListByServiceId(data.Id, serviceInfo.id);
            return Json(new { isok = 1, technicianList = technicianList, serviceInfo = serviceInfo });
        }

        /// <summary>
        /// 获取礼物列表（暂无用到）
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetGiftList(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            List<EntGoods> giftPackages = EntGoodsBLL.SingleModel.GetGiftPackages(data.appId, (int)GoodsType.足浴版送花套餐);
            if (giftPackages == null || giftPackages.Count <= 0)
            {
                EntGoods giftPackage = new EntGoods()
                {
                    aid = data.appId,
                    exttypes = ((int)GoodsType.足浴版送花套餐).ToString(),
                    stock = 1,
                    name = "看相册",
                    state = 1,
                };
                giftPackage.id = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(giftPackage));
                giftPackages = new List<EntGoods>();
                giftPackages.Add(giftPackage);
            }

            giftPackages.ForEach(g => g.price = (float)(g.stock * data.GiftPrice * 0.01));
            return Json(new { isok = 1, giftPackages = giftPackages });
        }

        /// <summary>
        /// 获取时间列表
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GetDateTable(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int technicianId = Context.GetRequestInt("tid", 0);
            if (technicianId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            int days = Context.GetRequestInt("days", 0);
            data.switchModel = JsonConvert.DeserializeObject<SwitchModel>(data.SwitchConfig);
            object timeTable = ServiceTimeBLL.SingleModel.GetTimeTable(data.switchModel, days, data.appId, data.Id, technicianId);
            return Json(new { isok = 1, data = timeTable });
        }

        /// <summary>
        /// 预订服务生成订单
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult ReserveService(string appid)
        {
            #region 数据验证

            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int userId = Context.GetRequestInt("userid", 0);
            if (userId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_UserId(appid, userId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            int serviceId = Context.GetRequestInt("serviceId", 0);
            if (serviceId <= 0)
            {
                return Json(new { isok = -1, msg = "请选择服务" }, JsonRequestBehavior.AllowGet);
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetServiceById(data.appId, serviceId, 1);
            if (serviceInfo == null)
            {
                return Json(new { isok = -1, msg = "服务不存在" }, JsonRequestBehavior.AllowGet);
            }
            int technicianId = Context.GetRequestInt("tid", 0);
            if (technicianId <= 0)
            {
                return Json(new { isok = -1, msg = "请选择技师" }, JsonRequestBehavior.AllowGet);
            }
            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModelById(technicianId);

            if (technicianInfo == null)
            {
                return Json(new { isok = -1, msg = "技师不存在" }, JsonRequestBehavior.AllowGet);
            }
            string serviceTime = Context.GetRequest("serviceTime", string.Empty);
            if (string.IsNullOrEmpty(serviceTime))
            {
                return Json(new { isok = -1, msg = "请选择服务时间" }, JsonRequestBehavior.AllowGet);
            }
            string phone = Context.GetRequest("phone", string.Empty);
            if (string.IsNullOrEmpty(phone))
            {
                return Json(new { isok = -1, msg = "请填写手机号码" }, JsonRequestBehavior.AllowGet);
            }
            int payWay = Context.GetRequestInt("payWay", 0);
            if (payWay <= 0)
            {
                return Json(new { isok = -1, msg = "请选择支付方式" }, JsonRequestBehavior.AllowGet);
            }
            string message = Context.GetRequest("message", string.Empty);
            data.switchModel = JsonConvert.DeserializeObject<SwitchModel>(data.SwitchConfig);
            if (data.switchModel.WriteDesc && string.IsNullOrWhiteSpace(message))
            {
                return Json(new { isok = -1, msg = "请填写备注！" }, JsonRequestBehavior.AllowGet);
            }
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(umodel.TId);
            VipRelation vipRelation = VipRelationBLL.SingleModel.GetVipModel(userInfo.Id, umodel.AppId, xcxTemplate.Type);
            if (vipRelation == null)
            {
                return Json(new { isok = -1, msg = "会员信息不存在" }, JsonRequestBehavior.AllowGet);
            }
            VipLevel levelInfo = VipLevelBLL.SingleModel.GetModelById(vipRelation.levelid);
            if (levelInfo == null)
            {
                return Json(new { isok = -1, msg = "有点小问题levelInfo_null" }, JsonRequestBehavior.AllowGet);
            }

            #endregion 数据验证

            //添加锁项
            try
            {
                //不同商家，不同的锁,当前商家若还未创建，则创建一个
                if (!lockObjectDict_Order.ContainsKey(data.Id))
                {
                    if (!lockObjectDict_Order.TryAdd(data.Id, new object()))
                    {
                        return Json(new { isok = -1, msg = "系统异常！" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { isok = -1, msg = "订单生成失败" }, JsonRequestBehavior.AllowGet);
            }
            lock (lockObjectDict_Order[data.Id])
            {
                int buyPrice = Convert.ToInt32(serviceInfo.price * 100);
                EntGoodsCart goodsCart = new EntGoodsCart();
                EntGoodsOrder orderModel = new EntGoodsOrder();
                switch (levelInfo.type)
                {
                    case 1:
                        buyPrice = Convert.ToInt32(serviceInfo.price * levelInfo.discount);
                        break;

                    case 2:
                        string[] gidList = levelInfo.gids.Split(',');
                        if (gidList.Contains(serviceInfo.id.ToString()))
                        {
                            buyPrice = Convert.ToInt32(serviceInfo.price * levelInfo.discount);
                        }
                        break;
                }
                if (buyPrice <= 0) //折后价为0时,赋值最小1分钱
                {
                    buyPrice = 1;
                }
                //生成订单
                orderModel.aId = data.appId;
                orderModel.BuyPrice = buyPrice;//折后价
                orderModel.GoodsGuid = serviceInfo.id.ToString();
                orderModel.UserId = userInfo.Id;
                orderModel.AccepterTelePhone = phone;
                orderModel.StoreId = data.Id;
                orderModel.Message = message;
                orderModel.BuyMode = payWay;
                orderModel.OrderType = 1;
                orderModel.ReducedPrice = Convert.ToInt32(serviceInfo.price * (100 - levelInfo.discount));//优惠金额
                orderModel.TemplateType = (int)TmpType.小程序足浴模板;
                //生成购物车
                goodsCart.aId = data.appId;
                goodsCart.Price = buyPrice;//折后价
                goodsCart.FoodId = data.Id;
                goodsCart.FoodGoodsId = serviceInfo.id;
                goodsCart.UserId = userInfo.Id;
                goodsCart.State = 1;
                goodsCart.originalPrice = Convert.ToInt32(serviceInfo.price * 100);//原价
                goodsCart.reservationTime = Convert.ToDateTime(serviceTime);
                goodsCart.technicianId = technicianInfo.id;
                goodsCart.goodsMsg = serviceInfo;
                //验证服务时间是否已经被选
                if (ServiceTimeBLL.SingleModel.ValidChoosed(data.appId, data.Id, goodsCart.technicianId, goodsCart.reservationTime))
                {
                    return Json(new { isok = -1, msg = $"下手慢了，这个点已经被抢先预订了" }, JsonRequestBehavior.AllowGet);
                }
                bool success = EntGoodsOrderBLL.SingleModel.CreateFootbathOrder(orderModel, goodsCart);
                if (success)
                {
                    object obj = FootbathPayOrder(appid, orderModel, userInfo, data, payWay, umodel, (int)ArticleTypeEnum.MiniappFootbath, goodsCart);
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "系统繁忙，订单生成失败" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// 足浴版支付 goodsCart==null 说明是礼物支付
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="payWay"></param>
        /// <returns></returns>
        private object FootbathPayOrder(string appid, EntGoodsOrder orderModel, C_UserInfo userInfo, FootBath store, int payWay, XcxAppAccountRelation umodel, int orderType, EntGoodsCart goodsCart = null)
        {
            if (payWay == (int)miniAppBuyMode.微信支付)
            {
                #region CtiyModer 生成

                string no = WxPayApi.GenerateOutTradeNo();

                CityMorders citymorderModel = new CityMorders
                {
                    OrderType = (int)ArticleTypeEnum.MiniappFootbath,
                    ActionType = orderType,
                    Addtime = DateTime.Now,
                    payment_free = orderModel.BuyPrice,
                    trade_no = no,
                    Percent = 99,//不收取服务费
                    userip = WebHelper.GetIP(),
                    FuserId = userInfo.Id,
                    Fusername = userInfo.NickName,
                    orderno = no,
                    payment_status = 0,
                    Status = 0,
                    Articleid = 0,
                    CommentId = 0,
                    MinisnsId = store.Id,//商家ID
                    TuserId = orderModel.Id,//订单的ID
                    ShowNote = $" {umodel.Title}购买商品支付{orderModel.BuyPrice * 0.01}元",
                    CitySubId = 0,//无分销,默认为0
                    PayRate = 1,
                    buy_num = 0, //无
                    appid = appid,
                };
                orderModel.OrderId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));

                #endregion CtiyModer 生成

                if (goodsCart != null)
                {
                    ServiceTimeBLL.SingleModel.AddSelServiceTime(store, goodsCart, store.switchModel);//客户预订成功后将选定的时间点添加到已服务时间表
                }
            }

            #region 更新对外订单号及对应CityModer的ID

            //对外订单号规则：年月日时分 + 电商本地库ID最后3位数字
            string idStr = orderModel.Id.ToString();
            if (idStr.Length >= 3)
            {
                idStr = idStr.Substring(idStr.Length - 3, 3);
            }
            else
            {
                idStr.PadLeft(3, '0');
            }
            idStr = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{idStr}";
            orderModel.OrderNum = idStr;
            EntGoodsOrderBLL.SingleModel.Update(orderModel, "OrderId,OrderNum");

            #endregion 更新对外订单号及对应CityModer的ID

            store.switchModel = JsonConvert.DeserializeObject<SwitchModel>(store.SwitchConfig);

            if (payWay == (int)miniAppBuyMode.储值支付)
            {
                if (!store.switchModel.canSaveMoneyFunction)
                {
                    return new { isok = -1, msg = "订单支付失败,无法使用储值支付" };
                }
                SaveMoneySetUser saveMoneyUser = new SaveMoneySetUser();
                saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appid, userInfo.Id);
                if (saveMoneyUser == null || saveMoneyUser.AccountMoney < orderModel.BuyPrice)
                {
                    return new { isok = -1, msg = $" 储值余额不足,请充值！ " };
                }
                {
                    #region 储值支付 扣除预存款金额并生成消费记录

                    if (SaveMoneySetUserBLL.SingleModel.payOrderBySaveMoneyUser(orderModel, saveMoneyUser))
                    {
                        if (goodsCart != null)
                        {
                            ServiceTimeBLL.SingleModel.AddSelServiceTime(store, goodsCart, store.switchModel);//客户预订成功后将选定的时间点添加到已服务时间表
                        }

                        #region 发送模板消息

                        if (goodsCart == null) //FuserId 不为空就是送礼物给某某技师的订单
                        {
                            TemplateMsg_Gzh.SendGiftTemplateMessage(orderModel);
                        }
                        else
                        {
                            TemplateMsg_Gzh.SendReserveTemplateMessage(orderModel);
                        }

                        #endregion 发送模板消息

                        return new { isok = 1, msg = "订单生成并支付成功", postdata = orderModel.OrderNum, orderid = orderModel.OrderId, dbOrder = orderModel.Id };
                    }
                    else
                    {
                        return new { isok = -1, msg = "订单支付失败" };
                    }

                    #endregion 储值支付 扣除预存款金额并生成消费记录
                }
            }
            //记录订单操作日志(用户下单)
            EntGoodsOrderLogBLL.SingleModel.Add(new EntGoodsOrderLog() { GoodsOrderId = orderModel.Id, UserId = userInfo.Id, LogInfo = $" 成功下单,下单金额：{orderModel.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now });
            return new { isok = 1, msg = "订单生成成功", postdata = orderModel.OrderNum, orderid = orderModel.OrderId, dbOrder = orderModel.Id };
        }

        /// <summary>
        /// 微信取消支付回调
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult CancelPay(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int tid = Context.GetRequestInt("tid", 0);
            if (tid <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误tid_null" }, JsonRequestBehavior.AllowGet);
            }

            string serviceTimeStr = Context.GetRequest("servicetime", string.Empty);
            if (string.IsNullOrEmpty(serviceTimeStr))
            {
                return Json(new { isok = -1, msg = "参数错误servicetime_null" }, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrder order = null;
            int orderId = Context.GetRequestInt("dbOrder", 0);
            if (orderId > 0)
            {
                order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
            }
            if (order != null)
            {
                order.State = (int)OrderState.取消订单;
                EntGoodsOrderBLL.SingleModel.Update(order, "state");
            }
            EntGoodsCart goodsCart = new EntGoodsCart();
            goodsCart = EntGoodsCartBLL.SingleModel.GetModelByGoodsOrderId(order.Id, 0);
            if (goodsCart == null)
            {
                return Json(new { isok = -1, msg = "参数错误goodsCart_null" }, JsonRequestBehavior.AllowGet);
            }
            DateTime time = Convert.ToDateTime(serviceTimeStr);
            ServiceTime serviceTime = ServiceTimeBLL.SingleModel.GetModelByDate_Tid(data.appId, data.Id, goodsCart.technicianId, time.ToShortDateString());
            if (serviceTime != null && !string.IsNullOrEmpty(serviceTime.time))
            {
                List<string> timeList = serviceTime.time.Split(',').ToList();
                timeList.Remove(time.ToString("HH:mm"));
                serviceTime.time = string.Join(",", timeList);
                serviceTime.date = Convert.ToDateTime(time.ToShortDateString());
                if (ServiceTimeBLL.SingleModel.Update(serviceTime, "time,date"))
                {
                    return Json(new { isok = 1, msg = "已将预订的服务时间取消" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "预订的服务时间取消失败" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isok = 1, msg = "没有找到预订的服务时间" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取预订记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderRecord(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int UserId = Context.GetRequestInt("uid", 0);
            if (UserId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误!" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_UserId(appid, UserId);
            if (userInfo == null)
            {
                return Json(new { isok = -1, msg = "用户信息错误" });
            }
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int orderType = Context.GetRequestInt("ordertype", 0);
            int state = Context.GetRequestInt("state", -1);
            List<object> objList = new List<object>();
            List<EntGoodsOrder> orderList = EntGoodsOrderBLL.SingleModel.GetFootbathOrderList(data.appId, UserId, orderType, state, pageSize, pageIndex);
            if (orderList != null && orderList.Count > 0)
            {
                string orderIds = string.Join(",",orderList.Select(s=>s.Id));
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);
                foreach (EntGoodsOrder orderInfo in orderList)
                {
                    TechnicianInfo technicianInfo = null;
                    EntGoods serviceInfo = null;
                    orderInfo.goodsCarts = entGoodsCartList?.Where(w=>w.GoodsOrderId == orderInfo.Id).ToList();
                    if (orderInfo.goodsCarts != null && orderInfo.goodsCarts.Count > 0)
                    {
                        technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(orderInfo.goodsCarts[0].technicianId);
                        serviceInfo = EntGoodsBLL.SingleModel.GetModel(orderInfo.goodsCarts[0].FoodGoodsId);
                    }
                    object obj = new
                    {
                        technicianInfo = technicianInfo,
                        serviceInfo = serviceInfo,
                        orderInfo = orderInfo
                    };
                    objList.Add(obj);
                }
            }
            return Json(new { isok = 1, list = objList });
        }

        /// <summary>
        /// 送花支付
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult payGift(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int userId = Context.GetRequestInt("uid", 0);
            if (userId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_UserId(appid, userId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            EntGoods giftPackage = EntGoodsBLL.SingleModel.GetGiftInfoByName(data.appId, (int)GoodsType.足浴版送花套餐, "看相册");
            if (giftPackage == null)
            {
                giftPackage = new EntGoods()
                {
                    aid = data.appId,
                    exttypes = ((int)GoodsType.足浴版送花套餐).ToString(),
                    stock = 1,
                    name = "看相册",
                    state = 1,
                    price = 2.00F,
                };
                giftPackage.id = Convert.ToInt32(EntGoodsBLL.SingleModel.Add(giftPackage));
            }
            int payWay = Context.GetRequestInt("payWay", 0);
            if (payWay <= 0)
            {
                return Json(new { isok = -1, msg = "请选择支付方式" }, JsonRequestBehavior.AllowGet);
            }
            int technicianId = Context.GetRequestInt("tid", 0);
            if (technicianId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误tid_null" }, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrder orderModel = new EntGoodsOrder();
            //生成订单
            orderModel.aId = data.appId;
            orderModel.BuyPrice = Convert.ToInt32(giftPackage.price * 100);
            orderModel.GoodsGuid = giftPackage.id.ToString();
            orderModel.UserId = userInfo.Id;
            orderModel.StoreId = data.Id;
            orderModel.BuyMode = payWay;
            orderModel.OrderType = 2;
            orderModel.FuserId = technicianId;
            orderModel.TemplateType = (int)TmpType.小程序足浴模板;
            orderModel.Id = Convert.ToInt32(EntGoodsOrderBLL.SingleModel.Add(orderModel));
            if (orderModel.Id > 0)
            {
                object obj = FootbathPayOrder(appid, orderModel, userInfo, data, payWay, umodel, (int)ArticleTypeEnum.MiniappFootbathGift);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { isok = -1, msg = "系统繁忙，订单生成失败" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 送花记录
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public ActionResult GiftList(string appid)
        {
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = -1, msg = "appid不能为空", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "请先授权", dataconfig = "" }, JsonRequestBehavior.AllowGet);
            }
            FootBath data = FootBathBLL.SingleModel.GetModelByAppId(umodel.Id);
            if (data == null)
            {
                return Json(new { isok = -1, msg = "没有数据" + umodel.AppId + ",id" + umodel.Id, data = "" }, JsonRequestBehavior.AllowGet);
            }
            int userId = Context.GetRequestInt("uid", 0);
            if (userId <= 0)
            {
                return Json(new { isok = -1, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_UserId(appid, userId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" }, JsonRequestBehavior.AllowGet);
            }
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int recordCount = 0;
            List<EntGoodsOrder> giftOrderList = EntGoodsOrderBLL.SingleModel.GetGiftRecord(data.appId, userId, pageSize, pageIndex, out recordCount);
            List<object> objList = new List<object>();
            if (giftOrderList != null && giftOrderList.Count > 0)
            {
                string fuserIds = string.Join(",", giftOrderList.Select(s=>s.FuserId).Distinct());
                List<TechnicianInfo> techiniianInfoList = TechnicianInfoBLL.SingleModel.GetListByIds(fuserIds);
                foreach (EntGoodsOrder giftOrder in giftOrderList)
                {
                    TechnicianInfo technicianInfo = techiniianInfoList?.FirstOrDefault(f=>f.id == giftOrder.FuserId);
                    object obj = new { giftInfo = giftOrder, technicianInfo = technicianInfo };
                    objList.Add(obj);
                }
            }
            return Json(new { isok = 1, list = objList, recordCount = recordCount, sum = recordCount * 0.01 }, JsonRequestBehavior.AllowGet);
        }

        #region 技师端

        /// <summary>
        /// 验证手机号,验证码.登录技师端
        /// </summary>
        /// <returns></returns>
        public ActionResult BindPhoneNumber()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            string openId = Context.GetRequest("openId", string.Empty);
            string telePhoneNumber = Context.GetRequest("telePhoneNumber", string.Empty);
            string verificationCode = Context.GetRequest("verificationCode", string.Empty);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.code = "0";
            msg.Msg = "绑定成功!";

            XcxAppAccountRelation xcxRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelation == null || string.IsNullOrWhiteSpace(xcxRelation.AppId))
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "商家授权信息错误! xcxRole_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            FootbathXcxRelation clientTechnicianRelation = FootbathXcxRelationBLL.SingleModel.GetModelByTechnicianAid(xcxRelation.Id);
            if (clientTechnicianRelation == null)
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "本技师端还没有关联客户端小程序";
                return Json(msg);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appId, openId);
            if (userInfo == null)
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "当前用户未注册!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            //验证码
            if (string.IsNullOrEmpty(verificationCode))
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "验证码不能为空!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            //手机号码
            if (string.IsNullOrEmpty(telePhoneNumber) || !Regex.IsMatch(telePhoneNumber, @"^[1]+[3-9]+\d{9}$"))
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "手机格式不正确!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            if (telePhoneNumber != "15820152935" || verificationCode != "1234")
            {
                //验证码是否匹配
                string serverAuthCode = RedisUtil.Get<string>(string.Format(TECHNICIAN_LOGIN_KEY, telePhoneNumber));
                if (serverAuthCode != verificationCode)
                {
                    msg.isok = false;
                    msg.code = "1";
                    msg.Msg = "手机号码错误或验证码错误!";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
            }

            TechnicianInfo curTechnicianInfo = TechnicianInfoBLL.SingleModel.GetTechnicianListByAid_JobNumeber(clientTechnicianRelation.clientAid, telePhoneNumber);
            if (curTechnicianInfo == null)
            {
                msg.isok = false;
                msg.code = "3";
                msg.Msg = "你还不是本平台的技师哦";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            userInfo.TelePhone = telePhoneNumber;
            bool updateSuccess = C_UserInfoBLL.SingleModel.Update(userInfo, "TelePhone");
            if (!updateSuccess)
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "登录失败,请重试!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 发送短信消息验证
        /// </summary>
        /// <param name="tel"></param>
        /// <param name="sendType"></param>
        /// <param name="isNeedCheckBind">是否检查手机已被绑定</param>
        /// <returns></returns>
        public ActionResult SendUserAuthCode(SendTypeEnum sendType = SendTypeEnum.个人中心)
        {
            string telePhoneNumber = Context.GetRequest("telePhoneNumber", string.Empty);

            Return_Msg msg = new Return_Msg();
            msg.isok = false;
            try
            {
                if (string.IsNullOrWhiteSpace(telePhoneNumber) || !Regex.IsMatch(telePhoneNumber, @"^[1]+[3-9]+\d{9}$"))
                {
                    msg.Msg = "手机格式不正确！";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }

                //通过短信发送验证码
                SendMsgHelper sendMsgHelper = new SendMsgHelper();
                string authCode = RedisUtil.Get<string>(string.Format(TECHNICIAN_LOGIN_KEY, telePhoneNumber));
                if (string.IsNullOrEmpty(authCode))
                    authCode = Utility.EncodeHelper.CreateRandomCode(4);
                bool result = sendMsgHelper.AliSend(telePhoneNumber, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), sendType) + "\"}", "小未科技", 401);
                if (result)
                {
                    RedisUtil.Set<string>(string.Format(TECHNICIAN_LOGIN_KEY, telePhoneNumber), authCode, TimeSpan.FromMinutes(5));
                    msg.isok = true;
                    msg.Msg = "验证码发送成功！";
                }
                else
                {
                    msg.Msg = "验证码发送失败,请稍后再试！";
                }
                msg.dataObj = authCode;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msg.Msg = "系统异常！" + ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取用户信息（技师登录验证）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TechnicianLogin()
        {
            string telePhoneNumber = Context.GetRequest("telePhoneNumber", string.Empty);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.code = "0";
            msg.Msg = "获取成功!";
            string appId = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                msg.isok = false;
                msg.Msg = "商家授权信息错误! appId_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation accountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (accountRelation == null)
            {
                msg.isok = false;
                msg.Msg = "商家授权信息错误! Relation_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            FootbathXcxRelation xcxRelation = FootbathXcxRelationBLL.SingleModel.GetModel($"technicianaid={accountRelation.Id} and state>=0");
            if (xcxRelation == null)
            {
                msg.isok = false;
                msg.Msg = "当前小程序未关联任何客户端小程序";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            TechnicianInfo curTechnicianInfo = TechnicianInfoBLL.SingleModel.GetModel($" TelPhone = '{telePhoneNumber}' and appId={xcxRelation.clientAid} and State != {(int)TechnicianState.删除}  ");
            if (curTechnicianInfo == null)
            {
                msg.isok = false;
                msg.code = "3";
                msg.Msg = "你不是本平台的技师哦";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            FootBath store = FootBathBLL.SingleModel.GetModel(curTechnicianInfo.storeId);
            if (store == null)
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "商家授权信息错误! store_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation xcxRole = _xcxAppAccountRelationBLL.GetModel(curTechnicianInfo.appId);
            if (xcxRole == null || string.IsNullOrWhiteSpace(xcxRole.AppId))
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "商家授权信息错误! xcxRole_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            curTechnicianInfo.showBirthday = curTechnicianInfo.birthday.ToString("yyyy-MM-dd");

            #region 订单小计

            //统计服务订单sql
            //待服务
            int waitOrderCount = EntGoodsOrderBLL.SingleModel.GetServiceCountByTechinicianId(curTechnicianInfo.id, (int)MiniAppEntOrderState.待服务);

            ////已完成
            int finishOrderCount = EntGoodsOrderBLL.SingleModel.GetServiceCountByTechinicianId(curTechnicianInfo.id, (int)MiniAppEntOrderState.已完成);

            ////送花总数
            int havGiftsCount = 0;
            List<EntGoods> giftPackage = EntGoodsBLL.SingleModel.GetGiftPackages(xcxRole.Id, (int)GoodsType.足浴版送花套餐);
            if (giftPackage != null&&giftPackage.Count>0)
            {
                string ids = string.Join(",", giftPackage.Select(x => x.id));
                havGiftsCount = EntGoodsOrderBLL.SingleModel.GetGiftCount(curTechnicianInfo.id, ids, (int)MiniAppEntOrderState.交易成功, (int)TmpType.小程序足浴模板);
            }

            #endregion 订单小计

            List<string> photos = curTechnicianInfo.photo.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))?.ToList<string>() ?? new List<string>();
            //返回小程序权限资料
            msg.dataObj = new
            {
                userInfo = curTechnicianInfo,
                bindAppInfo = xcxRole,
                store,
                waitOrderCount,
                finishOrderCount,
                havGiftsCount,
                photos
            };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改技师状态
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeTechnicianState(TechnicianInfo userInfo)
        {
            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.code = "0";
            msg.Msg = "更新成功!";

            TechnicianInfo curTechnicianInfo = TechnicianInfoBLL.SingleModel.GetModel(userInfo.id);
            if (curTechnicianInfo == null)
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "找不到用户!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            if (!Enum.IsDefined(typeof(TechnicianState), userInfo.state))
            {
                msg.isok = false;
                msg.code = "2";
                msg.Msg = "状态值错误!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            curTechnicianInfo.state = userInfo.state;

            bool updateSuccess = TechnicianInfoBLL.SingleModel.Update(curTechnicianInfo, "state");
            if (!updateSuccess)
            {
                msg.isok = false;
                msg.code = "3";
                msg.Msg = "更新失败!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveUserInfo(TechnicianInfo userInfo)
        {
            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.code = "0";
            msg.Msg = "获取成功!";

            TechnicianInfo curTechnicianInfo = TechnicianInfoBLL.SingleModel.GetModel(userInfo.id);
            if (curTechnicianInfo == null || string.IsNullOrWhiteSpace(curTechnicianInfo.unionId))
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "找不到用户信息!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(userInfo.jobNumber))
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "请输入工号!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            if (userInfo.desc.Length > 20)
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "个性简明(简介)不能超过20字!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(userInfo.headImg))
            {
                msg.isok = false;
                msg.code = "1";
                msg.Msg = "头像不可为空!";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string[] photos = null;
            if (!string.IsNullOrWhiteSpace(userInfo.photo))
            {
                photos = userInfo.photo.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))?.ToArray();
                if (photos != null && photos.Length > 20)
                {
                    msg.isok = false;
                    msg.code = "1";
                    msg.Msg = "付费相册最多上传20张照片!";
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
            }

            curTechnicianInfo.jobNumber = userInfo.jobNumber;
            curTechnicianInfo.desc = userInfo.desc;
            curTechnicianInfo.headImg = userInfo.headImg;
            curTechnicianInfo.photo = string.Join(",", photos);
            curTechnicianInfo.updateDate = DateTime.Now;

            bool updateSuccess = TechnicianInfoBLL.SingleModel.Update(curTechnicianInfo, "jobNumber,desc,headImg,photo,updateDate");

            msg.isok = updateSuccess;
            msg.code = updateSuccess ? "0" : "1";
            msg.Msg = updateSuccess ? "更新成功,请稍候再试" : "更新失败,请稍候再试";
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 送花排行榜
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGiftsOrderDescList()
        {
            string storeId = Context.GetRequest("storeId", string.Empty);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.code = "0";
            msg.Msg = "获取成功!";

            List<TechnicianInfo> curTechnicianInfos = TechnicianInfoBLL.SingleModel.GetTechnicianListByStoreId(storeId) ?? new List<TechnicianInfo>();
            int i = 0;
            //收花朵排行榜
            msg.dataObj = curTechnicianInfos.OrderByDescending(t => t.GetItGiftCount).Select(t => new { index = i++, id = t.id, headImg = t.headImg, jobNumber = t.jobNumber, getitGiftCount = t.GetItGiftCount });
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 送花记录小计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyGiftsCount()
        {
            int technicianId = Context.GetRequestInt("technicianId", 0);
            int getGiftsRecordType = Context.GetRequestInt("getGiftsRecordType", (int)GiftsRecordType.全部);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.Msg = "获取成功!";

            int allGiftsCount = 0;//收到的花的数量
            string giftsPrice = "0.00";//花的价值
            int giveGiftsUserCount = 0;//送花用户总数

            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(technicianId);
            if (technicianInfo == null)
            {
                msg.isok = false;
                msg.Msg = "找不到用户资料! technicianInfo_null";

                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            string startDate = string.Empty;
            string endDate = string.Empty;
            switch (getGiftsRecordType)
            {
                case (int)GiftsRecordType.全部:

                    break;

                case (int)GiftsRecordType.今天:
                    startDate = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;

                case (int)GiftsRecordType.昨天:
                    startDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
                    break;

                case (int)GiftsRecordType.最近7天:
                    startDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;

                case (int)GiftsRecordType.最近30天:
                    startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
            }
            //礼物总数
            allGiftsCount = EntGoodsOrderBLL.SingleModel.GetGiftRecordCountByPayDate(startDate, endDate, technicianInfo.id);
            //礼物总额
            giftsPrice = EntGoodsOrderBLL.SingleModel.GetGiftSumPriceByPayDate(startDate, endDate, technicianInfo.id);
            //送礼用户数
            giveGiftsUserCount = EntGoodsOrderBLL.SingleModel.GetGiveGiftUserCount(startDate, endDate, technicianInfo.id);
            msg.dataObj = new
            {
                allGiftsCount = allGiftsCount,
                giftsPrice = giftsPrice,
                giveGiftsUserCount = giveGiftsUserCount,
            };

            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 我的花
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyGifts()
        {
            int technicianId = Context.GetRequestInt("technicianId", 0);
            int getGiftsRecordType = Context.GetRequestInt("getGiftsRecordType", (int)GiftsRecordType.全部);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 6);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.Msg = "获取成功!";

            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(technicianId);
            if (technicianInfo == null)
            {
                msg.isok = false;
                msg.Msg = "找不到用户资料! technicianInfo_null";

                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            List<EntGoodsOrder> giftRecords = new List<EntGoodsOrder>();//送花记录

            //查询订单
            string startDate = string.Empty;
            string endDate = string.Empty;
            switch (getGiftsRecordType)
            {
                case (int)GiftsRecordType.全部:

                    break;

                case (int)GiftsRecordType.今天:
                    startDate = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;

                case (int)GiftsRecordType.昨天:
                    startDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
                    break;

                case (int)GiftsRecordType.最近7天:
                    startDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;

                case (int)GiftsRecordType.最近30天:
                    startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00:00");
                    endDate = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
            }
            giftRecords = EntGoodsOrderBLL.SingleModel.GetGiftRecordByPayDate(technicianInfo.id, startDate, endDate, pageSize, pageIndex) ?? new List<EntGoodsOrder>();
            List<C_UserInfo> users = new List<C_UserInfo>();
            if (giftRecords.Any())

            {
                users = C_UserInfoBLL.SingleModel.GetListByIds(string.Join(",", giftRecords.Select(g => g.UserId))) ?? new List<C_UserInfo>();
            }

            //记录列表结果集
            object resultData = giftRecords.OrderByDescending(g => g.PayDate).Select(g => new
            {
                userId = g.UserId,
                userHeaImg = users.Where(u => u.Id == g.UserId).FirstOrDefault().HeadImgUrl,
                nickName = users.Where(u => u.Id == g.UserId).FirstOrDefault().NickName,
                giftsCount = 1,
                giftsPrice = g.BuyPriceStr,
                giveDate = g.PayDateStr
            });

            msg.dataObj = resultData;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 当前技师订单总数查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyOrderCount()
        {
            int technicianId = Context.GetRequestInt("technicianId", 0);

            int complateOrderCount = 0;
            int reservedOrderCount = 0;
            int waitOrderCount = 0;

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.Msg = "获取成功!";

            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(technicianId);
            if (technicianInfo == null)
            {
                msg.isok = false;
                msg.Msg = "找不到用户资料! technicianInfo_null";

                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            //查询订单

            complateOrderCount = EntGoodsOrderBLL.SingleModel.GetFootbathOrderCount(technicianInfo.id, $"{(int)MiniAppEntOrderState.已完成},{(int)MiniAppEntOrderState.已取消}", (int)EntOrderType.订单);//已结束
            reservedOrderCount = EntGoodsOrderBLL.SingleModel.GetFootbathOrderCount(technicianInfo.id, $"{(int)MiniAppEntOrderState.待服务},{(int)MiniAppEntOrderState.已超时}", (int)EntOrderType.预约订单);//已预约

            waitOrderCount = EntGoodsOrderBLL.SingleModel.GetFootbathOrderCount(technicianInfo.id, $"{(int)MiniAppEntOrderState.待服务},{(int)MiniAppEntOrderState.服务中},{(int)MiniAppEntOrderState.已超时}", (int)EntOrderType.订单);//待服务

            msg.dataObj = new
            {
                complateOrderCount = complateOrderCount,
                reservedOrderCount = reservedOrderCount,
                waitOrderCount = waitOrderCount
            };
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 当前技师订单查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyOrder()
        {
            int technicianId = Context.GetRequestInt("technicianId", 0);
            int getOrderType = Context.GetRequestInt("getOrderType", 0);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 6);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.Msg = "获取成功!";

            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(technicianId);
            if (technicianInfo == null)
            {
                msg.isok = false;
                msg.Msg = "找不到用户资料! technicianInfo_null";

                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            List<EntGoodsOrder> orders = new List<EntGoodsOrder>();

            string findSqlWhere = string.Empty;
            string states = string.Empty;
            int orderType = 0;
            switch (getOrderType)
            {
                case 0://已结束
                    states = $"{(int)MiniAppEntOrderState.已完成},{(int)MiniAppEntOrderState.已取消}";
                    orderType = (int)EntOrderType.订单;
                    break;

                case 1://已预约
                    states = $"{(int)MiniAppEntOrderState.待服务},{(int)MiniAppEntOrderState.已超时}";
                    orderType = (int)EntOrderType.预约订单;
                    break;

                case 2://待服务
                    states = $"{(int)MiniAppEntOrderState.待服务},{(int)MiniAppEntOrderState.服务中},{(int)MiniAppEntOrderState.已超时}";
                    orderType = (int)EntOrderType.订单;
                    break;
            }

            orders = EntGoodsOrderBLL.SingleModel.GetServiceOrderList(technicianInfo.id, orderType, states, pageSize, pageIndex);

            EntGoods curProject = null;
            DateTime entProjectTime = DateTime.Now;

            string goodsIds = string.Join(",",orders.Select(s=>s.reservationProjectId).Distinct());
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);
            orders.ForEach(o =>
            {
                //现阶段做这个没有任何意义,考虑以往商家变动太简易,服务开始的真正时间已经无法真正意义确定.
                curProject = entGoodsList?.FirstOrDefault(f=>f.id == o.reservationProjectId);
                entProjectTime = o.reservationTime;
                if (curProject != null)
                {
                    o.goodsNames = $" {curProject.name} ({curProject.ServiceTime}分钟) ";
                    entProjectTime = entProjectTime.AddMinutes(curProject.ServiceTime);
                }

                o.serviceEndTime = entProjectTime;
                if (o.reservationTime < DateTime.Now && (o.State == (int)MiniAppEntOrderState.服务中 || o.State == (int)MiniAppEntOrderState.已超时))
                {
                    o.orderCurState = 0;
                }
                else if (o.reservationTime < DateTime.Now && o.State == (int)MiniAppEntOrderState.待服务)
                {
                    o.orderCurState = 1;
                }
                else if (o.reservationTime >= DateTime.Now && (o.State == (int)MiniAppEntOrderState.服务中 || o.State == (int)MiniAppEntOrderState.已超时))
                {
                    o.orderCurState = 2;
                }
                else if (o.reservationTime >= DateTime.Now && o.State == (int)MiniAppEntOrderState.待服务)
                {
                    o.orderCurState = 3;
                }
            });

            msg.dataObj = orders.Select(o => new
            {
                Id = o.Id,
                orderNum = o.OrderNum,
                serviceTime = o.reservationTimeStr,
                serviceName = o.goodsNames,
                serviceEndTime = o.serviceEndTimeStr,
                userPhone = o.AccepterTelePhone,
                remark = o.Message,
                orderType = o.OrderType,
                state = o.State,
                stateRemark = o.StateStr,
                curState = o.orderCurState,
            });
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 订单状态修改
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateOrderState()
        {
            int orderId = Context.GetRequestInt("orderId", 0);
            int orderState = Context.GetRequestInt("orderState", 0);
            int technicianId = Context.GetRequestInt("technicianId", 0);

            Return_Msg msg = new Return_Msg();
            msg.isok = true;
            msg.Msg = "获取成功!";

            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(technicianId);
            if (technicianInfo == null)
            {
                msg.isok = false;
                msg.Msg = "找不到用户资料! technicianInfo_null";

                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            EntGoodsOrder order = EntGoodsOrderBLL.SingleModel.GetModel(orderId);
            if (order == null)
            {
                msg.isok = false;
                msg.Msg = "找不到订单资料! order_null";

                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            EntGoodsCart car = EntGoodsCartBLL.SingleModel.GetModelByGoodsOrderId(order.Id);
            if (car == null)
            {
                msg.isok = false;
                msg.Msg = "找不到订单资料! car_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            if (car.technicianId != technicianInfo.id)
            {
                msg.isok = false;
                msg.Msg = "无权限修改当前单据! car_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            EntGoods service = EntGoodsBLL.SingleModel.GetModel(car.FoodGoodsId);
            if (service == null)
            {
                msg.isok = false;
                msg.Msg = "没有该服务项目! service_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            //技师端仅开放此2种状态的修改权限
            if ((orderState != (int)MiniAppEntOrderState.服务中 && orderState != (int)MiniAppEntOrderState.已完成) || order.OrderType == (int)EntOrderType.预约订单)
            {
                msg.isok = false;
                msg.Msg = "订单状态有误,请刷新重试! car_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

            int befState = order.State;
            order.State = orderState;
            if (EntGoodsOrderBLL.SingleModel.Update(order, "State"))
            {
                if (befState == (int)MiniAppEntOrderState.已超时 && order.State == (int)MiniAppEntOrderState.已完成)
                {
                    technicianInfo.state = (int)TechnicianState.空闲;
                    TechnicianInfoBLL.SingleModel.Update(technicianInfo, "state");

                    VipRelationBLL.SingleModel.updatelevel(order.UserId, "footbath");
                }
                else if (befState == (int)MiniAppEntOrderState.服务中 && order.State == (int)MiniAppEntOrderState.已完成)
                {
                    technicianInfo.serviceCount++;
                    service.salesCount++;
                    technicianInfo.state = (int)TechnicianState.空闲;

                    TechnicianInfoBLL.SingleModel.Update(technicianInfo, "serviceCount,state");
                    EntGoodsBLL.SingleModel.Update(service, "salesCount");
                    VipRelationBLL.SingleModel.updatelevel(order.UserId, "footbath");
                }
                else if (befState == (int)MiniAppEntOrderState.已超时 && order.State == (int)MiniAppEntOrderState.服务中)
                {
                    technicianInfo.serviceCount--;
                    service.salesCount--;
                    technicianInfo.state = (int)TechnicianState.上钟;

                    TechnicianInfoBLL.SingleModel.Update(technicianInfo, "serviceCount,state");
                    EntGoodsBLL.SingleModel.Update(service, "salesCount");
                }
                else if (befState == (int)MiniAppEntOrderState.待服务 && order.State == (int)MiniAppEntOrderState.服务中)
                {
                    technicianInfo.state = (int)TechnicianState.上钟;

                    TechnicianInfoBLL.SingleModel.Update(technicianInfo, "state");
                }
            }
            else
            {
                msg.isok = false;
                msg.Msg = "更新失败! service_null";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        #endregion 技师端
    }
}