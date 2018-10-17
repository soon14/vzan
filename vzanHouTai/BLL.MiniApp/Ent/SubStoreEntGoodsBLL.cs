using DAL.Base;
using Entity.MiniApp.Ent;
using System.Linq;

namespace BLL.MiniApp.Ent
{
    public class SubStoreEntGoodsBLL: BaseMySql<SubStoreEntGoods>
    {
        #region 单例模式
        private static SubStoreEntGoodsBLL _singleModel;
        private static readonly object SynObject = new object();

        private SubStoreEntGoodsBLL()
        {

        }

        public static SubStoreEntGoodsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SubStoreEntGoodsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据条件返回有效商品(上架且未删除)
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public SubStoreEntGoods GetModelByAppIdStoreIdGoodsId(int appId, int storeId, int goodsId)
        {
            return GetModel($" Aid = {appId} and StoreId = {storeId} and Pid = {goodsId} and subState = 1 and subTag = 1  ");
        }


        /// <summary>
        /// 查询当前商品库存
        /// </summary>
        /// <param name="goodsid"></param>
        /// <param name="attrSpacStr"></param>
        /// <returns></returns>
        public int GetGoodQtyByModel(SubStoreEntGoods goods, string attrSpacStr = "")
        {
            int goodQty = 0;

            //var good = GetModel(goodsid);
            if (string.IsNullOrWhiteSpace(attrSpacStr))
            {
                goodQty = goods.SubStock;
            }
            else
            {
                var goodList = goods.GASDetailList.Where(x => x.id.Equals(attrSpacStr)).ToList();
                if (goodList != null && goodList.Any())
                {
                    var goodBySpacStr = goodList.First();
                    if (goodBySpacStr != null)
                    {
                        goodQty = goodBySpacStr.stock;
                    }
                }

            }
            return goodQty;
        }
    }
}
