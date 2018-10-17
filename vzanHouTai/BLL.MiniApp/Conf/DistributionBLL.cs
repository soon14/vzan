using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp.Conf
{
    public class DistributionBLL : BaseMySql<Distribution>
    {
        #region 单例模式
        private static DistributionBLL _singleModel;
        private static readonly object SynObject = new object();

        private DistributionBLL()
        {

        }

        public static DistributionBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DistributionBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public Distribution GetModelByAgentId(int agentId)
        {
            return base.GetModel($"agentId={agentId}");
        }

        public List<Distribution> GetListByAgentId(int agentid)
        {
            return base.GetList($"parentAgentId={agentid}");
        }

        public Distribution GetModelByAgentIdAndParentId(int agentId, int parentId)
        {
            return base.GetModel($"agentId={agentId} and parentagentid={parentId}");
        }

        /// <summary>
        /// 是否存在分销商用户
        /// </summary>
        /// <returns></returns>
        public bool ExiteDistribution(int agentid)
        {
            return base.Exists($"parentAgentId = {agentid}");
        }

        public List<DistributionModel> GetDistributionList(int parentAgentId, string loginId, string username, int state, int pagesize, int pageindex, out int count ,out string errormsg)
        {
            errormsg = string.Empty;
            
            
            string sqlwhere = $"parentAgentId={parentAgentId}";
            count = -1;
            Account useraccount = null;
            if (!string.IsNullOrEmpty(loginId))
            {
                useraccount = AccountBLL.SingleModel.GetModel($" loginId='{loginId}'");

                if (useraccount == null) return null;
                //accountIds = $"and useraccountid='{useraccount.Id.ToString()}'";
                sqlwhere += $" and useraccountid='{useraccount.Id}'";
            }
            if (!string.IsNullOrEmpty(username))
            {
                sqlwhere += $" and name like '%{username}%'";
            }
            if (state > -2)
            {
                sqlwhere += $" and state ={state}";
            }
            errormsg = sqlwhere;
            List<DistributionModel> modellist = null;
            List<Distribution> list = base.GetList(sqlwhere, pagesize, pageindex, "*", "addtime desc");
            count = base.GetCount(sqlwhere);
            if(list!=null && list.Count > 0)
            {
                modellist = new List<DistributionModel>();
                string agentIds = string.Join(",",list.Select(s=>s.AgentId).Distinct());
                List<Agentinfo> agentInfoList = AgentinfoBLL.SingleModel.GetListByIds(agentIds);

                string accountIds = $"'{string.Join("','",list.Select(s=>s.useraccountid).Distinct())}'";
                List<Account> accountList = AccountBLL.SingleModel.GetListByAccountids(accountIds);

                foreach(var distribution in list)
                {
                    DistributionModel model = new DistributionModel();
                    Agentinfo agentInfo = agentInfoList?.FirstOrDefault(f=>f.id == distribution.AgentId);
                    
                    if (agentInfo == null)
                    {
                        agentInfo = new Agentinfo();
                    }
                    Account account = accountList?.FirstOrDefault(f=>f.Id.ToString() == agentInfo.useraccountid);
                    if(account==null)
                    {
                        continue;
                    }
                    model.createCustomerCount = AgentCustomerRelationBLL.SingleModel.GetCount($"agentid ={distribution.AgentId}");
                    model.buyTemplateCount = XcxAppAccountRelationBLL.SingleModel.GetCount($"agentid={distribution.AgentId}");
                    model.addtime = distribution.addtime;
                    model.AgentId = distribution.AgentId;
                    model.deposit = agentInfo.deposit;
                    model.LoginId = account.LoginId;
                    model.modifyDate = distribution.modifyDate;
                    model.name = distribution.name;
                    model.parentAgentId = distribution.parentAgentId;
                    model.remark = distribution.remark;
                    model.state = distribution.state;
                    modellist.Add(model);
                }
            }
            return modellist;
        }
        
        public bool AddDistribution(Account account, int parentAgentId,int agenttype, string username, List<XcxTemplate> templateList,int deposit, string remark,ref string msg)
        {
            bool result = false;
            
            
            TransactionModel tranModel = new TransactionModel();
            MySqlParameter[] pone = null;

            //代理商扣费
            Agentinfo agentinfop = AgentinfoBLL.SingleModel.GetModel(parentAgentId);
            if(agentinfop==null)
            {
                msg = "代理商不存在";
                return false;
            }
            List<Distribution> dlist = GetListByAgentId(agentinfop.id);
            if(dlist!=null && dlist.Count>0)
            {
                string agentids = string.Join(",",dlist.Select(s=>s.AgentId));
                List<Agentinfo> cagentinfolist = AgentinfoBLL.SingleModel.GetListByIds(agentids);
                if(cagentinfolist!=null && cagentinfolist.Count>0)
                {
                    int cost = cagentinfolist.Sum(s => s.deposit)+deposit;
                    if (agentinfop.deposit<=cost && deposit>0)
                    {
                        msg = "代理商预存款不足";
                        return false;
                    }
                }
            }
            
            //tranModel.Add($"update Agentinfo set deposit={cost} where id={agentinfop.id}");
            //AgentdepositLog agentdepositLog = new AgentdepositLog();
            //agentdepositLog.agentid = agentinfop.id;
            //agentdepositLog.addtime = DateTime.Now;
            //agentdepositLog.beforeDeposit = agentinfop.deposit;
            //agentdepositLog.cost = deposit;
            //agentdepositLog.afterDeposit = cost;
            //agentdepositLog.type = 13;
            //agentdepositLog.customerid = agentinfop.useraccountid;
            //agentdepositLog.costdetail = $"为分销代理商{username}充值{(deposit*0.01).ToString("0.00")}";
            //tranModel.Add(_agentdepositLogBll.BuildAddSql(agentdepositLog, out pone), pone);

            //插入代理表
            Agentinfo agent = new Agentinfo();
            agent.state = 1;
            agent.deposit = deposit;
            agent.addtime = DateTime.Now;
            agent.updateitme = DateTime.Now;
            agent.useraccountid = account.Id.ToString();
            agent.userLevel = 1;
            agent.AgentType = agenttype;
            agent.id = Convert.ToInt32(AgentinfoBLL.SingleModel.Add(agent));
            if (agent.id <= 0)
            {
                return result;
            }
            tranModel.Add($"update Agentinfo set deposit={deposit} where id={agent.id}");

            AgentdepositLog agentdepositLog = new AgentdepositLog();
            agentdepositLog.agentid = agent.id;
            agentdepositLog.addtime = DateTime.Now;
            agentdepositLog.beforeDeposit = 0;
            agentdepositLog.cost = agent.deposit;
            agentdepositLog.afterDeposit= agentdepositLog.beforeDeposit+ agent.deposit;
            agentdepositLog.type = 1;
            tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentdepositLog, out pone), pone);
            //插入小程序模板与代理关系表 自定义价格
            foreach (var template in templateList)
            {
                Xcxtemplate_Price xcxtemplate_Price = Xcxtemplate_PriceBLL.SingleModel.GetModel($"tid={template.Id} and agentid={agent.id} and VersionId={template.VersionId}");
                if (xcxtemplate_Price == null)
                {
                    xcxtemplate_Price = new Xcxtemplate_Price();
                    xcxtemplate_Price.price = template.Price;
                    xcxtemplate_Price.tid = template.Id.ToString();
                    xcxtemplate_Price.agentid = agent.id.ToString();
                    xcxtemplate_Price.VersionId = template.VersionId;
                    if (template.Price<=0)
                    {
                        xcxtemplate_Price.LimitCount = template.LimitCount<=0?10: template.LimitCount;
                    }
                    
                    tranModel.Add(Xcxtemplate_PriceBLL.SingleModel.BuildAddSql(xcxtemplate_Price, out pone), pone);
                }
                else
                {
                    tranModel.Add($"update xcxtemplate_Price set price={template.Price} where tid={template.Id} and agentid={agent.id} and VersionId={template.VersionId}");
                }
            }
            //插入分销商表
            Distribution distribution = new Distribution();
            distribution.name = username;
            distribution.AgentId = agent.id;
            distribution.useraccountid = account.Id.ToString();
            distribution.parentAgentId = parentAgentId;
            distribution.state = 1;
            distribution.remark = remark;
            distribution.addtime = DateTime.Now;
            distribution.modifyDate = DateTime.Now;
            tranModel.Add(base.BuildAddSql(distribution, out pone), pone);

            try
            {
                result = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception )
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
            }
            if (!result)
            {
                agent.state = -2;
                AgentinfoBLL.SingleModel.Update(agent, "state");
            }
            return result;
        }
        
        public bool UpdateDistribution(Distribution distribution, int deposit, List<XcxTemplate> list, out string errormsg)
        {
            TransactionModel tranModel = new TransactionModel();
            
            
            MySqlParameter[] pone = null;
            bool result = false;
            errormsg = string.Empty;
            tranModel.Add($"update Distribution set name='{distribution.name}',remark ='{distribution.remark}' where id={distribution.id}");
            
            //分销商预存款
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModel(distribution.AgentId);
            if(agentinfo.deposit!=deposit)
            {
                //充值
                int cost = Math.Abs(deposit - agentinfo.deposit);
                //if (agentinfo == null)
                //{
                //    errormsg = "分销商代理数据不存在";
                //    return result;
                //}
                //if (cost <= 0)
                //{
                //    errormsg = "预存款充值不能小于0";
                //    return result;
                //}
                tranModel.Add($"update Agentinfo set deposit={deposit} where id={distribution.AgentId}");
                AgentdepositLog agentdepositLog = new AgentdepositLog();
                agentdepositLog.agentid = agentinfo.id;
                agentdepositLog.addtime = DateTime.Now;
                agentdepositLog.beforeDeposit = agentinfo.deposit;
                agentdepositLog.cost = cost;
                agentdepositLog.afterDeposit = deposit;
                agentdepositLog.type = 1;
                tranModel.Add(AgentdepositLogBLL.SingleModel.BuildAddSql(agentdepositLog, out pone), pone);
                //代理商预存款
                Agentinfo agentinfop = AgentinfoBLL.SingleModel.GetModel(distribution.parentAgentId);
                if (agentinfo == null)
                {
                    errormsg = "代理商数据不存在";
                    return result;
                }
                List<Distribution> dlist = GetListByAgentId(agentinfop.id);
                if (dlist != null && dlist.Count > 0)
                {
                    string agentids = string.Join(",", dlist.Select(s => s.AgentId));
                    List<Agentinfo> cagentinfolist = AgentinfoBLL.SingleModel.GetListByIds(agentids);
                    if (cagentinfolist != null && cagentinfolist.Count > 0)
                    {
                        List<Agentinfo> otheragent = cagentinfolist.Where(w => w.id != agentinfo.id).ToList();
                        int tempcost = deposit;
                        //其他分销代理的预存款
                        if (otheragent!=null && otheragent.Count>0)
                        {
                            tempcost += otheragent.Sum(s => s.deposit);
                        }
                        
                        if (agentinfop.deposit <= tempcost && deposit > 0)
                        {
                            errormsg = "代理商预存款不足";
                            return false;
                        }
                    }
                }
                //tranModel.Add($"update Agentinfo set deposit={agentinfop.deposit - cost} where id={agentinfop.id}");
                //agentdepositLog = new AgentdepositLog();
                //agentdepositLog.agentid = agentinfop.id;
                //agentdepositLog.addtime = DateTime.Now;
                //agentdepositLog.beforeDeposit = agentinfop.deposit;
                //agentdepositLog.cost = cost;
                //agentdepositLog.afterDeposit = agentinfop.deposit - cost;
                //agentdepositLog.type = 13;
                //agentdepositLog.customerid = agentinfop.useraccountid;
                //agentdepositLog.costdetail = $"为分销代理商{distribution.name}充值{(cost * 0.01).ToString("0.00")}";
                //tranModel.Add(_agentdepositLogBll.BuildAddSql(agentdepositLog, out pone), pone);
            }
            
            foreach (var template in list)
            {
                Xcxtemplate_Price xcxtemplatePrice = Xcxtemplate_PriceBLL.SingleModel.GetModel($"tid ={template.Id} and agentid={distribution.AgentId} and VersionId={template.VersionId}");
                if (xcxtemplatePrice == null)
                {
                    xcxtemplatePrice = new Xcxtemplate_Price();
                    xcxtemplatePrice.price = template.Price;
                    xcxtemplatePrice.agentid = distribution.AgentId.ToString();
                    xcxtemplatePrice.tid = template.Id.ToString();
                    xcxtemplatePrice.state = 0;
                    xcxtemplatePrice.VersionId = template.VersionId;
                    if (template.Type == (int)TmpType.小程序多门店模板 || template.Type == (int)TmpType.小程序餐饮多门店模板 || template.Type == (int)TmpType.智慧餐厅)
                    {
                        xcxtemplatePrice.SPrice = template.SPrice;
                        xcxtemplatePrice.SCount = template.SCount;
                    }
                    Xcxtemplate_PriceBLL.SingleModel.Add(xcxtemplatePrice);
                    continue;
                }
                tranModel.Add($"update xcxtemplate_Price set price={template.Price} {((template.Type == (int)TmpType.小程序多门店模板 || template.Type == (int)TmpType.小程序餐饮多门店模板 || template.Type == (int)TmpType.智慧餐厅) ? $",SPrice={template.SPrice},SCount={template.SCount}" : "")} where tid ={template.Id} and agentid={distribution.AgentId} and VersionId={template.VersionId}");
            }
            try
            {
                result = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception )
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
            }
            return result;
        }
        
        public bool UpdateState(Distribution distribution, Agentinfo dagentInfo)
        {
            TransactionModel tranModel = new TransactionModel();
            bool result = false;
            tranModel.Add($"update Distribution set state={distribution.state},modifyDate='{distribution.modifyDate}' where id={distribution.id}");
            tranModel.Add($"update Agentinfo set state={dagentInfo.state},updateitme='{dagentInfo.updateitme}' where id={dagentInfo.id}");
            try
            {
                result = base.ExecuteTransactionDataCorect(tranModel.sqlArray, tranModel.ParameterArray);
            }
            catch (Exception )
            {
                log4net.LogHelper.WriteInfo(this.GetType(), Newtonsoft.Json.JsonConvert.SerializeObject(tranModel.sqlArray));
            }
            return result;
        }
    }
}
