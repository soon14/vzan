using DAL.Base;
using Entity.MiniApp;

namespace BLL.MiniApp
{
    public partial class OpenComponentConfigBLL : BaseMySql<OpenAuthorizerInfo>
    {
        #region µ¥ÀýÄ£Ê½
        private static OpenComponentConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private OpenComponentConfigBLL()
        {

        }

        public static OpenComponentConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new OpenComponentConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public OpenAuthorizerInfo GetModelByAppid(string appid)
        {
            var info = GetModel($"component_Appid='{appid}'");
            return info;
        }
    }
}