using Entity.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utility;

namespace Entity.MiniApp.Stores
{
    /// <summary>
    /// 小程序商城模板-店铺信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Store
    {
        public Store() { }
        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string name { get; set; }
        /// <summary>
        /// 店铺Logo
        /// </summary>
        [SqlField]
        public string logo { get; set; }
        /// <summary>
        /// 店铺照片，多张用英文逗号隔开
        /// </summary>
        [SqlField]
        public string pictures { get; set; }
        /// <summary>
        /// 营业时间JSON串对象
        /// </summary>
        [SqlField]
        public string businesstimes { get; set; }
        /// <summary>
        /// 店铺公告
        /// </summary>
        [SqlField]
        public string notice { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int appId { get; set; }
        /// <summary>
        /// 省id
        /// </summary>
        [SqlField]
        public int Province { get; set; }
        /// <summary>
        /// 城市id
        /// </summary>
        [SqlField]
        public int CityCode { get; set; }
        /// <summary>
        /// 区域id
        /// </summary>
        [SqlField]
        public int AreaCode { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; }
        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int BrowseCount { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double Lng { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double Lat { get; set; }

        /// <summary>
        /// 配置json string 串
        /// </summary>
        [SqlField]
        public string configJson { get; set; }


        /// <summary>
        /// 新闻资讯类别级别 默认为0 表示小类 1表示大类
        /// </summary>
        [SqlField]
        public int NewsTypeLevel { get; set; }


        /// <summary>
        /// 功能入口实体
        /// </summary>
        public StoreConfigModel funJoinModel { get; set; }

        public object kfInfo { get; set; }

        public List<EntGoodType> goodsCatList { get; set; }



        #region 来新订单提示音
        /// <summary>
        /// 是否开启新订单提示音
        /// </summary>
        [SqlField]
        public bool OpenNewOrderPrompt { get; set; } = false;
        /// <summary>
        /// 音频路径
        /// </summary>
        [SqlField]
        public string VoiceUrl { get; set; } = string.Empty;
        /// <summary>
        /// 0：系统音频，1：自定义音频
        /// </summary>
        [SqlField]
        public int VoiceType { get; set; } = 0;
        #endregion
    }


    public class StoreConfigModel
    {
        /// <summary>
        /// 开启客户储值功能
        /// </summary>
        public bool canSaveMoneyFunction { get; set; } = true;


        /// <summary>
        /// 开启到店自取 --暂用于多门店
        /// </summary>
        public bool openInvite { get; set; } = false;

        /// <summary>
        /// 开启到店消费
        /// </summary>
        public bool openToStoreConsume { get; set; } = false;

        /// <summary>
        /// 快递配送开关
        /// </summary>
        public bool openExpress { get; set; } = true;

        /// <summary>
        /// 一天中营业开始时间
        /// </summary>
        public string StartTime { get; set; } = string.Empty;
        /// <summary>
        /// 一天中营业结束时间
        /// </summary>
        public string EndTime { get; set; } = string.Empty;
        /// <summary>
        /// 营业日期
        /// </summary>
        public string Weeks { get; set; } = "周一,周二,周三,周四,周五,周六,周日";
        /// <summary>
        /// 是否显示热门搜索关键词
        /// </summary>
        public bool openSearchKeyword { get; set; } = false;
        /// <summary>
        /// 热门搜索关键词
        /// </summary>
        public List<string> searchKeyword { get; set; } = new List<string>();

        /// <summary>
        /// 商品详情弹框推送 [在线客服] 提示信息
        /// </summary>
        public bool openWxShopMessage { get; set; } = false;

        /// <summary>
        /// 启用运费模板ID
        /// </summary>
        public int FreightTemplateId { get; set; } = 0;

        /// <summary>
        /// 统一运费价格
        /// </summary>
        public int FreightPrice { get; set; }

        public bool FreightPriceSwitch { get; set; }

        /// <summary>
        /// 私信开关
        /// </summary>
        public bool imSwitch { get; set; } = false;

        /// <summary>
        /// 自动问候语
        /// </summary>
        public bool sayHello { get; set; } = true;

        public string helloWords { get; set; } = "你好，很高兴为你服务";

        /// <summary>
        /// 一物一码分享小程序码开关(通过小程序码识别进入对应的商品详情页)
        /// </summary>
        public bool productQrcodeSwitch { get; set; } = false;


        /// <summary>
        /// 排队开关
        /// </summary>
        public bool sortQueueSwitch { get; set; } = false;

        /// <summary>
        /// 下一个队列号
        /// </summary>
        public int sortNo_next { get; set; } = 1;

        // 营业日期开关
        public bool Monday { get; set; } = true;
        public bool Tuesday { get; set; } = true;
        public bool Wensday { get; set; } = true;
        public bool Thursday { get; set; } = true;
        public bool Friday { get; set; } = true;
        public bool Saturday { get; set; } = true;
        public bool Sunday { get; set; } = true;
        #region 外卖相关
        /// <summary>
        /// 外卖店铺地址：通过地图设置的地址
        /// </summary>
        public string takeoutAddress { get; set; } = string.Empty;
        /// <summary>
        /// 外卖开关
        /// </summary>
        public bool takeoutSwitch { get; set; } = true;

        /// <summary>
        /// 配送方式
        /// </summary>
        public int DistributionWay { get; set; } = (int)miniAppOrderGetWay.商家配送;

        /// <summary>
        /// 配送范围
        /// </summary>
        public int DeliveryRange { get; set; } = 0;

        /// <summary>
        /// 起送价
        /// </summary>
        public int OutSide { get; set; } = 0;
        public string OutSideStr { get { return (OutSide * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 配送费
        /// </summary>
        public int ShippingFee { get; set; } = 0;
        public string ShippingFeeStr { get { return (ShippingFee * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 餐盒费
        /// </summary>
        public int PackinFee { get; set; } = 0;
        public string PackinFeeStr { get { return (PackinFee * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 外卖设为首页
        /// </summary>
        public bool setIndexSwitch { get; set; } = false;

        /// <summary>
        /// 页面样式
        /// </summary>
        public int takeoutStyleType { get; set; } = 1;

        public string goodsCatIds { get; set; }
        /// <summary>
        /// 营业日期
        /// </summary>
        public string ShopDays
        {
            get
            {
                return GetShopDays();
            }
        }
        public string GetShopDays()
        {
            string result = string.Empty;
            List<string> fullarr = new List<string> { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            List<string> unselArr = new List<string>();
            if (!Monday)
            {
                unselArr.Add("周一");
            }
            if (!Tuesday)
            {
                unselArr.Add("周二");
            }
            if (!Wensday)
            {
                unselArr.Add("周三");
            }
            if (!Thursday)
            {
                unselArr.Add("周四");
            }
            if (!Friday)
            {
                unselArr.Add("周五");
            }
            if (!Saturday)
            {
                unselArr.Add("周六");
            }
            if (!Sunday)
            {
                unselArr.Add("周日");
            }
            if (unselArr.Count == 0)
            {
                result = "周一至周日";
            }
            else
            {
                string fullstr = string.Join("|", fullarr);
                List<string> selarr = new List<string>();
                //过滤不要的，重新拼接数组
                foreach (var item in unselArr)
                {
                    selarr = Regex.Split(fullstr, $"{item}", RegexOptions.IgnoreCase).ToList();
                    for (int i = 0; i < selarr.Count; i++)
                    {
                        selarr[i] = selarr[i].Trim('|').Trim(',');
                    }
                    fullstr = string.Join(",", selarr);
                }
                //得出每个连续的字符串
                List<string> list = fullstr.Split(',').ToList();
                foreach (var str in list)
                {
                    var vals = str.Trim('|').Split('|');
                    if (vals.Length == 1)
                    {
                        result += vals[0] + ",";
                    }
                    else if (vals.Length > 1)
                    {
                        result += $"{vals[0]}至{vals[vals.Length - 1]},";
                    }
                }
                result = result.Trim(',');
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 预约商品分类
        /// </summary>
        public string reserveClass { get; set; }

        /// <summary>
        /// 预约功能开关
        /// </summary>
        public bool reserveSwitch { get; set; }

        /// <summary>
        /// 运费计算方式（枚举：DeliveryFeeSumMethond）
        /// </summary>
        public int deliveryFeeSumMethond { get; set; }

        /// <summary>
        /// 启用新运费模板功能
        /// </summary>
        public bool enableDeliveryTemplate { get; set; }

        /// <summary>
        /// 退货地址
        /// </summary>
        public string returnAddress { get; set; }

        /// <summary>
        /// 开启物流跟踪组件功能
        /// </summary>
        public bool trackDelivery { get; set; }

        /// <summary>
        /// 物流跟踪接口密钥
        /// </summary>
        public string trackDeliveryKey { get; set; }

        #region 我的功能开关
        /// <summary>
        /// 普通订单状态栏
        /// </summary>
        public bool isopen_orderbar { get; set; } = true;

        /// <summary>
        /// 拼团
        /// </summary>
        public bool isopen_pintuan { get; set; } = true;

        /// <summary>
        /// 砍价
        /// </summary>
        public bool isopen_kanjia { get; set; } = true;

        /// <summary>
        /// 团购
        /// </summary>
        public bool isopen_tuangou { get; set; } = true;

        /// <summary>
        /// 分销
        /// </summary>
        public bool isopen_fenxiao { get; set; } = true;

        /// <summary>
        /// 客服
        /// </summary>
        public bool isopen_kefu { get; set; } = true;

        /// <summary>
        /// 我的预约单
        /// </summary>
        public bool isopen_yuyue { get; set; } = true;

        /// <summary>
        /// 购物车
        /// </summary>
        public bool isopen_car { get; set; } = true;

        /// <summary>
        /// 拿号排队
        /// </summary>
        public bool isopen_paidui { get; set; } = true;

        /// <summary>
        /// 积分中心
        /// </summary>
        public bool isopen_jifen { get; set; } = true;
        #endregion


        /// <summary>
        /// 签到送积分配置
        /// </summary>
        public string ExchangePlayCardConfig { get; set; }

        public ExchangePlayCardConfig PlayCardConfigModel { get; set; }

        /// <summary>
        /// 快速买单页面小程序码
        /// 专业版后台生成用于商家贴到店铺用户扫码进入买单
        /// </summary>
        public string StorePayQrcode { get; set; }

        /// <summary>
        /// 开启货到付款支付方式
        /// </summary>
        public bool CashOnDelivery { get; set; }
        /// <summary>
        /// 开启预约支付
        /// </summary>
        public bool OpenYuyuePay { get; set; } = false;
        /// <summary>
        /// 预约付费方式 0：固定金额付费，1：按产品价格百分比付费
        /// </summary>
        public int YuyuePayType { get; set; } = 0;
        /// <summary>
        /// 预约付费数值 YuyuePayType=0时支付金额  YuyuePayType=1时支付百分比
        /// </summary>
        public int YuyuePayCount { get; set; } = 100;
    }
}
