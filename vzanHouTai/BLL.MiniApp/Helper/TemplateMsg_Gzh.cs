using BLL.MiniApp.Conf;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Pin;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Qiye;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Tools;
using BLL.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.Stores;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utility;

namespace BLL.MiniApp.Helper
{
    /// <summary>
    /// 公众号模板消息逻辑类
    /// </summary>
    public static class TemplateMsg_Gzh
    {
        /// <summary>
        /// 公众号取消模板消息
        /// </summary>
        public static readonly string _CancelOrderTemplateId ="jSJdjUzHTrNiD_T1kSPMb5vazmB4QIOhLwpmAuhMZZw";

        #region 足浴模板消息
        /// <summary>
        /// 足浴订单预约成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendReserveTemplateMessage(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }
            
            EntGoodsCart cartInfo = EntGoodsCartBLL.SingleModel.GetModel($"GoodsOrderId={orderModel.Id}");
            if (cartInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约成功发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetModel($"id={cartInfo.FoodGoodsId}");
            if (serviceInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约成功发送模板消息给商家失败：没有找到服务项目信息 serviceId:{cartInfo.FoodGoodsId}");
                return;
            }
            FootBath store = FootBathBLL.SingleModel.GetModel(orderModel.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约成功发送模板消息给商家失败：没有找到门店信息 storeId:{store.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约成功发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约成功发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            //通知商家有顾客预订了要做的项目
            string title = $"您的店铺【{store.StoreName}】有新的预订单，请及时处理！";
            List<string> values = new List<string>();
            values.Add(serviceInfo.name);
            values.Add("无");
            values.Add(orderModel.AccepterTelePhone);
            values.Add(cartInfo.showReservationTime);
            values.Add(store.Address);
            string remark = "请您及时登录后台处理客户预订单.";

            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_footbath_ReserveTemplateId, "", unionId, title, values, remark, "");
            }

            //通知技师有顾客预约了技师的项目
            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel(cartInfo.technicianId);
            if (technicianInfo == null || !string.IsNullOrWhiteSpace(technicianInfo.unionId))
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约成功发送模板消息给技师失败：没有找到技师信息 technicianId:{cartInfo.technicianId},unionId:{technicianInfo.unionId}");
                return;
            }
            title = $"您在店铺【{store.StoreName}】有新的预订单，请知悉！";
            remark = "请您提前知悉,并合理安排好时间为顾客提供更优质的服务.";
            SendTemplateMessage(WebSiteConfig.DZ_footbath_ReserveTemplateId, technicianInfo.unionId, technicianInfo.unionId, title, values, remark, "");
        }

        /// <summary>
        /// 足浴预约超时未处理提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendReserveTimeOutTemplateMessage(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            EntGoodsCart cartInfo = EntGoodsCartBLL.SingleModel.GetModel($"GoodsOrderId={orderModel.Id}");
            if (cartInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约超时未处理发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetModel($"id={cartInfo.FoodGoodsId}");
            if (serviceInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约超时未处理发送模板消息给商家失败：没有找到服务项目信息 serviceId:{cartInfo.FoodGoodsId}");
                return;
            }
            FootBath store = FootBathBLL.SingleModel.GetModel(orderModel.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约超时未处理发送模板消息给商家失败：没有找到门店信息 storeId:{store.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约超时未处理发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴预约超时未处理发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            string title = $"您的顾客【{orderModel.AccepterTelePhone}】有预订单超过时间未处理，请知悉！";
            List<string> values = new List<string>();
            values.Add(serviceInfo.name);
            values.Add(cartInfo.showReservationTime);
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_footbath_ReserveTimeOutTemplateId, "", unionId, title, values, "您可登录后台查看该顾客预订单详情并进行调整。", "");
            }
        }

        /// <summary>
        /// 足浴客户送花提醒技师
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendGiftTemplateMessage(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }
            
