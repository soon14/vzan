using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Base;
using Entity.MiniApp.Im;
using MySql.Data.MySqlClient;
using System.Data;
using Entity.MiniApp;
using Entity.MiniApp.Qiye;
using BLL.MiniApp.Qiye;
using System.Text;

namespace BLL.MiniApp.Im
{
    public class ImMessageBLL : BaseMySql<ImMessage>
    {
        #region 单例模式
        private static ImMessageBLL _singleModel;
        private static readonly object SynObject = new object();

        private ImMessageBLL()
        {

        }

        public static ImMessageBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ImMessageBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public static string _redis_AllMessageCount = "messageCount_{0}";
        public static string _redis_UserMessageCount = "messageCount_{0}_{1}";

        public void UpdateIMReadState(int tuserid, int fuserId)
        {
            string sql = $"update ImMessage set isread=1 where ((fuserId={fuserId} and tuserid = {tuserid}) or (fuserId={tuserid} and tuserid = {fuserId}) ) and isread=0";
            SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql);
        }

        /// <summary>
        /// 缓存获取针对某个人的未读消息数
        /// </summary>
        /// <param name="tuserId"></param>
        /// <param name="fuserId"></param>
        /// <returns></returns>
        public int GetUserNoReadCount(int tuserId,int fuserId)
        {
            string key = string.Format(_redis_UserMessageCount,tuserId,fuserId);
            return RedisUtil.Get<int>(key);
        }

        /// <summary>
        /// 删除缓存获取针对某个人的未读消息数
        /// </summary>
        /// <param name="tuserId"></param>
        /// <param name="fuserId"></param>
        /// <returns></returns>
        public void RemoveUserNoReadCount(int tuserId, int fuserId)
        {
            string key = string.Format(_redis_UserMessageCount, fuserId, tuserId);
            RedisUtil.Set<int>(key,0);
        }

        /// <summary>
        /// 未读信息数量
        /// </summary>
        /// <param name="tuserId"></param>
        /// <returns></returns>
        public int GetCountNoRead(int fuserId, int tuserId)
        {
            string sqlWhere = $"fuserId={fuserId} and tuserId={tuserId} and isread=0";
            return base.GetCount(sqlWhere);
        }

        /// <summary>
        /// 获取是否有已阅读
        /// </summary>
        /// <param name="tuserid">接收者Userid</param>
        /// <param name="isread">阅读状态 0未读 1已读</param>
        /// <returns></returns>
        public bool ExitNoRead(long tuserid, int isread = 0)
        {
            return base.Exists($"tuserId={tuserid} and isRead={isread}");
        }
        /// <summary>
        /// 获取最近一条消息记录
        /// </summary>
        /// <param name="fuserId"></param>
        /// <param name="tuserId"></param>
        /// <param name="ver">区分不同版本 0足浴版 1专业版（通用版）</param>
        /// <returns></returns>
        public ImMessage GetNewMessage(int fuserId, int tuserId, int fuserType, int ver)
        {
            ImMessage message = null;
            string fkey = string.Empty;
            string tkey = string.Empty;
            List<ImMessage> messageList = new List<ImMessage>();
            //客户
            if (fuserType == 0)
            {
                if (ver == 0)
                {

                    fkey = $"immessagekey_{fuserId}_{tuserId}_1";//0跟1取决于接收消息的人的用户类型
                }
                else
                {
                    fkey = $"immessagekey_{fuserId}_{tuserId}_0";
                }
                messageList = RedisUtil.GetRange<ImMessage>(fkey);
                tkey = $"immessagekey_{tuserId}_{fuserId}_0";//0跟1取决于接收消息的人的用户类型
                messageList.AddRange(RedisUtil.GetRange<ImMessage>(tkey));
                if (messageList != null && messageList.Count > 0)
                {
                    messageList = messageList.OrderByDescending(msg => msg.sendDate).ToList();
                    message = messageList[0];
                }
                else
                {
                    if (ver == 0)
                    {
                        message = GetList($"(fuserid = {fuserId} and tuserid = {tuserId} and tuserType=1) or (fuserid = {tuserId} and tuserid = {fuserId} and tuserType=0)", 1, 1, "*", "senddate desc").FirstOrDefault();
                    }
                    else
                    {
                        message = GetList($"(fuserid = {fuserId} and tuserid = {tuserId} and tuserType=0) or (fuserid = {tuserId} and tuserid = {fuserId} and tuserType=0)", 1, 1, "*", "senddate desc").FirstOrDefault();
                    }
                }
            }
            //技师
            else if (fuserType == 1)
            {
                fkey = $"immessagekey_{fuserId}_{tuserId}_0";//0跟1取决于接收消息的人的用户类型
                messageList = RedisUtil.GetRange<ImMessage>(fkey);
                tkey = $"immessagekey_{tuserId}_{fuserId}_1";//0跟1取决于接收消息的人的用户类型
                messageList.AddRange(RedisUtil.GetRange<ImMessage>(tkey));
                if (messageList != null && messageList.Count > 0)
                {
                    messageList = messageList.OrderByDescending(msg => msg.sendDate).ToList();
                    message = messageList[0];
                }
                else
                {
                    message = GetList($"(fuserid = {fuserId} and tuserid = {tuserId} and tuserType=0) or (fuserid = {tuserId} and tuserid = {fuserId} and tuserType=1)", 1, 1, "*", "senddate desc").FirstOrDefault();
                }
            }

            return message;
        }

