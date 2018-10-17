using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class VipInsteadCardAuthBLL : BaseMySql<VipInsteadCardAuth>
    {

        #region 单例模式
        private static VipInsteadCardAuthBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipInsteadCardAuthBLL()
        {

        }

        public static VipInsteadCardAuthBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipInsteadCardAuthBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

    }
}
