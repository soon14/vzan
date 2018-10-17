using DAL.Base;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Dish
{
    public class DishSettingBLL : BaseMySql<DishSetting>
    {
        #region 单例模式
        private static DishSettingBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishSettingBLL()
        {

        }

        public static DishSettingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishSettingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DishSetting GetModelByAid(int aid)
        {
            DishSetting model = null;
            if (aid <= 0)
            {
                return model;
            }
            string sqlwhere = $" aid={aid}";
            model = GetModel(sqlwhere);
            return model;
        }
    }
}
