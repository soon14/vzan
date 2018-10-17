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
    /// 快跑者创建订单表单
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class UUOrder : UUBase
    {
        public UUOrder()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="timestamp"></param>
        /// <param name="openId"></param>
        /// <param name="price_token"></param>
        /// <param name="order_price">o</param>
        /// <param name="balance_paymoney">need_payprice</param>
        /// <param name="receiver">收件人姓名</param>
        /// <param name="receiver_phone">收件人电话</param>
        /// <param name="note">备注</param>
        /// <param name="callback_url">订单回调链接</param>
        /// <param name="aid"></param>
        /// <param name="storeId">店铺id</param>
        /// <param name="templateType">模板类型（TmpType）</param>
        /// <param name="orderType">订单类型（EntGoodsType）</param>
        /// <param name="orderId">订单ID</param>
        public UUOrder(string appid,string timestamp,string openId,string price_token,string order_price,string balance_paymoney,string receiver,string receiver_phone,string note,string callback_url,int aid,int storeId,int templateType,int orderType,int orderId,string orderNum)
        {
            this.appid = appid;
            this.nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            this.timestamp = timestamp;
            this.openid = openId;
            this.price_token = price_token;
            this.order_price = order_price;
            this.balance_paymoney = balance_paymoney;
            this.receiver = receiver;
            this.receiver_phone = receiver_phone;
            this.note = "测试订单，勿接";
            this.callback_url = callback_url;
            this.push_type = "0";
            this.special_type = "0";
            this.callme_withtake = "0";
            this.Aid = aid;
            this.StoreId = storeId;
            this.TemplateType = templateType;
            this.OrderType = orderType;
            this.OrderId = orderId;
            this.OrderNum = orderNum;
            this.UpdateTime = DateTime.Now;
        }

        [SqlField(IsPrimaryKey = true, IsAutoId = true), NoJoinField]
        public int Id { get; set; }
        /// <summary>
        /// uu平台appid
        /// </summary>
        [SqlField]
        public new string appid { get; set; }
        /// <summary>
        /// 商户OpenId
        /// </summary>
        [SqlField]
        public new string openid { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        [SqlField]
        public new string order_code { get; set; }
        /// <summary>
        /// 金额令牌，计算订单价格接口返回的price_token
        /// </summary>
        [SqlField]
        public string price_token { get; set; }
        /// <summary>
        /// 订单金额，计算订单价格接口返回的total_money
        /// </summary>
        [SqlField]
        public string order_price { get; set; }
        /// <summary>
        /// 实际余额支付金额计算订单价格接口返回的need_paymoney
        /// </summary>
        [SqlField]
        public string balance_paymoney { get; set; }
        /// <summary>
        /// 收件人
        /// </summary>
        [SqlField]
        public string receiver { get; set; }
        /// <summary>
        /// 收件人电话 手机号码； 虚拟号码格式（手机号_分机号码）例如：13700000000_1111
        /// </summary>
        [SqlField]
        public string receiver_phone { get; set; }
        /// <summary>
        /// 订单备注 最长140个汉字
        /// </summary>
        [SqlField]
        public string note { get; set; }
        /// <summary>
        /// 订单提交成功后及状态变化的回调地址
        /// </summary>
        [SqlField]
        public string callback_url { get; set; }
        /// <summary>
        /// 推送方式（0 开放订单，1指定跑男，2商户绑定的跑男）默认传0即可
        /// </summary>
        [SqlField]
        public string push_type { get; set; }
        /// <summary>
        /// 推送跑男的手机号，push_type为0这里就传空字符串
        /// </summary>
        [SqlField]
        public string push_str { get; set; }
        /// <summary>
        /// 特殊处理类型，是否需要保温箱 1需要 0不需要
        /// </summary>
        [SqlField]
        public string special_type { get; set; }
        /// <summary>
        /// 取件是否给我打电话 1需要 0不需要
        /// </summary>
        [SqlField]
        public string callme_withtake { get; set; }
        /// <summary>
        /// 发件人电话，（如果为空则是用户注册的手机号）
        /// </summary>
        [SqlField]
        public string pubUserMobile { get; set; }
        /// <summary>
        /// 点赞平台订单号
        /// </summary>
        [SqlField, NoJoinField]
        public string OrderNum { get; set; }
        [SqlField, NoJoinField]
        public int Aid { get; set; }
        [SqlField, NoJoinField]
        public int StoreId { get; set; }
        [SqlField, NoJoinField]
        public int OrderId { get; set; }
        [SqlField, NoJoinField]
        public int TemplateType { get; set; }
        [SqlField, NoJoinField]
        public int OrderType { get; set; }
        [SqlField, NoJoinField]
        public int State { get; set; }
        /// <summary>
        /// 跑男姓名(跑男接单后)
        /// </summary>
        [SqlField, NoJoinField]
        public string driver_name { get; set; }
        /// <summary>
        /// 跑男工号(跑男接单后)
        /// </summary>
        [SqlField, NoJoinField]
        public string driver_jobnum { get; set; }
        /// <summary>
        /// 跑男电话(跑男接单后)
        /// </summary>
        [SqlField, NoJoinField]
        public string driver_mobile { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField, NoJoinField]
        public DateTime UpdateTime { get; set; }
    }

    /// <summary>
    /// 不参与签名
    /// </summary>
    public class NoJoinField: Attribute
    {
        public NoJoinField() { }
    }
}
