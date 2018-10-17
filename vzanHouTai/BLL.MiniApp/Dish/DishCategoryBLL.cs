using DAL.Base;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace BLL.MiniApp.Dish
{
    //public interface IDishCategoryRepository
    //{
    //    List<DishCategory> GetDishCategorys(DishEnums.CategoryEnums type, int aId = 0, int storeId = 0, int pageIndex = -1, int pageSize = 0, bool getEffData = false);
    //}
    public class DishCategoryBLL : BaseMySql<DishCategory>//, IDishCategoryRepository
    {
        #region 单例模式
        private static DishCategoryBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishCategoryBLL()
        {

        }

        public static DishCategoryBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishCategoryBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 查询 app/store 下所有的有效分类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="getEffData">只返回有效数据</param>
        /// <returns></returns>
        public List<DishCategory> GetDishCategorys(DishEnums.CategoryEnums type, int aId = 0, int storeId = 0, int pageIndex = -1, int pageSize = 0, bool getEffData = false)
        {
            return GetDishCategorys((int)type, aId, storeId, pageSize, pageIndex, getEffData);
        }

        /// <summary>
        /// 查询 app/store 下所有的有效分类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="getEffData">只返回有效数据</param>
        /// <returns></returns>
        public List<DishCategory> GetDishCategorys(int type, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 0, bool getEffData = false)
        {
            string whereSql = $" type = {type} and aid = {aId} and storeId = {storeId} {(getEffData ? " and state = 1 " : "")} ";
            if (pageIndex > 0 && pageSize > 0)//分页
            {
                return base.GetList(whereSql, pageSize, pageIndex,"*", "is_order desc");
            }
            else //不分页
            {
                return base.GetList(whereSql);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="getEffData">只返回有效数据</param>
        /// <returns></returns>
        public int GetDishCategorysCount(int type, int aId = 0, int storeId = 0, bool getEffData = false)
        {
            string whereSql = $" type = {type} and aid = {aId} and storeId = {storeId} {(getEffData ? " and state = 1 " : "")} ";
            return base.GetCount(whereSql);
        }

        /// <summary>
        /// 查询 app/store 下指定分页的数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<DishCategory> GetDishCategorysByPage(int type, int aId = 0, int pageIndex = 0, int pageSize = 10, int storeId = 0)
        {
            return base.GetList($" type = {type} and aid = {aId} and storeId = {storeId} ", pageSize, pageIndex);
        }

        /// <summary>
        /// 批量更新桌台排序
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
                sql.Add($"update DishCategory set is_order=@is_order where id=@id");
                sqlParameters.Add(new MySqlParameter[] {
                    new MySqlParameter("@is_order",idSortArray[1]),
                    new MySqlParameter("@id",idSortArray[0])
                });
            }
            return ExecuteTransaction(sql.ToArray(), sqlParameters.ToArray());
        }

        public List<DishCategory> GetListById(string Id)
        {
            return GetList($"Id IN ({Id})");
        }

        public List<DishCategory> GetListByStore(int storeId, DishEnums.CategoryEnums type, int pageIdex = 1, int pageSize = 10)
        {
            string whereSql = BuildWhereSql(storeId: storeId, type: type);
            return GetList(whereSql, pageSize, pageIdex);
        }

        public int GetCountByStore(int storeId, DishEnums.CategoryEnums type)
        {
            string whereSql = BuildWhereSql(storeId: storeId, type: type);
            return GetCount(whereSql);
        }

        public string BuildWhereSql(DishEnums.CategoryEnums type, int? storeId = null, int? aId = null, bool isGetDel = false)
        {
            List<string> whereSql = new List<string> { $"Type = {(int)type}" };
            if(storeId.HasValue && storeId> 0)
            {
                whereSql.Add($"StoreId = {storeId}");
            }
            if(aId.HasValue && aId > 0)
            {
                whereSql.Add($"aId = {aId}");
            }
            if(!isGetDel)
            {
                whereSql.Add("State <> -1");
            }
            return string.Join(" AND ", whereSql);
        }

        public bool DeleteCategory(DishCategory category)
        {
            category.state = -1;
            return base.Update(category, "state");
        }


        #region 重写底层将更新时间添加时间自动计入
        public override object Add(DishCategory model)
        {
            model.addTime = DateTime.Now;
            return base.Add(model);
        }

        public override bool Update(DishCategory model)
        {
            model.updateTime = DateTime.Now;
            return base.Update(model);
        }

        public override bool Update(DishCategory model, string columnFields)
        {
            model.updateTime = DateTime.Now;
            columnFields += ",updateTime";
            return base.Update(model, columnFields);
        }
        #endregion
    }
}
