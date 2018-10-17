using System.ComponentModel;

namespace Entity.MiniApp
{
    #region 支付

    /// <summary>
    /// 支付状态
    /// </summary>
    public enum MiniappPayState
    {
        已失效 = -5,
        已过期 = -4,
        退款中 = -3,
        已退款 = -2,
        已收货 = -1,
        待发货 = 0,
        已发货 = 1,
        未到账 = 2,
        已到账 = 3,
        待支付 = 4,
        取消支付 = 5,
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum miniAppBuyMode
    {
        未选择支付方式 = 0,
        微信支付 = 1,
        储值支付 = 2,
        现金支付 = 3,
        线下微信支付 = 4,
        支付宝 = 5,
        银行卡支付 = 6,
        其他 = 7,
        到店支付 = 8,
        线下储值支付 = 9,
        线下支付 = 10,
        货到付款 = 11,
    }

    /// <summary>
    /// 配送方式
    /// </summary>
    public enum multiStoreOrderType
    {
        到店自取 = 1,
        同城配送 = 2,
        快递配送 = 3
    }

    #endregion 支付

    #region 订单状态

    /// <summary>
    /// 退货订单状态
    /// </summary>
    public enum ReturnGoodsState
    {
        拒绝退款 = -1,
        商家审核中 = 0,
        等待用户发货 = 1,
        等待商家收货 = 2,
        等待用户收货 = 3,
        退货成功 = 4,
    }

    public enum OrderState
    {
        已退款 = -3,
        退款中 = -2,
        取消订单 = -1,
        未付款 = 0,
        待核销 = 1,
        已核销 = 2,
        待发货 = 3,
        正在配送 = 4,
        待收货 = 5,
        已收货 = 6
    }

    public enum MiniAppEntOrderState
    {
        已删除 = -10,
        拒绝退货 = -5,
        退款成功 = -4,
        退款失败 = -3,
        退款中 = -2,
        已取消 = -1,
        待付款 = 0,
        待发货 = 1,
        //待送餐 = 1,
        待收货 = 2,
        交易成功 = 3,
        待服务 = 4,
        服务中 = 5,
        已完成 = 6,
        已超时 = 7,
        待自取 = 8,
        待接单 = 9,
        退款审核中 = 10,
        待配送 = 11,
        待确认送达 = 12,

        //退货流程枚举（开始）
        /// <summary>
        /// 用户向商家提交退货申请状态
        /// </summary>
        退货审核中 = 13,

        /// <summary>
        /// 退货申请批准，等待用户发货状态
        /// </summary>
        待退货 = 14,

        /// <summary>
        /// 用户已发货，待商家收货状态
        /// </summary>
        退货中 = 15,

        /// <summary>
        /// 商家已收货，并重新发货给用户状态
        /// </summary>
        换货中 = 16,

        /// <summary>
        /// 用户申请换货成功状态
        /// </summary>
        退换货成功 = 17,

        /// <summary>
        /// 用户申请退货退货成功状态
        /// </summary>
        退货退款成功 = 18,

        //退货流程枚举（结束）

        //取消订单
        申请取消订单 = 19,
    }

    public enum PlatChildOrderState
    {
        操作失败 = -6,
        拒绝退货 = -5,
        退款成功 = -4,
        退款失败 = -3,
        退款中 = -2,
        已取消 = -1,
        待付款 = 0,
        待发货 = 1,
        待自取 = 2,
        待收货 = 3,
        已完成 = 4,
        退款审核中 = 10,

        //退货流程枚举（开始）
        /// <summary>
        /// 用户向商家提交退货申请状态
        /// </summary>
        退货审核中 = 13,

        /// <summary>
        /// 退货申请批准，等待用户发货状态
        /// </summary>
        待退货 = 14,

        /// <summary>
        /// 用户已发货，待商家收货状态
        /// </summary>
        退货中 = 15,

        /// <summary>
        /// 商家已收货，并重新发货给用户状态
        /// </summary>
        换货中 = 16,

        /// <summary>
        /// 用户申请换货成功状态
        /// </summary>
        退换货成功 = 17,

        /// <summary>
        /// 用户申请退货退货成功状态
        /// </summary>
        退货退款成功 = 18

        //退货流程枚举（结束）
    }

