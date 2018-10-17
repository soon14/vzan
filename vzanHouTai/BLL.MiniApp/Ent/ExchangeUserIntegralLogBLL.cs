using DAL.Base;
using Entity.MiniApp.Ent;
using System;

namespace BLL.MiniApp.Ent
{
    public class ExchangeUserIntegralLogBLL : BaseMySql<ExchangeUserIntegralLog>
    {
        #region 单例模式
        private static ExchangeUserIntegralLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangeUserIntegralLogBLL()
        {

        }

        public static ExchangeUserIntegralLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangeUserIntegralLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 积分增加日志记录
        /// </summary>
        /// <param name="rule">对应的积分规则</param>
        /// <param name="userId">用户Id</param>
        /// <param name="orderId">订单Id 到店储值消费为0</param>
        /// <param name="usegoodid">本次符合规则的产品Id 到店储值消费为0</param>
        /// <param name="curintegral">本次增加的积分</param>
        /// <returns></returns>
        public ExchangeUserIntegralLog GetAddUserIntegralLog(ExchangeRule rule,int userId,int orderId,string usegoodid,int curintegral,int ordertype,int buyPrice)
        {

            return new ExchangeUserIntegralLog
            {
                ruleId = rule.Id,
                appId = rule.appId,
                integral = rule.integral,
                price = rule.price,
                ruleType = rule.ruleType,
                goodids = rule.goodids,
                orderId = orderId,
                usegoodids = usegoodid,
                userId = userId,
                actiontype = 0,
                curintegral = curintegral,
                AddTime = DateTime.Now,
                UpdateDate = DateTime.Now,
                ordertype=ordertype,
                buyPrice=buyPrice
            };



        }



      

    }
}
