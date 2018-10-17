using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Ent
{
    public class SalesManBLL : BaseMySql<SalesMan>
    {
        #region 单例模式
        private static SalesManBLL _singleModel;
        private static readonly object SynObject = new object();

        private SalesManBLL()
        {

        }

        public static SalesManBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SalesManBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 获取分销员列表
        /// </summary>
        /// <param name="strWhere"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<SalesMan> GetListSalesMan(string strWhere, int pageIndex = 1, int pageSize = 10, string strOrder = "")
        {
            string sql = $"select m.*,u.NickName from SalesMan m  LEFT join C_UserInfo u on m.UserId=u.Id   where {strWhere} group by m.userId order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {

                List<SalesMan> list = new List<SalesMan>();
                while (dr.Read())
                {
                    SalesMan salesMan = GetModel(dr);
                    if (salesMan != null && salesMan.Id > 0)
                    {
                        salesMan.nickName = (dr["NickName"] == DBNull.Value ? string.Empty : dr["NickName"].ToString());
                        salesMan.orderCount = VipRelationBLL.SingleModel.GetEntGoodsOrderCount(salesMan.UserId);
                        VipRelation vip = VipRelationBLL.SingleModel.GetModel($"uid={salesMan.UserId}");
                        if (vip != null)
                        {
                            salesMan.orderPrice = vip.pricestr;
                        }
                        else
                        {
                            salesMan.orderPrice = "0.00";
                        }
                        salesMan = GetSalesMan(salesMan);
                        list.Add(salesMan);
                    }

                }
                return list.Count > 0 ? list : null;
            }


        }

        /// <summary>
        /// 获取分销员个数根据条件
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public int GetListSalesManCount(string strWhere)
        {
            string sql = $"select count(m.Id) from SalesMan m  where {strWhere} ";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


        /// <summary>
        /// 获取分销员的推广订单数量 SalesManRecordOrder属于该分销员的数量
        /// </summary>
        /// <param name="strWhere"></param>
        /// <returns></returns>
        public int GetListSalesManOrderCount(string strWhere)
        {
            string sql = $"SELECT COUNT(Id) number from SalesManRecordOrder where salesmanrecordId in(SELECT Id from salesmanrecord WHERE {strWhere}) ";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


        /// <summary>
        /// 根据用户输入的关键词模糊查询名称或者电话 找到分销员ID集合
        /// </summary>
        /// <param name="keyMsg"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        public string GetSaleManIdsByPhoneName(string keyMsg, int aid)
        {

            string sql = $"select m.Id as salesManId from SalesMan m  LEFT join C_UserInfo u on m.UserId=u.Id   where m.TelePhone like @keyMsg or u.NickName like @keyMsg";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@keyMsg", $"%{keyMsg}%"));
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, parameters.ToArray()))
            {
                List<string> listId = new List<string>();
                while (dr.Read())
                {
                    if (dr["salesManId"] != DBNull.Value)
                    {
                        listId.Add(dr["salesManId"].ToString());
                    }
                }

                if (listId.Count > 0)
                {
                    return string.Join(",", listId);
                }
                else
                {
                    return "-1";
                }
            }






        }


        public SalesMan GetSalesMan(SalesMan salesMan)
        {
            string sql = $"select u.NickName,m.TelePhone  from SalesMan m  LEFT join C_UserInfo u on m.UserId=u.Id   where m.Id={salesMan.ParentSalesmanId}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                while (dr.Read())
                {
                    if (dr["NickName"] != DBNull.Value)
                    {
                        salesMan.ParentSalesmanNickName = dr["NickName"].ToString();
                    }
                    if (dr["TelePhone"] != DBNull.Value)
                    {
                        salesMan.ParentSalesmanPhone = dr["TelePhone"].ToString();
                    }

                }

            }

            return salesMan;
        }




    }
}
