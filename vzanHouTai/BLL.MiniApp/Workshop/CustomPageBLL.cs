using DAL.Base;
using Entity.MiniApp.Workshop;

namespace BLL.MiniApp.Workshop
{
    public class CustomPageBLL : BaseMySql<CustomPage>
    {
        #region 单例模式
        private static CustomPageBLL _singleModel;
        private static readonly object SynObject = new object();

        private CustomPageBLL()
        {

        }

        public static CustomPageBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CustomPageBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
