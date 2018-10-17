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
    public class AgentCaseBackBLL : BaseMySql<AgentCaseBack>
    {
        #region 单例模式
        private static AgentCaseBackBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentCaseBackBLL()
        {

        }

        public static AgentCaseBackBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentCaseBackBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private readonly string _redis_AgentCaseBackKey = "redis_AgentCaseBack_{0}_{1}_{2}_{3}";
        private readonly string _redis_AgentCaseBackVersion = "redis_AgentCaseBackVersion_{0}";//版本控制

        public List<AgentCaseBack> GetAgentCaseBackList(string agentname,string soucefrom, string starttime, string endtime, string agentid, int pageIndex, int pageSize, ref int count, bool reflesh = false,int datatype=0)
        {
            RedisModel<AgentCaseBack> model = new RedisModel<AgentCaseBack>();
            model = RedisUtil.Get<RedisModel<AgentCaseBack>>(string.Format(_redis_AgentCaseBackKey, agentid, pageSize, pageIndex, datatype));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_AgentCaseBackVersion, agentid));

            if (!string.IsNullOrEmpty(starttime) || !string.IsNullOrEmpty(endtime))
            {
                reflesh = true;
            }

            string othersqlwhere = "";
            if(!string.IsNullOrEmpty(agentname))
            {
                othersqlwhere += $" and a.name='{agentname}'";
                Account account = AccountBLL.SingleModel.GetModelByLoginid(agentname);
                if(account!=null)
                {
                    othersqlwhere = $" and (a.name='{agentname}' or a.useraccountid='{account.Id}') ";
                }
                reflesh = true;
            }
            if (!string.IsNullOrEmpty(soucefrom))
            {
                string accountid = "";
                Account account = AccountBLL.SingleModel.GetModelByLoginid(soucefrom);
                if(account==null || account?.Id == Guid.Empty)
                {
                    accountid = "";
                }
                else
                {
                    accountid = account.Id.ToString();
                }
                List<AgentCustomerRelation> crmodel = AgentCustomerRelationBLL.SingleModel.GetListByAccountId(agentid, accountid, soucefrom);
                if (crmodel == null || crmodel.Count<=0)
                {
                    return new List<AgentCaseBack>();
                }

                string qrcodeids = string.Join(",",crmodel.Select(s=>s.QrcodeId).Distinct());
                othersqlwhere += $" and ad.qrcodeid in ({qrcodeids})";
                reflesh = true;
            }

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<AgentCaseBack>();
                List<AgentCaseBack> list = new List<AgentCaseBack>();
                string sqlcount = $" select count(*) from AgentCaseBack ac right join (select * from agentdistributionrelation where parentagentid in ({agentid}) UNION select* from agentdistributionrelation where parentagentid in (select agentid from agentdistributionrelation where parentagentid in ({agentid}))) ad on ac.AgentDistributionRelatioinId = ad.id left join agentinfo a on a.id = ad.agentid";
                string sql = $" select ac.*,ad.`level`,ad.agentid,ad.qrcodeid,a.name agentname,a.useraccountid,a.LastDeposit from AgentCaseBack ac right join (select *,'一级分销' level from agentdistributionrelation where parentagentid in ({agentid}) UNION select *,'二级分销' level from agentdistributionrelation where parentagentid in (select agentid from agentdistributionrelation where parentagentid in ({agentid}))) ad on ac.AgentDistributionRelatioinId = ad.id left join agentinfo a on a.id = ad.agentid";
                string sqlwhere = $" where  1=1 and ac.id>0 and ac.datatype={datatype} {othersqlwhere}";
                string sqllimit = $" ORDER BY ac.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";
                if (!string.IsNullOrEmpty(starttime))
                {
                    sqlwhere += $" and ac.addtime>='{starttime}' ";
                }
                if (!string.IsNullOrEmpty(endtime))
                {
                    sqlwhere += $" and ac.addtime<='{endtime} 23:59' ";
                }

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit, null))
                {
                    while (dr.Read())
                    {
                        AgentCaseBack amodel = base.GetModel(dr);
                        amodel.Level = dr["level"].ToString();
                        amodel.AgentName = dr["agentname"].ToString();
                        amodel.UseraccountId = dr["useraccountid"].ToString();
                        if (dr["agentid"] != DBNull.Value)
                        {
                            amodel.AgentId = Convert.ToInt32(dr["agentid"]);
                        }
                        if (dr["qrcodeid"] != DBNull.Value)
                        {
                            amodel.QrCodeId = Convert.ToInt32(dr["qrcodeid"]);
                        }
                        if (dr["LastDeposit"] != DBNull.Value)
                        {
                            amodel.LastDeposit = Convert.ToInt32(dr["LastDeposit"]);
                        }
                        list.Add(amodel);
                    }
                }

                if (list == null || list.Count <= 0)
                {
                    return new List<AgentCaseBack>();
                }
                string qrcodeids = string.Join("", list.Select(s => s.QrCodeId).Distinct());
                List<AgentCustomerRelation> customerRelationlist = AgentCustomerRelationBLL.SingleModel.GetListByQrCodeId(qrcodeids);
                string cruserids = "'" + string.Join("','", customerRelationlist.Select(s => s.useraccountid)) + "'";
                string userids = "'" + string.Join("','", list.Select(s => s.UseraccountId)) + "'";
                if (cruserids.Length > 0)
                {
                    userids = userids + "," + cruserids;
                }
                List<Account> accountlist = AccountBLL.SingleModel.GetListByAccountids(userids);
                foreach (AgentCaseBack item in list)
                {
                    Account aitem = accountlist?.Where(w => w.Id.ToString() == item.UseraccountId).FirstOrDefault();
                    item.LoginId = aitem?.LoginId;
                    AgentCustomerRelation crmodel = customerRelationlist?.Where(w => w.QrcodeId == item.QrCodeId).FirstOrDefault();
                    Account critem = accountlist?.Where(w => w.Id.ToString() == crmodel?.useraccountid).FirstOrDefault();
                    item.SourceFrom = crmodel?.username + "/" + critem?.LoginId;
                }

                count = base.GetCountBySql(sqlcount + sqlwhere);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                if (!reflesh)
                {
                    RedisUtil.Set<RedisModel<AgentCaseBack>>(string.Format(_redis_AgentCaseBackKey, agentid, pageSize, pageIndex,datatype), model);
                }
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
                RedisUtil.SetVersion(string.Format(_redis_AgentCaseBackVersion, agentid));
            }
        }
    }
}
