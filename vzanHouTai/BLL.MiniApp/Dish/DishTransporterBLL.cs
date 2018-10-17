using DAL.Base;
using Entity.MiniApp.Dish;
using System.Collections.Generic;

namespace BLL.MiniApp.Dish
{
    public class DishTransporterBLL : BaseMySql<DishTransporter>
    {
        #region 单例模式
        private static DishTransporterBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishTransporterBLL()
        {

        }

        public static DishTransporterBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishTransporterBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据参数条件查询配送员
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="effData">只查出可用的记录</param>
        /// <returns></returns>
        public List<DishTransporter> GetTransportersByparams(int aId, int storeId, bool effData = false, int? pageIndex = null, int? pageSize = null)
        {
            string whereSql = $" aId = {aId} and storeId = {storeId} {(effData ? " and state = 1 " : " and state >= 0 ")} ";
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                return GetList(whereSql, pageSize.Value, pageIndex.Value);
            }
            return GetList(whereSql);
        }
    }
}
