using DAL.Base;
using Entity.MiniApp.Home;

namespace BLL.MiniApp.Home
{
    public  class RangeGwBLL : BaseMySql<RangeGw>
    {

        #region 单例模式
        private static RangeGwBLL _singleModel;
        private static readonly object SynObject = new object();

        private RangeGwBLL()
        {

        }

        public static RangeGwBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new RangeGwBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
