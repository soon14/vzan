using DAL.Base;
using Entity.MiniApp.Ent;

namespace BLL.MiniApp.Ent
{
    public class ExchangeRuleBLL : BaseMySql<ExchangeRule>
    {
        #region 单例模式
        private static ExchangeRuleBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangeRuleBLL()
        {

        }

        public static ExchangeRuleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangeRuleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
