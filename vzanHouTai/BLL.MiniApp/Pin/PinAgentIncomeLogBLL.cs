using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
    public class PinAgentIncomeLogBLL : BaseMySql<PinAgentIncomeLog>
    {
        #region 单例模式
        private static PinAgentIncomeLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinAgentIncomeLogBLL()
        {

        }

        public static PinAgentIncomeLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinAgentIncomeLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 代理提成日志
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="pinOrder"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool AddAgentLog(PinAgent agent, PinGoodsOrder pinOrder, TransactionModel tran = null)
        {

            bool istran = tran != null;
            if (!istran) tran = new TransactionModel();

            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(agent.aId);
            if (platform == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception("平台信息错误"));
                return false;
            }

            #region 旧代码 没有代理商等级时候的逻辑
            //int Percent = platform.agentExtract;//上级比例
            //PinAgent grandfatherAgent = null;
            //int money = pinOrder.money * Percent;
            //if (agent.fuserId > 0)
            //{
            //    PinAgent fatherAgent = PinAgentBLL.SingleModel.GetModelByUserId(agent.fuserId);
            //    if (fatherAgent != null)
            //    {
            //        grandfatherAgent = PinAgentBLL.SingleModel.GetModelByUserId(fatherAgent.fuserId);
            //        if (grandfatherAgent != null && platform.FirstExtract > 0 && platform.SecondExtract > 0 && (platform.FirstExtract + platform.SecondExtract) <= 1000)
            //        {
            //            //表示有上上级 重新分配上级提成比例
            //            money = Convert.ToInt32(platform.FirstExtract * 0.001 * money);
            //        }
            //    }
            //}

            //PinAgentIncomeLog log = new PinAgentIncomeLog()
            //{
            //    aId = agent.aId,
            //    userId = agent.fuserId,
            //    sourceUid = agent.userId,
            //    income = money,
            //    source = 0
            //};
            //C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(agent.userId);
            //if (userInfo == null)
            //{
            //    log.remark = $"代理付费日志出错：找不到代理用户信息 userid:{agent.userId}";
            //    log4net.LogHelper.WriteError(GetType(), new Exception(log.remark));
            //    tran.Add(BuildAddSql(log));
            //    if (istran) return true;
            //    return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            //}
            //log.remark = $"{userInfo.NickName} 成为代理";

            //if (agent.fuserId > 0)
            //{
            //    PinAgent fagent = PinAgentBLL.SingleModel.GetModelByUserId(agent.fuserId);
            //    if (fagent == null)
            //    {
            //        log.remark = $"代理付费日志出错：找不到上级代理 userid:{agent.fuserId}";
            //        log4net.LogHelper.WriteError(GetType(), new Exception(log.remark));
            //        tran.Add(BuildAddSql(log));
            //        if (istran) return true;
            //        return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            //    }
            //    log.agentId = fagent.id;
            //    log.beforeMoney = fagent.cash;
            //    log.afterMoney = fagent.cash + money;
            //    C_UserInfo fuserInfo = C_UserInfoBLL.SingleModel.GetModel(agent.fuserId);
            //    if (fuserInfo == null)
            //    {
            //        log.remark = $"代理付费日志出错：找不到上级代理用户信息 userid:{agent.fuserId}";
            //        log4net.LogHelper.WriteError(GetType(), new Exception(log.remark));
            //        tran.Add(BuildAddSql(log));
            //        if (istran) return true;
            //        return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            //    }
            //    log.remark += $"，{fuserInfo.NickName}获得提成{log.incomeStr}元";
            //}
            //tran.Add(BuildAddSql(log));

            #region 上上级分提成日志
            //if (grandfatherAgent != null && platform.FirstExtract > 0 && platform.SecondExtract > 0 && (platform.FirstExtract + platform.SecondExtract) <= 1000)
            //{
            //    //表示有上上级 记录分给上上级提成日志

            //    int grandfatherAgentMoney = Convert.ToInt32(pinOrder.money * Percent * platform.SecondExtract * 0.001);
            //    PinAgentIncomeLog grandfatherAgentLog = new PinAgentIncomeLog()
            //    {
            //        aId = grandfatherAgent.aId,
            //        userId = grandfatherAgent.userId,
            //        sourceUid = agent.userId,
            //        income = grandfatherAgentMoney,
            //        source = 0,
            //        ExtractType = 1
            //    };

            //    grandfatherAgentLog.agentId = grandfatherAgent.id;
            //    grandfatherAgentLog.beforeMoney = grandfatherAgent.cash;
            //    grandfatherAgentLog.afterMoney = grandfatherAgent.cash + grandfatherAgentMoney;

            //    C_UserInfo gfuserInfo = C_UserInfoBLL.SingleModel.GetModel(grandfatherAgent.userId);
            //    if (gfuserInfo == null)
            //    {
            //        grandfatherAgentLog.remark = $"代理付费日志出错：找不到上上级代理用户信息 userid:{grandfatherAgent.userId}";
            //        log4net.LogHelper.WriteError(GetType(), new Exception(grandfatherAgentLog.remark));
            //        tran.Add(BuildAddSql(grandfatherAgentLog));
            //        if (istran) return true;
            //        return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            //    }
            //    grandfatherAgentLog.remark += $"，{gfuserInfo.NickName}获得提成{grandfatherAgentLog.incomeStr}元";
            //    grandfatherAgentLog.remark += $"{userInfo.NickName} 成为代理(下下级)";
            //    tran.Add(BuildAddSql(grandfatherAgentLog));

            //}

            #endregion
            #endregion


            PinAgentLevelConfig agentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(agent.AgentLevel, agent.aId);
            if (agentLevelConfig == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"代理提成log错误：当前入驻代理商{agent.id}等级找不到");
                return false;
            }

            PinAgent fagent = PinAgentBLL.SingleModel.GetModelByUserId(agent.fuserId);
            if (fagent == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"代理提成log错误：找不到上级代理信息 userid:{agent.fuserId}");
                return false;
            }

            //查找该代理上级所属等级 根据所属等级进行不同比例分成

            PinAgentLevelConfig fagentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(fagent.AgentLevel, fagent.aId);
            if (fagentLevelConfig == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"代理提log成错误：上级代理商{fagent.id}等级找不到");
                return false;
            }

            int percent = fagentLevelConfig.AgentExtract;
            if (agentLevelConfig.LevelId > fagentLevelConfig.LevelId)
            {
                //表示当前代理商级别高于所属上级代理商级别则按越级比例分配给上级
                percent = platform.JumpExtract;
            }

            int money = pinOrder.money * percent;

            PinAgentIncomeLog fAgengLog = new PinAgentIncomeLog()
            {
                aId = agent.aId,
                userId = agent.fuserId,
                sourceUid = agent.userId,
                income = money,
                source = 0,
                agentId = fagent.id,
                beforeMoney =fagent.cash,
                afterMoney = fagent.cash + money
            };
            C_UserInfo agentUserInfo = C_UserInfoBLL.SingleModel.GetModel(agent.userId);
            if (agentUserInfo == null)
            {
                fAgengLog.remark = $"代理付费日志出错：找不到代理用户信息 userid:{agent.userId}";
                log4net.LogHelper.WriteInfo(GetType(), fAgengLog.remark);
                tran.Add(BuildAddSql(fAgengLog));
                if (istran) return true;
                return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            }
            fAgengLog.remark = $"{agentUserInfo.NickName}成为{agentLevelConfig.LevelName}代理";

            C_UserInfo fuserInfo = C_UserInfoBLL.SingleModel.GetModel(agent.fuserId);
            if (fuserInfo == null)
            {
                fAgengLog.remark = $"代理付费日志出错：找不到上级代理用户信息 userid:{agent.fuserId}";
                log4net.LogHelper.WriteInfo(GetType(), fAgengLog.remark);
                tran.Add(BuildAddSql(fAgengLog));
                if (istran) return true;
                return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            }
            fAgengLog.remark += $"，{fuserInfo.NickName}获得提成{fAgengLog.incomeStr}元";
            tran.Add(BuildAddSql(fAgengLog));

            #region 上上级分提成日志
            PinAgent grandfatherAgent = PinAgentBLL.SingleModel.GetModelByUserId(fagent.fuserId);

            if (grandfatherAgent != null)
            {
                //表示有上上级 记录分给上上级提成日志

                PinAgentLevelConfig gfagentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(grandfatherAgent.AgentLevel, grandfatherAgent.aId);
                if (fagentLevelConfig == null || gfagentLevelConfig.SecondAgentExtract <= 0 || (gfagentLevelConfig.AgentExtract + gfagentLevelConfig.SecondAgentExtract) > 1000)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"上上级代理提成错误：上上级代理商{grandfatherAgent.id}等级找不到或者比例不能分成");
                    return istran;
                }

                int grandfatherAgentMoney = pinOrder.money * gfagentLevelConfig.SecondAgentExtract;
                PinAgentIncomeLog grandfatherAgentLog = new PinAgentIncomeLog()
                {
                    aId = grandfatherAgent.aId,
                    userId = grandfatherAgent.userId,
                    sourceUid = agent.userId,
                    income = grandfatherAgentMoney,
                    source = 0,
                    ExtractType = 1,
                    agentId = grandfatherAgent.id,
                    beforeMoney = grandfatherAgent.cash,
                    afterMoney = grandfatherAgent.cash + grandfatherAgentMoney

                };


                C_UserInfo gfuserInfo = C_UserInfoBLL.SingleModel.GetModel(grandfatherAgent.userId);
                if (gfuserInfo == null)
                {
                    grandfatherAgentLog.remark = $"代理付费日志出错：找不到上上级代理用户信息 userid:{grandfatherAgent.userId}";
                    log4net.LogHelper.WriteError(GetType(), new Exception(grandfatherAgentLog.remark));
                    tran.Add(BuildAddSql(grandfatherAgentLog));
                    if (istran) return true;
                    return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
                }
                grandfatherAgentLog.remark += $"，{gfuserInfo.NickName}获得提成{grandfatherAgentLog.incomeStr}元";
                grandfatherAgentLog.remark += $"{fuserInfo.NickName} 成为代理(下下级)";
                tran.Add(BuildAddSql(grandfatherAgentLog));

            }

            #endregion 


            if (istran) return true;
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }
        /// <summary>
        /// 订单提成日志
        /// </summary>
        /// <param name="store"></param>
        /// <param name="pinOrder"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool AddAgentLog(PinStore store, int money, PinAgent agent, TransactionModel tran = null, string remark = "",int second=0)
        {
            bool istran = tran != null;
            if (!istran)
                tran = new TransactionModel();
            PinAgentIncomeLog log = new PinAgentIncomeLog()
            {
                aId = store.aId,
                agentId = agent.id,//store.agentId,
                sourceUid = store.userId,
                income = money,
                source = 1
            };

            log.remark = $"店铺：{store.storeName} 订单提现,{remark}";

            log.beforeMoney = agent.cash;
            log.afterMoney = agent.cash + money;
            C_UserInfo fuserInfo = C_UserInfoBLL.SingleModel.GetModel(agent.userId);
            if (fuserInfo == null)
            {
                log.remark = $"代理付费日志出错：找不到代理用户信息 userid:{agent.userId}";
                log4net.LogHelper.WriteError(GetType(), new Exception(log.remark));
                tran.Add(BuildAddSql(log));
                if (istran)
                    return true;
                return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            }
            string s = second == 1 ? "来自下下级" : string.Empty;
            log.remark += $"，{fuserInfo.NickName}获得提成{s}{log.incomeStr}元";
            tran.Add(BuildAddSql(log));

            if (istran)
                return true;
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public List<PinAgentIncomeLog> GetListByAgentId_type(int agentId, int source, int pageIndex, int pageSize, out int count)
        {
            count = 0;
            string sqlwhere = $" agentId={agentId} and source={source}";
            List<PinAgentIncomeLog> list = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            if (list != null && list.Count > 0)
            {
                string userIds = string.Join(",", list.Select(s => s.userId));
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                list.ForEach(log =>
                {
                    log.userInfo = userInfoList?.FirstOrDefault(f=>f.Id == log.sourceUid);
                });
            }
            count = GetCount(sqlwhere);
            return list;
        }
        
        public string GetIncomeSum(int agentId, int source, int extractType = 0)
        {
            string sql = $"select sum(income) from pinagentincomelog where source={source} and agentid={agentId} and ExtractType={extractType}";
            object result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            int sum = result == DBNull.Value ? 0 : Convert.ToInt32(result);
            return (sum * 0.001 * 0.01).ToString("0.00");
        }
    }
}
