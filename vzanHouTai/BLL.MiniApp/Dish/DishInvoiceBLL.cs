using DAL.Base;
using Entity.MiniApp.Dish;

namespace BLL.MiniApp.Dish
{
    public class DishInvoiceBLL : BaseMySql<DishInvoice>
    {
        #region 单例模式
        private static DishInvoiceBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishInvoiceBLL()
        {

        }

        public static DishInvoiceBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishInvoiceBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
