using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsAttrSpecBLL : BaseMySql<StoreGoodsAttrSpec>
    {
        #region 单例模式
        private static StoreGoodsAttrSpecBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsAttrSpecBLL()
        {

        }

        public static StoreGoodsAttrSpecBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsAttrSpecBLL();
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
        private static readonly string Redis_StoreGoodsAttrSpecList = "Miniapp_StoreGoodsAttrSpecList_{0}";

        /// <summary>
        /// 缓存version号
        /// </summary>
        private static readonly string Redis_StoreGoodsAttrSpecList_version = "Miniapp_StoreGoodsAttrSpecList_version_{0}";


        /// <summary>
        /// 根据缓存读取 商品规格/规格值
        /// </summary>
        /// <param name="goodId">商品Id</param>
        /// <returns></returns>
        public List<StoreGoodsAttrSpec> GetListByGoodsIdCache(int goodId)
        {
            string models_key = string.Format(Redis_StoreGoodsAttrSpecList, goodId);
            string version_key = string.Format(Redis_StoreGoodsAttrSpecList_version, goodId);

            RedisModel<StoreGoodsAttrSpec> redisModel_StoreGoodsAttrSpec = RedisUtil.Get<RedisModel<StoreGoodsAttrSpec>>(models_key);
            int version = RedisUtil.GetVersion(version_key);

            //if (redisModel_StoreGoodsAttrSpec == null || redisModel_StoreGoodsAttrSpec.DataList == null 
            //        || redisModel_StoreGoodsAttrSpec.DataList.Count <= 0 || redisModel_StoreGoodsAttrSpec.DataVersion != version)
            //{
                redisModel_StoreGoodsAttrSpec = new RedisModel<StoreGoodsAttrSpec>();
                List<StoreGoodsAttrSpec> storeGoodsAttrSpecs = GetList($" GoodsId = {goodId} ");

                redisModel_StoreGoodsAttrSpec.DataList = storeGoodsAttrSpecs;
                redisModel_StoreGoodsAttrSpec.DataVersion = version;
                redisModel_StoreGoodsAttrSpec.Count = storeGoodsAttrSpecs.Count;

                RedisUtil.Set<RedisModel<StoreGoodsAttrSpec>>(models_key, redisModel_StoreGoodsAttrSpec, TimeSpan.FromHours(12));
            //}
            return redisModel_StoreGoodsAttrSpec.DataList;
        }


        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveStoreGoodsAttrSpecsCache(int goodId)
        {
            if (goodId > 0)
            {

                RedisUtil.SetVersion(string.Format(Redis_StoreGoodsAttrSpecList_version, goodId));
            }
        }
    }
}
