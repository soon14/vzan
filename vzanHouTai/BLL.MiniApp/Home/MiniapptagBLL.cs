using DAL.Base;
using Entity.MiniApp.Home;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Home
{
    public class MiniapptagBLL : BaseMySql<Miniapptag>
    {
        #region 单例模式
        private static MiniapptagBLL _singleModel;
        private static readonly object SynObject = new object();

        private MiniapptagBLL()
        {

        }

        public static MiniapptagBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new MiniapptagBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<Miniapptag> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<Miniapptag>();

            return base.GetList($"id in ({ids}) and state>=0");
        }

        public List<Miniapptag> GetListByAgentIdAndTid(string name, int agentId, int tid, ref int count, int pageSize, int pageIndex)
        {
            List<Miniapptag> list = new List<Miniapptag>();
            string sql = $"select {"{0}"} from miniapptag t left join xcxtemplate x on t.tid=x.id";
            string sqlList = string.Format(sql, "t.*,x.tname");
            string sqlCount = string.Format(sql, "count(*)");
            string sqlWhere = $" where t.agentid={agentId} and t.state>=0";
            string sqlLimit = $" LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";
            if (tid != -999)
            {
                sqlWhere += $" and t.tid={tid} ";
            }
            List<MySqlParameter> parms = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += $" and t.tagname like @name";
                parms.Add(new MySqlParameter("@name", $"%{name}%"));
            }
            count = base.GetCountBySql(sqlCount + sqlWhere, parms.ToArray());
            if (count <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere + sqlLimit, parms.ToArray()))
            {
                while (dr.Read())
                {
                    Miniapptag model = base.GetModel(dr);
                    if (dr["tname"] != DBNull.Value)
                    {
                        model.tname = dr["tname"].ToString();
                    }
                    list.Add(model);
                }
            }

            return list;
        }

        public List<Miniapptag> GetListByName(string name, int agentId)
        {
            string sqlWhere = $" agentid={agentId} and state>=0";
            List<MySqlParameter> parms = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += $" and tagname = @name";
                parms.Add(new MySqlParameter("@name", name));
            }

            return base.GetList(sqlWhere,parms.ToArray());
        }
    }
}
