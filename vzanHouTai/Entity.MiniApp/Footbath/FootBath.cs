using Entity.Base;
using Entity.MiniApp.Conf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Footbath
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FootBath
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        ///  0表示总店  非零表示门店所属总店Id
        /// </summary>
        [SqlField]
        public int HomeId { get; set; } = 0;

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
        /// 详细地址
        /// </summary>
        [SqlField]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// 手机
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; } = string.Empty;
        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int BrowseCount { get; set; } = 0;

        /// <summary>
        /// 公告
        /// </summary>
        [SqlField]
        public string Notice { get; set; } = string.Empty;

        /// <summary>
        /// 营业时间
        /// </summary>
        [SqlField]
        public string ShopTime { get; set; } = "08:00:00 - 20:00:00";

        /// <summary>
        /// 开关设置
        /// </summary>
        [SqlField]
        public string SwitchConfig { get; set; } = "";

        public SwitchModel switchModel { get; set; }

        /// <summary>
        /// 送花套餐：一朵花的价格
        /// </summary>
        [SqlField]
        public int GiftPrice { get; set; } = 0;

        public string ShowGiftPrice
        {
            get
            {
                return (GiftPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 创建时间
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
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 营业日期
        /// </summary>
        public string shopDays { get; set; } = string.Empty;

        /// <summary>
        /// 预订日期
        /// </summary>
        public List<object> ReservationTime { get; set; }

        /// <summary>
        /// 管理者(多门店用以存 Miniaccount表 ID 标识店长是谁)
        /// </summary>
        [SqlField]
        public string ShopManager { get; set; } =string.Empty;

        /// <summary>
        /// 管理者账号
        /// </summary>
        [SqlField]
        public string ShopManagerName { get; set; } = string.Empty;

        /// <summary>
        /// 模板类型
        /// </summary>
        [SqlField]
        public int TemplateType { get; set; } = 0;


        /// <summary>
        /// 额外配置  (如多门店用来存店铺装修)
        /// </summary>
        [SqlField]
        public string ExtraConfig { get; set; } = string.Empty;


        /// <summary>
        /// 额外配置模板:根据模板类型返回不同Model
        /// </summary>
        public Object ExtraConfigModel
        {
            get
            {
                object model = new object();
                if (ExtraConfig != null && !string.IsNullOrEmpty(ExtraConfig))
                {


                    switch (TemplateType)
                    {
                        case (int)TmpType.小程序多门店模板:
                            model = JsonConvert.DeserializeObject<MultiStoreFitment>(ExtraConfig);
                            break;
                        default:
                            break;
                    }
                }

                return model;
            }
        }

        [SqlField]
        public DateTime OverTime { get; set; }

        public string OverTimeStr
        {

            get
            {
                return OverTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 门店是否上架 1 表示上架 0下架 总店暂时用不到
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        public string StateStr
        {
            get
            {
                return State == 0 ? "已下架" : "上架中";
            }
        }

        /// <summary>
        /// 门店是否删除 -1 表示删除 0未删除
        /// </summary>
        [SqlField]
        public int IsDel { get; set; } = 0;

        /// <summary>
        /// 当前总店下所有门店的用户列表
        /// </summary>
        public List<object> Accounts { get; set; }

        /// <summary>
        /// 多门店 配送方式配置
        /// </summary>
        [SqlField]
        public string TakeOutWayConfig { get; set; } = string.Empty;
        public TakeOutWayModel takeOutWayModel { get; set; }
        /// <summary>
        /// 门店装修页面信息
        /// </summary>
        [SqlField]
        public string StoreMaterialPages { get; set; } = string.Empty;

        /// <summary>
        /// 会员等级列表
        /// </summary>
        public List<VipLevel> levelList { get; set; }

    }


    /// <summary>
    /// 多门店配送方式模型
    /// </summary>
    public class TakeOutWayModel
    {
        /// <summary>
        /// 到店自取
        /// </summary>
        public SelfTake selfTake { get; set; } = new SelfTake();
        /// <summary>
        /// 同城配送
        /// </summary>
        public IntraCityService cityService { get; set; } = new IntraCityService();

        /// <summary>
        /// 快递配送
        /// </summary>
        public Expressdelivery GetExpressdelivery { get; set; } = new Expressdelivery();

        /// <summary>
        /// 距离目的地的差值
        /// </summary>
        public double TakeRangedistance { get; set; } = 0.00;


        /// <summary>
        /// 距离目的地的差值 带单位字符串
        /// </summary>
        public string TakeRangedistanceStr { get; set; } = string.Empty;

    }

    /// <summary>
    /// 到店自取(多门店)
    /// </summary>
    public class SelfTake
    {
        public bool IsOpen { get; set; } = false;
        /// <summary>
        /// 自取范围 半径多少公里
        /// </summary>
        public double TakeRange { get; set; } = 0.00;
    }

    /// <summary>
    /// 同城配送(多门店)
    /// </summary>
    public class IntraCityService
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsOpen { get; set; } = false;
        /// <summary>
        /// 是否自动接单
        /// </summary>
        public bool AutoReceiveOrder { get; set; } = false;
        /// <summary>
        /// 配送范围 半径多少公里
        /// </summary>
        public double TakeRange { get; set; } = 0.00;

       
        /// <summary>
        ///起送价 
        /// </summary>
        public double TakeStartPrice { get; set; } = 0.00;

        /// <summary>
        /// 配送费 
        /// </summary>
        public double TakeFright { get; set; } = 0.00;

        /// <summary>
        /// 订单满多少 免运费 
        /// </summary>
        public double FreeFrightCost { get; set; } = 0.00;
    }

    /// <summary>
    /// 快递配送(多门店)
    /// </summary>
    public class Expressdelivery
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsOpen { get; set; } = false;
        /// <summary>
        /// 配送费 
        /// </summary>
        public double TakeFright { get; set; } = 0.00;

        /// <summary>
        /// 订单满多少 免运费 
        /// </summary>
        public double FreeFrightCost { get; set; } = 0.00;
    }


    /// <summary>
    /// 足浴版开关配置类
    /// </summary>
    public class SwitchModel
    {
        /// <summary>
        /// 是否24小时营业
        /// </summary>
        public bool OpenAllDay { get; set; } = false;

        // 营业日期开关
        public bool Monday { get; set; } = true;
        public bool Tuesday { get; set; } = true;
        public bool Wensday { get; set; } = true;
        public bool Thursday { get; set; } = true;
        public bool Friday { get; set; } = true;
        public bool Saturday { get; set; } = true;
        public bool Sunday { get; set; } = true;

        //早中晚班值班时间
        public string morningShift { get; set; } = "8:00 - 15:59";
        public string noonShift { get; set; } = "16:00 - 23:59";
        public string eveningShift { get; set; } = "00:00 - 7:59";

        //支付方式开关

        /// <summary>
        /// 现金支付
        /// </summary>
        public bool CashPay { get; set; } = true;

        /// <summary>
        /// 支付宝支付
        /// </summary>
        public bool Alipay { get; set; } = true;

        /// <summary>
        /// 储值支付
        /// </summary>
        public bool SaveMoneyPay { get; set; } = true;

        /// <summary>
        /// 微信支付
        /// </summary>
        public bool WeChatPay { get; set; } = true;

        /// <summary>
        /// 银行卡支付
        /// </summary>
        public bool BankCardPay { get; set; } = true;

        /// <summary>
        /// 其他支付
        /// </summary>
        public bool OtherPay { get; set; } = false;

        /// <summary>
        /// 开启送花查看技师相册
        /// </summary>
        public bool ShowPhotoByGift { get; set; } = true;
        #region 预定配置
        /// <summary>
        /// 预订时间间隔
        /// </summary>
        public int TimeInterval { get; set; } = 30;

        /// <summary>
        /// 预定时间 默认7天，限制输入1~15之间的整数
        /// </summary>
        public int PresetTime { get; set; } = 7;

        /// <summary>
        /// 预订时是否支付费用
        /// </summary>
        public bool AdvancePayment { get; set; } = false;

        /// <summary>
        /// 预订技师
        /// </summary>
        public bool PresetTechnician { get; set; } = true;

        /// <summary>
        /// 性别必填
        /// </summary>
        public bool WriteSex { get; set; } = true;

        /// <summary>
        /// 买家留言  必填
        /// </summary>
        public bool WriteDesc { get; set; } = true;

        /// <summary>
        /// 自动锁定
        /// </summary>
        public bool AutoLock { get; set; } = true;

        #endregion

        /// <summary>
        /// 是否开启会员储值功能
        /// </summary>
        public bool canSaveMoneyFunction { get; set; } = true;

        /// <summary>
        /// 是否开启满减规则 默认关闭
        /// </summary>
        public bool discountRuleSwitch { get; set; } = false;

        /// <summary>
        /// 新用户首单立减
        /// </summary>
        public float newUserFirstOrderDiscountMoney { get; set; } = 0.00f;

        /// <summary>
        /// 用户首单立减
        /// </summary>
        public float userFirstOrderDiscountMoney { get; set; } = 0.00f;
    }
    /// <summary>
    /// 多门店-店铺装修
    /// </summary>
    public class MultiStoreFitment
    {
        /// <summary>
        /// 主题色
        /// </summary>
        public string SubjectColor { get; set; } = string.Empty;

        /// <summary>
        /// 轮播图路径
        /// </summary>
        public List<string> SlideShow { get; set; }

        /// <summary>
        /// 分类导航配置
        /// </summary>
        public List<ClassfiyNavigation> ClassfiyNavigations { get; set; }

        /// <summary>
        /// 首页引导语
        /// </summary>
        public string Introducer { get; set; }

        /// <summary>
        /// 首页展示商品ID集合
        /// </summary>
        public List<int> GoodsId { get; set; }
    }

    /// <summary>
    /// 分类导航
    /// </summary>
    public class ClassfiyNavigation
    {
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 跳转的分类ID
        /// </summary>
        public int SkipClassfiyId { get; set; } = 0;
    }
}
