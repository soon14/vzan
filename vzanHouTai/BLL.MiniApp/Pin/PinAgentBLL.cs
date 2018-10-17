using BLL.MiniApp.Ent;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entity.MiniApp.Pin.PinEnums;

namespace BLL.MiniApp.Pin
{
    public class PinAgentBLL : BaseMySql<PinAgent>
    {
        #region 单例模式
        private static PinAgentBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinAgentBLL()
        {

        }

        public static PinAgentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinAgentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PinAgent GetModelByUserId(int userId)
        {
            PinAgent agent = null;
            if (userId <= 0)
            {
                return agent;
            }
            string sqlwhere = $" userid={userId} and state=1";
            agent = GetModel(sqlwhere);

            if (agent!=null&&agent.IsExpired)
            {
                //表示已经过期
                return null;
            }
            return agent;
        }
        public PinAgent GetModelByUserId(long userId)
        {
            PinAgent agent = null;
            if (userId <= 0)
            {
                return agent;
            }
            string sqlwhere = $" userid={userId} and state=1";
            agent = GetModel(sqlwhere);
            return agent;
        }
        /// <summary>
        /// 代理提成
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="pinOrder"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public bool UpdateIncome(PinAgent agent, PinGoodsOrder pinOrder, TransactionModel tran = null)
        {
            bool istran = tran != null;
            if (!istran) tran = new TransactionModel();

            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(agent.aId);
            if (platform == null)
            {
                log4net.LogHelper.WriteError(GetType(), new Exception($"代理提成错误：找不到平台信息信息 userid:{agent.fuserId}"));
                return istran;
            }


            PinAgentLevelConfig agentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(agent.AgentLevel, agent.aId);
            if (agentLevelConfig == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"代理提成错误：当前入驻代理商{agent.id}等级找不到");
                return istran;
            }

            PinAgent fagent = GetModelByUserId(agent.fuserId);
            if (fagent == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"代理提成错误：找不到上级代理信息 userid:{agent.fuserId}");
                return istran;
            }

            //查找该代理上级所属等级 根据所属等级进行不同比例分成