    /// <summary>
    /// 企业订智推版单状态
    /// </summary>
    public enum QiyeOrderState
    {
        操作失败 = -6,
        拒绝退货 = -5,
        退款成功 = -4,
        退款失败 = -3,
        退款中 = -2,
        已取消 = -1,
        待付款 = 0,
        待发货 = 1,
        待自取 = 2,
        待收货 = 3,
        已完成 = 4,
        退款审核中 = 10,

        //退货流程枚举（开始）
        /// <summary>
        /// 用户向商家提交退货申请状态
        /// </summary>
        退货审核中 = 13,

        /// <summary>
        /// 退货申请批准，等待用户发货状态
        /// </summary>
        待退货 = 14,

        /// <summary>
        /// 用户已发货，待商家收货状态
        /// </summary>
        退货中 = 15,

        /// <summary>
        /// 商家已收货，并重新发货给用户状态
        /// </summary>
        换货中 = 16,

        /// <summary>
        /// 用户申请换货成功状态
        /// </summary>
        退换货成功 = 17,

        /// <summary>
        /// 用户申请退货退货成功状态
        /// </summary>
        退货退款成功 = 18

        //退货流程枚举（结束）
    }

    /// <summary>
    /// 小程序餐饮-订单状态
    /// </summary>
    public enum miniAppFoodOrderState
    {
        预下单 = -8,
        拒绝退款 = -7,
        付款中 = -6,
        已退款 = -5,
        退款失败 = -4,
        退款中 = -3,
        退款审核中 = -2,
        已取消 = -1,
        待付款 = 0,
        待接单 = 1,
        待送餐 = 2,
        待就餐 = 3,
        待确认送达 = 4,
        已完成 = 5
    }

    #endregion 订单

    #region 订单类型

    /// 订单类型 ，***增加枚举请注意值不要重复**
    /// </summary>
    public enum ArticleTypeEnum
    {
        /// <summary>
        /// 小程序电商模板订单
        /// </summary>
        MiniappGoods = 3001001,

        /// <summary>
        /// 小程序餐饮模板订单
        /// </summary>
        MiniappFoodGoods = 3001002,

        /// <summary>
        /// 小程序砍价订单 目前只在商城版
        /// </summary>
        MiniappBargain = 3001003,

        /// <summary>
        /// 小程序储值项目订单
        /// </summary>
        MiniappSaveMoneySet = 3001004,

        /// <summary>
        /// 小程序拼团订单
        /// </summary>
        MiniappGroups = 3001005,

        /// <summary>
        /// 小程序企业版订单
        /// </summary>
        MiniappEnt = 3001006,

        /// <summary>
        /// 小程序足浴版订单
        /// </summary>
        MiniappFootbath = 3001007,

        /// <summary>
        /// 小程序足浴版礼物订单
        /// </summary>
        MiniappFootbathGift = 3001008,

        /// <summary>
        /// 小程序多门店版订单
        /// </summary>
        MiniappMultiStore = 3001009,

        /// <summary>
        /// 小程序单独使用储值付款
        /// </summary>
        MiniappStoredvaluePay = 3001010,

        /// <summary>
        /// 小程序积分+微信支付方式订单
        /// </summary>
        MiniappExchangeActivity = 3001011,

        /// <summary>
        /// 小程序专业版拼团订单
        /// </summary>
        MiniappEntGoodsOrder = 3001012,

        /// <summary>
        /// 商家在后台手动扣除用户储值余额
        /// </summary>
        MiniappEditSaveMoney = 3001013,

        /// <summary>
        /// 小程序同城模板 选择置顶发帖支付
        /// </summary>
        City_StoreBuyMsg = 3001014,

        /// <summary>
        /// 小程序直接使用微信付款
        /// </summary>
        MiniappWXDirectPay = 3001015,

        /// <summary>
        /// 智慧餐厅 订单支付
        /// </summary>
        DishOrderPay = 3001016,

        /// <summary>
        /// 智慧餐厅 门店买单
        /// </summary>
        DishStorePayTheBill = 3001017,

        /// <summary>
        /// 平台版小程序 分类信息发帖支付
        /// </summary>
        PlatMsgPay = 3001018,

