using BLL.MiniApp.Conf;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using static Entity.MiniApp.Pin.PinEnums;

namespace BLL.MiniApp.Pin
{
    public class PinGoodsOrderBLL : BaseMySql<PinGoodsOrder>
    {
        public void CreateOrder(PinGoodsOrder order, C_UserInfo userInfo, PinGoods goods, SpecificationDetailModel specificationDetail)
        {
            if (order == null || userInfo == null || goods == null)
            {
                order = new PinGoodsOrder();
                return;
            }
            int price = specificationDetail != null ? specificationDetail.price : goods.price;
            int groupPrice = specificationDetail != null ? specificationDetail.groupPrice : goods.groupPrice;
            order.price = price * order.count;
            order.money = order.price + order.freight;
            order.returnMoney = groupPrice * order.count;
            order.userId = userInfo.Id;
            order.receivingNo = GetCount($" aid={order.aid} and storeId={order.storeId}") + 100001;
            order.outTradeNo = $"{DateTime.Now.ToString("yyyyMMdd")}{order.receivingNo}";
            order.goodsPhoto = JsonConvert.SerializeObject(goods);
            if (specificationDetail != null)
            {
                order.specificationPhoto = JsonConvert.SerializeObject(specificationDetail);
            }
            order.id = Convert.ToInt32(Add(order));
            if (order.id > 0)
            {
                switch (order.payway)
                {
                    case (int)PayWay.微信支付:
                        order.payNo = CreateWxOrder(order, userInfo.NickName);
                        Update(order, "payno");
                        break;

                    case (int)PayWay.余额支付:
                        PayOrderByAccount(order);//未写
                        break;

                    case (int)PayWay.线下支付:
                        order.state = (int)PinOrderState.交易成功;
                        Update(order, "state");
                        break;
                }
            }

        }

        private void PayOrderByAccount(PinGoodsOrder order)
        {
            throw new NotImplementedException();
        }

        public int CreateWxOrder(PinGoodsOrder order, string userName)
        {
            int cityMorderId = 0; //citymorder表ID
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(order.aid);
            if (xcx == null)
            {
                return cityMorderId;
            }

            string no = WxPayApi.GenerateOutTradeNo();
            CityMorders citymorderModel = new CityMorders
            {
                OrderType = (int)ArticleTypeEnum.PinOrderPay,
                ActionType = (int)ArticleTypeEnum.PinOrderPay,
                Addtime = DateTime.Now,
                payment_free = order.money,
                trade_no = no,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                FuserId = order.userId,
                Fusername = userName,
                orderno = no,
                payment_status = 0,
                Status = 0,
                Articleid = 0,
                CommentId = order.id,//订单Id
                MinisnsId = order.aid,// 订单aId
                TuserId = order.storeId,
                ShowNote = $" {xcx.Title}购买商品支付{order.moneyStr}元",
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                buy_num = 0, //无
                appid = xcx.AppId,
            };
            cityMorderId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));

