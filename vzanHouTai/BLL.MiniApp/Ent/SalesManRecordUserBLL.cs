using DAL.Base;
using Entity.MiniApp.Ent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Ent
{
    public class SalesManRecordUserBLL : BaseMySql<SalesManRecordUser>
    {
        #region 单例模式
        private static SalesManRecordUserBLL _singleModel;
        private static readonly object SynObject = new object();

        private SalesManRecordUserBLL()
        {

        }

        public static SalesManRecordUserBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SalesManRecordUserBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取累计客户列表
        /// </summary>
        /// <param name="strWhere"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<SalesManRecordUser> GetListSalesManRecordUser(int appId,int salesManId,int userId, int state=0, int pageIndex = 1, int pageSize = 10, string strOrder = "t.Id desc")
        {


            //TODO 测试使用分钟MINUTE 线上使用天Day 分销
            //   string sql = $"SELECT t.*,u.NickName,u.HeadImgUrl from (SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId UNION   SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time  DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id GROUP BY t.userId  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            string sql = $"SELECT t.*,u.NickName,u.HeadImgUrl from (SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId UNION   SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time  DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id GROUP BY t.userId  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            if (state == 1)
            {
                //表示未失效的
                // sql = $"SELECT t.*,u.NickName,u.HeadImgUrl from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
                sql = $"SELECT t.*,u.NickName,u.HeadImgUrl from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            }
            if (state == 2)
            {
                //表示已失效的
               // sql = $"SELECT t.*,u.NickName,u.HeadImgUrl from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
                sql = $"SELECT t.*,u.NickName,u.HeadImgUrl from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id  order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";
            }

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<SalesManRecordUser> list = new List<SalesManRecordUser>();
                while (dr.Read())
                {
                    SalesManRecordUser salesManRecordUser = GetModel(dr);
                    salesManRecordUser = GetRecordUserOrderCountMoneyCps(salesManRecordUser);
                    salesManRecordUser.NickName = dr["NickName"].ToString();
                    salesManRecordUser.ImgLogo = dr["HeadImgUrl"].ToString();
                    salesManRecordUser.cur_protected_time = Convert.ToInt32(dr["cur_protected_time"])>0 ? Convert.ToInt32(dr["cur_protected_time"]):0;

                    list.Add(salesManRecordUser);


                }
                return list.Count > 0 ? list : null;
            }


        }

        /// <summary>
        /// 获取累计客户条数根据条件
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public int GetSalesManRecordUserCount(int appId, int salesManId, int userId, int state = 0)
        {
            //TODO 测试使用分钟MINUTE 线上使用天Day 分销
           // string sql = $"SELECT COUNT(Id) as number from ( SELECT t.Id from (SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId UNION   SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time  DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id GROUP BY t.userId) w";
            string sql = $"SELECT COUNT(Id) as number from ( SELECT t.Id from (SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId UNION   SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time  DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id GROUP BY t.userId) w";

            if (state == 1)
            {
                //表示未失效的
            //    sql = $"SELECT COUNT(Id) as number from (SELECT t.Id from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id ) w ";
                sql = $"SELECT COUNT(Id) as number from (SELECT t.Id from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time>0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id ) w ";
            }
            if (state == 2)
            {
                //表示已失效的
              //  sql = $"SELECT COUNT(Id) as number from (SELECT t.Id from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time MINUTE),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id ) w";
                sql = $"SELECT COUNT(Id) as number from (SELECT t.Id from (  SELECT * from (  SELECT *,datediff( DATE_ADD(UpdateTime,INTERVAL protected_time Day),now()) as cur_protected_time  from  SalesManRecordUser ORDER BY cur_protected_time DESC ) m WHERE m.cur_protected_time<=0 and m.appId={appId} and m.salesManId={salesManId} and m.userId<>{userId}  GROUP BY m.userId) t LEFT join C_UserInfo u on t.userId=u.Id ) w";
            }
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


        /// <summary>
        /// 获取累计客户的订单成交数量-成交总额-成交总佣金
        /// </summary>
        /// <returns></returns>
        public SalesManRecordUser GetRecordUserOrderCountMoneyCps(SalesManRecordUser salesManRecordUser)
        {
            int aid = salesManRecordUser.appId;
            int userId = salesManRecordUser.userId;
            int salesManId = salesManRecordUser.salesManId;
            string sql = $"SELECT COUNT(car.Id) as number,SUM(car.price*.car.Count) as totalPrice,SUM(car.price*car.cps_rate*car.Count) as cpsMoney from entgoodscart car LEFT JOIN entgoodsorder o on car.goodsorderId=o.id where car.userId={salesManRecordUser.userId} and car.salesManRecordUserId in(SELECT Id from salesmanrecorduser where userId = {salesManRecordUser.userId} and appId = {salesManRecordUser.appId} and salesmanId = {salesManRecordUser.salesManId}) and o.State=3";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    salesManRecordUser = GetModel(dr);
                  //  salesManRecordUser.orderCount = dr["number"] == DBNull.Value ? 0 : Convert.ToInt32(dr["number"]);
                    salesManRecordUser.orderMoneyStr = dr["totalPrice"] == DBNull.Value ? "0.00" : (Convert.ToDouble(dr["totalPrice"]) * 0.01).ToString("0.00");
                    salesManRecordUser.cpsMoneyStr = dr["cpsMoney"] == DBNull.Value ? "0.00" : (Convert.ToDouble(dr["cpsMoney"]) * 0.0001).ToString("0.00");
                }
               
            }

            sql = $"SELECT count(goodsorderid) from(  SELECT goodsorderid from entgoodscart car LEFT JOIN entgoodsorder o on car.goodsorderId=o.id where car.userId={userId} and car.salesManRecordUserId in(SELECT Id from salesmanrecorduser where userId = {userId} and appId = {aid} and salesmanId ={salesManId}) and o.State=3 GROUP BY goodsorderid) c";
            
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != DBNull.Value)
            {
                salesManRecordUser.orderCount = Convert.ToInt32(obj);
            }

            return salesManRecordUser;


        }


        /// <summary>
        /// 根据订单号查询该订单下的产品绑定关系
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<RelationViewModel> GetRelationSearch(int orderId)
        {

            string sql = $"SELECT u.NickName,s.TelePhone,r.Id as relationFlag,r.UpdateTime as StartTime, DATE_ADD(r.UpdateTime,INTERVAL r.protected_time Day) as endTime from entgoodsorder o LEFT JOIN  entgoodscart car on o.Id=car.GoodsOrderId LEFT JOIN salesmanrecorduser r on r.Id=car.salesManRecordUserId LEFT JOIN salesman s on s.Id=r.salesmanId  LEFT JOIN c_userinfo u on u.Id=car.userId where o.Id={orderId}";
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<RelationViewModel> list = new List<RelationViewModel>();
                while (dr.Read())
                {
                    RelationViewModel relationViewModel = new RelationViewModel();

                    relationViewModel.orderUserName = Convert.ToString(dr["NickName"]);
                    relationViewModel.saleManTelephone = dr["TelePhone"] == DBNull.Value ? "-" : Convert.ToString(dr["TelePhone"]);
                    relationViewModel.relationFlag = dr["relationFlag"] == DBNull.Value ? 0 : Convert.ToInt32(dr["relationFlag"]);
                    relationViewModel.relationConnectTime = dr["StartTime"] == DBNull.Value ? "-" : Convert.ToString(dr["StartTime"]);
                    relationViewModel.relationEndTime = dr["endTime"] == DBNull.Value ? "-" : Convert.ToString(dr["endTime"]);

                    if (relationViewModel.relationEndTime != "-")
                    {
                        if (Convert.ToDateTime(relationViewModel.relationEndTime) > DateTime.Now)
                        {
                            relationViewModel.state = "绑定";
                        }
                        
                    }

                    list.Add(relationViewModel);


                }
                return list;
            }

        }



    }
}
