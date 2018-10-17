using DAL.Base;
using Entity.MiniApp.Home;

namespace BLL.MiniApp.Home
{
    public class HomebkmenuBLL : BaseMySql<Homebkmenu>
    {

        #region 单例模式
        private static HomebkmenuBLL _singleModel;
        private static readonly object SynObject = new object();

        private HomebkmenuBLL()
        {

        }

        public static HomebkmenuBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HomebkmenuBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
