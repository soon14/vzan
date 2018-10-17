using BLL.MiniApp.Helper;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Tools;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishOrderBLL : BaseMySql<DishOrder>
    {
        #region 单例模式
        private static DishOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishOrderBLL()
        {

        }

        public static DishOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region 订单查询 - 相关方法

        /// <summary>
        /// 根据参数返回相应结果
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="order_status">订单状态</param>
        /// <param name="order_type">订单类型</param>
        /// <param name="pay_status">支付状态</param>
        /// <param name="pay_id">支付类型</param>
        /// <returns></returns>
        public List<DishOrder> GetOrders(int aId = 0, int storeId = 0, string order_sn = "", int order_status = -1, int order_type = 0, int pay_status = -1, int pay_id = 0, DateTime? dateStart = null, DateTime? dateEnd = null, int pageIndex = 0, int pageSize = 0, int userId = 0, int is_ziqu = -1)
        {
            string whereSql = $"  aId = @aId and storeId = @storeId and is_delete = 0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            if (!string.IsNullOrWhiteSpace(order_sn))
            {
                whereSql += "and order_sn like @order_sn ";
                mysqlParams.Add(new MySqlParameter("@order_sn", "%" + order_sn + "%"));
            }
            if (order_status != -1)
            {
                whereSql += "and order_status = @order_status ";
                mysqlParams.Add(new MySqlParameter("@order_status", order_status));
            }
            if (order_type != 0)
            {
                whereSql += "and order_type = @order_type ";
                mysqlParams.Add(new MySqlParameter("@order_type", order_type));
            }
            if (pay_status != -1)
            {
                whereSql += "and pay_status = @pay_status ";
                mysqlParams.Add(new MySqlParameter("@pay_status", pay_status));
            }
            if (pay_id != 0)
            {
                whereSql += "and pay_id = @pay_id ";
                mysqlParams.Add(new MySqlParameter("@pay_id", pay_id));
            }
            if (dateStart!=null)
            {
                string dateStartStr = dateStart.Value.ToString("yyyy-MM-dd 00:00:00");
                whereSql += "and add_time >= @dateStart ";
                mysqlParams.Add(new MySqlParameter("@dateStart", dateStartStr));
            }
            if (dateEnd != null)
            {
                string dateEndStr = dateEnd.Value.AddDays(1).AddMilliseconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                whereSql += "and add_time <= @dateEnd ";
                mysqlParams.Add(new MySqlParameter("@dateEnd", dateEndStr));
            }
            if (userId > 0)
            {
                whereSql += "and user_id = @user_id ";
                mysqlParams.Add(new MySqlParameter("@user_id", userId));
            }
            if (is_ziqu > 0)
            {
                whereSql += "and is_ziqu = @is_ziqu ";
                mysqlParams.Add(new MySqlParameter("@is_ziqu", is_ziqu));
            }

            List<DishOrder> orders = new List<DishOrder>();
            if (pageIndex > 0 && pageSize > 0)
            {
                orders = base.GetListByParam(whereSql, mysqlParams.ToArray(), pageSize, pageIndex, "*", " id desc ");
            }
            else
            {
                orders = base.GetListByParam(whereSql, mysqlParams.ToArray(), 9999999, 1, "*", " id desc");
            }

            string userIds = string.Join(",",orders?.Select(s=>s.user_id).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
            orders.ForEach(o =>
            {
                o.user_name = userInfoList?.FirstOrDefault(f=>f.Id == o.user_id)?.NickName;
            });

            return orders;
        }

        /// <summary>
        /// 根据参数返回相应结果
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="order_status">订单状态</param>
        /// <param name="order_type">订单类型</param>
        /// <param name="pay_status">支付状态</param>
        /// <param name="pay_id">支付类型</param>
        /// <returns></returns>
        public int GetOrdersCount(int aId = 0, int storeId = 0, string order_sn = "", int order_status = -1, int order_type = 0, int pay_status = -1, int pay_id = 0, DateTime? dateStart = null, DateTime? dateEnd = null, int userId = 0, int is_ziqu = -1)
        {
            string whereSql = $"  aId = @aId and storeId = @storeId and is_delete = 0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));

            if (!string.IsNullOrWhiteSpace(order_sn))
            {
                whereSql += "and order_sn like @order_sn ";
                mysqlParams.Add(new MySqlParameter("@order_sn", "%" + order_sn + "%"));
            }
            if (order_status != -1)
            {
                whereSql += "and order_status = @order_status ";
                mysqlParams.Add(new MySqlParameter("@order_status", order_status));
            }
            if (order_type != 0)
            {
                whereSql += "and order_type = @order_type ";
                mysqlParams.Add(new MySqlParameter("@order_type", order_type));
            }
            if (pay_status != -1)
            {
                whereSql += "and pay_status = @pay_status ";
                mysqlParams.Add(new MySqlParameter("@pay_status", pay_status));
            }
            if (pay_id != 0)
            {
                whereSql += "and pay_id = @pay_id ";
                mysqlParams.Add(new MySqlParameter("@pay_id", pay_id));
            }
            if (dateStart != null)
            {
                string dateStartStr = dateStart.Value.ToString("yyyy-MM-dd 00:00:00");
                whereSql += "and add_time >= @dateStart ";
                mysqlParams.Add(new MySqlParameter("@dateStart", dateStartStr));
            }
            if (dateEnd != null)
            {
                string dateEndStr = dateEnd.Value.AddDays(1).AddMilliseconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                whereSql += "and add_time <= @dateEnd ";
                mysqlParams.Add(new MySqlParameter("@dateEnd", dateEndStr));
            }
            if (userId > 0)
            {
                whereSql += "and user_id = @user_id ";
                mysqlParams.Add(new MySqlParameter("@user_id", userId));
            }
            if (is_ziqu > 0)
            {
                whereSql += "and is_ziqu = @is_ziqu ";
                mysqlParams.Add(new MySqlParameter("@is_ziqu", is_ziqu));
            }

            return base.GetCount(whereSql, mysqlParams.ToArray());
        }

        /// <summary>
        /// 根据时间段获取销售额
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="payType">支付方式</param>
        /// <returns></returns>
        public double GetEarningsByDate(int storeId, DateTime startDate, DateTime endDate, int payType = -1)
        {
            string sql = $"select sum(settlement_total_fee) from DishOrder where storeid={storeId} and (order_status={(int)DishEnums.OrderState.已完成} or order_status={(int)DishEnums.OrderState.已确认})  and add_time between @startDate and @endDate";
            if (payType != -1)
            {
                sql += $" and pay_id={payType}";
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@startDate", startDate));
            parameters.Add(new MySqlParameter("@endDate", endDate));
            object sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parameters.ToArray());
            if (sum == DBNull.Value)
            {
                sum = 0;
            }
            //object wxSum=
            return Convert.ToDouble(sum);
        }

        public double GetEarnings(int storeId, int payType = -1)
        {
            string sql = $"select sum(settlement_total_fee) from DishOrder where storeid={storeId} and (order_status={(int)DishEnums.OrderState.已完成} or order_status={(int)DishEnums.OrderState.已确认})";
            if (payType != -1)
            {
                sql += $" and pay_id={payType}";
            }
            object sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            if (sum == DBNull.Value)
            {
                sum = 0;
            }
            return Convert.ToDouble(sum);
        }

        public double GetAccountSumByDate(int id, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取微信买单收入
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isByDate">是否根据日期查询</param>
        /// <returns></returns>
        public double GetWxMdIncomeByDate(int storeId, DateTime startDate, DateTime endDate, bool isByDate = true)
        {
            string sql = $"select sum(payment_free) from cityMorders where CommentId={storeId} and OrderType={(int)ArticleTypeEnum.DishStorePayTheBill} and Status=1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (isByDate)
            {
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
                sql += $" and Addtime between @startDate and @endDate";
            }
            object sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parameters.ToArray());
            if (sum == DBNull.Value)
            {
                sum = 0;
            }
            return Convert.ToDouble(sum) * 0.01;
        }

        /// <summary>
        /// 获取微信买单订单数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isByDate"></param>
        /// <returns></returns>
        public int GetWxMdCountByDate(int storeId, DateTime startDate, DateTime endDate, bool isByDate = true)
        {
            string sql = $"select count(1) from cityMorders where CommentId={storeId} and OrderType={(int)ArticleTypeEnum.DishStorePayTheBill} and Status=1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (isByDate)
            {
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
                sql += $" and Addtime between @startDate and @endDate";
            }
            object sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parameters.ToArray());
            if (sum == DBNull.Value)
            {
                sum = 0;
            }
            return Convert.ToInt32(sum);
        }

        /// <summary>
        /// 根据用户查询订单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="order_sn"></param>
        /// <param name="order_status"></param>
        /// <param name="order_type"></param>
        /// <param name="pay_status"></param>
        /// <param name="pay_id"></param>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<DishOrder> GetUserOrders(int storeId, int userId, string order_sn = "", int order_status = -1, int order_type = 0, int pay_status = 0, int pay_id = 0, DateTime? dateStart = null, DateTime? dateEnd = null, int pageIndex = 0, int pageSize = 0)
        {
            string whereSql = $"  user_id = @user_id and is_delete = 0 ";
            if (storeId > 0)
            {
                whereSql += $" and  storeId = {storeId} ";
            }

            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@user_id", userId));

            ////根据用户查询不查出已取消单据
            //whereSql += $" and order_status != {(int)DishEnums.OrderState.已取消} ";
            if (!string.IsNullOrWhiteSpace(order_sn))
            {
                whereSql += "and order_sn like @order_sn ";
                mysqlParams.Add(new MySqlParameter("@order_sn", "%" + order_sn + "%"));
            }
            if (order_status != -1) //查询全部
            {
                if (order_status == 2) //已完成/已取消
                {
                    whereSql += $" and (order_status = {(int)DishEnums.OrderState.已取消} or order_status = @order_status)";
                    mysqlParams.Add(new MySqlParameter("@order_status", order_status));
                }
                else
                {
                    whereSql += "and order_status = @order_status ";
                    mysqlParams.Add(new MySqlParameter("@order_status", order_status));
                }
            }
            if (order_type != 0)
            {
                whereSql += "and order_type = @order_type ";
                mysqlParams.Add(new MySqlParameter("@order_type", order_type));
            }
            if (pay_status != 0)
            {
                whereSql += "and pay_status = @pay_status ";
                mysqlParams.Add(new MySqlParameter("@pay_status", pay_status));
            }
            if (pay_id != 0)
            {
                whereSql += "and pay_id = @pay_id ";
                mysqlParams.Add(new MySqlParameter("@pay_id", pay_id));
            }
            if (dateStart != null)
            {
                string dateStartStr = dateStart.Value.ToString("yyyy-MM-dd 00:00:00");
                whereSql += "and add_time >= @dateStart ";
                mysqlParams.Add(new MySqlParameter("@dateStart", dateStartStr));
            }
            if (dateEnd != null)
            {
                string dateEndStr = dateEnd.Value.AddDays(1).AddMilliseconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                whereSql += "and add_time <= @dateEnd ";
                mysqlParams.Add(new MySqlParameter("@dateEnd", dateEndStr));
            }

            List<DishOrder> orders = new List<DishOrder>();
            if (pageIndex > 0 && pageSize > 0)
            {
                orders = base.GetListByParam(whereSql, mysqlParams.ToArray(), pageSize, pageIndex, "*", " id desc");
            }
            else
            {
                orders = base.GetListByParam(whereSql, mysqlParams.ToArray(), 1, 99999, "*", " id desc");
            }
            
            string userIds = string.Join(",", orders?.Select(s => s.user_id).Distinct());
            List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
            orders.ForEach(o =>
            {
                o.user_name = userInfoList?.FirstOrDefault(f=>f.Id == o.user_id)?.NickName;
            });
            return orders;
        }

        #endregion 订单查询 - 相关方法

        #region 订单批处理 - 相关方法

        /// <summary>
        /// 取消到期未支付订单<先就餐后付款的订单不取消>
        /// </summary>
        public bool CancelOrder()
        {
            string cancelOrderSql = $" update dishorder set order_status = {(int)DishEnums.OrderState.已取消} where pay_status = {(int)DishEnums.PayState.未付款} and  pay_end_time <= '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' and order_jiucan_type != 2 ";

            return ExecuteNonQuery(cancelOrderSql) > 0;
        }

        /// <summary>
        /// 批量改订单状态
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool BtachModifyOrderState(int[] ids, DishEnums.OrderState orderState)
        {
            string batchSql = $" update dishorder set order_status = {(int)orderState} where id in ({string.Join(",", ids)}) ";

            return ExecuteNonQuery(batchSql) > 0;
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool BtachDelete(int[] ids)
        {
            string batchSql = $" update dishorder set is_delete = 1 where id in ({string.Join(",", ids)}) ";

            return ExecuteNonQuery(batchSql) > 0;
        }

        /// <summary>
        /// 批量改支付状态
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool BtachModifyPayState(int[] ids, DishEnums.PayState payState)
        {
            string batchSql = $" update dishorder set pay_status = {(int)payState} where id in ({string.Join(",", ids)}) ";

            return ExecuteNonQuery(batchSql) > 0;
        }

        #endregion 订单批处理 - 相关方法

        #region 订单统计 - 相关方法

        /// <summary>
        /// 商品销量报表(退款不计入销量:<考虑菜品退款大多数情况下是因为菜品未上.故过滤退款菜品>)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable GoodSalesCountReport(int storeId, DateTime? startTime = null, DateTime? endTime = null)
        {
            string timeWhereSql = string.Empty;
            if (startTime != null)
            {
                timeWhereSql += $" and o.add_time >= '{startTime.Value.ToString("yyyy-MM-dd 00:00:00")}' ";
            }

            if (endTime != null)
            {
                timeWhereSql += $" and o.add_time <= '{endTime.Value.ToString("yyyy-MM-dd 23:59:59")}' ";
            }

            string sql = $@"select g.id as goods_id,g.g_name as goods_name,IFNULL(s.sales_count,0) as sales_count from dishgood g
                            left join (select c.goods_id,IFNULL(sum(c.goods_number),0) as sales_count from dishorder o
					                            left join dishshoppingcart c on o.id = c.order_id
					                            where o.storeId = {storeId} {timeWhereSql} and o.order_status in ({(int)DishEnums.OrderState.已确认},{(int)DishEnums.OrderState.已完成}) and o.is_delete = 0 and c.is_tuikuan = 0
					                            group by c.goods_id) s on g.id = s.goods_id
                            where g.storeid = {storeId} and g.state >= 0 order by s.sales_count asc";

            DataTable salesReport = new DataTable();
            salesReport.Columns.Add("goods_id", typeof(int));
            salesReport.Columns.Add("goods_name", typeof(string));
            salesReport.Columns.Add("sales_count", typeof(int));
            DataRow drReport = null;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    drReport = salesReport.NewRow();
                    drReport["goods_id"] = dr["goods_id"];
                    drReport["goods_name"] = dr["goods_name"];
                    drReport["sales_count"] = dr["sales_count"];

                    salesReport.Rows.Add(drReport);
                }
            }
            return salesReport;
        }

        /// <summary>
        /// 桌台统计(关乎金额:settlement_total_fee(订单最终金额) - refundmoney(此单已退款金额) 得出订单的实际有效付款金额)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable TableStatisticsReport(int storeId, DateTime? startTime = null, DateTime? endTime = null)
        {
            string timeWhereSql = string.Empty;
            if (startTime != null)
            {
                timeWhereSql += $" and total_o.add_time >= '{startTime.Value.ToString("yyyy-MM-dd 00:00:00")}' ";
            }

            if (endTime != null)
            {
                timeWhereSql += $" and total_o.add_time <= '{endTime.Value.ToString("yyyy-MM-dd 23:59:59")}' ";
            }

            string sql = $@"select t.id,t.table_name, count(0) as order_count,
                                sum(IFNULL(total_o.settlement_total_fee - total_o.refundmoney,0.00)) as all_total_fee,
                                sum(IFNULL(wx_o.settlement_total_fee - wx_o.refundmoney,0.00)) as wx_total_fee,
                                sum(IFNULL(else_o.settlement_total_fee - else_o.refundmoney,0.00)) as else_total_fee
                            from dishtable t
                            left join dishorder total_o ON t.id = total_o.order_table_id_zhen
                            left join dishorder wx_o ON wx_o.id = total_o.id and total_o.pay_id = 1
                            left join dishorder else_o ON else_o.id = total_o.id and total_o.pay_id != 1
                            where t.storeid = {storeId} AND t.state >= 0 {timeWhereSql} AND total_o.ORDER_STATUS IN (1,2)  AND total_o.IS_DELETE = 0 AND total_o.pay_status = 1
                            group by t.id";

            DataTable statisticsReport = new DataTable();
            statisticsReport.Columns.Add("id", typeof(int));
            statisticsReport.Columns.Add("table_name", typeof(string));
            statisticsReport.Columns.Add("order_count", typeof(int));
            statisticsReport.Columns.Add("all_total_fee", typeof(double));
            statisticsReport.Columns.Add("wx_total_fee", typeof(double));
            statisticsReport.Columns.Add("else_total_fee", typeof(double));
            DataRow drReport = null;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    drReport = statisticsReport.NewRow();
                    drReport["id"] = dr["id"];
                    drReport["table_name"] = dr["table_name"];
                    drReport["order_count"] = dr["order_count"];
                    drReport["all_total_fee"] = dr["all_total_fee"];
                    drReport["wx_total_fee"] = dr["wx_total_fee"];
                    drReport["else_total_fee"] = dr["else_total_fee"];

                    statisticsReport.Rows.Add(drReport);
                }
            }
            return statisticsReport;
        }

        /// <summary>
        /// 营业额报表(统计了所有的交易方式<包括线下交易>的金额)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable IncomeReport(int storeId, DateTime startTime, DateTime endTime)
        {
            if (startTime == null || endTime == null)
            {
                return null;
            }

            string timeWhereSql = string.Empty;
            timeWhereSql += $" and total_o.add_time >= '{startTime.ToString("yyyy-MM-dd HH:mm:ss")}' ";
            timeWhereSql += $" and total_o.add_time <= '{endTime.ToString("yyyy-MM-dd HH:mm:ss")}' ";

            //该sql若某天没有单据会不统计出结果,解决方式可在数据库建立辅助表。这里我认为代码里处理这个好些，可少链一次表
            string sql = $@"select
                            total_o.storeId,
                            DATE_FORMAT(total_o.add_time,'%Y-%m-%d') as order_date,

                            count(total_o.id) as order_count,
                            count(dn_o.id) as dn_order_count,
                            count(wm_o.id) as wm_order_count,
                            count(wx_o.id) as wx_order_count,
                            count(xj_o.id) as xj_order_count,
                            count(ye_o.id) as ye_order_count,

                            (SELECT count(0) from CityMorders cm where cm.MinisnsId=total_o.aId and cm.CommentId=total_o.storeId and cm.OrderType={(int)ArticleTypeEnum.DishStorePayTheBill} and DATE_FORMAT(cm.Addtime,'%Y-%m-%d')= DATE_FORMAT(total_o.add_time,'%Y-%m-%d') ) as wx_maidan_order_count,
(SELECT count(0) from DishCardAccountLog log where log.aid=total_o.aId and log.shop_id=total_o.storeId and log.account_type=2 and DATE_FORMAT(log.add_time,'%Y-%m-%d')= DATE_FORMAT(total_o.add_time,'%Y-%m-%d')) as ye_maidan_order_count,

                            sum(IFNULL(total_o.settlement_total_fee - total_o.refundmoney,0)) as total_total_fee,
                            sum(IFNULL(wx_o.settlement_total_fee - wx_o.refundmoney,0)) as wx_total_fee,
                            sum(IFNULL(xj_o.settlement_total_fee - xj_o.refundmoney,0)) as xj_total_fee,
                            sum(IFNULL(ye_o.settlement_total_fee - ye_o.refundmoney,0)) as ye_total_fee,

(SELECT sum(cm.payment_free)*0.01 from CityMorders cm where cm.MinisnsId=total_o.aId and cm.CommentId=total_o.storeId and cm.OrderType={(int)ArticleTypeEnum.DishStorePayTheBill} and DATE_FORMAT(cm.Addtime,'%Y-%m-%d')= DATE_FORMAT(total_o.add_time,'%Y-%m-%d') ) as wx_maidan_total_fee,
(SELECT sum(log.account_money) from DishCardAccountLog log where log.aid=total_o.aId and log.shop_id=total_o.storeId and log.account_type=2 and DATE_FORMAT(log.add_time,'%Y-%m-%d')= DATE_FORMAT(total_o.add_time,'%Y-%m-%d')) as ye_maidan_total_fee

                            from dishorder total_o
                            left join dishorder dn_o ON dn_o.id = total_o.id and total_o.order_type = 1
                            left join dishorder wm_o ON wm_o.id = total_o.id and total_o.order_type = 2
                            left join dishorder wx_o ON wx_o.id = total_o.id and total_o.pay_id = 1
                            left join dishorder xj_o ON xj_o.id = total_o.id and total_o.pay_id = 2
                            left join dishorder ye_o ON ye_o.id = total_o.id and total_o.pay_id = 4
                            left join dishorder wx_maidan_o ON wx_maidan_o.id = total_o.id and total_o.pay_id = 5
                            left join dishorder ye_maidan_o ON ye_maidan_o.id = total_o.id and total_o.pay_id = 6
                            where total_o.storeid = {storeId} {timeWhereSql} AND total_o.ORDER_STATUS IN ({(int)DishEnums.OrderState.已确认},{(int)DishEnums.OrderState.已完成}) AND total_o.IS_DELETE = 0
                            group by DATE_FORMAT(total_o.add_time,'%Y-%m-%d')";

            DataTable incomeReport = new DataTable();
            incomeReport.Columns.Add("storeId", typeof(int));
            incomeReport.Columns.Add("order_date", typeof(string));

            incomeReport.Columns.Add("order_count", typeof(int));
            incomeReport.Columns.Add("dn_order_count", typeof(int));
            incomeReport.Columns.Add("wm_order_count", typeof(int));
            incomeReport.Columns.Add("wx_order_count", typeof(int));
            incomeReport.Columns.Add("xj_order_count", typeof(int));
            incomeReport.Columns.Add("ye_order_count", typeof(int));

            incomeReport.Columns.Add("wx_maidan_order_count", typeof(int));
            incomeReport.Columns.Add("ye_maidan_order_count", typeof(int));

            incomeReport.Columns.Add("total_total_fee", typeof(double));
            incomeReport.Columns.Add("wx_total_fee", typeof(double));
            incomeReport.Columns.Add("xj_total_fee", typeof(double));
            incomeReport.Columns.Add("ye_total_fee", typeof(double));

            incomeReport.Columns.Add("wx_maidan_total_fee", typeof(double));
            incomeReport.Columns.Add("ye_maidan_total_fee", typeof(double));

            Func<string, DataRow> createDefaultRow = (date) =>
             {
                 DataRow dr = incomeReport.NewRow();

                 dr["storeId"] = storeId;
                 dr["order_date"] = startTime.ToString("yyyy-MM-dd");

                 dr["order_count"] = 0;
                 dr["dn_order_count"] = 0;
                 dr["wm_order_count"] = 0;
                 dr["wx_order_count"] = 0;
                 dr["xj_order_count"] = 0;
                 dr["ye_order_count"] = 0;

                 dr["wx_maidan_order_count"] = 0;
                 dr["ye_maidan_order_count"] = 0;

                 dr["total_total_fee"] = 0.00;
                 dr["wx_total_fee"] = 0.00;
                 dr["xj_total_fee"] = 0.00;
                 dr["ye_total_fee"] = 0.00;

                 dr["wx_maidan_total_fee"] = 0.00;
                 dr["ye_maidan_total_fee"] = 0.00;
                 return dr;
             }; //传入日期创建默认列

            DataRow drReport = null;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    while (!startTime.ToString("yyyy-MM-dd").Equals(dr["order_date"].ToString())) //数据库没有该日期则用默认数据添加一条
                    {
                        drReport = createDefaultRow(startTime.ToString("yyyy-MM-dd"));

                        incomeReport.Rows.Add(drReport);
                        startTime = startTime.AddDays(1);
                    }

                    drReport = incomeReport.NewRow();
                    drReport["storeId"] = dr["storeId"];
                    drReport["order_date"] = dr["order_date"];

                    drReport["order_count"] = dr["order_count"];
                    drReport["dn_order_count"] = dr["dn_order_count"];
                    drReport["wm_order_count"] = dr["wm_order_count"];
                    drReport["wx_order_count"] = dr["wx_order_count"];
                    drReport["xj_order_count"] = dr["xj_order_count"];
                    drReport["ye_order_count"] = dr["ye_order_count"];

                    drReport["wx_maidan_order_count"] = dr["wx_maidan_order_count"];
                    drReport["ye_maidan_order_count"] = dr["ye_maidan_order_count"];

                    drReport["total_total_fee"] = dr["total_total_fee"];
                    drReport["wx_total_fee"] = dr["wx_total_fee"];
                    drReport["xj_total_fee"] = dr["xj_total_fee"];
                    drReport["ye_total_fee"] = dr["ye_total_fee"];

                    drReport["wx_maidan_total_fee"] = Convert.IsDBNull(dr["wx_maidan_total_fee"]) ? 0.00 : dr["wx_maidan_total_fee"];
                    drReport["ye_maidan_total_fee"] = Convert.IsDBNull(dr["ye_maidan_total_fee"]) ? 0 : dr["ye_maidan_total_fee"];

                    incomeReport.Rows.Add(drReport);
                    startTime = startTime.AddDays(1);
                }

                while (startTime < endTime) //数据库没有该日期则用默认数据添加一条
                {
                    drReport = createDefaultRow(startTime.ToString("yyyy-MM-dd"));

                    incomeReport.Rows.Add(drReport);
                    startTime = startTime.AddDays(1);
                }
            }
            return incomeReport;
        }

        public int GetSaleCount(int storeId, int payType = -1)
        {
            string sqlwhere = $"storeid={storeId} and  (order_status={(int)DishEnums.OrderState.已完成} or order_status={(int)DishEnums.OrderState.已确认})";
            if (payType != -1)
            {
                sqlwhere += $" and pay_id={payType}";
            }
            return GetCount(sqlwhere);
        }

        public int GetSaleCountByDate(int storeId, DateTime startDate, DateTime endDate, int payType = -1)
        {
            int saleCount = 0;
            string sql = $"select count(1) from dishorder where storeid={storeId} and  (order_status={(int)DishEnums.OrderState.已完成} or order_status={(int)DishEnums.OrderState.已确认}) and add_time between @startDate and @endDate";
            if (payType != -1)
            {
                sql += $" and pay_id={payType}";
            }

            List<MySqlParameter> parames = new List<MySqlParameter>();
            parames.Add(new MySqlParameter("@startDate", startDate));
            parames.Add(new MySqlParameter("@endDate", endDate));
            saleCount = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parames.ToArray()) == DBNull.Value ? 0 : Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parames.ToArray()));
            return saleCount;
        }

        /// <summary>
        /// 查询门店销量=订单量+产品虚拟销量
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="type">0：月销量（默认值） 1:近30天销量</param>
        /// <returns></returns>
        public int GetXiaoLiang(int storeId, int type = 0)
        {
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            DateTime now = DateTime.Now;
            if (type == 0)
            {
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                endDate = new DateTime(now.Year, now.Month, now.Day);
                startDate = endDate.AddMonths(-1);
            }
            string sql = $"select count(1) from dishorder where storeId={storeId} and is_delete = 0 and (order_status={(int)DishEnums.OrderState.已完成} or order_status={(int)DishEnums.OrderState.已确认}) and add_time between '{startDate}' and '{endDate}'";
            int orderNum = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, sql));

            string sql2 = $" SELECT sum(yue_xiaoliang) from dishgood where storeid={storeId} and state<>-1";
            int virtualNum = 0;
            object obj = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql2);
            if (!Convert.IsDBNull(obj))
            {
                virtualNum = Convert.ToInt32(obj);
            }

            return orderNum + virtualNum;
        }

        /// <summary>
        /// 获取当前订单排号
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetNowHaoMa(int storeId)
        {
            object haoma = SqlMySql.ExecuteScalar(connName, CommandType.Text, $" select IFNULL(count(0),0) from DishOrder where storeId = {storeId} and ctime >= '{DateTime.Now.Date}'  ", null);
            return Convert.ToInt32(haoma) + 1;
        }

        #endregion 订单统计 - 相关方法

        #region 检索数据 - 相关方法

        /// <summary>
        /// 检查有否新的支付了的订单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="checkTime"></param>
        /// <returns></returns>
        public bool havingNewOrder(int storeId, DateTime checkTime)
        {
            string checkSql = $" storeId = {storeId} and add_time >= '{checkTime.ToString("yyyy-MM-dd HH:mm:ss")}' ";

            return Exists(checkSql);
        }

        /// <summary>
        /// 用户是否首次下单
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="orderId">订单ID,排除此ID不计</param>
        /// <returns></returns>
        public bool IsFristOrder(int user_id, int storeId, int orderId)
        {
            return !Exists($" user_id = {user_id} and storeId = {storeId} and id != {orderId} and is_delete = 0 ");
        }

        /// <summary>
        /// 判定是否可以配送并计算返回运费
        /// </summary>
        /// <param name="store"></param>
        /// <param name="u_lat"></param>
        /// <param name="u_lng"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public double GetOrderShipping_fee(DishStore store, double goodMoney, double u_lat, double u_lng, ref string errorMsg, string appid = "", string openid = "", string acceptername = "", string accepterphone = "", string address = "", string cityname = "")
        {
            //配送费
            double peisongfei = 0.00;
            //if (store.ps_open_status != 1) //配送跟运费是TM两回事
            //{
            //    //errorMsg = "商家未开启配送"; //不报错,商家不开配送是合理的,不计算运费
            //    return peisongfei;
            //}

            double lat = store.ws_lat;
            double lng = store.ws_lng;

            double distance = CommondHelper.GetDistance(lat, lng, u_lat, u_lng);
            distance = Math.Ceiling(distance); //配送距离有小数向上取整

            //不满足配送条件
            if (goodMoney < store.takeoutConfig.waimai_limit_jiner)
            {
                errorMsg = "订单金额不满足配送条件";
                return peisongfei;
            }
            //配送范围
            if (distance > store.takeoutConfig.waimai_limit_juli)
            {
                errorMsg = $"超出配送范围";
                return peisongfei;
            }
            string msg = "";
            switch (store.ps_type)
            {
                case (int)miniAppOrderGetWay.商家配送:
                    peisongfei = store.takeoutConfig.waimai_peisong_jiner;

                    //取到当前时段的配送费,若无则采用默认
                    string curDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    string nextDate = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd");
                    foreach (DishPsRule r in store.takeoutConfig.ps_rule)
                    {
                        if (!r.ps_time_b.IsNullOrWhiteSpace() && r.ps_time_b.Substring(0, 2).Equals("24"))
                        {
                            r.ps_time_b = $"{nextDate} 00:{r.ps_time_b.Substring(3, 2)}";
                        }
                        else
                        {
                            r.ps_time_b = $"{curDate} {r.ps_time_b}";
                        }

                        if (!r.ps_time_e.IsNullOrWhiteSpace() && r.ps_time_e.Substring(0, 2).Equals("24"))
                        {
                            r.ps_time_e = $"{nextDate} 00:{r.ps_time_e.Substring(3, 2)}";
                        }
                        else
                        {
                            r.ps_time_e = $"{curDate} {r.ps_time_e}";
                        }

                        if (DateTime.Now >= Convert.ToDateTime(r.ps_time_b) && DateTime.Now <= Convert.ToDateTime(r.ps_time_e))
                        {
                            peisongfei = r.ps_time_jiner;
                            break;
                        }
                    };

                    if (distance > store.takeoutConfig.waimai_peisong_base_juli)//超出基础配送距离
                    {
                        peisongfei += (Convert.ToInt32(distance) - store.takeoutConfig.waimai_peisong_base_juli) * store.takeoutConfig.waimai_peisong_base_step;
                    }
                    return peisongfei;

                case (int)miniAppOrderGetWay.达达配送:
                case (int)miniAppOrderGetWay.快跑者配送:
                case (int)miniAppOrderGetWay.UU配送:
                    //TODO:达达运费计算
                    peisongfei = DistributionApiConfigBLL.SingleModel.Getpeisongfei(cityname, appid, openid, u_lat.ToString(), u_lng.ToString(), acceptername, accepterphone, address, ref msg, store.ps_type, store.id, store.aid);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        errorMsg = msg;
                    }
                    return peisongfei * 0.01;
                //case (int)miniAppOrderGetWay.快跑者配送:
                //    //TODO:达达运费计算
                //    peisongfei = DistributionApiConfigBLL.SingleModel.Getpeisongfei(cityname, appid, openid, u_lat.ToString(), u_lng.ToString(), acceptername, accepterphone, address, ref msg, store.ps_type,store.id,store.aid);
                //    if (!string.IsNullOrEmpty(msg))
                //    {
                //        errorMsg = msg;
                //    }
                //    return peisongfei*0.01;
                default:
                    errorMsg = "无此配送方式";
                    return peisongfei;
            }
        }

        /// <summary>
        /// 检查库存,是否足够下单
        /// </summary>
        /// <param name="carts"></param>
        /// <returns></returns>
        public bool CheckStock(List<DishShoppingCart> carts)
        {
            DishGood curGood = new DishGood();

            string goodsIds = string.Join(",",carts?.Select(s=>s.goods_id).Distinct());
            List<DishGood> dishGoodList = DishGoodBLL.SingleModel.GetListByIds(goodsIds);
            foreach (DishShoppingCart c in carts)
            {
                curGood = dishGoodList?.FirstOrDefault(f=>f.id == c.goods_id);
                if (curGood == null)
                {
                    return false;
                }
                //商品库存是否足够
                if (curGood.day_kucun != -1 && GetGoodDaySalseCount(curGood.id, c.order_id) + c.goods_number > curGood.day_kucun)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 查询商品当天已下单库存 = (已售/将售)数量
        /// </summary>
        /// <param name="goods_id"></param>
        /// <returns></returns>
        public int GetGoodDaySalseCount(int goods_id, int orderId = 0)
        {
            //今日订单非删除非取消非退款且商品本身非退款的记录为今日的 已售/将售 数量
            int good_day_salesCount = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text,
                    $@"select IFNULL(sum(dishshoppingcart.goods_number),0) as good_day_salesCount from dishorder
                        left join dishshoppingcart on dishshoppingcart.order_id = dishorder.id
                        where dishorder.ctime >= '{DateTime.Now.Date}' and dishorder.is_delete = 0 and dishorder.order_status != {(int)DishEnums.OrderState.已取消} and dishshoppingcart.goods_id = {goods_id} and dishshoppingcart.is_tuikuan = 0 and dishorder.id != {orderId}", null));

            return good_day_salesCount;
        }

        public List<DishOrder> GetListByIds(string orderIds)
        {
            return GetList($"Id IN({orderIds})");
        }
        #endregion 检索数据 - 相关方法

        #region 填充订单资料 - 相关方法

        /// <summary>
        /// 完善订单_店内
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="order"></param>
        /// <param name="carts"></param>
        /// <param name="store"></param>
        /// <param name="result"></param>
        /// <param name="userInfo"></param>
        public void PerfectionOrder_DianNei(PostOrderInfo orderModel, DishOrder order, List<DishShoppingCart> carts, DishStore store, DishApiReturnMsg result, C_UserInfo userInfo)
        {
            //若添加菜品二次生成订单时,店主突然更换了就餐方式
            if (order.id > 0 && store.dianneiConfig.dish_diannei_fangshi != order.order_jiucan_type)
            {
                result.info = "门店已关闭这种就餐方式,无法生成订单";
                return;
            }

            //桌台号不存在
            DishTable table = DishTableBLL.SingleModel.GetModel(orderModel.dish_table_id);
            if (table == null)
            {
                result.info = "桌台号不存在";
                return;
            }
            //处理店铺营业时间字符串
            string curDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            string nextDate = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd");

            //是否在营业时段
            store.baseConfig.open_time.ForEach(o =>
            {
                if (!o.dish_open_btime.IsNullOrWhiteSpace() && o.dish_open_btime.Substring(0, 2).Equals("24"))
                {
                    o.dish_open_btime = $"{nextDate} 00:{o.dish_open_btime.Substring(3, 2)}";
                }
                else
                {
                    o.dish_open_btime = $"{curDate} {o.dish_open_btime}";
                }

                if (!o.dish_open_etime.IsNullOrWhiteSpace() && o.dish_open_etime.Substring(0, 2).Equals("24"))
                {
                    o.dish_open_etime = $"{nextDate} 00:{o.dish_open_etime.Substring(3, 2)}";
                }
                else
                {
                    o.dish_open_etime = $"{curDate} {o.dish_open_etime}";
                }
            });
            if (!store.baseConfig.open_time.Any(o => DateTime.Now >= Convert.ToDateTime(o.dish_open_btime) && DateTime.Now <= Convert.ToDateTime(o.dish_open_etime)))//是否营业
            {
                result.info = "门店店内业务当前时段不营业";
                return;
            }

            //因店内有先就餐后付款,故存在二次执行的可能.若为二次执行则查出该订单已包含的购物车内容重新参与计算价格
            if (order.id > 0)
            {
                carts.AddRange(DishShoppingCartBLL.SingleModel.GetCartsByOrderId(order.id, true) ?? new List<DishShoppingCart>());
            }

            //因店内有先就餐后付款,故存在二次执行的可能.若为二次执行则不重复更入标记性的内容
            if (order.id == 0)
            {
                order.order_sn = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{new Random().Next(0, 99999)}";//订单号
                order.order_haoma = GetNowHaoMa(store.id).ToString().PadLeft(3, '0'); //排号

                order.ctime = DateTime.Now;//订单创建时间
            }

            order.aId = store.aid;
            order.storeId = store.id;
            order.user_id = userInfo.Id;
            order.order_table_id_zhen = table.id;
            order.order_table_id = table.table_name;
            order.order_jiucan_type = store.dianneiConfig.dish_diannei_fangshi;//就餐类型
            order.order_type = orderModel.order_type;
            order.yongcan_renshu = orderModel.this_yongcan_renshu;
            order.post_info = orderModel.this_beizhu_info;
            order.add_time = DateTime.Now; //订单最后一次添加菜品的时间
            order.user_name = userInfo.NickName;
            order.goods_amount = carts.Sum(c => c.goods_price * c.goods_number); //商品金额
            order.pay_end_time = DateTime.Now.AddMinutes(store.gaojiConfig.dish_pay_limit_time);//必须在此时间内支付<此字段对 先就餐后支付 暂不存在任何意义>

            //完善发票信息,若有
            PerfectionOrder_Invoice(orderModel, order, result);
            if (result.info != null)
            {
                return;
            }

            //完善优惠券资料,若有
            PerfectionOrder_Quan(orderModel, order, result);
            if (result.info != null)
            {
                return;
            }

            //完善首单立减/满减资料
            PerfectionOrder_Jian(orderModel, order, store, userInfo, result);
            if (result.info != null)
            {
                return;
            }

            order.order_amount = order.goods_amount + order.dabao_fee + order.shipping_fee; //订单金额 <不计算任何优惠>
            order.settlement_total_fee = order.goods_amount + order.dabao_fee + order.shipping_fee - order.huodong_manjin_jiner - order.huodong_quan_jiner - order.huodong_shou_jiner;//总金额 <计算优惠>

            if (order.settlement_total_fee < 0)
            {
                order.settlement_total_fee = 0; //最低0元
            }
        }

        /// <summary>
        /// 完善订单_店内自取
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="order"></param>
        /// <param name="carts"></param>
        /// <param name="store"></param>
        /// <param name="result"></param>
        /// <param name="userInfo"></param>
        public void PerfectionOrder_DianNeiZiQu(PostOrderInfo orderModel, DishOrder order, List<DishShoppingCart> carts, List<DishGood> goods, DishStore store, DishApiReturnMsg result, C_UserInfo userInfo)
        {
            //自取验证自取人信息
            if (string.IsNullOrWhiteSpace(orderModel.ziqu_username))
            {
                result.info = "请填写自取人姓名";
                return;
            }
            if (string.IsNullOrWhiteSpace(orderModel.ziqu_userphone))
            {
                result.info = "请填写自取人号码";
                return;
            }

            //处理店铺营业时间字符串
            string curDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            string nextDate = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd");

            if (order.ziqu_time.Equals("立即取货")) //如果选择了立即取货,那么判定取货时间在不在设定的营业时间内
            {
                //是否在营业时段
                store.dianneiConfig.zq_time.ForEach(o =>
                {
                    if (!o.dish_zq_btime.IsNullOrWhiteSpace() && o.dish_zq_btime.Substring(0, 2).Equals("24"))
                    {
                        o.dish_zq_btime = $"{nextDate} 00:{o.dish_zq_btime.Substring(3, 2)}";
                    }
                    else
                    {
                        o.dish_zq_btime = $"{curDate} {o.dish_zq_btime}";
                    }

                    if (!o.dish_zq_etime.IsNullOrWhiteSpace() && o.dish_zq_etime.Substring(0, 2).Equals("24"))
                    {
                        o.dish_zq_etime = $"{nextDate} 00:{o.dish_zq_etime.Substring(3, 2)}";
                    }
                    else
                    {
                        o.dish_zq_etime = $"{curDate} {o.dish_zq_etime}";
                    }
                });
                if (!store.dianneiConfig.zq_time.Any(o => DateTime.Now >= Convert.ToDateTime(o.dish_zq_btime) && DateTime.Now <= Convert.ToDateTime(o.dish_zq_etime)))//是否营业
                {
                    result.info = "门店店内业务当前时段不营业";
                    return;
                }
            }
            //订单编号
            order.order_sn = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{new Random().Next(0, 99999)}";

            //基本设定
            order.aId = store.aid;
            order.storeId = store.id;
            order.user_id = userInfo.Id;
            order.order_type = orderModel.order_type;
            order.yongcan_renshu = orderModel.this_yongcan_renshu;
            order.post_info = orderModel.this_beizhu_info;
            order.ctime = order.add_time = DateTime.Now;
            order.pay_end_time = DateTime.Now.AddMinutes(store.gaojiConfig.dish_pay_limit_time);//10分钟后必须在10分钟内支付
            order.order_haoma = GetNowHaoMa(store.id).ToString().PadLeft(3, '0'); //排号
            order.user_name = userInfo.NickName;
            order.goods_amount = carts.Sum(c => c.goods_price * c.goods_number); //商品金额

            //自取资料填充
            order.is_ziqu = orderModel.is_ziqu;
            order.ziqu_username = orderModel.ziqu_username;
            order.ziqu_userphone = orderModel.ziqu_userphone;
            order.ziqu_time = orderModel.ziqu_time;
            //打包费
            order.dabao_fee = carts.Sum(c => goods.FirstOrDefault(g => g.id == c.goods_id).dabao_price * c.goods_number);

            //完善发票信息,若有
            PerfectionOrder_Invoice(orderModel, order, result);
            if (result.info != null)
            {
                return;
            }

            //完善优惠券资料,若有
            PerfectionOrder_Quan(orderModel, order, result);
            if (result.info != null)
            {
                return;
            }

            //完善首单立减/满减资料
            PerfectionOrder_Jian(orderModel, order, store, userInfo, result);
            if (result.info != null)
            {
                return;
            }

            order.order_amount = order.goods_amount + order.dabao_fee + order.shipping_fee; //订单金额 <不计算任何优惠>
            order.settlement_total_fee = order.goods_amount + order.dabao_fee + order.shipping_fee - order.huodong_manjin_jiner - order.huodong_quan_jiner - order.huodong_shou_jiner;//总金额 <计算优惠>

            if (order.settlement_total_fee < 0)
            {
                order.settlement_total_fee = 0; //最低0元
            }
        }

        /// <summary>
        /// 完善订单_店内自取
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="order"></param>
        /// <param name="carts"></param>
        /// <param name="store"></param>
        /// <param name="result"></param>
        /// <param name="userInfo"></param>
        public void PerfectionOrder_WaiMai(PostOrderInfo orderModel, DishOrder order, List<DishShoppingCart> carts, List<DishGood> goods, DishStore store, DishApiReturnMsg result, C_UserInfo userInfo, XcxAppAccountRelation xcxrelation)
        {
            if (orderModel.wx_address == null)
            {
                result.info = "请填写收货资料";
                return;
            }
            if (string.IsNullOrWhiteSpace(orderModel.wx_address.detailInfo))
            {
                result.info = "请填写收货地址";
                return;
            }
            if (string.IsNullOrWhiteSpace(orderModel.wx_address.userName))
            {
                result.info = "请填写收货人姓名";
                return;
            }
            if (string.IsNullOrWhiteSpace(orderModel.wx_address.telNumber))
            {
                result.info = "请填写收货人电话";
                return;
            }
            if (orderModel.wx_address.u_lat == 0.00d || orderModel.wx_address.u_lng == 0.00d)
            {
                result.info = "地址资料的坐标存在异常,请重新编辑该地址的定位";
                return;
            }

            //处理店铺营业时间字符串
            string curDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            string nextDate = DateTime.Now.Date.AddDays(1).ToString("yyyy-MM-dd");
            //是否在营业时段
            store.baseConfig.wm_time.ForEach(o =>
            {
                if (!o.dish_open_wm_btime.IsNullOrWhiteSpace() && o.dish_open_wm_btime.Substring(0, 2).Equals("24"))
                {
                    o.dish_open_wm_btime = $"{nextDate} 00:{o.dish_open_wm_btime.Substring(3, 2)}";
                }
                else
                {
                    o.dish_open_wm_btime = $"{curDate} {o.dish_open_wm_btime}";
                }

                if (!o.dish_open_wm_etime.IsNullOrWhiteSpace() && o.dish_open_wm_etime.Substring(0, 2).Equals("24"))
                {
                    o.dish_open_wm_etime = $"{nextDate} 00:{o.dish_open_wm_etime.Substring(3, 2)}";
                }
                else
                {
                    o.dish_open_wm_etime = $"{curDate} {o.dish_open_wm_etime}";
                }
            });
            if (!store.baseConfig.wm_time.Any(o => DateTime.Now >= Convert.ToDateTime(o.dish_open_wm_btime) && DateTime.Now <= Convert.ToDateTime(o.dish_open_wm_etime)))//是否营业
            {
                result.info = "门店外卖业务当前时段不营业";
                return;
            }
            //订单编号
            order.order_sn = $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{new Random().Next(0, 99999)}";

            //基本设定
            order.aId = store.aid;
            order.storeId = store.id;
            order.user_id = userInfo.Id;
            order.order_type = orderModel.order_type;
            order.yongcan_renshu = orderModel.this_yongcan_renshu;
            order.post_info = orderModel.this_beizhu_info;
            order.ctime = order.add_time = DateTime.Now;
            order.pay_end_time = DateTime.Now.AddMinutes(store.gaojiConfig.dish_pay_limit_time);//必须在此时间内支付<此字段对 先就餐后支付 暂不存在任何意义>
            order.order_haoma = GetNowHaoMa(store.id).ToString().PadLeft(3, '0'); //排号
            order.user_name = userInfo.NickName;
            order.goods_amount = carts.Sum(c => c.goods_price * c.goods_number); //商品金额
            order.dabao_fee = carts.Sum(c => goods.FirstOrDefault(g => g.id == c.goods_id).dabao_price * c.goods_number); //打包费

            //收货地址信息
            order.u_lat = orderModel.wx_address.u_lat;
            order.u_lng = orderModel.wx_address.u_lng;
            order.consignee = orderModel.wx_address.userName;
            order.mobile = orderModel.wx_address.telNumber;
            order.address = orderModel.wx_address.detailInfo;

            //获取了地址的资料
            WxHelper.txMap map = WxHelper.GetTXMapAddress(orderModel.wx_address.u_lat, orderModel.wx_address.u_lng);
            order.zipcode = map.result.address;
            order.country = map.result.address_component.nation;
            order.province = map.result.address_component.province;
            order.city = map.result.address_component.city;
            order.area = map.result.address_component.district;

            //配送方信息
            order.peisong_type = store.ps_type;
            order.peisong_status = (int)DishEnums.DeliveryState.待商家确认;

            //如果是外卖,计算运费及判定是否可以下单
            string errorMsg = string.Empty;
            order.peisong_amount = order.shipping_fee = GetOrderShipping_fee(store, order.goods_amount, order.u_lat, order.u_lng, ref errorMsg, xcxrelation.AppId, userInfo.OpenId, order.consignee, order.mobile, order.zipcode, order.city.Replace("市", "")); //外卖运费
            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                result.info = errorMsg;
                return;
            }

            //完善发票信息,若有
            PerfectionOrder_Invoice(orderModel, order, result);
            if (result.info != null)
            {
                return;
            }

            //完善优惠券资料,若有
            PerfectionOrder_Quan(orderModel, order, result);
            if (result.info != null)
            {
                return;
            }

            //完善首单立减/满减资料
            PerfectionOrder_Jian(orderModel, order, store, userInfo, result);
            if (result.info != null)
            {
                return;
            }

            order.order_amount = order.goods_amount + order.dabao_fee + order.shipping_fee; //订单金额 <不计算任何优惠>
            order.settlement_total_fee = order.goods_amount + order.dabao_fee + order.shipping_fee - order.huodong_manjin_jiner - order.huodong_quan_jiner - order.huodong_shou_jiner;//总金额 <计算优惠>

            if (order.settlement_total_fee < 0)
            {
                order.settlement_total_fee = 0; //最低0元
            }
        }

        /// <summary>
        /// 完善发票
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="order"></param>
        /// <param name="result"></param>
        public void PerfectionOrder_Invoice(PostOrderInfo orderModel, DishOrder order, DishApiReturnMsg result)
        {
            //发票
            if (orderModel.this_fapiao_id > 0)
            {
                DishInvoice invoice = DishInvoiceBLL.SingleModel.GetModel(orderModel.this_fapiao_id);
                if (invoice == null)
                {
                    result.info = "无效的发票资料设定ID";
                    return;
                }
                order.fapiao_text = invoice.fapiao_title;
                order.fapiao_no = invoice.fapiao_daima;
                order.fapiao_leixing_txt = invoice.fapiao_leixing == 1 ? "单位" : "个人";
                order.is_fapiao = 1;
            }
            else
            {
                order.is_fapiao = 0;
                order.fapiao_text = "不需要发票";
            }
        }

        /// <summary>
        /// 完善优惠券
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="order"></param>
        /// <param name="result"></param>
        public void PerfectionOrder_Quan(PostOrderInfo orderModel, DishOrder order, DishApiReturnMsg result)
        {
            //优惠券
            if (orderModel.quan_id > 0)
            {
                if (!orderModel.isReductionCard)
                {
                    DishActivityUser quan = DishActivityUserBLL.SingleModel.GetModel(orderModel.quan_id);
                    if (quan == null
                            || DateTime.Now < quan.quan_begin_time || DateTime.Now > quan.quan_end_time     //不在有效期内
                                || quan.quan_status == 1)   //已被使用
                    {
                        order.huodong_quan_id = 0;
                        result.info = "优惠券无效或当前未在其有效期内";
                        return;
                    }
                    if (order.goods_amount < quan.quan_limit_jiner)
                    {
                        order.huodong_quan_jiner = 0;
                        result.info = "未达到用条件";
                        return;
                    }
                    order.coupon_type = 0;
                    order.huodong_quan_id = quan.id;
                    order.huodong_quan_jiner = quan.quan_jiner;
                }
                else
                {
                    CouponLog reductionCard = CouponLogBLL.SingleModel.GetModel(orderModel.quan_id);
                    if (reductionCard == null || reductionCard.State != 0 || DateTime.Now < reductionCard.StartUseTime || DateTime.Now > reductionCard.EndUseTime)
                    {
                        result.info = "优惠券无效或当前未在其有效期内";
                        return;
                    }
                    Coupons coupons = CouponsBLL.SingleModel.GetModel(reductionCard.CouponId);
                    if (order.goods_amount < coupons.LimitMoney * 0.01)
                    {
                        order.huodong_quan_jiner = 0;
                        result.info = "未达到用条件";
                        return;
                    }
                    order.coupon_type = 1;
                    order.huodong_quan_id = reductionCard.Id;
                    order.huodong_quan_jiner = coupons.Money * 0.01;
                }
            }
        }

        /// <summary>
        /// 完善 首单立减/满减
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="order"></param>
        /// <param name="result"></param>
        public void PerfectionOrder_Jian(PostOrderInfo orderModel, DishOrder order, DishStore store, C_UserInfo userInfo, DishApiReturnMsg result)
        {
            //开启了首单立减
            if (store.gaojiConfig.huodong_shou_isopen == 1)
            {
                //是否首单
                bool isFirstOrder = IsFristOrder(userInfo.Id, store.id, order.id);
                if (isFirstOrder)
                {
                    order.huodong_shou_jiner = store.gaojiConfig.huodong_shou_jiner;
                }
                else
                {
                    order.huodong_shou_jiner = 0;
                }
            }
            //满减
            if (orderModel.manjian_id > 0 && order.huodong_shou_jiner == 0) //如果首单立减满足条件,不触发满减
            {
                DishActivity manjian = DishActivityBLL.SingleModel.GetModel(orderModel.manjian_id);
                if (manjian == null
                        || DateTime.Now < manjian.q_begin_time || DateTime.Now > manjian.q_end_time) //不在有效期内
                {
                    order.huodong_manjian_id = 0;
                    result.code = 0;
                    result.info = "找不到该满减活动或满减活动未在有效期内";
                    return;
                }
                if (order.goods_amount < manjian.q_xiaofei_jiner)
                {
                    order.huodong_manjin_jiner = 0;
                    result.code = 0;
                    result.info = "未达到用满减条件";
                    return;
                }

                order.huodong_manjin_jiner = manjian.q_diyong_jiner;
                order.huodong_manjian_id = manjian.id;
            }
        }

        #endregion 填充订单资料 - 相关方法

        #region 生成订单 - 相关方法

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="carts"></param>
        /// <param name="quan"></param>
        /// <returns></returns>
        public bool CreateOrder(DishOrder order, List<DishShoppingCart> carts, DishActivityUser quan,CouponLog reductionCard)
        {
            TransactionModel tran = new TransactionModel();
            if (order.id > 0) //二次下单(店内先用餐后付款会出现),只更新部分字段,为保留初次下单节点生成的一些标识
            {
                tran.Add(BuildUpdateSql(order, "order_status,pay_status,post_info,yongcan_renshu,order_table_id,order_table_id_zhen,fapiao_text,fapiao_no,fapiao_leixing_txt,quan_id,goods_amount,huodong_shou_jiner,huodong_quan_jiner,huodong_manjin_jiner,huodong_manjian_id,order_amount,dabao_fee,settlement_total_fee,add_time"));
                tran.Add($" update DishShoppingCart set order_id = {order.id} where id in ({string.Join(",", carts.Select(c => c.id))}) and IFNULL(order_id,0) = 0 ");
            }
            else
            {
                tran.Add(BuildAddSql(order));
                tran.Add($" update DishShoppingCart set order_id = (select last_insert_id()) where id in ({string.Join(",", carts.Select(c => c.id))}) and IFNULL(order_id,0) = 0 ");
            }

            if (quan != null)
            {
                //优惠券已使用
                quan.quan_status = 1;
                tran.Add(DishActivityUserBLL.SingleModel.BuildUpdateSql(quan, "quan_status"));
            }
            if (reductionCard != null)
            {
                //立减金已使用
                reductionCard.State = 1;
                tran.Add(CouponLogBLL.SingleModel.BuildUpdateSql(reductionCard, "State"));
            }
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        /// <summary>
        /// 重建订单，后台添加或删除未支付订单中的菜品
        /// </summary>
        /// <param name="order"></param>
        /// <param name="carts"></param>
        /// <returns></returns>
        public bool ReBuildOrder(DishOrder order, DishStore store, DishShoppingCart cartModel, List<DishShoppingCart> carts, List<DishGood> goods, DishReturnMsg _result)
        {
            TransactionModel tran = new TransactionModel();
            //更新购物车产品数量
            List<DishShoppingCart> cal_carts = JsonConvert.DeserializeObject<List<DishShoppingCart>>(JsonConvert.SerializeObject(carts));
            DishOrder pre_order = JsonConvert.DeserializeObject<DishOrder>(JsonConvert.SerializeObject(order));

            //删除数量为0的菜品
            if (cartModel.goods_number == 0)
            {
                cal_carts.Remove(cal_carts.Find(p => p.id == cartModel.id));
                tran.Add($" delete from DishShoppingCart where id={cartModel.id} ");
            }
            else
            {
                cal_carts.Find(p => p.id == cartModel.id).goods_number = cartModel.goods_number;
                tran.Add($" update DishShoppingCart set goods_number={cartModel.goods_number} where id={cartModel.id} ");
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(order.user_id);
            DishApiReturnMsg result = new DishApiReturnMsg();

            //重新计算金额
            order.goods_amount = cal_carts.Sum(c => c.goods_price * c.goods_number);//商品金额

            //店内自取、外卖计算打包费
            if ((order.order_type == (int)DishEnums.OrderType.店内 && order.is_ziqu == 1) || order.order_type == (int)DishEnums.OrderType.外卖)
            {
                order.dabao_fee = cal_carts.Sum(c => goods.FirstOrDefault(g => g.id == c.goods_id).dabao_price * c.goods_number);
            }
            else
            {
                order.dabao_fee = 0;
            }

            //如果是外卖,计算运费及判定是否可以下单，并计算打包费
            if (order.order_type == (int)DishEnums.OrderType.外卖)
            {
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(store.aid);
                string errorMsg = string.Empty;
                order.peisong_amount = order.shipping_fee = GetOrderShipping_fee(store, order.goods_amount, order.u_lat, order.u_lng, ref errorMsg, xcxrelation.AppId, userInfo.OpenId, order.consignee, order.mobile, order.zipcode, order.city.Replace("市", "")); //外卖运费
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    _result.code = 0;
                    _result.msg = errorMsg;
                    return false;
                }
            }


            //完善优惠券资料,若有
            PerfectionOrder_Quan(new PostOrderInfo { quan_id = order.huodong_quan_id }, order, result);
            //完善首单立减/满减资料
            PerfectionOrder_Jian(new PostOrderInfo { manjian_id = order.huodong_manjian_id }, order, store, userInfo, result);

            order.order_amount = order.goods_amount + order.dabao_fee + order.shipping_fee; //订单金额 <不计算任何优惠>
            order.settlement_total_fee = order.goods_amount + order.dabao_fee + order.shipping_fee - order.huodong_manjin_jiner - order.huodong_quan_jiner - order.huodong_shou_jiner;//总金额 <计算优惠>

            if (order.settlement_total_fee < 0)
            {
                order.settlement_total_fee = 0; //最低0元
            }
            //记录日志
            CommonLog log = new CommonLog
            {
                TemplateName = TmpType.智慧餐厅.ToString(),
                ActionType = CommonLogActionType.修改.ToString(),
                ModuleName = CommonLogModule.订单.ToString(),
                Info = $"修改订单产品 <br/>" +
                $"产品ID：{cartModel.id} <br/>" +
                $"产品名称：{cartModel.goods_name} <br/>" +
                $"产品数量：{carts.Find(p => p.id == cartModel.id)?.goods_number}=>{cartModel.goods_number} <br/>" +
                $"商品总金额：{pre_order.goods_amount}=>{order.goods_amount} <br/>" +
                $"订单金额：{pre_order.order_amount} =>{order.order_amount} <br/>" +
                $"总计：{pre_order.settlement_total_fee}=>{order.settlement_total_fee}",
                LogType = CommonLogType.Info.ToString(),
                StoreId = store.id,//门店ID
                AId = store.aid,
                UserName = $"{store.login_username}",
                AttachId = order.id
            };
            tran.Add(CommonLogBLL.SingleModel.BuildAddSql(log));

            //order.add_time = DateTime.Now; //订单最后一次更新菜品的时间 //add_time

            tran.Add(BuildUpdateSql(order, "goods_amount,order_amount,settlement_total_fee,huodong_quan_id,huodong_quan_jiner,huodong_shou_jiner,huodong_manjian_id,huodong_manjin_jiner,dabao_fee,coupon_type"));
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        #endregion 生成订单 - 相关方法

        #region 订单支付 - 相关方法

        /// <summary>
        /// 支付订单并处理数据
        /// </summary>
        /// <returns></returns>
        public bool PayOrderDisposeDB(DishOrder order, ref string msg, TransactionModel tran = null)
        {
            //是否要将sql加入tran返回
            bool isAddTran = tran != null;
            if (!isAddTran)
            {
                tran = new TransactionModel();
            }

            order.pay_time = DateTime.Now;
            switch (order.pay_id)
            {
                case (int)DishEnums.PayMode.微信支付:
                    order.pay_status = (int)DishEnums.PayState.已付款;
                    order.order_status = (int)DishEnums.OrderState.已确认;
                    tran.Add(BuildUpdateSql(order, "pay_id,pay_status,order_status,pay_time"));

                    //商户添加收益
                    if (!DishStoreEarningsBLL.SingleModel.AddStoreEarning(order.aId, order.storeId, order.settlement_total_fee, DishEnums.EarningsType.支付, $" 用户支付,订单号:{order.order_sn} ", ref msg, tran))
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception($"订单号:{order.order_sn}增加收益的sql生成失败"));
                        return false;
                    }
                    break;

                case (int)DishEnums.PayMode.余额支付:
                    order.pay_status = (int)DishEnums.PayState.已付款;
                    order.order_status = (int)DishEnums.OrderState.已确认;
                    tran.Add(BuildUpdateSql(order, "pay_id,pay_status,order_status,pay_time"));

                    //用户余额扣费
                    if (!PayOrderByBalance(order, ref msg, tran))
                    {
                        log4net.LogHelper.WriteError(GetType(), new Exception($"订单号:{order.order_sn}余额扣费的sql生成失败,{msg}"));
                        return false;
                    }
                    break;

                case (int)DishEnums.PayMode.线下支付:
                case (int)DishEnums.PayMode.货到支付:
                    order.pay_status = (int)DishEnums.PayState.已付款;
                    order.order_status = (int)DishEnums.OrderState.未确认;
                    tran.Add(BuildUpdateSql(order, "pay_id,pay_status,order_status,pay_time"));
                    break;

                default:
                    return false;
            }

            if (isAddTran)
            {
                return true;
            }
            else
            {
                return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            }
        }

        /// <summary>
        /// 支付订单后触发操作
        /// </summary>
        /// <param name="order"></param>
        public void AfterPayOrderOperation(DishOrder order)
        {
            //外卖下单<方法内逻辑只有在外卖配送才会去下单>
            string msg = string.Empty;
            if (!PostOrderPeisong(order, ref msg))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $" 智慧餐厅 - 支付后第三方配送下单失败:{msg} ");
            }

            try
            {
                //发 下单成功 模板消息
                DishStore store = DishStoreBLL.SingleModel.GetModelByAid_Id(order.aId, order.storeId);
                if (store != null)
                {
                    object messageData = TemplateMsg_Miniapp.DishMessageData(storeName: store.dish_name, sendMsgType: SendTemplateMessageTypeEnum.智慧餐厅订单支付成功, dishOrder: order);
                    TemplateMsg_Miniapp.SendTemplateMessage(order.user_id, SendTemplateMessageTypeEnum.智慧餐厅订单支付成功, (int)TmpType.智慧餐厅, messageData, $"pages/restaurant/restaurant-order-info/index?oid={order.id}", bigKeyword: "keyword1.DATA");
                    if (!string.IsNullOrEmpty(store.notifyOpenId))
                    {
                        TemplateMsg_Gzh.SendOrderSuccessTemplateMessage(order, store);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
            }

            //支付后打印的打印机打印订单
            PrinterHelper.DishPrintOrderByPrintType(order, 2);
        }

        /// <summary>
        /// 余额扣费
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool PayOrderByBalance(DishOrder order, ref string errMsg, TransactionModel tran = null)
        {
            //是否要将sql加入tran返回
            bool isAddTran = tran != null;
            if (!isAddTran)
            {
                tran = new TransactionModel();
            }

            //会员卡余额
            DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(order.storeId, order.user_id);
            if (vipCard == null)
            {
                errMsg = "未成为本店会员,不支持余额";
                return false;
            }

            //余额扣费
            DishCardAccountLog log = new DishCardAccountLog();
            if (vipCard.account_balance < order.settlement_total_fee)
            {
                errMsg = "余额不足,请充值后再支付";
                return false;
            }
            vipCard.account_balance = vipCard.account_balance - order.settlement_total_fee;

            tran.Add(DishVipCardBLL.SingleModel.BuildUpdateSql(vipCard, "account_balance"));
            DishCardAccountLogBLL.SingleModel.AddRecordLog(vipCard, 2, order.settlement_total_fee, $" 支付订单 {order.order_sn},消费￥{order.settlement_total_fee}元 ", tran);

            if (isAddTran)
            {
                return true;
            }
            else
            {
                return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            }
        }

        /// <summary>
        /// 生成citymorder
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public int CreateCityOrder(DishOrder order)
        {
            int cityMorderId = 0; //citymorder表ID
            XcxAppAccountRelation xcx = XcxAppAccountRelationBLL.SingleModel.GetModel(order.aId);
            if (xcx == null)
            {
                return cityMorderId;
            }

            string no = WxPayApi.GenerateOutTradeNo();
            CityMorders citymorderModel = new CityMorders
            {
                OrderType = (int)ArticleTypeEnum.DishOrderPay,
                ActionType = (int)ArticleTypeEnum.DishOrderPay,
                Addtime = DateTime.Now,
                payment_free = Convert.ToInt32(order.settlement_total_fee * 100),
                trade_no = no,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                FuserId = order.user_id,
                Fusername = order.user_name,
                orderno = no,
                payment_status = 0,
                Status = 0,
                Articleid = 0,
                CommentId = order.id,//订单Id
                MinisnsId = order.aId,// 订单aId
                TuserId = order.user_id,
                ShowNote = $" {xcx.Title}购买商品支付{order.settlement_total_fee}元",
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                buy_num = 0, //无
                appid = xcx.AppId,
            };
            cityMorderId = Convert.ToInt32(new CityMordersBLL().Add(citymorderModel));

            return cityMorderId;
        }

        #endregion 订单支付 - 相关方法

        #region 订单退款 - 相关方法

        /// <summary>
        /// 根据购物车ID集合退款
        /// </summary>
        /// <param name="cartIds"></param>
        /// <param name="result"></param>
        public void RefundOrderByCartIds(int[] cartIds, DishReturnMsg result)
        {
            if (cartIds?.Length > 0)
            {
                TransactionModel tranModel = new TransactionModel();

                double totalRefundMoney = 0.00;//退款总金额
                DishOrder order = null;
                List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetCartsByIds(cartIds);//选择要退款的菜品
                if (carts == null || carts.Count == 0)
                {
                    result.code = 0;
                    result.msg = "退款失败,选择的菜品无符合退款条件的菜品";
                    return;
                }
                //生成sql - 改订单商品状态为退款
                carts.ForEach(c =>
                {
                    c.is_tuikuan = 1;
                    tranModel.Add(DishShoppingCartBLL.SingleModel.BuildUpdateSql(c, "is_tuikuan"));
                });
                totalRefundMoney = carts.Sum(c => c.goods_price * c.goods_number);
                //订单已退款金额更新
                order = base.GetModel(carts[0].order_id);
                if (order == null)
                {
                    result.code = 0;
                    result.msg = "退款失败,无找到退款的源订单";
                    return;
                }
                if (order.pay_time == Convert.ToDateTime("0001-01-01 00:00:00"))
                {
                    result.code = 0;
                    result.msg = "该订单或未通过正常渠道付款故无法退款";
                    return;
                }
                order.refundMoney = order.refundMoney + totalRefundMoney;
                if (order.refundMoney > order.settlement_total_fee)
                {
                    result.code = 0;
                    result.msg = "退款失败,退款金额大于订单总额";
                    return;
                }
                tranModel.Add(BuildUpdateSql(order, "refundMoney") + $" and  refundMoney + {totalRefundMoney} <= settlement_total_fee");
                RefundOrder(order.user_id, order.aId, order.storeId, order.order_sn, Convert.ToInt32(totalRefundMoney * 100), order.pay_id, result, order.cityMordersId, tranModel);
                if (result.code == 0)
                {
                    return;
                }

                //执行退款流程所有sql
                if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
                {
                    result.code = 1;
                    result.msg = "退款成功";
                    return;
                }
                else
                {
                    result.code = 0;
                    result.msg = "退款失败,系统繁忙,请刷新页面重试";
                    return;
                }
            }
        }

        /// <summary>
        /// 根据订单ID退款
        /// </summary>
        /// <param name="cartIds"></param>
        /// <param name="result"></param>
        /// <param name="refundReason">退款原因</param>
        public void RefundOrderById(int id, DishReturnMsg result, string refundReason = "")
        {
            TransactionModel tranModel = new TransactionModel();

            //订单状态更新
            DishOrder order = base.GetModel(id);
            if (order == null)
            {
                result.code = 0;
                result.msg = "退款失败,未找到退款的源订单";
                return;
            }
            if (order.pay_time == Convert.ToDateTime("0001-01-01 00:00:00"))
            {
                result.code = 0;
                result.msg = "该订单或未通过正常渠道付款故无法退款";
                return;
            }
            double totalRefundMoney = order.settlement_total_fee - order.refundMoney; //退款总金额
            if (string.IsNullOrEmpty(refundReason))
            {
                refundReason = "商家发起退款";
            }

            order.refundMoney = order.settlement_total_fee;
            order.pay_status = (int)DishEnums.PayState.已退款;
            order.order_status = (int)DishEnums.OrderState.已取消;
            order.refundReason = refundReason;
            tranModel.Add(BuildUpdateSql(order, "refundMoney,pay_status,order_status,refundReason"));

            //购物车状态更新
            List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(order.id, true);
            if (carts != null)
            {
                carts.ForEach(c =>
                {
                    c.is_tuikuan = 1;
                    tranModel.Add(DishShoppingCartBLL.SingleModel.BuildUpdateSql(c, "is_tuikuan"));
                });
            }

            //根据订单支付形式走不同退款流程,生成不同sql
            RefundOrder(order.user_id, order.aId, order.storeId, order.order_sn, Convert.ToInt32(totalRefundMoney * 100), order.pay_id, result, order.cityMordersId, tranModel);
            if (result.code == 0)
            {
                return;
            }

            //执行退款流程所有sql
            if (ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray))
            {
                if (order.peisong_type == (int)miniAppOrderGetWay.快跑者配送)
                {
                    //配送取消
                    KPZOrderBLL.SingleModel.CancelOrder(order.aId, order.storeId, order.id);
                }

                try
                {
                    DishStore store = DishStoreBLL.SingleModel.GetModel(order.storeId);
                    if (store != null)
                    {
                        object messageData = TemplateMsg_Miniapp.DishMessageData(sendMsgType: SendTemplateMessageTypeEnum.智慧餐厅退款通知, storeName: store.dish_name, dishOrder: order);
                        TemplateMsg_Miniapp.SendTemplateMessage(order.user_id, SendTemplateMessageTypeEnum.智慧餐厅退款通知, (int)TmpType.智慧餐厅, messageData, $"pages/restaurant/restaurant-order-info/index?oid={order.id}", bigKeyword: "keyword1.DATA");//
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(GetType(), ex);
                }


                result.code = 1;
                result.msg = "退款成功";
                return;
            }
            else
            {
                result.code = 0;
                result.msg = "退款失败,系统繁忙,请刷新页面重试";
                return;
            }
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="cityMordersId">cityMordersId,非微信退款可不传</param>
        /// <param name="refundMoey">退款金额,单位(分)</param>
        /// <param name="payMode">退款渠道(订单下单的支付方式)</param>
        /// <param name="result">处理结果</param>
        public void RefundOrder(int userId, int aId, int storeId, string order_no, int refundMoey, int payMode, DishReturnMsg result, int cityMordersId = 0, TransactionModel tran = null)
        {
            bool isTran = tran == null; //是否传入了tran获取sql,若是则添加执行sql不执行tran,若不是则直接执行sql
            if (isTran)
            {
                tran = new TransactionModel();
            }

            switch (payMode)
            {
                case (int)DishEnums.PayMode.微信支付:
                    CityMorders cityMorder = new CityMordersBLL().GetModel(cityMordersId);
                    if (cityMorder == null)
                    {
                        result.code = 0;
                        result.msg = "退款失败,未找到退款的源微信订单";
                        return;
                    }
                    if (cityMorder.payment_status == 0) //微信订单未支付
                    {
                        result.code = 0;
                        result.msg = "退款失败,该订单未支付";
                        return;
                    }

                    ReFundQueue reModel = new ReFundQueue
                    {
                        minisnsId = -5,
                        money = refundMoey,
                        orderid = cityMorder.Id,
                        traid = cityMorder.trade_no,
                        addtime = DateTime.Now,
                        note = "小程序餐饮多门店版退款",
                        retype = 1
                    };
                    tran.Add(new ReFundQueueBLL().BuildAddSql(reModel));
                    //收益变动
                    string errMsg = string.Empty;
                    DishStoreEarningsBLL.SingleModel.AddStoreEarning(aId, storeId, refundMoey * 0.01, DishEnums.EarningsType.退款, $" 用户退款,来源订单号:{order_no} ", ref errMsg, tran);
                    if (!string.IsNullOrWhiteSpace(errMsg))
                    {
                        result.code = 0;
                        result.msg = "退款失败,收益变动记录生成失败";
                        return;
                    }

                    if (isTran)
                    {
                        if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                        {
                            result.code = 0;
                            result.msg = "加入退款队列失败";
                            return;
                        }
                    }

                    result.code = 1;
                    result.msg = "加入退款队列成功,将在1-7个工作日内响应退款";
                    return;

                case (int)DishEnums.PayMode.余额支付:
                    DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(storeId, userId);
                    vipCard.account_balance += refundMoey * 0.01;
                    tran.Add(DishVipCardBLL.SingleModel.BuildUpdateSql(vipCard, "account_balance"));
                    DishCardAccountLogBLL.SingleModel.AddRecordLog(vipCard, 1, refundMoey * 0.01, $" 订单 {order_no},退款￥{refundMoey * 0.01}元 ", tran);
                    if (isTran)
                    {
                        if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                        {
                            result.code = 0;
                            result.msg = "退款失败";
                            return;
                        }
                    }
                    result.code = 1;
                    result.msg = "退款成功";
                    return;

                default:
                    result.code = 0;
                    result.msg = "不支持该退款渠道";
                    return;
            }
        }

        #endregion 订单退款 - 相关方法

        #region 订单导出 - 相关方法

        /// <summary>
        /// 拼接要导出的数据表结构
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="order_status">订单状态</param>
        /// <param name="order_type">订单类型</param>
        /// <param name="pay_status">支付状态</param>
        /// <param name="pay_id">支付类型</param>
        /// <returns></returns>
        public void ExportOrders(int aId, int storeId, string order_sn = "", int order_status = 0, int order_type = 0, int pay_status = 0, int pay_id = 0, DateTime? dateStart = null, DateTime? dateEnd = null, int is_ziqu = 0)
        {
            List<DishOrder> orders = GetOrders(aId, storeId, order_sn, order_status, order_type, pay_status, pay_id, dateStart, dateEnd, is_ziqu: is_ziqu) ?? new List<DishOrder>();

            DataTable dtOrders = new DataTable();
            dtOrders.Columns.Add("ID");
            dtOrders.Columns.Add("订单编号");
            dtOrders.Columns.Add("用户名");
            dtOrders.Columns.Add("桌号");
            dtOrders.Columns.Add("排号");
            dtOrders.Columns.Add("订单类型");
            dtOrders.Columns.Add("订单状态");
            dtOrders.Columns.Add("支付状态");
            dtOrders.Columns.Add("支付方式");
            dtOrders.Columns.Add("订单金额");
            dtOrders.Columns.Add("订单时间");

            DataRow drOrders;
            foreach (DishOrder item in orders)
            {
                drOrders = dtOrders.NewRow();

                drOrders["ID"] = item.id;
                drOrders["订单编号"] = item.order_sn;
                drOrders["用户名"] = item.user_name;
                drOrders["桌号"] = item.order_table_id;
                drOrders["排号"] = item.order_haoma;
                drOrders["订单类型"] = item.order_type_txt;
                drOrders["订单状态"] = item.order_status_txt;
                drOrders["支付状态"] = item.pay_name;
                drOrders["支付方式"] = item.pay_status_txt;
                drOrders["订单金额"] = item.settlement_total_fee;
                drOrders["订单时间"] = item.add_time.ToString("yyyy-MM-dd HH:mm:ss");

                dtOrders.Rows.Add(drOrders);
            }

            //加默认列让这个表一定有数据
            dtOrders.Rows.Add(dtOrders.NewRow());
            ExcelHelper<DishOrder>.Out2Excel(dtOrders, $"{DateTime.Now.ToString("yyyy-MM-dd")}_订单记录"); //导出
        }

        #endregion 订单导出 - 相关方法

        /// <summary>
        /// 外卖下单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool PostOrderPeisong(DishOrder order, ref string msg)
        {
            DishStore store = DishStoreBLL.SingleModel.GetModel(order.storeId);
            if (store == null)
            {
                msg = "店铺资料错误";
                return false;
            }
            //对接物流平台
            DistributionApiConfigBLL distributionApiConfigBLL = DistributionApiConfigBLL.SingleModel;
            msg = distributionApiConfigBLL.AddDistributionOrder(store.aid, order.user_id, order.city.Replace("市", ""), order.u_lat.ToString(), order.u_lng.ToString(), store.ps_type, order, new object(), (int)TmpType.智慧餐厅, order.storeId, Convert.ToInt32(order.shipping_fee * 100));
            if (!string.IsNullOrEmpty(msg))
            {
                return false;
            }

            TransactionModel tran = new TransactionModel();
            msg = distributionApiConfigBLL.UpdatePeiSongOrder(order.id, order.aId, (int)TmpType.智慧餐厅, order.peisong_type, ref tran, true, store.id);
            if (!string.IsNullOrEmpty(msg))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), msg);
                return false;
            }
            return true;
        }
    }
}