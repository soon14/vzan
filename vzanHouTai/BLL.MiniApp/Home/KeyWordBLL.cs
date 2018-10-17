using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Home;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Home
{
    public class KeyWordBLL : BaseMySql<KeyWord>
    {
        private readonly string _redis_KeyWordListKey = "KeyWordListKey_{0}_{1}";
        private readonly string _redis_KeyWordListVersion = "KeyWordListVersion";//版本控制

        #region 单例模式
        private static KeyWordBLL _singleModel;
        private static readonly object SynObject = new object();

        private KeyWordBLL()
        {

        }

        public static KeyWordBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new KeyWordBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        public List<KeyWord> GetListByCache(int typeId, int pageIndex, int pageSize, string name, ref int count, bool reflesh = false)
        {
            string key = string.Format(_redis_KeyWordListKey, pageIndex, pageSize);
            RedisModel<KeyWord> model = RedisUtil.Get<RedisModel<KeyWord>>(key);
            int dataversion = RedisUtil.GetVersion(_redis_KeyWordListVersion);

            string sqlWhere = $" state>=0 ";
            if (typeId != -999)
            {
                sqlWhere += $" and typeid={typeId} ";
                reflesh = true;
            }

            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += " and name like @name ";
                paras.Add(new MySqlParameter("@name", $"%{name}%"));
                reflesh = true;
            }

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<KeyWord>();

                model.Count = base.GetCount(sqlWhere, paras.ToArray());
                model.DataList = base.GetList(sqlWhere, pageSize, pageIndex, "", "sort desc", paras.ToArray());
                if (!reflesh)
                {
                    RedisUtil.Set<RedisModel<KeyWord>>(key, model);
                }
            }

            count = model.Count;
            return model.DataList;
        }

        public List<KeyWord> GetDataList(int typeId, int pageIndex, int pageSize, string name, ref int count)
        {
            List<KeyWord> list = new List<KeyWord>();

            string sql = $"select {"{0}"}  from keyword k left join keywordtype t on k.typeid=t.id";
            string slqList = string.Format(sql, "k.*,t.name tname");
            string sqlCount = string.Format(sql, "count(*)");
            string sqlWhere = $" where k.state>=-1";
            string sqlLimit = $" ORDER BY k.sort desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

            if (typeId != -999)
            {
                sqlWhere += $" and k.typeid={typeId} ";
            }

            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += " and k.name like @name ";
                paras.Add(new MySqlParameter("@name", $"%{name}%"));
            }

            count = base.GetCountBySql(sqlCount + sqlWhere, paras.ToArray());
            if (count <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, slqList + sqlWhere + sqlLimit, paras.ToArray()))
            {
                while (dr.Read())
                {
                    KeyWord model = base.GetModel(dr);
                    if (dr["tname"] != DBNull.Value)
                    {
                        model.TypeName = dr["tname"].ToString();
                    }
                    list.Add(model);
                }
            }

            return list;
        }
        
        /// <summary>
        /// 判断是否有关键词使用了该类型
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public bool ExitModelByTypeId(int typeId)
        {
            string sqlWhere = $"typeid ={typeId} and state>=0";
            return base.Exists(sqlWhere);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache()
        {
            RedisUtil.SetVersion(_redis_KeyWordListVersion);
        }
    }
}
