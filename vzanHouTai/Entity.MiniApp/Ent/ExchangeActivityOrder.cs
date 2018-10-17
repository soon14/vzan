using Entity.Base;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 积分商城兑换订单
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ExchangeActivityOrder
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        /// 兑换商品Id
        /// </summary>
        [SqlField]
        public int ActivityId { get; set; } = 0;

        /// <summary>
        /// 用户Id
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;


        /// <summary>
        /// 对外订单号例如:201701051221
        /// </summary>
        [SqlField]
        public string OrderNum { get; set; } = string.Empty;


        /// <summary>
        ///  微信支付时候的
        /// </summary>
        [SqlField]
        public int CityMordersId { get; set; } = 0;

        // <summary>
        ///  积分兑换所需积分
        /// </summary>
        [SqlField]
        public int integral { get; set; } = 0;
        // <summary>
        ///  兑换数量
        /// </summary>
        [SqlField]
        public int BuyCount { get; set; } = 0;
        
        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr
        {
            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 支付成功时间
        /// </summary>
        [SqlField]
        public DateTime PayTime { get; set; }
        public string PayTimeStr
        {
            get
            {
                return PayTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        
        /// <summary>
        ///   0→积分兑换  1→ 积分+微信支付
        /// </summary>
        [SqlField]
        public int PayWay { get; set; } = 0;

        /// <summary>
        /// 微信支付的钱 只有积分+微信支付时候才会不等于0
        /// </summary>
        [SqlField]
        public int BuyPrice { get; set; } = 0;

        public string BuyPriceStr
        {
            get
            {

                return (BuyPrice * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 收货地址 json信息
        /// </summary>
        [SqlField]
        public string address { get; set; } = string.Empty;



        /// <summary>
        /// 兑换商品收货地址
        /// </summary>
        public string userAddress
        {
            get
            {
                if (string.IsNullOrEmpty(address))
                    return string.Empty;
                if (Way == 0)
                {
                    WxAddress addressInfo = JsonConvert.DeserializeObject<WxAddress>(address);
                    return $"姓名:{addressInfo.userName} 邮编:{addressInfo.postalCode} 电话:{addressInfo.telNumber} 地址:{addressInfo.provinceName}{addressInfo.cityName}{addressInfo.countyName}{addressInfo.detailInfo}";

                }

                ReceivingAddr receivingAddr = JsonConvert.DeserializeObject<ReceivingAddr>(address);
                string name = Way == 1 ? "消费者" : "自取人";
                return $"{name}:{receivingAddr.Name} 电话:{receivingAddr.Phone} 店铺:{receivingAddr.StoreName} 详细地址:{receivingAddr.StoreAddr}";


            }
        }


        /// <summary>
        /// 支付状态 Way=0 运送方式时候  0→未支付,1→待支付,2→待发货(支付后),3→待收货,4→交易完成
        /// 支付状态 Way=1 运送方式时候  0→未支付,1→待支付,2→待消费(支付后),3→待确认消费,4→交易完成
        /// 支付状态 Way=2 运送方式时候  0→未支付,1→待支付,2→待自取(支付后),3→待确认自取,4→交易完成
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;


        /// <summary>
        /// 运送方式 0表示快递  1表示到店消费 2表示到店自提
        /// </summary>
        [SqlField]
        public int Way { get; set; } = 0;



        public string nickName { get; set; }
        public string userLogo { get; set; }
        /// <summary>
        /// 兑换商品名称
        /// </summary>
        [SqlField]
        public string activityName { get; set; }

        /// <summary>
        /// 兑换商品图片
        /// </summary>
        [SqlField]
        public string activityImg { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string activityimg_fmt { get; set; }

        /// <summary>
        /// 商品原价
        /// </summary>
        [SqlField]
        public int originalPrice { get; set; }
        public string originalPriceStr
        {
            get
            {

                return (originalPrice * 0.01).ToString("0.00");
            }
        }

    }

    /// <summary>
    /// 到店自取收货信息
    /// </summary>
    public class ReceivingAddr
    {
        public string Name { get; set; }

        public string Phone { get; set; }

        public string StoreName { get; set; }

        public string StoreAddr { get; set; }
    }




}
