using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.PlatChild
{
    public class PlatChildGoodsLabelBLL : BaseMySql<PlatChildGoodsLabel>
    {
        #region 单例模式
        private static PlatChildGoodsLabelBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatChildGoodsLabelBLL()
        {

        }

        public static PlatChildGoodsLabelBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatChildGoodsLabelBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static readonly string Redis_PlatChildProductLabelList = "PlatChildProductLabel_{0}_{1}_{2}";
        public static readonly string Redis_PlatChildProductLabelList_version = "PlatChildProductLabel_version_{0}";

        public List<PlatChildGoodsLabel> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<PlatChildGoodsLabel>();

            return base.GetList($"id in ({ids})");
        }


        public string GetEntGoodsLabelStr(string plabels)
        {
            string sql = $"SELECT group_concat(name order by sort desc) from PlatChildGoodsLabel where id in ({plabels})";
            object result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if(result !=DBNull.Value)
            {
                return result.ToString();
            }

            return "";
        }
        public string GetGoodsLabel(string plabels)
        {
            string sql = $"SELECT group_concat(name order by sort desc) from PlatChildGoodsLabel where FIND_IN_SET(id,@plabels)";
            return DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@plabels", plabels) }).ToString();
        }

        /// <summary>
        /// 获取产品标签（加缓存）
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<PlatChildGoodsLabel> GetListByCach(int appId, int pageSize, int pageIndex, ref int count)
        {
            string strwhere = $"aid={appId} and state=1";
            string key = string.Format(Redis_PlatChildProductLabelList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_PlatChildProductLabelList_version, appId);
            int PlatChildProductLabelList_version = RedisUtil.GetVersion(version_key);

            RedisModel<PlatChildGoodsLabel> list = RedisUtil.Get<RedisModel<PlatChildGoodsLabel>>(key);

            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || PlatChildProductLabelList_version != list.DataVersion)
            {
                list = new RedisModel<PlatChildGoodsLabel>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", " sort desc,id asc ");
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = PlatChildProductLabelList_version;

                RedisUtil.Set<RedisModel<PlatChildGoodsLabel>>(key, list, TimeSpan.FromHours(12));
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
        public void RemovePlatChildProductLabelListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_PlatChildProductLabelList_version, appid));
            }
        }

        public PlatChildGoodsLabel GetLabelByName(int appId, string name)
        {
          return base.GetModel($"name=@name and aid={appId}  and state=1", new MySqlParameter[] { new MySqlParameter("name", name) });
        }


        public int GetProductLabelCount(int appId)
        {
           return base.GetCount($"aid={appId} and state=1");
        }



    }
}
