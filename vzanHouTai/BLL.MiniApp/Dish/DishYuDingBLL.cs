using DAL.Base;
using System.Data;
using Entity.MiniApp.Dish;
using System.Collections.Generic;
using Utility;
using Core.MiniApp;

namespace BLL.MiniApp.Dish
{
    public class DishYuDingBLL : BaseMySql<DishYuDing>
    {
        #region 单例模式
        private static DishYuDingBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishYuDingBLL()
        {

        }

        public static DishYuDingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishYuDingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<DishYuDing> GetListByParams(int storeId, int? pageIndex = null, int? pageSize = null)
        {
            if(pageIndex.HasValue && pageSize.HasValue)
            {
                return GetList($"Dish_Id = {storeId}", pageSize.Value, pageIndex.Value, "*", "ID DESC");
            }
            return GetList($"Dish_Id = {storeId}");
        }

        public int GetCountByStore(int storeId)
        {
            return GetCount($"Dish_Id = {storeId}");
        }
    }
}
