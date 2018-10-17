using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Helper;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Fds
{
    /// <summary>
    /// 预约功能BLL
    /// </summary>
    public class FoodReservationBLL : BaseMySql<FoodReservation>
    {
        #region 单例模式
        private static FoodReservationBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodReservationBLL()
        {

        }

        public static FoodReservationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodReservationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 预约下单
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dinningTime">预约时间</param>
        /// <param name="seats">用餐人数</param>
        /// <param name="note">预约备注</param>
        /// <param name="userName">预约人名</param>
        /// <param name="contact">联系方式</param>
        /// <returns>新增订单ID</returns>
        public int AddReserve(int foodId = 0, int appid = 0, int userId = 0, int type = 0, DateTime? dinningTime = null, int seats = 0, string note = null, string userName = null, string contact = null)
        {
            FoodReservation preReserve = new FoodReservation
            {
                CreateDate = DateTime.Now,
                DinnerTime = dinningTime.Value,
                UserName = userName,
                Seats = seats,
                Contact = contact,
                Note = note,
                State = (int)miniAppFoodOrderState.待付款,
                FoodId = foodId,
                AId = appid,
                UserId = userId,
                Type = type
            };

            var reserveId = 0;
            int.TryParse(Add(preReserve).ToString(), out reserveId);
            return reserveId;
        }

        /// <summary>
        /// 预约编辑
        /// </summary>
        /// <param name="reserveId">预约ID</param>
        /// <param name="dinningTime">预约时间</param>
        /// <param name="seats">用餐人数</param>
        /// <param name="note">预约备注</param>
        /// <param name="userName">预约人名</param>
        /// <param name="contact">联系方式</param>
        /// <returns>执行结果</returns>
        public bool EditReserve(int reserveId, DateTime dinningTime, int seats, string note, string userName, string contact)
        {
            var editReserve = GetModel(reserveId);
            if (editReserve == null)
            {
                return false;
            }

            editReserve.DinnerTime = dinningTime;
            editReserve.Seats = seats;
            editReserve.Note = note;
            editReserve.UserName = userName;
            editReserve.Contact = contact;

            return Update(editReserve, "DinnerTime,Seats,Note,UserName,Contact");
        }

        /// <summary>
        /// 更新预约状态
        /// </summary>
        /// <param name="reserveId">预约ID</param>
        /// <param name="state">目标预约状态</param>
        /// <param name="tableID">桌号ID</param>
        /// <returns></returns>
        public bool UpdateState(FoodReservation reservation, int state, int tableID = 0)
        {
            switch (reservation.Type)
            {
                case (int)miniAppReserveType.到店扫码:
                case (int)miniAppReserveType.预约支付:
                    return UpdateStateForFood(reservation, state, tableID);
                case (int)miniAppReserveType.预约购物_专业版:
                    return UpdateStateForEnterprise(reservation, state);
            }
            return false;
        }

        /// <summary>
        /// 餐饮版预约状态更新（流程）
        /// </summary>
        /// <param name="reserveId">预约ID</param>
        /// <param name="state">预约状态</param>
        /// <param name="tableID">分配桌号ID</param>
        /// <returns></returns>
        public bool UpdateStateForFood(FoodReservation reservation, int state, int tableID = 0)
        {
            if (!Enum.IsDefined(typeof(miniAppFoodOrderState), state)) { return false; }

            var result = false;
            switch (state)
            {
                case (int)miniAppFoodOrderState.待就餐:
                    result = ConfirmReserve(reservation);
                    break;
                case (int)miniAppFoodOrderState.已完成:
                    reservation.TableId = tableID;
                    result = ExpenReserve(reservation);
                    break;
                case (int)miniAppFoodOrderState.已取消:
                    if (reservation.Type == (int)miniAppReserveType.到店扫码 && (reservation.State == (int)miniAppFoodOrderState.待付款 || reservation.State == (int)miniAppFoodOrderState.待就餐))
                    {
                        result = CancelReserve(reservation);
                    }
                    break;
                case (int)miniAppFoodOrderState.已退款:
                    if (reservation.Type == (int)miniAppReserveType.预约支付 && reservation.State > (int)miniAppFoodOrderState.待付款)
                    {
                        result = RefundReserveFood(reservation);
                    }
                    break;
            }
            return result;
        }

        public bool UpdateStateForEnterprise(FoodReservation reservation, int state)
        {
            if (!Enum.IsDefined(typeof(MiniAppEntOrderState), state)) { return false; }

            var result = false;

            switch (state)
            {
                case (int)MiniAppEntOrderState.待自取:
                    result = ConfirmReserveEnt(reservation);
                    break;
                case (int)MiniAppEntOrderState.已完成:
                    result = CompleteReserveEnt(reservation);
                    break;
                case (int)MiniAppEntOrderState.退款审核中:
                    result = reservation.State == (int)MiniAppEntOrderState.待付款 ? CancelReserveEnt(reservation) : RequestRefund(reservation);
                    break;
                case (int)MiniAppEntOrderState.退款成功:
                    result = RefundReserveEnt(reservation);
                    break;
                case (int)MiniAppEntOrderState.已取消:
                    result = CancelReserveEnt(reservation);
                    break;
            }
            return result;
        }

        /// <summary>
        /// 预定接受
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public bool ConfirmReserve(FoodReservation reservation)
        {
            if (!CanbeAccpet(reservation))
            {
                return false;
            }
            //事务
            TransactionModel tranModel = new TransactionModel();
            //更新菜单内商品的库存
            if (reservation.Type == (int)miniAppReserveType.预约支付)
            {
                UpdateStockByTransaction(tranModel: ref tranModel, reservation: reservation);
            }
            //更新状态（接单成功 => 待就餐）
            reservation.State = (int)miniAppFoodOrderState.待就餐;
            tranModel.Add(BuildUpdateSql(reservation, "State"));
            //事务执行成功
            bool result = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            if (!result)
            {
                return false;
            }
            //发送小程序模板消息
            if (!SendReserveFoodMsg(reservation, SendTemplateMessageTypeEnum.预约点餐商家接单通知))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"餐饮版预约，模板消息发送失败：预约ID-{reservation.Id}");
            }
            return true;
        }

        /// <summary>
        /// 预约购物接单
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public bool ConfirmReserveEnt(FoodReservation reservation)
        {
            if (reservation.State != (int)MiniAppEntOrderState.待接单)
            {
                return false;
            }
            reservation.State = (int)MiniAppEntOrderState.待自取;
            if (!Update(reservation, "state"))
            {
                return false;
            }
            //发送小程序模板消息
            if (!SendReserveEntMsg(reservation, SendTemplateMessageTypeEnum.预约购物接单通知))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"专业版版预约，模板消息发送失败：预约ID-{reservation.Id}");
            }
            return true;
        }

        /// <summary>
        /// 预定消费
        /// </summary>
        /// <param name="reservation"></param>
        /// <param name="tableId">菜桌号</param>
        /// <returns></returns>
        public bool ExpenReserve(FoodReservation reservation)
        {
            if (reservation?.State != (int)miniAppFoodOrderState.待就餐)
            {
                return false;
            }
            if (reservation.Type == (int)miniAppReserveType.预约支付 && reservation.TableId == 0)
            {
                return false;
            }
            var result = false;
            reservation.State = (int)miniAppFoodOrderState.已完成;
            result = Update(reservation, "TableId,State");
            if (result && reservation.Type == (int)miniAppReserveType.到店扫码)
            {
                return SendReserveFoodMsg(reservation, SendTemplateMessageTypeEnum.预约点餐扫码就座通知);
            }
            else if (result && reservation.Type == (int)miniAppReserveType.预约支付)
            {
                return SendReserveFoodMsg(reservation, SendTemplateMessageTypeEnum.预约点餐就座通知);
            }
            return result;
        }

        /// <summary>
        /// 预约购物完成
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public bool CompleteReserveEnt(FoodReservation reservation)
        {
            if (reservation?.State != (int)MiniAppEntOrderState.待自取)
            {
                return false;
            }
            reservation.State = (int)MiniAppEntOrderState.已完成;
            if (!Update(reservation, "TableId,State"))
            {
                return false;
            }
            if (!SendReserveEntMsg(reservation, SendTemplateMessageTypeEnum.预约购物自取通知))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"专业版版预约，模板消息(完成)发送失败：预约ID-{reservation.Id}");
            }
            return true;
        }

        /// <summary>
        /// 预约取消
        /// </summary>
        /// <param name="reserveId">预约ID</param>
        /// <returns>执行结果</returns>
        public bool CancelReserve(FoodReservation reservation)
        {
            reservation.State = (int)miniAppFoodOrderState.已取消;
            return Update(reservation, "State") && SendReserveFoodMsg(reservation, SendTemplateMessageTypeEnum.预约点餐取消通知);
        }

        /// <summary>
        /// 预约取消（专业版）
        /// </summary>
        /// <param name="reserveId">预约ID</param>
        /// <returns>执行结果</returns>
        public bool CancelReserveEnt(FoodReservation reservation)
        {
            reservation.State = (int)MiniAppEntOrderState.已取消;
            bool result = Update(reservation, "State");
            if (!result)
            {
                return false;
            }
            if (!SendReserveEntMsg(reservation, SendTemplateMessageTypeEnum.预约购物取消通知))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"专业版版预约，模板消息(取消)发送失败：预约ID-{reservation.Id}");
            }
            return result;
        }

        /// <summary>
        /// 预定退款
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public bool RefundReserveFood(FoodReservation reservation)
        {
            
            var orders = FoodGoodsOrderBLL.SingleModel.GetOrderByReservation(reservation.Id);
            var result = true;
            orders.FindAll(thisOrder => thisOrder.State == (int)miniAppFoodOrderState.待接单 || thisOrder.State == (int)miniAppFoodOrderState.待送餐).ForEach((thisOrder) =>
            {
                if (thisOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                {
                    var userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(thisOrder.UserId) ?? new SaveMoneySetUser();
                    result = result && FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(thisOrder, userSaveMoney, thisOrder.State);
                }
                else if (thisOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                {
                    result = result && FoodGoodsOrderBLL.SingleModel.outOrder(thisOrder, thisOrder.State);
                }
                if (result)
                {
                    SendReserveFoodMsg(reservation, SendTemplateMessageTypeEnum.预约点餐退款通知, refundOrder: thisOrder);
                }
            });

            if (result)
            {
                reservation.State = (int)miniAppFoodOrderState.已退款;
                result = Update(reservation, "State");
            }

            return result;
        }

        public bool RefundReserveEnt(FoodReservation reservation)
        {
            List<EntGoodsOrder> orders = EntGoodsOrderBLL.SingleModel.GetReserveOrder(reservation.Id);

            bool result = false;
            orders.FindAll(thisOrder => thisOrder.State == (int)MiniAppEntOrderState.待自取).ForEach((thisOrder) =>
            {
                result = EntGoodsOrderBLL.SingleModel.outOrder(thisOrder, thisOrder.State, thisOrder.BuyMode);
                if (result)
                {
                    SendReserveEntMsg(reservation, SendTemplateMessageTypeEnum.预约购物退款通知, refundEntOrder: thisOrder);
                }
            });

            if (result)
            {
                //微信支付需要服务处理
                bool isNeddProess = orders.Count(thisOrder => thisOrder.BuyMode == (int)miniAppBuyMode.微信支付) > 0;
                reservation.State = isNeddProess ? (int)MiniAppEntOrderState.退款中 : (int)MiniAppEntOrderState.退款成功;
                result = Update(reservation, "State");
            }
            return result;
        }

        public bool RequestRefund(FoodReservation reservation)
        {
            if (reservation == null || (reservation.State != (int)MiniAppEntOrderState.待接单 && reservation.State != (int)MiniAppEntOrderState.待自取))
            {
                return false;
            }

            reservation.State = (int)MiniAppEntOrderState.退款审核中;
            bool result = Update(reservation, "State");
            if (!result)
            {
                return false;
            }

            //发送模板消息
            List<EntGoodsOrder> reservationOrder = EntGoodsOrderBLL.SingleModel.GetReserveOrder(reservation.Id);
            TemplateMsg_Gzh.OutOrderTemplateMessageForEnt(reservationOrder?.FirstOrDefault());
            return result;
        }

        /// <summary>
        /// 更新菜单内商品的库存
        /// </summary>
        public void UpdateStockByTransaction(ref TransactionModel tranModel, FoodReservation reservation)
        {
            //获取预约菜单
            var foodOrders = FoodGoodsOrderBLL.SingleModel.GetOrderByReservation(reservation.Id);
            if (foodOrders != null && foodOrders.Count > 0)
            {
                //更新库存
                var orderIds = string.Join(",", foodOrders.Select(order => order.Id));
                var goodsCart = FoodGoodsCartBLL.SingleModel.GetReserveFromCart(reservation.FoodId, reservation.UserId, orderIds, state: 1);
                //插入事务
                FoodGoodsOrderBLL.SingleModel.GetStockUpdateTranSql(TranModel: ref tranModel, goodsCar: goodsCart);
            }
            //获取预约商品
            //var entOrders = EntGoodsOrderBLL.SingleModel.GetReserveOrder(reservation.Id);
            //if(entOrders != null  && entOrders.Count > 0)
            //{
            //    var orderIds = string.Join(",", entOrders.Select(order => order.Id));
            //    var goodsCartItem = new EntGoodsCartBLL().GetListByOrderIds(orderIds);
            //    new EntGoodsBLL().
            //}
        }

        /// <summary>
        /// 更新为已支付
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public string UpdateToPay(FoodReservation reservation, FoodGoodsOrder order = null, List<FoodGoodsCart> cartItem = null)
        {
            if (reservation == null || reservation.State != (int)miniAppFoodOrderState.待付款)
            {
                return string.Empty;
            }
            reservation.State = (int)miniAppFoodOrderState.待接单;
            reservation.Type = (int)miniAppReserveType.预约支付;
            //打印订单
            PrintOrder(reservation, order, cartItem);
            return BuildUpdateSql(reservation, "State,Type");
        }

        public bool PrintOrder(FoodReservation reservation, FoodGoodsOrder foodOrder = null, List<FoodGoodsCart> cartItem = null, Account oper = null)
        {
            
            

            Food store = FoodBLL.SingleModel.GetModel(reservation.FoodId);
            if (!store.funJoinModel.reservationPrint)
            {
                return true;
            }
            List<FoodGoodsOrder> orders = FoodGoodsOrderBLL.SingleModel.GetOrderByReservation(reservation.Id);
            if (orders == null || orders.Count == 0)
            {
                return false;
            }

            bool printResult = false;
            //打印机列表
            List<FoodPrints> foodPrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {store.Id} and appId = {store.appId} and state >= 0 ") ?? new List<FoodPrints>();
            if (foodOrder != null && cartItem != null && cartItem.Count > 0)
            {
                printResult = FoodGoodsOrderBLL.SingleModel.PrintOrder(store, foodOrder, cartItem, foodPrintList, oper);
            }
            //else
            //{
            //    foreach (FoodGoodsOrder order in orders)
            //    {
            //        List<FoodGoodsCart> carlist = cartBLL.GetList($" GoodsOrderId={order.Id} and state=1");
            //        printResult = orderBLL.PrintOrder(store, order, carlist, foodPrintList, oper);
            //    }
            //}
            return printResult;
        }

        /// <summary>
        /// 更新为已支付
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public string UpdateToPayEnt(FoodReservation reservation)
        {
            if (reservation == null || reservation.State != (int)MiniAppEntOrderState.待付款)
            {
                return string.Empty;
            }
            reservation.State = (int)MiniAppEntOrderState.待接单;
            return BuildUpdateSql(reservation, "State");
        }

        /// <summary>
        /// 发送预约（餐饮版）模板消息
        /// </summary>
        /// <param name="reservation"></param>
        /// <param name="msgState">模板消息状态（接单、到店、退款、取消）</param>
        /// <param name="refundOrder">退款订单（仅退款消息需要）</param>
        /// <returns></returns>
        public bool SendReserveFoodMsg(FoodReservation reservation, SendTemplateMessageTypeEnum msgState, FoodGoodsOrder refundOrder = null)
        {
            return SendReserveMsg(reservation: reservation, msgState: msgState, tmpType: TmpType.小程序餐饮模板, refundOrder: refundOrder);
        }

        /// <summary>
        /// 发送预约（专业版）模板消息
        /// </summary>
        /// <param name="reservation"></param>
        /// <param name="msgState">模板消息状态（接单、退款、取消）</param>
        /// <param name="refundOrder">退款订单（仅退款消息需要）</param>
        /// <returns></returns>
        public bool SendReserveEntMsg(FoodReservation reservation, SendTemplateMessageTypeEnum msgState, EntGoodsOrder refundEntOrder = null)
        {
            return SendReserveMsg(reservation: reservation, msgState: msgState, tmpType: TmpType.小程序专业模板, refundEntOrder: refundEntOrder);
        }

        /// <summary>
        /// 发送预约模板消息
        /// </summary>
        /// <param name="reservation"></param>
        /// <param name="msgState">模板消息状态（接单、退款、取消）</param>
        /// <param name="tmpType">模板消息类型（专业版、餐饮版）</param>
        /// <param name="refundOrder">退款订单（仅退款消息需要）</param>
        /// <param name="refundEntOrder">退款订单（仅退款消息需要）</param>
        /// <returns></returns>
        public bool SendReserveMsg(FoodReservation reservation, SendTemplateMessageTypeEnum msgState, TmpType tmpType, EntGoodsOrder refundEntOrder = null, FoodGoodsOrder refundOrder = null)
        {
            //发送小程序模板消息给用户
            object orderData = TemplateMsg_Miniapp.GetReservationTempMsgData(reservation, msgState, refundOrder: refundOrder, refundEntOrder: refundEntOrder);
            if (orderData == null)
            {
                return false;
            }
            TemplateMsg_Miniapp.SendTemplateMessage(reservation.UserId, msgState, (int)tmpType, orderData);
            return true;
        }

        /// <summary>
        /// 获取进行中（未结束）的预约
        /// </summary>
        /// <param name="foodId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FoodReservation GetUnfinishReservation(int foodId, int userId)
        {
            //未接单预约
            var unConfirmSql = $@"({BuildWhereSql(foodId: foodId, userId: userId, state: (int)miniAppFoodOrderState.待接单, type: (int)miniAppReserveType.到店扫码)}) or 
                                  ({BuildWhereSql(foodId: foodId, userId: userId, state: (int)miniAppFoodOrderState.待接单, type: (int)miniAppReserveType.预约支付)})";
            
            ////编辑中预约
            //var preReserveSql = BuildPreReserveSql(foodId: foodId, userId: userId);
            //未消费预约
            var unUseSql = $@"({BuildWhereSql(foodId: foodId, userId: userId, state: (int)miniAppFoodOrderState.待就餐, type: (int)miniAppReserveType.到店扫码)}) or 
                              ({BuildWhereSql(foodId: foodId, userId: userId, state: (int)miniAppFoodOrderState.待就餐, type: (int)miniAppReserveType.预约支付)})";
            
            //筛选
            var sql = $"({unConfirmSql}) or ({unUseSql})";
            return GetModel(sql);
        }

        /// <summary>
        /// 获取进行中（未结束）的预约购物（专业版）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FoodReservation GetOnGoingReservation(int appId, int userId)
        {
            //未接单预约
            var unConfirmSql = BuildWhereSql(appId: appId, userId: userId, state: (int)MiniAppEntOrderState.待接单, type: (int)miniAppReserveType.预约购物_专业版);
            //退款中预约
            var refundingSql = BuildWhereSql(appId: appId, userId: userId, state: (int)MiniAppEntOrderState.退款审核中, type: (int)miniAppReserveType.预约购物_专业版);
            //未付款预约
            var unPaySql = BuildWhereSql(appId: appId, userId: userId, state: (int)MiniAppEntOrderState.待付款, type: (int)miniAppReserveType.预约购物_专业版);
            //未消费预约
            var unCompleteql = BuildWhereSql(appId: appId, userId: userId, state: (int)MiniAppEntOrderState.待自取, type: (int)miniAppReserveType.预约购物_专业版);
            //筛选
            var sql = $"({unConfirmSql}) or ({refundingSql}) or ({unPaySql}) or ({unCompleteql})";
            try
            {
                return GetModel(sql);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return null;
            }
        }

        /// <summary>
        /// 获取未支付成功的预约
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FoodReservation GetUnPayReservationEnt(int appId, int userId)
        {
            //未支付预约
            var unPaySql = BuildWhereSql(appId: appId, userId: userId, state: (int)MiniAppEntOrderState.待付款, type: (int)miniAppReserveType.预约购物_专业版);
            return GetModel(unPaySql);
        }

        /// <summary>
        /// 获取到店扫码支付菜单
        /// </summary>
        /// <param name="foodId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FoodReservation GetScanReserveByUser(int foodId, int userId)
        {
            var accept = BuildWhereSql(foodId: foodId, userId: userId, type: (int)miniAppReserveType.到店扫码, state: (int)miniAppFoodOrderState.待就餐);
            
            var waitForAccept = BuildWhereSql(foodId: foodId, userId: userId, type: (int)miniAppReserveType.到店扫码, state: (int)miniAppFoodOrderState.待付款);
            
            return GetModel($"({accept}) or ({waitForAccept})");
        }

        /// <summary>
        /// 预约列表筛选
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="foodId">店铺ID</param>
        /// <param name="userName">预约人名</param>
        /// <param name="contact">联系方式</param>
        /// <param name="state">预约状态</param>
        /// <param name="start">查询开始时间</param>
        /// <param name="end">查询结束时间</param>
        /// <returns></returns>
        public List<FoodReservation> GetByList(int pageIndex, int pageSize, int state, string userName = null, int? type = null, int appId = 0, int foodId = 0, string contact = null, DateTime? start = null, DateTime? end = null, DateTime? dinnerStart = null, DateTime? dinnerEnd = null)
        {
            var whereSql = BuildWhereSql(foodId: foodId, appId: appId, userName: userName, contact: contact, type: type, state: state, start: start, end: end, dinnerStart: dinnerStart, dinnerEnd: dinnerEnd);
            
            return GetList(whereSql, pageSize, pageIndex, "*", "ID DESC");
        }

        /// <summary>
        /// 预约列表数据量
        /// </summary>
        /// <param name="foodId">店铺ID</param>
        /// <param name="userName">预约人名</param>
        /// <param name="contact">联系方式</param>
        /// <param name="state">预约状态</param>
        /// <param name="start">查询开始时间</param>
        /// <param name="end">查询结束时间</param>
        /// <param name="dinnerStart">就餐查询开始时间</param>
        /// <param name="dinnerEnd">就餐查询结束时间</param>
        /// <returns></returns>
        public int GetListCount(int state, string userName, int? type = null, int foodId = 0, int appId = 0, string contact = null, DateTime? start = null, DateTime? end = null, DateTime? dinnerStart = null, DateTime? dinnerEnd = null)
        {
            var whereSql = BuildWhereSql(foodId: foodId, type: type, appId: appId, userName: userName, contact: contact, state: state, start: start, end: end, dinnerStart: dinnerStart, dinnerEnd: dinnerEnd);
            return GetCount(whereSql);
        }

        /// <summary>
        /// 获取商品格式
        /// </summary>
        /// <returns></returns>
        public string GetMenuString(int reserveId, int foodId, int userId)
        {
            var shopCartItems = GetMenuItem(reserveId: reserveId, foodId: foodId, userId: userId);
            return FormatMenuData(shopCartItems: shopCartItems);
        }

        /// <summary>
        /// 获取商品格式（专业版）
        /// </summary>
        /// <returns></returns>
        public string GetEntMenuString(int reserveId, int foodId, int userId)
        {
            var shopCartItems = GetEntItem(reserveId: reserveId, userId: userId);
            return FormatEntMenuData(shopCartItems: shopCartItems);
        }

        /// <summary>
        /// 获取预约点餐商品
        /// </summary>
        /// <param name="reserveId"></param>
        /// <param name="foodId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<FoodGoodsCart> GetMenuItem(int reserveId, int foodId, int userId)
        {
            
            var orders = FoodGoodsOrderBLL.SingleModel.GetOrderByReservation(reserveId);
            if (orders?.Count <= 0)
            {
                return new List<FoodGoodsCart>();
            }
            var orderIds = string.Join(",", orders.Select(order => order.Id));
            var shopCartItems = FoodGoodsCartBLL.SingleModel.GetReserveFromCart(foodId: foodId, userId: userId, orderIds: orderIds, state: 1);
            return shopCartItems;
        }

        /// <summary>
        /// 获取预约商品
        /// </summary>
        /// <param name="reserveId"></param>
        /// <param name="foodId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<EntGoodsCart> GetEntItem(int reserveId, int userId)
        {
            var orders = EntGoodsOrderBLL.SingleModel.GetReserveOrder(reserveId);
            if (orders == null || orders.Count == 0)
            {
                return new List<EntGoodsCart>();
            }
            var orderIds = string.Join(",", orders.Select(order => order.Id));
            var shopCartItems = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);
            return shopCartItems;
        }

        /// <summary>
        /// 构建WhereSQL语句
        /// </summary>
        /// <param name="foodId">店铺ID</param>
        /// <param name="userName">预约人名</param>
        /// <param name="contact">联系方式</param>
        /// <param name="state">预约状态</param>
        /// <param name="start">查询开始时间</param>
        /// <param name="end">查询结束时间</param>
        /// <returns></returns>
        public string BuildWhereSql(int foodId = 0, int userId = 0, int appId = 0, string userName = null, string contact = null, int? state = null, int? type = null, DateTime? start = null, DateTime? end = null, DateTime? dinnerStart = null, DateTime? dinnerEnd = null)
        {
            var whereSql = string.Empty;
            //店铺ID
            if (foodId > 0)
            {
                whereSql = $"FoodId = {foodId}";
            }
            //用户ID
            if (userId > 0)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"UserId = {userId}" : $"{whereSql} AND UserId = {userId}";
            }
            //小程序模板ID
            if (appId > 0)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"AId = {appId}" : $"{whereSql} AND AId = {appId}";
            }
            //预约姓名
            if (!string.IsNullOrWhiteSpace(userName))
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"UserName LIKE'%{userName}%'" : $"{whereSql} AND UserName LIKE'%{userName}%'";
            }
            //联系方式
            if (!string.IsNullOrWhiteSpace(contact))
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"Contact LIKE'%{contact}%'" : $"{whereSql} AND Contact LIKE'%{contact}%'";
            }
            //状态筛选
            string stateSql = state.HasValue ? BuildWhereStateSql(state.Value, type) : null;
            if (!string.IsNullOrWhiteSpace(stateSql))
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? stateSql : $"{whereSql} AND {stateSql}";
            }
            //下单时间筛选
            if (start.HasValue && end.HasValue)
            {
                var dateSql = string.Empty;
                if (start.Value == end.Value)
                {
                    dateSql = $"to_days(CreateDate) = to_days('{start.Value.ToShortDateString()}')";
                }
                else
                {
                    dateSql = $"to_days(CreateDate) >= to_days('{start.Value.ToShortDateString()}') And to_days(CreateDate) <= to_days('{end.Value.ToShortDateString()}')";
                }
                whereSql = string.IsNullOrWhiteSpace(whereSql) ?
                    dateSql :
                    $"{whereSql} AND {dateSql}";
            }
            //就餐时间筛选
            if (dinnerStart.HasValue && dinnerEnd.HasValue)
            {
                var dateSql = string.Empty;
                if (dinnerStart.Value == dinnerEnd.Value)
                {
                    dateSql = $"to_days(DinnerTime) = to_days('{dinnerStart.Value.ToShortDateString()}')";
                }
                else
                {
                    dateSql = $"to_days(DinnerTime) >= to_days('{dinnerStart.Value.ToShortDateString()}') And to_days(DinnerTime) <= to_days('{dinnerEnd.Value.ToShortDateString()}')";
                }
                whereSql = string.IsNullOrWhiteSpace(whereSql) ?
                    dateSql :
                    $"{whereSql} AND {dateSql}";
            }
            //预约类型筛选
            if (type.HasValue && Enum.IsDefined(typeof(miniAppReserveType), type))
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ?
                    $"Type = {type}" :
                    $"{whereSql} AND Type = {type}";
            }
            return whereSql;
        }

        /// <summary>
        /// 构建状态筛选WhereSQL
        /// </summary>
        /// <param name="state"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string BuildWhereStateSql(int state, int? type)
        {
            var stateSql = string.Empty;
            if (Enum.IsDefined(typeof(MiniAppEntOrderState), state) && type.HasValue && type.Value == (int)miniAppReserveType.预约购物_专业版)
            {
                //专业版
                switch (state)
                {
                    case (int)MiniAppEntOrderState.待付款:
                    case (int)MiniAppEntOrderState.待接单:
                        stateSql = $"State in({(int)MiniAppEntOrderState.退款审核中},{(int)MiniAppEntOrderState.待付款},{(int)MiniAppEntOrderState.待接单})";
                        break;
                    case (int)MiniAppEntOrderState.待自取:
                    case (int)MiniAppEntOrderState.退款审核中:
                        stateSql = $"State ={state}";
                        break;
                    case (int)MiniAppEntOrderState.已完成:
                        stateSql = $"State in ({(int)MiniAppEntOrderState.已完成},{(int)MiniAppEntOrderState.退款成功},{(int)MiniAppEntOrderState.已取消},{(int)MiniAppEntOrderState.退款中})";
                        break;
                }
            }
            else if (Enum.IsDefined(typeof(miniAppFoodOrderState), state) && (type == null || type == (int)miniAppReserveType.到店扫码 || type == (int)miniAppReserveType.预约支付))
            {
                //餐饮版
                switch (state)
                {
                    case (int)miniAppFoodOrderState.待接单:
                        stateSql = $"((State = {(int)miniAppFoodOrderState.待付款} AND Type = {(int)miniAppReserveType.到店扫码}) or (State = {(int)miniAppFoodOrderState.待接单} AND Type = {(int)miniAppReserveType.预约支付}))";
                        break;
                    case (int)miniAppFoodOrderState.待就餐:
                        stateSql = $"(State = {(int)miniAppFoodOrderState.待就餐})";
                        break;
                    case (int)miniAppFoodOrderState.已完成:
                        stateSql = $"(State in ({(int)miniAppFoodOrderState.已完成},{(int)miniAppFoodOrderState.已退款},{(int)miniAppFoodOrderState.已取消}))";
                        break;
                    case (int)miniAppFoodOrderState.待付款:
                        stateSql = $"(State ={(int)miniAppFoodOrderState.待付款})";
                        break;
                }
            }
            return stateSql;
        }

        /// <summary>
        /// 商品对象转String格式:"商品(规格)X数量;"
        /// </summary>
        /// <param name="shopCartItems"></param>
        /// <returns></returns>
        public string FormatMenuData(List<FoodGoodsCart> shopCartItems)
        {
            if (shopCartItems?.Count <= 0) { return string.Empty; }
            string menuStr = string.Empty;
            foreach (var item in shopCartItems)
            {
                menuStr = string.IsNullOrWhiteSpace(item.SpecInfo) ?
                          $"{menuStr}{item.GoodName}X{item.Count};" :
                          $"{menuStr}{item.GoodName}({item.SpecInfo})X{item.Count};";
            }
            return menuStr.TrimEnd(';');
        }

        public string FormatEntMenuData(List<EntGoodsCart> shopCartItems)
        {
            if (shopCartItems?.Count <= 0) { return string.Empty; }
            string menuStr = string.Empty;
            foreach (var item in shopCartItems)
            {
                menuStr = string.IsNullOrWhiteSpace(item.SpecInfo) ?
                          $"{menuStr}{item.GoodName}X{item.Count};" :
                          $"{menuStr}{item.GoodName}({item.SpecInfo})X{item.Count};";
            }
            return menuStr.TrimEnd(';');
        }

        public object ConvertReservationModel(FoodReservation reservation)
        {
            return new
            {
                UserName = reservation.UserName,
                Contact = reservation.Contact,
                Seats = reservation.Seats,
                DinnerTime = reservation.DinnerTime.ToString(),
                Note = reservation.Note,
                Type = reservation.Type,
                State = reservation.State
            };
        }

        public List<object> ConvertToAPIModel(List<FoodReservation> reservelist)
        {
            var result = new List<object>(reservelist.Count);
            
            
            reservelist.ForEach((reservation) =>
            {
                result.Add(new
                {
                    Id = reservation.Id,
                    Contact = reservation.Contact,
                    UserName = reservation.UserName,
                    OrderDate = reservation.CreateDate.ToString(),
                    DinnerTime = reservation.DinnerTime.ToString(),
                    Seats = reservation.Seats,
                    State = reservation.State,
                    Note = reservation.Note,
                    Type = reservation.Type,
                    Menu = GetMenuString(reservation.Id, reservation.FoodId, reservation.UserId),
                    TableNo = reservation.State == (int)miniAppFoodOrderState.已完成 ? FoodTableBLL.SingleModel.GetModel(reservation.TableId)?.Scene : null,
                    OrderId = FoodGoodsOrderBLL.SingleModel.GetOrderByReservation(reservation.Id).FirstOrDefault()?.OrderNum,
                });
            });
            return result;
        }

        public List<object> ConvertToEntApiModel(List<FoodReservation> reservelist)
        {
            var result = new List<object>(reservelist.Count);
            reservelist.ForEach((reservation) =>
            {
                result.Add(new
                {
                    Id = reservation.Id,
                    Contact = reservation.Contact,
                    UserName = reservation.UserName,
                    OrderDate = reservation.CreateDate.ToString(),
                    DinnerTime = reservation.DinnerTime.ToString(),
                    Seats = reservation.Seats,
                    State = reservation.State,
                    Note = reservation.Note,
                    Type = reservation.Type,
                    Menu = GetEntMenuString(reservation.Id, reservation.FoodId, reservation.UserId),
                });
            });
            return result;
        }

        public List<object> ConvertMenuItemToAPI(List<FoodGoodsCart> shopCartItems)
        {
            if (shopCartItems?.Count <= 0) { return null; }
            List<object> menuItems = new List<object>(shopCartItems.Count);
            foreach (var item in shopCartItems)
            {
                menuItems.Add(new
                {
                    Id = item.Id,
                    GoodId = item.FoodGoodsId,
                    Name = item.GoodName,
                    Img = item.GoodImg,
                    Count = item.Count,
                    Price = item.Price * 0.01,
                    Discount = item.discount * 0.01,
                    OriginalPrice = item.originalPrice * 0.01,
                    Spec = item.SpecInfo
                });
            }
            return menuItems;
        }

        public List<object> ConvertEntItemToAPI(List<EntGoodsCart> shopCartItems)
        {
            if (shopCartItems?.Count <= 0) { return null; }
            List<object> menuItems = new List<object>(shopCartItems.Count);
            foreach (var item in shopCartItems)
            {
                menuItems.Add(new
                {
                    Id = item.Id,
                    GoodId = item.FoodGoodsId,
                    Name = item.GoodName,
                    Count = item.Count,
                    Price = Math.Round(item.Price * 0.01, 3),
                    Discount = Math.Round(item.discount * 0.01, 3),
                    OriginalPrice = Math.Round(item.originalPrice * 0.01, 3),
                    Spec = item.SpecInfo
                });
            }
            return menuItems;
        }

        /// <summary>
        /// 接单校验
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public bool CanbeAccpet(FoodReservation reservation)
        {
            return (reservation.Type == (int)miniAppReserveType.到店扫码 && reservation.State == (int)miniAppFoodOrderState.待付款) ||
                   (reservation.Type == (int)miniAppReserveType.预约支付 && IsPayed(reservation));
        }

        /// <summary>
        /// 是否支付
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        public bool IsPayed(FoodReservation reservation)
        {
            return reservation?.State == (int)miniAppFoodOrderState.待接单;
        }
    }
}
