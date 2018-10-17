using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntGoodsOrder
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
        public int OrderId { get; set; } = 0;

        /// <summary>
        /// 购买价格
        /// </summary>
        [SqlField]
        public int BuyPrice { get; set; } = 0;

        public string BuyPriceStr { get { return (BuyPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 产品价格+优惠+运费
        /// </summary>
        public string GoodsMoney
        {
            get
            {
                return ((BuyPrice + ReducedPrice) * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 除去运费的商品原价
        /// </summary>
        public string OnlyGoodsMoney
        {
            get
            {
                return ((BuyPrice + ReducedPrice - FreightPrice) * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 商品GUID 用户存放已购买商品的id，便于做数据检索
        /// </summary>
        [SqlField]
        public string GoodsGuid { get; set; } = string.Empty;

        /// <summary>
        /// 购买的用户
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;

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

        public string StateStr
        {
            get
            {
                if (OrderType == 3 && GroupState == (int)MiniApp.GroupState.开团成功)
                {
                    return "拼团中";
                }
                else if (OrderType == 3 && GroupState == (int)MiniApp.GroupState.已过期)
                {
                    return "未成团";
                }
                else if (GetWay == 6 && State == 8)
                {
                    return "待消费";
                }

                return Enum.GetName(typeof(MiniAppEntOrderState), State);
            }
        }

        /// <summary>
        /// 拼团状态
        /// </summary>
        public int GroupState { get; set; } = 0;

        /// <summary>
        /// 配送模板ID
        /// 0表示到店自提
        /// </summary>
        [SqlField]
        public int FreightTemplateId { get; set; } = 0;

        /// <summary>
        /// 配送模板名称
        /// </summary>
        public string FreightTemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 地址ID
        /// </summary>
        [SqlField]
        public int AddressId { get; set; } = 0;

        /// <summary>
        /// 提货人姓名
        /// </summary>
        [SqlField]
        public string AccepterName { get; set; } = string.Empty;

        /// <summary>
        /// 提货人提货人手机号码
        /// </summary>
        [SqlField]
        public string AccepterTelePhone { get; set; } = string.Empty;

        /// <summary>
        /// 提货人性别
        /// </summary>
        //[SqlField]
        //public int AccepterSex { get; set; }
        /// <summary>
        /// 店铺id (多门店用来存店铺<FOOTBATH>ID)
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 买家留言
        /// </summary>
        [SqlField]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 对外订单号
        /// </summary>
        [SqlField]
        public string OrderNum { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 运费金额
        /// </summary>
        [SqlField]
        public int FreightPrice { get; set; } = 0;

        public string FreightPriceStr
        {
            get
            {
                return (FreightPrice * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 详细地址
        /// </summary>
        [SqlField]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 邮政编码
        /// </summary>
        [SqlField]
        public string ZipCode { get; set; } = string.Empty;

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
        /// 订单类型 0 服务订单(普通订单)/ 1 服务预订（足浴）/ 2 礼物订单（足浴）/3 拼团订单.. 见枚举 EntOrderType
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
                return OrderType == 0 ? "堂食" : "外卖";
            }
        }

        /// <summary>
        /// 取物号
        /// </summary>
        [SqlField]
        public string TablesNo { get; set; }

        /// <summary>
        /// 订单内商品数量
        /// </summary>
        [SqlField]
        public int QtyCount { get; set; } = 0;

        /// <summary>
        /// 优惠金额
        /// </summary>
        [SqlField]
        public int ReducedPrice { get; set; } = 0;

        public string ReducedPriceStr
        {
            get
            {
                return (ReducedPrice * 0.01).ToString("0.00");
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
                if (PayDate != Convert.ToDateTime("0001-01-01 00:00:00") || BuyMode == (int)miniAppBuyMode.货到付款)  //暂默认微信支付
                {
                    return Enum.GetName(typeof(miniAppBuyMode), BuyMode);
                }
                return "未知";
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
        /// 多门店订单类型
        /// </summary>
        public string MultiStoreGetWayStr
        {
            get
            {
                return Enum.GetName(typeof(multiStoreOrderType), GetWay);
            }
        }

        /// <summary>
        /// 打印的单据ID
        /// </summary>
        [SqlField]
        public string PrintId { get; set; } = string.Empty;

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

        /// <summary>
        /// 权限表Id
        /// </summary>
        [SqlField]
        public int aId { get; set; } = 0;

        /// <summary>
        /// 申请退款原因
        /// </summary>
        [SqlField]
        public string drawBackRemark { get; set; } = string.Empty;

        public List<EntGoodsCart> goodsCarts { get; set; }

        [SqlField]
        public int FuserId { get; set; } = 0;

        /// <summary>
        /// 小程序模板
        /// </summary>
        [SqlField]
        public int TemplateType { get; set; } = 0;

        /// <summary>
        /// 拼团ID
        /// </summary>
        [SqlField]
        public int GroupId { get; set; } = 0;

        /// <summary>
        /// 小程序appid
        /// </summary>
        [SqlField]
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 店铺二维码关联ID
        /// </summary>
        [SqlField]
        public int StoreCodeId { get; set; } = 0;

        [SqlField]
        public int ReserveId { get; set; } = 0;

        /// <summary>
        /// 餐盒费
        /// </summary>
        [SqlField]
        public int PackinPrice { get; set; } = 0;

        public string PackinPriceStr { get { return (PackinPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 附加属性json 字符串
        /// </summary>
        [SqlField]
        public string attribute { get; set; } = string.Empty;

        /// <summary>
        /// 附加属性
        /// </summary>
        public EntGoodsOrderAttr attrbuteModel { get; set; } = new EntGoodsOrderAttr();

        #region 分销相关

        /*两种分销方式：公众号分销，个人分销
         *两种分销方式不能共存二者只能选其一
         */

        /// <summary>
        /// 公众号分销：来源公众号appid
        ///公众号将小程序加入自定义菜单，用于给公众号返利
        ///
        /// </summary>
        public string FromAppId { get; set; } = string.Empty;

        /// <summary>
        /// 个人分销：来源用户ID
        /// 通过用户的分享购买成功，用于给用户返利
        /// </summary>
        public int FromUserId { get; set; } = 0;

        /// <summary>
        /// 分销金额 单位/分
        /// </summary>
        public int DistributionMoney { get; set; } = 0;

        #endregion 分销相关

        /// <summary>
        /// 会员等级名称
        /// </summary>
        public string vipLevelName { get; set; } = string.Empty;

        /// <summary>
        /// 商品清单
        /// </summary>
        public string goodsNames { get; set; } = string.Empty;

        /// <summary>
        /// 门店名称
        /// </summary>
        public string storeName { get; set; } = string.Empty;

        /// <summary>
        /// 会员昵称
        /// </summary>
        public string nickName { get; set; } = string.Empty;

        /// <summary>
        /// 是否是自己的订单
        /// </summary>
        public bool isSelfOrder { get; set; } = true;

        /// <summary>
        /// 预定时间
        /// </summary>
        public DateTime reservationTime { get; set; }

        public string reservationTimeStr
        {
            get
            {
                return reservationTime == null ? "0001-01-01 00:00:00" : reservationTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 足浴-服务ID
        /// </summary>
        public int reservationProjectId { get; set; }

        /// <summary>
        /// 订单相对当前时间的状态 :0.还未到时间已经开始服务,1.还未到时间,2.超出时间,3.超过时间却未开始服务
        /// </summary>
        public int orderCurState { get; set; }

        /// <summary>
        /// 预定时间
        /// </summary>
        public DateTime serviceEndTime { get; set; }

        public string serviceEndTimeStr
        {
            get
            {
                return reservationTime == null ? "0001-01-01 00:00:00" : serviceEndTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 剩余/超出天数
        /// </summary>
        public int dayLeft { get; set; }

        /// <summary>
        /// 剩余/超出时间
        /// </summary>
        public string timeLeft { get; set; } = string.Empty;

        /// <summary>
        /// 店铺二维码名称
        /// </summary>
        public string StoreCodeName { get; set; } = "";

        #region 达达物流

        public string DadaOrderStateStr
        {
            get; set;
        } = string.Empty;

        public string sourceid
        {
            get; set;
        } = string.Empty;

        public string dadaorderid
        {
            get; set;
        } = string.Empty;

        #endregion 达达物流

        /// <summary>
        /// 是否已评价
        /// </summary>
        [SqlField]
        public bool IsCommentting { get; set; } = false;

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

        /// <summary>
        /// 申请退款时间
        /// </summary>
        [SqlField]
        public DateTime ApplyReturnTime { get; set; }
    }

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntAdminGoodsOrder
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

        public List<EntOrderCardDetail> GoodsList { get; set; }
    }

    public class EntOrderCardDetail
    {
        public int Id { get; set; }
        public string GoodsName { get; set; }
        public string ImgUrl { get; set; }
        public string SpecInfo { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public ExtraConfig footBathConfig { get; set; }
    }

    /// <summary>
    /// EntGoodsOrder附加属性实体
    /// </summary>
    public class EntGoodsOrderAttr
    {
        public bool isNewTableNo { get; set; } = false;
        public string zqStoreName { get; set; } = string.Empty;
        public int zqStoreId { get; set; } = 0;
        /// <summary>
        /// 秒杀组件使用（支付回调获取）
        /// </summary>
        public int flashItemId { get; set; } = 0;
        public int flashDealId { get; set; } = 0;
        public string FreightInfo { get; set; } = string.Empty;
    }
}