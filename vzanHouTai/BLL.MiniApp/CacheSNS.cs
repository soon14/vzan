using DAL.Base;
using Entity.MiniApp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class CacheSNS
    {
        private static System.Web.Caching.Cache cacheManager = System.Web.HttpRuntime.Cache;

        #region 单例模式
        private static CacheSNS _singleModel;
        private static readonly object SynObject = new object();

        private CacheSNS()
        {

        }

        public static CacheSNS SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CacheSNS();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 用户权限列表
        /// </summary>
        public static ConcurrentDictionary<int, RightItem> dicDefaultRoleRight
        {
            get
            {
                var dic=RedisUtil.Get<ConcurrentDictionary<int, RightItem>>(MemCacheKey.DefaultRightsKeyRedis);
                if (dic == null)
                {
                    dic = new ConcurrentDictionary<int, RightItem>();
                    List<CategoryRole> list = new CategoryRoleBLL().GetList("CategoryId=0");
                    foreach(CategoryRole model in list) 
                    {
                        dic[model.RightCode] = model.RightObj;
                    }
                    RedisUtil.Set<ConcurrentDictionary<int, RightItem>>(MemCacheKey.DefaultRightsKeyRedis,dic,TimeSpan.FromDays(30));
                }
                return dic;
            }
        }

        /// <summary>
        /// 全局管理员列表
        /// </summary>
        public static ConcurrentDictionary<string , WholeAdmin> dicWholeAdmin
        {
            get
            {
                var dic=RedisUtil.Get<ConcurrentDictionary<string, WholeAdmin>>(MemCacheKey.WholeAdminKeyRedis);
                if (dic == null)
                {
                    dic = new ConcurrentDictionary<string, WholeAdmin>();
                    List<WholeAdmin> list = new WholeAdminBLL().GetList(); 
                    foreach (WholeAdmin model in list)
                    {
                        dic[model.Openid] = model;
                    }
                    RedisUtil.Set<ConcurrentDictionary<string, WholeAdmin>>(MemCacheKey.WholeAdminKeyRedis,dic,TimeSpan.FromDays(30));
                }
                return dic;
            }
        }

        ///// <summary>
        ///// WebConfig 表的缓存
        ///// xiaowei 2016-09-13 10:57:59
        ///// 用法： dicWebConfig[key]，注意判断空值和默认值
        ///// </summary>
        //public static List<WebConfig> ListWebConfig
        //{
        //    get
        //    {
        //        string cache_key = "minisns_webconfig";
        //        List<WebConfig> list = (List<WebConfig>)cacheManager.Get(cache_key);
        //        if (list == null || list.Count == 0)
        //        {
        //            list = new WebConfigBLL().GetList();
        //            cacheManager.Insert(cache_key, list, null, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.High, null);
        //        }
        //        return list;
        //    }
        //}
    }
}
