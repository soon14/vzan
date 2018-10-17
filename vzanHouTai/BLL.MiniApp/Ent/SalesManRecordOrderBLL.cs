using DAL.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Ent
{
    public class SalesManRecordOrderBLL : BaseMySql<SalesManRecordOrder>
    {

        #region 单例模式
        private static SalesManRecordOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private SalesManRecordOrderBLL()
        {

        }

        public static SalesManRecordOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SalesManRecordOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion


        /// <summary>
        /// 获取推广订单列表
        /// </summary>
        /// <param name="strWhere"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<SalesManRecordOrder> GetListSalesManRecordOrder(string strWhere, int pageIndex = 1, int pageSize = 10, string strOrder = "Id desc", int group = 1)
        {
            string sql = $"select m.*,u.NickName,o.TelePhone from ( select * from SalesManRecordOrder group by orderId) m  LEFT join SalesManRecord r on r.Id=m.salesManRecordId LEFT join SalesMan o on r.salesManId=o.Id  LEFT join C_UserInfo u on o.UserId=u.Id   where {strWhere}   order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            if (group == 0)
            {//用于后台不按订单分组
                sql = $"select m.*,u.NickName,o.TelePhone from  SalesManRecordOrder  m  LEFT join SalesManRecord r on r.Id=m.salesManRecordId LEFT join SalesMan o on r.salesManId=o.Id  LEFT join C_UserInfo u on o.UserId=u.Id   where {strWhere}  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            }
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<SalesManRecordOrder> list = new List<SalesManRecordOrder>();
                while (dr.Read())
                {
                    SalesManRecordOrder salesManRecordOrder = GetModel(dr);
                    if (salesManRecordOrder != null && salesManRecordOrder.Id > 0)
                    {
                        salesManRecordOrder.NickName = dr["NickName"].ToString();
                        salesManRecordOrder.TelePhone = dr["TelePhone"].ToString();
                        list.Add(salesManRecordOrder);
                    }

                }
                return list.Count > 0 ? list : null;
            }


        }

        /// <summary>
        /// 获取推广订单条数根据条数
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public int GetSalesManRecordOrderCount(string strWhere, int group = 1)
        {
            string sql = $"select count(m.Id) from ( select * from SalesManRecordOrder group by orderId) m  LEFT join SalesManRecord r on r.Id=m.salesManRecordId LEFT join SalesMan o on r.salesManId=o.Id  LEFT join C_UserInfo u on o.UserId=u.Id  where {strWhere} ";
            if (group == 0)
            {
                sql = $"select count(m.Id) from  SalesManRecordOrder  m  LEFT join SalesManRecord r on r.Id=m.salesManRecordId LEFT join SalesMan o on r.salesManId=o.Id  LEFT join C_UserInfo u on o.UserId=u.Id  where {strWhere} ";
            }
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


        /// <summary>
        /// 根据分销员分组获取其订单数据统计
        /// </summary>
        /// <param name="strWhere"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<SalesMan> GetSalesManRecordOrdersGroupBySalesMan(int appId, string strWhere, int pageIndex = 1, int pageSize = 10, string strOrder = "Id desc")
        {

            string sql = $"select o.Id,o.TelePhone,o.Remark,u.NickName,o.AddTime from SalesMan o LEFT join C_UserInfo u on o.UserId=u.Id  where {strWhere}  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<SalesMan> listSalesMan = new List<SalesMan>();

                while (dr.Read())
                {
                    SalesMan salesMan = SalesManBLL.SingleModel.GetModel(dr);

                    if (salesMan != null)
                    {
                        string salesManRecordIds = string.Join(",", GetSalesManRecordIds(salesMan.Id, appId));

                        OrderSum orderSumPeople = GetOrderSum(appId, salesManRecordIds, 0);
                        OrderSum orderSumAuto = GetOrderSum(appId, salesManRecordIds, 1);
                        salesMan.Remark = Convert.ToString(dr["Remark"]);
                        salesMan.nickName = Convert.ToString(dr["NickName"]);
                        if (!string.IsNullOrEmpty(salesMan.Remark))
                        {
                            salesMan.nickName += $"({salesMan.Remark})";
                        }

                        salesMan.autoPayOrderCount = orderSumAuto.payOrderCount;
                        salesMan.autoPayOrderTotalCpsMoney = orderSumAuto.payOrderTotalCpsMoney;
                        salesMan.autoPayOrderTotalPrice = orderSumAuto.payOrderTotalPrice;

                        salesMan.peoplePayOrderCount = orderSumPeople.payOrderCount;
                        salesMan.peoplePayOrderTotalCpsMoney = orderSumPeople.payOrderTotalCpsMoney;
                        salesMan.peoplePayOrderTotalPrice = orderSumPeople.payOrderTotalPrice;
                        listSalesMan.Add(salesMan);


                    }

                }
                return listSalesMan.Count > 0 ? listSalesMan : null;
            }


        }


        /// <summary>
        /// 分销员分组获取其条数
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public int GetSalesManRecordOrdersGroupBySalesManCount(string strWhere)
        {
            string sql = $"select count(o.Id) from SalesMan o LEFT join C_UserInfo u on o.UserId=u.Id  where {strWhere} ";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


        /// <summary>
        /// 获取分销员的订单统计数据
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="salesManRecordIds"></param>
        /// <param name="state">0表示人工结算 1表示自动结算</param>
        /// <returns></returns>
        public OrderSum GetOrderSum(int appId, string salesManRecordIds, int state)
        {
            OrderSum orderSum = new OrderSum();
            if (!string.IsNullOrEmpty(salesManRecordIds))
            {
                string sql = $"select count(Id) as PayOrderCount,sum(orderMoney) as PayOrderTotalPrice,sum(cpsMoney) as PayOrderTotalCpsMoney from SalesManRecordOrder where appId={appId} and salesManRecordId in({salesManRecordIds}) and state={state}";

                using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
                {

                    while (dr.Read())
                    {
                        orderSum.payOrderCount = Convert.ToInt32(dr["PayOrderCount"]);
                        orderSum.payOrderTotalCpsMoney = (dr["PayOrderTotalCpsMoney"] == DBNull.Value ? 0 : Convert.ToDouble(dr["PayOrderTotalCpsMoney"]) * 0.01).ToString("0.00");
                        orderSum.payOrderTotalPrice = (dr["PayOrderTotalPrice"] == DBNull.Value ? 0 : Convert.ToDouble(dr["PayOrderTotalPrice"]) * 0.01).ToString("0.00");

                    }

                }
            }
            return orderSum;
        }


        /// <summary>
        /// 根据分销员Id获取有效的推广记录
        /// </summary>
        /// <param name="salesManId"></param>
        /// <returns></returns>
        public List<int> GetSalesManRecordIds(int salesManId, int appId)
        {
            List<int> salesManRecordIds = new List<int>();
            List<SalesManRecord> listSalesManRecord = new SalesManRecordBLL().GetList($"appId={appId} and salesManId={salesManId} and state=1");
            foreach (var item in listSalesManRecord)
            {
                salesManRecordIds.Add(item.Id);
            }

            return salesManRecordIds;
        }




    }
}
