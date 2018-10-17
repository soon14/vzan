using DAL.Base;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Qiye;

namespace BLL.MiniApp.Qiye
{
    public class QiyeGoodsUnitBLL : BaseMySql<QiyeGoodsUnit>
    {
        #region 单例模式
        private static QiyeGoodsUnitBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsUnitBLL()
        {

        }

        public static QiyeGoodsUnitBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsUnitBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
