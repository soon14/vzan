using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.PlatChild;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.PlatChild
{
    public class PlatChildSpecificationBLL : BaseMySql<PlatChildSpecification>
    {
        #region 单例模式
        private static PlatChildSpecificationBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatChildSpecificationBLL()
        {

        }

        public static PlatChildSpecificationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatChildSpecificationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static readonly string Redis_PlatChildProductSKUList = "PlatChildProductSKU_{0}_{1}_{2}";
        public static readonly string Redis_PlatChildProductSKUList_version = "PlatChildProductSKU_version_{0}";

        /// <summary>
        /// 获取产品标签（加缓存）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<PlatChildSpecification> GetListByCach(int appId,int fid, int pageSize, int pageIndex, ref int count)
        {
            string strwhere = $"aid={appId} and state=1 and parentid={fid}";
            string key = string.Format(Redis_PlatChildProductSKUList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_PlatChildProductSKUList_version, appId);
            int entPlatChildProductSKU_version = RedisUtil.GetVersion(version_key);

            RedisModel<PlatChildSpecification> list = RedisUtil.Get<RedisModel<PlatChildSpecification>>(key);

            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || entPlatChildProductSKU_version != list.DataVersion)
            {
                list = new RedisModel<PlatChildSpecification>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", " sort desc,id asc ");
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = entPlatChildProductSKU_version;

                RedisUtil.Set<RedisModel<PlatChildSpecification>>(key, list, TimeSpan.FromHours(12));
            }
            else
            {
                count = list.Count;
            }

            return list.DataList;
        }

        /// <summary>
        /// 清除行业类型的列表缓存
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemovePlatChildProductSKUListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_PlatChildProductSKUList_version, appid));
            }
        }

        public PlatChildSpecification GetSKUByName(int appId, string name)
        {
            return base.GetModel($"name=@name and aid={appId}  and state=1", new MySqlParameter[] { new MySqlParameter("name", name) });
        }


        /// <summary>
        /// 获取规格数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public int GetCountPlatChildProductSKU(int aid,int parentId)
        {
            return base.GetCount($"aid={aid} and state=1 and parentid={parentId}");
        }


    }
}