            FootBath store = FootBathBLL.SingleModel.GetModel(orderModel.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴客户送花提醒技师发送模板消息给商家失败：没有找到门店信息 storeId:{store.Id}");
                return;
            }
            TechnicianInfo tenchnicianInfo = TechnicianInfoBLL.SingleModel.GetModel(orderModel.FuserId);
            if (tenchnicianInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"足浴客户送花提醒技师发送模板消息给商家失败：没有找到技师信息 orderModel.FuserId:{orderModel.FuserId}");
                return;
            }

            string title = $"您在店铺【{store.StoreName}】有顾客给您送花啦!";
            List<string> values = new List<string>();
            values.Add("客户送花查看相册!");
            values.Add("无");
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.PayDateStr);
            values.Add("无");
            string remark = "可登录小未程序技师端查看送花记录.";

            SendTemplateMessage(WebSiteConfig.DZ_multiStore_paySuccessTemplateId, "", tenchnicianInfo.unionId, title, values, remark, "");
        }
        #endregion

        #region 多门店模板消息
        /// <summary>
        /// 支付成功推送订单提醒给商家
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        public static void SendOrderSuccessTemplateMessage(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            List<EntGoodsCart> cartInfos = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderModel.Id}");
            if (cartInfos == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店订单支付成功发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            FootBath store = FootBathBLL.SingleModel.GetModel(orderModel.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店订单支付成功发送模板消息给商家失败：没有找到门店信息 storeId:{store.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店订单支付成功发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店订单支付成功发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            //商品内容
            List<string> goodNames = cartInfos.Select(c => EntGoodsBLL.SingleModel.GetModel(c.FoodGoodsId)?.name)?.ToList() ?? new List<string>();
            string entGoodContent = string.Join(",", goodNames);
            string title = $"您的店铺【{store.StoreName}】收到收货人为[{orderModel.AccepterName}]的新订单！";
            List<string> values = new List<string>();
            values.Add(orderModel.OrderNum);
            values.Add(entGoodContent);
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.AccepterTelePhone);
            values.Add(orderModel.Address);
            string remark = "请登录门店管理后台处理订单";
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", unionId, title, values, remark, "");
            }

            #region old code <- before 20180316
            //string title = $"您的店铺【{store.StoreName}】有新的订单，请及时处理！";
            //List<string> values = new List<string>();
            //values.Add(orderModel.GetWayStr);
            //values.Add(orderModel.AccepterName);
            //values.Add(orderModel.BuyPriceStr);
            //values.Add(orderModel.CreateDateStr);
            //values.Add(orderModel.OrderNum);
            //string remark = "请登录门店管理后台处理订单";
            //SendTemplateMessage(WebSiteConfig.DZ_multiStore_paySuccessTemplateId, "", account.UnionId, title, values, remark, "");
            #endregion
        }
        /// <summary>
        /// 退款通知
        /// </summary>
        /// <param name="orderModel"></param>
        public static void OutOrderTemplateMessage(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            string goodsName = string.Empty;
            orderModel.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderModel.Id}");
            if (orderModel.goodsCarts == null || orderModel.goodsCarts.Count <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店申请退款发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }

            string foodGoodsIds = string.Join(",", orderModel.goodsCarts?.Select(s => s.FoodGoodsId).Distinct());
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(foodGoodsIds);

            orderModel.goodsCarts.ForEach(cart =>
            {
                cart.goodsMsg = entGoodsList?.FirstOrDefault(f => f.id == cart.FoodGoodsId);
                if (cart.goodsMsg != null)
                {
                    goodsName += $"{cart.goodsMsg.name},";
                }
            });

            FootBath store = FootBathBLL.SingleModel.GetModel(orderModel.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店申请退款发送模板消息给商家失败：没有找到门店信息 storeId:{store.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店申请退款发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店申请退款发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"多门店申请退款发送模板消息给商家失败：没有找到客户信息 UserId:{orderModel.UserId}");
                return;
            }
            string title = $"您的店铺【{store.StoreName}】有一笔用户退款申请，请及时处理！";
            List<string> values = new List<string>();
            values.Add(goodsName.TrimEnd(','));
            values.Add(orderModel.goodsCarts.Count.ToString());
            values.Add(orderModel.BuyPriceStr);
            values.Add(userInfo.NickName);
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string remark = "请登录门店管理后台处理订单";
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_multiStore_outOrderTemplateId, "", unionId, title, values, remark, "");
            }
        }

        #endregion

        #region 专业版模板消息
        /// <summary>
        /// 专业版订单下单成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendEntPaySuccessTemplateMessage(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            Store store = StoreBLL.SingleModel.GetModelByRid(orderModel.aId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版申请退款发送模板消息给商家失败：没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            List<EntGoodsCart> cartInfos = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderModel.Id}");
            if (cartInfos == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版订单下单成功发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(orderModel.aId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版订单下单成功发送模板消息给商家失败：没有找到小程序信息 storeId:{orderModel.aId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版订单下单成功发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            store.name = XcxAppAccountRelationBLL.SingleModel.GetStoreName(store.name, relationInfo);
            
            //商品详情  
            List<String> goodsName = cartInfos.Select(c => EntGoodsBLL.SingleModel.GetModel(c.FoodGoodsId)?.name + "x" + c.Count)?.ToList() ?? new List<string>();
            string entGoodContent = string.Join(",", goodsName);
            string orderType = null;
            //订单类型描述
            if (orderModel.OrderType == (int)EntOrderType.拼团订单)
            {
                orderType = "拼团订单";
            }
            else if (orderModel.OrderType == (int)EntOrderType.预约订单)
            {
                orderType = "预约订单";
                orderModel.Address = "到店自取（等待商家接单）";
            }
            else
            {
                orderType = "订单";
            }
            //模板消息参数
            string title = $"您的店铺【{store.name}】收到收货人为[{orderModel.AccepterName}]的新{orderType}！";
            List<string> values = new List<string>();
            values.Add(orderModel.OrderNum);
            values.Add(entGoodContent);
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.AccepterTelePhone);
            values.Add(orderModel.Address);
            string remark = "请登录门店管理后台处理订单";
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                //发送模板消息到unionId
                SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", unionId, title, values, remark, "");
            }
            //发送消息到商家版小程序
            Dictionary<string, object> msgDic = new Dictionary<string, object>();
            msgDic.Add("订单号", orderModel.OrderNum);
            msgDic.Add("配送方式", orderModel.GetWayStr);
            msgDic.Add("提货人姓名", orderModel.AccepterName);
            msgDic.Add("支付金额", orderModel.BuyPriceStr);
            msgDic.Add("购买时间", orderModel.CreateDateStr);
            SystemUpdateMessageBLL.SingleModel.SendOrderMessage(msgDic, orderModel.aId, orderModel.Id);
        }
        /// <summary>
        /// 专业版申请退款模板消息
        /// </summary>
        /// <param name="orderModel"></param>
        public static void OutOrderTemplateMessageForEnt(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            //获取订单来源

            Store store = StoreBLL.SingleModel.GetModelByRid(orderModel.aId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版申请退款发送模板消息给商家失败：没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版申请退款发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版申请退款发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版申请退款发送模板消息给商家失败：没有找到客户信息 UserId:{orderModel.UserId}");
                return;
            }

            store.name = XcxAppAccountRelationBLL.SingleModel.GetStoreName(store.name, relationInfo);

            //订单明细
            //获取购物车记录
            string goodsName = string.Empty;
            orderModel.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderModel.Id}");
            if (orderModel.goodsCarts == null || orderModel.goodsCarts.Count <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版申请退款发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }

            string foodGoodsIds = string.Join(",",orderModel.goodsCarts?.Select(s=>s.FoodGoodsId).Distinct());
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(foodGoodsIds);

            //格式化商品信息
            orderModel.goodsCarts.ForEach(cart =>
            {
                cart.goodsMsg = entGoodsList?.FirstOrDefault(f=>f.id == cart.FoodGoodsId);
                if (cart.goodsMsg != null)
                {
                    goodsName += $"{cart.goodsMsg.name},";
                }
            });

            //发送目标商家
            bool isReservation = orderModel.OrderType == (int)EntOrderType.预约订单 && orderModel.ReserveId > 0;
            string manageTip = isReservation ? "（订单管理 => 预约订单）" : null;
            string orderType = isReservation ? "（预约订单）" : null;
            string title = $"您的店铺【{store.name}】有一笔用户退款申请{orderType}，请及时处理！";
            string remark = $"请登录门店管理后台处理订单{manageTip}";

            List<string> values = new List<string>();
            values.Add(goodsName.TrimEnd(','));
            values.Add(orderModel.goodsCarts.Count.ToString());
            values.Add(orderModel.BuyPriceStr);
            values.Add(userInfo.NickName);
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_multiStore_outOrderTemplateId, "", unionId, title, values, remark, "");
            }
        }

        /// <summary>
        /// 专业版退换货模板消息
        /// </summary>
        /// <param name="orderModel"></param>
        public static void ReturnDeliveryTemplateMsgForEnt(EntGoodsOrder orderModel, MiniAppEntOrderState operState)
        {
            if (orderModel == null)
            {
                return;
            }

            //获取订单来源
            Store store = StoreBLL.SingleModel.GetModelByRid(orderModel.aId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版退换货发送模板消息给商家失败：没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版退换货发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版退换货发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版退换货发送模板消息给商家失败：没有找到客户信息 UserId:{orderModel.UserId}");
                return;
            }

            //获取店铺名（兼容旧数据）
            store.name = XcxAppAccountRelationBLL.SingleModel.GetStoreName(store.name, relationInfo);

            //订单明细
            ReturnGoods returnInfo = ReturnGoodsBLL.SingleModel.GetByOrderId(orderModel.Id);
            //获取购物车记录
            string goodsName = string.Empty;
            if (returnInfo.ReturnType == (int)ReturnGoodsType.专业版退换货)
            {
                orderModel.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"Id IN ({returnInfo.GoodsId})");
            }
            else if (returnInfo.ReturnType == (int)ReturnGoodsType.专业版退货退款)
            {
                orderModel.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderModel.Id}");
            }
            if (orderModel.goodsCarts == null || orderModel.goodsCarts.Count <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"专业版退换货发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            //格式化商品信息
            goodsName = string.Join(",", orderModel.goodsCarts.Select(cartItem => cartItem.GoodName));

            string title = string.Empty;
            switch (operState)
            {
                case MiniAppEntOrderState.退货审核中:
                    title = $"您的店铺【{store.name}】有一笔订单用户申请退货，请及时处理！";
                    break;
                case MiniAppEntOrderState.退货中:
                    title = $"您的店铺【{store.name}】有一笔退货订单用户已发货，请及时处理！";
                    break;
                case MiniAppEntOrderState.退换货成功:
                    title = $"您的店铺【{store.name}】有一笔退换货订单用户已签收！";
                    break;
                default:
                    return;
            }

            //发送目标商家
            string remark = $"请登录门店管理后台处理订单";

            List<string> values = new List<string>();
            values.Add(goodsName.TrimEnd(','));
            values.Add(orderModel.goodsCarts.Count.ToString());
            values.Add(orderModel.BuyPriceStr);
            values.Add(userInfo.NickName);
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_multiStore_outOrderTemplateId, "", unionId, title, values, remark, "");
            }
        }

        /// <summary>
        /// 产品预约通知商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void ReserveInformTemplateMessageForEnt(EntUserForm form)
        {
            if (form == null)
            {
                return;
            }

            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModel(form.aid);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到小程序权限信息：Id = {form.aid}"));
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel(relationInfo.AccountId);
            if (account == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到小程序拥有者信息：Id = {relationInfo.AccountId}"));
                return;
            }
            Store store = StoreBLL.SingleModel.GetModelByRid(relationInfo.Id);
            if (store == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到店铺资料：Id = {relationInfo.Id}"));
                return;
            }

            store.name = XcxAppAccountRelationBLL.SingleModel.GetStoreName(store.name, relationInfo);

            try
            {
                form.formremark = JsonConvert.DeserializeObject<EntFormRemark>(form.remark);
            }
            catch (Exception)
            {
                form.formremark = new EntFormRemark();
            }

            string typename = form.type == 0 ? "表单资料" : "产品预约";
            string title = $"您的店铺【{store.name}】有用户提交了新的{typename}！";
            string reservationName = string.Empty;//预约项目
            string reservationTime = string.Empty;//预约时间
            string remark = $"请登录门店管理后台处理{typename}";
            try
            {
                //表单数据json
                JObject formJson = JObject.Parse(form.formdatajson);
                //截取预约时间
                reservationTime = formJson["预约时间"]?.ToString() ?? string.Empty;

                if (form.type == 0) //自定义表单
                {
                    reservationName = form.pagename;
                }
                else if (form.type == 1)
                {
                    //备注json
                    JObject remarkJson = JObject.Parse(form.remark);
                    if (remarkJson != null && remarkJson["goods"] != null)
                    {
                        JObject goodJson = JObject.Parse(remarkJson["goods"].ToString());
                        if (goodJson != null)
                        {
                            reservationName = formJson["name"]?.ToString() ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：预约表单内容缺失 ：Id = {form.id}"));
                return;
            }

            List<string> values = new List<string>();
            values.Add(title);
            values.Add(reservationName);
            values.Add(reservationTime);
            values.Add(remark);
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_ReserveInformTemplateId, "", unionId, title, values, remark, "");
            }
        }

        /// <summary>
        /// 申请取消订单通知商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void ApplyCancelOrderTemplateMessageForEnt(EntGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModel(orderModel.aId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 产品预约：找不到小程序权限信息：Id = {orderModel.aId}"));
                return;
            }
            
            string name = XcxAppAccountRelationBLL.SingleModel.GetAppName(0,relationInfo);
            
            Account account = AccountBLL.SingleModel.GetModel(relationInfo.AccountId);
            if (account == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Miniapp), new Exception($"专业版 - 申请取消订单：找不到小程序拥有者信息：Id = {relationInfo.AccountId}"));
                return;
            }
            
            string title = $"您的小程序【{name}】有用户申请取消订单了！";
            List<string> values = new List<string>();
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));//提交时间
            values.Add(orderModel.OrderNum);//订单编号
            values.Add("申请取消订单");//订单状态
            values.Add(name);//订单来源
            values.Add(orderModel.Remark);//订单详情
            string remark = "申请取消订单两个小时没有处理，则系统默认退款给用户，请尽快登录管理后台处理订单";
            SendTemplateMessage(_CancelOrderTemplateId, "", "", title, values, remark, account.OpenId);
        }
        #endregion

        #region 电商版模板消息
        /// <summary>
        /// 专业版订单下单成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendStorePaySuccessTemplateMessage(StoreGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到订单 orderId_null");
                return;
            }
            
            Store storeModel = StoreBLL.SingleModel.GetModel(orderModel.StoreId);
            if (storeModel == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到店铺信息 storeId:{orderModel.StoreId}");
                return;
            }
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(storeModel.appId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到授权信息 appId:{storeModel.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{xcx.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到商家信息 accountId:{xcx.AccountId}");
                return;
            }

            List<StoreGoodsCart> carts = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderModel.Id} and state = 1");
            if (carts == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }

            string goodsIds = string.Join(",",carts.Select(s=>s.GoodsId).Distinct());
            List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);
            foreach (StoreGoodsCart c in carts)
            {
                c.goodsMsg = storeGoodsList?.FirstOrDefault(f => f.Id == c.GoodsId);
                if (c.goodsMsg == null)
                {
                    log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到商品明细 goodsId:{c.GoodsId}");
                    return;
                }
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"电商版订单下单成功发送模板消息给商家失败：没有找到用户信息 userId:{orderModel.UserId}");
                return;
            }

            string entGoodContent = string.Join(",", carts?.Select(x => x.goodsMsg?.GoodsName));//商品名称
            string title = $"您的店铺【{storeModel.name}】收到收货人为[{orderModel.AccepterName}]的新订单！";
            List<string> values = new List<string>();
            values.Add(orderModel.OrderNum);
            values.Add(entGoodContent);
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.AccepterTelePhone);
            values.Add(orderModel.Address);
            string remark = "请登录门店管理后台处理订单";

            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(xcx.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", unionId, title, values, remark, "");
            }
        }
        #endregion

        #region 餐饮版模板消息
        /// <summary>
        /// 订单支付成功通知
        /// </summary>
        /// <param name="goodsOrder"></param>
        /// <param name="account"></param>
        /// <param name="store"></param>
        public static void SendOrderSuccessTemplateMessage(FoodGoodsOrder goodsOrder, Account account, Food store)
        {
            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(goodsOrder.UserId);
            
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版订单支付成功发送模板消息给商家失败：没有找到商家信息 account is null");
                return;
            }
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版订单支付成功发送模板消息给商家失败：没有找到门店信息 store is null");
                return;
            }
            List<FoodGoodsCart> cartList = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={goodsOrder.Id} and state=1");

            string goodsIds = string.Join(",",cartList?.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);

            cartList?.ForEach(cart =>
            {
                cart.goodsMsg = foodGoodsList?.FirstOrDefault(f=>f.Id == cart.FoodGoodsId);
            });
            if (goodsOrder.OrderType == 0)
            {
                goodsOrder.attrbuteModel = string.IsNullOrEmpty(goodsOrder.attribute) ? new FoodGoodsOrderAttr() : JsonConvert.DeserializeObject<FoodGoodsOrderAttr>(goodsOrder.attribute);
                goodsOrder.AccepterName = "匿名";
                goodsOrder.AccepterTelePhone = "无填写手机号";
                if (goodsOrder.attrbuteModel.isNewTableNo)
                {
                    FoodTable foodTable = FoodTableBLL.SingleModel.GetModelById(goodsOrder.TablesNo);
                    if (foodTable == null)
                    {
                        goodsOrder.Address = $"桌号未知";
                    }
                    else
                    {
                        goodsOrder.Address = $"桌台：{foodTable.Scene}";
                    }
                }
                else
                {
                    if (goodsOrder.TablesNo != -1)//若为-1,表示该桌没有选桌
                    {
                        goodsOrder.Address = $"{goodsOrder.TablesNo}号桌";
                    }
                    else
                    {
                        goodsOrder.Address = "无填写地址/桌号";
                    }
                }

            }
            string entGoodContent = string.Join(",", cartList?.Select(x => x.goodsMsg?.GoodsName));//商品名称
            string title = $"您的店铺【{store.FoodsName}】收到收货人为[{goodsOrder.AccepterName}]的新订单！";
            List<string> values = new List<string>();
            values.Add(goodsOrder.OrderNum);
            values.Add(entGoodContent);
            values.Add(goodsOrder.BuyPriceStr);
            values.Add(goodsOrder.AccepterTelePhone);
            values.Add(goodsOrder.Address);
            string remark = "请登录门店管理后台处理订单";

            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", unionId, title, values, remark, "");
            }
        }

        /// <summary>
        /// 退款通知
        /// </summary>
        /// <param name="orderModel"></param>
        public static void OutOrderTemplateMessage(FoodGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }
            
            string goodsName = string.Empty;
            
            Food store = FoodBLL.SingleModel.GetModel(orderModel.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版申请退款发送模板消息给商家失败：没有找到门店信息 storeId:{store.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版申请退款发送模板消息给商家失败：没有找到小程序信息 storeId:{store.appId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版申请退款发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版申请退款发送模板消息给商家失败：没有找到客户信息 UserId:{orderModel.UserId}");
                return;
            }
            List<FoodGoodsCart> goodsCartList = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {orderModel.Id} ");
            if (goodsCartList == null || goodsCartList.Count <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"餐饮版申请退款发送模板消息给商家失败：没有找到订单明细 GoodsOrderId:{orderModel.Id}");
                return;
            }

            string foodGoodsIds = string.Join(",", goodsCartList.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(foodGoodsIds);
            goodsCartList.ForEach(cart =>
            {
                cart.goodsMsg = foodGoodsList?.FirstOrDefault(f=>f.Id ==cart.FoodGoodsId);
            });
            goodsName = string.Join(",", goodsCartList?.Select(cart => cart.goodsMsg?.GoodsName));
            int sum = goodsCartList.Sum(x => x.Count);
            string title = $"您的店铺【{store.FoodsName}】有顾客申请退款,请及时处理！";
            List<string> values = new List<string>();
            values.Add(goodsName);
            values.Add($"总共 {sum} 件商品");
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.AccepterName);
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string remark = $"该订单于 {orderModel.CreateDate.ToString("yyyy-MM-dd HH: mm")} 下单. ";

            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_outOrderTemplateId, "", unionId, title, values, remark, "");
            }
        }
        #endregion

        #region 基础版拼团<团购> 模板消息
        /// <summary>
        /// 基础版拼团<团购>订单下单成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendGroupsPaySuccessTemplateMessage(GroupUser groupUser)
        {
            if (groupUser == null)
            {
                return;
            }
            
            Groups group = GroupsBLL.SingleModel.GetModel(groupUser.GroupId);
            if (group == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"基础版拼团订单下单成功发送模板消息给商家失败：没有找到拼团商品 groupId:{groupUser.GroupId}"));
                return;
            }
            Store store = StoreBLL.SingleModel.GetModel(group.StoreId);
            if (store == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"基础版拼团订单下单成功发送模板消息给商家失败：没有找到店铺信息 storeId:{group.StoreId}"));
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(store.appId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"基础版拼团订单下单成功发送模板消息给商家失败：没有找到小程序资料 relationInfoId:{store.appId}"));
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel($"id='{relationInfo.AccountId}'");
            if (account == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"基础版拼团订单下单成功发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}"));
                return;
            }

            store.name = XcxAppAccountRelationBLL.SingleModel.GetStoreName("", relationInfo);

            string title = $"您的店铺【{store.name}】收到收货人为[{groupUser.UserName}]的新团购订单！";
            List<string> values = new List<string>();
            values.Add(groupUser.OrderNo);
            values.Add(group.GroupName);
            values.Add((groupUser.BuyPrice * 0.01).ToString("0.00"));
            values.Add(groupUser.Phone);
            values.Add(groupUser.Address);
            string remark = "请登录门店管理后台处理订单";

            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", unionId, title, values, remark, "");
            }
        }

        #endregion

        #region 砍价 模板消息
        /// <summary>
        /// 砍价订单下单成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendBargainPaySuccessTemplateMessage(BargainUser bargainUser)
        {
            if (bargainUser == null)
            {
                return;
            }

            Bargain bargain = BargainBLL.SingleModel.GetModel(bargainUser.BId);
            if (bargain == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"砍价订单下单成功发送模板消息给商家失败：没有找到拼团商品 BargainId:{bargainUser.BId}"));
                return;
            }
            //小程序
            int aId = 0;
            string storeName = string.Empty;
            switch (bargain.BargainType)
            {
                case 0:
                    Store store = StoreBLL.SingleModel.GetModel(bargain.StoreId);
                    if (store != null)
                    {
                        aId = store.appId;
                        storeName = store.name;
                    }
                    break;
                    
                default:
                    aId = bargain.StoreId;
                    break;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(aId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"砍价订单下单成功发送模板消息给商家失败：没有找到小程序资料 relationInfoId:{aId}"));
                return;
            }

            storeName = XcxAppAccountRelationBLL.SingleModel.GetStoreName(storeName, relationInfo);

            Account account = AccountBLL.SingleModel.GetModel(relationInfo.AccountId);
            if (account == null)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"砍价订单下单成功发送模板消息给商家失败：没有找到商家信息 accountId:{relationInfo.AccountId}"));
                return;
            }
            string title = $"您的店铺【{storeName}】收到收货人为[{bargainUser.Name}]的新砍价订单！";
            List<string> values = new List<string>();
            values.Add(bargainUser.OrderId);
            values.Add(bargainUser.BName);
            values.Add(((bargainUser.CurrentPrice + bargain.GoodsFreight) * 0.01).ToString("0.00"));
            values.Add(bargainUser.TelNumber);
            values.Add(bargainUser.AddressDetail);
            string remark = "请登录门店管理后台处理订单";
            //获取UnionId
            List<string> unionIds = GetUnionIdForTemplate(relationInfo.agentId, account);
            foreach (var unionId in unionIds)
            {
                SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", unionId, title, values, remark, "");
            }
        }

        #endregion

        #region 独立小程序模板消息
        /// <summary>
        /// 订单下单成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendPlatChildPaySuccessTemplateMessage(PlatChildGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }
            
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(orderModel.AId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单下单成功提醒商家失败，没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            List<PlatChildGoodsCart> cartlist = PlatChildGoodsCartBLL.SingleModel.GetListByOrderIds(orderModel.Id.ToString());
            if (cartlist == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单下单成功提醒商家失败，没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(orderModel.AId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单下单成功提醒商家失败，没有找到小程序信息 aid:{orderModel.AId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModelByStringId(relationInfo.AccountId.ToString());
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单下单成功提醒商家失败，没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            store.Name = XcxAppAccountRelationBLL.SingleModel.GetStoreName("", relationInfo);

            //商品详情  
            List<String> goodsName = cartlist.Select(c => c.GoodsName + "x" + c.Count)?.ToList() ?? new List<string>();
            string entGoodContent = string.Join(",", goodsName);
            //订单类型描述
            string orderType = "订单";
            //模板消息参数
            string title = $"您的店铺【{store.Name}】收到收货人为[{orderModel.AccepterName}]的新{orderType}！";
            List<string> values = new List<string>();
            values.Add(orderModel.OrderNum);
            values.Add(entGoodContent);
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.AccepterTelePhone);
            values.Add(orderModel.Address);
            string remark = "请登录小未管理后台处理订单";
            SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", "", title, values, remark, account.OpenId);
        }
        /// <summary>
        /// 子模板申请退款模板消息
        /// </summary>
        /// <param name="orderModel"></param>
        public static void ReturnOrderTemplateMessageForPlatChild(PlatChildGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }
            
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(orderModel.AId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单退款成功提醒商家失败，没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(orderModel.AId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单退款成功提醒商家失败，没有找到小程序信息 aid:{orderModel.AId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModelByStringId(relationInfo.AccountId.ToString());
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单退款成功提醒商家失败，没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单退款成功提醒商家失败，没有找到客户信息 UserId:{orderModel.UserId}");
                return;
            }
            store.Name = XcxAppAccountRelationBLL.SingleModel.GetStoreName("", relationInfo);
            //订单明细
            //获取购物车记录
            string goodsName = string.Empty;
            orderModel.CartList = PlatChildGoodsCartBLL.SingleModel.GetListByOrderIds(orderModel.Id.ToString());
            if (orderModel.CartList == null || orderModel.CartList.Count <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单退款成功提醒商家失败，没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            //格式化商品信息
            orderModel.CartList.ForEach(cart =>
            {
                goodsName += $"{cart.GoodsName},";
            });

            //发送目标商家
            string title = $"您的店铺【{store.Name}】有一笔用户退款申请订单，请及时处理！";
            string remark = $"请登录小未管理后台处理订单";

            List<string> values = new List<string>();
            values.Add(goodsName.TrimEnd(','));
            values.Add(orderModel.CartList.Count.ToString());
            values.Add(orderModel.BuyPriceStr);
            values.Add(userInfo.NickName);
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            SendTemplateMessage(WebSiteConfig.DZ_multiStore_outOrderTemplateId, "", "", title, values, remark, account.OpenId);
        }
        #endregion

        #region 企业智推版模板消息
        /// <summary>
        /// 订单下单成功提醒商家
        /// </summary>
        /// <param name="orderModel"></param>
        public static void SendQiyePaySuccessTemplateMessage(QiyeGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(orderModel.AId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单下单成功提醒商家失败，没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            List<QiyeGoodsCart> cartlist = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(orderModel.Id.ToString());
            if (cartlist == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单下单成功提醒商家失败，没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(orderModel.AId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单下单成功提醒商家失败，没有找到小程序信息 aid:{orderModel.AId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModelByStringId(relationInfo.AccountId.ToString());
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单下单成功提醒商家失败，没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }

            store.Name = XcxAppAccountRelationBLL.SingleModel.GetStoreName("", relationInfo);

            //商品详情  
            List<string> goodsName = cartlist.Select(c => c.GoodsName + "x" + c.Count)?.ToList() ?? new List<string>();
            string entGoodContent = string.Join(",", goodsName);
            //订单类型描述
            string orderType = "订单";
            //模板消息参数
            string title = $"您的店铺【{store.Name}】收到收货人为[{orderModel.AccepterName}]的新{orderType}！";
            List<string> values = new List<string>();
            values.Add(orderModel.OrderNum);
            values.Add(entGoodContent);
            values.Add(orderModel.BuyPriceStr);
            values.Add(orderModel.AccepterTelePhone);
            values.Add(orderModel.Address);
            string remark = "请登录小未管理后台处理订单";
            SendTemplateMessage(WebSiteConfig.DZ_paySuccessTemplateId_new, "", "", title, values, remark, account.OpenId);
        }
        /// <summary>
        /// 企业智推版申请退款模板消息
        /// </summary>
        /// <param name="orderModel"></param>
        public static void ReturnOrderTemplateMessageForQiye(QiyeGoodsOrder orderModel)
        {
            if (orderModel == null)
            {
                return;
            }

            QiyeStore store = QiyeStoreBLL.SingleModel.GetModelByAId(orderModel.AId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单退款成功提醒商家失败，没有找到门店信息 storeId:{orderModel.StoreId}");
                return;
            }
            XcxAppAccountRelation relationInfo = XcxAppAccountRelationBLL.SingleModel.GetModelById(orderModel.AId);
            if (relationInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单退款成功提醒商家失败，没有找到小程序信息 aid:{orderModel.AId}");
                return;
            }
            Account account = AccountBLL.SingleModel.GetModelByStringId(relationInfo.AccountId.ToString());
            if (account == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单退款成功提醒商家失败，没有找到商家信息 accountId:{relationInfo.AccountId}");
                return;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(orderModel.UserId);
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：独立小程序订单退款成功提醒商家失败，没有找到客户信息 UserId:{orderModel.UserId}");
                return;
            }
            store.Name = XcxAppAccountRelationBLL.SingleModel.GetStoreName("", relationInfo);
            //订单明细
            //获取购物车记录
            string goodsName = string.Empty;
            orderModel.CartList = QiyeGoodsCartBLL.SingleModel.GetListByOrderIds(orderModel.Id.ToString());
            if (orderModel.CartList == null || orderModel.CartList.Count <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"模板消息：企业智推版订单退款成功提醒商家失败，没有找到订单明细 orderId:{orderModel.Id}");
                return;
            }
            //格式化商品信息
            orderModel.CartList.ForEach(cart =>
            {
                goodsName += $"{cart.GoodsName},";
            });

            //发送目标商家
            string title = $"您的店铺【{store.Name}】有一笔用户退款申请订单，请及时处理！";
            string remark = $"请登录小未管理后台处理订单";

            List<string> values = new List<string>();
            values.Add(goodsName.TrimEnd(','));
            values.Add(orderModel.CartList.Count.ToString());
            values.Add(orderModel.BuyPriceStr);
            values.Add(userInfo.NickName);
            values.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            SendTemplateMessage(WebSiteConfig.DZ_multiStore_outOrderTemplateId, "", "", title, values, remark, account.OpenId);
        }
        #endregion

        #region 拼享惠模板消息
        public static void SendPaySuccessTemplateMessage(PinGoodsOrder order)
        {

            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(order.aid, order.storeId);
            if (store == null && order.orderType == 0)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), new Exception($"拼享惠订单配送模板消息失败，找不到店铺信息 aid:{order.aid}, storeid：{order.storeId}"));
                return;
            }
            string templateId = "npHt7qPe12lg-ZRlLESa9GES3Nzpg5EhmkByM3-U3W4";
            var appId = Config.SenparcWeixinSetting.WeixinAppId;
            string title = $"你的店铺【{store.storeName}】有用户支付订单";
            string remark = $"请及时处理订单";
            List<string> vals = new List<string>();
            vals.Add($"{order.consignee}");
            vals.Add($"{order.outTradeNo}");
            vals.Add($"{order.money}");
            string goodsStr = order.goodsPhotoModel.name;
            if (order.specificationPhotoModel != null)
            {
                goodsStr += $" {order.specificationPhotoModel.name}";
            }
            vals.Add($"{goodsStr}");
            var data = GetTempateData(title, vals, remark);
            var result = TemplateApi.SendTemplateMessage(appId, store.wxOpenId, templateId, "", data);
        }
        #endregion

        #region 智慧餐厅模板消息
        /// <summary>
        /// 支付成功推送订单提醒给商家
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        public static void SendOrderSuccessTemplateMessage(DishOrder orderModel, DishStore store)
        {
            if (orderModel == null || store == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(store.notifyOpenId))
            {
                log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"智慧餐厅订单支付成功发送模板消息给商家失败：没有绑定模板消息接收者 storeId:{store.id}");
                return;
            }
            //商品内容
            string orderContent = string.Empty;
            orderModel.carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(orderModel.id) ?? new List<DishShoppingCart>();
            if (orderModel.carts.Count > 0)
            {
                orderContent = string.Join("\r\n", orderModel.carts.Select(p => p.goods_name + (string.IsNullOrEmpty(p.goods_attr) ? "" : "(" + p.goods_attr + ")") + "x" + p.goods_number));
            }


            string title = $"您的店铺【{store.dish_name}】收到新的订单！";
            List<string> values = new List<string>();
            values.Add(orderModel.order_type_txt);
            values.Add(orderModel.user_name);
            values.Add(orderModel.order_amount.ToString("0.00"));
            values.Add(orderModel.add_time_txt);
            values.Add(orderModel.order_sn);
            string remark = $"支付方式：{orderModel.pay_name} ";
            if (orderModel.order_type == (int)DishEnums.OrderType.外卖)
            {
                remark += $"\r\n地址：{orderModel.address}\r\n收货人：{orderModel.consignee}\r\n电话：{orderModel.mobile}";
            }
            SendTemplateMessage(WebSiteConfig.DZ_multiStore_paySuccessTemplateId, "", "", title, values, remark, store.notifyOpenId);
        }
        /// <summary>
        /// 到店买单支付成功通知
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="storeId"></param>
        /// <param name="addtime"></param>
        /// <param name="money">实付金额</param>
        /// <param name="discount">优惠金额</param>
        /// <param name="price">应付金额</param>
        public static void SendOrderSuccessTemplateMessage(string username,double money, int storeId, DateTime addtime, string payType, double price, double discount)
        {
            DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
            if (store == null)
            {
                return;
            }
            string title = $"您的店铺【{store.dish_name}】收到一笔门店买单付款";
            List<string> values = new List<string>();
            values.Add(addtime.ToString("yyyy-MM-dd HH:mm:ss"));
            values.Add(money.ToString("0.00"));
            values.Add(payType);

            string remark = $"应付总额：{price.ToString("0.00")}\r\n优惠金额：{discount.ToString("0.00")}\r\n付款人：{username}";

            SendTemplateMessage(WebSiteConfig.DZ_payTipsTemplateId, "", "", title, values, remark, store.notifyOpenId);
        }
        #endregion
    
        public static List<string> GetUnionIdForTemplate(int agentId, Account account)
        {
            List<string> unionIds = null;
            //代理商转发模板消息
            if (agentId > 0)
            {
                List<MsgAccount> msgAccounts = MsgAccountBLL.SingleModel.GetListByManagerAgent(agentId: agentId, managerId: account.Id);
                unionIds = msgAccounts.Select(thisAccount => thisAccount.UnionId).ToList();
            }
            //默认发给管理者
            if (unionIds == null || unionIds.Count == 0)
            {
                unionIds = new List<string> { account.UnionId };
            }
            return unionIds;
        }

        /// <summary>
        /// 拼接模板消息推送数据(通用方法)
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="url"></param>
        /// <param name="openId"></param>
        /// <param name="title"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GetTemplateModuleMsg(string templateId, string url, string openId, string title, List<string> values, string remark)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("first", new { color = "#111111", value = title });
            for (int i = 1; i <= values.Count; i++)
            {
                dic.Add($"keyword{i}", new { color = "#333333", value = values[i - 1] });
            }
            dic.Add("remark", new { color = "#333333", value = remark });
            return SerializeHelper.SerToJson(new
            {
                touser = openId,
                template_id = templateId,
                url = url,
                data = dic
            });
        }

        public static object GetTempateData(string title, List<string> values, string remark)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("first", new { color = "#111111", value = title });
            for (int i = 1; i <= values.Count; i++)
            {
                dic.Add($"keyword{i}", new { color = "#333333", value = values[i - 1] });
            }
            dic.Add("remark", new { color = "#333333", value = remark });
            return dic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateId">模板消息id</param>
        /// <param name="unionId">如果没有传openId，则通过unionId去找openId</param>
        /// <param name="title">模板消息标题</param>
        /// <param name="values">模板消息内容，按顺序排列</param>
        /// <param name="remark">模板消息的备注</param>
        /// <param name="openId">如果有传openId则直接调用openId</param>
        private static void SendTemplateMessage(string templateId, string url, string unionId, string title, List<string> values, string remark, string openId = "")
        {

            if (string.IsNullOrEmpty(openId))
            {
                UserBaseInfo baseInfo = UserBaseInfoBLL.SingleModel.GetModelByUnionidServerid(unionId, WebSiteConfig.DZ_WxSerId);
                if (baseInfo == null)
                {
                    log4net.LogHelper.WriteInfo(typeof(TemplateMsg_Gzh), $"发送模板消息给商家失败：没有找到商家用户基础信息 unionId:{unionId},serverId:{WebSiteConfig.DZ_WxSerId}");
                    return;
                }
                openId = baseInfo.openid;
            }

            try
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if (!string.IsNullOrEmpty(openId) && values != null && values.Count > 0)
                    {
                        string strMsg, urls;
                        urls = string.Format(WebSiteConfig.OpenDomain + "/cgibin/message/template?access_token={0}", WebSiteConfig.DZ_WxSerId);
                        strMsg = GetTemplateModuleMsg(templateId, url, openId, title, values, remark);
                        string result = HttpHelper.PostData(urls, strMsg);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(TemplateMsg_Gzh), ex);
            }
        }
    }
}