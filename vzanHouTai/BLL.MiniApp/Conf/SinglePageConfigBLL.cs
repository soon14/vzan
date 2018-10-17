using DAL.Base;
using Entity.MiniApp.Conf;

namespace BLL.MiniApp.Conf
{
    public class SinglePageConfigBLL : BaseMySql<SinglePageConfig>
    {
        #region 单例模式
        private static SinglePageConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private SinglePageConfigBLL()
        {

        }

        public static SinglePageConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SinglePageConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public SinglePageConfig GetModelByRid(int rid)
        {
            var info = GetModel($"Rid='{rid}'");
            return info;
        }
    }
}
