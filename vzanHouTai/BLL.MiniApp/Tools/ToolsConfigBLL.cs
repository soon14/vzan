using DAL.Base;
using Entity.MiniApp.Tools;

namespace BLL.MiniApp.Tools
{
    public class ToolsConfigBLL : BaseMySql<ToolsConfig>
    {
        #region 单例模式
        private static ToolsConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private ToolsConfigBLL()
        {

        }

        public static ToolsConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ToolsConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