        /// <summary>
        /// 智慧餐厅 会员卡充值
        /// </summary>
        DishCardAccount = 3001019,

        /// <summary>
        /// 付费内容支付（图文、视频教材）
        /// </summary>
        PayContent = 3001020,

        /// <summary>
        /// 独立小程序支付订单
        /// </summary>
        PlatChildOrderPay = 3001021,

        /// <summary>
        /// 拼享惠 订单支付
        /// </summary>
        PinOrderPay = 3001022,
        /// <summary>
        /// 在平台上支付独立小程序订单
        /// </summary>
        PlatChildOrderInPlatPay = 3001023,
        /// <summary>
        /// 企业智推版订单
        /// </summary>
        QiyeOrderPay = 3001024,

        /// <summary>
        /// 在平台店铺收费入驻
        /// </summary>
        PlatAddStorePay = 3001025,
        /// <summary>
        /// 在平台店铺续期支付
        /// </summary>
        PlatStoreAddTimePay = 3001026,
        /// <summary>
        /// 专业版预约表单付费订单
        /// </summary>
        EntSubscribeFormPay=3001027,
    }
    /// <summary>
    /// 订单小类型
    /// </summary>
    public enum OrderTypeEnum
    {
        普通 = 0,
        堂食 = 1,
        外卖 = 2,
        礼物 = 3,
        预约 = 4,
        拼团 = 5,
        团购 = 6,
        付费内容 = 7,
        拼小惠 = 8,
        秒杀 = 9,
    }
    /// <summary>
    /// 小程序餐饮-订单小类型（不用）
    /// </summary>
    public enum miniAppFoodOrderType
    {
        堂食 = 0,
        外卖 = 1,
        预约 = 2,
        拼团 = 3,
    }
    /// <summary>
    /// 行业版订单小类型 entgoodsorder表（不用）
    /// </summary>
    public enum EntOrderType
    {
        订单 = 0,
        预约订单 = 1,
        礼物订单 = 2,
        拼团订单 = 3,
        外卖订单 = 4,

        /// <summary>
        /// 图文、视频等
        /// </summary>
        付费内容订单 = 5,
        预约付费订单=6,
    }
    #endregion

    /// <summary>
    /// 餐饮商品购物车类型
    /// </summary>
    public enum miniAppShopCartType
    {
        普通商品 = 0,
        预定商品 = 1
    }

    public enum miniAppReserveType
    {
        到店扫码 = 0,
        预约支付 = 1,
        预约购物_专业版 = 2
    }

    /// <summary>
    ///
    /// </summary>
    public enum TableItemState
    {
        [Description("数据置于默认存在状态")]
        None = 0,

        [Description("数据置于逻辑删状态")]
        Delete = -1,

        [Description("数据置于修改状态")]
        Modify = 1
    }

    /// <summary>
    /// 总枚举
    /// </summary>
    public enum MiniappState
    {
        到期下架 = -4,
        彻底删除 = -2,
        删除 = -1,
        待审核 = 0,
        通过 = 1,
    }

    public enum ActiveState
    {
        已失效 = -4,
        彻底删除 = -2,
        删除 = -1,
        待审核 = 0,
        通过 = 1,
    }

    /// <summary>
    /// 附件来源枚举,添加时，注意检查，值不要重复
    /// </summary>
    public enum AttachmentItemType
    {
        小程序单页轮播图 = 73,
        小程序商城店铺轮播图 = 70,
        小程序商城商品轮播图 = 71,
        小程序商城商品详情图 = 72,
        小程序官网首页轮播图 = 74,
        小程序商城店铺详情轮播图 = 75,
        小程序餐饮店铺Logo = 100,
        小程序餐饮店铺优惠图片 = 101,
        小程序餐饮店铺轮播图 = 102,
        小程序餐饮店铺资质图片 = 103,
        小程序餐饮菜品介绍轮播图片 = 104,
        小程序餐饮菜品详情图 = 105,
        小程序店铺砍价轮播图 = 106,
        小程序店铺砍价详情图 = 107,
        小程序砍价背景音乐 = 108,
        小程序行业版分享店铺Logo = 109,
        小程序行业版分享广告图 = 110,
        小程序拼团轮播图 = 111,
        小程序拼团详情图 = 112,
        小程序餐饮门店图片 = 301,
        小程序餐饮版分享店铺Logo = 310,
        小程序餐饮版分享广告图 = 311,
        小程序电商版分享店铺Logo = 312,
        小程序电商版分享广告图 = 313,
        小程序语音 = 314,
        小程序足浴版店铺logo = 315,
        小程序足浴版门店图片 = 316,
        小程序微信会员卡授权函图片 = 317,
        小程序微信会员卡营业执照图片 = 318,
        小程序微信会员卡个体身份证图片 = 319,
        小程序多门店版门店logo = 401,
        小程序多门店版门店图片 = 402,
        小程序积分活动图片 = 403,
        小程序积分活动轮播图 = 404,
        小程序商品评论轮播图 = 405,
        小程序餐饮新订单提示语音 = 406,
        小程序专业版新订单提示语音 = 407,
        小程序智慧官网形象图 = 408,
    }

