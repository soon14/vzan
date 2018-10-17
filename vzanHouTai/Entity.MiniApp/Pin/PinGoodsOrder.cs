using Entity.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Newtonsoft.Json;

namespace Entity.MiniApp.Pin
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinGoodsOrder
    {
        /// <summary>
        /// id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; } = 0;

        [SqlField]
        public int aid { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;
        public string storeName { get; set; } = string.Empty;
        [SqlField]
        public int userId { get; set; } = 0;
        /// <summary>
        /// 订单来源 0：平台交易 ， 1：店内扫码
        /// </summary>
        [SqlField]
        public int sourceType { get; set; } = 0;
        /// <summary>
        /// 订单类型 0：商家订单 1：平台订单（代理费）
        /// </summary>
        [SqlField]
        public int orderType { get; set; } = 0;
        /// <summary>
        /// 对外订单号
        /// </summary>
        [SqlField]
        public string outTradeNo { get; set; } = string.Empty;

        /// <summary>
        /// 提货码 根据每个店铺订单递增的6位数
        /// </summary>
        [SqlField]
        public int receivingNo { get; set; } = 0;

        /// <summary>
        /// 拼团id
        /// </summary>
        [SqlField]
        public int groupId { get; set; } = 0;

        /// <summary>
        /// 商品id|代理id
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "参数错误，goodsid error")]
        [SqlField]
        public int goodsId { get; set; } = 0;

        /// <summary>
        /// 规格id
        /// </summary>
        [SqlField]
        public string specificationId { get; set; } = string.Empty;
        /// <summary>
        /// 运费模板id
        /// </summary>
        [SqlField]
        public int freightId { get; set; } = 0;

        /// <summary>
        /// 微信支付订单号
        /// </summary>
        [SqlField]
        public int payNo { get; set; } = 0;

        /// <summary>
        /// 商品总额
        /// </summary>
        [SqlField]
        public int price { get; set; } = 0;

        public string priceStr
        {
            get
            {
                return (price * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 运费
        /// </summary>
        [SqlField]
        public int freight { get; set; } = 0;

        public string freightStr
        {
            get
            {
                return (freight * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 实付金额
        /// </summary>
        [SqlField]
        public int money { get; set; } = 0;

        public string moneyStr
        {
            get
            {
                return (money * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 返利金额
        /// </summary>
        [SqlField]
        public int returnMoney { get; set; }

        public string returnMoneyStr
        {
            get
            {
                return (returnMoney * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 购买数量
        /// </summary>
        [SqlField]
        public int count { get; set; } = 0;

        /// <summary>
        /// 订单状态 见PinEnums-PinOrderState
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        public string stateStr
        {
            get
            {
                return Enum.GetName(typeof(PinEnums.PinOrderState), state);
            }
        }
        /// <summary>
        /// 付款状态 见PinEnums-PayState
        /// </summary>
        [SqlField]
        public int payState { get; set; } = 0;

        public string payStateStr
        {
            get
            {
                return Enum.GetName(typeof(Pin.PinEnums.PayState), payState);
            }
        }
        /// <summary>
        /// 配送方式  枚举PinEnums.SendWay  
        /// 商家配送 = 0,到店自取 = 1, 面对面交易 = 2
        /// </summary>
        [SqlField]
        public int sendway { get; set; } = 0;
        public string sendwayStr
        {
            get
            {
                return Enum.GetName(typeof(PinEnums.SendWay), sendway);
            }
        }
        /// <summary>
        /// 支付方式 见PinEnums-PayWay
        /// </summary>
        [SqlField]
        public int payway { get; set; } = 0;
        public string paywayStr
        {
            get
            {
                return Enum.GetName(typeof(PinEnums.PayWay), payway);
            }
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        public string addtimeStr
        {
            get
            {
                return addtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 支付时间
        /// </summary>
        [SqlField]
        public DateTime paytime { get; set; } = DateTime.Now;

        public string paytimeStr
        {
            get
            {
                return paytime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        ///送货时间
        /// </summary>
        [SqlField]
        public DateTime sendtime { get; set; } = DateTime.Now;

        public string sendtimeStr
        {
            get
            {
                return sendtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        ///收货时间
        /// </summary>
        [SqlField]
        public DateTime receivingtime { get; set; } = DateTime.Now;

        public string receivingtimeStr
        {
            get
            {
                return receivingtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 收件人
        /// </summary>
        [SqlField]
        public string consignee { get; set; } = string.Empty;

        /// <summary>
        /// 收件人联系方式
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 收货地址
        /// </summary>
        [SqlField]
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// 买家留言
        /// </summary>
        [SqlField]
        public string buyerRemark { get; set; } = string.Empty;

        /// <summary>
        /// 卖家留言
        /// </summary>
        [SqlField]
        public string sellerRemark { get; set; } = string.Empty;
        /// <summary>
        /// 日志说明
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 下单商品快照
        /// </summary>
        [SqlField]
        public string goodsPhoto { get; set; } = string.Empty;


        /// <summary>
        /// 下单商品快照类，用于显示
        /// </summary>
        public PinGoods goodsPhotoModel
        {
            get
            {
                if (!string.IsNullOrEmpty(goodsPhoto))
                    return JsonConvert.DeserializeObject<PinGoods>(goodsPhoto);
                else
                    return null;
            }
        }

        /// <summary>
        /// 已选规格快照
        /// </summary>
        [SqlField]
        public string specificationPhoto { get; set; } = string.Empty;

        /// <summary>
        /// 已选规格快照类，用于显示
        /// </summary>
        public SpecificationDetailModel specificationPhotoModel
        {
            get
            {
                if (!string.IsNullOrEmpty(specificationPhoto))
                    return JsonConvert.DeserializeObject<SpecificationDetailModel>(specificationPhoto);
                else
                    return null;
            }
        }

        public PinGroup groupInfo { get; set; } = null;
        /// <summary>
        /// 是否已返利 0:否  1：是
        /// </summary>
        [SqlField]
        public int isReturnMoney { get; set; } = 0;


        /// <summary>
        /// 物流信息
        /// 
        /// </summary>
        /*
         {
                ContactName: thisVue.contactName, 
                ContactTel: thisVue.contactTel, 
                CompanyCode: selectDelvery.Code, 
                CompanyTitle: selectDelvery.Title, 
                DeliveryNo: thisVue.deliveryNo, 
                Address: thisVue.address,
                selfDelivery: thisVue.selfDelivery, 
                Remark: thisVue.mark
         }
        */
        [SqlField]
        public string attachData { get; set; } = string.Empty;
        /// <summary>
        /// 订单状态加付款状态
        /// </summary>
        public string lastState
        {
            get
            {
                return GetLastState();
            }
        }
        private string GetLastState()
        {
            if (payState == (int)PinEnums.PayState.已退款)
            {
                return payStateStr;
            }
            else
            {
                return stateStr;
            }
        }

        public OrderAttachData GetAttachData()
        {
            return !string.IsNullOrWhiteSpace(attachData) ? JsonConvert.DeserializeObject<OrderAttachData>(attachData) : null;
        }
        /// <summary>
        /// 退款金额
        /// </summary>
        [SqlField]
        public int refundMoney { get; set; } = 0;
        /// <summary>
        /// 退货数量
        /// </summary>
        [SqlField]
        public int returnCount { get; set; } = 0;
    }

    public class OrderAttachData
    {
        public string ContactName { get; set; } = string.Empty;
        public string ContactTel { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;
        public string CompanyTitle { get; set; } = string.Empty;
        public string DeliveryNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool selfDelivery { get; set; } = false;
        public string Remark { get; set; } = string.Empty;
        public string FreightInfo { get; set; }
    }
}