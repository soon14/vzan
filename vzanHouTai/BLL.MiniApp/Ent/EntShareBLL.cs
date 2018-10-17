using DAL.Base;
using Entity.MiniApp.Ent;
namespace BLL.MiniApp.Ent
{
    public class EntShareBLL : BaseMySql<EntShare>
    {
        #region 单例模式
        private static EntShareBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntShareBLL()
        {

        }

        public static EntShareBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntShareBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
