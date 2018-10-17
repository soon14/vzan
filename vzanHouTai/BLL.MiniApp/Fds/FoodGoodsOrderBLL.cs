using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utility;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsOrderBLL : BaseMySql<FoodGoodsOrder>
    {
        #region 单例模式
        private static FoodGoodsOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsOrderBLL()
        {

        }

        public static FoodGoodsOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 提货码缓存Key
        /// </summary>
        public static readonly string _verificationNumKey = "dz_VerificationNumKey";

        public List<FoodGoodsOrder> GetListByGoodsState(int storeid, string groupids, miniAppFoodOrderState state)
        {
            return base.GetList($"storeid={storeid} and state={(int)state} and groupid in ({groupids})");
        }

        public List<FoodGoodsOrder> GetOrderList(int storeid, int userid, int pageSize, int pageIndex, int goodtype, int State)
        {
            string stateSql = (State != 10 ? $" and State = {State} " : "");
            return base.GetList($" StoreId = {storeid} and UserId = {userid} { stateSql } and State != {(int)miniAppFoodOrderState.付款中} and groupid=0", pageSize, pageIndex, "*", "CreateDate desc");
        }

        /// <summary>
        /// 获取餐饮提货码
        /// </summary>
        /// <returns></returns>
        public string GetVerificationNum()
        {
            string num = RedisUtil.Get<string>(_verificationNumKey);
            if (num == null || num.Length == 0)
            {
                string sql = "select max(verificationnum)from foodgoodsorder";
                object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
                num = "0";
                if (result != DBNull.Value)
                {
                    num = result.ToString();
                }
            }

            num = (int.Parse(num) + 1).ToString("000000");
            RedisUtil.Set<string>(_verificationNumKey, num);
            return num;
        }

        /// <summary>
        /// 获取订单号
        /// </summary>
        /// <param name="goodsOrder"></param>
        /// <returns></returns>
        public string GetGoodsOrderNo(FoodGoodsOrder goodsOrder)
        {
            return goodsOrder == null ? "" : goodsOrder.CreateDate.ToString("yyyy MMdd ") +
                   goodsOrder.Id.ToString().PadLeft(8, '0').Insert(4, " ");
        }


        //// <summary>
        /// 查订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="state">订单状态</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<FoodGoodsOrder> GetOrderByParames(int foodId, int type = 10, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10)
        {
            //条件拼接
            string strWhere = $" StoreId={foodId} ";
            orderNum = orderNum.Replace("'", "");
            strWhere += $" {(!string.IsNullOrWhiteSpace(orderNum) ? $" and OrderNum = '{orderNum}' " : "")} ";

            strWhere += $" { ((type == 10 || !Enum.IsDefined(typeof(miniAppFoodOrderType), type)) ? "" : $" and OrderType = {type} ")} ";

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(miniAppFoodOrderState), orderState)) ? $" and State != {(int)miniAppFoodOrderState.付款中} " : $" and State != {(int)miniAppFoodOrderState.付款中} and State = {orderState} ")} ";
            if (foodGoodsType != -999)
            {
                List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    List<FoodGoodsCart> carts = FoodGoodsCartBLL.SingleModel.GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.Id))}) and state = 1");
                    if (carts != null && carts.Any())
                    {
                        strWhere += $" and Id in ({ string.Join(",", carts.Select(x => x.GoodsOrderId).Distinct()) }) ";
                    }
                }
            }

            string orderField = "CreateDate DESC";
            List<FoodGoodsOrder> list = GetList(strWhere, pageSize, pageIndex, "*", orderField);
            return list;
        }

        //// <summary>
        /// 查订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="state">订单状态</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public int GetOrderByParamesCount(int foodId, int type = 10, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10)
        {
            string strWhere = $" StoreId={foodId} ";
            orderNum = orderNum.Replace("'", "");
            strWhere += $" {(!string.IsNullOrWhiteSpace(orderNum) ? $" and OrderNum = '{orderNum}' " : "")} ";

            strWhere += $" { ((type == 10 || !Enum.IsDefined(typeof(miniAppFoodOrderType), type)) ? "" : $" and OrderType = {type} ")} ";

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(miniAppFoodOrderState), orderState)) ? $" and State != {(int)miniAppFoodOrderState.付款中} " : $" and State != {(int)miniAppFoodOrderState.付款中} and State = {orderState} ")} ";

            //strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(C_Enums.miniAppFoodOrderState), orderState)) ? "" : $" and State = {orderState} ")} ";

            if (foodGoodsType != -999)
            {
                List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    List<FoodGoodsCart> carts = FoodGoodsCartBLL.SingleModel.GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.Id))}) and state = 1");
                    if (carts != null && carts.Any())
                    {
                        strWhere += $" and Id in ({ string.Join(",", carts.Select(x => x.GoodsOrderId).Distinct()) }) ";
                    }
                }
            }

            //string orderField = "CreateDate DESC";
            int totle = GetCount(strWhere);
            return totle;
        }



        //// <summary>
        /// 查购买者的订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="state">订单状态</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<FoodGoodsOrder> GetUserToOrder(int userId, int state, int pageIndex, int pageSize = 10)
        {
            string strWhere = $" UserId={userId}";

            switch (state)
            {
                case (int)OrderState.未付款:
                case (int)OrderState.待核销:
                case (int)OrderState.已核销:
                case (int)OrderState.待发货:
                case (int)OrderState.正在配送:
                case (int)OrderState.待收货:
                case (int)OrderState.已收货:
                case (int)OrderState.已退款:
                    strWhere += $" and State={state}";
                    break;
                case -10:
                    strWhere += $" and (State={(int)OrderState.待收货} or State={(int)OrderState.正在配送} or State={(int)OrderState.待核销})";
                    break;
            }
            string orderField = "CreateDate DESC";
            List<FoodGoodsOrder> list = GetList(strWhere, pageSize, pageIndex, "*", orderField);
            return list;
        }

        /// <summary>
        /// 商品订单过期//不用
        /// </summary>
        public void CityGoodsOrderTimeOut(int timeoutlength = -30)
        {
            TransactionModel tranModel = new TransactionModel();
            
            
            

            //订单超过30分钟取消订单
            List<FoodGoodsOrder> orderList = GetList($"State=0 and  CreateDate < date_add(NOW(), interval {timeoutlength} MINUTE)", 100, 1);
            if (orderList != null && orderList.Count > 0)
            {
                tranModel = new TransactionModel();
                //订单明细
                List<FoodGoodsCart> orderdetaillist = orderList.Any() ? FoodGoodsCartBLL.SingleModel.GetList($"State =0 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<FoodGoodsCart>();
                if (orderdetaillist != null && orderdetaillist.Count > 0)
                {
                    //商品
                    List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                    if (goodlist != null && goodlist.Count > 0)
                    {
                        foreach (FoodGoodsOrder item in orderList)
                        {
                            //商品明细
                            List<FoodGoodsCart> orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.OrderId).ToList() : new List<FoodGoodsCart>();
                            if (orderdetails != null && orderdetails.Count > 0)
                            {
                                for (int i = 0; i < orderdetails.Count; i++)
                                {
                                    //商品
                                    FoodGoods good = goodlist.Where(w => w.Id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                                    if (good != null)
                                    {
                                        //商品加总库存
                                        good.Inventory += orderdetails[i].Count;
                                        //订单明细中的规格属性，加规格属性库存
                                        for (int j = 0; j < good.GASDetailList.Count; j++)
                                        {
                                            if (good.GASDetailList[j].id == orderdetails[i].SpecIds)
                                            {
                                                good.GASDetailList[j].count += orderdetails[i].Count;
                                                good.AttrDetail = SerializeHelper.SerToJson(good.GASDetailList);
                                                break;
                                            }
                                        }
                                        //更改商品总库存和规格属性库存
                                        tranModel.Add($"update FoodGoods set AttrDetail={good.AttrDetail},Inventory={good.Inventory} where Id={good.Id}");
                                    }
                                }
                            }

                            //订单状态改成已过期
                            item.State = -1;
                            tranModel.Add($"update FoodGoodsOrder set State={item.State} where Id={item.Id}");

                            ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 方法操作：1.创建订单,2.将购物车对应内容转为订单内容,3.减库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="besidesSql">另外需要执行的sql</param>
        /// <param name="goodsCar"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool addGoodsOrder(ref FoodGoodsOrder order, List<FoodGoodsCart> goodsCar, int userid, StringBuilder besidesSql, ref string msg)
        {
            TransactionModel TranModel = new TransactionModel();

            if (besidesSql != null)
            {
                TranModel.Add(besidesSql.ToString());
            }

            //创建订单
            TranModel.Add(BuildAddSql(order));
            //将购物车记录转为订单明细记录
            TranModel.Add($" update FoodGoodsCart set GoodsOrderId = (select last_insert_id()),State = 1,UserId = {userid} where id in ({string.Join(",", goodsCar.Select(s => s.Id).Distinct())}) and state = 0; ");

            //根据订单内记录数量减库存（预约点餐不消耗库存，而是在商户接单是消耗库存）
            if (order.OrderType == (int)miniAppFoodOrderType.预约)
            {
                FoodGoodsCartBLL.SingleModel.UpdateCartGoodsInfo(tran: ref TranModel, shopCartItem: goodsCar);
            }
            else
            {
                //生成减库存的sql 并计入TranModel
                GetStockUpdateTranSql(TranModel: ref TranModel, goodsCar: goodsCar);
            }

            
            string key = string.Format(FoodGoodsBLL.foodGoodsKey, order.StoreId);
            RedisUtil.Remove(key);//移除该店铺下的所有缓存的产品

            if (!ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                msg = "您的订单在出单过程中失败,节点代号000";
                return false;
            }

            #region 取到数据库的订单Model赋值给order(因为底层事务的写法暂不支持直接取到ID,故采用此写法来取到model)
            FoodGoodsCart cartmodel = FoodGoodsCartBLL.SingleModel.GetModel("Id=" + goodsCar[0].Id + " and GoodsOrderId>0");
            if (cartmodel == null)
            {
                msg = "您的订单在出单过程中失败,节点代号001";
                return false;
            }
            int curGoodOrderId = cartmodel.GoodsOrderId;
            order = GetModel(curGoodOrderId);
            if (order == null)
            {
                msg = "您的订单在出单过程中失败,节点代号002";
                return false;
            }
            #endregion

            //添加订单日志记录
            FoodGoodsOrderLog curOrderLog = new FoodGoodsOrderLog()
            {
                GoodsOrderId = order.Id,
                UserId = order.UserId.ToString(),
                LogInfo = $" 成功下单,下单金额：{order.BuyPrice * 0.01} 元 ",
                CreateDate = DateTime.Now
            };
            FoodGoodsOrderLogBLL.SingleModel.Add(curOrderLog);
            return true;
        }

        /// <summary>
        /// 生成减库存的sql 并计入TranModel
        /// </summary>
        /// <param name="TranModel"></param>
        /// <param name="goodsCar"></param>
        public void GetStockUpdateTranSql(ref TransactionModel TranModel, List<FoodGoodsCart> goodsCar)
        {
            List<FoodGoods> goods = FoodGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.FoodGoodsId).Distinct().ToList())}) ");
            Utility.Easyui.EasyuiHelper<FoodGoodsAttrDetail> goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<FoodGoodsAttrDetail>();
            goodsCar.ForEach(x =>
            {
                FoodGoods good = goods.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(x.SpecIds))
                {

                    good.Stock -= x.Count;
                }
                else
                {
                    good.Stock -= x.Count;
                    List<FoodGoodsAttrDetail> GASDetailList = new List<FoodGoodsAttrDetail>();
                    good.GASDetailList.ForEach(y =>
                    {
                        if (y.id.Equals(x.SpecIds))
                        {
                            y.count -= x.Count;
                        }
                        GASDetailList.Add(y);
                    });
                    //规格库存详情重新赋值
                    good.AttrDetail = goodDtlJsonHelper.SToJsonArray(GASDetailList);
                }
            });
            foreach (FoodGoods good in goods)
            {
                TranModel.Add($" update Foodgoods set Stock={good.Stock},AttrDetail='{good.AttrDetail}' where Id = {good.Id} ");
            }
        }

        /// <summary>
        /// 获取预约订单
        /// </summary>
        /// <param name="reserveId"></param>
        /// <returns></returns>
        public List<FoodGoodsOrder> GetOrderByReservation(int reserveId, int state = -1)
        {
            string whereSql = $"ReserveId = {reserveId}";
            if (state > -1)
            {
                whereSql = $"{whereSql} AND State = {state}";
            }
            return GetList(whereSql, 99, 1, "*");
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="timeoutlength"></param>
        public bool updateFoodOrderState(FoodGoodsOrder order, int oldState, string updateColNames)
        {
            string updateSql = BuildUpdateSql(order, updateColNames) + $" and state = {oldState} ;";

            int updateLine = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);


            return updateLine >= 1;
        }

        /// <summary>
        /// 订单作废需要对应操作库存
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public bool updateStock(FoodGoodsOrder order, int oldState)
        {
            if (order == null)
            {
                return false;
            }
            if (!Enum.IsDefined(typeof(miniAppFoodOrderState), oldState)
                    || !Enum.IsDefined(typeof(miniAppFoodOrderState), order.State))
            {
                return false;
            }
            TransactionModel tranModel = new TransactionModel();
            
            
            
            

            //MiniappFoodGoodsOrder order = GetModel(orderid);
            tranModel = new TransactionModel();
            //订单明细
            List<FoodGoodsCart> orderdetaillist = FoodGoodsCartBLL.SingleModel.GetList($" State = 1 and GoodsOrderId in ({order.Id})") ?? new List<FoodGoodsCart>();
            if (orderdetaillist != null && orderdetaillist.Count > 0)
            {
                //商品
                List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                if (goodlist != null && goodlist.Count > 0)
                {

                    //商品明细
                    List<FoodGoodsCart> orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == order.Id).ToList() : new List<FoodGoodsCart>();
                    if (orderdetails != null && orderdetails.Count > 0)
                    {
                        for (int i = 0; i < orderdetails.Count; i++)
                        {
                            //商品
                            var good = goodlist.Where(w => w.Id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                            if (good != null)
                            {
                                //商品加总库存
                                good.Stock += orderdetails[i].Count;
                                var GASDetailList = good.GASDetailList;
                                //订单明细中的规格属性，加规格属性库存
                                for (int j = 0; j < GASDetailList.Count; j++)
                                {
                                    if (GASDetailList[j].id == orderdetails[i].SpecIds)
                                    {
                                        GASDetailList[j].count += orderdetails[i].Count;
                                        good.AttrDetail = SerializeHelper.SerToJson(GASDetailList);
                                        break;
                                    }
                                }
                                //更改商品总库存和规格属性库存
                                tranModel.Add($"update FoodGoods set AttrDetail='{good.AttrDetail}',Inventory={good.Inventory} where Id={good.Id}");

                            }
                        }
                    }

                    ////订单状态改成已过期
                    //order.State = (int)C_Enums.miniAppFoodOrderState.已取消;
                    tranModel.Add($"update FoodGoodsOrder set State={order.State} where Id={order.Id} and State = {oldState} ");

                    //事务内某行sql执行受影响行数为0,会回滚整个事务

                    string key = string.Format(FoodGoodsBLL.foodGoodsKey, order.StoreId);
                    RedisUtil.Remove(key);//移除该店铺下的所有缓存的产品

                    return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                }
            }
            return false;
        }

        //原定为15分钟,这里改为1小时是为了避免同用户操作冲突
        //public void updateOrderStateForCancle(int timeoutlength = -16)
        public void updateOrderStateForCancle(int timeoutlength = -1)
        {
            //string sql = $" update  miniappfoodgoodsorder set state = {(int)C_Enums.miniAppFoodOrderState.已取消} where state = {(int)C_Enums.miniAppFoodOrderState.待付款} and  (NOW()-INTERVAL 15 minute) <= CreateDate  ";
            TransactionModel tranModel = new TransactionModel();
            
            
            
            

            //订单超过1小时取消订单
            //List<MiniappFoodGoodsOrder> orderList = GetList($"State={(int)C_Enums.miniAppFoodOrderState.待付款} and  CreateDate <= date_add(NOW(), interval {timeoutlength} MINUTE)", 1000, 1);
            List<FoodGoodsOrder> orderList = GetList($"State={(int)miniAppFoodOrderState.待付款} and BuyMode!={(int)miniAppBuyMode.线下支付} and  CreateDate <= (NOW()+INTERVAL {timeoutlength} HOUR)", 1000, 1);
            if (orderList != null && orderList.Count > 0)
            {
                tranModel = new TransactionModel();
                //订单明细
                List<FoodGoodsCart> orderdetaillist = orderList.Any() ? FoodGoodsCartBLL.SingleModel.GetList($"State = 1 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<FoodGoodsCart>();
                if (orderdetaillist != null && orderdetaillist.Count > 0)
                {
                    //商品
                    List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                    if (goodlist != null && goodlist.Count > 0)
                    {
                        foreach (var item in orderList)
                        {
                            //商品明细
                            var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.Id).ToList() : new List<FoodGoodsCart>();
                            if (orderdetails != null && orderdetails.Count > 0)
                            {
                                for (int i = 0; i < orderdetails.Count; i++)
                                {
                                    //商品
                                    var good = goodlist.Where(w => w.Id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                                    if (good != null)
                                    {
                                        //商品加总库存(剩余库存)
                                        good.Stock += orderdetails[i].Count;
                                        var GASDetailList = good.GASDetailList;
                                        //订单明细中的规格属性，加规格属性库存
                                        for (int j = 0; j < GASDetailList.Count; j++)
                                        {
                                            if (GASDetailList[j].id == orderdetails[i].SpecIds)
                                            {
                                                GASDetailList[j].count += orderdetails[i].Count;
                                                good.AttrDetail = SerializeHelper.SerToJson(GASDetailList);
                                                break;
                                            }
                                        }
                                        //更改商品总库存和规格属性库存
                                        tranModel.Add($"update FoodGoods set AttrDetail='{good.AttrDetail}',Inventory={good.Inventory} where Id={good.Id}");

                                    }
                                }
                            }

                            //订单状态改成已过期
                            item.State = (int)miniAppFoodOrderState.已取消;
                            tranModel.Add($"update FoodGoodsOrder set State={item.State} where Id={item.Id} and State = {(int)miniAppFoodOrderState.待付款} ;");

                            //事务内某行sql执行受影响行数为0,会回滚整个事务
                            if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                            {

                                FoodGoodsOrderLogBLL.SingleModel.AddLog(item.Id, "0", "系统自动将超过15分钟未付款的订单取消成功！");
                            }
                            else
                            {
                                FoodGoodsOrderLogBLL.SingleModel.AddLog(item.Id, "0", "系统自动将超过15分钟未付款的订单取消失败！");
                            }
                        }

                        string key = string.Format(FoodGoodsBLL.foodGoodsKey, goodlist[0].FoodId);
                        RedisUtil.Remove(key);//移除该店铺下的所有缓存的产品
                    }

                }
            }
        }


        //自动完成订单
        public void updateOrderStateForComplete(int timeoutlength = -120)
        {
            TransactionModel tranModel = new TransactionModel();
            
    

            List<int> updateByOldStateList = new List<int>();
            updateByOldStateList.Add((int)miniAppFoodOrderState.待就餐);
            updateByOldStateList.Add((int)miniAppFoodOrderState.待确认送达);

            List<FoodGoodsOrder> list = GetList($" State in ({string.Join(", ", updateByOldStateList)}) and  DistributeDate <= (NOW()+INTERVAL {timeoutlength} MINUTE )");
            string updateSql = $" update  FoodGoodsOrder set State = {(int)miniAppFoodOrderState.已完成},AcceptDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where  State in ({string.Join(",", updateByOldStateList)}) and  DistributeDate <= (NOW()+interval {timeoutlength} MINUTE) ";
            //var updateSql = $" update  FoodGoodsOrder set State = {(int)miniAppFoodOrderState.已完成},AcceptDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where  State in ({string.Join(",", updateByOldStateList)}) and  DistributeDate <= '{DateTime.Now.AddMinutes(timeoutlength).ToString("yyyy-MM-dd HH:mm:ss")}' ";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);
            
            
            string updateSalesCount = "";
            if (list != null && list.Count > 0)
            {
                string goodsIds = string.Join(",",list.Select(s=>s.Id));
                List<FoodGoodsCart> foodGoodsCartList = FoodGoodsCartBLL.SingleModel.GetListByOrderIds(goodsIds);
                list.ForEach(order =>
                {
                    FoodGoodsOrderLogBLL.SingleModel.AddLog(order.Id, "0", "(堂食接单后/外卖开始配送后) 超过2小时,系统自动完成订单");
                    //会员加消费金额
                    if (!VipRelationBLL.SingleModel.updatelevel(order.UserId, "food"))
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常" + order.Id));
                    }

                    //加销量
                    var list2 = foodGoodsCartList?.Where(w=>w.GoodsOrderId == order.Id).ToList();
                    list2?.Select(x => x.FoodGoodsId).Distinct().ToList().ForEach(x =>
                    {
                        var salesCount1 = list2.Where(y => y.FoodGoodsId == x).Sum(y => y.Count);

                        updateSalesCount += $" update FoodGoods set salesCount = salesCount + {salesCount1} where id = {x} ;";
                    });
                });
            }

            if (!string.IsNullOrWhiteSpace(updateSalesCount))
            {
                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSalesCount, null);
            }


        }

        public TransactionModel addSalesCount(int orderId, TransactionModel _tranModel)
        {
            List<FoodGoodsCart> orderDetailList = FoodGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderId}");
            if (orderDetailList != null && orderDetailList.Count > 0)
            {
                //记录订单支付日志
                List<FoodGoods> orderGoodsList = FoodGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", orderDetailList.Select(x => x.FoodGoodsId).Distinct())})");
                //加商品销量
                orderGoodsList.ForEach(x =>
                {
                    var shopQty = orderDetailList.Where(y => y.FoodGoodsId == x.Id).Sum(y => y.Count);

                    _tranModel.Add($" Update FoodGoods set salesCount = salesCount + {shopQty} where Id = {x.Id} ;");
                });
            }
            return _tranModel;
        }


        //异常单据(付款中) 2小时未恢复正常则将订单取消
        public void updateOrderStateForCancleByPay(int timeoutlength = -1)
        {
            //string sql = $" update  miniappfoodgoodsorder set state = {(int)C_Enums.miniAppFoodOrderState.已取消} where state = {(int)C_Enums.miniAppFoodOrderState.待付款} and  (NOW()-INTERVAL 15 minute) <= CreateDate  ";
            TransactionModel tranModel = new TransactionModel();
            
            
            
            

            //付款中订单超过120分钟取消订单
            //List<MiniappFoodGoodsOrder> orderList = GetList($"State={(int)C_Enums.miniAppFoodOrderState.待付款} and  CreateDate <= date_add(NOW(), interval {timeoutlength} MINUTE)", 1000, 1);
            List<FoodGoodsOrder> orderList = GetList($"State={(int)miniAppFoodOrderState.付款中} and  CreateDate <= (NOW()+INTERVAL {timeoutlength} HOUR)", 1000, 1);
            if (orderList != null && orderList.Count > 0)
            {
                tranModel = new TransactionModel();
                //订单明细
                List<FoodGoodsCart> orderdetaillist = orderList.Any() ? FoodGoodsCartBLL.SingleModel.GetList($"State = 1 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<FoodGoodsCart>();
                if (orderdetaillist != null && orderdetaillist.Count > 0)
                {
                    //商品
                    List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                    if (goodlist != null && goodlist.Count > 0)
                    {
                        foreach (var item in orderList)
                        {
                            //商品明细
                            var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.Id).ToList() : new List<FoodGoodsCart>();
                            if (orderdetails != null && orderdetails.Count > 0)
                            {
                                for (int i = 0; i < orderdetails.Count; i++)
                                {
                                    //商品
                                    var good = goodlist.Where(w => w.Id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                                    if (good != null)
                                    {
                                        //商品加总库存(剩余库存)
                                        good.Stock += orderdetails[i].Count;
                                        var GASDetailList = good.GASDetailList;
                                        //订单明细中的规格属性，加规格属性库存
                                        for (int j = 0; j < GASDetailList.Count; j++)
                                        {
                                            if (GASDetailList[j].id == orderdetails[i].SpecIds)
                                            {
                                                GASDetailList[j].count += orderdetails[i].Count;
                                                good.AttrDetail = SerializeHelper.SerToJson(GASDetailList);
                                                break;
                                            }
                                        }
                                        //更改商品总库存和规格属性库存
                                        tranModel.Add($"update FoodGoods set AttrDetail='{good.AttrDetail}',Inventory={good.Inventory} where Id={good.Id}");

                                    }
                                }
                            }

                            //订单状态改成已过期
                            item.State = (int)miniAppFoodOrderState.已取消;
                            tranModel.Add($"update FoodGoodsOrder set State={item.State} where Id={item.Id} and State = {(int)miniAppFoodOrderState.付款中} ;");

                            //事务内某行sql执行受影响行数为0,会回滚整个事务
                            if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                            {
                                FoodGoodsOrderLogBLL.SingleModel.AddLog(item.Id, "0", "系统自动将付款中超过120分钟未付款的订单取消成功！");
                            }
                            else
                            {
                                FoodGoodsOrderLogBLL.SingleModel.AddLog(item.Id, "0", "系统自动将付款中超过120分钟未付款的订单取消失败！");
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool outOrder(FoodGoodsOrder item, int oldState)
        {
            item.State = (int)miniAppFoodOrderState.退款中;
            item.outOrderDate = DateTime.Now;

            //重新加回库存
            if (updateStock(item, oldState))
            {
                try
                {
                    //微信退款只在金额大于0的时候去插入申请队列,小于0时直接将状态改为退款成功,并将状态回滚
                    if (item.BuyPrice > 0)
                    {
                        CityMorders order = new CityMordersBLL().GetModel(item.OrderId);
                        if (order == null) //找不到微信订单直接返回退款失败
                        {
                            item.State = (int)miniAppFoodOrderState.退款失败;
                            Update(item, "State,outOrderDate,Remark");
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
                        new ReFundQueueBLL().Add(reModel);
                    }
                    else
                    {
                        item.State = (int)miniAppFoodOrderState.已退款;
                    }

                    Update(item, "State,outOrderDate,Remark");
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
        /// 储值支付 退款
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool outOrderBySaveMoneyUser(FoodGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser, int oldState)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }

            //回退库存
            if (!updateStock(dbOrder, oldState))
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

        /// <summary>
        /// 跟进 退款状态 (退款是否成功)
        /// </summary>
        /// <returns></returns>
        public bool updateOutOrderState()
        {
            
            

            

            TransactionModel tranModel = new TransactionModel();
            List<FoodGoodsOrder> itemList = GetList($" State = {(int)miniAppFoodOrderState.退款中} and outOrderDate <= (NOW()-interval 17 second) and BuyMode = 1 ", 1000, 1) ?? new List<FoodGoodsOrder>();
            List<CityMorders> orderList = new List<CityMorders>();
            List<Entity.MiniApp.ReFundResult> outOrderList = new List<Entity.MiniApp.ReFundResult>();
            if (itemList.Any())
            {
                orderList = new CityMordersBLL().GetList($" Id in ({string.Join(",", itemList.Select(x => x.OrderId))}) ", 1000, 1) ?? new List<CityMorders>();
                if (orderList.Any())
                {
                    outOrderList = RefundResultBLL.SingleModel.GetList($" transaction_id in ('{string.Join("','", orderList.Select(x => x.trade_no))}') and retype = 1") ?? new List<ReFundResult>();
                    itemList.ForEach(x =>
                    {
                        CityMorders curOrder = orderList.FirstOrDefault(y => y.Id == x.OrderId);
                        if (curOrder?.Id > 0)
                        {
                            //退款是排程处理,故无法确定何时执行退款,而现阶段退款操作成败与否都会记录在系统内
                            bool isExec = outOrderList?.Any(y => y.transaction_id == curOrder.trade_no) ?? false; //是否已经跟请求过微信退款
                            if (isExec)
                            {
                                //是否退款成功
                                bool refundSuccess = outOrderList?.Any(y => y.transaction_id == curOrder.trade_no && y.result_code == "SUCCESS") ?? false;
                                if (refundSuccess)
                                {
                                    x.State = (int)miniAppFoodOrderState.已退款;
                                    tranModel.Add(BuildUpdateSql(x, "State"));
                                }
                                else
                                {
                                    x.State = (int)miniAppFoodOrderState.退款失败;
                                    tranModel.Add(BuildUpdateSql(x, "State"));
                                }
                            }
                        }
                    });
                }
            }
            var isSuccess = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            return isSuccess;
        }

        /// <summary>
        /// 根据模板类型返回填充模板内容(餐饮) 
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="sendMsgType"></param>
        /// <returns></returns>
        public object getTemplateMessageData(int OrderId, SendTemplateMessageTypeEnum sendMsgType)
        {
            var model = GetModel(OrderId) ?? new FoodGoodsOrder();
            var modelDtl = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {model.Id} ") ?? new List<FoodGoodsCart>();
            
            string modelDtlName = "";
            string goodsIds = string.Join(",",modelDtl.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);
            modelDtl.ForEach(x =>
            {
                var good = foodGoodsList?.FirstOrDefault(f=>f.Id == x.FoodGoodsId)?? new FoodGoods();
                x.goodsMsg = good;
                //modelDtlName += good.GoodsName + "+";
            });
            //modelDtlName = modelDtlName.Substring(0, modelDtlName.Length - 1);
            modelDtlName = string.Join("+", modelDtl.Select(x => x.goodsMsg.GoodsName));
            var food = FoodBLL.SingleModel.GetModel(model.StoreId) ?? new Food();


            object postData = new object();
            switch (sendMsgType)
            {
                case SendTemplateMessageTypeEnum.餐饮订单支付成功通知:
                    postData = new
                    {
                        keyword1 = new { value = model.OrderNum, color = "#000000" },
                        keyword2 = new { value = model.CreateDateStr, color = "#000000" },
                        keyword3 = new { value = modelDtlName, color = "#000000" },
                        keyword4 = new { value = model.PayDateStr, color = "#000000" },
                        keyword5 = new { value = Enum.GetName(typeof(miniAppBuyMode), model.BuyMode), color = "#000000" },
                        keyword6 = new { value = model.BuyPriceStr, color = "#000000" },
                        keyword7 = new { value = miniAppFoodOrderState.待接单.ToString(), color = "#000000" },
                    };
                    break;
                case SendTemplateMessageTypeEnum.餐饮订单配送通知:
                    postData = new
                    {
                        keyword1 = new { value = model.CreateDateStr, color = "#000000" },
                        keyword2 = new { value = food.FoodsName, color = "#000000" },
                        keyword3 = new { value = model.OrderNum, color = "#000000" },
                        keyword4 = new { value = model.Address, color = "#000000" },
                        keyword5 = new { value = model.DistributeDateStr, color = "#000000" },
                        keyword6 = new { value = modelDtlName, color = "#000000" },
                        keyword7 = new { value = Enum.GetName(typeof(miniAppFoodOrderState), model.State), color = "#000000" },
                    };
                    break;
                case SendTemplateMessageTypeEnum.餐饮退款申请通知:
                    postData = new
                    {
                        keyword1 = new { value = model.OrderNum, color = "#000000" },
                        keyword2 = new { value = modelDtlName, color = "#000000" },
                        keyword3 = new { value = model.BuyPriceStr, color = "#000000" },
                        keyword4 = new { value = model.AccepterTelePhone, color = "#000000" },
                        keyword5 = new { value = model.AccepterName, color = "#000000" },
                        keyword6 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword7 = new { value = string.Empty, color = "#000000" },
                    };
                    break;
                case SendTemplateMessageTypeEnum.餐饮退款成功通知:
                    postData = new
                    {
                        keyword1 = new { value = model.OrderNum, color = "#000000" },
                        keyword2 = new { value = modelDtlName, color = "#000000" },
                        keyword3 = new { value = model.outOrderDate.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword4 = new { value = model.BuyPriceStr, color = "#000000" },
                        //keyword5 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                        keyword5 = new { value = "1-7个工作日", color = "#000000" },
                    };
                    break;
                case SendTemplateMessageTypeEnum.餐饮订单拒绝通知:
                    postData = new
                    {
                        keyword1 = new { value = model.OrderNum, color = "#000000" },
                        keyword2 = new { value = food.FoodsName, color = "#000000" },
                        keyword3 = new { value = model.Remark, color = "#000000" },
                        keyword4 = new { value = modelDtlName, color = "#000000" },
                        keyword5 = new { value = model.BuyPriceStr, color = "#000000" },
                        keyword6 = new { value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), color = "#000000" },
                    };
                    break;
                default:

                    break;
            }
            return postData;
        }


        /// <summary>
        /// 餐饮打印订单
        /// </summary>
        /// <param name="food"></param>
        /// <param name="foodGoodsOrder"></param>
        /// <param name="cars"></param>
        /// <param name="foodPrintList"></param>
        /// <param name="account">加传参数,打单失败会通过提示该用户,若不想提示,可传null</param>
        /// <returns></returns>
        public bool PrintOrder(Food food, FoodGoodsOrder foodGoodsOrder, List<FoodGoodsCart> cars, List<FoodPrints> foodPrintList, Account account, bool ispay = true)
        {
            SystemUpdateMessage msg = null;
            
            XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小程序餐饮模板);

            int tId = xcxTemplate == null ? 0 : xcxTemplate.Id;
            if (food == null || foodGoodsOrder == null || cars == null || !cars.Any() || foodPrintList == null || !foodPrintList.Any())
            {
                #region 记录系统通知,以此通知用户
                if (foodPrintList?.Count > 0)
                {
                    if (account != null)
                    {
                        msg = new SystemUpdateMessage();
                        msg.AccountId = account.Id.ToString();
                        msg.Title = $"{food?.FoodsName} 打印机打单异常 <单号:{foodGoodsOrder?.OrderNum}>";
                        msg.PublishUser = "系统监测自动添入";
                        msg.State = 0;
                        msg.Type = 2;
                        msg.UpdateTime = msg.AddTime = DateTime.Now;
                        msg.Year = DateTime.Now.Year;
                        msg.Month = DateTime.Now.Month;
                        msg.Day = DateTime.Now.Day;
                        msg.IsRead = 0;
                        msg.Type = 2;
                        msg.TId = tId;

                        if (food == null)
                        {
                            msg.Content = "系统未匹配到店铺资料,导致打单失败,请检查店铺配置,重新编辑保存.";
                        }
                        if (foodGoodsOrder == null || cars == null || !cars.Any())
                        {
                            log4net.LogHelper.WriteError(GetType(), new Exception($"{food?.FoodsName} 打印机打单异常,单号 {foodGoodsOrder?.OrderNum} 打印失败,数据丢失 cars: {cars == null || !cars.Any()}"));
                            msg.Content = "系统未检测到订单资料,可能下单过程中数据丢失";
                        }

                        SystemUpdateMessageBLL.SingleModel.Add(msg);
                    }
                }
                #endregion

                log4net.LogHelper.WriteInfo(GetType(), $"参数为空导致无法打印:food:{food == null},foodGoodsOrder :{ foodGoodsOrder == null }, cars: {cars == null || !cars.Any()}, foodPrintList : {foodPrintList == null || !foodPrintList.Any()}");
                return false;
            }

            string goodsIds = string.Join(",",cars.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);
            cars.ForEach(car =>
            {
                if (car.goodsMsg == null)
                {
                    car.goodsMsg = foodGoodsList?.FirstOrDefault(f=>f.Id == car.FoodGoodsId);
                }
            });
            
            List<FoodPrints> totalPrints = foodPrintList.Where(p => p.printType == 0)?.ToList(); //总票打印机集合
            List<FoodPrints> singlePrintsByCount = foodPrintList.Where(p => p.printType == 1)?.ToList(); //单票打印机(按份数)集合
            List<FoodPrints> singlePrintsByGoods = foodPrintList.Where(p => p.printType == 2)?.ToList(); //单票打印机(按菜品)集合

            string totalPrintContent = string.Empty; //总票打印机打印内容
            List<string> singlePrintContentsByCount = new List<string>(); //单票打印机(按份数)打印内容
            List<string> singlePrintContentsByGoods = new List<string>(); //单票打印机(按菜品)打印内容

            //格式化商品
            Func<List<FoodGoodsCart>, string> formatGoods = (goods) =>
            {
                #region 格式化商品
                return string.Join("", goods.Select(item =>
                     string.Concat(new[] {
                        "<tr><td>.</td></tr>",
                        //"<tr><td>@@2................................</td></tr>",
                        !string.IsNullOrEmpty(item.SpecInfo) ?
                        $"<tr><td><FS><FB>{item.goodsMsg?.GoodsName}</FB>（{item.SpecInfo}）</FS></td></tr>" :
                        $"<tr><td><FS>{item.goodsMsg?.GoodsName}</FS></td></tr>",
                        $"<tr><td>件数：<FS>{item.Count}</FS>；共：<FB>￥</FB><FS>{(item.Price * item.Count * 0.01).ToString("0.00")}</FS></td></tr>",
                        //"<tr><td></td></tr>",
                        //"<tr><td></td></tr>",
                     })
                ));
                #endregion
            };

            //格式化订单
            Func<FoodGoodsOrder, string> formatOrder = (order) =>
            {
                #region 格式化订单
                return string.Concat(new[] {
                        order.CouponPrice > 0 ? //显示使用优惠券信息
                            $"<tr><td>优惠券优惠金额:</td><td>-</td><td><FB>￥</FB><FS>{(order.CouponPrice * 0.01).ToString("0.00")}</FS></td></tr>" :
                            null,
                        "<tr><td>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄</td></tr>",
                        $"<tr><td>实收:</td><td>-</td><td><FS><FB>￥</FB></FS><FS2>{(order.BuyPrice * 0.01).ToString("0.00")}</FS2></td></tr>",
                        $"<tr><td></td><td></td><td>({Enum.GetName(typeof(miniAppBuyMode), order.BuyMode)})</td></tr>", //支付方式
                    });
                #endregion
            };

            //格式化脚部
            Func<FoodGoodsOrder, string> formatFoot = (order) =>
            {
                #region 格式化脚部
                return string.Concat(new[] {
                        $"┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n",
                        $"订单号：{order.OrderNum}\r\n",
                        $"下单时间：{order.CreateDate}\r\n",
                        $"打印时间：{DateTime.Now}\r\n",
                        $"订单备注：<FS>{order.Message}</FS>\r\n",
                    });
                #endregion
            };

            //预约订单（不需要支付）
            bool isReservationOrder = foodGoodsOrder.OrderType == (int)miniAppFoodOrderType.预约;
            //预约打印
            bool isPrintReservation = food.funJoinModel.reservationPrint && isReservationOrder;
            //有效订单打印状态
            bool isPrintState = foodGoodsOrder.OrderType == (int)miniAppFoodOrderType.堂食 && foodGoodsOrder.State == (int)miniAppFoodOrderState.待就餐;
            //堂食线下支付打印订单
            bool isOfflinePay = !ispay;

            if (isPrintState || isPrintReservation || isOfflinePay)
            {
                #region 堂食 - 总票打印内容拼接

                string tableNo = foodGoodsOrder.TablesNo.ToString();

                if (!string.IsNullOrEmpty(foodGoodsOrder.attribute))
                {
                    foodGoodsOrder.attrbuteModel = JsonConvert.DeserializeObject<FoodGoodsOrderAttr>(foodGoodsOrder.attribute);
                }
                if (foodGoodsOrder.attrbuteModel.isNewTableNo && foodGoodsOrder.TablesNo > 0)
                {
                    tableNo = FoodTableBLL.SingleModel.GetModel(foodGoodsOrder.TablesNo)?.Scene;
                }
                if (isReservationOrder)
                {
                    tableNo = "预约到店";
                }

                //拼接订单内容排版 --堂食
                totalPrintContent = string.Concat(new[] {
                    $"<MC>0,00005,0</MC><FS>{food.FoodsName}(堂食)</FS><center>\r\n<FS>桌台：{tableNo}</FS></center>\r\n",
                    "<table>",
                        formatGoods(cars),
                        formatOrder(foodGoodsOrder),
                        isOfflinePay ?
                            $"<tr><td></td><td></td><td>线下支付：(未付款)</td></tr>" :
                            null,
                    "</table>",
                    formatFoot(foodGoodsOrder)
                });

                #endregion

                #region 堂食 - 单票(按份数)打印内容拼接
                //拼接订单内容排版 --堂食
                foreach (FoodGoodsCart car in cars)
                {
                    for (int countIndex = 1; countIndex <= car.Count; countIndex++)
                    {
                        singlePrintContentsByCount.Add(string.Concat(new[]
                        {
                            $"<MC>0,00005,0</MC><center><FS>桌台：{tableNo}</FS></center>\r\n",
                            "<table>",
                                formatGoods(new List<FoodGoodsCart> { car }),
                            $"<tr>单票：第{countIndex}份<td></td><td></td><td></td></tr>",
                            "</table>",
                            "┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n",
                            $"订单号：{foodGoodsOrder.OrderNum}\r\n",
                        }));
                    }
                }
                #endregion

                #region 堂食 - 单票(按菜品)打印内容拼接
                //拼接订单内容排版 --堂食
                foreach (FoodGoodsCart car in cars)
                {
                    singlePrintContentsByGoods.Add(string.Concat(new[] {
                        $"<MC>0,00005,0</MC><center><FS>桌台：{tableNo}</FS></center>\r\n",
                        "<table>",
                        formatGoods(new List<FoodGoodsCart>() { car }),
                        "</table>",
                        "┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n",
                        $"订单号：{foodGoodsOrder.OrderNum}\r\n",
                    }));
                }
                #endregion
            }
            else if (foodGoodsOrder.OrderType == (int)miniAppFoodOrderType.外卖 && foodGoodsOrder.State == (int)miniAppFoodOrderState.待送餐)
            {
                #region 外卖 - 总票打印内容拼接
                //拼接订单内容排版 --外卖
                totalPrintContent = string.Concat(new[] {
                    $"<MC>0,00005,0</MC><FS>{food.FoodsName}(外卖)</FS>\r\n",
                    "<table>",
                        formatGoods(cars),
                        $"<tr><td>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄</td></tr>",
                        $"<tr><td>配送费:</td><td>-</td><td>{(foodGoodsOrder.FreightPrice * 0.01).ToString("0.00")}</td></tr>",
                        $"<tr><td>餐盒费:</td><td>-</td><td>{(foodGoodsOrder.PackinPrice * 0.01).ToString("0.00")}</td></tr>",
                        formatOrder(foodGoodsOrder),
                    "</table>",
                    formatFoot(foodGoodsOrder),
                    $"收货人：<FS><FB>{foodGoodsOrder.AccepterName}</FB></FS>\r\n",
                    $"联系电话：<FS><FB>{foodGoodsOrder.AccepterTelePhone}</FB></FS>\r\n",
                    $"收货地址：<FS><FB>{foodGoodsOrder.Address}</FB></FS>\r\n",
                });
                #endregion

                #region 外卖 - 单票(按份数)打印内容拼接
                //拼接订单内容排版 --外卖
                foreach (FoodGoodsCart car in cars)
                {
                    for (int countIndex = 1; countIndex <= car.Count; countIndex++)
                    {
                        singlePrintContentsByCount.Add(string.Concat(new[] {
                            $"<MC>0,00005,0</MC><center><FS>外卖</FS></center>\r\n",
                            "<table>",
                                formatGoods(new List<FoodGoodsCart>() { car }),
                            $"<tr><td></td><td></td><td>单票：第{countIndex}份</td></tr>",
                            $"</table>",
                            "┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n",
                            $"订单号：{foodGoodsOrder.OrderNum}\r\n",
                        }));
                    }
                }
                #endregion

                #region 外卖 - 单票(按菜品)打印内容拼接
                //拼接订单内容排版 --堂食
                foreach (FoodGoodsCart car in cars)
                {
                    singlePrintContentsByGoods.Add(string.Concat(new[] {
                        "<MC>0,00005,0</MC><center><FS>外卖</FS></center>\r\n",
                        "<table>",
                            formatGoods(new List<FoodGoodsCart>() { car }),
                        "</table>",
                            "┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n",
                        $"订单号：{foodGoodsOrder.OrderNum}\r\n",
                    }));
                }
                #endregion
            }

            //打印总票内容
            if (totalPrints != null && totalPrints.Any() && !string.IsNullOrWhiteSpace(totalPrintContent))
            {
                #region 记录打印机 打印日志
                totalPrints.ForEach(print =>
                {
                    string returnMsg = FoodYiLianYunPrintHelper.printContent(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, totalPrintContent);
                    FoodYlyReturnModel returnModel = SerializeHelper.DesFromJson<FoodYlyReturnModel>(returnMsg);
                    //记录订单打印日志
                    string remark = string.Empty;
                    if (returnModel.state == 2)
                    {
                        remark = "提交时间超时";
                        returnModel.state = -1;
                    }
                    else if (returnModel.state == 3)
                    {
                        remark = "参数有误";
                        returnModel.state = -1;
                    }
                    else if (returnModel.state == 4)
                    {
                        remark = "sign加密验证失败";
                        returnModel.state = -1;
                    }
                    else if (returnModel.state == 1)
                    {
                        remark = "发送成功";
                        returnModel.state = 0;
                    }

                    if (returnModel.state != 1 && returnModel.state != 0)
                    {
                        if (account != null)//打单异常加入系统日志
                        {
                            msg = new SystemUpdateMessage();
                            msg.AccountId = account.Id.ToString();
                            msg.Title = $"{food?.FoodsName} 打印机 [{print.Name}] 打单异常 <单号:{foodGoodsOrder?.OrderNum}>";
                            msg.PublishUser = "系统监测自动添入";
                            msg.State = 0;
                            msg.UpdateTime = msg.AddTime = DateTime.Now;
                            msg.Year = DateTime.Now.Year;
                            msg.Month = DateTime.Now.Month;
                            msg.Day = DateTime.Now.Day;
                            msg.Type = 2;
                            msg.IsRead = 0;
                            msg.TId = tId;

                            msg.Content = $"与打印机官方通讯出现问题,在打印机官方服务器恢复前无法打印任何单据.打印机官方返回信息:{remark}";

                            SystemUpdateMessageBLL.SingleModel.Add(msg);
                        }
                    }

                    var log = new FoodOrderPrintLog()
                    {
                        Dataid = returnModel.id,
                        addtime = DateTime.Now,
                        machine_code = print.PrintNo,
                        state = returnModel.state,
                        isupdate = 0,
                        remark = remark,
                        orderId = foodGoodsOrder.Id,
                        printsId = print.Id
                    };
                    FoodOrderPrintLogBLL.SingleModel.Add(log);
                });
                #endregion
            }

            //打印单票内容(按份数)
            if (singlePrintsByCount != null && singlePrintsByCount.Any()
                    && singlePrintContentsByCount?.Count > 0)
            {
                singlePrintsByCount.ForEach(print =>
                {
                    singlePrintContentsByCount.ForEach(content =>
                    {

                        string returnMsg = FoodYiLianYunPrintHelper.printContent(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, content);
                        FoodYlyReturnModel returnModel = SerializeHelper.DesFromJson<FoodYlyReturnModel>(returnMsg);

                        #region 记录打印机 打印日志
                        //记录订单打印日志
                        string remark = string.Empty;
                        if (returnModel.state == 2)
                        {
                            remark = "提交时间超时";
                            returnModel.state = -1;
                        }
                        else if (returnModel.state == 3)
                        {
                            remark = "参数有误";
                            returnModel.state = -1;
                        }
                        else if (returnModel.state == 4)
                        {
                            remark = "sign加密验证失败";
                            returnModel.state = -1;
                        }
                        else if (returnModel.state == 1)
                        {
                            remark = "发送成功";
                            returnModel.state = 0;
                        }
                        if (returnModel.state != 1 && returnModel.state != 0)//运行异常加入系统日志
                        {
                            if (account != null)
                            {
                                msg = new SystemUpdateMessage();
                                msg.AccountId = account.Id.ToString();
                                msg.Title = $"{food?.FoodsName} 打印机 [{print.Name}] 打单异常 <单号:{foodGoodsOrder?.OrderNum}>";
                                msg.PublishUser = "系统监测自动添入";
                                msg.State = 0;
                                msg.Type = 2;
                                msg.UpdateTime = msg.AddTime = DateTime.Now;
                                msg.Year = DateTime.Now.Year;
                                msg.Month = DateTime.Now.Month;
                                msg.Day = DateTime.Now.Day;
                                msg.IsRead = 0;
                                msg.TId = tId;

                                msg.Content = $"与打印机官方通讯出现问题,恢复前无法打印单据,故障信息:{remark};";

                                SystemUpdateMessageBLL.SingleModel.Add(msg);
                            }
                        }
                        var log = new FoodOrderPrintLog()
                        {
                            Dataid = returnModel.id,
                            addtime = DateTime.Now,
                            machine_code = print.PrintNo,
                            state = returnModel.state,
                            isupdate = 0,
                            remark = remark,
                            orderId = foodGoodsOrder.Id,
                            printsId = print.Id
                        };
                        FoodOrderPrintLogBLL.SingleModel.Add(log);
                        #endregion
                    });
                });
            }

            //打印单票内容(按菜品)
            if (singlePrintsByGoods != null && singlePrintsByGoods.Any()
                    && singlePrintContentsByGoods?.Count > 0)
            {
                singlePrintsByGoods.ForEach(print =>
                {
                    singlePrintContentsByGoods.ForEach(content =>
                    {

                        string returnMsg = FoodYiLianYunPrintHelper.printContent(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, content);
                        FoodYlyReturnModel returnModel = SerializeHelper.DesFromJson<FoodYlyReturnModel>(returnMsg);

                        #region 记录打印机 打印日志
                        //记录订单打印日志
                        string remark = string.Empty;
                        if (returnModel.state == 2)
                        {
                            remark = "提交时间超时";
                            returnModel.state = -1;
                        }
                        else if (returnModel.state == 3)
                        {
                            remark = "参数有误";
                            returnModel.state = -1;
                        }
                        else if (returnModel.state == 4)
                        {
                            remark = "sign加密验证失败";
                            returnModel.state = -1;
                        }
                        else if (returnModel.state == 1)
                        {
                            remark = "发送成功";
                            returnModel.state = 0;
                        }
                        if (returnModel.state != 1 && returnModel.state != 0)//运行异常加入系统日志
                        {
                            if (account != null)
                            {
                                msg = new SystemUpdateMessage();
                                msg.AccountId = account.Id.ToString();
                                msg.Title = $"{food?.FoodsName} 打印机 [{print.Name}] 打单异常 <单号:{foodGoodsOrder?.OrderNum}>";
                                msg.PublishUser = "系统监测自动添入";
                                msg.State = 0;
                                msg.Type = 2;
                                msg.UpdateTime = msg.AddTime = DateTime.Now;
                                msg.Year = DateTime.Now.Year;
                                msg.Month = DateTime.Now.Month;
                                msg.Day = DateTime.Now.Day;
                                msg.IsRead = 0;
                                msg.TId = tId;

                                msg.Content = $"与打印机官方通讯出现问题,恢复前无法打印单据,故障信息:{remark};";

                                SystemUpdateMessageBLL.SingleModel.Add(msg);
                            }
                        }
                        var log = new FoodOrderPrintLog()
                        {
                            Dataid = returnModel.id,
                            addtime = DateTime.Now,
                            machine_code = print.PrintNo,
                            state = returnModel.state,
                            isupdate = 0,
                            remark = remark,
                            orderId = foodGoodsOrder.Id,
                            printsId = print.Id
                        };
                        FoodOrderPrintLogBLL.SingleModel.Add(log);
                        #endregion
                    });
                });
            }
            return true;
        }



        /// <summary>
        /// 支付后处理
        /// </summary>
        /// <param name="foodGoodsOrder"></param>
        public void AfterPaySuccesExecFun(FoodGoodsOrder foodGoodsOrder)
        {

            if (foodGoodsOrder == null)
            {
                return;
            }
            
            
            
            List<FoodGoodsCart> carlist = new List<FoodGoodsCart>();

            Food food = FoodBLL.SingleModel.GetModel(foodGoodsOrder.StoreId);
            XcxAppAccountRelation xappr = XcxAppAccountRelationBLL.SingleModel.GetModel(food.appId);
            Account account = AccountBLL.SingleModel.GetModel(xappr.AccountId);
            List<FoodPrints> foodPrints = FoodPrintsBLL.SingleModel.GetList($" appId = {xappr.Id} and foodstoreid = {food.Id} and state >= 0 ") ?? new List<FoodPrints>();
            carlist = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={foodGoodsOrder.Id} and state=1");

            string goodsIds = string.Join(",",carlist?.Select(s=>s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);

            carlist?.ForEach(x =>
            {
                x.goodsMsg = foodGoodsList?.FirstOrDefault(f=>f.Id == x.FoodGoodsId);
            });


            //打单
            PrintOrder(food, foodGoodsOrder, carlist, foodPrints, account);

            //发送餐饮支付成功通知给用户
            object orderData = TemplateMsg_Miniapp.FoodGetTemplateMessageData(food, foodGoodsOrder, SendTemplateMessageTypeEnum.餐饮订单支付成功通知);
            TemplateMsg_Miniapp.SendTemplateMessage(foodGoodsOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单支付成功通知, TmpType.小程序餐饮模板, orderData);

            //发送餐饮支付成功通知给商家
            TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(foodGoodsOrder, account, food);
        }

        /// <summary>
        /// 暂不支付订单生成成功后的操作
        /// </summary>
        /// <param name="foodGoodsOrder"></param>
        public void AfterUnPaySuccesExecFun(FoodGoodsOrder foodGoodsOrder)
        {
            Food food = FoodBLL.SingleModel.GetModel(foodGoodsOrder.StoreId);
            XcxAppAccountRelation xappr = XcxAppAccountRelationBLL.SingleModel.GetModel(food.appId);
            Account account = AccountBLL.SingleModel.GetModel(xappr.AccountId);
            List<FoodPrints> foodPrints = FoodPrintsBLL.SingleModel.GetList($" appId = {xappr.Id} and foodstoreid = {food.Id} and state >= 0 ") ?? new List<FoodPrints>();
            List<FoodGoodsCart> carlist = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={foodGoodsOrder.Id} and state=1");
            
            string goodsIds = string.Join(",", carlist?.Select(s => s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);
            
            carlist?.ForEach(x =>
            {
                x.goodsMsg = foodGoodsList?.FirstOrDefault(f => f.Id == x.FoodGoodsId);
            });

            //打单
            PrintOrder(food, foodGoodsOrder, carlist, foodPrints, account, false);

        }

        #region 餐饮接口下单操作
        public string CommandFoodOrder(string jsondata, int rid, int userid, int paytype, ref object orderobj, ref Coupons reductionCart, ref StringBuilder sbUpdateGoodCartSql, ref CityMorders corder)
        {
            string dugmsg = "";
            

 
            FoodOrderPostModel postdata = JsonConvert.DeserializeObject<FoodOrderPostModel>(jsondata);
            if (postdata == null || postdata.FoodModel == null)
            {
                return "餐饮订单请求数据不能为空";
            }

            List<FoodGoodsCart> goodsCar = new List<FoodGoodsCart>();
            FoodGoodsOrder forder = new FoodGoodsOrder();
            forder = postdata.FoodModel;
            forder.GoodType = postdata.goodtype;
            int foodOrderType = 0;
            //数据基础验证
            dugmsg = CheckFoodData(rid, userid, postdata.goodCarIdStr, ref foodOrderType, ref forder, ref goodsCar);
            if (dugmsg != null && dugmsg.Length > 0)
            {
                return dugmsg;
            }

            //拼团
            int grouperprice = 0;//团长减价
            string groupmsg = "";
            EntGroupsRelation groupmodel = new EntGroupsRelation() { RId = rid };
            if (postdata.isgroup > 0 || postdata.groupid > 0)
            {
                forder.GoodType = postdata.goodtype;
                //判断是否是拼团，如果是拼团则将产品价格改成拼团价，获取团长优惠价
                groupmsg = EntGroupsRelationBLL.SingleModel.CommandEntGroup(postdata.isgroup, postdata.groupid, userid, 0, goodsCar[0].FoodGoodsId, ref grouperprice, ref groupmodel, (int)TmpType.小程序餐饮模板, goodsCar[0].Count);
                if (!string.IsNullOrEmpty(groupmsg))
                {
                    return groupmsg;
                }
            }

            dugmsg = CheckFoodCar(goodsCar, postdata.FoodModel.OrderType);
            if (dugmsg != null && dugmsg.Length > 0)
            {
                return dugmsg;
            }
            dugmsg = CommandFoodData(goodsCar, postdata.distributionprice, paytype, userid, rid, postdata.couponlogid, postdata.FoodModel.OrderType, postdata.FoodModel.GetWay, grouperprice, ref forder, ref sbUpdateGoodCartSql);
            if (dugmsg != null && dugmsg.Length > 0)
            {
                return dugmsg;
            }
            forder.PayDate = DateTime.Now;//支付时间
            forder.State = (int)miniAppFoodOrderState.待付款;

            ////自动接单则跳过待接单状态  --应该放在支付之后执行
            //if (forder.AutoAcceptOrder == 1)
            //{
            //    forder.State = (forder.OrderType == (int)miniAppFoodOrderType.堂食 ? (int)miniAppFoodOrderState.待就餐 : (int)miniAppFoodOrderState.待送餐);
            //    forder.ConfDate = DateTime.Now;//接单时间
            //    if (forder.State == (int)miniAppFoodOrderState.待就餐)
            //    {
            //        forder.DistributeDate = DateTime.Now;
            //    }
            //}

            //生成订单
            if (!addGoodsOrder(ref forder, goodsCar, userid, sbUpdateGoodCartSql, ref dugmsg))
            {
                return "餐饮订单生成失败!";
            }

            //更新预约状态
            if (forder.OrderType == (int)miniAppFoodOrderType.预约 && forder.ReserveId > 0)
            {
                
                FoodReservation reservation = FoodReservationBLL.SingleModel.GetModel(forder.ReserveId);
                string updateReservationSql = FoodReservationBLL.SingleModel.UpdateToPay(reservation: reservation, order: forder, cartItem: goodsCar);
                if (!string.IsNullOrWhiteSpace(updateReservationSql))
                {
                    TransactionModel tran = new TransactionModel();
                    tran.Add(updateReservationSql);
                    FoodReservationBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                }
            }

            #region 对接物流平台
            dugmsg = DistributionApiConfigBLL.SingleModel.AddDistributionOrder(rid, userid, postdata.cityname, postdata.lat, postdata.lnt, postdata.FoodModel.GetWay, forder, goodsCar, (int)TmpType.小程序餐饮模板, forder.StoreId,postdata.distributionprice);
            if (!string.IsNullOrEmpty(dugmsg))
            {
                return dugmsg;
            }
            #endregion

            #region 优惠券
            //若使用了优惠券将优惠券标记为已使用
            if (postdata.couponlogid > 0)
            {
                CouponLogBLL.SingleModel.Update(new CouponLog()
                {
                    Id = postdata.couponlogid,
                    State = 1,
                    OrderId = forder.Id
                }, "state,orderid");
            }
            #endregion

            #region (不参与当前生成逻辑)查找可领取的立减金  --作用于前端判定订单是否要跳转到立减金领取页面
            //【获取立减金，没有的话就传null】
            reductionCart = CouponsBLL.SingleModel.GetOpenedModel(rid);
            if (reductionCart != null)
            {
                int count = CouponLogBLL.SingleModel.GetCountById(reductionCart.Id);//已领取的份数
                reductionCart.RemNum = reductionCart.CreateNum - count;
            }
            #endregion

            #region 是否开团
            int groupid = postdata.groupid;
            groupmsg = EntGroupSponsorBLL.SingleModel.OpenGroup(postdata.isgroup, rid, paytype, userid, groupmodel, forder.BuyPrice, (int)TmpType.小程序餐饮模板, ref groupid);
            if (!string.IsNullOrEmpty(groupmsg))
            {
                return groupmsg;
            }
            if (groupid > 0)
            {
                forder.GroupId = groupid;
                base.Update(forder, "groupid");
            }
            #endregion

            corder.payment_free = forder.BuyPrice;
            corder.MinisnsId = forder.StoreId;//商家ID
            corder.TuserId = forder.Id;//订单的ID
            corder.ShowNote = $"餐饮版点餐支付{forder.BuyPrice * 0.01}元";
            corder.buy_num = forder.QtyCount; //无
            orderobj = forder;
            return "";
        }
        public object PayOrder(object orderobj, CityMorders order, ref int orderid)
        {
            FoodGoodsOrder forder = (FoodGoodsOrder)orderobj;
            //为0不需进入生成微信预支付订单的流程（免费订单）
            if (forder.BuyPrice == 0)
            {
                PayResult payresult = new PayResult();
                orderid = 1;
                new CityMordersBLL(payresult, order).MiniappFoodGoods(0, forder);
                return new { orderid = forder.Id, groupid = forder.GroupId, dbOrder = forder.Id };
            }
            else //生成微信预支付订单
            {
                #region CtiyModer 生成
                forder.OrderId = Convert.ToInt32(new CityMordersBLL().Add(order));
                orderid = forder.OrderId;
                base.Update(forder, "OrderId");
                return new { orderid = forder.OrderId, groupid = forder.GroupId, dbOrder = forder.Id };
                #endregion
            }
        }
        public object PayOrderByChuzhi(object orderobj, CityMorders order, int aid, SaveMoneySetUser saveMoneyUser, StringBuilder sbUpdateGoodCartSql, Coupons reductionCart, ref int orderid)
        {
            FoodGoodsOrder forder = (FoodGoodsOrder)orderobj;
            //储值支付 扣除预存款金额并生成消费记录
            if (payOrderBySaveMoneyUser(forder, saveMoneyUser, sbUpdateGoodCartSql))
            {
                //新订单电脑语音提示
                Utils.RemoveIsHaveNewOrder(aid);
                orderid = 1;
                return new { postdata = forder.OrderNum, groupid = forder.GroupId, orderid = 0, dbOrder = forder.Id, reductionCart = reductionCart };
            }
            return "";
        }
        public string CheckFoodData(int rid, int userid, string goodCarIdStr, ref int orderType, ref FoodGoodsOrder order, ref List<FoodGoodsCart> goodsCar)
        {
            
            #region  数据基础验证
            if (order == null)
            {
                return "订单不能为空";
            }

            if (string.IsNullOrEmpty(goodCarIdStr))
            {
                return "购物车异常";
            }
            int int_TryParseId = 0;
            //所有要下单的购物车记录
            List<string> goodCarIds = goodCarIdStr?.Split(',')?.Where(c => !string.IsNullOrWhiteSpace(c) && Int32.TryParse(c, out int_TryParseId))?.ToList();
            if (goodCarIds == null || !goodCarIds.Any())
            {
                return "您选择的购物车记录没有传达到购物中心";
            }

            goodsCar = FoodGoodsCartBLL.SingleModel.GetListByIds(string.Join(",", goodCarIds), userid, 0);
            if (goodsCar == null || goodsCar.Count <= 0)
            {
                return "找不到购物车记录";
            }

            orderType = order.TablesNo > 0 || order.TablesNo == -1 ? (int)miniAppFoodOrderType.堂食 : (int)miniAppFoodOrderType.外卖;

            #endregion

            return "";
        }

        public string CommandFoodData(List<FoodGoodsCart> goodsCar, int distributionprice, int buyMode, int userid, int rid, int couponLogId, int orderType, int getWay, int grouperprice, ref FoodGoodsOrder order, ref StringBuilder sbUpdateGoodCartSql)
        {
            Random random_Food = new Random(new Guid().GetHashCode());

            
            
            
            
            
            //通过缓存获取门店信息
            Food store = FoodBLL.SingleModel.GetModelByAppId(rid);
            if (store == null)
            {
                return "找不到店铺";
            }
            if (store.openState != 1)
            {
                return "商家未营业";
            }

            #region 订单数据的处理及价格计算
            order.AutoAcceptOrder = store.AutoAcceptOrder;
            int beforeDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠前商品总价
            int afterDiscountPrice = beforeDiscountPrice;//优惠后商品总价(默认没有优惠)
            #region 会员打折,计算  beforeDiscountPrice & afterDiscountPrice 

            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userid);
            if (vipInfo != null)
            {
                VipLevel levelinfo = VipLevelBLL.SingleModel.GetModelById(vipInfo.levelid);

                VipLevelBLL.SingleModel.CalculateVipGoodsCartPrice(goodsCar, levelinfo);
                afterDiscountPrice = goodsCar.Sum(x => x.Price * x.Count);//优惠后商品总价

                if (levelinfo != null)
                {
                    sbUpdateGoodCartSql = new StringBuilder();
                    foreach (FoodGoodsCart item in goodsCar)
                    {
                        sbUpdateGoodCartSql.Append(FoodGoodsCartBLL.SingleModel.BuildUpdateSql(item, "Price,originalPrice") + ";");
                    }
                }
            }


            //满减规则优惠金额
            if (store.funJoinModel.discountRuleSwitch)//若开启了满减优惠规则
            {
                int maxDiscountMoney = DiscountRuleBLL.SingleModel.getMaxDiscountMoney(afterDiscountPrice, rid);
                afterDiscountPrice -= maxDiscountMoney;
            }

            //首单立减优惠
            int firstOrderDiscountMoney = DiscountRuleBLL.SingleModel.getFirstOrderDiscountMoney(userid, rid, 0, TmpType.小程序多门店模板);
            afterDiscountPrice -= firstOrderDiscountMoney;


            #region  金额计算

            //优惠券优惠金额
            int couponsum = 0;
            if (couponLogId > 0)
            {
                List<CouponLog> couponlist = CouponLogBLL.SingleModel.GetCouponList(0, userid, store.Id, rid, 1, 1, "l.addtime desc",
                    string.Join(",", goodsCar.Select(e => e.FoodGoodsId).Distinct()),
                    JsonConvert.SerializeObject(goodsCar.Select(e => new
                    {
                        goodid = e.FoodGoodsId,
                        totalprice = e.Count * e.Price
                    })), couponLogId);
                //满足优惠券使用条件才去计算优惠券优惠金额
                if (couponlist?.Count >= 1 && couponlist[0].CanUse)
                {
                    string couponmsg = "";
                    //优惠金额
                    couponsum = CouponLogBLL.SingleModel.GetCouponPrice(couponLogId, goodsCar, ref couponmsg);
                    if (!string.IsNullOrEmpty(couponmsg))
                    {
                        return couponmsg;
                    }
                    afterDiscountPrice -= couponsum;
                }
                else
                {
                    return $" 优惠券失效 或 当前订单未满足优惠券使用条件！";
                }
            }
            order.CouponPrice = couponsum;
            #endregion

            #endregion

            switch (orderType)
            {
                case (int)miniAppFoodOrderType.外卖:
                    if (store.TakeOut != 1)
                    {
                        return "商家未开启外卖服务";
                    }
                    if (beforeDiscountPrice < store.OutSide && getWay == (int)miniAppOrderGetWay.商家配送)
                    {
                        return $"还差{((store.OutSide - afterDiscountPrice) * 0.01).ToString("0.00")}元起送,无法提交订单";
                    }

                    FoodAddress address = FoodAddressBLL.SingleModel.GetModel(order.AddressId);
                    if (address == null)
                    {
                        return "收货地址信息错误";
                    }

                    double distance = GetDistance(store.Lat, store.Lng, address.Lat, address.Lng);
                    if (distance > store.DeliveryRange)
                    {
                        return "超出配送范围,请更换收货地址";
                    }

                    //地址信息录入
                    order.AccepterName = address.NickName;
                    order.AccepterTelePhone = address.TelePhone;
                    order.ZipCode = address.ZipCode;
                    order.Address = $"{address.Address}";
                    //order.DistributionType = distributiontype;//物流接口配送
                    break;
                case (int)miniAppFoodOrderType.堂食:
                    if (store.TheShop != 1)
                    {
                        return "商家未开启堂食服务";
                    }
                    FoodTable table = FoodTableBLL.SingleModel.GetModelByScene(store.Id, order.TablesNo.ToString());
                    if (order.TablesNo != -1 && (table == null || table.Id <= 0))
                    {
                        return "桌台号错误";
                    }
                    break;
                default:
                    return "未知的配送方式";
            }
            
            order.VerificationNum = GetVerificationNum();//提货码
            order.OrderType = orderType;
            order.GetWay = orderType == (int)miniAppFoodOrderType.外卖 ? getWay : (int)miniAppOrderGetWay.到店自取;
            order.StoreId = store.Id;
            order.UserId = userid;
            order.CreateDate = DateTime.Now;
            order.State = (int)miniAppFoodOrderState.待付款;
            order.QtyCount = goodsCar.Sum(x => x.Count);
            order.ReducedPrice = beforeDiscountPrice - afterDiscountPrice;
            order.ReducedPrice += grouperprice;//加上团长优惠价格
            order.BuyMode = buyMode;
            order.FreightPrice = getWay > 1 ? distributionprice : (order.TablesNo > 0 || order.TablesNo == -1 ? 0 : store.ShippingFee); //堂食免运费
            order.BuyPrice = afterDiscountPrice + order.FreightPrice;
            order.BuyPrice -= grouperprice;//减去团长优惠价格
            order.BuyPrice = order.BuyPrice <= 0 ? 0 : order.BuyPrice;

            //对外订单号生成
            order.OrderNum = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}{random_Food.Next(999).ToString().PadLeft(3, '0')}";
            #endregion

            return "";
        }

        public string CheckFoodCar(List<FoodGoodsCart> goodsCar, int orderType)
        {

            #region  购物车商品是否可下单 - 验证
            string goodsIds = string.Join(",", goodsCar?.Select(s => s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);

            //失效菜品 & 异常菜品
            foreach (FoodGoodsCart car in goodsCar)
            {
                FoodGoods good = foodGoodsList?.FirstOrDefault(f=>f.Id == car.FoodGoodsId);
                if (good == null)
                {
                    return "菜品信息错误,请重新选择购买菜品！";
                }
                if (car.GoodsState > 0)
                {
                    return $"菜品 '{good.GoodsName}' 已经下架或被删除,请重新选择购买菜品！ ";
                }
                switch (orderType)
                {
                    case (int)miniAppFoodOrderType.外卖:
                        if (good.openTakeOut != 1)
                        {
                            return $"菜品'{good.GoodsName}' 未开启外卖送餐,请重新选择其他菜品！";
                        }
                        break;
                    case (int)miniAppFoodOrderType.堂食:
                        if (good.openTheShop != 1)
                        {
                            return $"菜品'{good.GoodsName}' 未开启堂食,请重新选择其他菜品！";
                        }
                        break;
                }

                //判定当前商品数量是否足够此次下单
                int curGoodQty = FoodGoodsBLL.SingleModel.GetGoodQty(good, car.SpecIds);
                int count = goodsCar.Where(y => y.FoodGoodsId == car.FoodGoodsId && y.SpecIds == car.SpecIds).Sum(y => y.Count);
                if (curGoodQty < count)
                {
                    return $"菜品: {good.GoodsName} {(!string.IsNullOrWhiteSpace(car.SpecInfo) ? "规格:" + car.SpecInfo : "")} 库存不足!";
                }
            }
            #endregion

            return "";
        }

        /// <summary>
        /// 腾讯地图,返回距离(公里) 
        /// 有误差,同腾讯地图api 8公里 误差在0.1米以内
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double EARTH_RADIUS = 6378.137;
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        private double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        /// <summary>
        /// 储值支付 扣除预存款金额并生成消费记录(电商)
        /// </summary>
        /// <param name="dbOrder"></param>
        /// <param name="saveMoneyUser"></param>
        /// <returns></returns>
        public bool payOrderBySaveMoneyUser(FoodGoodsOrder dbOrder, SaveMoneySetUser saveMoneyUser, StringBuilder updateCarPriceSql)
        {
            if (saveMoneyUser == null || saveMoneyUser.Id <= 0)
            {
                return false;
            }
            if (saveMoneyUser.AccountMoney < dbOrder.BuyPrice)
            {
                return false;
            }

            MySqlParameter[] _pone = null;
            TransactionModel _tranModel = new TransactionModel();
            if (updateCarPriceSql != null)
            {
                _tranModel.Add(updateCarPriceSql.ToString());
            }

            if (dbOrder != null)
            {
                Food foodStore = FoodBLL.SingleModel.GetModel(dbOrder.StoreId);
                if (!foodStore.funJoinModel.canSaveMoneyFunction)
                {
                    return false;
                }

                dbOrder.PayDate = DateTime.Now;//支付时间
                dbOrder.State = (int)miniAppFoodOrderState.待接单;

                //自动接单则跳过待接单状态
                if (foodStore != null && foodStore.AutoAcceptOrder == 1)
                {
                    dbOrder.State = (dbOrder.OrderType == (int)miniAppFoodOrderType.堂食 ? (int)miniAppFoodOrderState.待就餐 : (int)miniAppFoodOrderState.待送餐);
                    dbOrder.ConfDate = DateTime.Now;//接单时间
                    if (dbOrder.State == (int)miniAppFoodOrderState.待就餐)
                    {
                        dbOrder.DistributeDate = DateTime.Now;
                    }
                }

                //第三方配送
                TransactionModel temptran = new TransactionModel();
                string dadamsg = DistributionApiConfigBLL.SingleModel.UpdatePeiSongOrder(dbOrder.Id, foodStore==null?0:foodStore.appId, (int)TmpType.小程序餐饮模板, dbOrder.GetWay, ref temptran, false);
                if (!string.IsNullOrEmpty(dadamsg))
                {
                    LogHelper.WriteInfo(this.GetType(), dadamsg);
                }
                
                //修改拼团状态
                TransactionModel tran = new TransactionModel();
                EntGroupSponsorBLL.SingleModel.PayReturnUpdateGroupState(dbOrder.GroupId, foodStore.appId, ref tran, 0, (int)TmpType.小程序餐饮模板);
                if (tran.sqlArray != null && tran.sqlArray.Length > 0)
                {
                    _tranModel.Add(tran.sqlArray);
                }

                dbOrder.GoodsGuid = Guid.NewGuid().ToString().Replace("-", "");//此栏位暂无用处

                string sql = $"State,GoodsGuid,PayDate,ConfDate{(dbOrder.State == (int)miniAppFoodOrderState.待就餐 ? ",DistributeDate" : "")}";
                _tranModel.Add(base.BuildUpdateSql(dbOrder, sql, out _pone), _pone);

                //添加订单日志
                FoodGoodsOrderLog orderLog = new FoodGoodsOrderLog()
                {
                    GoodsOrderId = dbOrder.Id,
                    UserId = dbOrder.UserId.ToString(),
                    LogInfo = $" 订单成功支付(储值支付)：{dbOrder.BuyPrice * 0.01} 元 ",
                    CreateDate = DateTime.Now
                };
                _tranModel.Add(FoodGoodsOrderLogBLL.SingleModel.BuildAddSql(orderLog));
            }
            //添加储值日志
            SaveMoneySetUserLog userLog = new SaveMoneySetUserLog()
            {
                AppId = saveMoneyUser.AppId,
                UserId = dbOrder.UserId,
                MoneySetUserId = saveMoneyUser.Id,
                Type = -1,
                BeforeMoney = saveMoneyUser.AccountMoney,
                AfterMoney = saveMoneyUser.AccountMoney - dbOrder.BuyPrice,
                ChangeMoney = dbOrder.BuyPrice,
                ChangeNote = $" 购买商品,订单号:{dbOrder.OrderNum} ",
                CreateDate = DateTime.Now,
                State = 1
            };
            _tranModel.Add(SaveMoneySetUserLogBLL.SingleModel.BuildAddSql(userLog));

            //储值扣费
            _tranModel.Add($" update SaveMoneySetUser set AccountMoney = AccountMoney - {dbOrder.BuyPrice} where id =  {saveMoneyUser.Id} ; ");

            //执行sql
            bool isSuccess = base.ExecuteTransaction(_tranModel.sqlArray, _tranModel.ParameterArray);

            //操作成功
            if (isSuccess)
            {
                AfterPaySuccesExec(dbOrder);
            }

            return isSuccess;
        }

        /// <summary>
        /// 储值支付后
        /// </summary>
        /// <param name="foodGoodsOrder"></param>
        public void AfterPaySuccesExec(FoodGoodsOrder foodGoodsOrder)
        {
            if (foodGoodsOrder == null)
            {
                return;
            }

            #region 自动打单 + 发送餐饮订单支付成功通知 模板消息
            
            Food foodStore = FoodBLL.SingleModel.GetModel(foodGoodsOrder.StoreId);
            if (foodStore == null)
            {
                return;
            }
            XcxAppAccountRelation xcxAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(foodStore.appId);
            if (xcxAccountRelation == null)
            {
                return;
            }
            Account account = AccountBLL.SingleModel.GetModel(xcxAccountRelation.AccountId);
            if (account == null)
            {
                return;
            }
            List<FoodPrints> foodPrintList = FoodPrintsBLL.SingleModel.GetList($" foodstoreid = {foodStore.Id} and accountId = '{account.OpenId}' and state >= 0 ");
            List<FoodGoodsCart> cartlist = FoodGoodsCartBLL.SingleModel.GetList($" GoodsOrderId={foodGoodsOrder.Id} and state=1");
            
            string goodsIds = string.Join(",", cartlist?.Select(s => s.FoodGoodsId).Distinct());
            List<FoodGoods> foodGoodsList = FoodGoodsBLL.SingleModel.GetListByIds(goodsIds);

            cartlist?.ForEach(cart =>
            {
                cart.goodsMsg = foodGoodsList?.FirstOrDefault(f=>f.Id == cart.FoodGoodsId);
            });

            //打单
            PrintOrder(foodStore, foodGoodsOrder, cartlist, foodPrintList, account);

            //餐饮订单支付成功-发送商家模板消息
            TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(foodGoodsOrder, account, foodStore);
            //餐饮订单支付成功-发送客户模板消息
            object orderData = TemplateMsg_Miniapp.FoodGetTemplateMessageData(foodStore, foodGoodsOrder, SendTemplateMessageTypeEnum.餐饮订单支付成功通知);
            TemplateMsg_Miniapp.SendTemplateMessage(foodGoodsOrder.UserId, SendTemplateMessageTypeEnum.餐饮订单支付成功通知, TmpType.小程序餐饮模板, orderData);

            #endregion
        }
        #endregion

        #region 拼团
        public int GetGroupCount(int groupsporid)
        {
            return GetCount($"goodtype={(int)EntGoodsType.拼团产品} and  groupid={groupsporid} and state in({(int)miniAppFoodOrderState.待接单},{(int)miniAppFoodOrderState.待接单},{(int)miniAppFoodOrderState.待就餐})");
        }


        /// <summary>
        /// 获取我的拼团订单
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<FoodGoodsOrder> GetMyFoodGoupOrderList(int t, int aid, int userId, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
                pageIndex = 1;
            List<FoodGoodsOrder> list = new List<FoodGoodsOrder>();
            string where = $"select o.*,sor.state GroupState from foodgoodsorder o left join entgroupsponsor sor on o.groupid = sor.id left join entgroupsrelation r on r.id = sor.entgoodrid where o.goodtype = {(int)EntGoodsType.拼团产品} and r.state<>-1 and o.userid = {userId}";
            if (0 == t)
            {
                where += $" and (sor.state <> {(int)GroupState.已过期} or o.groupid>0)";
            }//拼团中
            else if (1 == t)
            {
                where += $" and sor.State={(int)GroupState.开团成功} and sor.EndDate>now()";
            }//已成团
            else if (2 == t)
            {
                where += $" and  (sor.State={(int)GroupState.团购成功})";
            }//未成团
            else if (-1 == t)
            {
                where += $" and sor.State not in({(int)GroupState.团购成功}) and g.EndDate<now()";
            }

            where += $" order by sor.StartDate DESC LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, where))
            {
                while (dr.Read())
                {
                    FoodGoodsOrder model = GetModel(dr);
                    if (dr["GroupState"] != DBNull.Value)
                    {
                        model.GroupState = Convert.ToInt32(dr["GroupState"]);
                    }

                    list.Add(model);
                }
            }

            return list;
        }
        public object GetMyGroupPostData(int state, int rid, int userId, int pageIndex, int pageSize)
        {
            List<FoodGoodsOrder> goodOrderList = GetMyFoodGoupOrderList(state, rid, userId, pageIndex, pageSize);
            if (goodOrderList != null && goodOrderList.Any())
            {
                string orderids = string.Join(",", goodOrderList.Select(x => x.Id).Distinct());
                List<FoodGoodsCart> goodOrderDtlList = FoodGoodsCartBLL.SingleModel.GetListByOrderIds(orderids);
                if (goodOrderDtlList != null && goodOrderDtlList.Any())
                {
                    string goodids = string.Join(",", goodOrderDtlList.Select(x => x.FoodGoodsId).Distinct());
                    List<FoodGoods> goodList = FoodGoodsBLL.SingleModel.GetListByIds(goodids);
                    //判断商品是否已评论
                    GoodsCommentBLL.SingleModel.DealGoodsCommentState<FoodGoodsCart>(ref goodOrderDtlList, rid, userId, (int)EntGoodsType.拼团产品, "FoodGoodsId", "GoodsOrderId");
                    goodOrderDtlList.ForEach(x =>
                    {
                        x.goodsMsg = goodList.Where(y => y.Id == x.FoodGoodsId).FirstOrDefault();
                    });
                    object postdata = goodOrderList.Select(x =>
                       new
                       {
                           orderId = x.Id,
                           state = x.State,
                           groupstate = x.GroupState,
                           groupid = x.GroupId,
                           orderNum = x.OrderNum,
                           goodList = goodOrderDtlList.Where(y => y.GoodsOrderId == x.Id).ToList(),
                           citymorderId = x.OrderId,
                           buyPrice = x.BuyPriceStr,
                       });
                    return postdata;
                }
            }

            return null;
        }
        /// <summary>
        /// 获取拼团购买人
        /// </summary>
        /// <param name="groupid">团id</param>
        /// <returns></returns>
        public List<object> GetPersonByGroup(string groupids)
        {
            List<object> userimglist = new List<object>();
            //获取该产品的拼团所有支付成功的订单
            List<FoodGoodsOrder> list = GetListGroupOrder(groupids, 0);
            if (list == null || list.Count <= 0)
            {
                return userimglist;
            }

            //用户ID
            string userids = string.Join(",", list.Select(s => s.UserId).Distinct());
            List<C_UserInfo> userlist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
            if (userlist == null || userlist.Count <= 0)
            {
                return userimglist;
            }

            foreach (FoodGoodsOrder item in list)
            {
                C_UserInfo usermodel = userlist.Where(w => w.Id == item.UserId).FirstOrDefault();

                if (usermodel != null)
                {
                    userimglist.Add(new { Id = usermodel.Id, HeadImgUrl = usermodel.HeadImgUrl, Address = item.Address, Phone = item.AccepterTelePhone, Name = item.AccepterName, JoinGroupTime = item.PayDateStr });
                }
            }

            return userimglist;
        }
        /// <summary>
        /// 获取用户已团产品数量
        /// </summary>
        /// <param name="userid">用户userid</param>
        /// <param name="entgoodrid">产品关联拼团表ID</param>
        /// <param name="storeid">多门店分店ID，其他的为0</param>
        /// <returns></returns>
        public int GetGroupPersonCount(int userid, int entgoodid)
        {
            string sqlwhere = $@"select sum(o.qtycount) qtycount from foodgoodsorder o left join foodgoodscart ec on o.id = ec.goodsorderid
									where o.goodtype = {(int)EntGoodsType.拼团产品}
									and o.state in ({(int)miniAppFoodOrderState.已完成},{(int)miniAppFoodOrderState.待就餐},{(int)miniAppFoodOrderState.待接单}) 
									and ec.foodgoodsid = {entgoodid}";
            //0：改产品所有已售数量，大于0该用户已买数量
            if (userid > 0)
            {
                sqlwhere += $" and o.userid ={userid}";
            }
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sqlwhere);
            if (result != DBNull.Value)

            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        /// <summary>
        /// 获取拼团所有成功的订单
        /// </summary>
        /// <param name="groupids"></param>
        /// <returns></returns>
        public List<FoodGoodsOrder> GetListGroupOrder(string groupids, int userid)
        {
            string sqlwhere = $"goodtype = {(int)EntGoodsType.拼团产品} and groupid in ({groupids}) and state not in ({(int)miniAppFoodOrderState.已取消},{(int)miniAppFoodOrderState.待付款})";

            if (userid > 0)
            {
                sqlwhere += $" and userid = {userid}";
            }
            return base.GetList(sqlwhere);
        }

        public List<FoodGoodsOrder> GetListGroupOrder(int groupid)
        {
            return base.GetList($"GroupId={groupid} and goodtype={(int)EntGoodsType.拼团产品} and state  in ({(int)miniAppFoodOrderState.待就餐},{(int)miniAppFoodOrderState.待接单})");
        }

        #endregion
    }

    public class MiniappAdminGoodsOrderBLL : BaseMySql<FoodAdminGoodsOrder>
    {
        //查找订单列表
        public List<FoodAdminGoodsOrder> GetAdminList(int sendway, int foodId, out int totalCount, int type = 10, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10, string nickName = "", int export = 0, string verificationNum = "", string startTime = "", string endTime = "", string tableNo = "")
        {
            List<FoodAdminGoodsOrder> list = new List<FoodAdminGoodsOrder>();

            List<MySqlParameter> mysqlParamters = new List<MySqlParameter>();

            //查询sql
            string selectSql = @"select o.*,u.nickname,u.headimgurl from foodgoodsorder o
                                    left join c_userinfo u on o.userid = u.id 
                                  where {0} order by {1} limit {2},{3} ";
            //总数sql
            string countSql = @"select count(0) from foodgoodsorder o
                                    left join c_userinfo u on o.userid = u.id 
                                    where {0} ";

            //where
            string strWhere = $" o.StoreId={foodId} ";
            //配送方式条件
            if (sendway != -999)
            {
                strWhere += $" and o.GetWay={sendway} ";
            }
            if (!string.IsNullOrWhiteSpace(orderNum))
            {
                strWhere += $" and o.OrderNum = @orderNum ";
                mysqlParamters.Add(new MySqlParameter("@orderNum", orderNum));
            }

            //拼团订单
            if (type == (int)miniAppFoodOrderType.拼团)
            {
                strWhere += $" and o.GoodType={(int)EntGoodsType.拼团产品}";
            }

            else
            {
                strWhere += $" { ((type == 10 || !Enum.IsDefined(typeof(miniAppFoodOrderType), type)) ? "" : $" and o.OrderType = {type} ")} and o.GoodType={(int)EntGoodsType.普通产品}";
            }

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(miniAppFoodOrderState), orderState)) ? $" and o.State != {(int)miniAppFoodOrderState.付款中} " : $" and o.State = {orderState} ")} ";
            if (foodGoodsType != -999)
            {
                List<FoodGoods> goodlist = FoodGoodsBLL.SingleModel.GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    List<FoodGoodsCart> carts = FoodGoodsCartBLL.SingleModel.GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.Id))}) and state = 1");
                    if (carts != null && carts.Any())
                    {
                        strWhere += $" and o.Id in ({ string.Join(",", carts.Select(x => x.GoodsOrderId).Distinct()) }) ";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(nickName))
            {
                strWhere += $" and u.nickName = @nickName ";
                mysqlParamters.Add(new MySqlParameter("@nickName", nickName));
            }
            //提货码
            if (!string.IsNullOrWhiteSpace(verificationNum))
            {
                strWhere += $" and o.verificationNum = @verificationNum ";
                mysqlParamters.Add(new MySqlParameter("@verificationNum", verificationNum));
            }
            //日期范围
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                strWhere += $" and o.createDate between @startTime and @endTime ";
                mysqlParamters.Add(new MySqlParameter("@startTime", startTime));
                mysqlParamters.Add(new MySqlParameter("@endTime", endTime));
            }
            //桌台号
            if (!string.IsNullOrEmpty(tableNo))
            {
                string tSqlwhere = $"state>=0 and foodid={foodId} and scene like @tableNo";
                List<MySqlParameter> tparamters = new List<MySqlParameter>();
                tparamters.Add(new MySqlParameter("@tableNo", $"%{tableNo}%"));
                List<FoodTable> tables = FoodTableBLL.SingleModel.GetListByParam(tSqlwhere, tparamters.ToArray());

                if (tables != null && tables.Count > 0)
                {
                    string tableIds = string.Join(",", tables.Select(table => table.Id));
                    strWhere += $" and o.TablesNo in ({tableIds}) ";
                }
                else
                {
                    totalCount = 0;
                    return list;
                }
            }
            string orderField = " o.Id DESC";


            selectSql = string.Format(selectSql, strWhere, orderField, export > 0 ? 0 : (pageIndex - 1) * pageSize, pageSize);
            countSql = string.Format(countSql, strWhere);

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, selectSql, mysqlParamters.ToArray()))
            {
                list = GetList(dr);
            }

            totalCount = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, countSql, mysqlParamters.ToArray()));
            return list;
        }
    }
}
