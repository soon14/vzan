using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
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
    public class AgentDistributionRelationBLL : BaseMySql<AgentDistributionRelation>
    {
        private readonly string _redis_AgentDistributionRelationKey = "redis_AgentDistributionRelation_{0}_{1}_{2}_{3}";
        public readonly string _redis_AgentDistributionRelationVersion = "redis_AgentDistributionRelationVersion_{0}";//版本控制

        public string CreateDistributionAgent(string accountid, int agentqrcodeid, int opentype = 0, string username = "",string appid="")
        {
            int followState = 0;
            //免费版，用户提交的资料直接分配给小未
            if(!string.IsNullOrEmpty(appid))
            {
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
                if(xcxrelation!=null)
                {
                    //int xcxType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(xcxrelation.Id);
                    followState = xcxrelation.Type == (int)TmpType.小程序企业模板 || xcxrelation.Type == (int)TmpType.小程序单页模板 ? 1 : 0;
                }
            }
            
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(accountid);
            if (agentmodel != null)
            {
                return "您已经是代理商了";
            }

            AgentQrCode agentqrmodel = AgentQrCodeBLL.SingleModel.GetModel(agentqrcodeid);
            if (agentqrmodel == null)
            {
                return "代理分销二维码已失效";
            }
            Agentinfo pagentmodel = AgentinfoBLL.SingleModel.GetModel(agentqrmodel.AgentId);
            if (pagentmodel == null || pagentmodel.id<=0)
            {
                return "该代理已被停用";
            }

            AgentDistributionRelation relation = new AgentDistributionRelation();
            relation.AddTime = DateTime.Now;
            relation.State = 1;
            relation.QrCodeId = agentqrcodeid;
            relation.UpdateTime = DateTime.Now;
            relation.FollowState = followState;
            relation.OpenType = opentype;
            relation.Id = Convert.ToInt32(base.Add(relation));
            if (relation.Id <= 0)
            {
                return "添加代理分销关联数据出错";
            }

            TransactionModel tran = new TransactionModel();

            agentmodel = new Agentinfo();
            agentmodel.addtime = DateTime.Now;
            agentmodel.updateitme = DateTime.Now;
            agentmodel.useraccountid = accountid;
            agentmodel.userLevel = 0;
            agentmodel.name = username;
            agentmodel.state = -1;
            agentmodel.IsOpenDistribution = 1;

            tran.Add(AgentinfoBLL.SingleModel.BuildAddSql(agentmodel));
            tran.Add($"update AgentDistributionRelation set ParentAgentId={pagentmodel.id},AgentId=(select last_insert_id()) where id={relation.Id}");
            tran.Add($"update agentinfo set AuthCode=CONCAT({agentmodel.addtime.ToString("yyyyMMdd")},(select last_insert_id())) where id=(select last_insert_id())");
            if (!base.ExecuteTransactionDataCorect(tran.sqlArray))
            {
                return "保存出错";
            }

            //清除缓存
            RemoveCache(pagentmodel.id);
            AgentQrCodeBLL.SingleModel.RemoveCache(pagentmodel.id);
            return "";
        }

        public int GetCountByAgentId(int agentid)
        {
            string sqlwhere = $"agentid={agentid}";
            return base.GetCount(sqlwhere);
        }

        public List<AgentDistributionRelation> GetListByQrcodeIds(string qrcodeids)
        {
            return base.GetList($"qrcodeid in ({qrcodeids})");
        }

        public List<DistributionUserInfo> GetDistributionUserInfo(int did)
        {
            
            List<DistributionUserInfo> list = new List<DistributionUserInfo>();
            string sql = $@"select ad.id,ad.parentagentid,ar.Percentage,ar.id ruleid, ar.name rulename,ar.nexpercentageid from(select * from AgentDistributionRelation where id = {did} or agentid = (select parentagentid from AgentDistributionRelation where id = {did})) ad
      left join agentinfo a on a.id = ad.parentagentid
left join agentdistributionrule ar on ar.id = a.agentdistributionruleid";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    DistributionUserInfo duserinfo = new DistributionUserInfo();
                    if (dr["parentagentid"] != DBNull.Value)
                    {
                        duserinfo.AgentId = Convert.ToInt32(dr["parentagentid"]);
                    }
                    if (dr["Percentage"] != DBNull.Value)
                    {
                        duserinfo.Percentage = Convert.ToDouble(dr["Percentage"]);
                    }
                    if (dr["ruleid"] != DBNull.Value)
                    {
                        duserinfo.RuleId = Convert.ToInt32(dr["ruleid"]);
                    }
                    if (dr["id"] != DBNull.Value)
                    {
                        duserinfo.Id = Convert.ToInt32(dr["id"]);
                    }
                    if (dr["nexpercentageid"] != DBNull.Value)
                    {
                        duserinfo.NexPercentageId = Convert.ToInt32(dr["nexpercentageid"]);
                        if (did != duserinfo.Id)
                        {
                            if (duserinfo.NexPercentageId > 0)
                            {
                                AgentDistributionRule agentrule = AgentDistributionRuleBLL.SingleModel.GetModel(duserinfo.NexPercentageId);
                                if (agentrule != null)
                                {
                                    duserinfo.Percentage = agentrule.Percentage;
                                }
                            }
                            else
                            {
                                duserinfo.Percentage = 0;
                            }
                        }
                    }
                    duserinfo.RuleName = dr["rulename"].ToString();
                    list.Add(duserinfo);
                }
            }

            return list;
        }

        public List<AgentDistributionRelation> GetAgentDistributionRelationList(string starttime, string endtime, string loginid, int agentqrcodeid, int agentid, int pageIndex, int pageSize, ref int count, bool reflesh = false)
        {
            string accountids = "";
            if (!string.IsNullOrEmpty(loginid))
            {
                Account accountlist = AccountBLL.SingleModel.GetModelByLoginid(loginid);
                if (accountlist == null)
                {
                    return new List<AgentDistributionRelation>();
                }
                accountids = "'" + accountlist.Id + "'";
                reflesh = true;
            }
            if (!string.IsNullOrEmpty(starttime) || !string.IsNullOrEmpty(endtime))
            {
                reflesh = true;
            }

            RedisModel<AgentDistributionRelation> model = new RedisModel<AgentDistributionRelation>();
            model = RedisUtil.Get<RedisModel<AgentDistributionRelation>>(string.Format(_redis_AgentDistributionRelationKey, agentid, agentqrcodeid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_AgentDistributionRelationVersion, agentid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<AgentDistributionRelation>();
                
                List<AgentDistributionRelation> list = new List<AgentDistributionRelation>();

                string sqlcount = $" select Count(*) from AgentDistributionRelation ad left join agentinfo a on ad.agentid = a.id ";
                string sql = $" select ad.*,a.useraccountid,a.LastDeposit,a.name agentname from AgentDistributionRelation ad left join agentinfo a on ad.agentid = a.id ";
                string sqlwhere = $" where ad.state>=0 and ad.ParentAgentId = {agentid} and qrcodeid={agentqrcodeid}";
                if (accountids.Length > 0)
                {
                    sqlwhere += $" and useraccountid in ({accountids}) ";
                }
                if (!string.IsNullOrEmpty(starttime))
                {
                    sqlwhere += $" and ad.addtime>='{starttime}' ";
                }
                if (!string.IsNullOrEmpty(endtime))
                {
                    sqlwhere += $" and ad.addtime<='{endtime} 23:59' ";
                }

                string sqllimit = $" ORDER BY ad.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit, null))
                {
                    while (dr.Read())
                    {
                        AgentDistributionRelation amodel = base.GetModel(dr);
                        amodel.UserAccountId = dr["useraccountid"].ToString();
                        amodel.UserName = dr["agentname"].ToString();
                        if (dr["LastDeposit"] != DBNull.Value)
                        {
                            amodel.LastDeposit = Convert.ToInt32(dr["LastDeposit"]);
                        }
                        AgentFollowLog agentlogmodel = AgentFollowLogBLL.SingleModel.GetLastModel(amodel.Id);
                        if (agentlogmodel != null)
                        {
                            amodel.Desc = agentlogmodel.Desc;
                        }
                        list.Add(amodel);
                    }
                }

                if (list == null || list.Count <= 0)
                {
                    return new List<AgentDistributionRelation>();
                }

                //查找来源
                string qrcodeid = string.Join(",", list.Select(s => s.QrCodeId).Distinct());
                List<AgentCustomerRelation> customerRelationlist = AgentCustomerRelationBLL.SingleModel.GetListByQrCodeId(qrcodeid);
                string cruserids = "'" + string.Join("','", customerRelationlist.Select(s => s.useraccountid)) + "'";
                string userids = "'" + string.Join("','", list.Select(s => s.UserAccountId)) + "'";
                if (cruserids.Length > 0)
                {
                    userids = userids + "," + cruserids;
                }
                List<Account> accountlist = AccountBLL.SingleModel.GetListByAccountids(userids);
                foreach (AgentDistributionRelation item in list)
                {
                    Account aitem = accountlist?.Where(w => w.Id.ToString() == item.UserAccountId).FirstOrDefault();
                    item.LoginId = aitem?.LoginId;
                    item.Phone = aitem?.ConsigneePhone;
                    AgentCustomerRelation crmodel = customerRelationlist?.Where(w => w.QrcodeId == item.QrCodeId).FirstOrDefault();
                    Account critem = accountlist?.Where(w => w.Id.ToString() == crmodel?.useraccountid).FirstOrDefault();
                    item.SourceFrom = crmodel?.username + "/" + (string.IsNullOrEmpty(critem?.ConsigneePhone) ? "未绑定手机号" : critem?.ConsigneePhone);
                }

                count = base.GetCountBySql(sqlcount + sqlwhere);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                if (!reflesh)
                {
                    RedisUtil.Set<RedisModel<AgentDistributionRelation>>(string.Format(_redis_AgentDistributionRelationKey, agentid, agentqrcodeid, pageSize, pageIndex), model);
                }
            }
            else
            {
                count = model.Count;
            }

            return model.DataList;
        }

        public List<AgentDistributionRelation> GetCustomerDistributionRelationList(int openstate, int opentype, string starttime, string endtime, string agentname, string loginid, string qrcodeids, int pageIndex, int pageSize, ref int count)
        {
            List<AgentDistributionRelation> list = new List<AgentDistributionRelation>();
            if (string.IsNullOrEmpty(qrcodeids))
            {
                return list;
            }
            
            string accountids = "";
            #region 模糊查找登陆账号
            if (!string.IsNullOrEmpty(loginid))
            {
                List<AgentDistributionRelation> agentRelationList = GetListByQrcodeIds(qrcodeids);
                if (agentRelationList == null || agentRelationList.Count <= 0)
                {
                    return list;
                }

                string agentids = string.Join(",", agentRelationList.Select(s => s.AgentId).Distinct());
                List<Agentinfo> agentlist = AgentinfoBLL.SingleModel.GetListByIds(agentids);
                if (agentlist == null || agentlist.Count <= 0)
                {
                    return list;
                }

                string useraccountids = "'" + string.Join("','", agentlist.Select(s => s.useraccountid).Distinct()) + "'";
                List<Account> agentaccountlist = AccountBLL.SingleModel.GetListByAccountids(useraccountids, loginid);
                if (agentaccountlist == null)
                {
                    return list;
                }
                accountids = "'" + string.Join("','", agentaccountlist.Select(s => s.Id).Distinct()) + "'";
            }
            #endregion

            string sqlcount = $" select Count(*) from AgentDistributionRelation ad left join agentinfo a on ad.agentid = a.id ";
            string sql = $" select ad.*,a.useraccountid,a.LastDeposit,a.name agentname from AgentDistributionRelation ad left join agentinfo a on ad.agentid = a.id ";
            string sqlwhere = $" where ad.state>=0 and ad.qrcodeid in ({qrcodeids}) and ad.agentId>0";
            if (!string.IsNullOrEmpty(agentname))
            {
                sqlwhere += $" and a.name like '%{agentname}%' ";
            }
            if (accountids.Length > 0)
            {
                sqlwhere += $" and useraccountid in ({accountids}) ";
            }
            if (!string.IsNullOrEmpty(starttime))
            {
                sqlwhere += $" and ad.addtime>='{starttime}' ";
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                sqlwhere += $" and ad.addtime<='{endtime} 23:59' ";
            }
            if (openstate != 999)
            {
                if (openstate == 0)
                {
                    sqlwhere += $" and a.LastDeposit<=0";
                }
                else
                {
                    sqlwhere += $" and a.LastDeposit>0";
                }
            }
            if (opentype != 999)
            {
                sqlwhere += $" and ad.OpenType={opentype}";
            }
            string sqllimit = $" ORDER BY ad.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere + sqllimit, null))
            {
                while (dr.Read())
                {
                    AgentDistributionRelation amodel = base.GetModel(dr);
                    amodel.UserAccountId = dr["useraccountid"].ToString();
                    amodel.UserName = dr["agentname"].ToString();
                    if (dr["LastDeposit"] != DBNull.Value)
                    {
                        amodel.LastDeposit = Convert.ToInt32(dr["LastDeposit"]);
                    }
                    //申请代理
                    if (amodel.OpenType == 0)
                    {
                        amodel.OpenOrder = AgentdepositLogBLL.SingleModel.GetOpenOrderCount("", amodel.AgentId, 1);
                    }
                    else if (amodel.OpenType == 1)//申请开通小程序
                    {
                        amodel.OpenOrder = AgentdepositLogBLL.SingleModel.GetOpenOrderCount(amodel.UserAccountId);
                    }
                    list.Add(amodel);
                }
            }

            if (list == null || list.Count <= 0)
            {
                return new List<AgentDistributionRelation>();
            }

            string userids = "'" + string.Join("','", list.Select(s => s.UserAccountId)) + "'";
            List<Account> accountlist = AccountBLL.SingleModel.GetListByAccountids(userids);
            foreach (AgentDistributionRelation item in list)
            {
                Account aitem = accountlist?.Where(w => w.Id.ToString() == item.UserAccountId).FirstOrDefault();
                item.LoginId = aitem?.LoginId;
                item.Phone = aitem?.ConsigneePhone;
            }
            
            count = base.GetCountBySql(sqlcount + sqlwhere);
            return list;
        }

        public List<AgentDistributionRelation> GetListByAgent(string accountids)
        {
            List<AgentDistributionRelation> list = new List<AgentDistributionRelation>();
            if (string.IsNullOrEmpty(accountids))
                return list;

            string sql = $"select ad.*,a.useraccountid from AgentDistributionRelation ad left join agentinfo a on ad.agentid = a.id ";
            string sqlwhere = $" where a.useraccountid in ({accountids})";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql + sqlwhere, null))
            {
                while (dr.Read())
                {
                    AgentDistributionRelation amodel = base.GetModel(dr);
                    amodel.UserAccountId = dr["useraccountid"].ToString();
                    list.Add(amodel);
                }
            }

            return list;
        }

        /// <summary>
        /// 申请代理的推广数据超过30天自动提交给小未跟进
        /// </summary>
        /// <param name="timelength"></param>
        public void StartAgentServiceCommand(int timelength = -30, int pageIndex = 1, int pageSize = 100)
        {
            string sqlwhere = $"State=1 and FollowState=0 and OpenType=0 and AddTime <= (NOW()+INTERVAL {timelength} DAY)";
            List<AgentDistributionRelation> agentDistributinlist = GetList(sqlwhere, pageSize, pageIndex);
            if (agentDistributinlist == null || agentDistributinlist.Count <= 0)
                return;

            TransactionModel tran = new TransactionModel();
            foreach (AgentDistributionRelation item in agentDistributinlist)
            {
                item.FollowState = (int)AgentDistributionFollowState.小未跟进;
                item.UpdateTime = DateTime.Now;
                item.SubmitTime = DateTime.Now;
                tran.Add(base.BuildUpdateSql(item, "UpdateTime,SubmitTime,FollowState"));
            }

            if (!base.ExecuteTransaction(tran.sqlArray))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "请代理的推广数据超过30天自动提交给小未跟进执行sql失败");
            }
        }
        /// <summary>
        /// 申请开发小程序的推广数据超过60天自动提交给小未跟进
        /// </summary>
        /// <param name="timelength"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public void StartCustomerServiceCommand(int timelength = -60, int pageIndex = 1, int pageSize = 100)
        {
            List<AgentDistributionRelation> agentDistributinlist = new List<AgentDistributionRelation>();
            string sql = $@"select * from (select d.*,(select count(*) from agentdepositlog l where l.customerid = a.useraccountid and l.type in (2,4,6,7,8)) openorder from AgentDistributionRelation d 
                left join agentinfo a on d.agentid = a.id
                where d.FollowState = 0 and d.OpenType = 0  and d.State = 1 ) r
                     where openorder <= 0
                 and AddTime <= (NOW() + INTERVAL {timelength} DAY) LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    AgentDistributionRelation amodel = base.GetModel(dr);
                    agentDistributinlist.Add(amodel);
                }
            }

            if (agentDistributinlist.Count <= 0)
                return;

            TransactionModel tran = new TransactionModel();
            foreach (AgentDistributionRelation item in agentDistributinlist)
            {
                item.FollowState = (int)AgentDistributionFollowState.小未跟进;
                item.UpdateTime = DateTime.Now;
                item.SubmitTime = DateTime.Now;
                tran.Add(base.BuildUpdateSql(item, "UpdateTime,SubmitTime,FollowState"));
            }

            if (!base.ExecuteTransaction(tran.sqlArray))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "请代理的推广数据超过60天自动提交给小未跟进执行sql失败");
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="agentid"></param>
        public void RemoveCache(int agentid)
        {
            if (agentid > 0)
            {
                RedisUtil.SetVersion(string.Format(_redis_AgentDistributionRelationVersion, agentid));
            }
        }

        #region 小未平台
        public List<AgentDistributionRelation> GetSysnDataList(string dname, string aname, int agentid, int opensyncdata, int pageSize, int pageIndex, ref int count)
        {
            
            List<MySqlParameter> parms = new List<MySqlParameter>();
            List<AgentDistributionRelation> list = new List<AgentDistributionRelation>();
            string sql = $@"select {"{0}"} from (
                        select * from AgentDistributionRelation where  parentagentid = {agentid} or parentagentid in (select agentid from AgentDistributionRelation where  parentagentid = {agentid})
                        ) ad left join agentinfo a on ad.agentid = a.id";
            string sqllist = string.Format(sql, "ad.*,a.state agentstate,a.name agentname");
            string sqlcount = string.Format(sql, "count(*)");
            string sqlwhere = $" where a.state = 1 ";
            string sqllimit = $" ORDER BY ad.addtime desc LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";
            //同步状态
            if (opensyncdata > -1)
            {
                sqlwhere += $" and ad.opensyncdata ={opensyncdata} ";
            }
            //分销商名称
            if (!string.IsNullOrEmpty(dname))
            {
                sqlwhere += $" and a.name like @name";
                parms.Add(new MySqlParameter("@name", $"%{dname}%"));
            }
            //所属上级
            if (!string.IsNullOrEmpty(aname))
            {
                List<Agentinfo> agentlist = AgentinfoBLL.SingleModel.GetListByName(aname, agentid);
                if (agentlist == null || agentlist.Count <= 0)
                    return list;
                string sqlagentids = string.Join(",",agentlist.Select(s=>s.id).Distinct());
                sqlwhere += $" and ad.parentagentid in ({sqlagentids}) ";
            }
            count = base.GetCountBySql(sqlcount+ sqlwhere, parms.ToArray());
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqllist + sqlwhere + sqllimit, parms.ToArray()))
            {
                while (dr.Read())
                {
                    AgentDistributionRelation amodel = base.GetModel(dr);
                    amodel.UserName = dr["agentname"].ToString();
                    list.Add(amodel);
                }
            }

            if (list == null || list.Count <= 0)
                return list;

            //所属上级名称
            string pagentids = string.Join(",", list.Select(s => s.ParentAgentId).Distinct());
            string cagentids = string.Join(",", list.Select(s => s.AgentId).Distinct());
            string agentids = pagentids + "," + cagentids;

            List<Agentinfo> pagentlist = AgentinfoBLL.SingleModel.GetListByIds(agentids);
            string accountids = "'" + string.Join("','", pagentlist?.Select(s => s.useraccountid).Distinct()) + "'";
            List<XcxAppAccountRelation> xcxrelationlist = XcxAppAccountRelationBLL.SingleModel.GetListByTidAccountId(accountids, (int)TmpType.小未平台);

            string aids = string.Join(",", xcxrelationlist?.Select(s => s.Id).Distinct());
            List<PlatStore> storelist = PlatStoreBLL.SingleModel.GetListByBindAids(aids);
            string storexcxaids = string.Join(",",storelist.Select(s=>s.Aid).Distinct());
            List<XcxAppAccountRelation> storexcxlist = XcxAppAccountRelationBLL.SingleModel.GetListByIds(storexcxaids);
            //List<PlatStatistics> statislist = new PlatStatisticsBLL().GetCountList(aids);

            foreach (AgentDistributionRelation item in list)
            {
                //所属上级名称
                Agentinfo tempagentinfo = pagentlist?.Where(w => w.id == item.ParentAgentId).FirstOrDefault();
                item.ParentName = tempagentinfo?.name;

                Agentinfo cagentinfo = pagentlist?.Where(w => w.id == item.AgentId).FirstOrDefault();
                XcxAppAccountRelation xcxrelation = xcxrelationlist.Where(w => w.AccountId.ToString() == cagentinfo?.useraccountid).FirstOrDefault();

                //平台访问量
                //PlatStatistics statismodel = statislist?.Where(w => w.AId == xcxrelation?.Id).FirstOrDefault();
                //item.PlatViewCount = statismodel!=null?statismodel.Count:0;
                item.PlatViewCount = PlatStatisticalFlowBLL.SingleModel.GetPVCount(xcxrelation?.Id);
                //店铺总数
                int? sum = storelist?.Where(s => s.BindPlatAid == xcxrelation?.Id).ToList().Count();
                item.StoreCount = sum.HasValue ? sum.Value : 0;
                //屏蔽店铺总数
                int? closesum = storelist?.Where(s => s.BindPlatAid == xcxrelation?.Id && s.State>0).ToList().Count();
                item.CloseSyncStoreCount = closesum.HasValue ? closesum.Value : 0;
                //小程序总数
                int? appsum = storexcxlist?.Where(s => !string.IsNullOrEmpty(s.AppId)).ToList().Count();
                item.AppCount = appsum.HasValue ? appsum.Value : 0;
            }

            return list;
        }
        #endregion
    }
}
