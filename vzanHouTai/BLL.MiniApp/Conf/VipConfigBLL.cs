using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class VipConfigBLL : BaseMySql<VipConfig>
    {

        #region 单例模式
        private static VipConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private VipConfigBLL()
        {

        }

        public static VipConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VipConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

    }
}
