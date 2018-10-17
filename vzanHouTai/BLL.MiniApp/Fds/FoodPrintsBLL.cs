using DAL.Base;
using Entity.MiniApp.Fds;

namespace BLL.MiniApp.Fds
{

    public class FoodPrintsBLL : BaseMySql<FoodPrints>
    {
        
        #region 单例模式
        private static FoodPrintsBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodPrintsBLL()
        {

        }

        public static FoodPrintsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodPrintsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
