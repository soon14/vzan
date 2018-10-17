using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity.MiniApp.Pin;
using Entity.MiniApp;

namespace User.MiniApp.Areas.Pin.Models
{
    public class PayRecordModel
    {
        public int CityMordersId { get; set; } = 0;

        /// <summary>
        /// 微信支付订单号
        /// </summary>
        public string transaction_id { get; set; } = string.Empty;

        /// <summary>
        /// 商户订单号
        /// </summary>
        public string out_trade_no { get; set; } = string.Empty;

        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime time_end { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public int total_fee { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string ShowNote { get; set; } = string.Empty;

        /// <summary>
        /// 0=商品，1=代理费
        /// </summary>
        public int OrderType { get; set; } = 0;

        /// <summary>
        /// 店铺ID
        /// </summary>
        public int TuserId { get; set; } = 0;

        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// 店铺电话
        /// </summary>
        public string StoreUserPhone { get; set; } = string.Empty;

        /// <summary>
        /// 付款用户ID
        /// </summary>
        public int PayUserId { get; set; } = 0;

        /// <summary>
        /// 付款用户昵称
        /// </summary>
        public string PayUserName { get; set; } = string.Empty;

        /// <summary>
        /// 付款用户电话
        /// </summary>
        public string PayUserPhone { get; set; } = string.Empty;

        /// <summary>
        /// 店铺所有者ID
        /// </summary>
        public int StoreUserId { get; set; } = 0;

        /// <summary>
        /// 店铺所有者昵称
        /// </summary>
        public string StoreUserName { get; set; } = string.Empty;

        /// <summary>
        /// 订单产品
        /// </summary>
        public PinGoods OrderGoods { get; set; } = new PinGoods();

        /// <summary>
        /// 产品名称
        /// </summary>
        public string GoodsName { get; set; } = string.Empty;

        /// <summary>
        /// 产品ID
        /// </summary>
        public int GoodsId { get; set; } = 0;

        public string GoodsImg { get; set; } = string.Empty;


        /// <summary>
        /// pingoodsorder表的goodsid,如果是代理费 这个字段保存的是pinagent的id
        /// </summary>
        public int GoodsOrderGoodsId { get; set; } = 0;
        /// <summary>
        /// 父级代理用户
        /// </summary>
        public C_UserInfo ParentAgentUser { get; set; }
        /// <summary>
        /// 父级代理的店铺
        /// </summary>
        public PinStore ParentAgentStore { get; set; }
    }
}