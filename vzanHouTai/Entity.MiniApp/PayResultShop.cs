using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class PayResultShop
    {
        #region Model
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 公众号appid
        /// </summary>
        [SqlField]
        public string appid { get; set; }
        /// <summary>
        /// 商户ID
        /// </summary>
        [SqlField]
        public string mch_id { get; set; }
        /// <summary>
        /// 业务结果:SUCCESS/FAIL
        /// </summary>
        [SqlField]
        public string result_code { get; set; }
        /// <summary>
        /// 用户标识oppenid
        /// </summary>
        [SqlField]
        public string openid { get; set; }
        /// <summary>
        /// 是否关注公众账号
        /// </summary>
        [SqlField]
        public string is_subscribe { get; set; }
        /// <summary>
        /// 交易类型
        /// </summary>
        [SqlField]
        public string trade_type { get; set; }
        /// <summary>
        /// 总金额(分为单位)
        /// </summary>
        [SqlField]
        public int total_fee { get; set; }
        /// <summary>
        /// 货币种类
        /// </summary>
        [SqlField]
        public string fee_type { get; set; }
        /// <summary>
        /// 微信支付订单号
        /// </summary>
        [SqlField]
        public string transaction_id { get; set; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        [SqlField]
        public string out_trade_no { get; set; }
        /// <summary>
        /// 支付完成时间
        /// </summary>
        [SqlField]
        public DateTime time_end { get; set; }
        /// <summary>
        /// 错误代码
        /// </summary>
        [SqlField]
        public string err_code { get; set; }
        /// <summary>
        /// 错误代码描述
        /// </summary>
        [SqlField]
        public string err_code_des { get; set; }
        /// <summary>
        /// 商家数据包
        /// </summary>
        [SqlField]
        public string attach { get; set; }
        /// <summary>
        /// 支付类型：1打赏，2悬赏,3付费板块发帖，4帖子置顶
        /// </summary>
        [SqlField]
        public int paytype { get; set; }
        /// <summary>
        /// 支付类型：1打赏，2悬赏,3付费板块发帖，4帖子置顶
        /// </summary>
        [SqlField]
        public string orderid { get; set; }

        /// <summary>
        /// 通知状态：-1，通知失败，1~5：当前通知次数。99：通知成功
        /// </summary>
        [SqlField]
        public int notify { get; set; }

        /// <summary>
        /// 最后一次通知时间
        /// </summary>
        [SqlField]
        public DateTime notify_time { get; set; }
        #endregion Model
    }
}
