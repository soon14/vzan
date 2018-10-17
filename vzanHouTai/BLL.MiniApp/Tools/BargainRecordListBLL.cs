using DAL.Base;
using Entity.MiniApp.Tools;

namespace BLL.MiniApp.Tools
{
    public class BargainRecordListBLL : BaseMySql<BargainRecordList>
    {
        #region 单例模式
        private static BargainRecordListBLL _singleModel;
        private static readonly object SynObject = new object();

        private BargainRecordListBLL()
        {

        }

        public static BargainRecordListBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new BargainRecordListBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
