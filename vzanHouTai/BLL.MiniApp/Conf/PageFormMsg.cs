using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class PageFormMsgBLL : BaseMySql<PageFormMsg>
    {
        #region 单例模式
        private static PageFormMsgBLL _singleModel;
        private static readonly object SynObject = new object();

        private PageFormMsgBLL()
        {

        }

        public static PageFormMsgBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PageFormMsgBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