        /// <summary>
        /// 获取历史消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fuserId"></param>
        /// <param name="tuserId"></param>
        /// <returns></returns>
        public List<ImMessage> GetHistory(int id, int fuserId, int tuserId, int fuserType, int ver)
        {
            List<ImMessage> messageList = new List<ImMessage>();
            string fkey = string.Empty;
            string tkey = string.Empty;
            //客户
            if (fuserType == 0)
            {
                if (ver == 0)
                {
                    fkey = $"immessagekey_{fuserId}_{tuserId}_1";//0跟1取决于接收消息的人的用户类型
                }
                else
                {
                    fkey = $"immessagekey_{fuserId}_{tuserId}_0";
                }
                messageList = RedisUtil.GetRange<ImMessage>(fkey);
                tkey = $"immessagekey_{tuserId}_{fuserId}_0";//0跟1取决于接收消息的人的用户类型
                messageList.AddRange(RedisUtil.GetRange<ImMessage>(tkey));
                // log4net.LogHelper.WriteInfo(GetType(), $"{fuserId},{tuserId},{ver}");
                if (ver == 0)
                {
                    if (id <= 0)
                    {
                        messageList.AddRange(GetList($"((fuserid={fuserId} and tuserid={tuserId} and tusertype=1)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=0))", 20, 1, "*", "senddate desc"));
                    }
                    else
                    {
                        messageList.AddRange(GetList($"((fuserid={fuserId} and tuserid={tuserId} and tusertype=1)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=0)) and id<{id}", 20, 1, "*", "senddate desc"));
                    }
                }
                else
                {
                    if (id <= 0)
                    {
                        messageList.AddRange(GetList($"((fuserid={fuserId} and tuserid={tuserId} and tusertype=0)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=0))", 20, 1, "*", "senddate desc"));
                    }
                    else
                    {
                        messageList.AddRange(GetList($"((fuserid={fuserId} and tuserid={tuserId} and tusertype=0)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=0)) and id<{id}", 20, 1, "*", "senddate desc"));
                    }
                }
            }
            //技师
            else if (fuserType == 1)
            {
                fkey = $"immessagekey_{fuserId}_{tuserId}_0";
                messageList = RedisUtil.GetRange<ImMessage>(fkey);
                tkey = $"immessagekey_{tuserId}_{fuserId}_1";
                messageList.AddRange(RedisUtil.GetRange<ImMessage>(tkey));

                messageList = messageList.Where(msg => (msg.fuserId == fuserId && msg.tuserId == tuserId && msg.tuserType == 0) || (msg.tuserId == fuserId && msg.fuserId == tuserId && msg.tuserType == 1)).ToList();
                if (id <= 0)
                {
                    messageList.AddRange(GetList($"((fuserid={fuserId} and tuserid={tuserId} and tusertype=0)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=1))", 20, 1, "*", "senddate desc"));
                }
                else
                {
                    messageList.AddRange(GetList($"((fuserid={fuserId} and tuserid={tuserId} and tusertype=0)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=1)) and id<{id}", 20, 1, "*", "senddate desc"));
                }
            }
            messageList = messageList.OrderBy(x => x.sendDate).ToList();
            return messageList;
        }

