using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntSpecificationBLL : BaseMySql<EntSpecification>
    {
        #region 单例模式
        private static EntSpecificationBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntSpecificationBLL()
        {

        }

        public static EntSpecificationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntSpecificationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string Redis_EntSpecificationList = "EntSpecification_{0}_{1}_{2}";
        private readonly string Redis_EntSpecificationList_version = "EntSpecification_version_{0}";


        public string GetEntSpecificationName(string specificationkeys)
        {
            string sql = $"SELECT GROUP_CONCAT(`name`) from EntSpecification where FIND_IN_SET(id,@specificationkeys)";
            return DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@specificationkeys", specificationkeys) }).ToString();
        }


        public string GetSKUId(int aid, string skuName,string parentIds="")
        {
            string sql = $"SELECT id from EntSpecification where aid={aid} and name=@skuName and state=1 ";
            if (!string.IsNullOrEmpty(parentIds))
            {
                sql += $" and parentid in({parentIds})";
            }
            
            object id = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                  CommandType.Text, sql,
                  new MySqlParameter[] { new MySqlParameter("@skuName", skuName) });
            if (id != DBNull.Value)
            {
                return Convert.ToString(id);
            }
            return string.Empty;

        }



        /// <summary>
        /// 获取产品标签（加缓存）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<EntSpecification> GetListByCach(int appId,int fid, int pageSize, int pageIndex, ref int count)
        {
            string strwhere = $"aid={appId} and state=1 and parentid={fid}";
            string key = string.Format(Redis_EntSpecificationList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_EntSpecificationList_version, appId);
            int entEntSpecification_version = RedisUtil.GetVersion(version_key);

            RedisModel<EntSpecification> list = RedisUtil.Get<RedisModel<EntSpecification>>(key);

            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || entEntSpecification_version != list.DataVersion)
            {
                list = new RedisModel<EntSpecification>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", " sort desc,id asc ");
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = entEntSpecification_version;

                RedisUtil.Set<RedisModel<EntSpecification>>(key, list, TimeSpan.FromHours(12));
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
        public void RemoveEntSpecificationListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_EntSpecificationList_version, appid));
            }
        }
    }
}
