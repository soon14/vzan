using DAL.Base;
using Entity.MiniApp.Ent;
using Entity.MiniApp.PlatChild;

namespace BLL.MiniApp.PlatChild
{
    public class PlatChildGoodsUnitBLL : BaseMySql<PlatChildGoodsUnit>
    {
        #region 单例模式
        private static PlatChildGoodsUnitBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatChildGoodsUnitBLL()
        {

        }

        public static PlatChildGoodsUnitBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatChildGoodsUnitBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
