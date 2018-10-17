using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Plat;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Qiye
{
    public class QiyeGoodsLabelBLL : BaseMySql<QiyeGoodsLabel>
    {
        #region 单例模式
        private static QiyeGoodsLabelBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeGoodsLabelBLL()
        {

        }

        public static QiyeGoodsLabelBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeGoodsLabelBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string Redis_QiyeProductLabelList = "QiyeProductLabel_{0}_{1}_{2}";
        private readonly string Redis_QiyeProductLabelList_version = "QiyeProductLabel_version_{0}";


        public List<QiyeGoodsLabel> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<QiyeGoodsLabel>();

            return base.GetList($"id in ({ids})");
        }
        public string GetEntGoodsLabelStr(string plabels)
        {
            string sql = $"SELECT group_concat(name order by sort desc) from QiyeGoodsLabel where id in ({plabels})";
            object result = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if(result !=DBNull.Value)
            {
                return result.ToString();
            }

            return "";
        }
        public string GetGoodsLabel(string plabels)
        {
            string sql = $"SELECT group_concat(name order by sort desc) from QiyeGoodsLabel where FIND_IN_SET(id,@plabels)";
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
        public List<QiyeGoodsLabel> GetListByCach(int appId, int pageSize, int pageIndex, ref int count)
        {
            string strwhere = $"aid={appId} and state=1";
            string key = string.Format(Redis_QiyeProductLabelList, appId, pageSize, pageIndex);
            string version_key = string.Format(Redis_QiyeProductLabelList_version, appId);
            int QiyeProductLabelList_version = RedisUtil.GetVersion(version_key);

            RedisModel<QiyeGoodsLabel> list = RedisUtil.Get<RedisModel<QiyeGoodsLabel>>(key);

            if (list == null || list.DataList == null || list.DataList.Count <= 0 || list.Count <= 0 || QiyeProductLabelList_version != list.DataVersion)
            {
                list = new RedisModel<QiyeGoodsLabel>();
                list.DataList = GetList(strwhere, pageSize, pageIndex, "*", " sort desc,id asc ");
                count = GetCount(strwhere);
                list.Count = count;
                list.DataVersion = QiyeProductLabelList_version;

                RedisUtil.Set<RedisModel<QiyeGoodsLabel>>(key, list, TimeSpan.FromHours(12));
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
        public void RemoveQiyeProductLabelListCache(int appid)
        {
            if (appid > 0)
            {
                RedisUtil.SetVersion(string.Format(Redis_QiyeProductLabelList_version, appid));
            }
        }

        public QiyeGoodsLabel GetLabelByName(int appId, string name)
        {
          return base.GetModel($"name=@name and aid={appId}  and state=1", new MySqlParameter[] { new MySqlParameter("name", name) });
        }


        public int GetProductLabelCount(int appId)
        {
           return base.GetCount($"aid={appId} and state=1");
        }



    }
}
