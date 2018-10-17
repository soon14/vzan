using BLL.MiniApp.Helper;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishCardAccountLogBLL : BaseMySql<DishCardAccountLog>
    {
        #region 单例模式
        private static DishCardAccountLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishCardAccountLogBLL()
        {

        }

        public static DishCardAccountLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishCardAccountLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public void AddRecordLog(DishVipCard card, double account_money, string account_info)
        {
            DishCardAccountLog log = new DishCardAccountLog();
            log.account_money = account_money;
            log.account_info = string.IsNullOrEmpty(account_info) ? "手动充值" : $"手动充值：{account_info}";
            log.account_type = 1;
            log.add_time = DateTime.Now;
            log.aId = card.aid;
            log.shop_id = card.shop_id;
            log.user_id = card.uid;
            log.state = 1;
            Add(log);

            //发 充值成功通知 模板消息
            DishStore store =DishStoreBLL.SingleModel.GetModelByAid_Id(card.aid, card.shop_id);
            object curSortQueue_TemplateMsgObj = TemplateMsg_Miniapp.DishMessageData(store.dish_name, card, log, log.account_money, SendTemplateMessageTypeEnum.充值成功通知);
            TemplateMsg_Miniapp.SendTemplateMessage(log.user_id, SendTemplateMessageTypeEnum.充值成功通知, (int)TmpType.智慧餐厅, curSortQueue_TemplateMsgObj, $"pages/restaurant/restaurant-card/index?dish_id={log.shop_id}&savemoney=0");

        }

        public void AddRecordLog(DishVipCard card, int account_type, double account_money, string info, TransactionModel tran = null)
        {
            DishCardAccountLog log = new DishCardAccountLog();
            log.account_money = account_money;
            log.account_info = info;
            log.account_type = account_type;
            log.add_time = DateTime.Now;
            log.aId = card.aid;
            log.shop_id = card.shop_id;
            log.user_id = card.uid;
            log.state = 1;

            if (tran != null)
            {
                tran.Add(BuildAddSql(log));
            }
            else
            {
                Add(log);
            }
        }

        public List<DishCardAccountLog> GetRecordLogList(int storeId, int uid, int pageIndex, int pageSize, out int recordCount, int account_type = -999)
        {
            List<DishCardAccountLog> logList = null;
            if (storeId <= 0 || uid <= 0)
            {
                recordCount = 0;
                return logList;
            }
            string sqlwhere = $" shop_id={storeId} and user_id={uid} and state=1";
            if (account_type != -999)
            {
                sqlwhere += $" and account_type={account_type}";
            }
            logList = GetList(sqlwhere, pageSize, pageIndex, "*", "id desc");
            recordCount = GetCount(sqlwhere);
            return logList;
        }

        public double GetAccountAll(int account_type, int shop_id, int user_id)
        {
            double sum = 0.00;
            string sql = $"select sum(account_money) from dishcardaccountlog where account_type={account_type} and shop_id={shop_id} and state=1 and user_id={user_id}";
            sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql) == DBNull.Value ? 0 : Convert.ToDouble(SqlMySql.ExecuteScalar(connName, CommandType.Text, sql));
            return sum;
        }

        public int CreateOrder(DishCardAccountLog accountLog)
        {
            int cityMorderId = 0;
            if (accountLog == null)
            {
                return cityMorderId;
            }
            XcxAppAccountRelation xcxAppAccount = XcxAppAccountRelationBLL.SingleModel.GetModel(accountLog.aId);
            if (xcxAppAccount == null)
            {
                return cityMorderId;
            }
            string no = WxPayApi.GenerateOutTradeNo();
            CityMorders cityMorder = new CityMorders()
            {
                OrderType = (int)ArticleTypeEnum.DishCardAccount,
                ActionType = (int)ArticleTypeEnum.DishCardAccount,
                Addtime = DateTime.Now,
                payment_free = (int)(accountLog.account_money * 100),
                trade_no = no,
                userip = WebHelper.GetIP(),
                FuserId = accountLog.user_id,
                orderno = no,
                payment_status = 0,
                Status = 0,
                Articleid = accountLog.id,
                MinisnsId = accountLog.aId,
                ShowNote = $"会员充值支付{accountLog.account_money}元",
                CitySubId = 0,
                PayRate = 1,
                buy_num = 0,
                appid = xcxAppAccount.AppId
            };
            cityMorderId = Convert.ToInt32(new CityMordersBLL().Add(cityMorder));
            return cityMorderId;
        }

        /// <summary>
        /// 根据日期查询门店余额消费总额
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public double GetAccountSumByDate(int storeId, DateTime startDate, DateTime endDate, bool isSelByDate = true)
        {
            string sql = $"select sum(account_money) from DishCardAccountLog where shop_id={storeId} and account_type=2 and state=1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (isSelByDate)
            {
                sql += $" and add_time between @startDate and @endDate";
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
            }
            object sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parameters.ToArray());
            if (sum == DBNull.Value)
            {
                sum = 0;
            }
            return Convert.ToDouble(sum);
        }

        /// <summary>
        /// 获取余额消费订单总数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public int GetCountByDate(int storeId, DateTime startDate, DateTime endDate, bool isSelByDate = true)
        {
            string sql = $"select count(1) from DishCardAccountLog where shop_id={storeId} and account_type=2 and state=1";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (isSelByDate)
            {
                sql += $" and add_time between @startDate and @endDate";
                parameters.Add(new MySqlParameter("@startDate", startDate));
                parameters.Add(new MySqlParameter("@endDate", endDate));
            }
            object sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parameters.ToArray());
            if (sum == DBNull.Value)
            {
                sum = 0;
            }
            return Convert.ToInt32(sum);
        }
    }
}