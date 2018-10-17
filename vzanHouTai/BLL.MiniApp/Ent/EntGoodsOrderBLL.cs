using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using BLL.MiniApp.User;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Footbath;
using Entity.MiniApp.Tools;
using Entity.MiniApp.User;
using Entity.MiniApp.ViewModel;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntGoodsOrderBLL : BaseMySql<EntGoodsOrder>
    {
        #region 单例模式
        private static EntGoodsOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodsOrderBLL()
        {

        }

        public static EntGoodsOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodsOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly SalesManRecordBLL salesManRecordBLL = new SalesManRecordBLL();
        
        #region 基础操作

        public int GetOrderPriceSumByAppId(string appId)
        {
            int priceSum = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return priceSum;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            string sql = $"select sum(buyprice) pricesum from entgoodsorder where appid=@appid and State =3";
            var result = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql, paramters.ToArray());
            if (result != DBNull.Value)
            {
                priceSum = Convert.ToInt32(result);
            }

            return priceSum;
        }

        public int GetGroupCount(int rid, int groupsporid)
        {
            return GetCount($"ordertype=3 and aId = {rid} and groupid={groupsporid} and state in({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.待自取})");
        }

        /// <summary>
        /// 获取已经完成交易的订单
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public int GetOrderSum(string appId, int state, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            string sqlwhere = $"appid=@appid and state ={state}";
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlwhere += " and CreateDate>=@startDate and CreateDate<=@endDate";
                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
            }
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        public List<EntGoodsOrder> GetListByGoodsState(int rid, string groupids, MiniAppEntOrderState state)
        {
            return base.GetList($"aid={rid} and state={(int)state} and groupid in ({groupids})");
        }

        /// <summary>
        /// 获取拼团所有支付成功的订单
        /// </summary>
        /// <param name="groupids"></param>
        /// <returns></returns>
        public List<EntGoodsOrder> GetListGroupOrder(string groupids)
        {
            return GetList($"ordertype = 3 and groupid in ({groupids}) and state in ({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.交易成功},{(int)MiniAppEntOrderState.待收货},{(int)MiniAppEntOrderState.待自取})");
        }

        public EntGoodsOrder GetModelByOrderId(int aid, int orderId)
        {
            string whereSql = $"aid={aid} and orderid={orderId}";
            return base.GetModel(whereSql);
        }

        public List<EntGoodsOrder> GetListApplyCancelService(int timeLength)
        {
            string sqlwhere = $"state={(int)MiniAppEntOrderState.申请取消订单} and ApplyReturnTime<'{DateTime.Now.AddMinutes(timeLength)}'";
            return base.GetList(sqlwhere, 500, 1);
        }

        public List<EntGoodsOrder> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<EntGoodsOrder>();

            return base.GetList($"id in ({ids})");
        }
        #endregion 基础操作

        /// <summary>
        /// 获取订单号
        /// </summary>
        /// <param name="goodsOrder"></param>
        /// <returns></returns>
        public string GetGoodsOrderNo(EntGoodsOrder goodsOrder)
        {
            return goodsOrder == null ? "" : goodsOrder.CreateDate.ToString("yyyy MMdd ") +
                   goodsOrder.Id.ToString().PadLeft(8, '0').Insert(4, " ");
        }

        public List<EntGoodsOrder> getModelByWhere(int appId, int pagesize, int pageindex, out int totalCount, string goodsName = "", string accName = "", string accPhone = "", int orderState = -999, DateTime? startDate = null, DateTime? endDate = null, string orderNum = "", string tablesNo = "", int getWay = -1, int export = 0, int ordertype = 0, int reservationId = 0, int flashDealId = 0, int qrCodeId = 0)
        {
            string sqlstr = @"select o.* from entgoodsorder o";

            int tempOrderState = orderState;
            if (tempOrderState == -1000)
            {
                orderState = 3;//拼团跟普通产品订单 交易成功都为3 只有交易成功的订单才能评论orderState=-1000表示查询待评价订单
            }
            if (tempOrderState == -2000)
            {
                orderState = -999;//分销订单
            }


            //sqlstr += "left JOIN entgoodscart c on c.GoodsOrderId = o.Id";
            //sqlstr += "left join entgoods g on c.FoodGoodsId = g.id";
            string sqlWhere = $" where o.aid = '{appId}' and o.State<>{(int)MiniAppEntOrderState.已删除} {(orderState != -999 ? $" and  o.State  = {orderState} " : $" ")  } and o.ordertype ={ordertype}";
            if (tempOrderState == -1000)
            {
                sqlstr += " left JOIN GoodsComment c on c.orderid = o.Id ";
                sqlWhere += $" and c.id is NULL";
            }
            if (tempOrderState == -2000)
            {

                sqlWhere += $"  and o.Id in ({GetDistributionOrderIds(appId)})";//分销订单
            }

            //拼团条件
            string tempwhere = GetGroupSql(orderState, appId);
            if (!string.IsNullOrEmpty(tempwhere))
            {
                sqlWhere = tempwhere;
            }

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
                var goods = EntGoodsBLL.SingleModel.GetList($" name Like '{goodsName}' ");
                if (goods != null && goods.Any())
                {
                    var goodCar = EntGoodsCartBLL.SingleModel.GetList($" state = 1 and FoodGoodsId in ({string.Join(",", goods.Select(x => x.id))}) ");
                    if (goodCar != null && goodCar.Any())
                    {
                        lists = goodCar.Select(x => x.GoodsOrderId).ToList();
                    }
                }
                sqlWhere += $" and o.id in ({string.Join(",", lists)}) ";
            }
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

            if (!string.IsNullOrWhiteSpace(tablesNo))
            {
                sqlWhere += " and o.tablesNo = @tablesNo ";
                mysqlPameter.Add(new MySqlParameter("@tablesNo", tablesNo));
            }

            if (getWay >= 0)
            {
                sqlWhere += " and o.getWay = @getWay ";
                mysqlPameter.Add(new MySqlParameter("@getWay", getWay));
            }

            if (reservationId > 0)
            {
                sqlWhere += " and o.reserveid = @reservationId ";
                mysqlPameter.Add(new MySqlParameter("@reservationId", reservationId));
            }
            if (flashDealId > 0)
            {
                sqlWhere += $" and instr(attribute,'\"flashDealId\":{flashDealId}') > 0";
            }
            if (qrCodeId > 0)
            {
                sqlWhere += $" and StoreCodeId = {qrCodeId}";
            }

            List<EntGoodsOrder> list = new List<EntGoodsOrder>();
            totalCount = 0;
            EntGoodsOrder curOrder = null;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlstr + sqlWhere + $" order by id desc limit { (export > 0 ? 0 : (pageindex - 1) * pagesize)},{pagesize} ", mysqlPameter.ToArray()))
            {
                while (dr.Read())
                {
                    curOrder = GetModel(dr);
                    if (curOrder != null)
                    {
                        if (curOrder.TablesNo == null || curOrder.TablesNo.Equals("0"))//这里curOrder.TablesNo可能为NULL，所以需要先进行判断否则报异常curOrder.TablesNo == null
                        {
                            curOrder.TablesNo = "";
                        }
                        list.Add(curOrder);
                    }
                }
                string sqlCountStr = @"select Count(0) from entgoodsorder o ";
                if (tempOrderState == -1000)
                {
                    sqlCountStr += " left JOIN GoodsComment c on c.orderid = o.Id ";

                }
                //-- left JOIN miniappentgoodscart c on c.GoodsOrderId = o.Id
                //-- left join miniappentgoods g on c.FoodGoodsId = g.id";
                totalCount = GetCountBySql(sqlCountStr + sqlWhere, mysqlPameter.ToArray());
            }

            return list;
        }


        public List<EntGoodsOrder> getListEntGoodsOrder(int appId, int userId, int orderState, int pagesize, int pageindex)
        {
            string sqlstr = @"select o.* from entgoodsorder o";

            int tempOrderState = orderState;
            if (tempOrderState == -1000)
            {
                orderState = (int)MiniAppEntOrderState.交易成功;//拼团跟普通产品订单 交易成功都为3 只有交易成功的订单才能评论orderState=-1000表示查询待评价订单
            }

            string stateSql = "";
            if (orderState != 10)
            {
                if (orderState == (int)MiniAppEntOrderState.交易成功)
                {
                    if (tempOrderState != -1000)
                    {
                        List<int> completionState = new List<int>();
                        completionState.Add((int)MiniAppEntOrderState.交易成功);
                        completionState.Add((int)MiniAppEntOrderState.退款中);
                        completionState.Add((int)MiniAppEntOrderState.退款失败);
                        completionState.Add((int)MiniAppEntOrderState.退款成功);
                        completionState.Add((int)MiniAppEntOrderState.退货退款成功);
                        completionState.Add((int)MiniAppEntOrderState.退换货成功);
                        stateSql += $" and o.State in ({string.Join(",", completionState)}) ";
                    }
                    else
                    {
                        stateSql += $" and o.State = {orderState} ";
                    }
                }
                else if (orderState == (int)MiniAppEntOrderState.待收货)
                {
                    stateSql += $" and o.State in ({(int)MiniAppEntOrderState.待收货},{(int)MiniAppEntOrderState.待自取}) ";
                }
                else if (orderState == (int)MiniAppEntOrderState.退货中)
                {
                    stateSql += $" and o.State in ({(int)MiniAppEntOrderState.退货审核中},{(int)MiniAppEntOrderState.待退货},{(int)MiniAppEntOrderState.退货中},{(int)MiniAppEntOrderState.换货中})";
                }
                else
                {
                    stateSql += $" and o.State = {orderState} ";
                }
            }




            string sqlWhere = $" where o.aId = {appId} and o.UserId = {userId} and o.OrderType in ({(int)EntGoodsType.普通产品},{(int)EntGoodsType.团购商品}) and o.GroupId = 0 and o.State<>{(int)MiniAppEntOrderState.已删除} { stateSql } ";
            if (tempOrderState == -1000)
            {
                sqlstr += " left JOIN GoodsComment c on c.orderid = o.Id ";
                sqlWhere += $" and c.id is NULL";
            }
            List<EntGoodsOrder> list = new List<EntGoodsOrder>();
            EntGoodsOrder curOrder = null;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlstr + sqlWhere + $" order by CreateDate desc limit {  (pageindex - 1) * pagesize},{pagesize} "))
            {
                while (dr.Read())
                {
                    curOrder = GetModel(dr);
                    if (curOrder != null)
                    {
                        list.Add(curOrder);
                    }
                }
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
        public List<EntGoodsOrder> GetOrderByParames(int foodId, int type = 10, string orderNum = "", int orderState = -999, int foodGoodsType = -999, int pageIndex = 1, int pageSize = 10)
        {
            string strWhere = $" StoreId={foodId} ";
            orderNum = orderNum.Replace("'", "");
            strWhere += $" {(!string.IsNullOrWhiteSpace(orderNum) ? $" and OrderNum = '{orderNum}' " : "")} ";

            strWhere += $" { ((type == 10 || !Enum.IsDefined(typeof(miniAppFoodOrderType), type)) ? "" : $" and OrderType = {type} ")} ";

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(MiniAppEntOrderState), orderState)) ? $" " : $" and State = {orderState} ")} ";

            if (foodGoodsType != -999)
            {
                var goodlist =GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    var carts = EntGoodsCartBLL.SingleModel.GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.Id))}) and state = 1");
                    if (carts != null && carts.Any())
                    {
                        strWhere += $" Id in ({ string.Join(",", carts.Select(x => x.GoodsOrderId).Distinct()) }) ";
                    }
                }
            }

            string orderField = "CreateDate DESC";
            List<EntGoodsOrder> list = GetList(strWhere, pageSize, pageIndex, "*", orderField);
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

            strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(MiniAppEntOrderState), orderState)) ? $"  " : $" and State = {orderState} ")} ";

            //strWhere += $" { ((orderState == -999 || !Enum.IsDefined(typeof(C_Enums.MiniAppEntOrderState), orderState)) ? "" : $" and State = {orderState} ")} ";

            if (foodGoodsType != -999)
            {
                var goodlist = EntGoodsBLL.SingleModel.GetList($" typeid = {foodGoodsType} ");
                if (goodlist != null && goodlist.Any())
                {
                    var carts = EntGoodsCartBLL.SingleModel.GetList($" FoodGoodsId in ({string.Join(",", goodlist.Select(x => x.id))}) and state = 1");
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

        /// <summary>
        /// 方法操作：1.创建订单,2.将购物车对应内容转为订单内容,3.减库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="goodsCar"></param>
        /// <param name="userInfo"></param>
        ///
        /// <returns></returns>
        public bool addGoodsOrder(EntGoodsOrder order, List<EntGoodsCart> goodsCar, C_UserInfo userInfo, StringBuilder extraSql, ref string msg)
        {
            if (order == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $" function:(addGoodsOrder) || order_null  ");
                return false;
            }
            if (goodsCar == null || !goodsCar.Any())
            {
                log4net.LogHelper.WriteInfo(GetType(), $" function:(addGoodsOrder) || goodsCar_nullOrEmpty  ");
                return false;
            }
            if (userInfo == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $" function:(addGoodsOrder) || userInfo_null ");
                return false;
            }

            TransactionModel tran = new TransactionModel();
            //更新会员打折后的购物车
            if (extraSql != null)
            {
                tran.Add(extraSql.ToString());
            }

            //创建订单
            tran.Add(BuildAddSql(order));
            //将购物车记录转为订单明细记录
            tran.Add($" update EntGoodsCart set GoodsOrderId = (select last_insert_id()),State = 1,UserId = {userInfo.Id} where id in ({string.Join(",", goodsCar.Select(s => s.Id).Distinct())}) and state = 0; ");

            //根据订单内记录数量减库存,加销量
            List<EntGoods> goods = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.FoodGoodsId).Distinct().ToList())}) ");

            Utility.Easyui.EasyuiHelper<EntGoodsAttrDetail> goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<EntGoodsAttrDetail>();

            goodsCar.ForEach(x =>
            {
                var curGood = goods.Where(y => y.id == x.FoodGoodsId).FirstOrDefault();
                curGood.salesCount += x.Count;
                if (curGood.stockLimit) //限制库存时才去操作库存
                {
                    if (string.IsNullOrWhiteSpace(x.SpecIds))
                    {
                        curGood.stock -= x.Count;
                    }
                    else
                    {
                        curGood.stock -= x.Count;
                        List<EntGoodsAttrDetail> entGoodsAttrDtls = new List<EntGoodsAttrDetail>();
                        curGood.GASDetailList.ForEach(y =>
                        {
                            if (y.id.Equals(x.SpecIds))
                            {
                                y.stock -= x.Count;
                            }
                            entGoodsAttrDtls.Add(y);
                        });
                        //规格库存详情重新赋值
                        curGood.specificationdetail = goodDtlJsonHelper.SToJsonArray(entGoodsAttrDtls);
                    }
                }
                //更新商品库存
                tran.Add(EntGoodsBLL.SingleModel.BuildUpdateSql(curGood, $"stock,specificationdetail,salesCount"));

                //购物车更入商品折扣后价格
                tran.Add($" update EntGoodsCart set price = {x.Price},originalPrice = {x.originalPrice} where id = {x.Id} ");
            });

            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public List<EntGoodsOrder> GetListByAppId_TableNo(string appId, string tableNo)
        {
            List<EntGoodsOrder> order = null;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(tableNo))
            {
                return order;
            }
            string sqlwhere = $"appid=@appid and ordernum like @tableNo";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            paramters.Add(new MySqlParameter("@tableNo", $"%{tableNo}%"));
            order = GetListByParam(sqlwhere, paramters.ToArray());
            return order;
        }

        /// <summary>
        /// 专业版更新订单状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oldState"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool UpdateEntGoodsOrderState(int orderId, int oldState, int state)
        {
            string updateCol = "";
            string sql = "";
            switch (state)
            {
                case (int)MiniAppEntOrderState.待收货:
                    updateCol += $" ,DistributeDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' ";
                    sql = $" update EntGoodsOrder set State = {state}{updateCol} where id = {orderId} and State = {oldState} ";
                    break;

                case (int)MiniAppEntOrderState.交易成功:
                    sql = $" update EntGoodsOrder set State = {state},AcceptDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where id = {orderId} and State = {oldState} ";
                    break;

                default:
                    sql = $" update EntGoodsOrder set State = {state} where id = {orderId} and State = {oldState} ";
                    break;
            }
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql) > 0;
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="timeoutlength"></param>
        public bool updateFoodOrderState(EntGoodsOrder order, int oldState, string updateColNames)
        {
            var updateSql = BuildUpdateSql(order, updateColNames) + $" and state = {oldState} ;";

            var updateLine = SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);

            return updateLine >= 1;
        }

        public List<EntGoodsOrder> GetGiftRecord(int aId, int orderState, int tmpType, string name, string jobNumber, int sex, string createtime, List<TechnicianInfo> technicianList, List<C_UserInfo> userList, int pageSize, int pageIndex, out int recordCount, out int sumPrice)
        {
            string sqlwhere = $"aid={aId} and OrderType=2 and state={orderState} and templateType={tmpType}";
            if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(jobNumber) || sex > 0)
            {
                string tids = string.Join(",", technicianList.Select(t => t.id).ToList());
                sqlwhere += $" and fuserid in ({tids})";
            }
            string uids = string.Join(",", userList.Select(u => u.Id).ToList());
            if (!string.IsNullOrEmpty(uids))
            {
                sqlwhere += $" and UserId in ({uids})";
            }
            if (!string.IsNullOrEmpty(createtime))
            {
                List<string> datestr = createtime.Split('~').ToList();
                if (datestr != null && datestr.Count >= 2)
                {
                    sqlwhere += $" and CreateDate>='{datestr[0]} 00:00:00'and CreateDate<='{datestr[1]} 23:59:59'";
                }
            }

            recordCount = GetCount(sqlwhere);
            sumPrice = GetGiftSumPrice(sqlwhere);
            return GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
        }

        public EntGoodsOrder GetModelByOrdernum(string orderNum)
        {
            string sqlwhere = $"ordernum =@ordernum";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@ordernum", orderNum));
            return GetModel(sqlwhere, parameters.ToArray());
        }

        /// <summary>
        /// 根据收货号码获取下单用户Id集合用逗号分隔
        /// </summary>
        /// <param name="accepterTelePhone">收货号码</param>
        /// <returns></returns>
        public string GetListUserIdByAccepterTelePhone(string accepterTelePhone)
        {
            List<int> listUserId = new List<int>();
            string sqlwhere = $"AccepterTelePhone =@AccepterTelePhone";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@AccepterTelePhone", accepterTelePhone));
            List<EntGoodsOrder> list = GetListByParam(sqlwhere, parameters.ToArray());
            foreach (EntGoodsOrder item in list)
            {
                if (!listUserId.Contains(item.UserId))
                {
                    listUserId.Add(item.UserId);
                }
            }
            if (listUserId.Count > 0)
            {
                return string.Join(",", listUserId);
            }

            return string.Empty;
        }

        /// <summary>
        /// 订单作废需要对应操作库存
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public bool updateStock(EntGoodsOrder order, int oldState)
        {
            TransactionModel tranModel = new TransactionModel();

            tranModel = new TransactionModel();

            //清除商品缓存
            EntGoodsBLL.SingleModel.RemoveEntGoodListCache(order.aId);

            if (order == null)
            {
                return false;
            }
            if (!Enum.IsDefined(typeof(MiniAppEntOrderState), oldState)
                    || !Enum.IsDefined(typeof(MiniAppEntOrderState), order.State))
            {
                return false;
            }
            //订单明细
            List<EntGoodsCart> orderdetaillist = EntGoodsCartBLL.SingleModel.GetList($" State = 1 and GoodsOrderId in ({order.Id})") ?? new List<EntGoodsCart>();
            if (orderdetaillist != null && orderdetaillist.Count > 0)
            {
                //商品
                List<EntGoods> goodlist = EntGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                if (goodlist != null && goodlist.Count > 0)
                {
                    //商品明细
                    var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == order.Id).ToList() : new List<EntGoodsCart>();
                    if (orderdetails != null && orderdetails.Count > 0)
                    {
                        for (int i = 0; i < orderdetails.Count; i++)
                        {
                            //商品
                            var good = goodlist.Where(w => w.id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                            if (good != null)
                            {
                                //销量回滚
                                good.salesCount -= orderdetails[i].Count;

                                if (good.stockLimit)//库存回滚
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
                                            good.specificationdetail = SerializeHelper.SerToJson(GASDetailList);
                                            break;
                                        }
                                    }
                                }
                                //更改商品总库存和规格属性库存
                                tranModel.Add($"update EntGoods set specificationdetail='{good.specificationdetail}',stock={good.stock},salesCount = {good.salesCount} where Id={good.id}");
                            }
                        }
                    }

                    ////订单状态改成已过期
                    tranModel.Add($"update EntGoodsOrder set State={order.State} where Id={order.Id} and State = {oldState} ");

                    return ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
                }
            }
            return false;
        }

        /// <summary>
        /// 订单超过2小时未付款取消订单
        /// </summary>
        /// <param name="timeoutlength"></param>
        public void updateOrderStateForCancle(int timeoutlength = -24)
        {
            //string sql = $" update  miniappfoodgoodsorder set state = {(int)C_Enums.MiniAppEntOrderState.已取消} where state = {(int)C_Enums.MiniAppEntOrderState.待付款} and  (NOW()-INTERVAL 15 minute) <= CreateDate  ";
            TransactionModel tranModel = new TransactionModel();
            //MiniappFoodGoodsAttrBLL goodsattrbll = new MiniappEntGoodsAttrBLL();


            //订单超过24小时取消订单
            List<EntGoodsOrder> orderList = GetList($"State={(int)MiniAppEntOrderState.待付款} and  CreateDate <= (NOW()+INTERVAL {timeoutlength} HOUR) and templatetype={(int)TmpType.小程序专业模板}", 1000, 1);
            if (orderList != null && orderList.Count > 0)
            {
                tranModel = new TransactionModel();
                //订单明细
                List<EntGoodsCart> orderdetaillist = orderList.Any() ? EntGoodsCartBLL.SingleModel.GetList($"State = 1 and GoodsOrderId in ({string.Join(",", orderList.Select(s => s.Id).Distinct())})") : new List<EntGoodsCart>();
                if (orderdetaillist != null && orderdetaillist.Count > 0)
                {
                    //商品
                    List<EntGoods> goodlist = EntGoodsBLL.SingleModel.GetList($"Id in ({string.Join(",", orderdetaillist.Select(s => s.FoodGoodsId).Distinct())})");
                    if (goodlist != null && goodlist.Count > 0)
                    {
                        foreach (var item in orderList)
                        {
                            //商品明细
                            var orderdetails = orderdetaillist.Any() ? orderdetaillist.Where(w => w.GoodsOrderId == item.Id).ToList() : new List<EntGoodsCart>();
                            if (orderdetails != null && orderdetails.Count > 0)
                            {
                                for (int i = 0; i < orderdetails.Count; i++)
                                {
                                    //商品
                                    var good = goodlist.Where(w => w.id == orderdetails[i].FoodGoodsId).FirstOrDefault();
                                    if (good != null)
                                    {
                                        good.salesCount -= orderdetails[i].Count;
                                        if (good.stockLimit)
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
                                                    good.specificationdetail = SerializeHelper.SerToJson(GASDetailList);
                                                    break;
                                                }
                                            }
                                        }
                                        //更改商品总库存和规格属性库存
                                        tranModel.Add($"update EntGoods set specificationdetail='{good.specificationdetail}',stock={good.stock},salesCount = {good.salesCount} where Id={good.id}");
                                    }
                                }
                            }

                            //订单状态改成已过期
                            item.State = (int)MiniAppEntOrderState.已取消;
                            tranModel.Add($"update EntGoodsOrder set State={item.State} where Id={item.Id} and State = {(int)MiniAppEntOrderState.待付款} ;");

                            //事务内某行sql执行受影响行数为0,会回滚整个事务
                            if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                            {
                                //object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(item, SendTemplateMessageTypeEnum.专业版订单取消通知);
                                //TemplateMsg_Miniapp.SendTemplateMessage(item, SendTemplateMessageTypeEnum.专业版订单取消通知, TmpType.小程序专业模板, orderData);

                                EntGoodsOrderLogBLL.SingleModel.AddLog(item.Id, 0, "系统自动将超过15分钟未付款的订单取消成功！");
                                object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(item, SendTemplateMessageTypeEnum.专业版订单取消通知);
                                TemplateMsg_Miniapp.SendTemplateMessage(item.UserId, SendTemplateMessageTypeEnum.专业版订单取消通知, TmpType.小程序专业模板, orderData);
                            }
                            else
                            {
                                EntGoodsOrderLogBLL.SingleModel.AddLog(item.Id, 0, "系统自动将超过15分钟未付款的订单取消失败！");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 专业版： 成功支付下单后操作 : 1.打印订单, 2.发送模板消息
        /// </summary>
        /// <param name="dbOrder"></param>
        public void AfterPayOrderBySaveMoney(EntGoodsOrder dbOrder)
        {
            Task.Factory.StartNew(() =>
            {
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModel(dbOrder.aId);
                Account account = null;
                int tId = 0;
                if (app != null)
                {
                    account = AccountBLL.SingleModel.GetModel(app.AccountId);
                    tId = app.TId;
                }

                //打印机打印内容
                string printContent = PrinterHelper.entPrintOrderContent(dbOrder);
                List<FoodPrints> prints = FoodPrintsBLL.SingleModel.GetList($" appId = {dbOrder.aId} and state >=0 ") ?? new List<FoodPrints>();
                PrinterHelper.printContent(prints, printContent, dbOrder.Id, tId, account);

                //发给用户
                object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(dbOrder, SendTemplateMessageTypeEnum.专业版订单支付成功通知);
                string orderUrl = $"pages/good/goodOlt?dbOrder={dbOrder.Id}&check=true";
                TemplateMsg_Miniapp.SendTemplateMessage(dbOrder.UserId, SendTemplateMessageTypeEnum.专业版订单支付成功通知, TmpType.小程序专业模板, orderData, orderUrl);
                //发给用户提示拼团已经成功
                if (dbOrder.GroupId > 0)
                {
                    EntGroupSponsor group = EntGroupSponsorBLL.SingleModel.GetModel(dbOrder.GroupId);
                    if (group != null)
                    {
                        //判断是否开团成功
                        List<EntGoodsOrder> curGroupOrders = GetList($"ordertype=3 and aId = {dbOrder.aId} and groupid={dbOrder.GroupId} and state in({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.待自取})");
                        if (curGroupOrders != null && curGroupOrders.Count == group.GroupSize) //开团成功
                        {
                            curGroupOrders.ForEach(g =>
                            {
                                object groupData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(g, SendTemplateMessageTypeEnum.拼团拼团成功提醒);
                                string groupOrderUrl = $"pages/group2/group2Dlt?id={dbOrder.Id}";
                                TemplateMsg_Miniapp.SendTemplateMessage(g.UserId, SendTemplateMessageTypeEnum.拼团拼团成功提醒, TmpType.小程序专业模板, groupData, groupOrderUrl);
                            });
                        }
                    }
                }
                //更新预约订单状态
                if (dbOrder.OrderType == (int)EntOrderType.预约订单 && dbOrder.ReserveId > 0)
                {
                    TransactionModel _tranModel = new TransactionModel();
                    
                    FoodReservation reservation = FoodReservationBLL.SingleModel.GetModel(dbOrder.ReserveId);
                    _tranModel.Add(FoodReservationBLL.SingleModel.UpdateToPayEnt(reservation));
                    FoodReservationBLL.SingleModel.ExecuteTransactionDataCorect(_tranModel.sqlArray, _tranModel.ParameterArray);
                }
                if (!string.IsNullOrWhiteSpace(dbOrder.attribute))
                {
                    try
                    {
                        dbOrder.attrbuteModel = JsonConvert.DeserializeObject<EntGoodsOrderAttr>(dbOrder.attribute);
                    }
                    catch (Exception error)
                    {
                        LogHelper.WriteError(this.GetType(), error);
                    }
                }
                //插入秒杀支付记录
                if (dbOrder.attrbuteModel?.flashItemId > 0 && !FlashDealPaymentBLL.SingleModel.PayByEntOrder(order: dbOrder))
                {
                    LogHelper.WriteInfo(this.GetType(), $"秒杀支付记录写入失败：{JsonConvert.SerializeObject(dbOrder)}");
                }
                //发给商家
                TemplateMsg_Gzh.SendEntPaySuccessTemplateMessage(dbOrder);
            });
        }

        //自动完成订单（10天）
        public void updateOrderStateForComplete(int timeoutlength = -10)
        {
            //  TransactionModel tranModel = new TransactionModel();
            //MiniappFoodGoodsOrderLogBLL orderLogBll = new MiniappFoodGoodsOrderLogBLL();

            List<int> updateByOldStateList = new List<int>();
            updateByOldStateList.Add((int)MiniAppEntOrderState.待收货);

            List<EntGoodsOrder> list = GetList($" State in ({string.Join(", ", updateByOldStateList)}) and  DistributeDate <= (NOW()+INTERVAL {timeoutlength * 60 * 24} MINUTE ) and templatetype={(int)TmpType.小程序专业模板}");
            string updateSql = $" update entgoodsorder set State = {(int)MiniAppEntOrderState.交易成功},AcceptDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' where  State in ({string.Join(",", updateByOldStateList)}) and  DistributeDate <= (NOW()+interval {timeoutlength * 60 * 24} MINUTE) and templatetype={(int)TmpType.小程序专业模板}";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateSql, null);

            //var userlogBll = new  TemplateMsg_UserLogBLL();


            if (list != null && list.Count > 0)
            {
                string orderIds = string.Join(",",list.Select(s=>s.Id));
                List<EntGoodsCart> entGoodsCartList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderIds);
                list.ForEach(order =>
                {
                    //加销量
                    List<EntGoodsCart> list2 = entGoodsCartList?.Where(w=>w.GoodsOrderId==order.Id).ToList();
                    list2?.Select(x => x.FoodGoodsId).Distinct().ToList().ForEach(x =>
                    {
                        var salesCount1 = list2.Where(y => y.FoodGoodsId == x).Sum(y => y.Count);
                        SqlMySql.ExecuteNonQuery(EntGoodsBLL.SingleModel.connName, CommandType.Text, $" update entgoods set salesCount = salesCount + {salesCount1} where id = {x} ");
                    });

                    //会员加消费金额
                    if (!VipRelationBLL.SingleModel.updatelevel(order.UserId, "entpro", order.BuyPrice))
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception(" 用户自动升级逻辑异常(订单发货后 超过10天,系统自动完成订单)" + order.Id));
                    }

                    //消费符合积分规则赠送积分
                    if (!ExchangeUserIntegralBLL.SingleModel.AddUserIntegral(order.UserId, order.aId, 0, order.Id))
                        log4net.LogHelper.WriteError(GetType(), new Exception("赠送积分失败(订单发货后 超过10天,系统自动完成订单)" + order.Id));

                    //确认收货后 判断该订单购物车里面是否是分销产生的 如果购物车里的产品佣金比例不为零则需要操作分销相关的

                    #region 分销相关
                    List<EntGoodsCart> listEntGoodsCart = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {order.Id} ");
                    PayDistributionMoney(listEntGoodsCart, order);

                    #endregion 分销相关

                    EntGoodsOrderLogBLL.SingleModel.AddLog(order.Id, 0, "订单发货后 超过10天,系统自动完成订单");
                });
            }
        }

        public TransactionModel addSalesCount(int orderId, TransactionModel _tranModel)
        {
            List<EntGoodsCart> orderDetailList = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={orderId}");
            if (orderDetailList != null && orderDetailList.Count > 0)
            {
                //记录订单支付日志
                List<EntGoods> orderGoodsList = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", orderDetailList.Select(x => x.FoodGoodsId).Distinct())})");
                //加商品销量
                orderGoodsList.ForEach(x =>
                {
                    var shopQty = orderDetailList.Where(y => y.FoodGoodsId == x.id).Sum(y => y.Count);

                    _tranModel.Add($" Update entgoods set salesCount = salesCount + {shopQty} where Id = {x.id} ;");
                });
            }
            return _tranModel;
        }

        /// <summary>
        ///  订单退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldState"></param>
        /// <param name="BuyMode">默认微信支付</param>
        /// <param name="isPartOut">是否部分退款</param>
        /// <returns></returns>
        public bool outOrder(EntGoodsOrder item, int oldState, int BuyMode = (int)miniAppBuyMode.微信支付, int? newState = null, bool isPartOut = false, string remark = "商家操作退款")
        {
            //重新加回库存
            if (updateStock(item, oldState))
            {
                int money = isPartOut ? item.refundFee : item.BuyPrice;//兼容多版本，目前只有专业版订单有部分退款
                item.refundFee = money;

                TransactionModel tran = new TransactionModel();
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
                            CityMorders order = new CityMordersBLL().GetModel(item.OrderId);
                            item.State = (int)MiniAppEntOrderState.退款中;
                            if (newState.HasValue)
                            {
                                item.State = newState.Value;
                            }
                            if (order == null)
                            {
                                item.State = (int)MiniAppEntOrderState.退款失败;
                                Update(item, "State,outOrderDate,Remark,refundFee");
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
                            tran.Add(new ReFundQueueBLL().BuildAddSql(reModel));
                        }
                        tran.Add(base.BuildUpdateSql(item, "State,outOrderDate,Remark,refundFee"));
                        if (base.ExecuteTransactionDataCorect(tran.sqlArray))
                        {
                            //发给用户退款通知
                            object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(item, SendTemplateMessageTypeEnum.专业版订单退款通知, remark);
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
                    var r = XcxAppAccountRelationBLL.SingleModel.GetModel(item.aId);
                    if (r == null)
                        return false;

                    var saveMoneyUser = SaveMoneySetUserBLL.SingleModel.getModelByUserId(r.AppId, item.UserId);
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
                    tran.Add($" update EntGoodsOrder set State = {item.State },outOrderDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',Remark = @Remark,refundFee={item.refundFee} where Id = {item.Id} and state <> {item.State} ; ",
                        new MySqlParameter[] { new MySqlParameter("@Remark", item.Remark) });//防止重复退款

                    //记录订单储值支付退款日志
                    tran.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog() { GoodsOrderId = item.Id, UserId = item.UserId, LogInfo = $" 储值支付订单退款成功：{item.refundFee * 0.01} 元 ", CreateDate = DateTime.Now }));
                    bool isSuccess = ExecuteTransaction(tran.sqlArray, tran.ParameterArray);

                    if (isSuccess)
                    {
                        //发给用户退款通知
                        object orderData = TemplateMsg_Miniapp.EnterpriseGetTemplateMessageData(item, SendTemplateMessageTypeEnum.专业版订单退款通知, remark);

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
        public bool updateOutOrderState()
        {
            
            

            

            TransactionModel tranModel = new TransactionModel();
            List<EntGoodsOrder> itemList = GetList($" State in ({(int)MiniAppEntOrderState.退款中},{(int)MiniAppEntOrderState.退款失败}) and templatetype in ({(int)TmpType.小程序专业模板},{(int)TmpType.小程序足浴模板},{(int)TmpType.小程序多门店模板}) and outOrderDate <= (NOW()-interval 17 second) and BuyMode = 1 ", 1000, 1) ?? new List<EntGoodsOrder>();
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
                                    x.State = (int)MiniAppEntOrderState.退款成功;
                                    tranModel.Add(BuildUpdateSql(x, "State"));
                                }
                                else
                                {
                                    x.State = (int)MiniAppEntOrderState.退款失败;
                                    tranModel.Add(BuildUpdateSql(x, "State"));
                                }
                            }
                        }
                    });
                }
            }
            bool isSuccess = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            return isSuccess;
        }

        /// <summary>
        /// 跟进 退款状态 (退款是否成功)
        /// </summary>
        /// <returns></returns>
        public bool UpdateReturnOrderState()
        {
            TransactionModel tranModel = new TransactionModel();
            string sql = $@"select eo.*,r.result_code as refundcode from entgoodsorder eo left join citymorders co on eo.orderid = co.id left join ReFundResult r on r.transaction_id = co.trade_no where State in ({(int)MiniAppEntOrderState.退款中}) and eo.templatetype in ({(int)TmpType.小程序专业模板},{(int)TmpType.小程序足浴模板},{(int)TmpType.小程序多门店模板}) and eo.outOrderDate <= (NOW() - interval 17 second) and eo.BuyMode = 1";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    EntGoodsOrder model = base.GetModel(dr);
                    if (dr["refundcode"].ToString() == "SUCCESS")
                    {
                        model.State = (int)MiniAppEntOrderState.退款成功;
                    }
                    else
                    {
                        model.State = (int)MiniAppEntOrderState.退款失败;
                    }
                    tranModel.Add(BuildUpdateSql(model, "State"));
                }
            }
            bool isSuccess = ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            return isSuccess;
        }

        /// <summary>
        ///  重新退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="oldState"></param>
        /// <param name="BuyMode">默认微信支付</param>
        /// <returns></returns>
        public void ReturnOrderAgain(EntGoodsOrder order, ref string msg)
        {
            TransactionModel tranModel = new TransactionModel();
            ReFundQueueBLL reFundQueueBLL = new ReFundQueueBLL();
            order.State = (int)MiniAppEntOrderState.退款成功;
            order.outOrderDate = DateTime.Now;

            if (order.BuyMode == (int)miniAppBuyMode.微信支付)
            {
                if (order.BuyPrice > 0)
                {
                    order.State = (int)MiniAppEntOrderState.退款中;
                    ReFundQueue reFundQueue = reFundQueueBLL.GetModelByOrderId(order.OrderId);
                    if (reFundQueue == null)
                    {
                        msg = "退款：退款队列数据不存在";
                        return;
                    }

                    reFundQueue.state = 0;
                    tranModel.Add(reFundQueueBLL.BuildUpdateSql(reFundQueue, "state"));
                }
                tranModel.Add($"update EntGoodsOrder set state={order.State},outOrderDate='{order.outOrderDate}' where id={order.Id}");
            }
            if (!ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
            {
                msg = "退款：操作失败";
            }
        }

        #region 足浴

        /// <summary>
        /// 足浴版验证技师是否有交易成功过
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="technicianId"></param>
        /// <returns></returns>
        public bool ValidIsSuccess(int aId, int userId, int technicianId)
        {
            int count = GetCount($"aid={aId} and OrderType=2 and userid={userId} and state={(int)MiniAppEntOrderState.交易成功} and fuserid={technicianId}");
            return count > 0;
        }

        /// <summary>
        /// 足浴版获取送花总额
        /// </summary>
        /// <param name="sqlwhere"></param>
        /// <returns></returns>
        public int GetGiftSumPrice(string sqlwhere)
        {
            int result = 0;
            string sql = $"select sum(BuyPrice) as sum from entgoodsorder where {sqlwhere}";
            DataSet ds = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (ds.Tables.Count <= 0) return result;
            DataTable dt = ds.Tables[0];
            result = Convert.ToInt32(dt.Rows[0]["sum"] != DBNull.Value ? dt.Rows[0]["sum"] : 0);
            return result;
        }

        /// <summary>
        /// 足浴版根据aid、id获取订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public EntGoodsOrder GetModelByAidAndId(int appId, int id, int orderType)
        {
            return GetModel($"aid={appId} and id={id} and OrderType={orderType}");
        }

        /// <summary>
        /// 足浴版获取未失败的订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public EntGoodsOrder GetModelByOrderIdAndAid(int orderId, int appId, int orderType)
        {
            return GetModel($"id={orderId} and aid={appId} and OrderType={orderType} and state>{(int)MiniAppEntOrderState.待付款}");
        }

        public List<EntGoodsOrder> GetFootbathOrderList(int aid, int userId, int orderType, int state, int pageSize, int pageIndex)
        {
            string sqlwhere = $"aid={aid} and OrderType !=2 and UserId={userId}";
            if (orderType > 0)//orderType：0所有 1订单
            {
                sqlwhere = $"aid={aid} and OrderType =0 and UserId={userId}";
            }

            if (state > -1)
            {
                sqlwhere += $" and state={state}";
            }
            return GetList(sqlwhere, pageSize, pageIndex);
        }

        /// <summary>
        /// 超时没有进行服务的预订单状态改为：（待服务　－＞　已超时）
        /// </summary>
        public void timeOutFootBathOrder()
        {
            string msg = "1";
            try
            {
                msg += 2;
                string findTimeOutOrderSql = $@"select b.* from entgoodscart as a Inner JOIN entgoodsorder as b on a.GoodsOrderId = b.Id  where b.templatetype = {(int)TmpType.小程序足浴模板} and a.reservationTime < now() and b.state = {(int)MiniAppEntOrderState.待服务}";
                List<EntGoodsOrder> entGoodOrders = new List<EntGoodsOrder>();
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, findTimeOutOrderSql, null))
                {
                    while (dr.Read())
                    {
                        entGoodOrders.Add(GetModel(dr));
                    }
                }
                msg += 3;
                if (entGoodOrders != null && entGoodOrders.Any())
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "足浴订单超时没有开始服务");
                    msg += 5;
                    //更新超时预订单状态为已超时
                    string updateTimeOutOrderSql = $" update entgoodsorder set state = {(int)MiniAppEntOrderState.已超时} where id in ({string.Join(",", entGoodOrders.Select(x => x.Id))}) and state = {(int)MiniAppEntOrderState.待服务} ";
                    SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateTimeOutOrderSql, null);
                    msg += 4;

                    List<EntGoodsCart> reserveInfos = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId IN ({string.Join(",", entGoodOrders.Select(x => x.Id))}) ");
                    int curTechnicianInfoId = 0;
                    int curServiceInfoId = 0;
                    string addServiceCountSql = "";
                    entGoodOrders.ForEach(x =>
                    {
                        msg += 6;
                        //如果是订单超时,需要给服务技师加上接单数
                        if (x.OrderType == (int)EntOrderType.订单)
                        {
                            curTechnicianInfoId = (reserveInfos.Where(y => y.GoodsOrderId == x.Id)?.First()?.technicianId ?? 0);
                            curServiceInfoId = (reserveInfos.Where(y => y.GoodsOrderId == x.Id)?.First()?.FoodGoodsId ?? 0);

                            if (curTechnicianInfoId > 0)
                            {
                                addServiceCountSql = $" update TechnicianInfo set servicecount = servicecount + 1 where id = {curTechnicianInfoId} ;";
                                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, addServiceCountSql, null);
                            }
                            if (curServiceInfoId > 0)
                            {
                                addServiceCountSql = $" update EntGoods set salesCount = salesCount + 1 where id = {curServiceInfoId} ;";
                                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, addServiceCountSql, null);
                            }
                        }
                        msg += 6;
                        //预订单超时提醒商家:订单超时没有处理
                        if (x.OrderType == (int)EntOrderType.预约订单)
                        {
                            msg += 8;
                            TemplateMsg_Gzh.SendReserveTimeOutTemplateMessage(x);
                        }
                        msg += 7;

                        //通知顾客订单超时商家未处理
                        object objData = TemplateMsg_Miniapp.FootbathGetTemplateMessageData(x, SendTemplateMessageTypeEnum.足浴预约超时通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(x.UserId, SendTemplateMessageTypeEnum.足浴预约超时通知, TmpType.小程序足浴模板, objData);
                        msg += 9;
                    });
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(GetType(), msg + ex.Message);
            }
        }

        public List<EntGoodsOrder> GetGiftRecord(int aid, int userId, int pageSize, int pageIndex, out int recordCount)
        {
            string sqlwhere = $"aid={aid} and OrderType=2 and userid={userId} and state={(int)MiniAppEntOrderState.交易成功} and templateType={(int)TmpType.小程序足浴模板}";
            recordCount = GetCount(sqlwhere);
            return GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
        }

        /// <summary>
        /// 提前半小时提醒顾客去消费（1分钟跑一次,不然会多发）
        /// </summary>
        public void timeReadyFootBathOrder()
        {
            try
            {
                string findTimeReadyOrderSql = $@"select b.* from entgoodscart as a INNER JOIN entgoodsorder as b on a.GoodsOrderId = b.Id
								where b.templatetype = {(int)TmpType.小程序足浴模板} and b.OrderType = {(int)EntOrderType.订单} and b.state = {(int)MiniAppEntOrderState.待服务}
								and DATE_FORMAT(DATE_ADD(a.reservationTime, INTERVAL -30 MINUTE),'%Y-%m-%d %H:%i') = DATE_FORMAT(now(),'%Y-%m-%d %H:%i') ";

                List<EntGoodsOrder> entGoodOrders = new List<EntGoodsOrder>();
                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, findTimeReadyOrderSql, null))
                {
                    while (dr.Read())
                    {
                        entGoodOrders.Add(GetModel(dr));
                    }
                }
                if (entGoodOrders != null && entGoodOrders.Any())
                {
                    entGoodOrders.ForEach(x =>
                    {
                        //提前半小时发送模板消息提醒顾客去消费
                        object objData = TemplateMsg_Miniapp.FootbathGetTemplateMessageData(x, SendTemplateMessageTypeEnum.足浴已预约活动开始提醒);
                        TemplateMsg_Miniapp.SendTemplateMessage(x.UserId, SendTemplateMessageTypeEnum.足浴已预约活动开始提醒, TmpType.小程序足浴模板, objData);
                    });
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(GetType(), ex.Message);
            }
        }

        /// <summary>
        /// 服务结束（预定时间）的订单自动改为已完成（服务中 -＞　已完成）
        /// </summary>
        public void timeOverFootBathOrder()
        {
            DateTime dt1 = DateTime.Now;
            string findTimeOverOrderSql = $@"select b.* from entgoodscart as a
												inner join entgoodsorder as b on a.GoodsOrderId = b.Id
												inner join entgoods c on a.foodgoodsid = c.id
										where b.templatetype = { (int)TmpType.小程序足浴模板}
										and b.OrderType = { (int)EntOrderType.订单} and DATE_ADD(a.reservationTime, INTERVAL c.ServiceTime MINUTE) < now()
										and b.state = { (int)MiniAppEntOrderState.服务中} ";

            List<EntGoodsOrder> entGoodOrders = new List<EntGoodsOrder>();
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, findTimeOverOrderSql, null))
            {
                while (dr.Read())
                {
                    entGoodOrders.Add(GetModel(dr));
                }
            }

            if (entGoodOrders != null && entGoodOrders.Any())
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "足浴订单服务到时间自动完成");
                //更新超时预订单状态为已完成
                string updateTimeOverOrderSql = $" update entgoodsorder set state = {(int)MiniAppEntOrderState.已完成} where id in ({string.Join(",", entGoodOrders.Select(x => x.Id))}) and state = {(int)MiniAppEntOrderState.服务中} ";

                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, updateTimeOverOrderSql, null);

                //服务完成,对应更新技师状态为休息中
                EntGoodsCart curCart = null;
                EntGoods curService = null;
                List<EntGoodsCart> cars = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId in ({string.Join(",", entGoodOrders.Select(x => x.Id))}) ");
                TechnicianInfo curTechicinaInfo = null;
                
                string goodsIds = string.Join(",", cars.Select(s=>s.FoodGoodsId).Distinct());
                List<EntGoods> entGodosList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);

                string technicianIds = string.Join(",",cars.Select(s=>s.technicianId).Distinct());
                List<TechnicianInfo> techniacianInfoList = TechnicianInfoBLL.SingleModel.GetListByIds(technicianIds);

                entGoodOrders.ForEach(x =>
                {
                    curCart = cars.Where(y => y.GoodsOrderId == x.Id)?.First();
                    if (curCart != null)
                    {
                        curService = entGodosList?.FirstOrDefault(f=>f.id == curCart.FoodGoodsId);
                        curTechicinaInfo = techniacianInfoList?.FirstOrDefault(f=>f.id == curCart.technicianId);
                        if (curTechicinaInfo != null && curService != null)
                        {
                            curTechicinaInfo.state = (int)TechnicianState.休息中;
                            curTechicinaInfo.serviceCount++;
                            curService.salesCount++;

                            TechnicianInfoBLL.SingleModel.Update(curTechicinaInfo, "serviceCount,State");
                            EntGoodsBLL.SingleModel.Update(curService, "salesCount");
                        }
                    }
                    VipRelationBLL.SingleModel.updatelevel(x.UserId, "footbath");
                });
            }
        }

        /// <summary>
        /// 获取足浴预订列表数据
        /// </summary>
        /// <param name="appId">小程序appid</param>
        /// <param name="name">客户名称</param>
        /// <param name="phone">客户号码</param>
        /// <param name="servicetime">服务时间</param>
        /// <param name="sex">客户性别</param>
        /// <param name="payState">支付方式</param>
        /// <param name="state">预订状态</param>
        /// <param name="roomIds">包间id</param>
        /// <param name="serviceIds">服务项目id</param>
        /// <param name="technicianIds">技师id</param>
        /// <param name="pageIndex">分页页码</param>
        /// <param name="pageSize">行数</param>
        /// <returns></returns>
        public List<EntGoodsOrder> GetFootbathOrderList(int appId, string name, string phone, string servicetime, int payState, int state, string roomIds, string serviceIds, string technicianIds, int pageIndex, int pageSize, int orderType, out int recordCount, string ConfDate = "", string OrderNum = "")
        {
            string sql = string.Empty;
            string countsql = string.Empty;
            try
            {
                recordCount = 0;
                List<EntGoodsOrder> list = new List<EntGoodsOrder>();
                List<MySqlParameter> parameters = new List<MySqlParameter>();
                sql = $"select b.* from entgoodscart as a LEFT JOIN entgoodsorder as b on a.GoodsOrderId = b.Id";
                string sqlwhere = $" where b.aid={appId} and b.OrderType={orderType}";
                if (!string.IsNullOrEmpty(name))
                {
                    sqlwhere += $" and b.AccepterName like @name";
                    parameters.Add(new MySqlParameter("@name", $"%{name}%"));
                }
                if (!string.IsNullOrEmpty(OrderNum))
                {
                    sqlwhere += $" and b.OrderNum like @OrderNum";
                    parameters.Add(new MySqlParameter("@OrderNum", $"%{OrderNum}%"));
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    sqlwhere += $" and b.AccepterTelePhone like @phone";
                    parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
                }
                if (!string.IsNullOrWhiteSpace(servicetime))
                {
                    List<string> datestr = servicetime.Split('~').ToList();
                    if (datestr != null && datestr.Count >= 2)
                    {
                        sqlwhere += $" and a.reservationtime>='{datestr[0]} 00:00:00'and a.reservationtime<='{datestr[1]} 23:59:59'";
                    }
                }
                if (!string.IsNullOrWhiteSpace(ConfDate))
                {
                    List<string> datestr = ConfDate.Split('~').ToList();
                    if (datestr != null && datestr.Count >= 2)
                    {
                        sqlwhere += $" and b.ConfDate>='{datestr[0]} 00:00:00'and b.ConfDate<='{datestr[1]} 23:59:59'";
                    }
                }
                if (payState > -1)
                {
                    sqlwhere += $" and b.BuyMode={payState}";
                }
                if (state > -6)
                {
                    sqlwhere += $" and b.State={state}";
                }
                if (!string.IsNullOrEmpty(roomIds))
                {
                    sqlwhere += $" and a.roomno in ({roomIds})";
                }
                if (!string.IsNullOrEmpty(serviceIds))
                {
                    List<string> typeidSplit = serviceIds.SplitStr(",");
                    if (typeidSplit.Count > 0)
                    {
                        typeidSplit = typeidSplit.Select(p => p = "FIND_IN_SET('" + p + "',GoodsGuid)").ToList();
                        sqlwhere += $" and (" + string.Join(" or ", typeidSplit) + ")";
                    }
                }
                if (!string.IsNullOrEmpty(technicianIds))
                {
                    sqlwhere += $" and a.technicianId in({technicianIds})";
                }
                sql += $"{sqlwhere} group by b.id order by b.id desc limit {(pageIndex - 1) * pageSize},{pageSize}";
                DataSet ds = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, sql, parameters.ToArray());
                if (ds.Tables.Count <= 0)
                    return list;
                DataTable dt = ds.Tables[0];
                if (dt == null || dt.Rows.Count <= 0)
                    return list;
                list = TransOrderEntity(dt);
                countsql = $"select count(*) as count from (select b.* from entgoodscart as a LEFT JOIN entgoodsorder as b on a.GoodsOrderId = b.Id {sqlwhere} group by b.id) as c  ";
                DataSet dc = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, countsql, parameters.ToArray());
                DataTable dct = dc.Tables[0];
                recordCount = Convert.ToInt32(dct.Rows[0]["count"]);

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(sql + "||" + countsql + "||" + ex.Message);
            }
        }

        /// <summary>
        /// 获取送礼数
        /// </summary>
        /// <param name="fuserId"></param>
        /// <param name="ids"></param>
        /// <param name="orderState"></param>
        /// <param name="temType"></param>
        /// <returns></returns>
        public int GetGiftCount(int fuserId, string ids, int orderState, int temType)
        {
            
            int orderCount = 0;
            if (!string.IsNullOrEmpty(ids))
            {
                string findGiftOrderCountSql = $"select count(0) from entgoodsorder where FuserId = {fuserId} and GoodsGuid in ({ids}) and state = {orderState} And TemplateType = {temType};";
                Int32.TryParse(SqlMySql.ExecuteScalar(connName, CommandType.Text, findGiftOrderCountSql, null).ToString(), out orderCount);
            }
            
            return orderCount;
        }

        public int GetServiceCountByTechinicianId(int technicianId, int orderState)
        {
            string findOrderCountSql = $"select count(0) from entgoodscart c INNER JOIN entgoodsorder o on c.goodsorderid = o.Id where c.state = 1 and o.state = {orderState} and c.technicianId = {technicianId}";
            int orderCount = 0;
            //待服务
            Int32.TryParse(SqlMySql.ExecuteScalar(connName, CommandType.Text, findOrderCountSql, null).ToString(), out orderCount);
            return orderCount;
        }

        /// <summary>
        /// 转换足浴订单实体
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private List<EntGoodsOrder> TransOrderEntity(DataTable dt)
        {
            
            List<EntGoodsOrder> list = new List<EntGoodsOrder>();
            foreach (DataRow row in dt.Rows)
            {
                EntGoodsOrder model = new EntGoodsOrder();
                model.aId = Convert.ToInt32(row["aid"]);
                model.BuyPrice = Convert.ToInt32(row["BuyPrice"]);
                model.CreateDate = Convert.ToDateTime(row["CreateDate"]);
                model.AccepterName = row["AccepterName"].ToString();
                model.Id = Convert.ToInt32(row["Id"]);
                model.BuyMode = Convert.ToInt32(row["BuyMode"]);
                model.AccepterTelePhone = row["AccepterTelePhone"].ToString();
                model.Message = row["Message"].ToString();
                model.PayDate = Convert.ToDateTime(row["PayDate"].ToString());
                model.UserId = Convert.ToInt32(row["UserId"]);
                model.State = Convert.ToInt32(row["state"]);
                model.OrderNum = row["OrderNum"].ToString();
                model.Remark = row["remark"].ToString();
                model.Message = row["message"].ToString();
                model.goodsCarts = EntGoodsCartBLL.SingleModel.GetList($"State=1 and GoodsOrderId={model.Id}");
                model.ConfDate = Convert.ToDateTime(row["ConfDate"].ToString());
                model.ReducedPrice = Convert.ToInt32(row["ReducedPrice"]);
                if (model.goodsCarts != null && model.goodsCarts.Count > 0)
                {
                    string goodsIds = string.Join(",",model.goodsCarts.Select(s=>s.FoodGoodsId).Distinct());
                    List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);

                    string technicianIds = string.Join(",",model.goodsCarts.Select(s=>s.technicianId).Distinct());
                    List<TechnicianInfo> technicianInfoList = TechnicianInfoBLL.SingleModel.GetListByIds(technicianIds);

                    model.goodsCarts.ForEach(g =>
                    {
                        g.roomName = "未分配";
                        if (g.FoodGoodsId > 0)
                        {
                            g.goodsMsg = entGoodsList?.FirstOrDefault(f=>f.id == g.FoodGoodsId
                            );
                        }
                        if (g.technicianId > 0)
                        {
                            TechnicianInfo technicianInfo = technicianInfoList?.FirstOrDefault(f=>f.id==g.technicianId && f.state>-1);
                            g.technicianName = technicianInfo == null ? "未分配" : technicianInfo.jobNumber;
                        }
                        if (g.roomNo > 0)
                        {
                            EntGoodType roomInfo = EntGoodTypeBLL.SingleModel.GetModel($"id={g.roomNo} and type={(int)GoodProjectType.足浴版包间分类} and state>0");
                            g.roomName = roomInfo == null ? "未分配" : roomInfo.name;
                        }
                        if (!string.IsNullOrEmpty(g.extraConfig))
                        {
                            g.footBathConfig = JsonConvert.DeserializeObject<ExtraConfig>(g.extraConfig);
                        }
                    });
                }

                list.Add(model);
            }
            return list;
        }

        /// <summary>
        /// 足浴版生成用户预订单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="goodsCart"></param>
        /// <returns></returns>
        public bool CreateFootbathOrder(EntGoodsOrder orderModel, EntGoodsCart goodsCart)
        {
            //生成订单明细
            goodsCart.Id = Convert.ToInt32(EntGoodsCartBLL.SingleModel.Add(goodsCart));//GoodsOrderId,state
            if (goodsCart.Id <= 0)
            {
                return false;
            }
            goodsCart.State = 1;

            var TranModel = new TransactionModel();
            TranModel.Add(BuildAddSql(orderModel));
            TranModel.Add($"update EntGoodsCart set state={goodsCart.State},GoodsOrderId=(select last_insert_id()) where id={goodsCart.Id}");
            bool result = ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray);
            if (result)
            {
                goodsCart = EntGoodsCartBLL.SingleModel.GetModel($"id={goodsCart.Id}");
                orderModel.Id = goodsCart.GoodsOrderId;
            }
            return result;
        }

        /// <summary>
        /// 足浴版退款
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public bool outOrder(string appId, EntGoodsOrder orderInfo, ServiceTime serviceTime)
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
                    Update(orderInfo, "State,outOrderDate,Remark");
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
                    new ReFundQueueBLL().Add(reModel);
                    result = Update(orderInfo, "State,outOrderDate");
                    return result;
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"{ex.Message} xxxxxxxxxxxxxxxx小程序足浴退款订单插入队列失败 ID={orderInfo.Id}");
                }
            }
            return result;
        }

        /// <summary>
        /// 根据支付时间获取送花记录
        /// </summary>
        /// <param name="technicianId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public List<EntGoodsOrder> GetGiftRecordByPayDate(int technicianId, string startDate, string endDate, int pageSize, int pageIndex)
        {
            string findSqlWhere = $" FuserId = '{technicianId}' AND STATE = {(int)MiniAppEntOrderState.交易成功} And TemplateType = {(int)TmpType.小程序足浴模板} And IFNULL(GoodsGuid,'') != ''  ";
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                findSqlWhere += $" And PayDate >= '{startDate}' And PayDate <= '{endDate}' ";
            }
            return GetList(findSqlWhere, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取送礼的用户数
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="technicianId"></param>
        /// <returns></returns>
        public int GetGiveGiftUserCount(string startDate, string endDate, int technicianId)
        {
            string findUserCountSql = $"select count(0) as userCount from(select * from entgoodsorder where FuserId = '{technicianId}' AND STATE = {(int)MiniAppEntOrderState.交易成功} And TemplateType = {(int)TmpType.小程序足浴模板} And IFNULL(GoodsGuid,'') != '' group by userid) as g ";//用户数量
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                findUserCountSql += $" And PayDate >= '{startDate}' And PayDate <= '{endDate}' ";
            }
            return Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, findUserCountSql, null));
        }

        /// <summary>
        /// 根据支付时间获取送花总额
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="technicianId"></param>
        /// <returns></returns>
        public string GetGiftSumPriceByPayDate(string startDate, string endDate, int technicianId)
        {
            string findGiftsPriceSql = $" select sum(IFNULL(buyprice,0)) as price FROM entgoodsorder where FuserId = '{technicianId}' AND STATE = {(int)MiniAppEntOrderState.交易成功} And TemplateType = {(int)TmpType.小程序足浴模板} And IFNULL(GoodsGuid,'') != '' ";//价值
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                findGiftsPriceSql += $" And PayDate >= '{startDate}' And PayDate <= '{endDate}' ";
            }
            int priceSum = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, findGiftsPriceSql, null));
            return (priceSum * 0.01).ToString("0.00");
        }

        /// <summary>
        /// 获取足浴订单总数
        /// </summary>
        /// <param name="technicianId"></param>
        /// <param name="states"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        public int GetFootbathOrderCount(int technicianId, string states, int orderType)
        {
            string findSql = $"select count(0) as orderCount from entgoodsorder o inner join entgoodscart c on o.id = c.GoodsOrderId where c.technicianId = {technicianId} and c.state = 1 and o.state in ({states}) and o.OrderType = {orderType} ";
            int orderCount = 0;
            Int32.TryParse(SqlMySql.ExecuteScalar(connName, CommandType.Text, findSql, null).ToString(), out orderCount);
            return orderCount;
        }

        /// <summary>
        /// 根据时间获取送礼数
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="technicianId"></param>
        /// <returns></returns>
        public int GetGiftRecordCountByPayDate(string startDate, string endDate, int technicianId)
        {
            string findGiftsCountSql = $" select count(0) as qty FROM entgoodsorder where FuserId = {technicianId} AND STATE = {(int)MiniAppEntOrderState.交易成功} And TemplateType = {(int)TmpType.小程序足浴模板} And IFNULL(GoodsGuid,'') != '' ";
            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                findGiftsCountSql += $" And PayDate >= '{startDate}' And PayDate <= '{endDate}' ";
            }
            return Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, findGiftsCountSql, null));
        }

        #endregion 足浴

        /// <summary>
        /// 多门店支付后处理
        /// </summary>
        /// <param name="goodsOrder"></param>
        /// <param name="extraMsg"></param>
        /// <returns></returns>
        public bool HandleAfterPayOrder(EntGoodsOrder goodsOrder, string extraMsg = "")
        {
            if (goodsOrder == null)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception($"多门店支付处理失败：没有找到订单信息（{extraMsg}）"));
                return false;
            }
            FootBath store = FootBathBLL.SingleModel.GetModel($"appId={goodsOrder.aId} and id={goodsOrder.StoreId} and State>0");
            if (store == null)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception($"多门店支付处理失败：没有找到门店信息（{extraMsg}, StoreId：{goodsOrder.StoreId},appId:{goodsOrder.aId} ）"));
                return false;
            }
            TakeOutWayModel outWayModel = JsonConvert.DeserializeObject<TakeOutWayModel>(store.TakeOutWayConfig);
            goodsOrder.PayDate = DateTime.Now;
            switch (goodsOrder.GetWay)
            {
                case (int)multiStoreOrderType.到店自取:
                    goodsOrder.State = (int)MiniAppEntOrderState.待自取;
                    break;

                case (int)multiStoreOrderType.同城配送:

                    goodsOrder.State = (int)MiniAppEntOrderState.待接单;
                    if (outWayModel.cityService.AutoReceiveOrder)
                    {
                        goodsOrder.State = (int)MiniAppEntOrderState.待配送;
                    }
                    break;

                case (int)multiStoreOrderType.快递配送:
                    goodsOrder.State = (int)MiniAppEntOrderState.待发货;
                    break;
            }

            //判断是否是拼团订单
            TransactionModel tran = new TransactionModel();
            if (!EntGroupSponsorBLL.SingleModel.PayReturnUpdateGroupState(goodsOrder.GroupId, goodsOrder.aId, ref tran, 1))
            {
                return false;
            }

            if (Update(goodsOrder, "state,paydate"))
            {
                TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(goodsOrder);

                object objData = TemplateMsg_Miniapp.MutilStoreGetTemplateMessageData(goodsOrder, SendTemplateMessageTypeEnum.多门店订单支付成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(goodsOrder.UserId, SendTemplateMessageTypeEnum.多门店订单支付成功通知, TmpType.小程序多门店模板, objData);
            }
            else
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception($"多门店支付处理失败：更新订单支付状态时失败（{extraMsg}, StoreId：{goodsOrder.StoreId},appId:{goodsOrder.aId} ）"));
                return false;
            }

            return true;
        }

        public List<EntGoodsOrder> GetServiceOrderList(int technicianId, int orderType, string states, int pageSize, int pageIndex)
        {
            string findSql = $"select o.*,c.reservationTime,c.foodgoodsid as reservationProjectId from entgoodsorder o inner join entgoodscart c on o.id = c.GoodsOrderId where c.technicianId = {technicianId} and c.state = 1  and o.state in ({states}) and o.OrderType = {orderType} order by o.createdate desc limit {(pageIndex - 1) * pageSize},{pageSize} ";

            List<EntGoodsOrder> orders = new List<EntGoodsOrder>();
            EntGoodsOrder curOrder = new EntGoodsOrder();
            string curOrderTime = string.Empty;
            string curOrderProjectId = string.Empty;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, findSql, null))
            {
                while (dr.Read())
                {
                    curOrder = GetModel(dr);
                    if (curOrder != null)
                    {
                        curOrderTime = dr["reservationTime"].ToString();
                        curOrderProjectId = dr["reservationProjectId"].ToString();
                        if (!string.IsNullOrWhiteSpace(curOrderTime))
                        {
                            curOrder.reservationTime = Convert.ToDateTime(curOrderTime);
                        }
                        else
                        {
                            curOrder.reservationTime = Convert.ToDateTime("0001-01-01 00:00:00");
                        }
                        if (!string.IsNullOrWhiteSpace(curOrderProjectId))
                        {
                            curOrder.reservationProjectId = Convert.ToInt32(curOrderProjectId);
                        }
                        else
                        {
                            curOrder.reservationProjectId = 0;
                        }

                        orders.Add(curOrder);
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// 方法操作涵盖3部分<不分前后>：1.创建订单,2.将购物车对应内容转为订单内容,3.减库存
        /// </summary>
        /// <param name="order"></param>
        /// <param name="goodsCar"></param>
        /// <param name="userInfo"></param>
        /// <param name="extraSql">额外的订单生成节点前要执行的Sql</param>
        /// <param name="couponLogId">本订单使用的优惠券ID</param>
        /// <returns></returns>
        public bool AddGoodsOrder_MultiStore(ref EntGoodsOrder order, List<EntGoodsCart> goodsCar, C_UserInfo userInfo, FootBath storeMaterial, StringBuilder extraSql, ref string msg, int couponLogId = 0)
        {
            TransactionModel TranModel = new TransactionModel();
            //额外的订单生成节点前要执行的Sql
            if (extraSql != null)
            {
                TranModel.Add(extraSql.ToString());
            }

            //处理商品库存
            if (!HandleStockSql_MultiStore(goodsCar, storeMaterial, TranModel, 1))
            {
                msg = $"您钦点的订单在转化过程中的节点代号000失败了,建议重新下单！";
                log4net.LogHelper.WriteInfo(GetType(), $"生成库存处理sql失败 HandleStockSql_MultiStore_error || carIds:{goodsCar.Select(g => g.Id)}");
                return false;
            }

            //创建订单
            TranModel.Add(BuildAddSql(order));
            //将购物车记录转为订单明细记录
            TranModel.Add($" update EntGoodsCart set GoodsOrderId = (select last_insert_id()),State = 1,UserId = {userInfo.Id} where id in ({string.Join(",", goodsCar.Select(c => c.Id))}) and state = 0; ");

            //执行生成订单全部操作
            if (!ExecuteTransactionDataCorect(TranModel.sqlArray, TranModel.ParameterArray))
            {
                msg = $"您钦点的订单在转化过程中的节点代号001失败了,建议重新下单！";
                return false;
            }

            #region 取到数据库的订单Model赋值给order(因为底层事务的写法暂不支持直接取到ID,故采用此写法来取到model)

            EntGoodsCart cartmodel = EntGoodsCartBLL.SingleModel.GetModel("Id=" + goodsCar[0].Id + " and GoodsOrderId>0 and State = 1 ");
            if (cartmodel == null)
            {
                msg = $"您钦点的订单在转化过程中的节点代号002失败了,建议重新下单！";
                return false;
            }
            order = GetModel(cartmodel.GoodsOrderId);
            if (order == null)
            {
                msg = $"您钦点的订单在转化过程中的节点代号003失败了,建议重新下单！";
                return false;
            }

            #endregion 取到数据库的订单Model赋值给order(因为底层事务的写法暂不支持直接取到ID,故采用此写法来取到model)

            //添加订单日志记录
            EntGoodsOrderLog curOrderLog = new EntGoodsOrderLog()
            {
                GoodsOrderId = order.Id,
                UserId = userInfo.Id,
                LogInfo = $" 成功下单,下单金额：{order.BuyPrice * 0.01} 元 ",
                CreateDate = DateTime.Now
            };
            EntGoodsOrderLogBLL.SingleModel.Add(curOrderLog);

            //若使用了优惠券将优惠券标记为已使用
            if (couponLogId > 0)
            {
                CouponLogBLL.SingleModel.Update(new CouponLog()
                {
                    Id = couponLogId,
                    State = 1,
                    OrderId = order.Id
                }, "state,orderid");
            }
            return true;
        }

        /// <summary>
        /// 根据购物车生成相应处理库存的sql
        /// </summary>
        ///  <param name="operation">1 为减去库存 , -1为回滚库存</param>
        public bool HandleStockSql_MultiStore(List<EntGoodsCart> goodsCar, FootBath storeMaterial, TransactionModel tranModel, int operation)
        {
            //根据购物车记录数量减库存
            List<EntGoods> goods = EntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.FoodGoodsId).Distinct().ToList())}) ");
            if (goods == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), "拼接sql出错 goods_null");
                return false;
            }
            List<SubStoreEntGoods> subGoods = new List<SubStoreEntGoods>();
            if (storeMaterial.HomeId != 0)
            {
                subGoods = SubStoreEntGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", goodsCar.Select(x => x.SubGoodId).Distinct().ToList())}) ");
                if (subGoods == null)
                {
                    log4net.LogHelper.WriteInfo(GetType(), "拼接sql出错 subGoods_null");
                    return false;
                }
            }
            var goodDtlJsonHelper = new Utility.Easyui.EasyuiHelper<EntGoodsAttrDetail>();

            //在model中处理库存
            foreach (EntGoodsCart x in goodsCar)
            {
                EntGoods good = goods.Where(y => y.id == x.FoodGoodsId)?.FirstOrDefault();
                if (good != null || good.stockLimit) //限制库存时才去操作库存
                {
                    //总店去更新总店商品库存表
                    if (storeMaterial.HomeId == 0)
                    {
                        if (string.IsNullOrWhiteSpace(x.SpecIds))
                        {
                            good.stock -= (x.Count * operation);
                        }
                        else
                        {
                            good.stock -= (x.Count * operation);
                            var GASDetailList = new List<EntGoodsAttrDetail>();
                            good.GASDetailList.ForEach(y =>
                            {
                                if (y.id.Equals(x.SpecIds))
                                {
                                    y.stock -= (x.Count * operation);
                                }
                                GASDetailList.Add(y);
                            });
                            //规格库存详情重新赋值
                            string specificationdetail_new = goodDtlJsonHelper.SToJsonArray(GASDetailList);
                            if (specificationdetail_new.Equals(good.specificationdetail))
                            {
                                log4net.LogHelper.WriteInfo(GetType(), "拼接sql出错 specificationdetail_error");
                                return false;
                            }
                            good.specificationdetail = specificationdetail_new;
                        }
                    }
                    else //分店去分店更新库存
                    {
                        SubStoreEntGoods subGood = subGoods?.Where(y => y.Pid == x.FoodGoodsId).FirstOrDefault();
                        if (subGood != null)
                        {
                            if (string.IsNullOrWhiteSpace(x.SpecIds))
                            {
                                subGood.SubStock -= (x.Count * operation);
                            }
                            else
                            {
                                subGood.SubStock -= (x.Count * operation);
                                var GASDetailList = new List<EntGoodsAttrDetail>();
                                subGood.GASDetailList?.ForEach(y =>
                                {
                                    if (y.id.Equals(x.SpecIds))
                                    {
                                        y.stock -= (x.Count * operation);
                                    }
                                    GASDetailList.Add(y);
                                });

                                //规格库存详情重新赋值
                                string specificationdetail_new = goodDtlJsonHelper.SToJsonArray(GASDetailList);
                                if (specificationdetail_new.Equals(subGood.SubSpecificationdetail))
                                {
                                    log4net.LogHelper.WriteInfo(GetType(), "拼接sql出错 SubSpecificationdetail_error");
                                    return false;
                                }
                                //规格库存详情重新赋值
                                subGood.SubSpecificationdetail = specificationdetail_new;
                            }
                        }
                    }
                }
            }
            //生成更新库存sql
            goods.ForEach(x =>
            {
                if (x.stockLimit)
                {
                    //总店
                    if (storeMaterial.HomeId == 0)
                    {
                        tranModel.Add($" update Entgoods set Stock={x.stock},specificationdetail='{x.specificationdetail}' where Id = {x.id} ");
                    }
                    else //分店
                    {
                        SubStoreEntGoods subGood = subGoods.Where(y => y.Pid == x.id)?.First();
                        if (subGood != null)
                        {
                            tranModel.Add($" update SubStoreEntGoods set SubStock={subGood.SubStock},SubSpecificationdetail='{subGood.SubSpecificationdetail}' where Id = {subGood.Id} ");
                        }
                    }
                }
            });
            return true;
        }

        /// <summary>
        /// 多门店退款
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public bool outOrder(List<EntGoodsCart> goodsCar, FootBath storeMaterial, EntGoodsOrder orderInfo)
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
                        Update(orderInfo, "State,outOrderDate");
                    }
                    else
                    {
                        CityMorders order = new CityMordersBLL().GetModel(orderInfo.OrderId);
                        orderInfo.State = (int)MiniAppEntOrderState.退款中;
                        if (order == null)
                        {
                            orderInfo.State = (int)MiniAppEntOrderState.退款失败;

                            Update(orderInfo, "State,outOrderDate");
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
                        new ReFundQueueBLL().Add(reModel);
                    }
                    TransactionModel tranModel = new TransactionModel();
                    tranModel.Add(BuildUpdateSql(orderInfo, "State,outOrderDate"));
                    if (!HandleStockSql_MultiStore(goodsCar, storeMaterial, tranModel, -1))
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

        /// <summary>
        /// 获取订单销售总额
        /// </summary>
        /// <returns></returns>
        public int GetOrderPriceSum(int storeid, int aid, string startdate, string enddate)
        {
            string sql = $"select sum(buyprice) price from entgoodsorder where aid = {aid} and templatetype =  {(int)TmpType.小程序多门店模板} and state={(int)MiniAppEntOrderState.交易成功} and AcceptDate<='{enddate}' and AcceptDate>='{startdate}'";

            if (storeid > 0)
            {
                sql += $" and storeid = {storeid}";
            }

            var result = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql);
            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        /// <summary>
        /// 获取店铺商品销售记录
        /// </summary>
        /// <param name="storeid">门店ID</param>
        /// <param name="rid">权限ID</param>
        /// <param name="odbtype">排序</param>
        /// <param name="starttime">开始时间</param>
        /// <param name="endtime">结束时间</param>
        /// <param name="limit">返回条数</param>
        /// <returns></returns>
        public List<MiniAppStoreGoods> GetOrderGoodsSaleLog(int storeid, int rid, int odbtype, string starttime, string endtime, int limit = 10)
        {
            List<MiniAppStoreGoods> groupslist = new List<MiniAppStoreGoods>();
            string orderby = "goods.xprice";
            string wheresql = " goods.xprice>0";
            string limitsql = " LIMIT " + limit;
            if (odbtype == 1)
            {
                orderby = "goods.buynum";
                wheresql = " goods.buynum>0";
            }
            if (limit == 0)
            {
                limitsql = "";
            }
            string sql = $@"
			select * from (select o.id,
					sum(o.buyprice) xprice,
			c.foodgoodsid as storeid,
			sum(c.count) as buynum,
			g.name groupname,
			1 goodtype
			from entgoodsorder o left join entgoodscart c
			on o.id = c.goodsorderid
			left join entgoods g on c.foodgoodsid = g.id
			where  o.templatetype={(int)TmpType.小程序多门店模板}
			{(storeid > 0 ? $" and o.storeid={storeid}" : "")}
			and o.state = {(int)MiniAppEntOrderState.交易成功}
			and o.aid = {rid} and AcceptDate>='{starttime}' and AcceptDate<='{endtime}'
			group by c.foodgoodsid
			) goods where {wheresql}
			ORDER BY {orderby} desc";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    MiniAppStoreGoods model = new MiniAppStoreGoods();
                    model.Id = Convert.ToInt32(dr["id"]);
                    model.StoreId = Convert.ToInt32(dr["storeid"]);
                    model.GoodsName = dr["groupname"].ToString();
                    model.IsSell = Convert.ToInt32(dr["goodtype"]);
                    if (DBNull.Value != dr["xprice"])
                    {
                        model.Price = Convert.ToInt32(dr["xprice"]);
                    }
                    if (DBNull.Value != dr["buynum"])
                    {
                        model.salesCount = Convert.ToInt32(dr["buynum"]);
                    }

                    groupslist.Add(model);
                }
            }

            return groupslist;
        }

        /// <summary>
        /// 获取店铺商品分类销售记录
        /// </summary>
        /// <param name="storeid">门店ID</param>
        /// <param name="rid">权限ID</param>
        /// <param name="odbtype">排序</param>
        /// <param name="starttime">开始时间</param>
        /// <param name="endtime">结束时间</param>
        /// <param name="limit">返回条数</param>
        /// <returns></returns>
        public List<MiniAppStoreGoods> GetOrderGoodsTypeSaleLog(int storeid, int rid, string starttime, string endtime, ref int salesum)
        {
            List<MiniAppStoreGoods> groupslist = new List<MiniAppStoreGoods>();

            string sql = $@"select et.id,et.name,sum(goods.xprice) salesum,goods.storeid from entgoodtype et right JOIN (
			select o.buyprice xprice,
			c.foodgoodsid as storeid,
			g.ptypes
			from entgoodsorder o left join entgoodscart c
			on o.id = c.goodsorderid
			left join entgoods g on c.foodgoodsid = g.id
			where  o.templatetype={(int)TmpType.小程序多门店模板}
			{(storeid > 0 ? $" and o.storeid={storeid}" : "")}
			and o.state = {(int)MiniAppEntOrderState.交易成功}
			and o.aid = {rid} and AcceptDate>='{starttime}' and AcceptDate<='{endtime}'
			) goods
			on FIND_IN_SET(et.id,goods.ptypes)
			GROUP BY id";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    MiniAppStoreGoods model = new MiniAppStoreGoods();
                    model.TypeId = Convert.ToInt32(dr["id"]);
                    model.StoreId = Convert.ToInt32(dr["storeid"]);
                    model.GoodsName = dr["name"].ToString();
                    if (DBNull.Value != dr["salesum"])
                    {
                        model.Price = Convert.ToInt32(dr["salesum"]);
                        salesum += model.Price;
                    }

                    groupslist.Add(model);
                }
            }

            return groupslist;
        }

        #region 高级拼团

        /// <summary>
        /// 获取拼团订单记录条件sql
        /// </summary>
        /// <param name="state">30:"拼团中",31:"拼团成功",32:"拼团失败"</param>
        /// <returns></returns>
        private string GetGroupSql(int state, int rid)
        {
            string wheresql = "";
            if (state == 30 || state == 31 || state == 32)
            {
                string statestr = state == 30 ? ((int)GroupState.开团成功).ToString() : state == 31 ? ((int)GroupState.团购成功).ToString() : $"{(int)GroupState.成团失败},{(int)GroupState.已过期}";
                List<EntGroupSponsor> sponsorlist = EntGroupSponsorBLL.SingleModel.GetListGroupSponsorByRid(rid, statestr);
                if (sponsorlist != null && sponsorlist.Count > 0)
                {
                    string groupids = string.Join(",", sponsorlist.Select(s => s.Id).Distinct());
                    wheresql = $" where o.aid = '{rid}' and o.groupid in ({groupids}) and o.state not in ({(int)MiniAppEntOrderState.已取消}) ";
                }
            }

            return wheresql;
        }

        /// <summary>
        /// 获取我的拼团订单
        /// </summary>
        /// <param name="state"></param>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<EntGoodsOrder> GetMyEntGoupOrderList(int state, int aid, int userId, int pageIndex, int pageSize, int storeid = 0)
        {
            if (pageIndex <= 0)
                pageIndex = 1;
            List<EntGoodsOrder> list = new List<EntGoodsOrder>();
            string where = $"select o.*,sor.state GroupState from entgoodsorder o left join entgroupsponsor sor on o.groupid = sor.id left join entgroupsrelation r on r.id = sor.entgoodrid where o.ordertype = 3 and o.userid = {userId} and r.storeid={storeid}";
            if (0 == state)
            {
                where += $" and o.groupid>0";
            }//拼团中
            else if (1 == state)
            {
                where += $" and sor.State={(int)GroupState.开团成功} and sor.EndDate>now()";
            }//已成团
            else if (2 == state)
            {
                where += $" and  (sor.State={(int)GroupState.团购成功})";
            }//未成团
            else if (-1 == state)
            {
                where += $" and sor.State not in({(int)GroupState.团购成功}) and g.EndDate<now()";
            }

            where += $" order by sor.StartDate DESC LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, where))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    if (dr["GroupState"] != DBNull.Value)
                    {
                        model.GroupState = Convert.ToInt32(dr["GroupState"]);
                    }

                    list.Add(model);
                }
            }

            return list;
        }

        public object GetMyGroupPostData(int state, int rid, int userId, int pageIndex, int pageSize, int storeId)
        {
            List<EntGoodsOrder> goodOrderList = GetMyEntGoupOrderList(state, rid, userId, pageIndex, pageSize, storeId);
            if (goodOrderList != null && goodOrderList.Any())
            {
                string orderids = string.Join(",", goodOrderList.Select(x => x.Id).Distinct());
                List<EntGoodsCart> goodOrderDtlList = EntGoodsCartBLL.SingleModel.GetListByOrderIds(orderids);
                if (goodOrderDtlList != null && goodOrderDtlList.Any())
                {
                    string goodids = string.Join(",", goodOrderDtlList.Select(x => x.FoodGoodsId).Distinct());
                    List<EntGoods> goodList = EntGoodsBLL.SingleModel.GetListByIds(goodids);
                    //判断商品是否已评论
                    GoodsCommentBLL.SingleModel.DealGoodsCommentState<EntGoodsCart>(ref goodOrderDtlList, rid, userId, (int)EntGoodsType.拼团产品, "FoodGoodsId", "GoodsOrderId");
                    goodOrderDtlList.ForEach(x =>
                    {
                        x.goodsMsg = goodList.Where(y => y.id == x.FoodGoodsId).FirstOrDefault();
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
                           getWay = x.GetWay,
                           getWayStr = x.GetWayStr,
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
        public List<object> GetPersonByGroup(string groupids, ref int groupnum)
        {
            List<object> userimglist = new List<object>();
            //获取该产品的拼团所有支付成功的订单，用来计算该拼团有多少用户购买
            List<EntGoodsOrder> list = GetListGroupOrder(groupids);
            if (list == null || list.Count <= 0)
            {
                return userimglist;
            }

            groupnum = list.Sum(s => s.QtyCount);
            //用户ID
            string userids = string.Join(",", list.Select(s => s.UserId).Distinct());
            List<C_UserInfo> userlist = C_UserInfoBLL.SingleModel.GetListByIds(userids);
            if (userlist == null || userlist.Count <= 0)
            {
                return userimglist;
            }

            foreach (EntGoodsOrder item in list)
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
        /// 获取团已团数量
        /// </summary>
        /// <param name="groupids"></param>
        /// <returns></returns>
        public int GetGroupCount(string groupids, int userid)
        {
            string sql = $"select sum(QtyCount) from entgoodsorder where ordertype = 3 and groupid in ({groupids}) and state in ({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.交易成功},{(int)MiniAppEntOrderState.待收货},{(int)MiniAppEntOrderState.待自取})";
            if (userid > 0)
            {
                sql += $" and userid={userid}";
            }
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        /// <summary>
        /// 获取用户已团产品数量
        /// </summary>
        /// <param name="userid">用户userid</param>
        /// <param name="entgoodrid">产品关联拼团表ID</param>
        /// <param name="storeid">多门店分店ID，其他的为0</param>
        /// <returns></returns>
        public int GetGroupPersonCount(int userid, int entgoodid, int storeid)
        {
            string sqlwhere = $@"select sum(o.qtycount) qtycount from entgoodsorder o left join entgoodscart ec on o.id = ec.goodsorderid
									where o.ordertype = 3
									and o.state in ({(int)MiniAppEntOrderState.待发货},{(int)MiniAppEntOrderState.交易成功},{(int)MiniAppEntOrderState.待收货},{(int)MiniAppEntOrderState.待自取},{(int)MiniAppEntOrderState.待付款})
									and ec.foodgoodsid = {entgoodid}";
            if (storeid > 0)
            {
                sqlwhere += $" and o.storeid = {storeid}";
            }
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
        /// 获取用户参加该拼团的订单
        /// </summary>
        /// <param name="grouid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public EntGoodsOrder GetModelGroupByGrouId(int grouid, int userid)
        {
            string sqlwhere = $"userid={userid} and ordertype = 3 and groupid = {grouid} and state in ({ (int)MiniAppEntOrderState.待发货},{ (int)MiniAppEntOrderState.交易成功},{ (int)MiniAppEntOrderState.待收货},{ (int)MiniAppEntOrderState.待自取},{ (int)MiniAppEntOrderState.待付款})";

            EntGoodsOrder model = GetModel(sqlwhere);

            return model;
        }

        #region 拼团退款

        /// <summary>
        /// 拼团退款
        /// </summary>
        /// <param name="item"></param>
        /// <param name="type">0：拼团失败退款，1：店主手动退款</param>
        /// <returns></returns>
        public bool RefundOne(EntGoodsOrder item, ref string msg)
        {
            int paytype = item.BuyMode;

            TransactionModel tranmodel = new MiniApp.TransactionModel();
            EntGroupSponsor csg = EntGroupSponsorBLL.SingleModel.GetModel(item.GroupId);
            if (csg == null)
            {
                msg = "小程序拼团商品不存在啦=" + item.GroupId;
                item.State = (int)MiniAppEntOrderState.已取消;
                Update(item, "State");
                return false;
            }
            EntGroupSponsor gsinfo = EntGroupSponsorBLL.SingleModel.GetModel(item.GroupId);
            if (gsinfo == null)
            {
                msg = "小程序拼团团购不存在啦=" + item.GroupId;
                item.State = (int)MiniAppEntOrderState.已取消;
                Update(item, "State");
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
                CityMordersBLL mbll = new CityMordersBLL();
                CityMorders order = mbll.GetModel(item.OrderId);
                if (order == null)
                {
                    msg = "xxxxxxxxxxxxxxxxxx小程序拼团退款查不到支付订单 ID=" + item.Id;
                    item.State = (int)MiniappPayState.已失效;
                    Update(item, "State");
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
                tranmodel.Add(new ReFundQueueBLL().BuildAddSql(reModel));
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

            if (!updateStock(item, (int)MiniAppEntOrderState.退款成功))
            {
                msg = "xxxxxxxxxxxxxxxxxx专业版拼团退款更新库存失败，ID=" + item.Id;
                return false;
            }

            msg = "xxxxxxxxxxxxxxxxxx专业版拼团退款成功，ID=" + item.Id;

            //根据订单释放库存
            return true;
        }

        #endregion 拼团退款

        #endregion 高级拼团

        #region 商家小程序

        /// <summary>
        /// 获取已支付用户数
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetPayUserCount(string appId, string startDate, string endDate)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            string sql = $"select count(1) from (select userId from entgoodsorder where appid = @appid and State in (1,2,3,4,5) and CreateDate >= @startDate and CreateDate <= @endDate group by userid) a";
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, paramters.ToArray());
            if (result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }

            return count;
        }

        /// <summary>
        /// 根据条件获取订单数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetOrderSumByCondition(string appId, int orderType, int type, string value, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();

            string sqlwhere = GetSqlwhere(paramters, appId, orderType, type, value, startDate, endDate);
            if (sqlwhere == null)
            {
                return count;
            }
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        /// <summary>
        /// 根据条件获取订单列表（orderType=3是高级拼团订单）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderType"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object GetListByCondition(string appId, int aid, int pageIndex, int pageSize, int orderType, int state, int type, string value, string startDate = "", string endDate = "")
        {

            List<EntGoodsOrder> orderList = null;
            List<MySqlParameter> parameters = new List<MySqlParameter>();

            string sqlwhere = GetSqlwhere(parameters, appId, orderType, type, value, startDate, endDate);
            if (sqlwhere == null)
            {
                return orderList;
            }

            #region 拼团条件
            List<EntGroupSponsor> sponsorlist = null;
            if (orderType == 3)
            {
                string statestr = "-1";
                string groupSqlWhere = "";
                switch (state)
                {
                    case 30:
                    case 31:
                    case 32:
                        statestr = state == 30 ? ((int)GroupState.开团成功).ToString() : state == 31 ? ((int)GroupState.团购成功).ToString() : $"{(int)GroupState.成团失败},{(int)GroupState.已过期}";
                        groupSqlWhere = $" and state not in ({(int)MiniAppEntOrderState.已取消}) ";
                        break;
                    case 1://待发货
                    case 2://待收货
                        statestr = ((int)GroupState.团购成功).ToString();
                        groupSqlWhere = $" and state in ({state}) ";
                        break;
                }

                sponsorlist = EntGroupSponsorBLL.SingleModel.GetListGroupSponsorByRid(aid, statestr);
                if (sponsorlist != null && sponsorlist.Count > 0)
                {
                    string groupids = string.Join(",", sponsorlist.Select(s => s.Id).Distinct());
                    sqlwhere += $" and groupid in ({groupids}) {groupSqlWhere}";
                }
            }
            #endregion

            switch (state)
            {
                case -999:
                    break;
                case 30:
                case 31:
                case 32:
                case 1://待发货
                case 2://待收货
                    if (orderType != 3)//拼团
                    {
                        sqlwhere += $" and state={state}";
                    }
                    break;
                default:
                    sqlwhere += $" and state={state}";
                    break;
            }
            orderList = GetListByParam(sqlwhere, parameters.ToArray(), pageSize, pageIndex, "*", "id desc");
            //判断是否有团订单
            EntGroupSponsorBLL.SingleModel.GetSponsorState(ref orderList);
            if (orderList != null && orderList.Count > 0)
            {
                foreach (var goodsOrder in orderList)
                {
                    goodsOrder.goodsCarts = EntGoodsCartBLL.SingleModel.GetOrderDetail(goodsOrder.Id);
                }
            }

            return orderList;
        }

        private string GetSqlwhere(List<MySqlParameter> parameters, string appId, int orderType, int type, string value, string startDate = "", string endDate = "")
        {
            string sqlwhere = "appid=@appid and ordertype!=3";
            if (orderType == 3)
            {
                sqlwhere = "appid=@appid and ordertype=3";
            }
            parameters.Add(new MySqlParameter("@appid", appId));
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                sqlwhere += " and CreateDate >= @startDate and CreateDate <= @endDate";
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
            }
            if (!string.IsNullOrEmpty(value))
            {
                switch (type)
                {
                    case 0://订单号
                        sqlwhere += " and OrderNum like @orderNum";
                        parameters.Add(new MySqlParameter("@orderNum", $"%{value}%"));
                        break;

                    case 1://商品名称
                        List<EntGoods> goodsList = EntGoodsBLL.SingleModel.GetList($" name like '%{value}%'");
                        if (goodsList == null || goodsList.Count <= 0)
                            return null;
                        List<EntGoodsCart> goodsCartList = EntGoodsCartBLL.SingleModel.GetList($" state = 1 and FoodGoodsId in ({string.Join(",", goodsList.Select(x => x.id))}) ");
                        if (goodsCartList == null || goodsCartList.Count <= 0)
                            return null;
                        sqlwhere += $" and id in ({string.Join(",", goodsCartList.Select(c => c.GoodsOrderId))}) ";
                        break;

                    case 2://手机号码
                        sqlwhere += " and AccepterTelePhone like @phone";
                        parameters.Add(new MySqlParameter("@phone", $"%{value}%"));
                        break;

                    case 3://客户名
                        sqlwhere += " and AccepterName like @name";
                        parameters.Add(new MySqlParameter("@name", $"%{value}%"));
                        break;

                    case 4://提货码
                        sqlwhere += " and tableNo like @tableNo";
                        parameters.Add(new MySqlParameter("@tableNo", $"%{value}%"));
                        break;
                }
            }
            return sqlwhere;
        }

        /// <summary>
        /// 根据状态获取订单数量
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int GetOrderSumByCondition(string appId, int orderType, int type, string value, int state)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            string sqlwhere = GetSqlwhere(paramters, appId, orderType, type, value);
            sqlwhere += $" and state={state}";
            //if (!string.IsNullOrEmpty(value))
            //{
            //    switch (type)
            //    {
            //        case 0://订单号
            //            sqlwhere += " and OrderNum=@orderNum";
            //            paramters.Add(new MySqlParameter("@orderNum", value));
            //            break;
            //        case 1://商品名称
            //            List<EntGoods> goodsList = new EntGoodsBLL().GetList($" name like '%{value}%'");
            //            if (goodsList == null || goodsList.Count <= 0) return count;
            //            List<EntGoodsCart> goodsCartList = new EntGoodsCartBLL().GetList($" state = 1 and FoodGoodsId in ({string.Join(",", goodsList.Select(x => x.id))}) ");
            //            if (goodsCartList == null || goodsCartList.Count <= 0) return count;
            //            sqlwhere += $" and id in ({string.Join(",", goodsCartList.Select(c => c.FoodGoodsId))}) ";
            //            break;
            //        case 2://手机号码
            //            sqlwhere += " and AccepterTelePhone like @phone";
            //            paramters.Add(new MySqlParameter("@phone", $"%{value}%"));
            //            break;
            //        case 3://客户名
            //            sqlwhere += " and AccepterName like @name";
            //            paramters.Add(new MySqlParameter("@name", $"%{value}%"));
            //            break;
            //    }
            //}
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        public int GetOrderSum(string appId, string states, string startDate = "", string endDate = "")
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            string sqlwhere = $"appid=@appid and state in ({states})";
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                //sqlwhere += " and CreateDate>=@startDate and CreateDate<=@endDate";
                sqlwhere += " and CreateDate between @startDate and @endDate";

                paramters.Add(new MySqlParameter("@startDate", startDate));
                paramters.Add(new MySqlParameter("@endDate", endDate));
            }
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        /// <summary>
        /// 获取指定时间段内小程序完成订单总额
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderState"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetOrderPriceSumByAppId_Date(string appId, string startDate, string endDate)
        {
            int priceSum = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return priceSum;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            //string sql = $"select sum(buyprice) pricesum from entgoodsorder where appid=@appid and State =3 and CreateDate>=@startDate and CreateDate<=@endDate";
            string sql = $"select sum(buyprice) pricesum from entgoodsorder where appid=@appid and State =3 and CreateDate  between @startDate and @endDate";

            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, paramters.ToArray());
            if (result != DBNull.Value)
            {
                priceSum = Convert.ToInt32(result);
            }

            return priceSum;
        }

        /// <summary>
        /// 获取指定时间段内小程序的所有订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetOrderSum(string appId, string startDate, string endDate)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return count;
            }
            //string sqlwhere = $"appid=@appid and CreateDate>=@startDate and CreateDate<=@endDate";
            string sqlwhere = $"appid=@appid and CreateDate between @startDate and @endDate";

            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            paramters.Add(new MySqlParameter("@startDate", startDate));
            paramters.Add(new MySqlParameter("@endDate", endDate));
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        public EntGoodsOrder GetModelByAppIdAndId(string appId, int orderId)
        {
            if (string.IsNullOrEmpty(appId) || orderId <= 0)
            {
                return null;
            }
            string sqlwhere = $"appid=@appid and id={orderId}";
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            return GetModel(sqlwhere, paramters.ToArray());
        }

        #endregion 商家小程序

        /// <summary>
        /// 获取预约订单
        /// </summary>
        /// <param name="reserveId"></param>
        /// <returns></returns>
        public List<EntGoodsOrder> GetReserveOrder(int reserveId)
        {
            return GetList($"ReserveId = {reserveId}");
        }

        public List<EntGoodsOrder> GetByIds(List<int> orderIds)
        {
            return GetList($"Id in ({string.Join(",", orderIds)})");
        }

        /// <summary>
        /// 生成对外订单号
        /// </summary>
        /// <returns></returns>
        public string GetOutOrderNum()
        {
            string outOrderNum = MaxId.ToString();
            outOrderNum = outOrderNum.Length >= 3 ? outOrderNum.Substring(outOrderNum.Length - 3, 3) : outOrderNum.PadLeft(3, '0');
            outOrderNum = $"{DateTime.Now.ToString("yyyyMMddHHmm")}{outOrderNum}";
            return outOrderNum;
        }

        /// <summary>
        /// 生成付费内容订单
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="aid"></param>
        /// <param name="payContentId"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public EntGoodsOrder BuildOrderForPayContent(string appId, int aid, int payContentId, C_UserInfo userInfo, PayContentPayment payInfo)
        {
            return new EntGoodsOrder
            {
                AppId = appId,
                aId = aid,
                OrderId = 0,
                OrderNum = GetOutOrderNum(),
                BuyPrice = payInfo.PayAmount,
                ReducedPrice = payInfo.DiscountAmount,
                State = (int)MiniAppEntOrderState.待付款,
                UserId = userInfo.Id,
                CreateDate = DateTime.Now,
                OrderType = (int)EntOrderType.付费内容订单,
                TemplateType = (int)TmpType.小程序专业模板,
                AccepterName = userInfo.NickName,
                AccepterTelePhone = userInfo.TelePhone,
                Address = $"{userInfo.Address}",
                GoodsGuid = payContentId.ToString()
            };
        }

        public List<string> UpdatePaidContentOrder(PaidContentRecord record)
        {
            List<string> updateSql = new List<string>();
            EntGoodsOrder order = GetModel(record.PayId);
            order.State = (int)MiniAppEntOrderState.交易成功;
            order.PayDate = DateTime.Now;
            order.GoodsGuid = Guid.NewGuid().ToString().Replace("-", "");
            updateSql.Add(BuildUpdateSql(order, "State,PayDate,GoodsGuid"));
            //记录订单支付日志
            updateSql.Add(EntGoodsOrderLogBLL.SingleModel.BuildAddSql(new EntGoodsOrderLog()
            {
                GoodsOrderId = order.Id,
                UserId = order.UserId,
                LogInfo = $" 订单成功支付付费内容：{order.BuyPrice * 0.01} 元 ",
                CreateDate = DateTime.Now
            }));
            return updateSql;
        }

        public void UpdatePaidContenSuccess(PaidContentRecord record)
        {
            EntGoodsOrder order = GetModel(record.PayId);
            //发送模板消息
            AfterPayOrderBySaveMoney(order);
            //新订单电脑语音提示
            Utils.RemoveIsHaveNewOrder(order.aId);
            VipRelationBLL.SingleModel.updatelevel(record.UserId, "entpro");
        }


        /// <summary>
        /// 确认收货计算订单分销佣金
        /// </summary>
        /// <param name="listEntGoodsCart"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public void PayDistributionMoney(List<EntGoodsCart> listEntGoodsCart, EntGoodsOrder order)
        {
            listEntGoodsCart.ForEach(x =>
            {
                //将购物车里需要计算佣金的产品计算佣金分给对应的分销员
                TransactionModel tranModel = new TransactionModel();
                if (x.recordId > 0)
                {

                    SalesManRecord salesManRecord = salesManRecordBLL.GetModel($"Id={x.recordId} and state=1");
                    if (salesManRecord == null)
                    {
                        LogHelper.WriteInfo(this.GetType(), $"购物车{x.Id}确认收货后分销计算佣金失败SalesManRecord为NULL");
                    }
                    else
                    {

                        ConfigModel configModel = new ConfigModel();
                        SalesManConfig salesManConfig = SalesManConfigBLL.SingleModel.GetModel($"appId={x.aId}");
                        if (salesManConfig != null && !string.IsNullOrEmpty(salesManConfig.configStr))
                        {
                            configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManConfig.configStr);
                        }

                        salesManRecord.configModel = JsonConvert.DeserializeObject<ConfigModel>(salesManRecord.configStr);

                        SalesManRecordUser salesManRecordUser = SalesManRecordUserBLL.SingleModel.GetModel($"recordId={salesManRecord.Id}");
                        if (salesManRecordUser == null && configModel.payMentManager.allow_seller_buy == 0 && x.salesManRecordUserId != -1)
                        {
                            LogHelper.WriteInfo(this.GetType(), $"购物车{x.Id}确认收货后分销计算佣金失败salesManRecordUser为NULL");
                        }
                        else
                        {
                            //当条件都符合的时候才进行佣金订单计算

                            int salesManId = salesManRecord.salesManId;
                            if (salesManRecordUser != null)
                            {
                                salesManId = salesManRecordUser.salesManId;
                                //延续对应分销员的保护期
                                salesManRecordUser.UpdateTime = DateTime.Now;
                                salesManRecordUser.protected_time = salesManRecord.configModel.salesManManager.protected_time;
                                tranModel.Add(SalesManRecordUserBLL.SingleModel.BuildUpdateSql(salesManRecordUser));
                            }




                            SalesMan salesMan = SalesManBLL.SingleModel.GetModel(salesManId);
                            double firstCps_rate = 1, secondCps_rate = 1;
                            SalesMan parentSalesman = SalesManBLL.SingleModel.GetModel(salesMan.ParentSalesmanId);
                            if (parentSalesman != null)//salesManRecordUserId=-1表示分销员自己购买商品产生的
                            {
                                //表示该分销商品对应的分销员有上级分销员,需要重新根据二级分销规则进行佣金再分配
                                if (configModel != null)
                                {
                                    firstCps_rate = configModel.secondSalesManConfig.FirstCps_rate * 0.01;//直销佣金比例
                                    secondCps_rate = configModel.secondSalesManConfig.SecondCps_rate * 0.01;//渠道佣金比例

                                }
                            }
                            //分销订单记录表新增一条
                            SalesManRecordOrder salesManRecordOrder = new SalesManRecordOrder()
                            {
                                appId = order.aId,
                                salesManRecordId = x.recordId,
                                orderNumber = order.OrderNum,
                                orderId = order.Id,
                                CarId = x.Id,
                                orderMoney = x.Price * x.Count,
                                cps_rate = x.cps_rate,
                                cpsMoney = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01),
                                state = configModel.payMentManager.auto_settle, //salesManRecord.configModel.payMentManager.auto_settle,
                                addTime = DateTime.Now,
                                PayState = configModel.payMentManager.auto_settle == 0 ? -1 : 0

                            };

                            if ((firstCps_rate + secondCps_rate) <= 1 && firstCps_rate >= 0 && firstCps_rate <= 1 && secondCps_rate >= 0 && secondCps_rate <= 1)
                            {
                                //如果当前分销员有上级,则重新计算直销佣金
                                salesManRecordOrder.cpsMoney = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * firstCps_rate);
                            }
                            salesManRecordOrder.Remark = $"本次推广订单应得佣金{(int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * 0.01)}元,比例{x.cps_rate * 0.01};直销佣金比例{firstCps_rate};渠道佣金比例{secondCps_rate};对应购物车Id={x.Id}";


                            if (x.salesManRecordUserId == -1)
                            {
                                salesManRecordOrder.Remark = $"本次推广订单应得佣金{(int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * 0.01)}元,比例{x.cps_rate * 0.01};分销员自己购买商品并且开启了分销员购买权限;对应购物车Id={x.Id}";

                            }

                            tranModel.Add(SalesManRecordOrderBLL.SingleModel.BuildAddSql(salesManRecordOrder));

                            //更新分销员可提现佣金
                            SalesManCashLog salesManCashLog = new SalesManCashLog();
                            salesManCashLog.Aid = x.aId;
                            salesManCashLog.SaleManId = salesMan.Id;
                            salesManCashLog.AddTime = DateTime.Now;

                            int curCash = 0;
                            if (configModel.payMentManager.auto_settle == 0)
                            {
                                salesManCashLog.CashLog = $"本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                salesMan.useCashTotal += salesManRecordOrder.cpsMoney;//人工结算 自动累计到总收益不能到可提现金额
                                salesManCashLog.CashLog += $"本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                salesManCashLog.Remark = $"变更原因:对应购物车Id={x.Id}";
                                #region 如果当前分销员有上级,则还需要更新上级分销员佣金情况
                                if (parentSalesman != null && (firstCps_rate + secondCps_rate) <= 1 && firstCps_rate >= 0 && firstCps_rate <= 1 && secondCps_rate >= 0 && secondCps_rate <= 1)
                                {
                                    curCash = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * secondCps_rate);//计算渠道佣金

                                    //更新上级分销员佣金
                                    SalesManCashLog parentSalesmanCashLog = new SalesManCashLog();
                                    parentSalesmanCashLog.Aid = x.aId;
                                    parentSalesmanCashLog.SaleManId = parentSalesman.Id;
                                    parentSalesmanCashLog.AddTime = DateTime.Now;
                                    parentSalesmanCashLog.CashLog = $"本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";

                                    parentSalesman.useCashTotal += curCash;
                                    parentSalesman.UpdateTime = DateTime.Now;

                                    parentSalesmanCashLog.CashLog += $"本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                    parentSalesmanCashLog.Remark = $"变更原因:对应购物车Id={x.Id},来自下级分销员{salesMan.Id}";

                                    SalesManRelation salesManRelation = new SalesManRelation();
                                    salesManRelation.Aid = x.aId;
                                    salesManRelation.UserId = salesMan.UserId;
                                    salesManRelation.AddTime = DateTime.Now;
                                    salesManRelation.Price = curCash;
                                    salesManRelation.ParentSaleManId = parentSalesman.Id;
                                    salesManRelation.AutoSettle = 0;

                                    tranModel.Add(SalesManRelationBLL.SingleModel.BuildAddSql(salesManRelation));
                                    tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(parentSalesman));
                                    tranModel.Add(SalesManCashLogBLL.SingleModel.BuildAddSql(parentSalesmanCashLog));
                                }
                                #endregion
                            }
                            else
                            {
                                salesManCashLog.CashLog = $"本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                salesMan.useCash += salesManRecordOrder.cpsMoney;
                                salesManCashLog.CashLog += $"本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                if (parentSalesman != null && (firstCps_rate + secondCps_rate) <= 1 && firstCps_rate >= 0 && firstCps_rate <= 1 && secondCps_rate >= 0 && secondCps_rate <= 1)
                                {
                                    curCash = (int)Math.Ceiling(x.Price * x.Count * x.cps_rate * 0.01 * secondCps_rate);//计算渠道佣金

                                    //更新上级分销员佣金
                                    SalesManCashLog parentSalesmanCashLog = new SalesManCashLog();
                                    parentSalesmanCashLog.Aid = x.aId;
                                    parentSalesmanCashLog.SaleManId = parentSalesman.Id;
                                    parentSalesmanCashLog.AddTime = DateTime.Now;
                                    parentSalesmanCashLog.CashLog = $"本次变更前useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";

                                    parentSalesman.UpdateTime = DateTime.Now;
                                    parentSalesman.useCash += curCash;

                                    parentSalesmanCashLog.CashLog += $"本次变更后useCashTotal={salesMan.totalIncome}元,可提现金额useCash={salesMan.useCashStr}元";
                                    parentSalesmanCashLog.Remark = $"变更原因:对应购物车Id={x.Id},来自下级分销员{salesMan.Id}";

                                    SalesManRelation salesManRelation = new SalesManRelation();
                                    salesManRelation.Aid = x.aId;
                                    salesManRelation.UserId = salesMan.UserId;
                                    salesManRelation.AddTime = DateTime.Now;
                                    salesManRelation.Price = curCash;
                                    salesManRelation.ParentSaleManId = parentSalesman.Id;
                                    salesManRelation.AutoSettle = 1;

                                    tranModel.Add(SalesManRelationBLL.SingleModel.BuildAddSql(salesManRelation));

                                    tranModel.Add(SalesManCashLogBLL.SingleModel.BuildAddSql(parentSalesmanCashLog));
                                    tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(parentSalesman));
                                }
                            }

                            salesMan.UpdateTime = DateTime.Now;
                            tranModel.Add(SalesManCashLogBLL.SingleModel.BuildAddSql(salesManCashLog));
                            tranModel.Add(SalesManBLL.SingleModel.BuildUpdateSql(salesMan));
                            if (tranModel.sqlArray != null && tranModel.sqlArray.Length > 0)
                            {
                                if (!SalesManRecordUserBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                                {
                                    LogHelper.WriteInfo(this.GetType(), $"确认收货后购物车对应产品分销计算佣金失败");
                                }
                            }
                        }
                    }
                }

            });

        }

        /// <summary>
        /// 获取小程序分销订单ID(购物车里包含分销商品的订单ID)
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public string GetDistributionOrderIds(int aid)
        {

            List<EntGoodsCart> list = new List<EntGoodsCart>();
            string sql = $"SELECT goodsOrderId from entgoodscart where aid={aid} and state <>-1 and recordId<>0 and goodsOrderId>0 GROUP BY goodsOrderId";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    EntGoodsCart entGoodsCart = EntGoodsCartBLL.SingleModel.GetModel(dr);
                    if (entGoodsCart != null)
                    {
                        list.Add(entGoodsCart);
                    }
                }

            }

            List<int> listOrderIds = list.Select(x => x.GoodsOrderId).ToList();
            if (listOrderIds != null && listOrderIds.Count > 0)
            {
                return string.Join(",", listOrderIds);
            }

            return "0";
        }
        /// <summary>
        /// 获取用户指定状态订单数
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="userId"></param>
        /// <param name="orderState"></param>
        /// <returns></returns>
        public int GetEntGoodsOrderRecordCount(int aid, int userId, int orderState)
        {
            string sqlstr = @"select count(*) from entgoodsorder o";

            int tempOrderState = orderState;
            if (tempOrderState == -1000)
            {
                orderState = (int)MiniAppEntOrderState.交易成功;//拼团跟普通产品订单 交易成功都为3 只有交易成功的订单才能评论orderState=-1000表示查询待评价订单
            }

            string stateSql = "";
            if (orderState != 10)
            {
                if (orderState == (int)MiniAppEntOrderState.交易成功)
                {
                    if (tempOrderState != -1000)
                    {
                        List<int> completionState = new List<int>();
                        completionState.Add((int)MiniAppEntOrderState.交易成功);
                        completionState.Add((int)MiniAppEntOrderState.退款中);
                        completionState.Add((int)MiniAppEntOrderState.退款失败);
                        completionState.Add((int)MiniAppEntOrderState.退款成功);
                        completionState.Add((int)MiniAppEntOrderState.退货退款成功);
                        completionState.Add((int)MiniAppEntOrderState.退换货成功);
                        stateSql += $" and o.State in ({string.Join(",", completionState)}) ";
                    }
                    else
                    {
                        stateSql += $" and o.State = {orderState} ";
                    }
                }
                else if (orderState == (int)MiniAppEntOrderState.待收货)
                {
                    stateSql += $" and o.State in ({(int)MiniAppEntOrderState.待收货},{(int)MiniAppEntOrderState.待自取}) ";
                }
                else if (orderState == (int)MiniAppEntOrderState.退货中)
                {
                    stateSql += $" and o.State in ({(int)MiniAppEntOrderState.退货审核中},{(int)MiniAppEntOrderState.待退货},{(int)MiniAppEntOrderState.退货中},{(int)MiniAppEntOrderState.换货中})";
                }
                else
                {
                    stateSql += $" and o.State = {orderState} ";
                }
            }

            string sqlWhere = $" where o.aId = {aid} and o.UserId = {userId} and o.OrderType in ({(int)EntGoodsType.普通产品},{(int)EntGoodsType.团购商品}) and o.GroupId = 0 and o.State<>{(int)MiniAppEntOrderState.已删除} { stateSql } ";
            
            return GetCountBySql(sqlstr + sqlWhere);
        }

        /// <summary>
        /// 申请取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ApplyReturnOrder(int orderId,ref string msg)
        {
            if (orderId <= 0)
            {
                msg = "订单Id不能为0";
                return false;
            }

            EntGoodsOrder model = base.GetModel(orderId);
            if (model == null)
            {
                msg = "订单不存在";
                return false;
            }

            if(!(model.State==(int)MiniAppEntOrderState.待发货 || model.State == (int)MiniAppEntOrderState.待自取 || model.State == (int)MiniAppEntOrderState.待服务))
            {
                msg = "该订单不支持取消";
                return false;
            }

            model.State = (int)MiniAppEntOrderState.申请取消订单;
            model.ApplyReturnTime = DateTime.Now;
            if(!base.Update(model, "State,ApplyReturnTime"))
            {
                msg = "更新订单状态失败";
                return false;
            }

            msg = "已提交取消订单申请，等待商家确认订单状态，2小时内未处理系统将自动退回您的支付账号";

            //发公众号消息给商家
            TemplateMsg_Gzh.ApplyCancelOrderTemplateMessageForEnt(model);
            return true;
        }

        /// <summary>
        /// 申请取消订单无人处理2个小时后自动发起退款
        /// </summary>
        /// <param name="timeLength"></param>
        /// <returns></returns>
        public bool StartApplyCancelOrderServer(int timeLength)
        {
            List<EntGoodsOrder> list = GetListApplyCancelService(timeLength);
            if (list == null || list.Count <= 0)
                return false;

            foreach (EntGoodsOrder item in list)
            {
                item.refundFee = item.BuyPrice;
                outOrder(item, (int)MiniAppEntOrderState.申请取消订单, item.BuyMode, isPartOut: true);
            }

            return true;
        }
    }

    public class EntAdminGoodsOrderBLL : BaseMySql<EntAdminGoodsOrder>
    {
        //查找订单列表
        public List<EntAdminGoodsOrder> GetAdminList(string where, int pagesize, int pageindex, out int totalCount, string goodsName = "", string accName = "", string accPhone = "", string orderState = "", bool export = false)
        {
            List<EntAdminGoodsOrder> list = new List<EntAdminGoodsOrder>();
            string sql;
            string sqlCount;

            sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from entgoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";
            //sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,user.NickName,user.TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders inner join c_userinfo user on user.Id=orders.UserId {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc {(pagesize == 0 ? "" : " limit " + (pageindex <= 0 ? 0 : pageindex - 1) * pagesize + "," + pagesize)}";
            if (export)//导出Excel的话，不需要分页
            {
                sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,orders.AccepterName as NickName,orders.AccepterTelePhone as TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from entgoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc";
                //sql = $@"select orders.Id,orders.OrderId,orders.BuyPrice,orders.UserId,user.NickName,user.TelePhone,orders.Message,orders.CreateDate,orders.State,orders.OrderNum,orders.Remark,orders.FreightPrice,orders.Address from miniappgoodsorder orders inner join c_userinfo user on user.Id=orders.UserId {(string.IsNullOrEmpty(where) ? "" : " where " + where)} order by Id desc";
            }
            sqlCount = $@"select count(*) from entgoodsorder orders {(string.IsNullOrEmpty(where) ? "" : " where " + where)}";

            //拼接数据
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    var cartlist = EntGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={model.Id}");
                    var detaillist = new List<EntOrderCardDetail>();
                    foreach (var item in cartlist)
                    {
                        var cart = new EntOrderCardDetail();
                        cart.Id = item.Id;
                        var goods = EntGoodsBLL.SingleModel.GetModel(item.FoodGoodsId);
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