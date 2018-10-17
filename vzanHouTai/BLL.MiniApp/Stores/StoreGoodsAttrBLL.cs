using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsAttrBLL : BaseMySql<StoreGoodsAttr>
    {
        #region 单例模式
        private static StoreGoodsAttrBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsAttrBLL()
        {

        }

        public static StoreGoodsAttrBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsAttrBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 缓存key号
        /// </summary>
        public static readonly string Redis_StoreGoodsAttr = "Miniapp_StoreGoodsAttr_{0}";

        /// <summary>
        /// 缓存version号  model/list
        /// </summary>
        public static readonly string Redis_StoreGoodsAttr_version = "Miniapp_StoreGoodsAttr_version_{0}";


        /// <summary>
        /// 根据缓存读取 商品规格/规格值
        /// </summary>
        /// <param name="goodId">商品Id</param>
        /// <returns></returns>
        public StoreGoodsAttr GetModelByCache(int goodsAttrId)
        {
            string models_key = string.Format(Redis_StoreGoodsAttr, goodsAttrId);
            string version_key = string.Format(Redis_StoreGoodsAttr_version, goodsAttrId);

            RedisModel<StoreGoodsAttr> redisModel_StoreGoodsAttr = RedisUtil.Get<RedisModel<StoreGoodsAttr>>(models_key);
            int version = RedisUtil.GetVersion(version_key);

            if (redisModel_StoreGoodsAttr == null || redisModel_StoreGoodsAttr.DataList == null
                    || redisModel_StoreGoodsAttr.DataList.Count <= 0 || redisModel_StoreGoodsAttr.DataVersion != version)
            {
                redisModel_StoreGoodsAttr = new RedisModel<StoreGoodsAttr>();
                List<StoreGoodsAttr> storeGoodsAttrSpecs = new List<StoreGoodsAttr>() { GetModel(goodsAttrId) };

                redisModel_StoreGoodsAttr.DataList = storeGoodsAttrSpecs;
                redisModel_StoreGoodsAttr.DataVersion = version;
                redisModel_StoreGoodsAttr.Count = storeGoodsAttrSpecs.Count;

                RedisUtil.Set<RedisModel<StoreGoodsAttr>>(models_key, redisModel_StoreGoodsAttr, TimeSpan.FromHours(12));
            }
            return redisModel_StoreGoodsAttr.DataList[0];
        }


        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveStoreGoodsAttrCache(int goodsAttrId)
        {
            if (goodsAttrId > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_StoreGoodsAttr_version, goodsAttrId));
            }
        }

    }
}
