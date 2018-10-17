using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class PageIndexControlBLL : BaseMySql<PageIndexControl>
    {
        #region 单例模式
        private static PageIndexControlBLL _singleModel;
        private static readonly object SynObject = new object();

        private PageIndexControlBLL()
        {

        }

        public static PageIndexControlBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PageIndexControlBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