    /// <summary>
    /// 视频附件来源枚举
    /// </summary>
    public enum AttachmentVideoType
    {
        小程序商城商品视频 = 8,
        小程序餐饮菜品视频 = 20,
        小程序砍价商品视频 = 21,
        小程序拼团视频 = 30,
    }

    /// <summary>
    /// 发送验证码类型
    /// </summary>
    public enum SendTypeEnum
    {
        抢优惠 = 1,
        拼团 = 2,
        商户入驻 = 6,
        申请个人分销 = 7,
        个人中心 = 8,
        申请小程序 = 9,
        手机号码注册 = 10,
        提现号码验证 = 11,
    }

    /// <summary>
    /// 模板消息类型 不同模板用十位区分
    /// </summary>
    public enum SendTemplateMessageTypeEnum
    {
        None = 0,
        餐饮订单支付成功通知 = 1,
        餐饮订单配送通知 = 2,
        餐饮退款申请通知 = 3,
        餐饮退款成功通知 = 4,
        餐饮订单拒绝通知 = 5,

        电商订单支付成功通知 = 6,
        电商订单配送通知 = 7,

        足浴预约成功通知 = 11,
        足浴预约取消通知 = 12,
        足浴退款通知 = 13,
        足浴预约超时通知 = 14,
        足浴已预约活动开始提醒 = 15,

        多门店订单支付成功通知 = 21,
        多门店订单配送通知 = 22,
        多门店订单取消通知 = 23,
        多门店退款通知 = 24,
        多门店反馈处理结果通知 = 25,
        多门店订单确认通知 = 26,
        多门店订单发货提醒 = 27,

        专业版订单支付成功通知 = 31,
        专业版订单取消通知 = 32,
        专业版订单退款通知 = 33,
        专业版订单发货提醒 = 34,
        专业版产品预约成功通知 = 35,
        专业版订单强行发货通知 = 36,

        排队拿号排队成功通知 = 41,
        排队拿号排队即将排到通知 = 42,
        排队拿号排队到号通知 = 43,
        排队拿号排队即将排到通知_通用 = 44,
        排队拿号排队到号通知_通用 = 45,

        拼团拼团成功提醒 = 51,
        拼团基础版订单支付成功通知 = 52,
        拼团基础版订单取消通知 = 53,
        拼团基础版订单退款通知 = 54,
        拼团基础版订单发货提醒 = 55,

        砍价订单支付成功提醒 = 61,
        砍价订单取消通知 = 62,
        砍价订单退款通知 = 63,
        砍价订单发货提醒 = 64,

        //预约枚举
        预约点餐商家接单通知 = 70,

        预约点餐就座通知 = 71,
        预约点餐扫码就座通知 = 72,
        预约点餐取消通知 = 73,
        预约点餐退款通知 = 74,
        预约点餐支付通知 = 75,
        预约购物接单通知 = 76,
        预约购物自取通知 = 77,
        预约购物退款通知 = 78,
        预约购物取消通知 = 79,

        //预约枚举结束
        达达配送接单通知 = 80,

        达达配送配送中通知 = 81,
        达达配送已送达通知 = 82,

        //退换货枚举
        退换货订单申请审核 = 90,

        退换货订单商家发货 = 91,
        //退换货枚举结束

