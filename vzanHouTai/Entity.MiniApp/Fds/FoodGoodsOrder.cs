using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsOrder
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
        /// 购买价格
        /// </summary>
        [SqlField]
        public int BuyPrice { get; set; }

        public string BuyPriceStr { get { return (BuyPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 商品GUID
        /// </summary>
        [SqlField]
        public string GoodsGuid { get; set; } = string.Empty;
        /// <summary>
        /// 购买的用户
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 购买时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 购买时间
        /// </summary>
        public string CreateDateStr
        {
            get
            {
                if (CreateDate != null)
                {
                    return CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "0001-01-01 00:00:00";
            }
        }

        /// <summary>
        /// 接单时间
        /// </summary>
        [SqlField]
        public DateTime ConfDate { get; set; }

        /// <summary>
        /// 接单时间
        /// </summary>
        public string ConfDateStr
        {
            get
            {
                if (ConfDate != null)
                {
                    return ConfDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "0001-01-01 00:00:00";
            }
        }

        /// <summary>
        /// 发货时间
        /// </summary>
        [SqlField]
        public DateTime DistributeDate { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public string DistributeDateStr
        {
            get
            {
                if (DistributeDate != null)
                {
                    return DistributeDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "0001-01-01 00:00:00";
            }
        }
        /// <summary>
        /// 收货时间
        /// </summary>
        [SqlField]
        public DateTime AcceptDate { get; set; }
        /// <summary>
        /// 收货时间
        /// </summary>
        public string AcceptDateStr
        {
            get
            {
                if (AcceptDate != null)
                {
                    return AcceptDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "0001-01-01 00:00:00";
            }
        }
        /// <summary>
        /// 订单状态    取消订单 = -1,未付款 = 0,待核销 = 1,已核销 = 2,待发货 = 3,待收货 = 4,已收货 = 5,
        /// C_Enums.OrderState
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 配送模板ID
        /// 0表示到店自提
        /// </summary>
        [SqlField]
        public int FreightTemplateId { get; set; } = 0;

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
        /// 6位提货码
        /// </summary>
        [SqlField]
        public string VerificationNum { get; set; } = "";

        /// <summary>
        /// 提货人提货人手机号码
        /// </summary>
        [SqlField]
        public string AccepterTelePhone { get; set; } = "";

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

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
        /// 运费金额
        /// </summary>
        [SqlField]
        public int FreightPrice { get; set; }

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
        /// 支付时间
        /// </summary>
        [SqlField]
        public DateTime PayDate { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public string PayDateStr
        {
            get
            {
                if (PayDate != null)
                {
                    return PayDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                return "0001-01-01 00:00:00";
            }
        }

        /// <summary>
        /// 订单类型 0 点餐 / 1 外卖
        /// </summary>
        [SqlField]
        public int OrderType { get; set; } = 0;

        /// <summary>
        /// 订单类型 0 点餐 / 1 外卖
        /// </summary>
        public string OrderTypeStr
        {
            get
            {
                return OrderType == 1 ? "外卖" : "堂食";
            }
        }

        /// <summary>
        /// 桌台号
        /// </summary>
        [SqlField]
        public int TablesNo { get; set; }
        public string TableName { get; set; } = string.Empty;
        /// <summary>
        /// 订单内商品数量
        /// </summary>
        [SqlField]
        public int QtyCount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        [SqlField]
        public int ReducedPrice { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        [SqlField]
        public int BuyMode { get; set; } = 0;

        /// <summary>
        /// 配送方式
        /// </summary>
        [SqlField]
        public int GetWay { get; set; }

        /// <summary>
        /// 打印的单据ID
        /// </summary>
        [SqlField]
        public string PrintId { get; set; } = "";

        /// <summary>
        ///  -1为异常  0未打印 1 为成功
        /// </summary>
        [SqlField]
        public int PrintSuccess { get; set; } = 0;

        /// <summary>
        /// 退款时间
        /// </summary>
        [SqlField]
        public DateTime outOrderDate { get; set; }

        /// <summary>
        /// 上次的状态
        /// </summary>
        [SqlField]
        public int lastState { get; set; }


        ///// <summary>
        ///// 申请退款原因
        ///// </summary>
        //[SqlField]
        //public string drawBackRemark { get; set; }

        #region 拼团
        /// <summary>
        /// 拼团ID
        /// </summary>
        [SqlField]
        public int GroupId { get; set; } = 0;
        public int GroupState { get; set; } = 0;
        /// <summary>
        /// 0：不是团产品，1：是团产品
        /// </summary>
        [SqlField]
        public int GoodType { get; set; } = 0;
        #endregion
        /// <summary>
        /// 预定ID
        /// </summary>
        [SqlField]
        public int ReserveId { get; set; }

        #region 达达物流
        /// <summary>
        /// 达达物流订单状态
        /// </summary>
        public int dadastate { get; set; }
        ///// <summary>
        ///// 0:没有配送物流，1：达达配送，2：蜂鸟配送
        ///// </summary>
        //[SqlField]
        //public int DistributionType { get; set; } = 0;
        public string DadaOrderStateStr
        {
            get; set;
        }
        public string sourceid
        {
            get; set;
        }
        public string dadaorderid
        {
            get; set;
        }
        /// <summary>
        /// 达达运费
        /// </summary>
        public double dadafreightPrice
        {
            get; set;
        }
        #endregion

        /// <summary>
        /// 是否自动接单
        /// </summary>
        public int AutoAcceptOrder { get; set; }

        /// <summary>
        /// 餐盒费
        /// </summary>
        [SqlField]
        public int PackinPrice { get; set; }
        public string PcakinPriceStr
        {
            get
            {
                return (PackinPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 附加属性json字符串
        /// </summary>
        [SqlField]
        public string attribute { get; set; } = "";
        /// <summary>
        /// 优惠券优惠价格
        /// </summary>
        [SqlField]
        public int CouponPrice { get; set; } = 0;
        /// <summary>
        /// 附加属性
        /// </summary>
        public FoodGoodsOrderAttr attrbuteModel { get; set; } = new FoodGoodsOrderAttr();
    }

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodAdminGoodsOrder : FoodGoodsOrder
    {
        [SqlField]
        public string NickName { get; set; } = "";

        [SqlField]
        public string HeadImgUrl { get; set; } = "";
    }

    public class FoodOrderCardDetail
    {
        public int Id { get; set; }
        public string GoodsName { get; set; }
        public string ImgUrl { get; set; }
        public string SpecInfo { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
    }

    public class FoodGoodsOrderAttr
    {
        public bool isNewTableNo { get; set; } = false;
    }

}
