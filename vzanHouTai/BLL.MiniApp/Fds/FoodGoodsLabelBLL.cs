using DAL.Base;
using Entity.MiniApp.Fds;

namespace BLL.MiniApp.Fds
{

    public class FoodGoodsLabelBLL : BaseMySql<FoodGoodsLabel>
    {

        #region 单例模式
        private static FoodGoodsLabelBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsLabelBLL()
        {

        }

        public static FoodGoodsLabelBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsLabelBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