            return cityMorderId;
        }

        public bool havingNewOrder(int storeId, DateTime checkTime)
        {
            string checkSql = $" storeId = {storeId} and addtime >= '{checkTime.ToString("yyyy-MM-dd HH:mm:ss")}' ";

            return Exists(checkSql);
        }

        public List<PinGoodsOrder> GetListByCondition(int aid, int storeId, string orderId, string storeName, string goodsName, int orderState, string consignee, string phone, int sendWay, int groupState, int groupId, string startDate, string endDate, int pageIndex, int pageSize, out int recordCount)
        {
            List<PinGoodsOrder> orderList = new List<PinGoodsOrder>();
            recordCount = 0;
            string sqlwhere = $"aid={aid} and storeId={storeId}";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(orderId))
            {
                sqlwhere += $" and outTradeNo like @outTradeNo";
                parameters.Add(new MySqlParameter("@outTradeNo", $"%{orderId}%"));
            }
            if (!string.IsNullOrEmpty(goodsName))
            {
                List<MySqlParameter> Gparameters = new List<MySqlParameter>();
                Gparameters.Add(new MySqlParameter("@goodsName", $"%{goodsName}%"));
                List<PinGoods> goodsList = PinGoodsBLL.SingleModel.GetListByParam($"aid={aid}  and storeId={storeId} and name like @goodsName", Gparameters.ToArray());
                if (goodsList == null || goodsList.Count <= 0)
                {
                    return orderList;
                }
                string gids = string.Join(",", goodsList.Select(goods => goods.id).ToList());
                sqlwhere += $" and goodsId in ({gids})";
            }
            if (orderState != -999)
            {
                sqlwhere += $" and state={orderState}";
            }
            if (!string.IsNullOrEmpty(consignee))
            {
                sqlwhere += $" and consignee like @consignee";
                parameters.Add(new MySqlParameter("@consignee", $"%{consignee}%"));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                sqlwhere += $" and phone like @phone";
                parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
            }
            if (sendWay != -999)
            {
                sqlwhere += $" and sendway ={sendWay}";
            }
            if (groupState != -999)
            {
                List<PinGroup> groupList = PinGroupBLL.SingleModel.GetList($"aid={aid}  and storeId={storeId} and state={groupState}");
                if (groupList == null || groupList.Count <= 0)
                {
                    return orderList;
                }
                string groupIds = string.Join(",", groupList.Select(group => group.id).ToList());
                sqlwhere += $" and groupId in ({groupIds})";
            }
            if (groupId > 0)
            {
                sqlwhere += $" and groupId ={groupId}";
            }
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime start = Convert.ToDateTime($"{startDate} 00:00:00");
                DateTime end = Convert.ToDateTime($"{endDate} 23:59:59");
                sqlwhere += " and addtime between @start and @end";
                parameters.Add(new MySqlParameter("@start", start));
                parameters.Add(new MySqlParameter("@end", end));
            }
            orderList = GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex, "*", "id desc");
            recordCount = GetCount(sqlwhere, parameters.ToArray());
            return orderList;
        }
        /// <summary>
        /// 返利
        /// </summary>
        /// <param name="order"></param>
        public bool ReturnMoney(PinGoodsOrder order, TransactionModel tran, ref string msg)
        {
            if (order == null)
            {
                msg = "订单不存在 order null";
                return false;
            }

            bool result = RefundMoney(order, order.returnMoney, ref msg, tran);

            order.isReturnMoney = 1;
            order.remark = msg;
            MySqlParameter[] paramList = null;
            tran.Add(new PinGoodsOrderBLL().BuildUpdateSql(order, "isReturnMoney,remark", out paramList), paramList);

            return result;
            //log4net.LogHelper.WriteInfo(GetType(), msg);
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int Refund(PinGoodsOrder order, ref string msg)
        {
            int code = 0;
            if (order == null)
            {
                return code;
            }

            TransactionModel tran = new TransactionModel();
            MySqlParameter[] pone = null;
            order.payState = (int)PinEnums.PayState.已退款;
            order.remark += "(商家操作退款)";
            tran.Add(BuildUpdateSql(order, "paystate,remark", out pone), pone);
            if (!RefundMoney(order, order.money, ref msg, tran))
            {
                return code;
            }
            
            if (!PinGroupBLL.SingleModel.RollbackEntrantCount(order, ref msg, tran))
            {
                msg = "回滚问题";
                return code;
            }
            code = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
            if (code == 1)
            {
                //操作成功发送模板消息通知用户
                SendTemplateMsg_Refund(order);
            }
            return code;
        }
        /// <summary>
        /// 根据售后申请退款
        /// </summary>
        /// <param name="apply"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool RefundByApply(PinRefundApply apply, ref string msg)
        {
            bool result = false;
            PinGoodsOrder order = GetModel(apply.id);
            if (order == null || order.state == (int)PinOrderState.已删除)
            {
                msg = "订单不存在";
                return result;
            }
            if (order.state <= 0)
            {
                msg = "当前订单状态无法退款";
                return result;
            }
            int money = 0;//订单实际剩余金额
            if (order.isReturnMoney == 1)
            {
                money = order.money - order.refundMoney - order.returnMoney;
            }
            else
            {
                money = order.money - order.refundMoney;
            }
            if (money < apply.money)
            {
                msg = "当前订单支付剩余金额不足以退款";
                return result;
            }
            //交易完成的订单要扣会商家的收益
            if (order.state > (int)PinOrderState.交易成功)
            {
                
                PinStore pinStore = PinStoreBLL.SingleModel.GetModelByAid_Id(order.aid, order.storeId);
                if (pinStore == null)
                {
                    msg = "店铺信息错误";
                    return result;
                }
                if ((order.sourceType == 0 && pinStore.cash < apply.money) || (order.sourceType == 1 && pinStore.qrcodeCash < apply.money))
                {
                    msg = "店铺可提现金额不足以退款";
                    return result;
                }
                if (order.sourceType == 0)
                {
                    pinStore.cash -= apply.money;
                    result = PinStoreBLL.SingleModel.Update(pinStore, "cash");
                }
                else
                {
                    pinStore.qrcodeCash -= apply.money;
                    result = PinStoreBLL.SingleModel.Update(pinStore, "qrcodecash");
                }
                if (!result)
                {
                    msg = "操作失败，扣减商家收益异常";
                    return result;
                }

            }
            apply.state = (int)RefundApplyState.已退款;
            apply.updateTime = DateTime.Now;
            if (PinRefundApplyBLL.SingleModel.Update(apply, "state,updatetime"))
            {
                order.refundMoney += apply.money;
                result = RefundMoney(order, apply.money, ref msg) && Update(order, "refundmoney");
            }
            return result;
        }
        /// <summary>
        /// 微信退款
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool RefundMoney(PinGoodsOrder order, int money, ref string msg, TransactionModel tran = null)
        {
            bool istran = tran != null;
            if (!istran) tran = new TransactionModel();
            CityMorders cityMorder = new CityMordersBLL().GetModel(order.payNo);
            if (cityMorder == null)
            {

                msg = "退款失败,未找到退款的源微信订单";
                return false;
            }
            if (cityMorder.payment_status == 0) //微信订单未支付
            {
                msg = "退款失败,该订单未支付";
                return false;
            }

            ReFundQueue reModel = new ReFundQueue
            {
                minisnsId = -5,
                money = money,
                orderid = cityMorder.Id,
                traid = cityMorder.trade_no,
                addtime = DateTime.Now,
                note = "小程序拼享惠退款",
                retype = 1
            };
            tran.Add(new ReFundQueueBLL().BuildAddSql(reModel));
            if (!istran)
            {
                if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                {
                    msg = "加入退款队列失败";
                    return false;
                }
            }

            msg = "加入退款队列成功,将在1-7个工作日内响应退款";
            return true;
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public int CancelOrder(List<PinGoodsOrder> orders, string remark = "商家取消交易")
        {
            int code = 0;
            string orderIds = string.Join(",", orders.Select(order => order.id));
            string sql = $"update PinGoodsOrder set state={(int)PinEnums.PinOrderState.交易取消},remark='{remark}' where id in ({orderIds})";
            TransactionModel tran = new TransactionModel();
            
            tran.Add(sql);

            foreach (var order in orders)
            {
                order.state = (int)PinEnums.PinOrderState.交易取消;
                string childmsg = "";
                PinGroupBLL.SingleModel.RollbackEntrantCount(order, ref childmsg, tran);
            }
            code = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
            if (code == 1)//操作成功发送模板消息通知用户
            {
                foreach (var order in orders)
                {
                    SendTemplateMsg_PayCancel(order);
                }
            }
            return code;

        }

        /// <summary>
        /// 获取时间段内的订单数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetSaleCountByDate(int storeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            int total = 0;
            if (storeId <= 0)
            {
                return total;
            }
            string sql = $"select count(1) from pingoodsorder where storeId={storeId} and state not in ({(int)PinEnums.PinOrderState.交易失败},{(int)PinEnums.PinOrderState.交易取消})";
            List<MySqlParameter> paramers = new List<MySqlParameter>();
            if (startDate != null && endDate != null)
            {
                sql += $" and addtime between @startDate and @endDate";
                paramers.Add(new MySqlParameter("@startDate", startDate));
                paramers.Add(new MySqlParameter("@endDate", endDate));
            }
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, paramers.ToArray());
            total = result == DBNull.Value ? 0 : Convert.ToInt32(result);
            return total;
        }

        /// <summary>
        /// 获取时间段内的收入
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetEarningsByDate(int storeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            int total = 0;
            if (storeId <= 0)
            {
                return total;
            }
            string sql = $"select sum(money) from pingoodsorder where storeId={storeId}  and state not in ({(int)PinEnums.PinOrderState.交易失败},{(int)PinEnums.PinOrderState.交易取消}) and paystate ={(int)PayState.已付款}";
            List<MySqlParameter> paramers = new List<MySqlParameter>();
            if (startDate != null && endDate != null)
            {
                sql += $" and addtime between @startDate and @endDate";
                paramers.Add(new MySqlParameter("@startDate", startDate));
                paramers.Add(new MySqlParameter("@endDate", endDate));
            }
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, paramers.ToArray());
            total = result == DBNull.Value ? 0 : Convert.ToInt32(result);
            return total;
        }

        /// <summary>
        /// 完成交易
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public int OrderSuccess(List<PinGoodsOrder> orders, TransactionModel tran = null, string remark = "商家确认交易完成")
        {
            int code = 0;
            if (orders == null || orders.Count <= 0)
            {
                return code;
            }
            bool isTran = tran != null;
            if (!isTran)
            {
                tran = new TransactionModel();
            }
            string orderIds = string.Join(",", orders.Select(order => order.id));

            string sql = $"update PinGoodsOrder set state={(int)PinEnums.PinOrderState.交易成功},remark='商家确认交易完成',receivingtime='{DateTime.Now}' where id in ({orderIds})";
            tran.Add(sql);

            string groupIds = string.Join(",", orders.Select(order => order.groupId));
            
            List<PinGroup> groupList = PinGroupBLL.SingleModel.GetList($" id in ({groupIds})");
            if (groupList != null && groupList.Count > 0)
            {
                MySqlParameter[] pone = null;
                foreach (var item in groupList)
                {
                    item.successCount += orders.Where(order => order.groupId == item.id).Count();
                    item.state = item.successCount >= item.groupCount ? (int)PinEnums.GroupState.拼团成功 : item.state;
                    tran.Add(PinGroupBLL.SingleModel.BuildUpdateSql(item, "successCount,state", out pone), pone);
                }
            }
            //添加销量

            string goodsIds = string.Join(",",orders.Select(s=>s.goodsId).Distinct());
            List<PinGoods> pinGoodsList = PinGoodsBLL.SingleModel.GetListByIds(goodsIds);

            string storeIds = string.Join(",",orders.Select(s=>s.storeId).Distinct());
            List<PinStore> pinStoreList = PinStoreBLL.SingleModel.GetListByIds(storeIds);
            
            foreach (var order in orders)
            {
                PinGoods goods = pinGoodsList?.FirstOrDefault(f=>f.id == order.goodsId);
                if (goods == null) continue;
                goods.sales += order.count;
                MySqlParameter[] pone = null;
                tran.Add(PinGoodsBLL.SingleModel.BuildUpdateSql(goods, "sales", out pone), pone);
                PinStore store = pinStoreList?.FirstOrDefault(f=>f.id == order.storeId);
                if (store == null)
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception($"找不到店铺信息 orderid:{order.id}"));
                }
                else
                {
                    PinStoreBLL.SingleModel.AddIncome(order, store, tran);//商家增加收入
                }
            }
            if (isTran)
            {
                code = 1;
                return code;
            }
            code = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
            return code;
        }

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public int SendGoods(List<PinGoodsOrder> orders, string attachData = "")
        {
            int code = 0;

            string zqOrderIds = string.Join(",", orders.Where(order => order.sendway == (int)PinEnums.SendWay.到店自取).Select(order => order.id));
            string psOrderIds = string.Join(",", orders.Where(order => order.sendway == (int)PinEnums.SendWay.商家配送).Select(order => order.id));
            if (string.IsNullOrEmpty(zqOrderIds) && string.IsNullOrEmpty(psOrderIds))
            {
                return code;
            }
            DeliveryUpdatePost DeliveryInfo = null;
            //如果商家配送，验证物流信息输入是否完整
            if (!string.IsNullOrEmpty(psOrderIds) && !string.IsNullOrEmpty(attachData))
            {
                DeliveryInfo = JsonConvert.DeserializeObject<DeliveryUpdatePost>(attachData);
                bool isCompleteInfo = ((!string.IsNullOrWhiteSpace(DeliveryInfo.CompanyCode) && !string.IsNullOrWhiteSpace(DeliveryInfo.DeliveryNo)) || DeliveryInfo.SelfDelivery)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.ContactName)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.ContactTel)
                                              && !string.IsNullOrWhiteSpace(DeliveryInfo.Address);
                if (!isCompleteInfo)
                    return code;
            }

            TransactionModel tran = new TransactionModel();
            if (!string.IsNullOrEmpty(zqOrderIds))
            {
                tran.Add($"update PinGoodsOrder set state={(int)PinEnums.PinOrderState.待自取} where id in ({zqOrderIds})");
            }
            if (!string.IsNullOrEmpty(psOrderIds))
            {
                tran.Add($"update PinGoodsOrder set state={(int)PinEnums.PinOrderState.待收货},attachData=@attachData where id in ({psOrderIds})", new MySqlParameter[] {
                    new MySqlParameter("@attachData",attachData)
                });

            }
            code = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
            if (code == 1 && DeliveryInfo != null)
            {
                DeliveryFeedbackBLL.SingleModel.AddOrderFeed(orders.FirstOrDefault().id, DeliveryInfo, DeliveryOrderType.拼享惠订单商家发货);
            }
            //发送模板消息通知
            foreach (var order in orders)
            {
                SendTemplateMsg_Send(order);
            }
            return code;

        }

        /// <summary>
        /// 根据orderids,不满足条件state获取数据
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="orderIds"></param>
        /// <param name="states"></param>
        /// <returns></returns>
        public List<PinGoodsOrder> GetListByIds(int aid, int storeId, string orderIds, string states)
        {
            string sqlwhere = $"aid ={aid} and storeid={storeId} and id in ({orderIds}) and state not in ({states}) ";
            return GetList(sqlwhere);
        }

        public List<PinGoodsOrder> GetListByIds(string orderIds)
        {
            if (string.IsNullOrEmpty(orderIds))
                return new List<PinGoodsOrder>();

            orderIds = orderIds.LastIndexOf(',') == orderIds.Length - 1 ? orderIds.Substring(0, orderIds.Length - 1) : orderIds;

            string sqlwhere = $"id in ({orderIds})";
            return GetList(sqlwhere);
        }

        public bool UpdateState(PinGoodsOrder order, TransactionModel tran, MySqlParameter[] _pone = null)
        {
            //是否要将sql加入tran返回
            bool isAddTran = tran != null;
            if (!isAddTran) tran = new TransactionModel();

            order.paytime = DateTime.Now;
            switch (order.payway)
            {
                case (int)PayWay.微信支付:
                    order.payState = (int)PayState.已付款;
                    order.state = order.sendway == (int)SendWay.商家配送 ? (int)PinOrderState.待发货 : (int)PinOrderState.待自取;
                    tran.Add(BuildUpdateSql(order, "paystate,state,paytime", out _pone), _pone);
                    break;
                default:
                    return false;
            }
            if (order.sendway == (int)SendWay.面对面交易)
            {
                List<PinGoodsOrder> orderList = new List<PinGoodsOrder>() { order };
                OrderSuccess(orderList, tran);
            }
            if (isAddTran) return true;
            else return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public string CancelTimeoutOrder()
        {
            string sqlwhere = $"state>{(int)PinOrderState.交易取消} and paystate={(int)PayState.未付款} and addtime<'{DateTime.Now.AddMinutes(-30)}'";
            List<PinGoodsOrder> list = GetList(sqlwhere);
            if (list == null || list.Count <= 0) return "没有可执行数据";
            
            
            string msg = string.Empty;
            foreach (var order in list)
            {
                TransactionModel _tranModel = new TransactionModel();
                order.state = (int)PinOrderState.交易失败;
                order.remark = "订单超时：30分钟内未支付，交易失败";
                _tranModel.Add(BuildUpdateSql(order, "state,remark"));
                if (order.goodsId > 0)
                {
                    PinGoodsBLL.SingleModel.RollbackGoodsStock(order, _tranModel);
                }
                else
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception($"订单超时取消失败，goodsId为0，orderId：{order.id}"));
                    msg += $"订单超时取消失败，goodsId为0，orderId：{order.id}|";
                    continue;
                }
                if (order.groupId > 0)
                {
                    PinGroupBLL.SingleModel.RollbackEntrantCount(order, ref msg, _tranModel);
                }
                if (!base.ExecuteTransaction(_tranModel.sqlArray, _tranModel.ParameterArray))
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception($"订单超时取消失败,orderId：{order.id}"));
                    msg += $"订单超时取消失败,orderId：{order.id}| ";
                }
            }
            return msg;
        }

        public List<PinGoodsOrder> GetListByGroupId(int groupId)
        {
            List<PinGoodsOrder> orders = new List<PinGoodsOrder>();
            if (groupId <= 0)
            {
                return orders;
            }
            string sqlwhere = $" groupId={groupId} and state>=0 and paystate !={(int)PayState.已退款}";
            orders = GetList(sqlwhere);
            return orders;
        }

        public PinGoodsOrder GetModelByAid_StoreId_Id(int aid, int storeId, int orderId)
        {
            PinGoodsOrder order = null;
            if (aid <= 0 || storeId <= 0 || orderId <= 0)
            {
                return order;
            }
            string sqlwhere = $" aid={aid} and storeId={storeId} and id={orderId}";
            order = GetModel(sqlwhere);
            return order;
        }

        public List<PinGoodsOrder> GetListByCondition(int aid, int userId, int state, int groupState, int commentState, int pageIndex, int pageSize)
        {
            List<PinGoodsOrder> list = new List<PinGoodsOrder>();
            if (aid <= 0 || userId <= 0)
            {
                return list;
            }
            string sql = "select a.* from pingoodsorder as a";
            string sqlwhere = $" where a.aid={aid} and a.userId={userId}";
            if (state != -999)
            {
                sqlwhere += $" and a.state={state}";
            }
            else
            {
                sqlwhere += $" and a.state!=-3";
            }
            if (groupState != -999)
            {
                
                sql += $" left join pingroup as b on a.groupId=b.id ";
                sqlwhere += $" and b.state={groupState}";
            }
            if (commentState == 1)
            {
                sql += $" left join pincomment as c on a.id=c.oid and a.userid=c.uid";
                sqlwhere += $" and c.id is null";
            }
            sql = $"{sql}{sqlwhere}  order by a.id desc limit {(pageIndex - 1) * pageSize},{pageSize} ";

            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    list.Add(model);
                }
                return list;
            }
        }

        public List<PinGoodsOrder> AdminGetList(int aid, int storeId, int selType, string val, int state, int pageIndex, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<PinGoodsOrder> orderList = new List<PinGoodsOrder>();
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sqlwhere = $" aid={aid} and storeId={storeId}";
            if (state != -999)
            {
                sqlwhere += $" and state={state}";
            }
            if (!string.IsNullOrEmpty(val))
            {
                switch (selType)
                {
                    case 1://订单号
                        sqlwhere += $" and outTradeNo like @outTradeNo";
                        parameters.Add(new MySqlParameter("@outTradeNo", $"%{val}%"));
                        break;
                    case 2://提货号
                        sqlwhere += $" and receivingNo ={val}";
                        break;
                    case 3://商品名称
                        List<MySqlParameter> Gparameters = new List<MySqlParameter>();
                        Gparameters.Add(new MySqlParameter("@goodsName", $"%{val}%"));
                        List<PinGoods> goodsList = PinGoodsBLL.SingleModel.GetListByParam($"aid={aid}  and storeId={storeId} and name like @goodsName", Gparameters.ToArray());
                        if (goodsList == null || goodsList.Count <= 0)
                        {
                            return orderList;
                        }
                        string gids = string.Join(",", goodsList.Select(goods => goods.id).ToList());
                        sqlwhere += $" and goodsId in ({gids})";
                        break;
                    case 4://收货人
                        sqlwhere += " and consignee like @consignee";
                        parameters.Add(new MySqlParameter("@consignee", $"%{val}%"));
                        break;
                    case 5://手机号
                        sqlwhere += " and phone like @phone";
                        parameters.Add(new MySqlParameter("@phone", $"%{val}%"));
                        break;
                }
            }
            if (startDate != null && endDate != null)
            {
                sqlwhere += $" and addtime between @startDate and @endDate";
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
            }
            orderList = GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex, "*", "id desc");
            return orderList;
        }

        public int GetCountByUserId_GoodsId(int userId, int goodsId)
        {
            string sqlwhere = $"userId={userId} and goodsId={goodsId} and state>=0";
            return GetCount(sqlwhere);
        }

        public int GetCountByState_Date(int aid, int storeId, int state = -999, DateTime? startDate = null, DateTime? endDate = null)
        {
            int recordCount = 0;
            if (aid <= 0 || storeId <= 0)
            {
                return recordCount;
            }
            string sqlwhere = $"aid={aid} and storeId={storeId}";
            if (state != -999)
            {
                sqlwhere += $" and state={state}";
            }
            List<MySqlParameter> paramers = new List<MySqlParameter>();
            if (startDate != null && endDate != null)
            {
                sqlwhere += $" and addtime between @startDate and @endDate";
                paramers.Add(new MySqlParameter("@startDate", startDate));
                paramers.Add(new MySqlParameter("@endDate", endDate));
            }
            recordCount = GetCount(sqlwhere, paramers.ToArray());
            return recordCount;
        }

        /// <summary>
        /// 单个产品已拼数量
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int GetPinGoodsCount(int goodsId)
        {
            string sql = $"select IFNULL(sum(count),0) from pingoodsorder as o " +
                $"left join pingroup as g " +
                $"on o.groupid=g.id " +
                $"where " +
                $"g.state in (2,3) and g.goodsid={goodsId}";
            return Convert.ToInt32(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, sql));
        }
        /// <summary>
        /// 店铺已拼产品数量
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int GetStorePinGoodsCount(int storeId)
        {
            string sql = $"select IFNULL(sum(count),0) from pingoodsorder as o " +
                $"left join pingroup as g " +
                $"on o.groupid=g.id " +
                $"where " +
                $"g.state in (2,3) and o.storeid={storeId}";
            return Convert.ToInt32(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, sql));
        }

        /// <summary>
        /// 店铺产品数量
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public int GetStoreGoodsCount(int aId, int storeId)
        {
            return PinGoodsBLL.SingleModel.GetCount($"aid={aId} and storeid={storeId} and state=1");
        }

        /// <summary>
        /// 单个产品拼团人数
        /// </summary>
        /// <returns></returns>
        public int GetPinUserCount(int goodsId)
        {
            string sql = $"select count(DISTINCT userid)  from pingoodsorder as o" +
                $" left join pingroup as g" +
                $" on o.groupid=g.id " +
                $"where " +
                $"g.state in (2,3) and  g.goodsid={goodsId}";
            return Convert.ToInt32(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(), CommandType.Text, sql));
        }

        /// <summary>
        /// 检查待收货、待自取订单是否超过规定时间可以自动完成交易
        /// </summary>
        /// <returns></returns>
        public bool CheckOrderSuccess(int aid = 6933593)
        {
            
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aid);
            if (platform == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("拼享惠订单自动完成交易服务异常：找不到平台配置 aid:" + aid));
                return false;
            }
            if (platform.orderSuccessDays > 0)
            {
                DateTime date = DateTime.Now.AddDays(-platform.orderSuccessDays);
                string sqlwhere = $"ordertype=0 and aid={aid} and state in ({(int)PinOrderState.待收货},{(int)PinOrderState.待自取}) and payState!={(int)PayState.未付款} and paytime<'{date}'";
                List<PinGoodsOrder> list = GetList(sqlwhere);
                if (list != null && list.Count > 0)
                {
                    OrderSuccess(list, remark: "交易完成");
                }
            }
            return true;
        }

        #region 消息通知
        /// <summary>
        /// 订单支付成功发送支付成功模板消息
        /// </summary>
        /// <param name="pinOrder"></param>
        public void SendTemplateMsg_PaySuccess(PinGoodsOrder pinOrder)
        {
            
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(pinOrder.aid, pinOrder.storeId);
            if (store == null && pinOrder.orderType == 0)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠发送订单支付成功模板消息失败，找不到店铺信息 aid:{pinOrder.aid}, storeid：{pinOrder.storeId}"));
                return;
            }
            //发给用户通知
            object orderData = TemplateMsg_Miniapp.PinGetTemplateMessageData(SendTemplateMessageTypeEnum.拼享惠订单支付成功买家通知, order: pinOrder, store: store);
            TemplateMsg_Miniapp.SendTemplateMessage(pinOrder.userId, SendTemplateMessageTypeEnum.拼享惠订单支付成功买家通知, TmpType.拼享惠, orderData, $"pages/shopping/orderInfo/orderInfo?orderid={pinOrder.id}&storeid={pinOrder.storeId}");
            //发给商家通知
            if (pinOrder.orderType == 0)
            {
                TemplateMsg_Gzh.SendPaySuccessTemplateMessage(pinOrder);
            }
        }
        /// <summary>
        /// 订单取消发送订单取消通知模板消息
        /// </summary>
        /// <param name="pinOrder"></param>
        public void SendTemplateMsg_PayCancel(PinGoodsOrder pinOrder)
        {
            

            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(pinOrder.aid, pinOrder.storeId);
            if (store == null && pinOrder.orderType == 0)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠发送订单取消模板消息失败，找不到店铺信息 aid:{pinOrder.aid}, storeid：{pinOrder.storeId}"));
                return;
            }

            //如果未支付，获取预支付码的formid是无效的，要取消
            if (pinOrder.payState == (int)PayState.未付款)
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(pinOrder.userId);
                if (userInfo == null)
                {
                    log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠发送订单取消模板消息失败，找不到用户信息 userid：{pinOrder.userId}"));
                    return;
                }
                TemplateMsg_UserParam userParam = TemplateMsg_UserParamBLL.SingleModel.GetModel($" Form_Id is not null and appId = '{userInfo.appId}'  and  Open_Id = '{userInfo.OpenId}' and State = 1 and LoseDateTime > now() and orderId={pinOrder.id} ");
                if (userParam != null)
                {
                    userParam.State = -1;
                    TemplateMsg_UserParamBLL.SingleModel.Update(userParam, "state");
                }
            }
            //发给用户通知
            object orderData = TemplateMsg_Miniapp.PinGetTemplateMessageData(SendTemplateMessageTypeEnum.拼享惠订单取消通知, order: pinOrder, store: store);
            TemplateMsg_Miniapp.SendTemplateMessage(pinOrder.userId, SendTemplateMessageTypeEnum.拼享惠订单取消通知, TmpType.拼享惠, orderData, $"pages/shopping/orderInfo/orderInfo?orderid={pinOrder.id}&storeid={pinOrder.storeId}");
        }
        /// <summary>
        /// 订单退款发送退款通知模板消息
        /// </summary>
        /// <param name="pinOrder"></param>
        public void SendTemplateMsg_Refund(PinGoodsOrder pinOrder)
        {
            

            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(pinOrder.aid, pinOrder.storeId);
            if (store == null && pinOrder.orderType == 0)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠发送退款模板消息失败，找不到店铺信息 aid:{pinOrder.aid}, storeid：{pinOrder.storeId}"));
                return;
            }
            //发给用户通知
            object orderData = TemplateMsg_Miniapp.PinGetTemplateMessageData(SendTemplateMessageTypeEnum.拼享惠退款通知, order: pinOrder, store: store);
            TemplateMsg_Miniapp.SendTemplateMessage(pinOrder.userId, SendTemplateMessageTypeEnum.拼享惠退款通知, TmpType.拼享惠, orderData, $"pages/shopping/orderInfo/orderInfo?orderid={pinOrder.id}&storeid={pinOrder.storeId}");
        }
        /// <summary>
        /// 订单配送通知模板消息
        /// </summary>
        /// <param name="pinOrder"></param>
        public void SendTemplateMsg_Send(PinGoodsOrder pinOrder)
        {
            

            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(pinOrder.aid, pinOrder.storeId);
            if (store == null && pinOrder.orderType == 0)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"拼享惠订单配送模板消息失败，找不到店铺信息 aid:{pinOrder.aid}, storeid：{pinOrder.storeId}"));
                return;
            }
            //发给用户通知
            object orderData = TemplateMsg_Miniapp.PinGetTemplateMessageData(SendTemplateMessageTypeEnum.拼享惠订单配送通知, order: pinOrder, store: store);
            TemplateMsg_Miniapp.SendTemplateMessage(pinOrder.userId, SendTemplateMessageTypeEnum.拼享惠订单配送通知, TmpType.拼享惠, orderData, $"pages/shopping/orderInfo/orderInfo?orderid={pinOrder.id}&storeid={pinOrder.storeId}");
        }
        #endregion


        public List<PinGoodsOrder> GetListByDraw(string phone, int state, int pageIndex, int pageSize, ref int count, ref string msg)
        {
            List<PinGoodsOrder> list = new List<PinGoodsOrder>();

            
            int aid =6933593;
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                msg = "无效模板";
                return list;
            }
            if (string.IsNullOrEmpty(xcxrelation.AppId))
            {
                msg = "无效appid";
                return list;
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, xcxrelation.AppId);
            if (userInfo == null)
            {
                return list;
            }

            string sql = $"select {"{0}"} from pingoodsorder o left join pingroup g on o.groupid=g.id {"{1}"}";
            string sqlList = "";//string.Format(sql, "o.*");
            string sqlCount = "";// string.Format(sql, "Count(*)");
            string sqlWhere = $" where o.aid={aid} and o.userid = {userInfo.Id} ";

            if (state == 0)//未提现
            {
                sqlList = string.Format(sql, "o.*", "");
                sqlCount = string.Format(sql, "Count(*)", "");
                sqlWhere += $" and o.returnMoney>0 and g.groupcount <= g.successCount and g.state = {(int)PinEnums.GroupState.拼团成功} and o.isReturnMoney = {state}";
            }
            else if (state == 1)//提现中
            {
                sqlList = string.Format(sql, "o.*", "left join drawcashapply d on o.id=d.orderid");
                sqlCount = string.Format(sql, "Count(*)", "left join drawcashapply d on o.id=d.orderid");
                sqlWhere += $"  and o.isReturnMoney = {state} and d.drawState in({(int)DrawCashState.未开始提现},{(int)DrawCashState.提现中})";
            }
            else//提现成功
            {
                sqlList = string.Format(sql, "o.*", "left join drawcashapply d on o.id=d.orderid");
                sqlCount = string.Format(sql, "Count(*)", "left join drawcashapply d on o.id=d.orderid");
                sqlWhere += $"  and ((o.isReturnMoney = 1 and d.drawState={(int)DrawCashState.提现成功}) or (g.groupcount <= g.successCount and g.state = {(int)PinEnums.GroupState.已返利}))";
            }

            string sqlLimit = $" order by o.id desc limit {(pageIndex - 1) * pageSize},{pageSize} ";

            count = base.GetCountBySql(sqlCount + sqlWhere);
            //log4net.LogHelper.WriteInfo(this.GetType(), sqlList + sqlWhere + sqlLimit);
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sqlList + sqlWhere + sqlLimit))
            {
                while (dr.Read())
                {
                    PinGoodsOrder model = GetModel(dr);
                    list.Add(model);
                }
                return list;
            }
        }
    }
}