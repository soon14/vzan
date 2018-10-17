
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Stores;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Stores
{
    public class StoreGoodsCartBLL : BaseMySql<StoreGoodsCart>
    {
        #region 单例模式
        private static StoreGoodsCartBLL _singleModel;
        private static readonly object SynObject = new object();

        private StoreGoodsCartBLL()
        {

        }

        public static StoreGoodsCartBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new StoreGoodsCartBLL();
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
        public static readonly string Redis_MyCarts = "Miniapp_StoreGoodsCart_{0}";

        /// <summary>
        /// model_version_key  ->根据 权限表Id
        /// </summary>
        public static readonly string Redis_MyCarts_version = "Miniapp_StoreGoodsCart_version_{0}";



        #region  缓存方法

        /// <summary>
        /// 从缓存读取配置 ,如果没有，new 一个默认的对象
        /// </summary>
        /// <param>
        /// </param>
        /// <returns></returns>
        public List<StoreGoodsCart> GetMyCart(int StoreId, int UserId)
        {
            string model_key = string.Format(Redis_MyCarts, UserId);
            string version_key = string.Format(Redis_MyCarts_version, UserId);

            int version = RedisUtil.GetVersion(version_key);
            RedisModel<StoreGoodsCart> redisModel_myCart = RedisUtil.Get<RedisModel<StoreGoodsCart>>(model_key);

            //if (redisModel_myCart == null || redisModel_myCart.DataList == null 
            //        || redisModel_myCart.DataList.Count <= 0 || redisModel_myCart.DataVersion != version)
            //{
                redisModel_myCart = new RedisModel<StoreGoodsCart>();

                List <StoreGoodsCart> myCart = GetList($" UserId = {UserId} and state = 0 ");
                redisModel_myCart.DataList = myCart;
                redisModel_myCart.DataVersion = version;
                redisModel_myCart.Count = myCart.Count;

                RedisUtil.Set<RedisModel<StoreGoodsCart>>(model_key, redisModel_myCart, TimeSpan.FromHours(12));
            //}
            return redisModel_myCart.DataList;
        }

        /// <summary>
        /// 更新此列表缓存最新版本号,达到加载覆盖旧数据
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveStoreGoodsCartCache(int UserId)
        {
            if (UserId > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_MyCarts_version, UserId));
            }
        }
        #endregion

        public List<StoreGoodsCart> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<StoreGoodsCart>();

            return base.GetList($"id in ({ids})");
        }

        public List<StoreGoodsCart> GetListByOrderIds(string orderIds)
        {
            if (string.IsNullOrEmpty(orderIds))
                return new List<StoreGoodsCart>();

            return base.GetList($"GoodsOrderId in ({orderIds})");
        }

        //商品更新的时候，更新购物车里面有效商品的价格
        public void UpdateCartByGoodsId(int gid,string specids,int price)
        {
            var list = GetList($"GoodsId={gid} and IFNULL(Specids,'')='{specids}' and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 ");
            foreach (var item in list)
            {
                item.Price = price;
                Update(item, "Price");
            }
        }


        /// <summary>
        /// 商品/多规格商品 更新商品时,根据规格,应更新购物车内记录状态
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="specids"></param>
        /// <param name="newGoodState"> 0 正常   1 已下架    2 已删除 </param>
        /// <param name="oldGoodState"> 默认查询所有商品状态的记录 </param>
        /// <param name="returnSql"> 是否返回sql而不执行(用于拼接入tranModel语句) </param>
        public List<string> UpdateCartGoodsStateByGoodsIdSpecids(int gid, string specids, int newGoodState, int oldGoodState = 10, bool returnSql = true)
        {
            var list = GetList($"GoodsId={gid} and IFNULL(Specids,'')='{specids}' and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 { (oldGoodState == 10 ? "" : $"and goodsState = {oldGoodState}") }");
            var returnSqlList = new List<string>();
            foreach (var item in list)
            {
                item.GoodsState = newGoodState;
                if (!returnSql)
                {
                    Update(item, "GoodsState");
                }
                else
                {
                    returnSqlList.Add(BuildUpdateSql(item, "GoodsState"));
                }
            }
            return returnSqlList;
        }
        /// <summary>
        /// 商品/多规格商品 上下架/删除时,应更新购物车内记录状态
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="specids"></param>
        /// <param name="newGoodState"> 0 正常   1 已下架    2 已删除 </param>
        /// <param name="oldGoodState"> 默认查询所有商品状态的记录 </param>
        /// <param name="returnSql"> 是否返回sql而不执行(用于拼接入tranModel语句) </param>
        public List<string> UpdateCartGoodsStateByGoodsId(int gid, int newGoodState, int oldGoodState = 10, bool returnSql = true)
        {
            var list = GetList($"GoodsId={gid} and (ISNULL(GoodsOrderId) or GoodsOrderId<=0) and State=0 { (oldGoodState == 10 ? "" : $"and goodsState = {oldGoodState}") }");
            var returnSqlList = new List<string>();
            foreach (var item in list)
            {
                item.GoodsState = newGoodState;
                if (!returnSql)
                {
                    Update(item, "GoodsState");
                }
                else
                {
                    returnSqlList.Add(BuildUpdateSql(item, "GoodsState"));
                }
            }
            return returnSqlList;
        }
        


        public List<StoreGoodsCart> GetMyCartById(string ids)
        {
          return GetList($" Id in ({ids})");
        }
    }
}