            PinAgentLevelConfig fagentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(fagent.AgentLevel, fagent.aId);
            if (fagentLevelConfig == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"代理提成错误：上级代理商{fagent.id}等级找不到");
                return istran;
            }
            int percent = fagentLevelConfig.AgentExtract;
            if (agentLevelConfig.LevelId > fagentLevelConfig.LevelId)
            {
                //表示当前代理商级别高于所属上级代理商级别则按越级比例分配给上级
                percent = platform.JumpExtract;
            }

        

            //上级代理获得提成
            fagent.cash += pinOrder.money * percent;
            fagent.money += pinOrder.money * percent;
            tran.Add(BuildUpdateSql(fagent, "cash,money"));


            #region 查找上级代理商是否有上级，如果有上级则上级也要获得抽成
            PinAgent grandfatherAgent = GetModelByUserId(fagent.fuserId);
            if (grandfatherAgent != null)
            {
                //上上级
                PinAgentLevelConfig gfagentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(grandfatherAgent.AgentLevel, grandfatherAgent.aId);
                if (fagentLevelConfig == null || gfagentLevelConfig.SecondAgentExtract <= 0 || (gfagentLevelConfig.AgentExtract + gfagentLevelConfig.SecondAgentExtract) > 1000)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"上上级代理提成错误：上上级代理商{grandfatherAgent.id}等级找不到或者比例不能分成");
                    return istran;
                }

                grandfatherAgent.cash += pinOrder.money * gfagentLevelConfig.SecondAgentExtract;
                grandfatherAgent.money += pinOrder.money * gfagentLevelConfig.SecondAgentExtract;
                tran.Add(BuildUpdateSql(grandfatherAgent, "cash,money"));
            } 
            #endregion



            if (istran) return true;
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        /// <summary>
        /// 代理升级
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="money"></param>
        /// <param name="oldAgentLevel"></param>
        /// <returns></returns>
        public bool UpdateAgentLevel(PinAgent agent, int money, int oldAgentLevel, DateTime oldAddTime)
        {
            if (base.Update(agent, "AgentLevel,addTime"))
            {
                TransactionModel tran = new TransactionModel();
                PinAgentChangeLog pinAgentChangeLog = new PinAgentChangeLog()
                {
                    Aid = agent.aId,
                    AgentId = agent.id,
                    ChangeMoney = money,
                    Remark = $"代理商升级由之前的{oldAgentLevel}等级升级为{agent.AgentLevel},花费{money * 0.01}元",
                    ChangeType = 1,
                    AddTime = DateTime.Now

                };
                tran.Add(PinAgentChangeLogBLL.SingleModel.BuildAddSql(pinAgentChangeLog));
                PinGoodsOrder pinOrder = new PinGoodsOrder() { money = money };
                UpdateIncome(agent, pinOrder, tran);
                PinAgentIncomeLogBLL.SingleModel.AddAgentLog(agent, pinOrder, tran);//插入提成日志
                if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                {
                    agent.addTime = oldAddTime;
                    agent.AgentLevel = oldAgentLevel;
                    base.Update(agent, "AgentLevel,addTime");
                    return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 代理续费
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public bool AddAgentTime(PinAgent agent, int money, int timeLength)
        {
            TransactionModel tran = new TransactionModel();
            PinAgentChangeLog pinAgentChangeLog = new PinAgentChangeLog()
            {
                Aid = agent.aId,
                AgentId = agent.id,
                ChangeMoney = money,
                Remark = $"代理商续费{timeLength}年,花费{money * 0.01}元",
                ChangeType = 0,
                AddTime = DateTime.Now

            };
            tran.Add(PinAgentChangeLogBLL.SingleModel.BuildAddSql(pinAgentChangeLog));

            if (agent.IsExpired)
            {
                //如果过期了则当前时间为入驻时间
                agent.addTime = DateTime.Now;
                agent.AgentTime = timeLength;
                tran.Add(base.BuildUpdateSql(agent, "addTime,AgentTime"));
            }
            else
            {
                agent.AgentTime += timeLength;
                tran.Add(base.BuildUpdateSql(agent, "AgentTime"));
            }



            PinGoodsOrder pinOrder = new PinGoodsOrder() { money = money };
            UpdateIncome(agent, pinOrder, tran);
            PinAgentIncomeLogBLL.SingleModel.AddAgentLog(agent, pinOrder, tran);//插入提成日志
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }




        public C_UserInfo GetUserInfoByAgentId(int agentId)
        {
            string sqlwhere = $"id=(select userId from pinagent where id={agentId})";
            return C_UserInfoBLL.SingleModel.GetModel(sqlwhere);

        }

        /// <summary>
        /// 获取代理申请入驻费用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetAgentFee(int agentId)
        {
            string fee = "0.00";
            string sql = $"select money from pingoodsorder where orderType=1 and payState=1 and goodsid={agentId}";
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            int addMoney = result != DBNull.Value ? Convert.ToInt32(result) : 0;//小程序端入驻的费用

            sql = $"select sum(changemoney) from PinAgentChangeLog where agentid={agentId}";//查出后台续费 升级的花费
             result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            addMoney += result != DBNull.Value ? Convert.ToInt32(result) : 0;

            fee = (addMoney * 0.01).ToString("0.00");
            return fee;
        }

        public List<PinAgent> GetListByAid_State(string appid, int aid, int pageSize, int pageIndex, out int recordCount, string storeName = "", string phone = "", string nickName = "", string fnickName = "", string fphone = "", int state = 1)
        {
            List<PinAgent> list = new List<PinAgent>();
            recordCount = 0;
            string fsqlwhere = $"appid='{appid}'";
            List<MySqlParameter> paras = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(fnickName))
            {
                fsqlwhere += " and nickname like @nickname";
                paras.Add(new MySqlParameter("@nickname", $"%{fnickName}%"));
            }
            if (!string.IsNullOrEmpty(fphone))
            {
                fsqlwhere += " and telephone like @telephone";
                paras.Add(new MySqlParameter("@telephone", $"%{fphone}%"));
            }
            string fuserIds = string.Empty;
            if (paras.Count > 0)
            {
                List<C_UserInfo> fuserList = C_UserInfoBLL.SingleModel.GetList(fsqlwhere, paras.ToArray());
                if (fuserList == null || fuserList.Count <= 0)
                {
                    return list;
                }
                fuserIds = string.Join(",", fuserList.Select(user => user.Id));
            }
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            string sql = $" select a.* from pinAgent as a";
            string sqlcount = $"select count(1) from pinAgent as a";
            string sqlwhere = $" where  a.aid={aid}";
            if (state == -999)
            {
                sqlwhere += $"  and a.state>{(int)AgentState.未可用}";
            }
            else
            {
                sqlwhere += $" and a.state={state} ";
            }
            if (!string.IsNullOrEmpty(fuserIds))
            {
                sqlwhere += $" and a.fuserId in ({fuserIds})";
            }
            if (!string.IsNullOrEmpty(storeName) || !string.IsNullOrEmpty(phone))
            {
                sql = $"{sql} left join pinstore as b on a.userid=b.userid";
                sqlcount = $"{sqlcount} left join pinstore as b on a.userid=b.userid";
                sqlwhere += " and b.state=1";
                if (!string.IsNullOrEmpty(storeName))
                {
                    sqlwhere += " and b.storename like @storename";
                    parameters.Add(new MySqlParameter("@storeName", $"%{storeName}%"));
                }
                if (!string.IsNullOrEmpty(phone))
                {
                    sqlwhere += " and b.phone like @phone";
                    parameters.Add(new MySqlParameter("@phone", $"%{phone}%"));
                }
            }
            if (!string.IsNullOrEmpty(nickName))
            {
                sql = $"{sql} left join c_userInfo as c on a.userid=c.id";
                sqlcount = $"{sqlcount} left join c_userInfo as c on a.userid=c.id";
                sqlwhere += " and c.nickname like @nickname";
                parameters.Add(new MySqlParameter("@nickname", $"%{nickName}%"));
            }
            sql = $"{sql} {sqlwhere} order by a.id desc limit {(pageIndex - 1) * pageSize},{pageSize} ";
            list = GetListBySql(sql, parameters.ToArray());
            sqlcount = $"{sqlcount} {sqlwhere}";
            recordCount = GetCountBySql(sqlcount, parameters.ToArray());
            return list;
        }

    
        /// <summary>
        /// 获取商家订单提成
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        public bool AddStoreIncome(DrawCashApply apply)
        {
            bool result = false;
            TransactionModel tran = new TransactionModel();

            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, apply.userId);
            if (store == null)
            {
                log4net.LogHelper.WriteInfo(GetType(), $"获取商家订单提成失败：商户不存在 userId:{apply.userId}");
                return result;
            }

            //更新提现状态
            tran.Add(DrawCashApplyBLL.SingleModel.BuildUpdateSql(apply, "drawstate,drawtime,updatetime,remark"));

            //各级分成
            if (store.agentId > 0)
            {
                PinAgent fagent = GetModelById(store.agentId);
                if (fagent == null)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"获取商家订单提成失败：代理不存在 agentId:{store.agentId}");
                    return result;
                }

                PinAgentLevelConfig fagentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(fagent.AgentLevel, fagent.aId);
                if (fagentLevelConfig == null)
                {
                    log4net.LogHelper.WriteInfo(GetType(), $"获取商家订单提成错误：代理商{fagent.id}等级找不到");
                    return result;
                }

                //表示商家上级找到了进行分成
                int money = fagentLevelConfig.OrderExtract * apply.applyMoney;
                fagent.cash += money;
                fagent.money += money;
                tran.Add(BuildUpdateSql(fagent, "cash,money"));
                PinAgentIncomeLogBLL.SingleModel.AddAgentLog(store, money, fagent, tran, "");//插入上级提成日志



                #region 表示有上上级推广者
               
                PinStore fatherStore = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, fagent.userId);//查找出推广者的店铺，然后再找出上上级推广者
                if (fatherStore != null)
                {
                    if (fatherStore.agentId > 0)
                    {
                        //表示有上上级推广者
                        PinAgent gfAgent = GetModelById(fatherStore.agentId);
                        if (gfAgent != null)
                        {
                            PinAgentLevelConfig gfLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(gfAgent.AgentLevel, gfAgent.aId);
                            if (gfLevelConfig == null)
                            {
                                log4net.LogHelper.WriteInfo(GetType(), $"获取商家订单提成错误：上上级代理商{fagent.id}等级找不到");
                                return result;
                            }

                            int gfatherAgentMoney = gfLevelConfig.SecondOrderExtract* apply.applyMoney;
                            gfAgent.cash += gfatherAgentMoney;
                            gfAgent.money += gfatherAgentMoney;
                            tran.Add(BuildUpdateSql(gfAgent, "cash,money"));
                            PinAgentIncomeLogBLL.SingleModel.AddAgentLog(store, gfatherAgentMoney, gfAgent, tran, "");//插入上上级提成日志

                        }


                    }
                }
                #endregion


              
            }
           
            result = ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
            return result;
        }
        /// <summary>
        /// 平台后台-配置代理
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int SetAgent(int aid, int userId, int state, out string msg,int agentMoney=0)
        {
            int code = 0;
            PinAgent agent = null;
            switch (state)
            {
                case 0://取消代理
                    agent = GetModelByUserId(userId);
                    if (agent == null)
                    {
                        code = 1;
                        msg = "操作成功";
                    }
                    else
                    {
                        agent.state = 0;
                        code = Update(agent, "state") ? 1 : 0;
                        msg = code == 1 ? "操作成功" : "操作失败";
                    }
                    break;
                case 1:
                    agent = GetModel($"userid={userId}");
                    if (agent == null)
                    {
                        agent = new PinAgent
                        {
                            aId = aid,
                            userId = userId,
                            addTime = DateTime.Now,
                            state = 1,
                            AgentLevel=1,
                            AgentTime=1
                         
                        };
                        code = Convert.ToInt32(Add(agent)) > 0 ? 1 : 0;
                        msg = code == 1 ? "操作成功" : "操作失败";
                    }
                    else
                    {
                        agent.AgentLevel = 1;
                        agent.AgentTime = 1;
                        agent.addTime = DateTime.Now;
                        agent.state = 1;
                        code = Update(agent, "state,AgentLevel,AgentTime,addTime") ? 1 : 0;
                        msg = code == 1 ? "操作成功" : "操作失败";
                    }

                    PinGoodsOrder pinOrder = new PinGoodsOrder() { money = agentMoney };
                    TransactionModel tran = new TransactionModel();
                    PinAgentChangeLog pinAgentChangeLog = new PinAgentChangeLog()
                    {
                        Aid = agent.aId,
                        AgentId = agent.id,
                        ChangeMoney = agentMoney,
                        Remark = $"成为代理,花费{agentMoney * 0.01}元",
                        ChangeType = 2,
                        AddTime = DateTime.Now

                    };
                    tran.Add(PinAgentChangeLogBLL.SingleModel.BuildAddSql(pinAgentChangeLog));
                    UpdateIncome(agent, pinOrder, tran);
                    PinAgentIncomeLogBLL.SingleModel.AddAgentLog(agent, pinOrder, tran);//插入提成日志

                    //分提成
                    if (!base.ExecuteTransactionDataCorect(tran.sqlArray))
                    {
                        agent.state = 0;
                        Update(agent, "state");
                        code = 0;
                        msg = "代理分成异常";
                    }



                    break;
                default:
                    msg = "参数错误";
                    break;
            }
            return code;

        }

        public PinAgent GetModelById(int agentId)
        {
            PinAgent agent = null;
            if (agentId <= 0)
            {
                return agent;
            }
            string sqlwhere = $" id={agentId} and state=1";
            agent = GetModel(sqlwhere);
            return agent;
        }

        public int GetCountByFuserId(int fuserId)
        {
            string sqlwhere = $" fuserId={fuserId} and state=1";
            return GetCount(sqlwhere);
        }

        public List<object> GetListByAgentId_type(int aid, int fuserId, int pageIndex, int pageSize, out int count, int extractType = 0)
        {
            string sqlwhere = $" fuserId={fuserId} and state=1 and aid={aid}";
            if (extractType == 1)
            {
                //查找下下级
                sqlwhere = $" aid={aid} and fuserId in({GetGrandfatherAgentIds(fuserId)}) and state=1 ";
            }
            count = GetCount(sqlwhere);
            List<PinAgent> list = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            List<object> objList = new List<object>();
            string agentLevelName = string.Empty;
            if (list != null && list.Count > 0)
            {
                string userIds = string.Join(",", list.Select(s => s.userId));
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                foreach (var agent in list)
                {
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == agent.userId) ?? new C_UserInfo();
                    string sql = $"select sum(income) as income from pinagentincomelog where source=0 and userid={fuserId} and  sourceuid={agent.userId} and ExtractType={extractType}";
                    var result = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql);

                    string income = result == DBNull.Value ? "0.00" : (Convert.ToInt32(result) * 0.001 * 0.01).ToString("0.00");

                    PinAgentLevelConfig pinAgentLevelConfig = PinAgentLevelConfigBLL.SingleModel.GetPinAgentLevelConfig(agent.AgentLevel,aid);
                    if (pinAgentLevelConfig != null)
                    {
                        agentLevelName = pinAgentLevelConfig.LevelName;//等级名称
                    }

                    objList.Add(new { headImg = userInfo.HeadImgUrl, name = userInfo.NickName, income, addtime = agent.addTimeStr, phone = userInfo.TelePhone, storeId = userInfo.StoreId, agentLevelName= agentLevelName });
                }
            }
            return objList;
        }

        /// <summary>
        /// 获取下级userId集合
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public string GetGrandfatherAgentIds(int agentId)
        {
            List<PinAgent> list = base.GetList($" fuserId={agentId} and state=1 ", 1000000, 1, "userId");
            if (list == null || list.Count <= 0)
            {
                return "-1";
            }
            List<int> listIds = new List<int>();
            foreach (PinAgent item in list)
            {
                if (!listIds.Contains(item.userId))
                {
                    listIds.Add(item.userId);
                }
            }

            return string.Join(",", listIds);
        }

        /// <summary>
        /// 获取发展的入驻代理商
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="fuserId"></param>
        /// <param name="extractType">0 查找下级 1查找下下级</param>
        /// <returns></returns>
        public int GetAgentCount(int aid, int fuserId, int extractType)
        {

            string sqlwhere = $" fuserId={fuserId} and state=1 and aid={aid}";
            if (extractType == 1)
            {
                //查找下下级
                sqlwhere = $" aid={aid} and fuserId in({GetGrandfatherAgentIds(fuserId)}) and state=1 ";
            }
            return GetCount(sqlwhere);
        }



    }
}
