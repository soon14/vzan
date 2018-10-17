using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.User
{
    public class MsgAccountBLL : BaseMySql<MsgAccount>
    {
        #region 单例模式
        private static MsgAccountBLL _singleModel;
        private static readonly object SynObject = new object();

        private MsgAccountBLL()
        {

        }

        public static MsgAccountBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new MsgAccountBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public string GetUnionIdBy(int agentId, Guid managerId)
        {
            return GetByManagerAgent(agentId, managerId)?.UnionId;
        }

        /// <summary>
        /// 获取发送消息账号
        /// </summary>
        /// <param name="agentId">代理ID</param>
        /// <param name="managerId">管理员ID</param>
        /// <returns></returns>
        public MsgAccount GetByManagerAgent(int agentId, Guid managerId)
        {
            string whereSql = BuildWhereSql(agentId: agentId, managerGuid: managerId.ToString(), state: 0);
            return GetModel(whereSql);
        }

        public List<MsgAccount> GetListByManagerAgent(int agentId, Guid managerId)
        {
            string whereSql = BuildWhereSql(agentId: agentId, managerGuid: managerId.ToString(), state: 0);
            int pageSzie = 5;
            if (agentId == 9)//测试账号
            {
                pageSzie = 15;
            }
            return GetList(whereSql, pageSzie, 1, "*", "ID DESC");
        }

        public bool UpdateMsgAccount(int agentId, Guid managerId, WeiXinUser userInfo)
        {
            if (isRepeat(agentId: agentId, managerAccount: managerId, unionId: userInfo.unionid))
            {
                return true;
            }
            TransactionModel updateTran = new TransactionModel();
            //获取当前配置消息账号
            //MsgAccount currentMsgAccount = GetByManagerAgent(agentId: agentId, managerId: managerId);
            //if (currentMsgAccount != null)
            //{
            //    //删除现有配置消息账号
            //    currentMsgAccount.State = -1;
            //    updateTran.Add(BuildUpdateSql(currentMsgAccount, "State"));
            //}
            string addresss = $"{userInfo.province} {userInfo.city} {userInfo.country}";
            //添加新消息账号
            MsgAccount newMsgAccount = new MsgAccount()
            {
                AddTime = DateTime.Now,
                Agentid = agentId,
                ManagerGuid = managerId,
                OpenId = userInfo.openid,
                UnionId = userInfo.unionid,
                NickName = userInfo.nickname,
                HeadImgUrl = userInfo.headimgurl,
                Sex = userInfo.sex,
                Address = addresss,
                State = 0,
            };
            updateTran.Add(BuildAddSql(newMsgAccount));
            //return 执行事务
            return ExecuteTransaction(updateTran.sqlArray, updateTran.ParameterArray);
        }

        public bool isRepeat(int agentId, Guid managerAccount, string unionId)
        {
            string whereSql = BuildWhereSql(agentId: agentId, managerGuid: managerAccount.ToString(), state: 0, unionId: unionId);
            return GetCount(whereSql) > 0;
        }

        public bool UpdateToUnBind(MsgAccount msgAccount)
        {
            //MsgAccount msgAccount = GetByManagerAgent(agentId: agentId, managerId: managerId);
            msgAccount.State = -1;
            return Update(msgAccount, "State");
        }

        public string BuildWhereSql(int agentId = 0, string managerGuid = null, int? state = 0, string unionId = null)
        {
            string whereSql = string.Empty;
            if (state.HasValue)
            {
                whereSql = $"State = {state.Value}";
            }
            if (agentId > 0)
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"AgentId = {agentId}" : $"{whereSql} AND AgentId = {agentId}";
            }
            if (!string.IsNullOrWhiteSpace(managerGuid))
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"ManagerGuid = '{managerGuid}'" : $"{whereSql} AND ManagerGuid = '{managerGuid}'";
            }
            if (!string.IsNullOrWhiteSpace(unionId))
            {
                whereSql = string.IsNullOrWhiteSpace(whereSql) ? $"UnionId = '{unionId}'" : $"{whereSql} AND UnionId = '{unionId}'";
            }
            return whereSql;
        }
    }
}
