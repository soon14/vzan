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
    /// 请求基类
    /// </summary>
    public class UUBase
    {
        /// <summary>
        /// 用户openid
        /// </summary>
        public string user_openid { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string order_code { get; set; }
        public string appid { get; set; }
        /// <summary>
        /// 随机字符串，不长于32位
        /// </summary>
        public string nonce_str { get; set; }
        /// <summary>
        /// 时间戳，以秒计算时间，即unix-timestamp
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 用户openid,详情见 获取openid接口
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 加密签名，详情见 消息体签名算法
        /// </summary>
        public string sign { get; set; }
    }
    /// <summary>
    /// 返回值基类
    /// </summary>
    public class UUBaseResult
    {
        /// <summary>
        /// 状态，ok/fail表示成功
        /// </summary>
        public string return_code { get; set; }
        /// <summary>
        /// 返回信息，如非空，为错误原因，如签名失败、参数格式校验错误
        /// </summary>
        public string return_msg { get; set; }
        /// <summary>
        /// 第三方用户唯一凭证
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 随机字符串，不长于32位
        /// </summary>
        public string nonce_str { get; set; }
        /// <summary>
        /// 加密签名，详情见消息体签名算法
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 第三方对接平台订单id
        /// </summary>
        public string origin_id { get; set; }
        /// <summary>
        /// 绑定用户时返回的用户OpenId
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 下单时返回订单号
        /// </summary>
        public string ordercode { get; set; }
    }

    #region 订单相关
    /// <summary>
    /// 获取运费请求实体
    /// </summary>
    public class UUGetPrice : UUBase
    {
        /// <summary>
        /// 第三方对接平台订单id
        /// </summary>
        public string origin_id { get; set; }
        /// <summary>
        /// 起始地址
        /// </summary>
        public string from_address { get; set; }
        /// <summary>
        /// 起始地址具体门牌号
        /// </summary>
        public string from_usernote { get; set; }
        /// <summary>
        /// 目的地址
        /// </summary>
        public string to_address { get; set; }
        /// <summary>
        /// 目的地址具体门牌号
        /// </summary>
        public string to_usernote { get; set; }
        /// <summary>
        /// 预约类型 0实时订单 1预约取件时间
        /// </summary>
        public string subscribe_type { get; set; }
        /// <summary>
        /// 预约时间（如：2015-09-18 14:00:25:000）没有可以传空字符串
        /// </summary>
        public string subscribe_time { get; set; }
        /// <summary>
        /// 订单小类 0帮我送(默认) 1帮我买
        /// </summary>
        public string send_type { get; set; }
        /// <summary>
        /// 订单所在城市名 称(如郑州市就填”郑州市“，必须带上“市”)
        /// </summary>
        public string city_name { get; set; }
        /// <summary>
        /// 订单所在县级地名称(如金水区就填“金水区”)
        /// </summary>
        public string county_name { get; set; }
        /// <summary>
        /// 起始地坐标经度，如果无，传0(坐标系为百度地图坐标系)
        /// </summary>
        public string from_lng { get; set; }
        /// <summary>
        /// 起始地坐标纬度，如果无，传0(坐标系为百度地图坐标系)
        /// </summary>
        public string from_lat { get; set; }
        /// <summary>
        /// 目的地坐标经度，如果无，传0(坐标系为百度地图坐标系)
        /// </summary>
        public string to_lng { get; set; }
        /// <summary>
        /// 目的地坐标纬度，如果无，传0(坐标系为百度地图坐标系)
        /// </summary>
        public string to_lat { get; set; }
    }
    /// <summary>
    /// 获取运费返回结果实体
    /// </summary>
    public class UUGetPriceResult : UUBaseResult
    {
        /// <summary>
        /// 金额令牌，提交订单前必须先计算价格
        /// </summary>
        public string price_token { get; set; }
        /// <summary>
        /// 订单总金额（优惠前）
        /// </summary>
        public string total_money { get; set; }
        /// <summary>
        /// 实际需要支付金额
        /// </summary>
        public string need_paymoney { get; set; }
        /// <summary>
        /// 总优惠金额
        /// </summary>
        public string total_priceoff { get; set; }
        /// <summary>
        /// 配送距离（单位：米）
        /// </summary>
        public string distance { get; set; }
        /// <summary>
        /// 跑腿费
        /// </summary>
        public string freight_money { get; set; }

        public string business_priceoff { get; set; }
        /// <summary>
        /// 优惠券ID
        /// </summary>
        public string couponid { get; set; }
        /// <summary>
        /// 优惠券金额
        /// </summary>
        public string coupon_amount { get; set; }
        /// <summary>
        /// 加价金额
        /// </summary>
        public string addfee { get; set; }
        /// <summary>
        /// 商品保价金额
        /// </summary>
        public string goods_insurancemoney { get; set; }
        /// <summary>
        /// Token过期时间
        /// </summary>
        public string expires_in { get; set; }
    }
    public class UUOrderFee
    {
        /// <summary>
        /// 运费
        /// </summary>
        public int Fee { get; set; }
        /// <summary>
        /// 金额令牌，提交订单前必须先计算价格
        /// </summary>
        public string price_token { get; set; }
        /// <summary>
        /// 订单总金额（优惠前）
        /// </summary>
        public string total_money { get; set; }
        /// <summary>
        /// 实际需要支付金额
        /// </summary>
        public string need_paymoney { get; set; }
    }
    ///// <summary>
    ///// UU下单实体
    ///// </summary>
    //public class UUOrder : UUBase
    //{
    //    public int Id { get; set; }
    //    /// <summary>
    //    /// 订单号
    //    /// </summary>
    //    public string order_code { get; set; }
    //    /// <summary>
    //    /// 金额令牌，计算订单价格接口返回的price_token
    //    /// </summary>
    //    public string price_token { get; set; }
    //    /// <summary>
    //    /// 订单金额，计算订单价格接口返回的total_money
    //    /// </summary>
    //    public string order_price { get; set; }
    //    /// <summary>
    //    /// 实际余额支付金额计算订单价格接口返回的need_paymoney
    //    /// </summary>
    //    public string balance_paymoney { get; set; }
    //    /// <summary>
    //    /// 收件人
    //    /// </summary>
    //    public string receiver { get; set; }
    //    /// <summary>
    //    /// 收件人电话 手机号码； 虚拟号码格式（手机号_分机号码）例如：13700000000_1111
    //    /// </summary>
    //    public string receiver_phone { get; set; }
    //    /// <summary>
    //    /// 订单备注 最长140个汉字
    //    /// </summary>
    //    public string note { get; set; }
    //    /// <summary>
    //    /// 订单提交成功后及状态变化的回调地址
    //    /// </summary>
    //    public string callback_url { get; set; }
    //    /// <summary>
    //    /// 推送方式（0 开放订单，1指定跑男，2商户绑定的跑男）默认传0即可
    //    /// </summary>
    //    public string push_type { get; set; }
    //    /// <summary>
    //    /// 推送跑男的手机号，push_type为0这里就传空字符串
    //    /// </summary>
    //    public string push_str { get; set; }
    //    /// <summary>
    //    /// 特殊处理类型，是否需要保温箱 1需要 0不需要
    //    /// </summary>
    //    public string special_type { get; set; }
    //    /// <summary>
    //    /// 取件是否给我打电话 1需要 0不需要
    //    /// </summary>
    //    public string callme_withtake { get; set; }
    //    /// <summary>
    //    /// 发件人电话，（如果为空则是用户注册的手机号）
    //    /// </summary>
    //    public string pubUserMobile { get; set; }
    //    /// <summary>
    //    /// 点赞平台订单号
    //    /// </summary>
    //    public string OrderNum { get; set; }
    //    public int Aid { get; set; }
    //    public int StoreId { get; set; }
    //    public int OrderId { get; set; }
    //    public int TemplateType { get; set; }
    //    public int OrderType { get; set; }
    //}
    /// <summary>
    /// UU取消订单实体
    /// </summary>
    public class UUCancelOrder : UUBase
    {
        /// <summary>
        /// 第三方对接平台订单id，order_code和origin_id必须二选其一，如果都传，则只根据order_code返回
        /// </summary>
        public string origin_id { get; set; }
        /// <summary>
        /// 取消原因
        /// </summary>
        public string reason { get; set; }
    }
    /// <summary>
    /// UU订单详情返回结果实体
    /// </summary>
    public class UUOrderDetailResult : UUBaseResult
    {
        public string order_code { get; set; }
        /// <summary>
        /// 起始地址
        /// </summary>
        public string from_address { get; set; }
        /// <summary>
        /// 起始地坐标经度(坐标系为百度地图坐标系)
        /// </summary>
        public string from_lng { get; set; }
        /// <summary>
        /// 起始地坐标纬度(坐标系为百度地图坐标系)
        /// </summary>
        public string from_lat { get; set; }
        /// <summary>
        /// 目的地址
        /// </summary>
        public string to_address { get; set; }
        /// <summary>
        /// 目的地坐标经度(坐标系为百度地图坐标系)
        /// </summary>
        public string to_lng { get; set; }
        /// <summary>
        /// 目的地坐标纬度(坐标系为百度地图坐标系)
        /// </summary>
        public string to_lat { get; set; }
        /// <summary>
        /// 订单的距离
        /// </summary>
        public string distance { get; set; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public string order_price { get; set; }
        /// <summary>
        /// 收件人
        /// </summary>
        public string receiver { get; set; }
        /// <summary>
        /// 收件人电话
        /// </summary>
        public string receiver_phone { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 总优惠金额
        /// </summary>
        public string price_off { get; set; }
        /// <summary>
        /// 当前状态1下单成功 3跑男抢单 4已到达 5已取件 6到达目的地 10收件人已收货 -1订单取消
        /// </summary>
        public string state { get; set; }
        /// <summary>
        /// 发单时间(格式2015-07-01 15:23:56)
        /// </summary>
        public string add_time { get; set; }
        /// <summary>
        /// 收货时间(格式2015-07-01 15:23:56)
        /// </summary>
        public string finish_time { get; set; }
        /// <summary>
        /// 评价星级
        /// </summary>
        public string start_level { get; set; }
        /// <summary>
        /// 评价内容
        /// </summary>
        public string comment_note { get; set; }
        /// <summary>
        /// 跑男姓名(跑男接单后)
        /// </summary>
        public string driver_name { get; set; }
        /// <summary>
        /// 跑男工号(跑男接单后)
        /// </summary>
        public string driver_jobnum { get; set; }
        /// <summary>
        /// 跑男电话(跑男接单后)
        /// </summary>
        public string driver_mobile { get; set; }
        /// <summary>
        /// 订单小类 1帮我买
        /// </summary>
        public string send_type { get; set; }
        /// <summary>
        /// 跑男的坐标
        /// </summary>
        public string driver_lastloc { get; set; }
    }
    /// <summary>
    /// 订单回调实体
    /// </summary>
    public class UUOrderCallBackResult
    {
        public string appid { get; set; }
        public string nonce_str { get; set; }
        public string sign { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string order_code { get; set; }
        /// <summary>
        /// 跑男姓名(跑男接单后)
        /// </summary>
        public string driver_name { get; set; }
        /// <summary>
        /// 跑男工号(跑男接单后)
        /// </summary>
        public string driver_jobnum { get; set; }
        /// <summary>
        /// 跑男电话(跑男接单后)
        /// </summary>
        public string driver_mobile { get; set; }
        /// <summary>
        /// 当前状态1下单成功 3跑男抢单 4已到达 5已取件 6到达目的地 10收件人已收货 -1订单取消
        /// </summary>
        public string state { get; set; }
        /// <summary>
        /// 当前状态说明
        /// </summary>
        public string state_text { get; set; }
        /// <summary>
        /// 跑男头像(跑男接单后)
        /// </summary>
        public string driver_photo { get; set; }
        public string timestamp { get; set; }
        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string origin_id { get; set; }
    }
    #endregion

    #region 用户相关
    /// <summary>
    /// UU发送验证码
    /// </summary>
    public class UUBindUserApply : UUBase
    {
        /// <summary>
        /// 用户手机号
        /// </summary>
        public string user_mobile { get; set; }
        /// <summary>
        /// 用户IP地址
        /// </summary>
        public string user_ip { get; set; }
    }
    /// <summary>
    /// UU用户详情返回结果实体
    /// </summary>
    public class UUUserDetailResult : UUBaseResult
    {
        /// <summary>
        /// 账户余额
        /// </summary>
        public string AccountMoney { get; set; }
        /// <summary>
        /// 账户冻结余额
        /// </summary>
        public string LockedMoney { get; set; }
    }
    /// <summary>
    /// UU用户绑定
    /// </summary>
    public class UUUserSubmit : UUBase
    {
        /// <summary>
        /// 用户手机号
        /// </summary>
        public string user_mobile { get; set; }
        /// <summary>
        /// 发送的短信验证码,详情见 发送短信验证码接口
        /// </summary>
        public string validate_code { get; set; }
        /// <summary>
        /// 城市名称如“郑州市”
        /// </summary>
        public string city_name { get; set; }
        /// <summary>
        /// 区名称如“金水区”
        /// </summary>
        public string county_name { get; set; }
        /// <summary>
        /// 用户IP地址
        /// </summary>
        public string reg_ip { get; set; }
    }
    #endregion
}
