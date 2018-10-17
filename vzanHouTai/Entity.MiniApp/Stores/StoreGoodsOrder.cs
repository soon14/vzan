using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Stores
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreGoodsOrder
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
        /// <summary>
        /// 购买价格
        /// </summary>
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
        /// -- 0表示到店自提 提示作废
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
        public string AccepterName { get; set; }

        /// <summary>
        /// 提货人提货人手机号码
        /// </summary>
        [SqlField]
        public string AccepterTelePhone { get; set; }

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 买家留言
        /// </summary>
        [SqlField]
        public string Message { get; set; }

        /// <summary>
        /// 对外订单号
        /// </summary>
        [SqlField]
        public string OrderNum { get; set; }

        /// <summary>
        /// 卖家备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; }

        /// <summary>
        /// 运费金额
        /// </summary>
        [SqlField]
        public int FreightPrice { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        [SqlField]
        public string ZipCode { get; set; }

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
        /// 支付方式 默认为1(微信支付)
        /// </summary>
        [SqlField]
        public int buyMode { get; set; } = 1;

    }

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreAdminGoodsOrder
    {
        [SqlField]
        public int Id { get; set; }
        [SqlField]
        public int OrderId { get; set; }
        [SqlField]
        public string OrderNum { get; set; }
        [SqlField]
        public int BuyPrice { get; set; }
        [SqlField]
        public int UserId { get; set; }
        [SqlField]
        public string NickName { get; set; }
        [SqlField]
        public string TelePhone { get; set; }
        [SqlField]
        public string Message { get; set; }
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public string Remark { get; set; }
        [SqlField]
        public int FreightPrice { get; set; }
        [SqlField]
        public string Address { get; set; }

        [SqlField]
        public int buyMode { get; set; }

        [SqlField]
        public string userName { get; set; }

        [SqlField]
        public string levelname { get; set; }

        [SqlField]
        public int count { get; set; }

        [SqlField]
        public string goodsName { get;set; }

        [SqlField]
        public int price { get; set; } = 0;

        public string priceStr { get { return (price * 0.01).ToString("0.00"); } }

        public List<StoreOrderCardDetail> GoodsList { get; set; }
    }

    public class StoreOrderCardDetail
    {
        public int Id { get; set; }
        public string GoodsName { get; set; }
        public string ImgUrl { get; set; }
        public string SpecInfo { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
    }
}
