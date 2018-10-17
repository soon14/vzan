using DAL.Base;
using Entity.MiniApp.Workshop;

namespace BLL.MiniApp.Workshop
{
    public class CustomPageFormDataBLL : BaseMySql<CustomPageFormData>
    {
        #region 单例模式
        private static CustomPageFormDataBLL _singleModel;
        private static readonly object SynObject = new object();

        private CustomPageFormDataBLL()
        {

        }

        public static CustomPageFormDataBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CustomPageFormDataBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
