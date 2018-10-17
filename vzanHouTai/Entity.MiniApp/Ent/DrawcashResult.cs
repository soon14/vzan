using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    [Serializable]
    /// <summary>
    ///DrawcashResult:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [SqlTable(ConnName = dbEnum.MINIAPP)]
    public class DrawcashResult
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; }

        /// <summary>
        /// 随机码
        /// </summary>
        [SqlField]
        public string nonce_str { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        [SqlField]
        public string sign { get; set; }
        /// <summary>
        /// 提现订单号
        /// </summary>
        [SqlField]
        public string partner_trade_no { get; set; }
        /// <summary>
        /// 用户openid
        /// </summary>
        [SqlField]
        public string openid { get; set; }
        /// <summary>
        /// 用户验证类型
        /// </summary>
        [SqlField]
        public string check_name { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        [SqlField]
        public string re_user_name { get; set; }
        /// <summary>
        /// 提现金额
        /// </summary>
        [SqlField]
        public int amount { get; set; }
        /// <summary>
        /// 企业付款描述信息
        /// </summary>
        [SqlField]
        public string desc { get; set; }
        /// <summary>
        /// Ip地址
        /// </summary>
        [SqlField]
        public string spbill_create_ip { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 返回状态码
        /// </summary>
        [SqlField]
        public string return_code { get; set; }
        /// <summary>
        /// 返回信息,如非空，为错误原因
        /// </summary>
        [SqlField]
        public string return_msg { get; set; }
        /// <summary>
        /// 企业付款成功，返回的微信订单号
        /// </summary>
        [SqlField]
        public string payment_no { get; set; }
        /// <summary>
        /// 企业付款成功时间
        /// </summary>
        [SqlField]
        public DateTime payment_time { get; set; }
        /// <summary>
        /// 业务结果,SUCCESS/FAIL
        /// </summary>
        [SqlField]
        public string result_code { get; set; }
        /// <summary>
        /// 错误码(付款失败有)
        /// </summary>
        [SqlField]
        public string err_code { get; set; }
        /// <summary>
        /// 错误代码描述
        /// </summary>
        [SqlField]
        public string err_code_des { get; set; }
        /// <summary>
        /// 公众号appid
        /// </summary>
        [SqlField]
        public string appid { get; set; } = string.Empty;
        /// <summary>
        /// 商户ID
        /// </summary>
        [SqlField]
        public string mch_id { get; set; } = string.Empty;

    }

}
