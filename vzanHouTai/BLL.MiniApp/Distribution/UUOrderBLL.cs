using BLL.MiniApp.Conf;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class UUOrderBLL : BaseMySql<UUOrder>
    {
        #region 单例模式
        private static UUOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private UUOrderBLL()
        {

        }

        public static UUOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UUOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public UUOrder GetModelByOrderCode(string order_code)
        {
            return base.GetModel($"order_code='{order_code}'");
        }

        /// <summary>
        /// aid+storeid+orderid获取UU配送唯一订单
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public UUOrder GetModelByOrerId(int aid,int storeid,int orderid)
        {
            return GetModel($"aid={aid} and storeid={storeid} and orderid={orderid}");
        }

        /// <summary>
        /// 生成UU配送订单
        /// </summary>
        /// <returns></returns>
        public string AddUUOrder(DistributionApiModel model)
        {
            string msg = "";
            UUCustomerRelation customerRelation = UUCustomerRelationBLL.SingleModel.GetModelByAid(model.aid, model.storeid,0);
            if (customerRelation == null)
            {
                msg = "请先配置UU配送";
                return msg;
            }
            UUOrderFee feeResult= GetUUFee(model.storeid,model.aid, model.address,ref msg);
            if(msg.Length>0)
            {
                return msg;
            }
            string orderNum = CommonCore.GetOrderNumByUserId(model.userid.ToString());
            string appid = UUApi._appid;
            string timestamp = UUApi.GetTimestamp();
            //店铺信息
            ShopInfo shopInfo = GetStoreAddressPoint(customerRelation.AId, customerRelation.StoreId, model.temptype);

            //生成订单实体类对象
            UUOrder data = new UUOrder(appid,timestamp,customerRelation.OpenId,feeResult.price_token,feeResult.total_money,feeResult.need_paymoney,model.accepterName,model.accepterTelePhone,model.remark,UUApi._notisUrl,customerRelation.AId,customerRelation.StoreId,model.temptype,model.ordertype,model.orderid, orderNum);
            
            TransactionModel tran = new TransactionModel();
            tran.Add(base.BuildAddSql(data));

            if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            {
                msg = "添加UU配送订单出错";
            }
            return msg;
        }

        /// <summary>
        /// 获取UU配送运费
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="aid"></param>
        /// <param name="lat"></param>
        /// <param name="lnt"></param>
        /// <param name="orderPrice"></param>
        /// <returns></returns>
        public UUOrderFee GetUUFee(int storeId, int aid, string toAddress,ref string msg)
        {
            msg = "获取运费出错";
            UUOrderFee feeResult = new UUOrderFee();
            UUCustomerRelation customerRelation = UUCustomerRelationBLL.SingleModel.GetModelByAid(aid,storeId, 0);
            if(customerRelation==null)
            {
                msg = "UU配送:未设置UU配置";
                return feeResult;
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                msg = "UU配送:权限模板不存在";
                return feeResult;
            }

            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (xcxtemplate == null)
            {
                msg = "UU配送:小程序模板不存在";
                return feeResult;
            }
            
            //店铺信息
            ShopInfo shopinfo = GetStoreAddressPoint(aid, storeId, xcxtemplate.Type);

            UUGetPriceResult result = UUApi.GetOrderPrice(customerRelation.OpenId, shopinfo.ShopAddress,"",toAddress,"",shopinfo.CityName,shopinfo.CountyName);

            if (result == null)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：返回结果为null，");
                return feeResult;
            }
            if (result.return_code != "ok")
            {
                LogHelper.WriteInfo(this.GetType(), $"UU配送配送：请求参数，aid【{aid}】storeid【{storeId}】" + JsonConvert.SerializeObject(result));
                msg = result.return_msg;
                return feeResult;
            }
            else
            {
                decimal maxFee = Math.Max(Convert.ToDecimal(result.total_money), Convert.ToDecimal(result.need_paymoney));
                int fee = Convert.ToInt32(maxFee * 100);
                feeResult.Fee = fee;
                feeResult.price_token = result.price_token;
                feeResult.total_money = result.total_money;
                feeResult.need_paymoney = result.need_paymoney;
                msg = "";
                return feeResult;
            }
        }

        /// <summary>
        /// 获取店铺信息
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <param name="temptype"></param>
        /// <returns></returns>
        public ShopInfo GetStoreAddressPoint(int aid, int storeid, int temptype)
        {
            ShopInfo shopinfo = new ShopInfo();
            switch (temptype)
            {
                case (int)TmpType.小程序餐饮模板:
                    shopinfo = FoodBLL.SingleModel.GetAddressPoint(aid); break;
                case (int)TmpType.智慧餐厅:
                    shopinfo = DishStoreBLL.SingleModel.GetAddressPoint(storeid); break;
            }

            if(shopinfo!=null)
            {
                //请求腾讯地图，获取城市和区域
                AddressApi addressInfo = AddressHelper.GetAddressByApi(shopinfo.Lng,shopinfo.Lat);
                if (addressInfo == null || addressInfo.result == null || addressInfo.result.address_component == null)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(),"UU配送调用腾讯地图获取店铺地址异常："+JsonConvert.SerializeObject(addressInfo));
                    return shopinfo;
                }

                shopinfo.CityName = addressInfo.result.address_component.city;
                shopinfo.CountyName = addressInfo.result.address_component.district;
            }

            return shopinfo;
        }

        /// <summary>
        /// 处理餐饮UU配送回调
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool UUToFoodReturn(UUOrder order)
        {
            

            bool isSuccess = false;
            string updatesql = "State";
            FoodGoodsOrder foodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel(order.OrderId);// and  OrderType ={(int)miniAppFoodOrderType.店内点餐 } ");
            if (foodGoodOrder == null)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：找不到餐饮订单");
                return isSuccess;
            }

            string msg = string.Empty;
            switch (order.State)
            {
                case (int)UUOrderEnum.订单取消:
                    isSuccess = base.Update(order, updatesql);
                    if (!isSuccess)
                    {
                        LogHelper.WriteInfo(this.GetType(), "UU配送：修改系统订单状态出错," + JsonConvert.SerializeObject(order));
                        return isSuccess;
                    }

                    //退款接口 abel
                    //判断是否是取消订单，取消订单则要执行退款
                    if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                    {
                        isSuccess = FoodGoodsOrderBLL.SingleModel.outOrder(foodGoodOrder, foodGoodOrder.State);
                    }
                    else if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                    {
                        SaveMoneySetUser userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(foodGoodOrder.UserId) ?? new SaveMoneySetUser();
                        isSuccess = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(foodGoodOrder, userSaveMoney, foodGoodOrder.State);
                    }

                    #region 餐饮退款成功通知 模板消息
                    if (isSuccess)
                    {
                        object postData2 = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(foodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮退款成功通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(foodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮退款成功通知, (int)TmpType.小程序餐饮模板, postData2);
                    }

                    #endregion
                    return isSuccess;
                case (int)UUOrderEnum.下单成功:
                case (int)UUOrderEnum.跑男抢单:
                case (int)UUOrderEnum.已到达:
                case (int)UUOrderEnum.已取件:
                    updatesql += ",driver_name,driver_mobile,driver_jobnum";
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待送餐;
                    if(order.State== (int)UUOrderEnum.已取件)
                    {
                        foodGoodOrder.State = (int)miniAppFoodOrderState.待确认送达;
                        #region 发送餐饮订单配送通知 模板消息
                        C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(foodGoodOrder.UserId) ?? new C_UserInfo();
                        object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(foodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(userinfo.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知, (int)TmpType.小程序餐饮模板, postData);
                        #endregion
                    }
                    break;
                case (int)UUOrderEnum.到达目的地:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待确认送达;
                    break;
                case (int)UUOrderEnum.收件人已收货:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.已完成;
                    break;
            }
            isSuccess = base.Update(order, updatesql);
            if (!isSuccess)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：修改系统订单状态出错，" + JsonConvert.SerializeObject(order));
                return isSuccess;
            }
            isSuccess = FoodGoodsOrderBLL.SingleModel.Update(foodGoodOrder, "State");

            return isSuccess;
        }

        /// <summary>
        /// 处理智慧餐厅UU配送回调
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool UUToMutilFoodReturn(UUOrder order)
        {
            bool isSuccess = false;
            string updatesql = "State";
            DishOrder dishOrder = DishOrderBLL.SingleModel.GetModel(order.OrderId);
            if (dishOrder == null)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：找不到智慧餐厅订单");
                return isSuccess;
            }

            string msg = string.Empty;
            switch (order.State)
            {
                case (int)UUOrderEnum.订单取消:
                    isSuccess = base.Update(order, updatesql);
                    if (!isSuccess)
                    {
                        LogHelper.WriteInfo(this.GetType(), "UU配送：修改UU订单状态出错，" + JsonConvert.SerializeObject(order));
                        return isSuccess;
                    }

                    //退款接口 abel
                    //判断是否是取消订单，取消订单则要执行退款
                    if (dishOrder.pay_status == (int)DishEnums.PayState.已付款)
                    {
                        DishReturnMsg result = new DishReturnMsg();
                        DishOrderBLL.SingleModel.RefundOrderById(order.OrderId, result);
                        isSuccess = result.code == 1;
                        if (isSuccess)
                        {
                            LogHelper.WriteInfo(this.GetType(), result.msg);
                        }
                    }

                    return isSuccess;
                case (int)UUOrderEnum.跑男抢单:
                case (int)UUOrderEnum.已到达:
                    updatesql += ",driver_name,driver_mobile,driver_jobnum";
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.待取货;
                    dishOrder.peisong_open = 1;
                    dishOrder.peisong_user_name = order.driver_name;
                    dishOrder.peisong_user_phone = order.driver_mobile;
                    #region 发送餐饮订单配送通知 模板消息

                    #endregion
                    break;
                case (int)UUOrderEnum.已取件:
                case (int)UUOrderEnum.到达目的地:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.配送中;
                    break;
                case (int)UUOrderEnum.收件人已收货:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.已完成;
                    break;
            }
            isSuccess = base.Update(order, updatesql);
            if (!isSuccess)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：修改智慧餐厅订单状态出错:" + JsonConvert.SerializeObject(order));
                return isSuccess;
            }
            isSuccess = DishOrderBLL.SingleModel.Update(dishOrder, "peisong_status,peisong_open,peisong_user_name,peisong_user_phone");

            return isSuccess;
        }

        /// <summary>
        /// UU配送推单
        /// </summary>
        /// <param name="orderId">小程序订单表ID</param>
        /// <param name="rid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public string GetUUOrderUpdateSql(int orderId, int aid, int storeId, ref TransactionModel tran, bool getTranSql = false)
        {
            UUOrder order = GetModelByOrerId(aid, storeId, orderId);
            if (order == null)
            {
                return "UU配送：订单不存在（推单）";
            }
            UUCustomerRelation customerRelation = UUCustomerRelationBLL.SingleModel.GetModelByAid(aid, storeId, 0);
            if (customerRelation == null)
            {
                return "UU配送:未设置UU配置（推单）";
            }

            UUBaseResult result = UUApi.AddOrder(order);
            if (result == null)
            {
                return "UU配送:推送订单接口异常（推单）";
            }

            if (result.return_code == "ok")
            {
                order.State = (int)UUOrderEnum.下单成功;
                if (result == null || string.IsNullOrEmpty(result.ordercode))
                {
                    LogHelper.WriteInfo(this.GetType(), "UU配送:推送订单接口返回值异常（推单）" + JsonConvert.SerializeObject(result));
                    return "UU配送:推送订单接口返回值异常（推单）";
                }
                order.order_code = result.ordercode;
                if (getTranSql)
                {
                    return base.ExecuteNonQuery($"update UUOrder set state={order.State},order_code='{order.order_code}' where id={order.Id}") > 0 ? "" : "UU配送:修改UU订单状态出错（推单）";
                }
                else
                {
                    tran.Add($"update UUOrder set state={order.State},order_code='{order.order_code}' where id={order.Id}");
                }

                return "";
            }

            return result.return_msg;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool CancelOrder(int aid, int storeId, int orderId, string reason = "")
        {
            bool success = false;
            UUOrder model = GetModelByOrerId(aid, storeId, orderId);
            if (model == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"UU配送：没有找到快跑者订单数据_{aid}_{storeId}_{orderId}(取消订单)");
                return success;
            }
            UUCustomerRelation customerRelation = UUCustomerRelationBLL.SingleModel.GetModelByAid(aid, storeId, 0);
            if (customerRelation == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"UU配送：无效UU配置{aid}_{storeId}_{orderId}(取消订单)");
                return success;
            }

            if (!(model.State == (int)UUOrderEnum.下单成功 || model.State == (int)UUOrderEnum.已到达 ||  model.State == (int)UUOrderEnum.跑男抢单))
            {
                LogHelper.WriteInfo(this.GetType(), $"UU配送：取件前才能取消_{aid}_{storeId}_{orderId}(取消订单)");
                return success;
            }

            UUBaseResult result = UUApi.CanecelOrder(model.openid,model.order_code, reason);
            if (result == null || result.return_code != "ok")
            {
                LogHelper.WriteInfo(this.GetType(), $"UU配送：接口返回异常（取消订单）_{model.order_code}_{customerRelation.Id}" + (result != null ? result.return_msg : ""));
            }
            else
            {
                model.State = (int)UUOrderEnum.订单取消中;
                success = base.Update(model, "State");
                if (!success)
                {
                    LogHelper.WriteInfo(this.GetType(), $"UU配送：更新系统订单状态失败（取消订单）_{model.Id}");
                }
            }
            return success;
        }
    }
}