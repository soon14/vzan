using Entity.Base;
using Entity.MiniApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class BargainUser
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 砍价商品id 商品id
        /// </summary>
        [SqlField]
        public int BId { get; set; }
        /// <summary>
        /// 名称标题
        /// </summary>
        [SqlField]
        public string BName { get; set; }
        /// <summary>
        /// 领取人id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 当前价格
        /// </summary>
        [SqlField]
        public int CurrentPrice { get; set; }

        /// <summary>
        /// 当前价 单位 元
        /// </summary>
        public string CurrentPriceStr { get { return (CurrentPrice * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 帮砍人数
        /// </summary>
        [SqlField]
        public int HelpNum { get; set; }
        /// <summary>
        /// 支付购买时间
        /// </summary>
        [SqlField]
        public DateTime BuyTime { get; set; } = Convert.ToDateTime("3000-01-01 01:01:01");

        /// <summary>
        /// 支付购买时间 字符串类型
        /// </summary>
        public string BuyTimeStr {
            get
            {
                if (BuyTime == Convert.ToDateTime("3000-01-01 01:01:01"))
                    return string.Empty;
                return BuyTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            set
            {

            }
        }

        /// <summary>
        /// 生成支付订单时间 字符串类型
        /// </summary>
        public string CreatCityModerTimeStr { get; set; } = string.Empty;

        /// <summary>
        /// 订单的ID，对外展示的
        /// </summary>
        [SqlField]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 订单的ID，CityMorders 表Id
        /// </summary>
        [SqlField]
        public int CityMordersId { get; set; } = 0;

        

        /// <summary>
        /// 退款时间
        /// </summary>
        [SqlField]
        public DateTime outOrderDate { get; set; }
        

        /// <summary>
        /// 开始时间
        /// </summary>
        [SqlField]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;


        public string CreateDateStr
        {
            get
            {
                return CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// -1 已经关闭,0未付款，1已购买,2退款中,3退款成功,4退款失败,5 继续支付(已经生成订单Id),6待收货,7待发货,8交易成功
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;    

        public string StateStr
        {
            get
            {
                string str = "未知";
                switch (State)
                {
                    case -1:
                        str = "已关闭";
                        break;
                    case 0:
                    case 5:
                        str = "未付款";
                        break;
                    case 1:
                        str = "已付款";
                        break;
                    case 2:
                        str = "退款中";
                        break;
                    case 3:
                        str = "退款成功";
                        break;
                    case 4:
                        str = "退款失败";
                        break;
                    case 6:
                        str = "待收货";
                        break;
                    case 7:
                        str = "待发货";
                        if (GetWay == 1)
                        {
                            str = "待自取";
                        }
                        if (GetWay == 2)
                        {
                            str = "待消费";
                        }
                        break;
                    case 8:
                        str = "交易成功";
                        break;

                }
                return str;
            }
        }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; }
        /// <summary>
        /// 是否已发送消息提醒 结束前1小时提醒
        /// </summary>
        [SqlField]
        public int SendInfo { get; set; }
        /// <summary>
        /// 是否已发送消息提醒 过期使用剩余24小时提醒
        /// </summary>
        [SqlField]
        public int SendInfo2 { get; set; }
        /// <summary>
        /// 收货地址Id  旧的不用了
        /// </summary>
        [SqlField]
        public int WxAddressId { get; set; } = 0;


        /// <summary>
        /// 收货地址
        /// </summary>
        [SqlField]
        public string Address { get; set; } = string.Empty;


        /// <summary>
        /// 收货人
        /// </summary>
        public string AddressUserName
        {
            get
            {
                if (string.IsNullOrEmpty(Address))
                    return string.Empty;
                WxAddress address = JsonConvert.DeserializeObject<WxAddress>(Address);
                return address.userName;
            }
        }



        /// <summary>
        /// 收货电话
        /// </summary>
        public string TelNumber
        {
            get
            {
                if (string.IsNullOrEmpty(Address))
                    return string.Empty;
                WxAddress address = JsonConvert.DeserializeObject<WxAddress>(Address);
                return address.telNumber;
            }
        }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string AddressDetail
        {
            get
            {
                if (string.IsNullOrEmpty(Address))
                    return string.Empty;
                WxAddress address = JsonConvert.DeserializeObject<WxAddress>(Address);
                return $"{address.provinceName} {address.cityName} {address.countyName} {address.detailInfo}";
            }
        }



        /// <summary>
        /// 1表示微信支付 2表示储值支付
        /// </summary>
        [SqlField]
        public int PayType { get; set; } = 1;

        public string PayTypeStr
        {
            get
            {
                return PayType == 1 ? "微信支付" : "储值支付";
            }
        }
        /// <summary>
        /// 生成支付订单时间
        /// </summary>
        [SqlField]
        public DateTime CreateOrderTime { get; set; }
        public string CreateOrderTimeStr
        {
            get
            {
                if (CreateOrderTime == Convert.ToDateTime("0001-01-01 00:00:00"))
                    return string.Empty;
                return CreateOrderTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 发货时间
        /// </summary>
        [SqlField]
        public DateTime SendGoodsTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 快递名称
        /// </summary>
        [SqlField]
        public string SendGoodsName { get; set; } = string.Empty;

        /// <summary>
        /// 快递单号
        /// </summary>
        [SqlField]
        public string WayBillNo { get; set; } = string.Empty;


        /// <summary>
        /// 0表示快递配送 1表示到店自取 2到店消费
        /// </summary>
        [SqlField]
        public int GetWay { get; set; }

        /// <summary>
        /// 到店自提或者到店消费门店名称
        /// </summary>
        [SqlField]
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// 砍价留言
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;


        //View 显示数据
        public string OriginalPrice { get; set; }
        public string FloorPrice { get; set; }
       /// <summary>
       /// 
       /// </summary>
        public int RemainNum { get; set; }
        /// <summary>
        /// 用户微信头像
        /// </summary>
        public string ShopLogoUrl { get; set; } = string.Empty;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; }


        /// <summary>
        /// 运费 单位 元
        /// </summary>
        public string GoodsFreightStr { get; set; } = string.Empty;

        /// <summary>
        /// 砍价商品主图
        /// </summary>
        public string ImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// /// <summary>
        /// 是否结束 对应砍价商品已过期或者剩余数量为0
        /// </summary>
        /// </summary>
        public int IsEnd { get; set; } = 0;

        /// <summary>
        /// 确认收货时间
        /// </summary>
        [SqlField]
        public DateTime ConfirmReceiveGoodsTime { get; set; } = DateTime.MinValue;

        [SqlField]
        public int aid { get; set; }
        
        /// <summary>
        /// 会员级别
        /// </summary>
        public string VipLeve { get; set; } = "未知";

        /// <summary>
        /// 是否已评论
        /// </summary>
        [SqlField]
        public bool IsCommentting { get; set; } = false;

        [SqlField]
        public int FreightFee { get; set; } = 0;

        public string PayAmount { get { return ((CurrentPrice + FreightFee) * 0.01).ToString("F2"); } }
        /// <summary>
        /// 退款金额
        /// </summary>
        [SqlField]
        public int refundFee { get; set; } = 0;
        public string refundFeeStr
        {
            get
            {
                return (refundFee * 0.01).ToString("0.00");
            }
        }
    }
}
