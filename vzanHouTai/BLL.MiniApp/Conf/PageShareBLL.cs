using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public  class PageShareBLL : BaseMySql<PageShare>
    {
        #region 单例模式
        private static PageShareBLL _singleModel;
        private static readonly object SynObject = new object();

        private PageShareBLL()
        {

        }

        public static PageShareBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PageShareBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
