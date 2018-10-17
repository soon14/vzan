using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class VipWxCardCodeBLL : BaseMySql<VipWxCardCode>
    {
        #region 单例模式
        private static VipWxCardCodeBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipWxCardCodeBLL()
        {

        }

        public static VipWxCardCodeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipWxCardCodeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
