using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class VipWxCardBLL : BaseMySql<VipWxCard>
    {
        #region 单例模式
        private static VipWxCardBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipWxCardBLL()
        {

        }

        public static VipWxCardBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipWxCardBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
