using BLL.MiniApp.cityminiapp;
using DAL.Base;
using Entity.MiniApp.cityminiapp;
using Entity.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Dish
{
    public class DishPictureBLL : BaseMySql<DishPicture>
    {
        #region 单例模式
        private static DishPictureBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishPictureBLL()
        {

        }

        public static DishPictureBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishPictureBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<DishPicture> GetLunboImgs(int pageSize, int pageIndex, int aid,out int recordCount)
        {
            List<DishPicture> list = null;
            recordCount = 0;
            if (aid <= 0)
            {
                return list;
            }
            string sqlwhere = $"state>-1 and aid={aid} and type=0";
            recordCount = GetCount(sqlwhere);
            return GetList(sqlwhere, pageSize, pageIndex, "*", "is_order desc");
        }

        public DishPicture GetModelById_Aid(int id, int aid)
        {
            DishPicture model = null;
            if (aid <= 0 || id <= 0) return model;
            string sqlwhere = $"aid={aid} and id={id} and state=1";
            model = GetModel(sqlwhere);
            return model;
        }
    }
}