        //独立小程序模板枚举
        独立小程序版订单支付成功通知 = 100,
        独立小程序版订单取消通知 = 101,
        独立小程序版订单退款通知 = 102,
        独立小程序版订单发货提醒 = 103,

        拼享惠发送申诉结果通知 = 110,
        拼享惠订单支付成功买家通知 = 111,
        拼享惠订单支付成功卖家通知 = 112,
        拼享惠订单取消通知 = 113,
        拼享惠退款通知 = 114,
        拼享惠订单配送通知 = 115,
        //秒杀组件枚举
        秒杀开始通知 = 120,

        充值成功通知 = 116,
        下单成功通知 = 117,
        智慧餐厅退款通知 = 118,
        智慧餐厅订单支付成功 = 119,

        企业智推版订单支付成功通知 = 130,
        企业智推版订单取消通知 = 131,
        企业智推版订单退款通知 = 132,
        企业智推版订单发货提醒 = 133,
    }

    /// <summary>
    /// 小程序餐饮-打印机类型
    /// </summary>
    public enum miniappPrintType
    {
        易联云 = 1,
    }

    /// <summary>
    /// 权限类型 - 对应 Role 表的ID
    /// </summary>
    public enum RoleType
    {
        总店管理员 = 1,
        分店管理员 = 2,
    }

    public enum Miapp_Miniappmoduls_Level
    {
        /// <summary>
        /// 首页
        /// </summary>
        ModelData = 1,

        /// <summary>
        /// 企业介绍
        /// </summary>
        FirstModel = 2,

        /// <summary>
        /// 板块2
        /// </summary>
        TwoModel = 3,

        /// <summary>
        /// 板块3
        /// </summary>
        ThreeModel = 4,

        /// <summary>
        /// 产品展示
        /// </summary>
        FourModel = 5,

        /// <summary>
        /// 发展历程
        /// </summary>
        FiveModel = 6,

        /// <summary>
        /// 联系我们
        /// </summary>
        SixModel = 7,

        /// <summary>
        /// 企业动态
        /// </summary>
        EightModel = 8,

        /// <summary>
        /// 新闻颜色
        /// </summary>
        NightModel = 9,
    }

    //绑定类型，1：论坛，2：直播，3：同城，4：商城
    public enum PayCenterSettingType
    {
        Minisns = 1,
        Live = 2,
        City = 3,
        Shop = 4
    }

    public enum GoodProjectType
    {
        行业版商品分类 = 0,
        足浴版包间分类 = 1,
        足浴版送花套餐 = 2,//已弃用
        足浴版服务项目分类 = 3,
    }

    public enum TechnicianState
    {
        删除 = -1,
        空闲 = 0,
        上钟 = 1,

        //将下钟 = 2,
        休息中 = 3,
    }

    public enum GoodsType
    {
        足浴版服务 = 1,
        足浴版送花套餐 = 2,
    }
    /// <summary>
    /// 送花记录查询类型
    /// </summary>
    public enum GiftsRecordType
    {
        全部 = 0,
        今天 = 1,
        昨天 = 2,
        最近7天 = 3,
        最近30天 = 4,
    }

    public enum UserType
    {
        普通用户 = 0,
        客服 = 1
    }

    /// <summary>
    /// 产品类型
    /// </summary>
    public enum EntGoodsType
    {
        普通产品 = 0,
        拼团产品 = 1,
        砍价产品 = 2,
        团购商品 = 3,
        秒杀商品 = 4,
        预约商品 = 5
    }

    public enum EntGoodCartType
    {
        普通 = 0,
        预约 = 1,
        外卖 = 2,
        预约表单=3,
    }

    /// <summary>
    /// 退货订单类型
    /// </summary>
    public enum ReturnGoodsType
    {
        专业版退货退款 = 0,
        专业版退换货 = 1
    }

    /// <summary>
    /// 付费内容类型（图文、视频教材等）
    /// </summary>
    public enum PaidContentType
    {
        专业版图文 = 0,
        专业版视频 = 1,
        专业版音频 = 2
    }

    /// <summary>
    /// 点赞数据类型枚举
    /// </summary>
    public enum PointsDataType
    {
        帖子 = 0,
        商品 = 1,
        评论 = 2,
        名片 = 3,
        店铺 = 4,
        小未平台 = 5,
        员工 = 6,
        客户 = 7,
        企业智推版 = 8,
    }

