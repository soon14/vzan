using DAL.Base;
using Entity.MiniApp.Home;

namespace BLL.MiniApp.Home
{
    public class HfeedbackBLL : BaseMySql<Hfeedback>
    {
        #region 单例模式
        private static HfeedbackBLL _singleModel;
        private static readonly object SynObject = new object();

        private HfeedbackBLL()
        {

        }

        public static HfeedbackBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new HfeedbackBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
