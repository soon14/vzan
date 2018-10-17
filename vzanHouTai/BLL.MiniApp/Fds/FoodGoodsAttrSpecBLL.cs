using DAL.Base;
using Entity.MiniApp.Fds;
using System.Collections.Generic;

namespace BLL.MiniApp.Fds
{
    public class FoodGoodsAttrSpecBLL : BaseMySql<FoodGoodsAttrSpec>
    {
        #region 单例模式
        private static FoodGoodsAttrSpecBLL _singleModel;
        private static readonly object SynObject = new object();

        private FoodGoodsAttrSpecBLL()
        {

        }

        public static FoodGoodsAttrSpecBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FoodGoodsAttrSpecBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<FoodGoodsAttrSpec> GetListByGId(int goodid)
        {
            return base.GetList($" FoodGoodsId = {goodid} ");
        }
    }
}
