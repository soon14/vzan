using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.MiniApp.Models.Pin
{
    /// <summary>
    /// 创建订单请求参数
    /// </summary>
    public class PinOrderModel
    {
        /// <summary>
        /// 小程序aid
        /// </summary>
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "参数错误 aid error")]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 店铺id
        /// </summary>
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "参数错误 storeId error")]
        public int storeId { get; set; } = 0;
        /// <summary>
        /// 拼团id 如果没有可不传
        /// </summary>
        public int groupId { get; set; } = 0;
        /// <summary>
        /// 所选商品id
        /// </summary>
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "参数错误 goodsId error")]
        public int goodsId { get; set; } = 0;
        /// <summary>
        /// 所选规格id，如果没有规格则不传值
        /// </summary>
        public string specificationId { get; set; } = string.Empty;
        /// <summary>
        /// 购买数量
        /// </summary>
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "商品数量不能为零")]
        public int count { get; set; } = 0;
        /// <summary>
        /// 配送方式  0：商家配送    1：到店自取    2：面对面交易
        /// </summary>
        public int sendway { get; set; } = 0;

        /// <summary>
        /// 支付方式    0：微信支付  1：余额支付  2：线下支付（暂时只有微信支付）
        /// </summary>
        public int payway { get; set; } = 0;
        /// <summary>
        /// 收件人
        /// </summary>

        public string consignee { get; set; } = string.Empty;
        /// <summary>
        /// 收件人联系方式
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string address { get; set; } = string.Empty;
        /// <summary>
        /// 收货地址ID
        /// </summary>
        public int addressId { get; set; } = 0;
        /// <summary>
        /// 买家留言
        /// </summary>
        public string buyerRemark { get; set; } = string.Empty;

        public int sourceType { get; set; } = 0;
    }
}