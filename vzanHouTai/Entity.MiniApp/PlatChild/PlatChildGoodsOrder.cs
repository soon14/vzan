using Entity.Base;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.PlatChild
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildGoodsOrder
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 同城订单id
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }
        /// <summary>
        /// 购买的用户
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 模板类型
        /// </summary>
        [SqlField]
        public int TemplateType { get; set; }
        /// <summary>
        /// 购买时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 购买时间
        /// </summary>
        public string AddTimeStr
        {
            get
            {
                if (AddTime != null)
                {
                    return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "";
            }
        }

        /// <summary>
        /// 支付时间
        /// </summary>
        [SqlField]
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public string PayTimeStr
        {
            get
            {
                if (PayTime != null)
                {
                    return PayTime.ToString("yyyy-MM-dd HH:mm:ss")== "0001-01-01 00:00:00"?"": PayTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "";
            }
        }

        /// <summary>
        /// 发货时间
        /// </summary>
        [SqlField]
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public string SendTimeStr
        {
            get
            {
                if (SendTime != null)
                {
                    return SendTime.ToString("yyyy-MM-dd HH:mm:ss") == "0001-01-01 00:00:00" ? "" : SendTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "";
            }
        }
        /// <summary>
        /// 收货时间
        /// </summary>
        [SqlField]
        public DateTime AcceptTime { get; set; }
        /// <summary>
        /// 收货时间
        /// </summary>
        public string AcceptTimeStr
        {
            get
            {
                if (AcceptTime != null)
                {
                    return AcceptTime.ToString("yyyy-MM-dd HH:mm:ss") == "0001-01-01 00:00:00" ? "" : AcceptTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "";
            }
        }

        /// <summary>
        /// 退款时间
        /// </summary>
        [SqlField]
        public DateTime RefundTime { get; set; }
        /// <summary>
        /// 订单状态（QiyeOrderState）
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        public string StateStr
        {
            get
            {
                return Enum.GetName(typeof(PlatChildOrderState), State);
            }
        }
        
        /// <summary>
        /// 配送模板ID
        /// 0表示到店自提
        /// </summary>
        [SqlField]
        public int FreightTemplateId { get; set; } = 0;

        /// <summary>
        /// 配送模板名称
        /// </summary>
        [SqlField]
        public string FreightTemplateName { get; set; } = "";


        /// <summary>
        /// 地址ID
        /// </summary>
        [SqlField]
        public int AddressId { get; set; } = 0;

        /// <summary>
        /// 提货人姓名
        /// </summary>
        [SqlField]
        public string AccepterName { get; set; } = "";

        /// <summary>
        /// 提货人提货人手机号码
        /// </summary>
        [SqlField]
        public string AccepterTelePhone { get; set; } = "";

        /// <summary>
        /// 店铺id 
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 买家留言
        /// </summary>
        [SqlField]
        public string Message { get; set; } = "";

        /// <summary>
        /// 对外订单号
        /// </summary>
        [SqlField]
        public string OrderNum { get; set; } = "";

        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = "";


        /// <summary>
        /// 详细地址
        /// </summary>
        [SqlField]
        public string Address { get; set; } = "";
        /// <summary>
        /// 邮政编码
        /// </summary>
        [SqlField]
        public string ZipCode { get; set; } = "";


        /// <summary>
        /// 取物号
        /// </summary>
        [SqlField]
        public string LadingCode { get; set; } = "";

        /// <summary>
        /// 订单内商品数量
        /// </summary>
        [SqlField]
        public int QtyCount { get; set; }

        /// <summary>
        /// 运费金额
        /// </summary>
        [SqlField]
        public int FreightPrice { get; set; }

        public string FreightPriceStr
        {
            get
            {
                return (FreightPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 购买价格
        /// </summary>
        [SqlField]
        public int BuyPrice { get; set; }

        public string BuyPriceStr { get { return (BuyPrice * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 会员优惠金额
        /// </summary>
        [SqlField]
        public int VipReducedPrice { get; set; }
        /// <summary>
        /// 优惠总金额
        /// </summary>
        [SqlField]
        public int ReducedPrice { get; set; }
        public string ReducedPriceStr
        {
            get
            {
                return (ReducedPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 优惠券优惠金额
        /// </summary>
        [SqlField]
        public int CouponPrice { get; set; }
        public string CouponPriceStr
        {
            get
            {
                return (CouponPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 总价
        /// </summary>
        [SqlField]
        public int SumPrice { get; set; }
        public string SumPriceStr
        {
            get
            {
                return (SumPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 支付方式
        /// </summary>
        [SqlField]
        public int BuyMode { get; set; } = 0;

        public string BuyModeStr
        {
            get
            {
                return Enum.GetName(typeof(miniAppBuyMode), BuyMode);
            }
        }

        /// <summary>
        /// 配送方式 到店自取 = 0, 商家配送 = 1 （多门店见此枚举：multiStoreOrderType）
        /// </summary>
        [SqlField]
        public int GetWay { get; set; }

        public string GetWayStr
        {
            get
            {
                return Enum.GetName(typeof(miniAppOrderGetWay), GetWay);
            }
        }

        /// <summary>
        /// 打印的单据ID
        /// </summary>
        [SqlField]
        public int PrintId { get; set; }

        /// <summary>
        ///  -1为异常  0未打印 1 为成功
        /// </summary>
        [SqlField]
        public int PrintSuccess { get; set; } = 0;

        /// <summary>
        /// 申请退款原因
        /// </summary>
        [SqlField]
        public string RefundRemark { get; set; } = "";

        /// <summary>
        /// 权限表Id
        /// </summary>
        [SqlField]
        public int AId { get; set; }
        
        /// <summary>
        /// 小程序appid
        /// </summary>
        [SqlField]
        public string AppId { get; set; } = "";
        
        /// <summary>
        /// 是否已评论
        /// </summary>
        [SqlField]
        public bool IsCommentting { get; set; } = false;
        /// <summary>
        /// 购物车ID，格式：1，2，3
        /// </summary>
        public string CartIds { get; set; }
        /// <summary>
        /// 用户领取优惠券ID
        /// </summary>
        public int CouponLogId { get; set; }
        /// <summary>
        /// 地址信息
        /// </summary>
        public string WxAddresJson { get; set; }
        /// <summary>
        /// 商品
        /// </summary>
        public PlatChildGoods Goods { get; set; }
        /// <summary>
        /// 购物车
        /// </summary>
        public List<PlatChildGoodsCart> CartList { get; set; }
        /// <summary>
        /// 物流信息
        /// </summary>
        public DeliveryFeedback DeliveryInfo { get; set; }
        /// <summary>
        /// 店铺客户电话
        /// </summary>
        public string StorePhone { get; set; }
        public string StoreName { get; set; }
    }
}
