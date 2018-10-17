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
    public class EntGoodLabelBLL : BaseMySql<EntGoodLabel>
    {
        #region 单例模式
        private static EntGoodLabelBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodLabelBLL()
        {

        }

        public static EntGoodLabelBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodLabelBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private readonly string Redis_EntGoodLabelList = "EntGoodLabel_{0}_{1}_{2}";
        private readonly string Redis_EntGoodLabelList_version = "EntGoodLabel_version_{0}";

        /// <summary>
        /// 根据名称获取Id
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="labelName"></param>
        /// <returns></returns>
        public string GetEntGoodLabelId(int aid, string labelName)
        {
            string sql = $"SELECT id from EntGoodLabel where aid={aid} and name=@labelName and state=1";
            object id = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                  CommandType.Text, sql,
                  new MySqlParameter[] { new MySqlParameter("@labelName", labelName) });
            if (id != DBNull.Value)
            {
                return Convert.ToString(id);
            }
            return string.Empty;

        }



        public string GetEntGoodsLabelStr(string plabels)
        {
            string sql = $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({plabels})";
            object result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if(result !=DBNull.Value)
            {
                return result.ToString();
            }

            return "";
        }
        public string GetEntGoodsLabel(string plabels)
        {
            string sql = $"SELECT group_concat(name order by sort desc) from entgoodlabel where FIND_IN_SET(id,@plabels)";
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
        public List<EntGoodLabel> GetListByCach(int appId, int pageSize, int pageIndex, ref int count)
        {
            string strwhere = $"aid={appId} and state=1";
            string key = string.Format(Redis_EntGoodLabelList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_EntGoodLabelList_version, appId);
            int entGoodLabelList_version = RedisUtil.GetVersion(version_key);

            RedisModel<EntGoodLabel> list = RedisUtil.Get<RedisModel<EntGoodLabel>>(key);

            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || entGoodLabelList_version != list.DataVersion)
            {
                list = new RedisModel<EntGoodLabel>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", " sort desc,id asc ");
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = entGoodLabelList_version;

                RedisUtil.Set<RedisModel<EntGoodLabel>>(key, list, TimeSpan.FromHours(12));
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
        public void RemoveEntGoodLabelListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_EntGoodLabelList_version, appid));
            }
        }
    }
}