        /// <summary>
        /// 小未平台获取私信
        /// </summary>
        /// <param name="tuserId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ImMessage> GetListCardByTuserid(int tuserId, int pageSize, int pageIndex, ref int count)
        {
            List<ImMessage> list = new List<ImMessage>();
            string sql = $"select {"{0}"} from immessage im left join platmycard p on im.fuserid = p.userid where im.id in(select max(id)from immessage where (tuserid = {tuserId} ) group by fuserid, tuserid)";
            string sqllist = string.Format(sql, "im.*,p.name cardname,p.ImgUrl cardimgurl");
            string sqlcount = string.Format(sql, "count(*)");
            count = base.GetCountBySql(sqlcount);
            string sqlpage = $" LIMIT {(pageIndex - 1) * pageSize},{pageSize} ";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqllist + sqlpage, null))
            {
                while (dr.Read())
                {
                    ImMessage model = base.GetModel(dr);
                    model.CardName = dr["cardname"].ToString();
                    model.CardImgUrl = dr["cardimgurl"].ToString();
                    list.Add(model);
                }
            }
            return list;
        }

        /// <summary>
        /// 小未平台获取私信
        /// </summary>
        /// <param name="tuserId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ImMessage> GetListCardByTuseridV2(string appid, int fuserId, int pageSize, int pageIndex, ref int count)
        {
            List<ImMessage> list = new List<ImMessage>();
            List<ImContact> contactList = ImContactBLL.SingleModel.GetAllContact(fuserId, appid);
            if (contactList == null || contactList.Count <= 0)
                return list;

            foreach (ImContact item in contactList)
            {
                ImMessage model = GetNewMessage(item.tuserId, item.fuserId, 0, 1);
                if (model == null)
                    continue;
                model.NoReadMessageCount= GetUserNoReadCount(item.tuserId,item.fuserId);
                model.CardName = item.tuserNicename;
                model.CardImgUrl = item.tuserHeadImg;
                model.tuserId = item.tuserId;
                model.isRead = 0;
                list.Add(model);
            }
            return list;
        }

        /// <summary>
        /// 获取对话者userid
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ImMessage> GetListByTuserid(int userId)
        {
            List<ImMessage> list = new List<ImMessage>();
            string sql = $"select id,fuserid,tuserid from immessage where fuserId={userId} GROUP BY tuserid UNION select id,fuserid,tuserid from immessage where tuserid = {userId} GROUP BY fuserId";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                list = base.GetList(dr);
            }
            if (list == null || list.Count <= 0)
            {
                return list;
            }

            List<ImMessage> tempList = new List<ImMessage>();
            foreach (ImMessage item in list)
            {
                ImMessage model = new ImMessage();
                if (item.tuserId == userId && item.fuserId > 0)
                {
                    model.tuserId = item.fuserId;
                }
                else if (item.fuserId == userId && item.tuserId > 0)
                {
                    model.tuserId = item.tuserId;
                }
                else
                {
                    continue;
                }

                tempList.Add(model);
            }

            return tempList;
        }

        public List<ImMessage> GetListByTFUserId(int tuserId, int fuserId, int pageIndex, int pageSize, ref int count)
        {
            List<ImMessage> list = new List<ImMessage>();
            string sqlWhere = $"((fuserid={fuserId} and tuserid={tuserId} and tusertype=0)or(fuserid={tuserId} and tuserid={fuserId} and tusertype=0))";

            list = base.GetList(sqlWhere, pageSize, pageIndex, "*", "senddate desc");
            count = base.GetCount(sqlWhere);
            if (count > 0)
            {
                string userIds = string.Join(",", list.Select(s => s.fuserId));
                string userIds2 = string.Join(",", list.Select(s => s.tuserId));
                if (userIds.Length > 0 && userIds2.Length > 0)
                {
                    userIds += "," + userIds2;
                }
                else if (userIds.Length <= 0 && userIds2.Length > 0)
                {
                    userIds = userIds2;
                }
                List<C_UserInfo> userList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                foreach (ImMessage item in list)
                {
                    C_UserInfo tempModel = userList?.FirstOrDefault(f => f.Id == item.fuserId);
                    if (tempModel != null)
                    {
                        item.FUserImgUrl = tempModel.HeadImgUrl;
                    }
                    //tempModel = userList?.FirstOrDefault(f => f.Id == item.fuserId);
                    //if (tempModel != null)
                    //{
                    //    item.FUserImgUrl = tempModel.HeadImgUrl;
                    //}
                }
            }

            return list;
        }

        public int GetCountByTuesrId(int tuserid)
        {
            string sql = $"select Count(*) from (select * from immessage where tuserid = {tuserid} group by fuserid) t";
            return base.GetCountBySql(sql);
        }

        /// <summary>
        /// 企业智推版获取客户与员工私信记录
        /// </summary>
        /// <param name="tuserId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="count"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<ImMessage> GetListByTuserId(int tuserId, int pageSize, int pageIndex, ref int count, string name)
        {
            List<ImMessage> list = new List<ImMessage>();

            string sql = $"select {"{0}"} from (select * from immessage where tuserid={tuserId} group by fuserid) i left join c_userinfo c on i.fuserid=c.id";
            string sqlCount = string.Format(sql, "count(*)");
            //(select count(*) from immessage where tuserid={tuserId} and fuserid=i.fuserid and isread=0) messagecount,(select msg from immessage where tuserid = {tuserId} and fuserid = i.fuserid  order by senddate DESC LIMIT 1) lastmsg,
            string sqlList = string.Format(sql, $@"i.*,c.NickName, c.HeadImgUrl, c.TelePhone");
            string sqlWhere = $" where i.tuserid={tuserId}";
            string sqlPage = $" limit {(pageIndex - 1) * pageSize},{pageSize}";

            List<MySqlParameter> parms = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sqlWhere += $" and (c.NickName like @name or c.TelePhone like @name)";
                parms.Add(new MySqlParameter("@name", $"%{name}%"));
            }

            count = base.GetCountBySql(sqlCount + sqlWhere, parms.ToArray());
            if (count <= 0)
                return list;

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sqlList + sqlWhere + sqlPage, parms.ToArray()))
            {
                while (dr.Read())
                {
                    ImMessage model = base.GetModel(dr);
                    model.NoReadMessageCount = GetNoReadCount(model.fuserId, tuserId);
                    ImMessage imModel = GetNewMessage(model.fuserId, tuserId, 0, 1);
                    if (imModel != null)
                    {
                        model.LastMsg = imModel.msg;
                        model.msgType = imModel.msgType;
                    }

                    if (dr["HeadImgUrl"] != DBNull.Value)
                    {
                        model.CardImgUrl = dr["HeadImgUrl"].ToString();
                    }
                    if (dr["NickName"] != DBNull.Value)
                    {
                        model.CardName = dr["NickName"].ToString();
                    }


                    list.Add(model);
                }
            }

            string userIds = string.Join(",", list.Select(s => s.fuserId).Distinct());
            List<QiyeCustomer> customerList = QiyeCustomerBLL.SingleModel.GetListByUserIds(userIds);
            if (customerList == null || customerList.Count <= 0)
                return list;

            string empIds = string.Join(",", customerList.Select(s => s.StaffId).Distinct());
            List<QiyeEmployee> employeeList = QiyeEmployeeBLL.SingleModel.GetListByIds(empIds);
            if (employeeList == null || employeeList.Count <= 0)
                return list;

            foreach (ImMessage item in list)
            {
                QiyeCustomer customerModel = customerList.FirstOrDefault(f => f.UserId == item.fuserId);
                if (customerModel == null)
                    continue;

                QiyeEmployee empModel = employeeList.FirstOrDefault(f => f.Id == customerModel.StaffId);
                if (empModel == null)
                    continue;

                item.Desc = customerModel.Desc;
                item.EmployeeName = empModel.Name;
            }
            return list;
        }
        
        /// <summary>
        /// 企业智推版客户发送私信数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ImMessage> GetListByQiyeCustomerUserIds(string userIds)
        {
            List<ImMessage> list = new List<ImMessage>();
            if (string.IsNullOrEmpty(userIds))
                return list;

            //用storeid表示数量
            string sql = $"select Count(*) storeid,fuserid from immessage where fuserid in ({userIds}) GROUP BY fuserid";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                list = base.GetList(dr);
            }
            return list;
        }

        /// <summary>
        /// 获取缓存私信信息
        /// </summary>
        /// <param name="fuserId"></param>
        /// <param name="tuserId"></param>
        /// <returns></returns>
        public List<ImMessage> GetListByFTuserId(int fuserId, int tuserId)
        {
            string fkey = $"immessagekey_{fuserId}_{tuserId}_0";
            List<ImMessage> messageList = RedisUtil.GetRange<ImMessage>(fkey);

            return messageList;
        }

        /// <summary>
        /// 获取未读数量
        /// </summary>
        /// <param name="fuserId"></param>
        /// <param name="tuserId"></param>
        /// <returns></returns>
        public int GetNoReadCount(int fuserId, int tuserId)
        {
            int count = 0;
            List<ImMessage> messageList = GetListByFTuserId(fuserId, tuserId);
            if (messageList != null && messageList.Count >= 0)
            {
                count = messageList.Count(c => c.isRead == 0);
            }

            count += GetCountNoRead(fuserId, tuserId);

            return count;
        }

        /// <summary>
        /// 同步消息
        /// </summary>
        public void AddMessageRecord()
        {
            string messagekeys = "messagekeys";
            string msg = string.Empty;
            List<string> list = RedisUtil.GetRange<string>(messagekeys);
            if (list.Count <= 0)
            {
                msg = "没有缓存数据";
                return;
            }
            List<ImMessage> msgList = new List<ImMessage>();

            foreach (string key in list)
            {
                msgList = new List<ImMessage>();
                msgList = RedisUtil.GetRange<ImMessage>(key);
                if (msgList.Count > 0)
                {


                    StringBuilder sb = new StringBuilder();
                    foreach (var msgInfo in msgList)
                    {

                        msgInfo.msg = msgInfo.msg.Replace("\\", "\\\\");
                        msgInfo.updateDate = msgInfo.createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sb.Append(BuildAddSql(msgInfo));
                    }
                    try
                    {
                        ExecuteNonQuery(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteInfo(GetType(), $"私信错误：{sb.ToString()}{ex.Message}");
                    }
                    RedisUtil.RemoveItemListAll<string>(key);
                }
            }

            RedisUtil.RemoveItemListAll<string>(messagekeys);

        }
    }
}
