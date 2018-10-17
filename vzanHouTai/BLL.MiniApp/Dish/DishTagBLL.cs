using DAL.Base;
using Entity.MiniApp.Dish;
using System.Collections.Generic;

namespace BLL.MiniApp.Dish
{
    public class DishTagBLL : BaseMySql<DishTag>
    {
        #region 单例模式
        private static DishTagBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishTagBLL()
        {

        }

        public static DishTagBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishTagBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<DishTag> GetTagByParams(int aId, int storeId, int type, int? pageIndex = 0, int? pageSize = null)
        {
            string whereSql = BuildWhereSql(aId: aId, storeId: storeId, type: type);
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                return GetList(whereSql, pageSize.Value, pageIndex.Value);
            }
            return GetList(whereSql);
        }

        public int GetCountByParams(int aId, int storeId, int type)
        {
            string whereSql = BuildWhereSql(aId: aId, storeId: storeId, type: type);
            return GetCount(whereSql);
        }

        public string BuildWhereSql(int? aId = null, int? storeId = null, int? type = null, bool getDelete = false)
        {
            List<string> whereSql = new List<string>();
            if(!getDelete)
            {
                whereSql.Add("state = 1");
            }
            if(aId > 0)
            {
                whereSql.Add($"aId = {aId.Value}");
            }
            if (storeId > 0)
            {
                whereSql.Add($"storeId = {storeId.Value}");
            }
            if (type > 0)
            {
                whereSql.Add($"type = {type.Value}");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