    /// <summary>
    /// 0表示 收藏 1表示点赞,2关注，3看过，4私信
    /// </summary>
    public enum PointsActionType
    {
        收藏 = 0,
        点赞 = 1,
        关注 = 2,
        看过 = 3,
        私信 = 4,
        评论 = 5,
        转发 = 6,
        扫码 = 7,
    }

    public enum PrintContentType
    {
        打印专业版订单 = 0,

        /// <summary>
        /// 和普通订单有区别，数据结构很任性
        /// </summary>
        打印专业版砍价订单 = 1,
        打印餐饮版粘贴标签 = 2,
    }

    public enum FlashDealState
    {
        已删除 = -1,
        已下架 = 0,
        已上架 = 1,
        已开始 = 2,
        /// <summary>
        /// 服务处理到期结束
        /// </summary>
        已结束 = 3,
    }

    public enum FlashItemState
    {
        已删除 = -1,
        已结束 = 0,
        使用中 = 1
    }

    /// <summary>
    /// 代理商消费变更明细枚举
    /// </summary>
    public enum AgentDepositLogType
    {
        充值 = 1,
        开通客户模板 = 2,
        分销商开通客户模板 = 3,
        代理商续费 = 4,
        分销商续费 = 5,
        开通新门店 = 6,
        代理商升级专业版版本 = 8,
        分销商升级专业版版本 = 9,
        变更智慧餐厅门店 = 10,
        分销商变更智慧餐厅门店 = 11,
        开启水印 = 12,
        分销代理充值 = 13,
        普通用户续费模板 = 14,
        普通用户充值 = 15,
        普通用户开通模板 = 16,
        变更企业智推版员工 = 17,
        分销商变更企业智推版员工 = 18,
        升级企业版 = 19,
        分销商升级企业版 = 20,
    }


    #region 第三方订单配送
    /// <summary>
    /// 订单配送方式
    /// </summary>
    public enum miniAppOrderGetWay
    {
        到店自取 = 0,
        商家配送 = 1,
        达达配送 = 2,
        蜂鸟配送 = 3,
        快跑者配送 = 4,
        UU配送 = 5,
        到店消费 = 6,
    }
    /// <summary>
    /// 达达订单状态
    /// </summary>
    public enum DadaOrderEnum
    {
        #region 系统状态

        推单中 = -2,
        取消中 = -1,
        待支付 = 0,

        #endregion 系统状态

        待接单 = 1,
        待取货 = 2,
        配送中 = 3,
        已完成 = 4,
        已取消 = 5,
        已过期 = 7,
        指派单 = 8,
        妥投异常之物品返回中 = 9,
        妥投异常之物品返回完成 = 10,
        系统故障订单发布失败 = 1000,
    }

    /// <summary>
    /// 快跑者配送订单状态：1：待发单，2：待抢单，3：待接单，4：取单中，5：送单中，6：已送达，7：已撤销
    /// </summary>
    public enum KPZOrderEnum
    {
        待请求 = 0,
        待发单 = 1,
        待抢单 = 2,
        待接单 = 3,
        取单中 = 4,
        送单中 = 5,
        已送达 = 6,
        已撤销 = 7,
    }
    /// <summary>
    /// UU配送订单状态：1下单成功 3跑男抢单 4已到达 5已取件 6到达目的地 10收件人已收货 -1订单取消
    /// </summary>
    public enum UUOrderEnum
    {
        订单取消中 = -2,
        订单取消 = -1,
        下单成功 = 1,
        跑男抢单 = 3,
        已到达 = 4,
        已取件 = 5,
        到达目的地 = 6,
        收件人已收货 = 10,
    }

    /// <summary>
    /// 蜂鸟状态返回码
    /// </summary>
    public enum FNOrderEnum
    {
        #region 系统状态

        推单中 = -2,
        已取消 = -1,

        #endregion 系统状态

