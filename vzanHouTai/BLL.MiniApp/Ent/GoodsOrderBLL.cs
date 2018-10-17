using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Entity.MiniApp.Ent;
using BLL.MiniApp.Conf;
using BLL.MiniApp;

namespace BLL.MiniApp.Ent
{
    public class MiniappEntGoodsOrderBLL : BaseMySql<GoodsOrder>
    {
        /// <summary>
        /// 获取订单号
        /// </summary>
        /// <param name="goodsOrder"></param>
        /// <returns></returns>
        public string GetGoodsOrderNo(GoodsOrder goodsOrder)
        {
            return goodsOrder == null ? "" : goodsOrder.CreateDate.ToString("yyyy MMdd ") +
                   goodsOrder.Id.ToString().PadLeft(8, '0').Insert(4, " ");
        }

        public List<GoodsOrder> getModelByWhere(int appId, int pagesize, int pageindex, out int totalCount, string goodsName = "", string accName = "", string accPhone = "", int orderState = -999, DateTime? startDate = null, DateTime? endDate = null, string orderNum = "")
        {
            string sqlstr = @"select o.* from miniappentgoodsorder o";
            //--  left JOIN miniappentgoodscart c on c.GoodsOrderId = o.Id
            //--  left join miniappentgoods g on c.FoodGoodsId = g.id";
            string sqlWhere = $" where o.aid = '{appId}' {(orderState != -999 ? $" and  o.State  = {orderState} " : $" ")  }";
            List<MySqlParameter> mysqlPameter = new List<MySqlParameter>();
            if (!string.IsNullOrWhiteSpace(orderNum))
            {
                sqlWhere += " and o.OrderNum like @orderNum ";
                mysqlPameter.Add(new MySqlParameter("@orderNum", orderNum));
            }

            if (!string.IsNullOrWhiteSpace(goodsName))
            {
                var lists = new List<int>();
                lists.Add(0);
                goodsName = goodsName.Replace("'", "");
                var goods = new MiniappEntGoodsBLL().GetList($" name Like '{goodsName}' ");
                if (goods != null && goods.Any())
                {
                    var goodCar = new MiniappEntGoodsCartBLL().GetList($" state = 1 and FoodGoodsId in ({string.Join(",", goods.Select(x => x.id))}) ");
                    if (goodCar != null && goodCar.Any())
                    {
                        lists = goodCar.Select(x => x.GoodsOrderId).ToList();
                        //sqlWhere += $" and o.id in ({string.Join(",", goodCar.Select(x => x.GoodsOrderId))}) ";
                    }

                }
                sqlWhere += $" and o.id in ({string.Join(",", lists)}) ";
            }
            //if (!string.IsNullOrWhiteSpace(goodsName))
            //{
            //    sqlWhere += " and g.name like @goodname ";
            //    mysqlPameter.Add(new MySqlParameter("@goodname", goodsName));
            //}
            if (!string.IsNullOrWhiteSpace(accName))
            {
                sqlWhere += " and o.AccepterName like @acceptername ";
                mysqlPameter.Add(new MySqlParameter("@acceptername", accName));
            }
            if (!string.IsNullOrWhiteSpace(accPhone))
            {
                sqlWhere += " and o.AccepterTelePhone like @accpetertelephone ";
                mysqlPameter.Add(new MySqlParameter("@accpetertelephone", accPhone));
            }
            if (startDate != null)
            {
                sqlWhere += " and o.CreateDate >= @startDate ";
                mysqlPameter.Add(new MySqlParameter("@startDate", startDate?.ToString("yyyy-MM-dd 00:00:00")));
            }
            if (endDate != null)
            {
                sqlWhere += " and o.CreateDate <= @endDate ";
                mysqlPameter.Add(new MySqlParameter("@endDate", endDate?.ToString("yyyy-MM-dd 23:59:59")));
            }

            List<GoodsOrder> list = new List<GoodsOrder>();
            totalCount = 0;
            log4net.LogHelper.WriteInfo(GetType(), sqlstr + sqlWhere + $" order by id desc limit {(pageindex - 1) * pagesize},{pagesize} ");
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlstr + sqlWhere + $" order by id desc limit {(pageindex - 1) * pagesize},{pagesize} ", mysqlPameter.ToArray()))
            {
                while (dr.Read())
                {
                    list.Add(GetModel(dr));
                }
                string sqlCountStr = @"select Count(0) from miniappentgoodsorder o ";
                //-- left JOIN miniappentgoodscart c on c.GoodsOrderId = o.Id
                //-- left join miniappentgoods g on c.FoodGoodsId = g.id";
                totalCount = GetCountBySql(sqlCountStr + sqlWhere, mysqlPameter.ToArray());
            }

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
        public List<GoodsOrder> GetOrderByParames(int foodId, int type = 10, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10)
        {
            string strWhere = $" StoreId={foodId} ";
            orderNum = orderNum.Replace("'", "");
            strWhere += $" {(!string.IsNullOrWhiteSpace(orderNum) ? $" and OrderNum = '{orderNum}' " : "")} ";

            strWhere += $" { ((type == 10 || !Enum.IsDefined(typeof(C_Enums.miniAppFoodOrderType), type)) ? "" : $" and OrderType = {type} ")} ";

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(C_Enums.MiniAppEntOrderState), orderState)) ? $" " : $" and State = {orderState} ")} ";

            if (foodGoodsType != -999)
            {
                var goodlist = new MiniappEntGoodsOrderBLL().GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    var carts = new MiniappEntGoodsCartBLL().GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.Id))}) and state = 1");
                    if (carts != null && carts.Any())
                    {
                        strWhere += $" Id in ({ string.Join(",", carts.Select(x => x.GoodsOrderId).Distinct()) }) ";
                    }
                }
            }

            string orderField = "CreateDate DESC";
            List<GoodsOrder> list = GetList(strWhere, pageSize, pageIndex, "*", orderField);
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

            strWhere += $" { ((type == 10 || !Enum.IsDefined(typeof(C_Enums.miniAppFoodOrderType), type)) ? "" : $" and OrderType = {type} ")} ";

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(C_Enums.MiniAppEntOrderState), orderState)) ? $"  " : $" and State = {orderState} ")} ";

            //strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(C_Enums.MiniAppEntOrderState), orderState)) ? "" : $" and State = {orderState} ")} ";

            if (foodGoodsType != -999)
            {
                var goodlist = new MiniappEntGoodsBLL().GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    var carts = new MiniappEntGoodsCartBLL().GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.id))}) and state = 1");
                    if (carts != null && carts.Any())
                    {
                        strWhere += $" Id in ({ string.Join(",", carts.Select(x => x.GoodsOrderId).Distinct()) }) ";
                    }
                }
            }

            //string orderField = "CreateDate DESC";
            var totle = GetCount(strWhere);
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
        public List<GoodsOrder> GetUserToOrder(int userId, int state, int pageIndex, int pageSize = 10)
        {
            string strWhere = $" UserId={userId}";

            switch (state)
            {
                case (int)C_Enums.OrderState.未付款:
                case (int)C_Enums.OrderState.待核销:
                case (int)C_Enums.OrderState.已核销:
                case (int)C_Enums.OrderState.待发货:
                case (int)C_Enums.OrderState.正在配送:
                case (int)C_Enums.OrderState.待收货:
                case (int)C_Enums.OrderState.已收货:
                case (int)C_Enums.OrderState.已退款:
                    strWhere += $" and State={state}";
                    break;
                case -10:
                    strWhere += $" and (State={(int)C_Enums.OrderState.待收货} or State={(int)C_Enums.OrderState.正在配送} or State={(int)C_Enums.OrderState.待核销})";
                    break;
            }
            string orderField = "CreateDate DESC";
            List<GoodsOrder> list = GetList(strWhere, pageSize, pageIndex, "*", orderField);
            return list;
        }

        ///// <summary>
        ///// 商品订单过期//不用
        ///// </summary>
        //public void CityGoodsOrderTimeOut(int timeoutlength = -30)
        //{
        //    TransactionModel tranModel = new TransactionModel();
        //    MiniappEntGoodsCartBLL goodsorderdetailbll = new MiniappEntGoodsCartBLL();
        //    //MiniappEntGoodsAttrBLL goodsattrbll = new MiniappFoodGoodsAttrBLL();
        //    MiniappEntGoodsBLL goodsbll = new MiniappEntGoodsBLL();

        //    //订单超过30分钟取消订单
        //    List<MiniappEntGoodsOrder> orderList = GetList($"State=0 and  CreateDate < date_add(NOW(), interval {timeoutlength} MINUTE)", 100, 1);
        //    if (orderList != null && orderList.Count > 0)
        //    {
        //        tranModel = new TransactionModel();
        //        //订单明细
        //        List<MiniappEntGoodsCart> orderdetaillist = orderList.Any() ? goodsorderdetailbll.GetList($"State =0 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<MiniappEntGoodsCart>();
        //        if (orderdetaillist != null && orderdetaillist.Count > 0)
        //        {
        //            //商品
        //            List<MiniappEntGoods> goodlist = goodsbll.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
        //            if (goodlist != null && goodlist.Count > 0)
        //            {
        //                foreach (var item in orderList)
        //                {
        //                    //商品明细
        //                    var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.OrderId).ToList() : new List<MiniappEntGoodsCart>();
        //                    if (orderdetails != null && orderdetails.Count > 0)
        //                    {
        //                        for (int i = 0; i < orderdetails.Count; i++)
        //                        {
        //                            //商品
        //                            var good = goodlist.Where(w => w.id == orderdetails[i].FoodGoodsId).FirstOrDefault();
        //                            if (good != null)
        //                            {
        //                                //商品加总库存
        //                                good.stock += orderdetails[i].Count;
        //                                //订单明细中的规格属性，加规格属性库存
        //                                for (int j = 0; j < good.GASDetailList.Count; j++)
        //                                {
        //                                    if (good.GASDetailList[j].id == orderdetails[i].SpecIds)
        //                                    {
        //                                        good.GASDetailList[j].stock += orderdetails[i].Count;
        //                                        good.specificationdetail = Utility.Serialize.SerializeHelper.SerToJson(good.GASDetailList);
        //                                        break;
        //                                    }
        //                                }
        //                                //更改商品总库存和规格属性库存
        //                                tranModel.Add($"update MiniappEntGoods set specificationdetail={good.specificationdetail},stock={good.stock} where Id={good.id}");
        //                            }
        //                        }
        //                    }

        //                    //订单状态改成已过期
        //                    item.State = -1;
        //                    tranModel.Add($"update MiniappEntGoodsOrder set State={item.State} where Id={item.Id}");

        //                    ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 方法操作：1.创建订单,2.将购物车对应内容转为订单内容,3.减库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="goodsCar"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool addGoodsOrder(GoodsOrder order, List<GoodsCart> goodsCar, C_UserInfo userInfo, ref string msg)
        {
            var TranModel = new TransactionModel();

            //创建订单
            TranModel.Add(BuildAddSql(order));
            log4net.LogHelper.WriteInfo(GetType(), BuildAddSql(order));
            //将购物车记录转为订单明细记录
            TranModel.Add($" update MiniappEntGoodsCart set GoodsOrderId = (select last_insert_id()),State = 1,UserId = {userInfo.Id} where id in ({string.Join(",", goodsCar.Select(s => s.Id).Distinct())}) and state = 0; ");

            //根据订单内记录数量减库存
            var goodsList = new MiniappEntGoodsBLL().GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.FoodGoodsId).Distinct().ToList())}) ");

            var goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<GoodsAttrDetail>();
            msg += "a";
            goodsCar.ForEach(x =>
            {
                var good = goodsList.Where(y => y.id == x.FoodGoodsId).FirstOrDefault();
                if (good.stockLimit) //限制库存时才去操作库存
                {
                    if (string.IsNullOrWhiteSpace(x.SpecIds))
                    {

                        good.stock -= x.Count;
                    }
                    else
                    {
                        good.stock -= x.Count;
                        //var goodList = good.GASDetailList.Where(z => z.id.Equals(x.SpecIds));
                        var GASDetailList = new List<GoodsAttrDetail>();
                        good.GASDetailList.ForEach(y =>
                        {
                            if (y.id.Equals(x.SpecIds))
                            {
                                y.stock -= x.Count;
                            }
                            GASDetailList.Add(y);
                            //log4net.LogHelper.WriteInfo(GetType(), goodDtlJsonHelper.SToJsonArray(new List<MiniappGoodsAttrDetail>() { y }));
                        });
                        //good2.count -= x.Count;
                        //规格库存详情重新赋值
                        good.specificationdetail = goodDtlJsonHelper.SToJsonArray(GASDetailList);
                    }
                }
                //购物车更入商品折扣后价格
                TranModel.Add($" update MiniappEntGoodsCart set price = {x.Price} where id = {x.Id} ");
            });
            msg += "b";
            var _miniappgoodsBll = new MiniappEntGoodsBLL();
            msg += "abel";
            goodsList.ForEach(x =>
            {
                if (x.stockLimit)
                {
                    TranModel.Add($" update miniappEntgoods set Stock={x.stock},specificationdetail='{x.specificationdetail}' where Id = {x.id} ");
                }
            });
            msg += "c";
            var stringSql = "";
            for (int i = 0; i < TranModel.sqlArray.Length; i++)
            {
                stringSql += TranModel.sqlArray[i];
            }
            msg += "d";
            //log4net.LogHelper.WriteInfo(GetType(),stringSql);
            foreach (var x in TranModel.sqlArray)
            {
                log4net.LogHelper.WriteInfo(GetType(), x);
            }
            return ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="timeoutlength"></param>
        public bool updateFoodOrderState(GoodsOrder order, int oldState, string updateColNames)
        {
            var updateSql = BuildUpdateSql(order, updateColNames) + $" and state = {oldState} ;";

            var updateLine = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);


            return updateLine >= 1;
        }

        /// <summary>
        /// 订单作废需要对应操作库存
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public bool updateStock(GoodsOrder order, int oldState)
        {
            //log4net.LogHelper.WriteInfo(GetType(), "a");
            if (order == null)
            {
                return false;
            }
            //log4net.LogHelper.WriteInfo(GetType(), "b");
            if (!Enum.IsDefined(typeof(C_Enums.MiniAppEntOrderState), oldState)
                    || !Enum.IsDefined(typeof(C_Enums.MiniAppEntOrderState), order.State))
            {
                return false;
            }
            //log4net.LogHelper.WriteInfo(GetType(), "c");
            TransactionModel tranModel = new TransactionModel();
            MiniappEntGoodsCartBLL goodsorderdetailbll = new MiniappEntGoodsCartBLL();
            //MiniappEntGoodsAttrBLL goodsattrbll = new MiniappFoodGoodsAttrBLL();
            MiniappEntGoodsBLL goodsbll = new MiniappEntGoodsBLL();
            //MiniappEntGoodsOrderLogBLL orderLogBll = new MiniappEntGoodsOrderLogBLL();

            //MiniappFoodGoodsOrder order = GetModel(orderid);
            tranModel = new TransactionModel();
            //订单明细
            List<GoodsCart> orderdetaillist = goodsorderdetailbll.GetList($" State = 1 and GoodsOrderId in ({order.Id})") ?? new List<GoodsCart>();
            if (orderdetaillist != null && orderdetaillist.Count > 0)
            {
                //商品
                List<Goods> goodlist = goodsbll.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                if (goodlist != null && goodlist.Count > 0)
                {

                    //商品明细
                    var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == order.Id).ToList() : new List<GoodsCart>();
                    if (orderdetails != null && orderdetails.Count > 0)
                    {
                        for (int i = 0; i < orderdetails.Count; i++)
                        {
                            //商品
                            var good = goodlist.Where(w => w.id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                            if (good != null && good.stockLimit)
                            {
                                //商品加总库存
                                good.stock += orderdetails[i].Count;
                                var GASDetailList = good.GASDetailList;
                                //订单明细中的规格属性，加规格属性库存
                                for (int j = 0; j < GASDetailList.Count; j++)
                                {
                                    if (GASDetailList[j].id == orderdetails[i].SpecIds)
                                    {
                                        GASDetailList[j].stock += orderdetails[i].Count;
                                        good.specificationdetail = Utility.Serialize.SerializeHelper.SerToJson(GASDetailList);
                                        break;
                                    }
                                }
                                //更改商品总库存和规格属性库存
                                tranModel.Add($"update MiniappEntGoods set specificationdetail='{good.specificationdetail}',stock={good.stock} where Id={good.id}");

                            }
                        }
                    }

                    ////订单状态改成已过期
                    //order.State = (int)C_Enums.MiniAppEntOrderState.已取消;
                    tranModel.Add($"update MiniappEntGoodsOrder set State={order.State} where Id={order.Id} and State = {oldState} ");
                    
                    return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                }
            }
            return false;
        }

        //订单超过15分钟未付款取消订单
        public void updateOrderStateForCancle(int timeoutlength = -15)
        {
            //string sql = $" update  miniappfoodgoodsorder set state = {(int)C_Enums.MiniAppEntOrderState.已取消} where state = {(int)C_Enums.MiniAppEntOrderState.待付款} and  (NOW()-INTERVAL 15 minute) <= CreateDate  ";
            TransactionModel tranModel = new TransactionModel();
            MiniappEntGoodsCartBLL goodsorderdetailbll = new MiniappEntGoodsCartBLL();
            //MiniappFoodGoodsAttrBLL goodsattrbll = new MiniappEntGoodsAttrBLL();
            MiniappEntGoodsBLL goodsbll = new MiniappEntGoodsBLL();
            MiniappEntGoodsOrderLogBLL orderLogBll = new MiniappEntGoodsOrderLogBLL();

            //订单超过15分钟取消订单
            List<GoodsOrder> orderList = GetList($"State={(int)C_Enums.MiniAppEntOrderState.待付款} and  CreateDate <= (NOW()+INTERVAL {timeoutlength * 60} second)", 1000, 1);
            if (orderList != null && orderList.Count > 0)
            {
                tranModel = new TransactionModel();
                //订单明细
                List<GoodsCart> orderdetaillist = orderList.Any() ? goodsorderdetailbll.GetList($"State = 1 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<GoodsCart>();
                if (orderdetaillist != null && orderdetaillist.Count > 0)
                {
                    //商品
                    List<Goods> goodlist = goodsbll.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                    if (goodlist != null && goodlist.Count > 0)
                    {
                        foreach (var item in orderList)
                        {
                            //商品明细
                            var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.Id).ToList() : new List<GoodsCart>();
                            if (orderdetails != null && orderdetails.Count > 0)
                            {
                                for (int i = 0; i < orderdetails.Count; i++)
                                {
                                    //商品
                                    var good = goodlist.Where(w => w.id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                                    if (good != null && good.stockLimit)
                                    {
                                        //商品加总库存(剩余库存)
                                        good.stock += orderdetails[i].Count;
                                        var GASDetailList = good.GASDetailList;
                                        //订单明细中的规格属性，加规格属性库存
                                        for (int j = 0; j < GASDetailList.Count; j++)
                                        {
                                            if (GASDetailList[j].id == orderdetails[i].SpecIds)
                                            {
                                                GASDetailList[j].stock += orderdetails[i].Count;
                                                good.specificationdetail = Utility.Serialize.SerializeHelper.SerToJson(GASDetailList);
                                                break;
                                            }
                                        }
                                        //更改商品总库存和规格属性库存
                                        tranModel.Add($"update MiniappEntGoods set specificationdetail='{good.specificationdetail}',stock={good.stock} where Id={good.id}");

                                    }
                                }
                            }

                            //订单状态改成已过期
                            item.State = (int)C_Enums.MiniAppEntOrderState.已取消;
                            tranModel.Add($"update MiniappEntGoodsOrder set State={item.State} where Id={item.Id} and State = {(int)C_Enums.MiniAppEntOrderState.待付款} ;");

                            //事务内某行sql执行受影响行数为0,会回滚整个事务
                            if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                            {
                                orderLogBll.AddLog(item.Id, 0, "系统自动将超过15分钟未付款的订单取消成功！");
                            }
                            else
                            {
                                orderLogBll.AddLog(item.Id, 0, "系统自动将超过15分钟未付款的订单取消失败！");
                            }
                        }
                    }
                }
            }


        }


        //自动完成订单（10天）
        public void updateOrderStateForComplete(int timeoutlength = -10)
        {
            TransactionModel tranModel = new TransactionModel();
            //MiniappFoodGoodsOrderLogBLL orderLogBll = new MiniappFoodGoodsOrderLogBLL();

            List<int> updateByOldStateList = new List<int>();
            updateByOldStateList.Add((int)C_Enums.MiniAppEntOrderState.待收货);

            var list = GetList($" State in ({string.Join(", ", updateByOldStateList)}) and  DistributeDate <= (NOW()+INTERVAL {timeoutlength * 60 * 24} MINUTE )");
            var updateSql = $" update  MiniappFoodGoodsOrder set State = {(int)C_Enums.MiniAppEntOrderState.交易成功},AcceptDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where  State in ({string.Join(",", updateByOldStateList)}) and  DistributeDate <= (NOW()+interval {timeoutlength} MINUTE) ";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);

            var userlogBll = new  TemplateMsg_UserLogBLL();
            //list.ForEach(x =>
            //{
            //    //orderLogBll.AddLog(x.Id, "0", "(堂食接单后/外卖开始配送后) 超过2小时,系统自动完成订单");
            //    //userlogBll.confTemplateMsg(x.Id, 8);
            //});



        }

        public TransactionModel addSalesCount(int orderId, TransactionModel _tranModel)
        {
            List<GoodsCart> orderDetailList = new MiniappEntGoodsCartBLL().GetList($"GoodsOrderId={orderId}");
            if (orderDetailList != null && orderDetailList.Count > 0)
            {
                //记录订单支付日志
                List<Goods> orderGoodsList = new MiniappEntGoodsBLL().GetList($" Id in ({string.Join(",", orderDetailList.Select(x => x.FoodGoodsId).Distinct())})");
                //加商品销量
                orderGoodsList.ForEach(x =>
                {
                    var shopQty = orderDetailList.Where(y => y.FoodGoodsId == x.id).Sum(y => y.Count);

                    _tranModel.Add($" Update MiniappFoodGoods set salesCount = salesCount + {shopQty} where Id = {x.id} ;");
                });
            }
            return _tranModel;
        }



        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool outOrder(GoodsOrder item, int oldState)
        {
            //var item = GetModel(orderId);
            //var oldState = item.State;

            CityMorders order = new CityMordersBLL().GetModel(item.OrderId);
            item.State = (int)C_Enums.MiniAppEntOrderState.退款中;
            item.outOrderDate = DateTime.Now;
            if (order == null)
            {
                item.State = (int)C_Enums.MiniAppEntOrderState.退款失败;
                Update(item, "State,outOrderDate,Remark");
                return false;
            }

            //重新加回库存
            if (updateStock(item, oldState))
            {
                Update(item, "Remark");//添加拒绝原因
                ReFundQueue reModel = new ReFundQueue
                {
                    minisnsId = -5,
                    money = item.BuyPrice,
                    orderid = order.Id,
                    traid = order.trade_no,
                    addtime = DateTime.Now,
                    note = "小程序行业版退款",
                    retype = 1
                };
                try
                {
                    new ReFundQueueBLL().Add(reModel);
                    Update(item, "State,outOrderDate");
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
        /// 跟进 退款状态 (退款是否成功)
        /// </summary>
        /// <returns></returns>
        public bool updateOutOrderState()
        {

            MsnModelHelper msnHelper = new MsnModelHelper();
             TemplateMsgBLL tmgBll = new  TemplateMsgBLL();
             TemplateMsg_UserBLL tmguBll = new  TemplateMsg_UserBLL();
             TemplateMsg_UserLogBLL tmgulBll = new  TemplateMsg_UserLogBLL();
            C_UserInfoBLL userInfoBll = new C_UserInfoBLL();

            TransactionModel tranModel = new TransactionModel();
            var itemList = GetList($" State = {(int)C_Enums.MiniAppEntOrderState.退款中} and outOrderDate <= (NOW()-interval 17 second) ", 1000, 1) ?? new List<GoodsOrder>();
            var orderList = new List<CityMorders>();
            var outOrderList = new List<Entity.MiniApp.ReFundResult>();
            if (itemList.Any())
            {
                orderList = new CityMordersBLL().GetList($" Id in ({string.Join(",", itemList.Select(x => x.OrderId))}) ", 1000, 1) ?? new List<CityMorders>();
                if (orderList.Any())
                {
                    outOrderList = new RefundResultBLL().GetList($" transaction_id in ('{string.Join("','", orderList.Select(x => x.trade_no))}') and retype = 1") ?? new List<ReFundResult>();
                    itemList.ForEach(x =>
                    {
                        var curOrder = orderList.Where(y => y.Id == x.OrderId).FirstOrDefault();
                        if (curOrder != null)
                        {
                            //退款是排程处理,故无法确定何时执行退款,而现阶段退款操作成败与否都会记录在系统内
                            var curOutOrder = outOrderList.Where(y => y.transaction_id == curOrder.trade_no).FirstOrDefault();
                            if (curOutOrder != null && curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = (int)C_Enums.MiniAppEntOrderState.退款成功;
                                tranModel.Add(BuildUpdateSql(x, "State"));
                            }
                            else if (curOutOrder != null && curOutOrder.result_code.Equals("SUCCESS"))
                            {
                                x.State = (int)C_Enums.MiniAppEntOrderState.退款失败;
                                tranModel.Add(BuildUpdateSql(x, "State"));
                            }
                        }
                    });
                }
            }
            var isSuccess = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);

             TemplateMsg_UserParamBLL userParamBLL = new  TemplateMsg_UserParamBLL();
            return isSuccess;
        }
        
        
    }
    public class MiniappEntAdminGoodsOrderBLL : BaseMySql<AdminGoodsOrder>
    {
        //查找订单列表
        public List<AdminGoodsOrder> GetAdminList(string where, int pagesize, int pageindex, out int totalCount, string goodsName = "", string accName = "", string accPhone = "", string orderState = "",  bool export = false)
        {
            List<AdminGoodsOrder> list = new List<AdminGoodsOrder>();
            string sql;
            string sqlCount;

            sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappentgoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";
            //sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,user.NickName,user.TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders inner join c_userinfo user on user.Id=orders.UserId {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";
            if (export)//导出Excel的话，不需要分页
            {
                sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc";
                //sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,user.NickName,user.TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders inner join c_userinfo user on user.Id=orders.UserId {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc";
            }
            sqlCount = $@"select count(*) from miniappgoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)}";

            MiniappEntGoodsBLL _goodsbll = new MiniappEntGoodsBLL();
            //拼接数据
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    var cartlist = new MiniappEntGoodsCartBLL().GetList($"GoodsOrderId={model.Id}");
                    var detaillist = new List<OrderCardDetail>();
                    foreach (var item in cartlist)
                    {
                        var cart = new OrderCardDetail();
                        cart.Id = item.Id;
                        var goods = _goodsbll.GetModel(item.FoodGoodsId);
                        if (goods != null)
                        {
                            cart.GoodsName = goods.name;
                            cart.ImgUrl = goods.img;
                        }
                        cart.SpecInfo = item.SpecInfo;
                        cart.Price = item.Price;
                        cart.Count = item.Count;
                        detaillist.Add(cart);
                    }
                    model.GoodsList = detaillist;
                    list.Add(model);
                }
            }
            totalCount = GetCountBySql(sqlCount);
            return list;
        }
    }
}
