using BLL.MiniApp.Home;
using DAL.Base;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Home;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class AgentinfoCaseBLL : BaseMySql<AgentinfoCase>
    {
        #region 单例模式
        private static AgentinfoCaseBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentinfoCaseBLL()
        {

        }

        public static AgentinfoCaseBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentinfoCaseBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<AgentinfoCase> GetCaseList(string tagName,int state,string name, int agentId, int tid, int pageIndex, int pageSize, ref int count)
        {
            List<AgentinfoCase> list = new List<AgentinfoCase>();

            string sql = $"select {"{0}"} from agentinfocase a left join xcxtemplate x on a.tid=x.id";
            string sqlList = string.Format(sql, " a.*,x.TName ");
            string sqlCount = string.Format(sql, " count(*) ");
            string sqlWhere = $" where a.agentid={agentId} and a.state>=0";
            string sqlLimit = $" ORDER BY a.sort desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

            if (tid != -999)
            {
                sqlWhere += $" and a.tid={tid}";
            }
            if(state!=-999)
            {
                sqlWhere += $" and a.state={state}";
            }
            List<MySqlParameter> parms = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += $" and CaseName like @name";
                parms.Add(new MySqlParameter("@name", $"%{name}%"));
            }
            if (!string.IsNullOrEmpty(tagName))
            {
                List<Miniapptag> tagNameList= MiniapptagBLL.SingleModel.GetListByName(tagName,agentId);
                if(tagNameList==null || tagNameList.Count<=0)
                {
                    return list;
                }
                StringBuilder sqlWhereBuilder = new StringBuilder();
                foreach (Miniapptag item in tagNameList)
                {
                    sqlWhereBuilder.Append($" FIND_IN_SET({item.id},tagids) or");
                }
                sqlWhere += $" and ({sqlWhereBuilder.ToString().TrimEnd('r').TrimEnd('o')})";
            }

            count = base.GetCountBySql(sqlCount + sqlWhere, parms.ToArray());
            if (count <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere + sqlLimit, parms.ToArray()))
            {
                while (dr.Read())
                {
                    AgentinfoCase model = base.GetModel(dr);
                    if (dr["TName"] != DBNull.Value)
                    {
                        model.TName = dr["TName"].ToString();
                    }
                    list.Add(model);
                }
            }

            string tagIds = string.Join(",", list.Where(w => !string.IsNullOrEmpty(w.TagIds))?.Select(s => s.TagIds));
            List<Miniapptag> tagList = MiniapptagBLL.SingleModel.GetListByIds(tagIds);
            if (tagList != null && tagList.Count > 0)
            {
                foreach (AgentinfoCase item in list)
                {
                    if (!string.IsNullOrEmpty(item.TagIds))
                    {
                        string tempTagIds = $",{item.TagIds},";
                        List<Miniapptag> tempTagList = tagList.Where(w => tempTagIds.Contains($",{w.id},")).ToList();
                        if(tempTagList!=null && tempTagList.Count>0)
                        {
                            item.TagNames = string.Join(" | ",tempTagList.Select(s=>s.tagname));
                        }
                    }
                }
            }

            return list;
        }
    }
}