        系统已接单 = 1,//蜂鸟配送开放平台接单后, 商户接收到系统已接单状态, 支持取消
        已分配骑手 = 20,//蜂鸟配送开放平台接单后, 商户接收到已分配骑手状态, 支持取消
        骑手已到店 = 80,//蜂鸟配送开放平台将骑手已到店状态回调给商户, 支持取消
        配送中 = 2,//蜂鸟配送开放平台将骑手配送中状态回调给商户, 不支持取消
        已送达 = 3,//蜂鸟配送开放平台将已送达状态回调给商户, 不支持取消
        异常 = 5//推单到物流开放平台后任何阶段产生异常, 蜂鸟配送开放平台将异常状态回调给商户
    }
    #endregion

    #region 代理商

    /// <summary>
    /// 代理分销跟进状态枚举
    /// </summary>
    public enum AgentDistributionFollowState
    {
        代理跟进 = 0,
        小未跟进 = 1,
    }

    /// <summary>
    /// 返款态枚举
    /// </summary>
    public enum AgentCaseBackState
    {
        永久删除 = -2,
        失效 = -1,
        待确认 = 0,
        驳回 = 1,
        已确认 = 2,
        已打款 = 3,
    }
    #endregion

    #region 营销插件

    /// <summary>
    /// 营销工具类型
    /// </summary>
    public enum MarketingType
    {
        小程序砍价 = 1,
        小程序拼团 = 2,
        小程序优惠券 = 3,
    }

    /// <summary>
    /// 优惠券类型
    /// </summary>
    public enum TicketType
    {
        优惠券 = 0,
        立减金 = 1,
    }

    /// <summary>
    /// 优惠券状态
    /// </summary>
    public enum CouponState
    {
        已失效 = -1,
        已关闭 = 0,
        已开启 = 1,
    }


    /// <summary>
    /// 拼团枚举
    /// </summary>
    public enum GroupState
    {
        待付款 = 0,
        开团成功 = 1,
        团购成功 = 2,
        成团失败 = -1,
        已过期 = -4,
    }
    #endregion

    #region 小程序模板管理

    /// <summary>
    /// 项目小程序类型
    /// </summary>
    public enum ProjectType
    {
        测试 = 0,
        同城 = 1,
        小程序 = 2,
        直播 = 3,
        论坛 = 4,
        有约 = 5,
    }

    /// <summary>
    /// 小程序状态
    /// </summary>
    public enum XcxTypeEnum
    {
        通过审核 = -5,//小未客服审核状态
        不通过 = -4,//小未客服审核状态
        待审核 = -3,//小未客服审核状态
        未发布 = -2,
        提交审核失败 = -1,
        审核成功 = 0,
        审核失败 = 1,
        审核中 = 2,
        发布成功 = 3,
        发布失败 = 4,
    }

    /// <summary>
    /// 小程序认证类型
    /// </summary>
    public enum XcxMiniAppAuthoType
    {
        未认证 = -1,
        已微信认证 = 0,
        新浪微博认证 = 1, 腾讯微博认证 = 2,
        已资质认证通过但还未通过名称认证 = 3,
        已资质认证通过还未通过名称认证但通过了新浪微博认证 = 4,
        已资质认证通过还未通过名称认证但通过了腾讯微博认证 = 5
    }

    /// <summary>
    /// 模板类型 值同 miniapptemplatetype 表
    /// </summary>
    public enum TmpType
    {
        小程序企业模板 = 1,
        小程序单页模板 = 4,
        小程序电商模板 = 6,
        小程序餐饮模板 = 8,
        小程序行业模板 = 12,
        小程序专业模板 = 22,
        小程序电商模板测试 = 23,
        小程序足浴模板 = 25,
        小程序多门店模板 = 26,
        小程序足浴技师端模板 = 47,
        小程序同城模板 = 49,
        小程序餐饮多门店模板 = 51,
        智慧餐厅 = 52,
        小未平台 = 53,
        小未平台子模版 = 54,
        拼享惠 = 55,
        企业智推版 = 56
    }
    #endregion

    #region 物流
    /// <summary>
    /// 物流信息类型
    /// </summary>
    public enum DeliveryOrderType
    {
        专业版订单商家发货 = 0,
        专业版订单用户退货 = 1,
        专业版订单商家换货 = 2,
        独立小程序订单商家发货 = 10,
        拼享惠订单商家发货 = 11,
        专业版砍价发货 = 12,
    }
    /// <summary>
    /// 运费模板单位
    /// </summary>
    public enum DeliveryUnit
    {
        件数 = 0,
        重量 = 1
    }

