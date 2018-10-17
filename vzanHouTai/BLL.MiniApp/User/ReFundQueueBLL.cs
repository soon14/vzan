using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BLL.MiniApp
{
    public partial class ReFundQueueBLL : BaseMySql<ReFundQueue>
    {
        protected readonly CityMordersBLL _cityMordersBLL = new CityMordersBLL();
        

        #region 专业版退款

        /// <summary>
        ///  订单退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldState"></param>
        /// <param name="BuyMode">默认微信支付</param>
        /// <param name="isPartOut">是否部分退款</param>
        /// <returns></returns>
        public bool EntReFundQueue(EntGoodsOrder item, int oldState, int BuyMode = (int)miniAppBuyMode.微信支付, int? newState = null, bool isPartOut = false)
        {
            //重新加回库存
            if (EntGoodsOrderBLL.SingleModel.updateStock(item, oldState))
            {
                int money = isPartOut ? item.refundFee : item.BuyPrice;//兼容多版本，目前只有专业版订单有部分退款
                item.refundFee = money;
                if (BuyMode == (int)miniAppBuyMode.微信支付)
                {
                    try
                    {
                        item.outOrderDate = DateTime.Now;
                        if (item.BuyPrice == 0)  //金额为0时,回滚库存后,默认退款成功
                        {
                            item.State = (int)MiniAppEntOrderState.退款成功;
                        }
                        else
                        {
                            CityMorders order = _cityMordersBLL.GetModel(item.OrderId);
                            item.State = (int)MiniAppEntOrderState.退款中;
                            if (newState.HasValue)
                            {
                                item.State = newState.Value;
                            }
                            if (order == null)
                            {
                                item.State = (int)MiniAppEntOrderState.退款失败;
                                EntGoodsOrderBLL.SingleModel.Update(item, "State,outOrderDate,Remark,refundFee");
                                return false;
                            }
                            //微信支付
                            ReFundQueue reModel = new ReFundQueue
                            {
                                minisnsId = -5,
                                money = item.refundFee,
                                orderid = order.Id,
                                traid = order.trade_no,
                                addtime = DateTime.Now,
                                note = "小程序行业版退款",
                                retype = 1
                            };
                            base.Add(reModel);
                        }
                        bool isSuccess = EntGoodsOrderBLL.SingleModel.Update(item, "State,outOrderDate,Remark,refundFee");
                        if (isSuccess)
                        {
                            //发给用户退款通知
                            object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(item, SendTemplateMessageTypeEnum.专业版订单退款通知, "商家操作退款");
                            TemplateMsg_Miniapp.SendTemplateMessage(item.UserId, SendTemplateMessageTypeEnum.专业版订单退款通知, TmpType.小程序专业模板, orderData);
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序餐饮退款订单插入队列失败 ID={item.Id}");
                    }
                }
                else
                {
                    XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModel(item.aId);
                    if (r == null)
                        return false;

                    SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(r.AppId, item.UserId);
                    TransactionModel tran = new TransactionModel();
                    tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
                    {
                        AppId = r.AppId,
                        UserId = item.UserId,
                        MoneySetUserId = saveMoneyUser.Id,
                        Type = 1,
                        BeforeMoney = saveMoneyUser.AccountMoney,
                        AfterMoney = saveMoneyUser.AccountMoney + item.refundFee,
                        ChangeMoney = item.refundFee,
                        ChangeNote = $"专业版购买商品退款,订单号:{item.OrderNum} ",
                        CreateDate = DateTime.Now,
                        State = 1
                    }));

                    item.State = (int)MiniAppEntOrderState.退款成功;
                    if (newState.HasValue)
                    {
                        item.State = newState.Value;
                    }
                    tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {item.refundFee} where id =  {saveMoneyUser.Id} ; ");
                    tran.Add($" update EntGoodsOrder set State = {item.State },outOrderDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',Remark = @Remark where Id = {item.Id} and state <> {item.State} ; ",
                        new MySqlParameter[] { new MySqlParameter("@Remark", item.Remark) });//防止重复退款

                    //记录订单储值支付退款日志
                    tran.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = item.Id, UserId = item.UserId, LogInfo = $" 储值支付订单退款成功：{item.refundFee * 0.01} 元 ", CreateDate = DateTime.Now }));
                    bool isSuccess = ExecuteTransaction(tran.sqlArray, tran.ParameterArray);

                    if (isSuccess)
                    {
                        //发给用户退款通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(item, SendTemplateMessageTypeEnum.专业版订单退款通知, "商家操作退款");
                        TemplateMsg_Miniapp.SendTemplateMessage(item.UserId, SendTemplateMessageTypeEnum.专业版订单退款通知, TmpType.小程序专业模板, orderData);
                    }
                    return isSuccess;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 跟进 退款状态 (退款是否成功)
        /// </summary>
        /// <returns></returns>
        public bool UpdateEntReFundQueueOrderState()
        {
            TransactionModel tranModel = new TransactionModel();
            List<EntGoodsOrder> itemList = EntGoodsOrderBLL.SingleModel.GetList($" State = {(int)MiniAppEntOrderState.退款中} and templatetype in ({(int)TmpType.小程序专业模板},{(int)TmpType.小程序足浴模板},{(int)TmpType.小程序多门店模板}) and outOrderDate <= (NOW()-interval 17 second) ", 1000, 1) ?? new List<EntGoodsOrder>();
            List<CityMorders> orderList = new List<CityMorders>();
            List<ReFundResult> outOrderList = new List<ReFundResult>();
            if (itemList.Any())
            {
                orderList = _cityMordersBLL.GetList($" Id in ({string.Join(",", itemList.Select(x => x.OrderId))}) ", 1000, 1) ?? new List<CityMorders>();
                if (orderList.Any())
                {
                    outOrderList = RefundResultBLL.SingleModel.GetList($" transaction_id in ('{string.Join("','", orderList.Select(x => x.trade_no))}') and retype = 1") ?? new List<ReFundResult>();
                    itemList.ForEach(x =>
                    {
                        var curOrder = orderList.Where(y => y.Id == x.OrderId).FirstOrDefault();
                        if (curOrder != null)
                        {
                            //退款是排程处理,故无法确定何时执行退款,而现阶段退款操作成败与否都会记录在系统内
                            var curOutOrder = outOrderList.Where(y => y.transaction_id == curOrder.trade_no).FirstOrDefault();
                            if (curOutOrder != null && curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = (int)MiniAppEntOrderState.退款成功;
                                tranModel.Add(EntGoodsOrderBLL.SingleModel.BuildUpdateSql(x, "State"));
                            }
                            else if (curOutOrder != null && !curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = (int)MiniAppEntOrderState.退款失败;
                                tranModel.Add(EntGoodsOrderBLL.SingleModel.BuildUpdateSql(x, "State"));
                            }
                        }
                    });
                }
            }
            var isSuccess = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            return isSuccess;
        }

        #endregion 专业版退款

        #region 足浴退款

        /// <summary>
        /// 足浴版退款
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public bool ZYReFundQueue(string appId, EntGoodsOrder orderInfo, ServiceTime serviceTime)
        {
            bool result = false;
            if (orderInfo == null || orderInfo.Id <= 0)
            {
                return result;
            }
            orderInfo.outOrderDate = DateTime.Now;
            if (orderInfo.BuyMode == (int)miniAppBuyMode.储值支付)
            {
                var saveMoneyUser = new SaveMoneySetUser();
                saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appId, orderInfo.UserId);
                if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
                {
                    return result;
                }

                TransactionModel tran = new TransactionModel();
                tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
                {
                    AppId = saveMoneyUser.AppId,
                    UserId = orderInfo.UserId,
                    MoneySetUserId = saveMoneyUser.Id,
                    Type = 1,
                    BeforeMoney = saveMoneyUser.AccountMoney,
                    AfterMoney = saveMoneyUser.AccountMoney + orderInfo.BuyPrice,
                    ChangeMoney = orderInfo.BuyPrice,
                    ChangeNote = $" 购买商品,订单号:{orderInfo.OrderNum} ",
                    CreateDate = DateTime.Now,
                    State = 1
                }));
                saveMoneyUser.AccountMoney += orderInfo.BuyPrice;
                tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {orderInfo.BuyPrice} where id =  {saveMoneyUser.Id} ; ");
                tran.Add($" update EntGoodsOrder set state = {(int)MiniAppEntOrderState.退款成功 },outOrderDate = '{orderInfo.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss")}',Remark = @Remark where Id = {orderInfo.Id} and state <> {(int)MiniAppEntOrderState.退款成功 } ; ", new MySqlParameter[] { new MySqlParameter("@Remark", orderInfo.Remark) });//防止重复退款
                if (serviceTime != null)
                {
                    tran.Add($"update servicetime set time='{serviceTime.time}' where id={serviceTime.Id}");//取消已预订的技师服务时间
                }
                //记录订单退款日志
                tran.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = orderInfo.Id, UserId = orderInfo.UserId, LogInfo = $" 订单储值支付,退款成功：{orderInfo.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
                result = ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
                if (result)
                {
                    object objData = TemplateMsg_Miniapp.FootbathGetTemplateMessageData(orderInfo, SendTemplateMessageTypeEnum.足浴退款通知);
                    TemplateMsg_Miniapp.SendTemplateMessage(orderInfo.UserId, SendTemplateMessageTypeEnum.足浴退款通知, TmpType.小程序足浴模板, objData);
                }
                return result;
            }

            if (orderInfo.BuyMode == (int)miniAppBuyMode.微信支付)
            {
                CityMorders order = new CityMordersBLL().GetModel(orderInfo.OrderId);
                orderInfo.State = (int)MiniAppEntOrderState.退款中;

                if (order == null)
                {
                    orderInfo.State = (int)MiniAppEntOrderState.退款失败;
                    EntGoodsOrderBLL.SingleModel.Update(orderInfo, "State,outOrderDate,Remark");
                    return result;
                }

                //微信支付
                ReFundQueue reModel = new ReFundQueue
                {
                    minisnsId = -5,
                    money = orderInfo.BuyPrice,
                    orderid = order.Id,
                    traid = order.trade_no,
                    addtime = DateTime.Now,
                    note = "小程序足浴版退款",
                    retype = 1
                };
                try
                {
                    base.Add(reModel);
                    result = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "State,outOrderDate");
                    return result;
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序足浴退款订单插入队列失败 ID={orderInfo.Id}");
                }
            }
            return result;
        }

        #endregion 足浴退款

        #region 多门店退款

        /// <summary>
        /// 多门店退款
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public bool MultiStoreReFundQueue(List<EntGoodsCart> goodsCar, FootBath storeMaterial, EntGoodsOrder orderInfo)
        {
            bool result = false;
            orderInfo.outOrderDate = DateTime.Now;
            orderInfo.State = (int)MiniAppEntOrderState.退款中;
            if (orderInfo.BuyMode == (int)miniAppBuyMode.微信支付)
            {
                try
                {
                    if (orderInfo.BuyPrice == 0)
                    {
                        orderInfo.State = (int)MiniAppEntOrderState.退款成功;
                        EntGoodsOrderBLL.SingleModel.Update(orderInfo, "State,outOrderDate");
                    }
                    else
                    {
                        CityMorders order = _cityMordersBLL.GetModel(orderInfo.OrderId);
                        orderInfo.State = (int)MiniAppEntOrderState.退款中;
                        if (order == null)
                        {
                            orderInfo.State = (int)MiniAppEntOrderState.退款失败;

                            EntGoodsOrderBLL.SingleModel.Update(orderInfo, "State,outOrderDate");
                            return result;
                        }

                        //微信支付
                        ReFundQueue reModel = new ReFundQueue
                        {
                            minisnsId = -5,
                            money = orderInfo.BuyPrice,
                            orderid = order.Id,
                            traid = order.trade_no,
                            addtime = DateTime.Now,
                            note = "小程序多门店退款",
                            retype = 1
                        };
                        base.Add(reModel);
                    }
                    TransactionModel tranModel = new TransactionModel();
                    tranModel.Add(EntGoodsOrderBLL.SingleModel.BuildUpdateSql(orderInfo, "State,outOrderDate"));
                    if (!EntGoodsOrderBLL.SingleModel.HandleStockSql_MultiStore(goodsCar, storeMaterial, tranModel, -1))
                    {
                        log4net.LogHelper.WriteInfo(GetType(), "生成库存处理sql失败 HandleStockSql_MultiStore_error");
                        return false;
                    }
                    result = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                    return result;
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序多门店退款订单插入队列失败 ID={orderInfo.Id}");
                    log4net.LogHelper.WriteError(GetType(), ex);
                }
            }
            return result;
        }

        #endregion 多门店退款

        #region 拼团退款

        /// <summary>
        /// 拼团退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type">0：拼团失败退款，1：店主手动退款</param>
        /// <returns></returns>
        public bool EntGroupReFundQueue(EntGoodsOrder item, ref string msg)
        {

            int paytype = item.BuyMode;

            TransactionModel tranmodel = new MiniApp.TransactionModel();
            EntGroupSponsor csg = EntGroupSponsorBLL.SingleModel.GetModel(item.GroupId);
            if (csg == null)
            {
                msg = "小程序拼团商品不存在啦=" + item.GroupId;
                item.State = (int)MiniAppEntOrderState.已取消;
                EntGoodsOrderBLL.SingleModel.Update(item, "State");
                return false;
            }
            EntGroupSponsor gsinfo = EntGroupSponsorBLL.SingleModel.GetModel(item.GroupId);
            if (gsinfo == null)
            {
                msg = "小程序拼团团购不存在啦=" + item.GroupId;
                item.State = (int)MiniAppEntOrderState.已取消;
                EntGoodsOrderBLL.SingleModel.Update(item, "State");
                return false;
            }

            if (item.BuyPrice <= 0)
            {
                msg = "xxxxxxxxxxxxx小程序拼团价格为0不需要退款=" + item.Id;
                return false;
            }

            if (item.State == (int)MiniAppEntOrderState.退款成功)
            {
                msg = "xxxxxxxxxxxxx小程序拼团状态有误，不能退款=" + item.Id + ",paystate=" + item.State + "," + (int)MiniAppEntOrderState.退款成功;
                return false;
            }

            item.State = (int)MiniAppEntOrderState.退款成功;
            //更新用户订单状态
            tranmodel.Add($"update EntGoodsOrder set State={item.State} where id={item.Id}");

            //判断是否是微信支付
            if (paytype == (int)miniAppBuyMode.微信支付)
            {
                CityMorders order = _cityMordersBLL.GetModel(item.OrderId);
                if (order == null)
                {
                    msg = "xxxxxxxxxxxxxxxxxx小程序拼团退款查不到支付订单 ID=" + item.Id;
                    item.State = (int)MiniappPayState.已失效;
                    EntGoodsOrderBLL.SingleModel.Update(item, "State");
                    return false;
                }

                //插入退款队列
                ReFundQueue reModel = new ReFundQueue();
                reModel.minisnsId = -5;
                reModel.money = item.BuyPrice;
                reModel.orderid = item.OrderId;
                reModel.traid = order.trade_no;
                reModel.addtime = DateTime.Now;
                reModel.note = "小程序专业版拼团退款";
                reModel.retype = 1;
                tranmodel.Add(base.BuildAddSql(reModel));
            }
            else if (paytype == (int)miniAppBuyMode.储值支付)
            {
                //储值卡退款
                tranmodel.Add(SaveMoneySetUserBLL.SingleModel.GetCommandCarPriceSql(item.AppId, item.UserId, item.BuyPrice, 1, item.OrderId, item.OrderNum).ToArray());
                if (tranmodel.sqlArray.Length <= 0)
                {
                    msg = "xxxxxxxxxxxxxxxxxx专业版拼团储值卡退款失败，ID=" + item.Id;
                    return false;
                }
            }

            if (tranmodel.sqlArray.Length <= 0)
            {
                msg = "xxxxxxxxxxxxxxxxxx专业版拼团退款失败，ID=" + item.Id;
                return false;
            }

            if (!ExecuteTransactionDataCorect(tranmodel.sqlArray, tranmodel.ParameterArray))
            {
                msg = "xxxxxxxxxxxxxxxxxx专业版拼团退款事务执行失败，ID=" + item.Id + "sql:" + string.Join(";", tranmodel.sqlArray);
                return false;
            }

            if (!EntGoodsOrderBLL.SingleModel.updateStock(item, (int)MiniAppEntOrderState.退款成功))
            {
                msg = "xxxxxxxxxxxxxxxxxx专业版拼团退款更新库存失败，ID=" + item.Id;
                return false;
            }

            msg = "xxxxxxxxxxxxxxxxxx专业版拼团退款成功，ID=" + item.Id;

            //根据订单释放库存
            return true;
        }

        #endregion 拼团退款

        #region 砍价退款

        /// <summary>
        /// 砍价退款（照搬后台的）
        /// </summary>
        /// <param name="bargainUser"></param>
        /// <param name="bargain"></param>
        /// <param name="appId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool BargainReFundQueue(BargainUser bargainUser, Bargain bargain, string appId, out string msg)
        {
            bargainUser.State = 2;
            bargainUser.outOrderDate = DateTime.Now;

            if (bargainUser.PayType == (int)miniAppBuyMode.储值支付)
            {
                bargainUser.refundFee = bargainUser.CurrentPrice + bargain.GoodsFreight;
                bargainUser.State = 3;
                SaveMoneySetUser saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(appId, bargainUser.UserId);
                TransactionModel tran = new TransactionModel();
                tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
                {
                    AppId = appId,
                    UserId = bargainUser.UserId,
                    MoneySetUserId = saveMoneyUser.Id,
                    Type = 1,
                    BeforeMoney = saveMoneyUser.AccountMoney,
                    AfterMoney = saveMoneyUser.AccountMoney + bargainUser.refundFee,
                    ChangeMoney = bargainUser.refundFee,
                    ChangeNote = $"小程序砍价购买商品[{bargainUser.BName}]退款,订单号:{bargainUser.OrderId} ",
                    CreateDate = DateTime.Now,
                    State = 1
                }));

                tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {bargainUser.refundFee} where id =  {saveMoneyUser.Id} ; ");

                string updateBargainUser = BargainUserBLL.SingleModel.BuildUpdateSql(bargainUser, "State,outOrderDate,refundFee");

                tran.Add(updateBargainUser);

                bool isok = BargainBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray);
                if (isok)
                {
                    object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, "商家操作退款");
                    TemplateMsg_Miniapp.SendTemplateMessage(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, orderData);
                    msg = "退款成功,请查看账户余额!";
                }
                else
                {
                    msg = "退款异常!";//返回订单信息
                }
                return isok;
            }
            else
            {
                bool isok = false;

                CityMorders order = _cityMordersBLL.GetModel(bargainUser.CityMordersId);
                if (order == null)
                {
                    msg = "订单信息有误!";
                    return isok;
                }
                bargainUser.refundFee = bargainUser.CurrentPrice + bargain.GoodsFreight;
                if (BargainUserBLL.SingleModel.Update(bargainUser, "State,outOrderDate,refundFee"))
                {
                    ReFundQueue reModel = new ReFundQueue
                    {
                        minisnsId = -5,
                        money = bargainUser.refundFee,
                        orderid = order.Id,
                        traid = order.trade_no,
                        addtime = DateTime.Now,
                        note = "小程序砍价订单退款",
                        retype = 1
                    };
                    try
                    {
                        int funid = Convert.ToInt32(base.Add(reModel));
                        if (funid > 0)
                        {
                            object orderData = TemplateMsg_Miniapp.BargainGetTemplateMessageData(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, "商家操作退款");
                            TemplateMsg_Miniapp.SendTemplateMessage(bargainUser, SendTemplateMessageTypeEnum.砍价订单退款通知, orderData);
                            isok = true;
                            msg = "操作成功,已提交退款申请!";
                            return isok;
                        }
                        else
                        {
                            isok = false;
                            msg = "退款异常插入队列小于0!";
                            return isok;
                        }
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序砍价退款订单插入队列失败 ID={order.Id}");
                        isok = false;
                        msg = "退款异常(插入队列失败)!";
                        return isok;
                    }
                }
                else
                {
                    isok = false;
                    msg = "退款异常!";
                    return isok;
                }
            }
        }

        #endregion 砍价退款

        #region 团购退款

        /// <summary>
        /// 拼团退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type">0：拼团失败退款，1：店主手动退款</param>
        /// <returns></returns>
        public bool GroupReFundQueue(GroupUser item, ref string msg, int type = 0)
        {
            //0：微信支付，1：储值卡支付
            int paytype = item.PayType;

            TransactionModel tranmodel = new TransactionModel();
            Groups csg = GroupsBLL.SingleModel.GetModel(item.GroupId);
            if (csg == null)
            {
                msg = "小程序拼团商品不存在啦=" + item.GroupId;
                item.State = (int)MiniappPayState.已失效;
                GroupUserBLL.SingleModel.Update(item, "State");
                return false;
            }
            GroupSponsor gsinfo = GroupSponsorBLL.SingleModel.GetModel(item.GroupSponsorId);
            if (gsinfo == null && item.IsGroup == 1)
            {
                msg = "小程序拼团团购不存在啦=" + item.GroupSponsorId;
                item.State = (int)MiniappPayState.已失效;
                GroupUserBLL.SingleModel.Update(item, "State");
                return false;
            }

            if (item.BuyPrice <= 0)
            {
                msg = "xxxxxxxxxxxxx小程序拼团价格为0不需要退款=" + item.Id;
                return false;
            }

            if (item.PayState == (int)MiniappPayState.已退款)
            {
                msg = "xxxxxxxxxxxxx小程序拼团状态有误，不能退款=" + item.Id + ",paystate=" + item.PayState + "," + (int)MiniappPayState.已退款;
                return false;
            }

            item.State = (int)MiniappPayState.已退款;
            //更新用户订单状态
            tranmodel.Add($"update GroupUser set State={item.State} where id={item.Id}");

            //判断是否是微信支付
            if (paytype == 0)
            {
                CityMorders order = _cityMordersBLL.GetModel(item.OrderId);
                if (order == null)
                {
                    msg = "xxxxxxxxxxxxxxxxxx小程序拼团退款查不到支付订单 ID=" + item.Id;
                    item.State = (int)MiniappPayState.已失效;
                    GroupUserBLL.SingleModel.Update(item, "State");
                    return false;
                }

                //插入退款队列
                ReFundQueue reModel = new ReFundQueue();
                reModel.minisnsId = -5;
                reModel.money = item.BuyPrice;
                reModel.orderid = item.OrderId;
                reModel.traid = order.trade_no;
                reModel.addtime = DateTime.Now;
                reModel.note = "小程序拼团退款";
                reModel.retype = 1;
                tranmodel.Add(base.BuildAddSql(reModel));
            }
            else if (paytype == 1)
            {
                //储值卡退款
                tranmodel.Add(SaveMoneySetUserBLL.SingleModel.GetCommandCarPriceSql(item.AppId, item.ObtainUserId, item.BuyPrice, 1, item.OrderId, item.OrderNo).ToArray());
                if (tranmodel.sqlArray.Length <= 0)
                {
                    msg = "xxxxxxxxxxxxxxxxxx拼团储值卡退款失败，ID=" + item.Id;
                    return false;
                }
            }

            //是店主手动退款不加库存 --统一,只要是退款就加库存
            //if (type == 0)
            {
                if (gsinfo.State == 2 && item.IsGroup == 1)
                {
                    msg = "小程序团购成功，不能退款=" + item.GroupSponsorId;
                    return false;
                }

                //退款成功，更新剩余数量
                tranmodel.Add($"update groups set RemainNum ={(csg.RemainNum + item.BuyNum)} where id={csg.Id}");
                //LogHelper.WriteInfo(GetType(), $"修改拼团失败库存：update groups set RemainNum ={(csg.RemainNum + item.BuyNum)} where id={csg.Id}");
            }

            if (tranmodel.sqlArray.Length <= 0)
            {
                msg = "xxxxxxxxxxxxxxxxxx拼团退款失败，ID=" + item.Id;
                return false;
            }

            if (!ExecuteTransactionDataCorect(tranmodel.sqlArray, tranmodel.ParameterArray))
            {
                msg = "xxxxxxxxxxxxxxxxxx拼团退款事务执行失败，ID=" + item.Id + "sql:" + string.Join(";", tranmodel.sqlArray);
                return false;
            }

            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(item.AppId);
            if (xcx == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"发送模板消息,参数不足,XcxAppAccountRelation_null:appId = {item.AppId}"));
                return true;
            }

            //发给用户发货通知
            object groupData = TemplateMsg_Miniapp.GroupGetTemplateMessageData("商家操作退款", item, SendTemplateMessageTypeEnum.拼团基础版订单退款通知);
            TemplateMsg_Miniapp.SendTemplateMessage(item.ObtainUserId, SendTemplateMessageTypeEnum.拼团基础版订单退款通知, xcx.Type, groupData);
            msg = "xxxxxxxxxxxxxxxxxx拼团退款成功，ID=" + item.Id;
            return true;
        }

        #endregion 团购退款

        #region 餐饮退款

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool FoodReFundQueue(FoodGoodsOrder item, int oldState)
        {
            
            item.State = (int)miniAppFoodOrderState.退款中;
            item.outOrderDate = DateTime.Now;

            //重新加回库存
            if (FoodGoodsOrderBLL.SingleModel.updateStock(item, oldState))
            {
                try
                {
                    //微信退款只在金额大于0的时候去插入申请队列,小于0时直接将状态改为退款成功,并将状态回滚
                    if (item.BuyPrice > 0)
                    {
                        CityMorders order = _cityMordersBLL.GetModel(item.OrderId);
                        if (order == null) //找不到微信订单直接返回退款失败
                        {
                            item.State = (int)miniAppFoodOrderState.退款失败;
                            FoodGoodsOrderBLL.SingleModel.Update(item, "State,outOrderDate,Remark");
                            return false;
                        }

                        ReFundQueue reModel = new ReFundQueue
                        {
                            minisnsId = -5,
                            money = item.BuyPrice,
                            orderid = order.Id,
                            traid = order.trade_no,
                            addtime = DateTime.Now,
                            note = "小程序餐饮订单退款",
                            retype = 1
                        };
                        base.Add(reModel);
                    }
                    else
                    {
                        item.State = (int)miniAppFoodOrderState.已退款;
                    }

                    FoodGoodsOrderBLL.SingleModel.Update(item, "State,outOrderDate,Remark");
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序餐饮退款订单插入队列失败 ID={item.Id}");
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 餐饮储值支付 退款
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool FoodSaveMoneyReFundQueue(FoodGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser, int oldState)
        {
            
            
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }

            //回退库存
            if (!FoodGoodsOrderBLL.SingleModel.updateStock(dbOrder, oldState))
            {
                return false;
            }

            TransactionModel tran = new TransactionModel();
            tran.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = dbOrder.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = 1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney + dbOrder.BuyPrice,
                ChangeMoney = dbOrder.BuyPrice,
                ChangeNote = $" 购买商品,订单号:{dbOrder.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1
            }));
            saveMoneyUser.AccountMoney += dbOrder.BuyPrice;
            tran.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney + {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} ; ");
            tran.Add($" update foodgoodsorder set state = {(int)miniAppFoodOrderState.已退款 },outOrderDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',Remark = @Remark where Id = {dbOrder.Id} and state <> {(int)miniAppFoodOrderState.已退款 } ; ", new MySqlParameter[] { new MySqlParameter("@Remark", dbOrder.Remark) });//防止重复退款

            //记录订单退款日志
            tran.Add(FoodGoodsOrderLogBLL.SingleModel.BuildAddSql(new FoodGoodsOrderLog() { GoodsOrderId = dbOrder.Id, UserId = dbOrder.UserId.ToString(), LogInfo = $" 订单储值支付,退款成功：{dbOrder.BuyPrice * 0.01} 元 ", CreateDate = DateTime.Now }));
            return ExecuteTransaction(tran.sqlArray, tran.ParameterArray);
        }

        ///// <summary>
        ///// 跟进 退款状态 (退款是否成功)
        ///// </summary>
        ///// <returns></returns>
        public bool FoodUpdateReFundQueueState()
        {
            
            TransactionModel tranModel = new TransactionModel();
            List<FoodGoodsOrder> itemList = FoodGoodsOrderBLL.SingleModel.GetList($" State = {(int)miniAppFoodOrderState.退款中} and outOrderDate <= (NOW()-interval 17 second) ", 1000, 1) ?? new List<FoodGoodsOrder>();
            List<CityMorders> orderList = new List<CityMorders>();
            List<ReFundResult> outOrderList = new List<ReFundResult>();
            if (itemList.Any())
            {
                orderList = _cityMordersBLL.GetList($" Id in ({string.Join(",", itemList.Select(x => x.OrderId))}) ", 1000, 1) ?? new List<CityMorders>();
                if (orderList.Any())
                {
                    outOrderList = RefundResultBLL.SingleModel.GetList($" transaction_id in ('{string.Join("','", orderList.Select(x => x.trade_no))}') and retype = 1") ?? new List<ReFundResult>();
                    itemList.ForEach(x =>
                    {
                        CityMorders curOrder = orderList.Where(y => y.Id == x.OrderId).FirstOrDefault();
                        if (curOrder != null)
                        {
                            //退款是排程处理,故无法确定何时执行退款,而现阶段退款操作成败与否都会记录在系统内
                            ReFundResult curOutOrder = outOrderList.Where(y => y.transaction_id == curOrder.trade_no).FirstOrDefault();
                            if (curOutOrder != null && curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = (int)miniAppFoodOrderState.已退款;
                                tranModel.Add(FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(x, "State"));
                            }
                            else if (curOutOrder != null && !curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = (int)miniAppFoodOrderState.退款失败;
                                tranModel.Add(FoodGoodsOrderBLL.SingleModel.BuildUpdateSql(x, "State"));
                            }
                        }
                    });
                }
            }
            return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        }

        #endregion 餐饮退款

        public ReFundQueue GetModelByOrderId(int orderid)
        {
            return base.GetModel($"orderid={orderid}");
        }
    }

    /// <summary>
    /// 报表
    /// </summary>
    public class ReFundQueueReportBLL : BaseMySql<ReFundQueueReport>
    {
        /// <summary>
        /// 退款记录报表
        /// </summary>
        /// <returns></returns>
        public List<ReFundQueueReport> getReFundQueueReport(DateTime? starttime = null, DateTime? endtime = null, int pageIndex = 1, int pageSize = 50)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" select r.addtime,r.orderid,r.trano,r.money,r.note, ");
            sb.Append(" (case when r.trano != '' then '已处理' else '未知' end) as state_name, ");
            sb.Append(@" (case r.state
						when 1 then
									(case s.result_code
												when 'SUCCESS' then '处理成功'
												when 'FAIL' then '处理失败' else '未知' end)
						when 0 then '证书不匹配'
						else '未知' end) as result_msg,  ");
            sb.Append(" (case when s.result_code = 'FAIL' then s.err_code_des else '' end) as result_note ");
            sb.Append(" from ");
            sb.Append($" (select * from refundqueue  ");
            sb.Append($" where retype = 1 "); //retype = 1 :同城退款记录
            sb.Append(starttime != null ? $" and DATE_FORMAT(addtime,'%Y-%m-%d') >=  DATE_FORMAT(@starttime,'%Y-%m-%d')" : "");
            sb.Append(endtime != null ? $" and DATE_FORMAT(addtime,'%Y-%m-%d') <=  DATE_FORMAT(@endtime,'%Y-%m-%d')" : "");
            sb.Append($" order by addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ) as r  "); //retype = 1 :同城退款记录
            sb.Append(" left join  refundresult s  on r.trano = s.transaction_id ");

            List<MySqlParameter> mysql = new List<MySqlParameter>();
            if (starttime != null)
            {
                mysql.Add(new MySqlParameter("@starttime", starttime));
            }
            if (endtime != null)
            {
                mysql.Add(new MySqlParameter("@endtime", endtime));
            }

            //List<ReFundQueue> list = base.GetListBySql(sb.ToString());
            List<ReFundQueueReport> list = new List<ReFundQueueReport>();
            using (MySqlDataReader reader = SqlMySql.ExecuteDataReader(new ReFundQueueBLL().connName, CommandType.Text, sb.ToString(), mysql.ToArray()))
            {
                while (reader.Read())
                {
                    list.Add(GetModel(reader));
                }
            }
            return list ?? new List<ReFundQueueReport>();
        }

        /// <summary>
        /// 退款记录报表所选时段总流水
        /// </summary>
        /// <returns></returns>
        public double getReFundQueueReport_SumMoney(DateTime? starttime = null, DateTime? endtime = null, int pageIndex = 1, int pageSize = 50)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($" select ifnull(sum(r.money),0) from refundqueue  ");
            sb.Append($" where retype = 1 "); //retype = 1 :同城退款记录
            sb.Append(starttime != null ? $" and DATE_FORMAT(addtime,'%Y-%m-%d') >=  DATE_FORMAT(@starttime,'%Y-%m-%d')" : "");
            sb.Append(endtime != null ? $" and DATE_FORMAT(addtime,'%Y-%m-%d') <=  DATE_FORMAT(@endtime,'%Y-%m-%d')" : "");
            //sb.Append($" order by addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ) as r  "); //retype = 1 :同城退款记录

            List<MySqlParameter> mysql = new List<MySqlParameter>();
            if (starttime != null)
            {
                mysql.Add(new MySqlParameter("@starttime", starttime));
            }
            if (endtime != null)
            {
                mysql.Add(new MySqlParameter("@endtime", endtime));
            }

            var sumMoney = Convert.ToDouble(SqlMySql.ExecuteScalar(new ReFundQueueBLL().connName, CommandType.Text, sb.ToString(), mysql.ToArray()));

            return sumMoney;
        }

        /// <summary>
        /// 退款记录报表
        /// </summary>
        /// <returns></returns>
        public int getReFundQueueReport_Count(DateTime? starttime = null, DateTime? endtime = null, int pageIndex = 1, int pageSize = 50)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($" select count(0) from refundqueue  ");
            sb.Append($" where retype = 1 "); //retype = 1 :同城退款记录
            sb.Append(starttime != null ? $" and DATE_FORMAT(addtime,'%Y-%m-%d') >=  DATE_FORMAT(@starttime,'%Y-%m-%d')" : "");
            sb.Append(endtime != null ? $" and DATE_FORMAT(addtime,'%Y-%m-%d') <=  DATE_FORMAT(@endtime,'%Y-%m-%d')" : "");

            List<MySqlParameter> mysql = new List<MySqlParameter>();
            if (starttime != null)
            {
                mysql.Add(new MySqlParameter("@starttime", starttime));
            }
            if (endtime != null)
            {
                mysql.Add(new MySqlParameter("@endtime", endtime));
            }

            //List<ReFundQueue> list = base.GetListBySql(sb.ToString());
            List<ReFundQueueReport> list = new List<ReFundQueueReport>();
            return Convert.ToInt32(SqlMySql.ExecuteScalar(Utility.dbEnum.QLWL.ToString(), CommandType.Text, sb.ToString(), mysql.ToArray()));
        }
    }

    public class ReFundQueueReportAmountBLL : BaseMySql<ReFundQueueReportAmount>
    {
        public ReFundQueueReportAmount getMoneyAmount()
        {
            ReFundQueueReportAmount amount = new ReFundQueueReportAmount();

            //当日统计
            string day_amount_sql = $" select ifnull(sum(money),0) as day_amount from refundqueue "
                                  + $" WHERE retype = 1 "
                                  + $" and date_format(addtime, '%Y-%m-%d') = date_format(now(), '%Y-%m-%d'); ";
            amount.day_amount = Convert.ToDouble(SqlMySql.ExecuteScalar(Utility.dbEnum.QLWL.ToString(), CommandType.Text, day_amount_sql, new MySqlParameter[] { }));
            //当周统计
            string week_amount_sql = $" SELECT ifnull(sum(money),0) as week_amount FROM refundqueue "
                                   + $" WHERE retype = 1 "
                                   + $" and YEARWEEK(date_format(addtime, '%Y-%m-%d')) = YEARWEEK(date_format(now(), '%Y-%m-%d')); ";
            amount.week_amount = Convert.ToDouble(SqlMySql.ExecuteScalar(Utility.dbEnum.QLWL.ToString(), CommandType.Text, week_amount_sql, new MySqlParameter[] { }));
            //当月统计
            string month_amount_sql = $" SELECT ifnull(sum(money),0) as month_amount FROM refundqueue "
                                    + $" WHERE retype = 1 "
                                    + $" and date_format(addtime, '%Y-%m') = date_format(now(), '%Y-%m'); ";
            amount.month_amount = Convert.ToDouble(SqlMySql.ExecuteScalar(Utility.dbEnum.QLWL.ToString(), CommandType.Text, month_amount_sql, new MySqlParameter[] { }));

            //string day_amount_sql = $" SELECT sum(payment_free) as day_amount FROM citymorders WHERE YEARWEEK(date_format(payment_time, '%Y-%m-%d')) = YEARWEEK(date_format('{date}', '%Y-%m-%d')); ";
            //amount.day_amount = Convert.ToDouble(SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, day_amount_sql, null));
            return amount;
        }
    }
}