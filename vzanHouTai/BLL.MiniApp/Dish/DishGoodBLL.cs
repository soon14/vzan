using DAL.Base;
using System.Data;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using Utility;
using Core.MiniApp;
using MySql.Data.MySqlClient;
using System.Text;
using System;

namespace BLL.MiniApp.Dish
{
    public class DishGoodBLL : BaseMySql<DishGood>
    {
        #region 单例模式
        private static DishGoodBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishGoodBLL()
        {

        }

        public static DishGoodBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishGoodBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<DishGood> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<DishGood>();

            return base.GetList($"id in ({ids})");
        }

        public Tuple<List<DishGood>,int> GetListFromTable(int aId,int storeId, int pageIndex, int pageSize,string kw="",int cate_id = 0)
        {
            List<MySqlParameter> parameters = null;
            StringBuilder sqlFilter = new StringBuilder();
            StringBuilder sqlFilter2 = new StringBuilder();
            pageIndex = pageIndex - 1;
            if (pageIndex < 0)
                pageIndex = 0;
            
            if (!string.IsNullOrEmpty(kw))
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@kw",Utils.FuzzyQuery(kw)));
                sqlFilter.Append(" and g.g_name like @kw ");
                sqlFilter2.Append(" and g_name like @kw ");
            }
            if (cate_id > 0)
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@cate_id", cate_id));
                sqlFilter.Append(" and g.cate_id=@cate_id ");
                sqlFilter2.Append(" and cate_id=@cate_id ");
            }
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT g.*,(SELECT title from dishcategory c where c.id=g.cate_id) as cat_name from dishgood g where g.aid={aId} and g.storeid={storeId} and g.state<>-1 {sqlFilter}  order by g.is_order desc limit {pageSize * pageIndex},{pageSize}",parameters?.ToArray()).Tables[0];
            int totalCount = GetCount($"aid={aId} and storeid={storeId} and state<>-1 {sqlFilter2}",parameters?.ToArray());
            return Tuple.Create(DataHelper.ConvertDataTableToList<DishGood>(dt), totalCount);
        }

        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="sortData">
        /// id_is_order,id_is_order,id_is_order
        /// </param>
        /// <returns></returns>
        public bool UpdateSortBatch(string sortData)
        {
            if (string.IsNullOrEmpty(sortData))
                return false;

            string[] sortDataArray = sortData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (sortDataArray.Length <= 0)
                return false;
            List<string> sql = new List<string>();
            List<MySqlParameter[]> sqlParameters = new List<MySqlParameter[]>();
            for (int i = 0; i < sortDataArray.Length; i++)
            {
                string[] idSortArray = sortDataArray[i].Split('_');
                sql.Add($"update dishgood set is_order=@is_order where id=@id");
                sqlParameters.Add(new MySqlParameter[] {
                    new MySqlParameter("@is_order",idSortArray[1]),
                    new MySqlParameter("@id",idSortArray[0])
                });
            }
            return ExecuteTransaction(sql.ToArray(), sqlParameters.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<DishGood> GetGoodsByIds(int[] ids)
        {
            if (ids == null || ids.Length <= 0)
            {
                return null;
            }

            return GetList($" id in ({string.Join(",", ids)})  ");
        }
        /// <summary>
        /// 获取已上架商品数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetCountByStoreId(int storeId)
        {
            string sqlwhere = $"storeid={storeId} and state=1";
            return GetCount(sqlwhere);
        }



        public int GetSalesCountByTimeSpan(int storeId,int goodId, DateTime? startTime = null, DateTime? endTime = null)
        {
            string timeWhereSql = string.Empty;
            if (startTime != null) timeWhereSql += $" and o.add_time >= '{startTime.Value.ToString("yyyy-MM-dd")}' ";
            if (endTime != null) timeWhereSql += $" and o.add_time <= '{endTime.Value.ToString("yyyy-MM-dd")}' ";

            string salesCountSql = $@"select IFNULL(sum(c.goods_number),0) as sales_count from dishorder o
					                            left join dishshoppingcart c on o.id = c.order_id
					                            where o.storeId = {storeId} and c.goods_id = {goodId} {timeWhereSql} and o.order_status in ({(int)DishEnums.OrderState.已确认},{(int)DishEnums.OrderState.已完成}) and o.is_delete = 0 and c.is_tuikuan = 0";

            return Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, salesCountSql, null));
        }


        public bool SyncGoodByPrintTag(int tagsId)
        {
            string sql = $" update dishgood set g_print_tag = 0 where g_print_tag = {tagsId} ;";

            return ExecuteNonQuery(sql) > 0;
        }

        public List<DishGood> GetListByCondition(int aid, int storeId, int pageSize, int pageIndex, int state,out int recordCount)
        {
            string sqlwhere = $" aid={aid} and storeId={storeId} and state={state}";
            recordCount = GetCount(sqlwhere);
            return GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
        }

        public bool DeleteProduct(DishGood product)
        {
            product.state = -1;
            return Update(product, "state");
        }
    }
}
