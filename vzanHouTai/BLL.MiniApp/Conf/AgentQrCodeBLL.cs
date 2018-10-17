using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class AgentQrCodeBLL : BaseMySql<AgentQrCode>
    {
        #region 单例模式
        private static AgentQrCodeBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentQrCodeBLL()
        {

        }

        public static AgentQrCodeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentQrCodeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        private readonly string _redis_AgentQrCodeKey = "redis_AgentQrCode_{0}_{1}_{2}";
        private readonly string _redis_AgentQrCodeVersion = "redis_AgentQrCodeVersion_{0}";//版本控制

        public bool ExitModel(string name, int agentid)
        {
            MySqlParameter[] parm = new MySqlParameter[] { new MySqlParameter("@Name", name) };
            return base.Exists($"Name=@Name and agentid={agentid} and state<>-2", parm);
        }

        public AgentQrCode GetModelById(int id)
        {
            string sqlwhere = $"id={id} and state<>-2";
            return base.GetModel(sqlwhere);
        }
        public int GetCountByAgentId(int agentid)
        {
            string sqlwhere = $"agentid={agentid} and state<>-2";
            return base.GetCount(sqlwhere);
        }

        public AgentQrCode GetModelById(int agentid, int id)
        {
            return base.GetModel($"id={id} and agentid={agentid} and state<>-2");
        }
        
        public List<AgentQrCode> GetListByAgentId(int agentid, int pageSize, int pageIndex)
        {
            string sqlwhere = $"agentid={agentid} and state<>-2";
            return base.GetList(sqlwhere, pageSize, pageIndex, "*");
        }

        public List<AgentQrCode> GetDataList(int agentid, int pageSize, int pageIndex)
        {
            List<AgentQrCode> list = new List<AgentQrCode>();
            string sql = "select *,(select count(*) from AgentDistributionRelation where QrCodeId = qr.id) opencount from agentqrcode qr";
            string sqlwhere = $" where qr.agentid={agentid} and qr.state<>-2";
            string sqllimit = $" LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit, null))
            {
                while (dr.Read())
                {
                    AgentQrCode amodel = base.GetModel(dr);
                    if (dr["opencount"] != DBNull.Value)
                    {
                        amodel.OpenCount = Convert.ToInt32(dr["opencount"]);
                    }
                    list.Add(amodel);
                }
            }
            return list;
        }

        public List<AgentQrCode> GetAgentQrCodeList(int agentid, int pageIndex, int pageSize, ref int count, bool reflesh = false)
        {
            RedisModel<AgentQrCode> model = new RedisModel<AgentQrCode>();
            model = RedisUtil.Get<RedisModel<AgentQrCode>>(string.Format(_redis_AgentQrCodeKey, agentid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_AgentQrCodeVersion, agentid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<AgentQrCode>();
                List<AgentQrCode> list = GetDataList(agentid, pageSize, pageIndex);

                if (list == null || list.Count <= 0)
                {
                    return new List<AgentQrCode>();
                }

                string qrcodeids = string.Join(",", list.Select(s => s.Id).Distinct());
                List<AgentCustomerRelation> rlist = AgentCustomerRelationBLL.SingleModel.GetListByQrCodeId(qrcodeids);
                if (rlist != null && rlist.Count > 0)
                {
                    string userids = "'" + string.Join("','", rlist.Select(s => s.useraccountid)) + "'";
                    List<Account> accountlist = AccountBLL.SingleModel.GetListByAccountids(userids);
                    if (accountlist != null && accountlist.Count > 0)
                    {
                        foreach (AgentQrCode item in list)
                        {
                            AgentCustomerRelation rmodel = rlist.FirstOrDefault(f => f.QrcodeId == item.Id);
                            if (rmodel == null)
                                continue;
                            Account account = accountlist.Where(w => w.Id.ToString() == rmodel.useraccountid).FirstOrDefault();
                            if (account == null)
                                continue;
                            item.LoginId = account.LoginId;
                            item.OpenExtension = rmodel.OpenExtension;
                            item.CustomerRelationId = rmodel.id;
                        }
                    }

                }

                count = GetCountByAgentId(agentid);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                RedisUtil.Set<RedisModel<AgentQrCode>>(string.Format(_redis_AgentQrCodeKey, agentid, pageSize, pageIndex), model);
            }
            else
            {
                count = model.Count;
            }

            return model.DataList;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache(int agentid)
        {
            if (agentid > 0)
            {
                RedisUtil.SetVersion(string.Format(_redis_AgentQrCodeVersion, agentid));
            }
        }
    }
}
