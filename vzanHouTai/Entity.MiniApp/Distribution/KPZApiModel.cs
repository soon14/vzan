using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 快跑者配送api返回结果
    /// </summary>
    public class KPZResult<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public T data { get; set; }
    }
    /// <summary>
    ///  快跑者获取运费
    /// </summary>
    public class KPZFee
    {
        /// <summary>
        /// 第三方系统商户ID
        /// </summary>
        public int shop_id { get; set; }
        /// <summary>
        /// 取单坐标火星坐标
        /// </summary>
        public string get_tag { get; set; }
        /// <summary>
        /// 送达坐标火星坐标
        /// </summary>
        public string customer_tag { get; set; }
        /// <summary>
        /// 第三方订单原配送费
        /// </summary>
        public string pay_fee { get; set; }
        /// <summary>
        /// 第三方订单总价
        /// </summary>
        public string order_price { get; set; }
    }

    /// <summary>
    /// 快跑者订单回调
    /// </summary>
    public class KPZApiReponseModel
    {
        /// <summary>
        /// 请求的过期时间，时间戳格式，如：1477483702
        /// </summary>
        public int expire_time { get; set; }
        /// <summary>
        /// 加密密钥，按支付宝密钥排序，可参考签名与验签说明，主要用于验证请求权限
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 状态发生改变的配送订单的订单单号
        /// </summary>
        public string trade_no { get; set; }
        /// <summary>
        /// 配送订单状态码，可选值为 4,5,6,7，具体参考下面的订单状态码说明表
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 创建订单时传递自定义参数 note
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 配送员的名字
        /// </summary>
        public string courier { get; set; }
        /// <summary>
        /// 配送员的电话号码
        /// </summary>
        public string tel { get; set; }
        /// <summary>
        /// 配送订单状态的改变时间，如：2017-01-01 01:01:01
        /// </summary>
        public string update_time { get; set; }
    }

    public class OrderTradeNo
    {
        /// <summary>
        /// 状态发生改变的配送订单的订单单号
        /// </summary>
        public string trade_no { get; set; }
    }
}
