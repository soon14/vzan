using DAL.Base;
using Entity.MiniApp.Fds;

namespace BLL.MiniApp.Fds
{
    public class FoodOrderPrintLogBLL : BaseMySql<FoodOrderPrintLog>
    {
        #region 单例模式
        private static FoodOrderPrintLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodOrderPrintLogBLL()
        {

        }

        public static FoodOrderPrintLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodOrderPrintLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