    public enum DeliveryConfigType
    {
        专业版普通产品 = 0,
        专业版拼团产品 = 1,
        砍价产品 = 2,
        团购商品 = 3,
        秒杀商品 = 4,
        运费模板 = 5
    }

    /// <summary>
    /// 运费计算规则算法（详细说明：http://doc.vzan.com/enterprisePro/#_22）
    /// </summary>
    public enum DeliveryFeeSumMethond
    {
        有赞 = 0,
        淘宝 = 1
    }

    /// <summary>
    /// 物流跟踪订单本地状态
    /// </summary>
    public enum DeliveryFeedState
    {
        /// <summary>
        /// 已删除订单（不同步物流）
        /// </summary>
        删除 = -1,

        /// <summary>
        /// 已停用订单（不支持查询或异常订单，不同步物流）
        /// </summary>
        停止 = 0,

        /// <summary>
        /// 正常订单（等待查询配送状态，未订阅物流推送）
        /// </summary>
        正常 = 1,

        /// <summary>
        /// 等待查询订单（已查询查询配送状态 **配送中**，等待订阅物流推送）
        /// </summary>
        等待 = 2,

        /// <summary>
        /// 进行中（已查询配送状态 **配送中**，已订阅物流推送）
        /// </summary>
        进行 = 3,

        /// <summary>
        /// 已结束（已查询配送状态 **已配送**，停止订阅物流推送）
        /// </summary>
        结束 = 4
    }

    /// <summary>
    /// 物流跟踪接口配送状态
    /// </summary>
    public enum DeliveryApiFeedState
    {
        无轨迹 = 0,
        已揽收 = 1,
        在途中 = 2,
        签收 = 3,
        问题件 = 4,
    }

    /// <summary>
    /// 物流跟踪订阅同步状态
    /// </summary>
    public enum DeliverySubscribeSyncState
    {
        失败 = -1,
        未同步 = 0,
        成功 = 1
    }
    #endregion

    #region 提现审核
    /// <summary>
    /// 提现状态
    /// </summary>
    public enum DrawCashState
    {
        提现失败 = -1,
        未开始提现 = 0,
        提现中 = 1,
        提现成功 = 2,
        人工提现 = 3
    }
    /// <summary>
    /// 提现审核状态
    /// </summary>
    public enum ApplyState
    {
        删除 = -2,
        审核不通过 = -1,
        待审核 = 0,
        审核通过 = 1,
    }
    /// <summary>
    /// 提现方式
    /// </summary>
    public enum DrawCashWay
    {
        微信提现 = 0,
        银行卡人工提现 = 1,
    }
    /// <summary>
    /// 申请提现类型
    /// </summary>
    public enum DrawCashApplyType
    {
        分销收益提现 = 0,
        普通提现 = 1,
        平台店铺提现 = 2,
        拼享惠平台交易 = 3,
        拼享惠扫码收益 = 4,
        拼享惠代理收益 = 5,
        拼享惠用户返现 = 6,
    }
    #endregion

    /// <summary>
    /// 订阅消息发送服务
    /// </summary>
    public enum SubscribeMsgType
    {
        小程序模板消息 = 0,
        公众号模板消息 = 1
    }

    /// <summary>
    /// 订阅消息发送状态
    /// </summary>
    public enum SubscribeMsgState
    {
        删除 = -2,
        发送失败 = -1,
        等待发送 = 0,
        发送成功 = 1,
    }

    public enum UserTrackType
    {
        二维码扫描 = 0,
        二维码下单 = 1,
    }

    public enum UserTrackState
    {
        删除 = -1,
        新增 = 0,
        已统计 = 1,
    }

    public enum NavMenus
    {
        阿拉丁统计 = 1,
        平台配置 = 2,
        提现配置 = 3,
        提现申请 = 4,
        分类管理 = 5,
        产品管理 = 6,
        商家管理 = 7,
        账号管理 = 8,
        小程序管理 = 9
    }

    public enum NavMenuType
    {
        拼享惠 = 0,
        专业版 = 1,
    }

    /// <summary>
    /// 微信小程序关键词状态
    /// </summary>
    public enum KeyWordState
    {
        正常 = 0,
        已售 = -1,
        已删除 = -2,
    }
}