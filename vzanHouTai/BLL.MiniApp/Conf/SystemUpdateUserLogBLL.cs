using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class SystemUpdateUserLogBLL : BaseMySql<SystemUpdateUserLog>
    {
        #region 单例模式
        private static SystemUpdateUserLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private SystemUpdateUserLogBLL()
        {

        }

        public static SystemUpdateUserLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SystemUpdateUserLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public int GetSMessageUserLogCount(string accountid)
        {
            return GetCount($"state=1 and accountid = '{accountid}'");
        }

        /// <summary>
        /// 全部标记为已读
        /// </summary>
        /// <param name="accountid"></param>
        /// <returns></returns>
        public bool AddSMUserLog(string accountid,string accountaddtime)
        {
            TransactionModel tran = new TransactionModel();

            List<SystemUpdateMessage> smslog = SystemUpdateMessageBLL.SingleModel.GetSystemUpdateMessageList(accountid, accountaddtime);
            if(smslog==null || smslog.Count<=0)
            {
                return true;
            }

            foreach (SystemUpdateMessage item in smslog)
            {
                tran.Add(base.BuildAddSql(new SystemUpdateUserLog () { State=1, AccountId=accountid, UpdateMessageId=item.Id}));
            }

            if(tran.sqlArray.Length>0)
            {
                SystemUpdateMessageBLL.SingleModel.RemoveCache(accountid);
                return base.ExecuteTransactionDataCorect(tran.sqlArray);
            }

            return false;
        }

        /// <summary>
        /// 标记为已读
        /// </summary>
        /// <param name="sysid">系统日志ID</param>
        /// <param name="accountid"></param>
        /// <returns></returns>
        public bool Readed(int sysid,string accountid)
        {
            //清缓存
            SystemUpdateMessageBLL.SingleModel.RemoveCache(accountid);

            SystemUpdateMessage sysmodel = SystemUpdateMessageBLL.SingleModel.GetModel(sysid);

            if (sysmodel!=null && sysmodel.State>=0)
            {
                SystemUpdateUserLog umodel = GetModel($"UpdateMessageId={sysid} and AccountId='{accountid}'");
                if(umodel==null)
                {
                    umodel= new SystemUpdateUserLog() { State = 1, AccountId = accountid, UpdateMessageId = sysid };
                    object result = Add(umodel);
                    if(DBNull.Value!=result)
                    {
                        return Convert.ToInt32(result)>0;
                    }
                }
                else
                {
                    //标记订单消息为已读
                    if(sysmodel.Type==3 || sysmodel.Type == 4)
                    {
                        umodel.State = 1;
                        return Update(umodel, "state");
                    }
                }
            }

            return false;
        }

    }
}