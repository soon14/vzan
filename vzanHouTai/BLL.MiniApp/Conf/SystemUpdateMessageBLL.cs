using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BLL.MiniApp.Conf
{
    public class SystemUpdateMessageBLL : BaseMySql<SystemUpdateMessage>
    {
        #region 单例模式
        private static SystemUpdateMessageBLL _singleModel;
        private static readonly object SynObject = new object();

        private SystemUpdateMessageBLL()
        {

        }

        public static SystemUpdateMessageBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SystemUpdateMessageBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private static readonly string _redis_systempupdatelogkey = "redis_SystemUpdateMessage_{0}";
        private static readonly string _redis_userallsystempupdatelogkey = "redis_AllSystemUpdateMessage_{0}_{1}_{2}";
        private static readonly string _redis_allsystempupdatelogkey = "redis_AllSystemUpdateMessage_{0}_{1}";
        private static readonly string _redis_systempupdatelogversion = "redis_SystemUpdateMessage_version";//版本

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">状态：-1：删除，0：正常</param>
        /// <returns></returns>
        public int GetSystemMessageCount(int state)
        {
            return GetCount($"state={state}");
        }

        public List<SystemUpdateMessage> GetListByPage(int pageSize,int pageIndex,ref int count)
        {
            RedisModel<SystemUpdateMessage> model = new RedisModel<SystemUpdateMessage>();
            model = RedisUtil.Get<RedisModel<SystemUpdateMessage>>(string.Format(_redis_allsystempupdatelogkey, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(_redis_systempupdatelogversion);
            
            if (model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                model = new RedisModel<SystemUpdateMessage>();
                string sqlwhere = $"Type in (0,1) and state>-1";
                model.DataList = base.GetList(sqlwhere, pageSize,pageIndex,"*", "AddTime desc");

                count = base.GetCount(sqlwhere);
                
                model.DataVersion = dataversion;
                model.Count = count;
                RedisUtil.Set<RedisModel<SystemUpdateMessage>>(string.Format(_redis_allsystempupdatelogkey, pageSize, pageIndex), model);
            }
            else
            {
                count = model!=null?model.Count:0;
            }

            return model.DataList;
        }

        /// <summary>
        /// 获取用户所有更新日志
        /// </summary>
        /// <returns></returns>
        public List<SystemUpdateMessage> GetAllSystemUpdateMessageList(string tids, string accountid, int agentid, int pageIndex, int pageSize, ref int count)
        {
            RedisModel<SystemUpdateMessage> model = new RedisModel<SystemUpdateMessage>();
            model = RedisUtil.Get<RedisModel<SystemUpdateMessage>>(string.Format(_redis_userallsystempupdatelogkey, accountid, pageSize, pageIndex));
            int dataversion = RedisUtil.GetVersion(_redis_systempupdatelogversion);

            if (model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                string sql = $"select sys.*,(select state from systemupdateuserlog sysl where sys.id = sysl.updatemessageid and sysl.AccountId='{accountid}') isread from systemupdatemessage sys where ";
                model = new RedisModel<SystemUpdateMessage>();
                List<SystemUpdateMessage> list = new List<SystemUpdateMessage>();
                string sqlwhere = $"state=0 and (type in (0,1) or (type=2 and accountid='{accountid}'))";
                //如果不是代理商，查询相对应的系统更新
                if (agentid <= 0)
                {
                    sqlwhere = $"state=0 and (type=0 or (type=2 and accountid='{accountid}'))";
                    if (!string.IsNullOrEmpty(tids))
                    {
                        sqlwhere = $"state=0 and (type=0 or (type=2 and accountid='{accountid}') or  (tid in ({tids}) and type=1))";
                    }
                }

                sql += sqlwhere;
                sql += $" ORDER BY isread asc,updatetime DESC LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

                using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
                {
                    while (dr.Read())
                    {
                        SystemUpdateMessage sysmodel = GetModel(dr);
                        if (dr["isread"] != DBNull.Value)
                        {
                            sysmodel.IsRead = Convert.ToInt32(dr["isread"]);
                        }

                        list.Add(sysmodel);
                    }
                }

                count = GetCount(sqlwhere);

                model.DataList = list;
                model.DataVersion = dataversion;
                model.Count = count;
                RedisUtil.Set<RedisModel<SystemUpdateMessage>>(string.Format(_redis_userallsystempupdatelogkey, accountid, pageSize, pageIndex), model);
            }
            else
            {
                count = model.Count;
            }

            return model.DataList;
        }

        /// <summary>
        /// 获取用户更新日志
        /// </summary>
        /// <returns></returns>
        public List<SystemUpdateMessage> GetSystemUpdateMessageList(string accountid, string accountaddtime)
        {
            RedisModel<SystemUpdateMessage> model = new RedisModel<SystemUpdateMessage>();
            model = RedisUtil.Get<RedisModel<SystemUpdateMessage>>(string.Format(_redis_systempupdatelogkey, accountid));
            int dataversion = RedisUtil.GetVersion(_redis_systempupdatelogversion);

            if (model == null || model.DataList == null || model.DataList.Count <= 0 || model.DataVersion != dataversion)
            {
                int agentid = 0;
                //判断是否是代理商
                Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(accountid);
                if (agentinfo != null)
                {
                    agentid = agentinfo.id;
                }

                model = new RedisModel<SystemUpdateMessage>();
                List<SystemUpdateMessage> list = new List<SystemUpdateMessage>();
                //系统更新日志数量
                int smessagecount = GetSystemMessageCount(0);
                //用户查看日志数量
                int smessageuserlogcount = SystemUpdateUserLogBLL.SingleModel.GetSMessageUserLogCount(accountid);
                //判断用户是否有未查看的更新日志
                if (!(smessagecount > 0 && smessagecount == smessageuserlogcount))
                {
                    string sqlwhere = $"state=0 and (type in (0,1) and updatetime>='{accountaddtime}' or (type=2 and accountid='{accountid}'))";
                    //如果不是代理商，查询相对应的系统更新
                    if (agentid <= 0)
                    {
                        sqlwhere = $"state=0 and ((type=0 and updatetime>='{accountaddtime}') or (type=2 and accountid='{accountid}'))";
                        //获取查询用户开通模板最小时间查询语句，
                        string tids = XcxAppAccountRelationBLL.SingleModel.GetListXcxappListSQL(accountid);
                        if (!string.IsNullOrEmpty(tids))
                        {
                            sqlwhere = $"state=0 and ((type=0 and updatetime>='{accountaddtime}') or (type=2 and accountid='{accountid}') {tids})";
                        }
                    }
                    List<SystemUpdateUserLog> logs = SystemUpdateUserLogBLL.SingleModel.GetList($"state = 1 and accountid = '{accountid}'");
                    if (logs != null && logs.Count > 0)
                    {
                        sqlwhere += string.Format(" and id not in({0})", string.Join(",", logs.Select(s => s.UpdateMessageId).Distinct()));
                    }


                    //获取还未查看的系统更新日志
                    list = GetList(sqlwhere, 10000, 1, "", "updatetime desc");
                }

                model.DataList = list;
                model.DataVersion = dataversion;
                RedisUtil.Set<RedisModel<SystemUpdateMessage>>(string.Format(_redis_systempupdatelogkey, accountid), model);
            }

            return model.DataList;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="yellowpagesType"></param>
        public void RemoveCache(string accountid)
        {
            if (!string.IsNullOrEmpty(accountid))
            {
                RedisUtil.SetVersion(_redis_systempupdatelogversion);
            }
        }

        /// <summary>
        /// 专业版发送预约信息通知到商家小程序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        public bool SendSubscribeMessage(int formId, int aid, string content)
        {
            
            
            bool result = false;
            if (formId <= 0 || string.IsNullOrEmpty(content) || aid <= 0)
            {
                return result;
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                return result;
            }
            SystemUpdateMessage message = new SystemUpdateMessage()
            {
                Title = formId.ToString(),
                AccountId = xcxAppAccountRelation.AccountId.ToString(),
                aid = aid,
                Content = content,
                Type = 3,
                AddTime = DateTime.Now
            };
            message.Id = Convert.ToInt32(SingleModel.Add(message));
            if (message.Id <= 0)
            {
                return result;
            }
            SystemUpdateUserLog log = new SystemUpdateUserLog()
            {
                AccountId = xcxAppAccountRelation.AccountId.ToString(),
                UpdateMessageId = message.Id
            };
            result = Convert.ToInt32(SystemUpdateUserLogBLL.SingleModel.Add(log)) > 0;
            return result;
        }
        /// <summary>
        /// 专业版发送订单消息到商家版小程序
        /// </summary>
        /// <param name="msgDic"></param>
        /// <param name="aId"></param>
        /// <param name="id"></param>
        internal bool SendOrderMessage(Dictionary<string, object> msgDic, int aid, int orderId)
        {
            
            
            bool result = false;
            if (msgDic.Count <= 0 || orderId <= 0 || aid <= 0)
            {
                return result;
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                return result;
            }
            SystemUpdateMessage message = new SystemUpdateMessage()
            {
                Title = orderId.ToString(),
                AccountId = xcxAppAccountRelation.AccountId.ToString(),
                aid = aid,
                Content = JsonConvert.SerializeObject(msgDic),
                Type = 4,
                AddTime = DateTime.Now
            };
            message.Id = Convert.ToInt32(SingleModel.Add(message));
            if (message.Id <= 0)
            {
                return result;
            }
            SystemUpdateUserLog log = new SystemUpdateUserLog()
            {
                AccountId = xcxAppAccountRelation.AccountId.ToString(),
                UpdateMessageId = message.Id
            };
            result = Convert.ToInt32(SystemUpdateUserLogBLL.SingleModel.Add(log)) > 0;
            return result;
        }
        /// <summary>
        /// 获取用户未读订单通知数量
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="accountId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetUnreadOrderMessageCount(int aid, string accountId, int type)
        {
            int count = 0;
            if (aid <= 0 || string.IsNullOrEmpty(accountId) || (type != 3 && type != 4))
            {
                return count;
            }
            string sql = $"select count(1) as count from SystemUpdateMessage a left join SystemUpdateUserLog b on a.id=b.UpdateMessageId where a.accountId=@accountId and a.type={type} and a.aid={aid} and b.state=0";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@accountId", accountId));
            count = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parameters.ToArray()));
            return count;
        }

        public SystemUpdateMessage GetModelByOrderId_Type(int orderId, int type, int aid, string accountId)
        {
            SystemUpdateMessage message = null;
            if (aid <= 0 || string.IsNullOrEmpty(accountId) || orderId <= 0 || (type != 3 && type != 4))
            {
                return message;
            }
            string sqlwhere = $"Title='{orderId}' and type={type} and aid={aid} and accountId=@accountId";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@accountId", accountId));
            message = GetModel(sqlwhere, parameters.ToArray());
            return message;
        }

        public List<SystemUpdateMessage> GetUnreadOrderMessage(int aid, string accountId, int pageIndex, int pageSize, int type)
        {
            List<SystemUpdateMessage> messageList = new List<SystemUpdateMessage>();
            if (aid <= 0 || string.IsNullOrEmpty(accountId) || (type != 3 && type != 4))
            {
                return messageList;
            }
            string sql = $"select a.* from SystemUpdateMessage a left join SystemUpdateUserLog b on a.id=b.UpdateMessageId where  a.aid={aid} and a.accountId=@accountId and a.type={type} and b.state=0 order by id desc limit {(pageIndex - 1) * pageSize},{pageSize}";
            List<MySqlParameter> paramerters = new List<MySqlParameter>();
            paramerters.Add(new MySqlParameter("@accountId", accountId));
            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, paramerters.ToArray()))
            {
                messageList = GetList(dr);
            }
            return messageList;
        }
    }
}
