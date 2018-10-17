using DAL.Base;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using static Entity.MiniApp.Dish.DishEnums;

namespace BLL.MiniApp.Dish
{
    public class DishTableBLL : BaseMySql<DishTable>
    {
        #region 单例模式
        private static DishTableBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishTableBLL()
        {

        }

        public static DishTableBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishTableBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据参数返回相应结果
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="effData">只查询有效数据(state = 1)</param>
        /// <param name="orderbyCols">排序语句</param>
        /// <returns></returns>
        public List<DishTable> GetTableByParams(int aid, int storeId, bool effData = false, bool orderBySort = true,int? pageIndex = null,int? pageSize = null)
        {
            string whereSql = $" aid = {aid} and storeId = {storeId}  ";
            
            if (effData)
            {
                whereSql += "and state >= 0 ";
            }
            List<DishTable> modelList = null;
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                modelList= base.GetList(whereSql, pageSize.Value, pageIndex.Value);
            }
            else
            {
                modelList = base.GetList(whereSql);
            }
            if (orderBySort)
            {
                modelList = modelList.OrderBy(t => t.table_sort).ToList();
            }

            return modelList;
        }

        /// <summary>
        /// 检测是否存在同名桌台号
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="effData">只查询有效数据(state = 1)</param>
        /// <param name="orderbyCols">排序语句</param>
        /// <returns></returns>
        public bool CheckExistTableName(int id, int aId,int storeId, string table_name)
        {
            string whereSql = $" aId = @aId and storeId = @storeId and table_name = @table_name and id != @id  and state != -1 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@id", id));
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@table_name", table_name));

            return base.Exists(whereSql,mysqlParams.ToArray());
        }


        /// <summary>
        /// 批量更新分类
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
                sql.Add($"update DishTable set table_sort=@table_sort where id=@id");
                sqlParameters.Add(new MySqlParameter[] {
                    new MySqlParameter("@table_sort",idSortArray[1]),
                    new MySqlParameter("@id",idSortArray[0])
                });
            }
            return ExecuteTransaction(sql.ToArray(), sqlParameters.ToArray());
        }

        /// <summary>
        /// 清台
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool ClearTable(int aId,int storeId)
        {
            string sql = $"update dishtable set state={(int)DishEnums.TableStateEnums.空闲} where aId = {aId} and storeId = {storeId} and state >= 0 ;";
            return ExecuteNonQuery(sql) > 0;
        }
        /// <summary>
        /// 获取餐桌数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetCountByStoreId(int storeId)
        {
            string sqlwhere = $" storeid={storeId} and state>{(int)TableStateEnums.已删除}";
            return GetCount(sqlwhere);
        }
    }
}