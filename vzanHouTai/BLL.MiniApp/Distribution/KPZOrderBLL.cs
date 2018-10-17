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
    public class KPZOrderBLL : BaseMySql<KPZOrder>
    {
        #region 单例模式
        private static KPZOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private KPZOrderBLL()
        {

        }

        public static KPZOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new KPZOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public KPZOrder GetModelByOrderNo(string trade_no)
        {
            return base.GetModel($"trade_no='{trade_no}'");
        }

        /// <summary>
        /// aid+storeid+orderid获取快跑者唯一订单
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public KPZOrder GetModelByOrerId(int aid,int storeid,int orderid)
        {
            return GetModel($"aid={aid} and storeid={storeid} and orderid={orderid}");
        }

        /// <summary>
        /// 生成快跑者配送订单
        /// </summary>
        /// <param name="rid">权限表ID</param>
        /// <param name="userinfo">用户信息</param>
        /// <param name="orderid">订单ID</param>
        /// <param name="price">订单价格</param>
        /// <param name="receviename">收货人姓名</param>
        /// <param name="receviephone">收货人电话</param>
        /// <param name="address">收货人地址</param>
        /// <param name="lat">纬度</param>
        /// <param name="lnt">经度</param>
        /// <param name="ordertype">看枚举TmpType</param>
        /// <returns></returns>
        public string AddKPZOrder(DistributionApiModel model)
        {
            string msg = "";
            KPZStoreRelation storerelation = KPZStoreRelationBLL.SingleModel.GetModelBySidAndAid(model.aid, model.storeid);
            if (storerelation == null)
            {
                msg = "请先配置快跑者配送";
                return msg;
            }
            
            //再请求一次腾讯地图，获取准确的坐标
            AddressApi addressModel = AddressHelper.GetLngAndLatByAddress(model.address);
            if (addressModel != null && addressModel.result != null && addressModel.result.location != null)
            {
                model.lnt = addressModel.result.location.lng;
                model.lat = addressModel.result.location.lat;
            }

            TransactionModel tran = new TransactionModel();
            string orderno = CommonCore.GetOrderNumByUserId(model.userid.ToString());
            float buyprice = model.buyprice / 100.0f;
            float payfee = model.fee / 100.0f;
            int paystatus = 0;
            //店铺信息
            ShopInfo shopinfo = GetStoreAddressPoint(storerelation.AId,storerelation.StoreId, model.temptype);
            
            //生成订单实体类对象
            KPZOrder data = new KPZOrder(storerelation.Id,shopinfo.ShopName, storerelation.TelePhone,shopinfo.ShopAddress,shopinfo.ShopTag, model.ordercontent, model.remark,orderno, model.accepterName, model.accepterTelePhone, model.address, model.lnt + ","+ model.lat,buyprice, buyprice,paystatus,payfee, model.orderid, model.aid, model.temptype, model.storeid);
            tran.Add(base.BuildAddSql(data));
            
            if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            {
                msg = "添加快跑者配送订单出错";
            }
            return msg;
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="storeid"></param>
        /// <param name="aid"></param>
        /// <param name="lat"></param>
        /// <param name="lnt"></param>
        /// <param name="orderprice"></param>
        /// <returns></returns>
        public int GetKPZFee(int storeid,int aid,string address,string lat,string lnt,int orderprice,ref string msg)
        {
            int fee = 0;
            KPZResult<KPZFee> result = new KPZResult<KPZFee>();
            if (!string.IsNullOrEmpty(address))
            {
                //再请求一次腾讯地图，获取准确的坐标
                AddressApi addressModel = AddressHelper.GetLngAndLatByAddress(address);
                if (addressModel != null && addressModel.result != null && addressModel.result.location != null)
                {
                    lnt = addressModel.result.location.lng.ToString();
                    lat = addressModel.result.location.lat.ToString();
                }
                else
                {
                    msg = "快跑者配送：获取腾讯地址失败";
                    return 0;
                }
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                msg = "快跑者配送：权限模板不存在";
                return fee;
            }
            
            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if(xcxtemplate==null)
            {
                msg = "快跑者配送：小程序模板不存在";
                return fee;
            }
            
            KPZStoreRelation storerelation = KPZStoreRelationBLL.SingleModel.GetModelBySidAndAid(aid,storeid);
            if(storerelation==null)
            {
                msg = "快跑者配送：未设置快跑者配送配置";
                return fee;
            }
            
            int shopid = storerelation.Id;
            string sendtag = lnt + "," + lat;
            //店铺信息
            ShopInfo shopinfo = GetStoreAddressPoint(storerelation.AId,storerelation.StoreId, xcxtemplate.Type);
            string payfee = "0";
            
            if (shopinfo.ShopTag.Length<=0)
            {
                msg = "快跑者配送：店铺经纬不能为空";
                return fee;
            }

            if (sendtag.Length <= 1)
            {
                msg = "快跑者配送：取货经纬不能为空";
                return fee;
            }
            result = KPZApi.GetFee(shopid, sendtag, shopinfo.ShopTag, (orderprice * 0.01).ToString("0.00"), payfee, storerelation.TeamToken);
            
            if(result==null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "快跑者配送：返回结果为null，");
                return fee;
            }
            if(result.data==null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "快跑者配送：返回运费为null，");
                return fee;
            }
            if (result.code == 200)
            {
                fee = Convert.ToInt32(result.data.pay_fee);
                return fee;
            }
            else
            {
                msg = result.message;
            }

            if (result == null)
            {
                result = new KPZResult<KPZFee>();
                result.code = 204;
            }
            //msg = "获取运费出错";

            log4net.LogHelper.WriteInfo(this.GetType(), $"快跑者配送：请求参数，shopid【{shopid}】sendtag【{sendtag}】gettag【{shopinfo.ShopTag}】orderprice【{orderprice}】payfee【{payfee}】" + JsonConvert.SerializeObject(result));

            return fee;
        }

        /// <summary>
        /// 获取店铺经纬度，lat+','+lng
        /// </summary>
        /// <returns></returns>
        public ShopInfo GetStoreAddressPoint(int aid,int storeid,int temptype)
        {
            ShopInfo shopinfo = new ShopInfo();
            switch(temptype)
            {
                case (int)TmpType.小程序餐饮模板:
                    shopinfo =  FoodBLL.SingleModel.GetAddressPoint(aid);break;
                case (int)TmpType.智慧餐厅:
                    shopinfo = DishStoreBLL.SingleModel.GetAddressPoint(storeid);break;
            }
            
            return shopinfo;
        }

        /// <summary>
        /// 处理餐饮快跑者配送回调
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool KPZToFoodReturn(KPZOrder order)
        {
            

            bool isSuccess = false;
            string updatesql = "status";
            FoodGoodsOrder foodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel(order.OrderId);// and  OrderType ={(int)miniAppFoodOrderType.店内点餐 } ");
            if (foodGoodOrder == null)
            {
                LogHelper.WriteInfo(this.GetType(), "快跑者配送：找不到餐饮订单");
                return isSuccess;
            }

            string msg = string.Empty;
            switch (order.status)
            {
                case (int)KPZOrderEnum.已撤销:
                    isSuccess = base.Update(order, updatesql);
                    if (!isSuccess)
                    {
                        LogHelper.WriteInfo(this.GetType(), "快跑者配送：修改系统订单状态出错," + JsonConvert.SerializeObject(order));
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
                case (int)KPZOrderEnum.待发单:
                case (int)KPZOrderEnum.待抢单:
                case (int)KPZOrderEnum.待接单:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待接单;
                    break;
                case (int)KPZOrderEnum.取单中:
                    updatesql += ",courier,tel";
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待送餐;
                    #region 发送餐饮订单配送通知 模板消息
                    C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(foodGoodOrder.UserId) ?? new C_UserInfo();
                    object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(foodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知);
                    TemplateMsg_Miniapp.SendTemplateMessage(userinfo.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知, (int)TmpType.小程序餐饮模板, postData);
                    #endregion
                    break;
                case (int)KPZOrderEnum.送单中:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待确认送达;
                    break;
                case (int)KPZOrderEnum.已送达:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.已完成;
                    break;
            }
            isSuccess = base.Update(order, updatesql);
            if (!isSuccess)
            {
                LogHelper.WriteInfo(this.GetType(), "快跑者配送：修改系统订单状态出错，" + JsonConvert.SerializeObject(order));
                return isSuccess;
            }
            isSuccess = FoodGoodsOrderBLL.SingleModel.Update(foodGoodOrder, "State");

            return isSuccess;
        }

        /// <summary>
        /// 处理智慧餐厅快跑者配送回调
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool KPZToMutilFoodReturn(KPZOrder order)
        {
            bool isSuccess = false;
            string updatesql = "status";
            DishOrder dishOrder = DishOrderBLL.SingleModel.GetModel(order.OrderId);
            if (dishOrder == null)
            {
                LogHelper.WriteInfo(this.GetType(), "快跑者配送：找不到智慧餐厅订单");
                return isSuccess;
            }

            string msg = string.Empty;
            switch (order.status)
            {
                case (int)KPZOrderEnum.已撤销:
                    isSuccess = base.Update(order, updatesql);
                    if (!isSuccess)
                    {
                        LogHelper.WriteInfo(this.GetType(), "快跑者配送：修改快跑者订单状态出错，" + JsonConvert.SerializeObject(order));
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
                case (int)KPZOrderEnum.待发单:
                case (int)KPZOrderEnum.待抢单:
                case (int)KPZOrderEnum.待接单:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.待商家确认;
                    break;
                case (int)KPZOrderEnum.取单中:
                    updatesql += ",courier,tel";
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.待取货;
                    dishOrder.peisong_open = 1;
                    dishOrder.peisong_user_name = order.courier;
                    dishOrder.peisong_user_phone = order.tel;
                    #region 发送餐饮订单配送通知 模板消息
                    
                    #endregion
                    break;
                case (int)KPZOrderEnum.送单中:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.配送中;
                    break;
                case (int)KPZOrderEnum.已送达:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.已完成;
                    break;
            }
            isSuccess = base.Update(order, updatesql);
            if (!isSuccess)
            {
                LogHelper.WriteInfo(this.GetType(), "快跑者配送：修改智慧餐厅订单状态出错:" + JsonConvert.SerializeObject(order));
                return isSuccess;
            }
            isSuccess = DishOrderBLL.SingleModel.Update(dishOrder, "peisong_status,peisong_open,peisong_user_name,peisong_user_phone");

            return isSuccess;
        }

        /// <summary>
        /// 快跑者配送推单
        /// </summary>
        /// <param name="orderid">小程序订单表ID</param>
        /// <param name="rid"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public string GetKPZOrderUpdateSql(int orderid, int aid,int storeid,ref TransactionModel tran, bool gettransql = false)
        {
            KPZOrder order = GetModelByOrerId(aid,storeid,orderid);
            if (order == null)
            {
                return "快跑者配送：订单不存在";
            }

            KPZStoreRelation kpzstore = KPZStoreRelationBLL.SingleModel.GetModel(order.shop_id);
            if(kpzstore==null)
            {
                return "快跑者配送：关联店铺不存在";
            }
            
            KPZResult<OrderTradeNo> result = KPZApi.CreateOrder(order, kpzstore.TeamToken);
            if (result == null)
            {
                return "快跑者配送：新增订单接口异常";
            }
            if (result.code == 200)
            {
                order.status = (int)KPZOrderEnum.待发单;
                if(result.data == null || string.IsNullOrEmpty(result.data.trade_no))
                {
                    LogHelper.WriteInfo(this.GetType(), "快跑者新增订单接口返回值异常" + JsonConvert.SerializeObject(result));
                    return "快跑者新增订单接口返回值异常";
                }
                order.trade_no = result.data.trade_no;
                if (gettransql)
                {
                    return base.ExecuteNonQuery($"update KPZOrder set status={order.status},trade_no='{order.trade_no}' where id={order.id}") > 0 ? "" : "修改快跑者订单状态出错";
                }
                else
                {
                    tran.Add($"update KPZOrder set status={order.status},trade_no='{order.trade_no}' where id={order.id}");
                }

                return "";
            }

            return result.message;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeid"></param>
        /// <param name="orderid"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool CancelOrder(int aid,int storeid,int orderid,string reason="")
        {
            bool success = false;
            KPZOrder model = GetModelByOrerId(aid, storeid, orderid);
            if (model == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"快跑者配送：取消订单失败，没有找到快跑者订单数据_{aid}_{storeid}_{orderid}");
                return success;
            }
            KPZStoreRelation storemodel = KPZStoreRelationBLL.SingleModel.GetModel(model.shop_id);
            if (storemodel == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"快跑者配送：取消订单失败，快跑者配送没配置_{aid}_{storeid}_{orderid}");
                return success;
            }
            if (model.status==(int)KPZOrderEnum.已送达)
            {
                LogHelper.WriteInfo(this.GetType(), $"快跑者配送：取消订单失败，该订单已送达_{aid}_{storeid}_{orderid}");
                return success;
            }

            KPZResult<object> result = KPZApi.CancelOrder(model.trade_no, reason, storemodel.TeamToken);
            if (result == null || result.code != 200)
            {
                LogHelper.WriteInfo(this.GetType(), $"快跑者配送：取消订单失败，接口返回异常_{model.trade_no}_{storemodel.TeamToken}"+(result!=null?result.message:""));
            }
            else
            {
                model.status = (int)KPZOrderEnum.已撤销;
                success = base.Update(model, "status");
                if(!success)
                {
                    LogHelper.WriteInfo(this.GetType(), $"快跑者配送：更新系统订单状态失败_{model.id}");
                }
            }
            return success;
        }
    }
}