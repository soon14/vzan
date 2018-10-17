using DAL.Base;
using Entity.MiniApp.Dish;

namespace BLL.MiniApp.Dish
{
    public class DishUserAddressBLL : BaseMySql<DishUserAddress>
    {
        #region 单例模式
        private static DishUserAddressBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishUserAddressBLL()
        {

        }

        public static DishUserAddressBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishUserAddressBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
