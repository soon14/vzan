using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsTypeBLL : BaseMySql<StoreGoodsType>
    {
        #region 单例模式
        private static StoreGoodsTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsTypeBLL()
        {

        }

        public static StoreGoodsTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion


        public List<StoreGoodsType> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<StoreGoodsType>();

            return base.GetList($"id in ({ids})");
        }

        /// <summary>
        /// model_key  ->根据 StoreId
        /// </summary>
        public static readonly string Redis_StoreGoodsType = "Miniapp_StoreGoodsType_{0}";

        /// <summary>
        /// model_version_key  ->根据 StoreId
        /// </summary>
        public static readonly string Redis_StoreGoodsType_version = "Miniapp_StoreGoodsType_version_{0}";


        /// <summary>
        /// 查询店铺的分类 缓存
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public List<StoreGoodsType> GetlistByStoreId(int storeId)
        {

            string model_key = string.Format(Redis_StoreGoodsType, storeId);
            string version_key = string.Format(Redis_StoreGoodsType_version, storeId);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<StoreGoodsType> redisModel_StoreGoodsType = RedisUtil.Get<RedisModel<StoreGoodsType>>(model_key);

            //if (redisModel_StoreGoodsType == null || redisModel_StoreGoodsType.DataList == null 
            //        || redisModel_StoreGoodsType.DataList.Count <= 0 || redisModel_StoreGoodsType.DataVersion != version)
            //{
                redisModel_StoreGoodsType = new RedisModel<StoreGoodsType>();
                List<StoreGoodsType> StoreGoodsType = GetList($"StoreId={storeId} and State>=0", 50, 1, "Id,Name,SortVal,LogImg", "SortVal desc,Id desc");

                redisModel_StoreGoodsType.DataList = StoreGoodsType;
                redisModel_StoreGoodsType.DataVersion = version;
                redisModel_StoreGoodsType.Count = GetCountByStoreId(storeId);

                RedisUtil.Set<RedisModel<StoreGoodsType>>(model_key, redisModel_StoreGoodsType, TimeSpan.FromHours(12));
            //}
            return redisModel_StoreGoodsType.DataList;
        }

        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveStoreGoodsTypeCache(int storeId)
        {
            if (storeId > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_StoreGoodsType_version, storeId));
            }
        }


        public int GetCountByStoreId(int storeId)
        {
            return GetCount($"StoreId={storeId} and State>=0");
        }


        /// <summary>
        /// 批量更新排序
        /// </summary>
        /// <param name="list"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public bool UpdateListSort(List<StoreGoodsType> list, int storeId)
        {
            if (list == null || list.Count <= 0)
            {
                return false;
            }
            DateTime date = DateTime.Now;

            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                string sql = $"update  StoreGoodsType set SortVal={item.SortVal},UpdateDate='{date}'  where Id={item.Id};";
                item.UpdateDate = date;
                sb.Append(sql);
            }
            this.RemoveStoreGoodsTypeCache(storeId);
            return ExecuteNonQuery(sb.ToString()) > 0;
        }




    }
}
