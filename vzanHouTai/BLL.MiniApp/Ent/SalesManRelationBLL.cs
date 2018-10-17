using DAL.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Ent
{
    public class SalesManRelationBLL : BaseMySql<SalesManRelation>
    {
        #region 单例模式
        private static SalesManRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private SalesManRelationBLL()
        {

        }

        public static SalesManRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SalesManRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 根据分销员Id获取下级分销员列表
        /// </summary>
        /// <param name="saleManId"></param>
        /// <param name="totalCount"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<ViewSalesManRelation> GetListBySaleManId(int saleManId, out int totalCount, int pageIndex = 1, int pageSize = 10)
        {
            List<ViewSalesManRelation> list = new List<ViewSalesManRelation>();
            //string sql = $"SELECT sr.*,u.NickName,u.HeadImgUrl FROM(SELECT userid, SUM(price) as totalCpsPrice from SalesManRelation where ParentSaleManId = {saleManId} GROUP BY userid) sr LEFT JOIN c_userinfo u on sr.userid = u.Id   LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            string sql = $"SELECT s.userid,u.NickName,u.HeadImgUrl from salesman s LEFT JOIN c_userinfo u on s.userid = u.Id where  s.ParentSalesManId = {saleManId} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {

                while (dr.Read())
                {
                    ViewSalesManRelation viewSalesManRelation = new ViewSalesManRelation();
                    if (dr["NickName"] != DBNull.Value)
                    {
                        viewSalesManRelation.Name = Convert.ToString(dr["NickName"]);
                    }
                    if (dr["HeadImgUrl"] != DBNull.Value)
                    {
                        viewSalesManRelation.Avatar = Convert.ToString(dr["HeadImgUrl"]);
                    }
                    if (dr["userid"] != DBNull.Value)
                    {
                        viewSalesManRelation.CpsPrice = (GetTotalSecondCpsPrice(saleManId,Convert.ToInt32(dr["userid"])) * 0.01).ToString("0.00");
                    }

                    list.Add(viewSalesManRelation);
                }

            }
            totalCount = 0;
            if (list != null && list.Count > 0)
            {
                sql = $"SELECT COUNT(s.userid)  from salesman s LEFT JOIN c_userinfo u on s.userid = u.Id where  s.ParentSalesManId = {saleManId}";
                object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
                if (obj != DBNull.Value)
                {
                    totalCount = Convert.ToInt32(obj);
                }
            }



            return list;

        }

        public int GetTotalSecondCpsPrice(int saleManId,int userId=0)
        {
            int totalSecondCpsPrice = 0;
            string sql = $"SELECT SUM(price) as totalCpsPrice from SalesManRelation where ParentSaleManId = {saleManId}";
            if (userId > 0)
            {
                sql += $" and userId={userId}";
            }
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != DBNull.Value)
            {
                totalSecondCpsPrice = Convert.ToInt32(obj);
            }
            return totalSecondCpsPrice;


        }


    }
}
