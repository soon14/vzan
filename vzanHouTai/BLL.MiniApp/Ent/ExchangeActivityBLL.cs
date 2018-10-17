using DAL.Base;
using Entity.MiniApp.Ent;

namespace BLL.MiniApp.Ent
{
    public class ExchangeActivityBLL : BaseMySql<ExchangeActivity>
    {
        #region 单例模式
        private static ExchangeActivityBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangeActivityBLL()
        {

        }

        public static ExchangeActivityBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangeActivityBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
