using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class AgentFollowLogBLL : BaseMySql<AgentFollowLog>
    {
        #region 单例模式
        private static AgentFollowLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentFollowLogBLL()
        {

        }

        public static AgentFollowLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentFollowLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private readonly string _redis_AgentFollowLogKey = "redis_AgentFollowLog_{0}_{1}_{2}";
        private readonly string _redis_AgentFollowLogVersion = "redis_AgentFollowLogVersion_{0}";//版本控制

        public AgentFollowLog GetLastModel(int agentdistributionrid)
        {
            List<AgentFollowLog> list = base.GetList($"AgentDistributionRelatioinId={agentdistributionrid} and state<>-2", 1, 1, "*", "addtime desc");
            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return new AgentFollowLog();
        }
        public List<AgentFollowLog> GetListByAgentDistributionRid(int agentdistributionrid, int pageSize, int pageIndex)
        {
            return base.GetList($"AgentDistributionRelatioinId={agentdistributionrid} and state<>-2", pageSize, pageIndex, "*", "addtime desc");
        }

        public int GetCountByAgentDistributionRid(int agentdistributionrid)
        {
            return base.GetCount($"AgentDistributionRelatioinId={agentdistributionrid} and state<>-2");
        }

        public List<AgentFollowLog> GetAgentFollowLogList(int agentdistributionrid, int pageIndex, int pageSize, ref int count, bool reflesh = false)
        {
            RedisModel<AgentFollowLog> model = new RedisModel<AgentFollowLog>();
            model = RedisUtil.Get<RedisModel<AgentFollowLog>>(string.Format(_redis_AgentFollowLogKey, agentdistributionrid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(string.Format(_redis_AgentFollowLogVersion, agentdistributionrid));

            if (reflesh || model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<AgentFollowLog>();
                List<AgentFollowLog> list = GetListByAgentDistributionRid(agentdistributionrid, pageSize, pageIndex);

                count = GetCountByAgentDistributionRid(agentdistributionrid);
                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                if (!reflesh)
                {
                    RedisUtil.Set<RedisModel<AgentFollowLog>>(string.Format(_redis_AgentFollowLogKey, agentdistributionrid, pageSize, pageIndex), model);
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
        public void RemoveCache(int agentdistributionrid, int agentid)
        {
            if (agentdistributionrid > 0)
            {
                RedisUtil.SetVersion(string.Format(_redis_AgentFollowLogVersion, agentdistributionrid));
            }
            if (agentid > 0)
            {
                AgentDistributionRelationBLL agentDistributionRelationBLL = new Conf.AgentDistributionRelationBLL();
                RedisUtil.SetVersion(string.Format(agentDistributionRelationBLL._redis_AgentDistributionRelationVersion, agentid));
            }

        }
    }
}
