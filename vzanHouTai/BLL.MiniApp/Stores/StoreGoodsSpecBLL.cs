using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsSpecBLL : BaseMySql<StoreGoodsSpec>
    {
        #region 单例模式
        private static StoreGoodsSpecBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsSpecBLL()
        {

        }

        public static StoreGoodsSpecBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsSpecBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// model_key  ->根据 权限表Id
        /// </summary>
        public static readonly string Redis_StoreGoodsSpec = "Miniapp_StoreGoodsSpec_{0}";

        /// <summary>
        /// model_version_key  ->根据 权限表Id
        /// </summary>
        public static readonly string Redis_StoreGoodsSpec_version = "Miniapp_StoreGoodsSpec_version_{0}";


        public StoreGoodsSpec GetModelByCache(int id)
        {
            string model_key = string.Format(Redis_StoreGoodsSpec, id);
            string version_key = string.Format(Redis_StoreGoodsSpec_version, id);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<StoreGoodsSpec> redisModel_StoreGoodsSpec = RedisUtil.Get<RedisModel<StoreGoodsSpec>>(model_key);

            if (redisModel_StoreGoodsSpec == null || redisModel_StoreGoodsSpec.DataList == null  
                    || redisModel_StoreGoodsSpec.DataList.Count <= 0 || redisModel_StoreGoodsSpec.DataVersion != version)
            {
                redisModel_StoreGoodsSpec = new RedisModel<StoreGoodsSpec>();
                List<StoreGoodsSpec> StoreGoodsSpecs = new List<StoreGoodsSpec>() { base.GetModel(id) };

                redisModel_StoreGoodsSpec.DataList = StoreGoodsSpecs;
                redisModel_StoreGoodsSpec.DataVersion = version;
                redisModel_StoreGoodsSpec.Count = StoreGoodsSpecs.Count;

                RedisUtil.Set<RedisModel<StoreGoodsSpec>>(model_key, redisModel_StoreGoodsSpec, TimeSpan.FromHours(12));
            }
            return redisModel_StoreGoodsSpec.DataList[0];
        }

        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveStoreGoodsSpecCache(int id)
        {
            if (id > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_StoreGoodsSpec_version, id));
            }
        }

    }
}
