using DAL.Base;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Dish
{
    public class DishVipCardSettingBLL : BaseMySql<DishVipCardSetting>
    {
        #region 单例模式
        private static DishVipCardSettingBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishVipCardSettingBLL()
        {

        }

        public static DishVipCardSettingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishVipCardSettingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DishVipCardSetting GetModelByStoreId(int storeId)
        {
            if (storeId <= 0)
            {
                return null;
            }
            string sqlwhere = $" storeid={storeId}";
            return GetModel(sqlwhere);
        }

        public DishVipCardSetting GetModelByStoreId_Id(int storeId, int id)
        {
            if (storeId <= 0||id<=0)
            {
                return null;
            }
            string sqlwhere = $" storeid={storeId} and id={id}";
            return GetModel(sqlwhere);
        }
    }
}
