using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp.Qiye
{
    public class QiyeSpecificationBLL : BaseMySql<QiyeSpecification>
    {
        #region 单例模式
        private static QiyeSpecificationBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeSpecificationBLL()
        {

        }

        public static QiyeSpecificationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeSpecificationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string Redis_QiyeProductSKUList = "QiyeProductSKU_{0}_{1}_{2}";
        private readonly string Redis_QiyeProductSKUList_version = "QiyeProductSKU_version_{0}";

        /// <summary>
        /// 获取产品标签（加缓存）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<QiyeSpecification> GetListByCach(int appId,int fid, int pageSize, int pageIndex, ref int count)
        {
            string strwhere = $"aid={appId} and state=1 and parentid={fid}";
            string key = string.Format(Redis_QiyeProductSKUList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_QiyeProductSKUList_version, appId);
            int entQiyeProductSKU_version = RedisUtil.GetVersion(version_key);

            RedisModel<QiyeSpecification> list = RedisUtil.Get<RedisModel<QiyeSpecification>>(key);

            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || entQiyeProductSKU_version != list.DataVersion)
            {
                list = new RedisModel<QiyeSpecification>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", " sort desc,id asc ");
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = entQiyeProductSKU_version;

                RedisUtil.Set<RedisModel<QiyeSpecification>>(key, list, TimeSpan.FromHours(12));
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
        public void RemoveQiyeProductSKUListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_QiyeProductSKUList_version, appid));
            }
        }

        public QiyeSpecification GetSKUByName(int appId, string name)
        {
            return base.GetModel($"name=@name and aid={appId}  and state=1", new MySqlParameter[] { new MySqlParameter("name", name) });
        }


        /// <summary>
        /// 获取规格数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public int GetCountQiyeProductSKU(int aid,int parentId)
        {
            return base.GetCount($"aid={aid} and state=1 and parentid={parentId}");
        }


    }
}
