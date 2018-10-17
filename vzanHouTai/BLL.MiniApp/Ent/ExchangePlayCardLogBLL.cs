using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.Ent;


namespace BLL.MiniApp.Ent
{
    public class ExchangePlayCardLogBLL : BaseMySql<ExchangePlayCardLog>
    {

        #region 单例模式
        private static ExchangePlayCardLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangePlayCardLogBLL()
        {

        }

        public static ExchangePlayCardLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangePlayCardLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
