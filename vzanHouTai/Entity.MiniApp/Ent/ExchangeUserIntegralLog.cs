using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    ///用户积分日志记录 消费时候赠送或者退款时扣除
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ExchangeUserIntegralLog
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 规则Id
        /// </summary>
        [SqlField]
        public int ruleId { get; set; } = 0;
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;
       
        /// <summary>
        /// 赠送积分
        /// </summary>
        [SqlField]
        public int integral { get; set; } = 0;


        /// <summary>
        /// 当选择部分商品消费赠送积分的时候 此字段为商品id集合以逗号分隔
        /// </summary>
        [SqlField]
        public string goodids { get; set; } = string.Empty;


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }
        public string UpdateDateStr
        {
            get
            {
                return UpdateDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        
        /// <summary>
        /// 积分赠送规则方式  0→全场商品消费赠送积分  1→ 部分商品消费赠送积分
        /// </summary>
        [SqlField]
        public int ruleType { get; set; } = 0;

        /// <summary>
        /// 每笔消费满多少钱赠送积分
        /// </summary>
        [SqlField]
        public int price { get; set; } = 0;

        public string priceStr
        {
            get
            {

                return (price * 0.01).ToString("0.00");
            }
        }
       



        /// <summary>
        /// 用户Id
        /// </summary>
        [SqlField]
        public int userId { get; set; }

        /// <summary>
        /// 订单Id entgoodorder里的Id
        /// </summary>
        [SqlField]
        public int orderId { get; set; }

        /// <summary>
        /// 满足积分规则的产品Id
        /// </summary>
        [SqlField]
        public string usegoodids { get; set; }

        /// <summary>
        ///积分交易动作 0→增加(消费赠送) -1→减少(发生退款)
        /// </summary>
        [SqlField]
        public int actiontype { get; set; }

        /// <summary>
        /// 本次发生变化的积分
        /// </summary>
        [SqlField]
        public int curintegral { get; set; }

        /// <summary>
        /// 本次送积分交易金额
        /// </summary>
        [SqlField]
        public int buyPrice { get; set; }

        public string buyPriceStr
    {
            get
            {

                return (buyPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        ///积分来自的订单类型 0→普通商品订单 1→兑换商品订单 2→充钱储值 3→签到送积分 4→后台手动赠送积分 5→后台手动减少积分
        /// </summary>
        [SqlField]
        public int ordertype { get; set; } = 0;

    }



}
