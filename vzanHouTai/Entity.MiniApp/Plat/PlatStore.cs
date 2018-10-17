using Entity.Base;
using Entity.MiniApp.PlatChild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{

    /// <summary>
    /// 平台版店铺信息
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStore
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 小程序appId   小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// myCardId  关联表 获取注册用户
        /// </summary>
        [SqlField]
        public int MyCardId { get; set; }

        /// <summary>
        /// 主题颜色
        /// </summary>
        [SqlField]
        public string ColorTxt { get; set; } = "#1098f7";

        /// <summary>
        /// 首页轮播图 限制5张
        /// </summary>
        [SqlField]
        public string Banners { get; set; } = string.Empty;
        
        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用百度地图
        /// </summary>
        [SqlField]
        public bool UseBaidu { get; set; } = false;
        
        /// <summary>
        /// 地理位置 纬度
        /// </summary>
        [SqlField]
        public double Lat { get; set; }

        /// <summary>
        /// 地理位置 经度
        /// </summary>
        [SqlField]
        public double Lng { get; set; }
        
        /// <summary>
        /// 门店设施/提供的服务,例如:WIFi,停车位,接送服务
        /// </summary>
        [SqlField]
        public string StoreService { get; set; } = string.Empty;


        public List<StoreServiceModel> StoreServiceModelList { get; set; } = new List<StoreServiceModel>();


        /// <summary>
        /// 营业时间
        /// </summary>
        [SqlField]
        public string OpenTime { get; set; } = string.Empty;

        /// <summary>
        /// 客服电话
        /// </summary>
        [SqlField]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 业务简述
        /// </summary>
        [SqlField]
        public string BusinessDescription { get; set; } = string.Empty;
        /// <summary>
        /// 店铺描述
        /// </summary>
        [SqlField]
        public string StoreDescription { get; set; } = string.Empty;

        /// <summary>
        /// 店铺图片 限制30张
        /// </summary>
        [SqlField]
        public string StoreImgs { get; set; } = string.Empty;
        
        /// <summary>
        /// 店铺访问量
        /// </summary>
        [SqlField]
        public int StorePV { get; set; }

        /// <summary>
        /// 店铺虚拟访问量
        /// </summary>
        [SqlField]
        public int StoreVirtualPV { get; set; }




        /// <summary>
        /// 店铺状态0表示正常 -1表示无效
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 店铺所属平台小程序aid
        /// </summary>
        [SqlField]
        public int BindPlatAid { get; set; }
        
        /// <summary>
        /// 店铺所属分类这里保存的是原分类   保存小类的类别Id
        /// </summary>
        [SqlField]
        public int Category { get; set; }

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        
        /// <summary>
        /// 省Code
        /// </summary>
        [SqlField]
        public int ProvinceCode { get; set; }
        /// <summary>
        /// 市Code
        /// </summary>
        [SqlField]
        public int CityCode { get; set; }
        /// <summary>
        /// 县区Code
        /// </summary>
        [SqlField]
        public int CountryCode { get; set; }
        
        /// <summary>
        /// 店铺公告
        /// </summary>
        [SqlField]
        public string StoreRemark { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否置顶0不置顶 1置顶
        /// </summary>
        [SqlField]
        public int Top { get; set; }
        
        /// <summary>
        /// 开关设置
        /// </summary>
        [SqlField]
        public string SwitchConfig { get; set; } = string.Empty;

        /// <summary>
        /// 是否开启分销，0：关闭，1：开启
        /// </summary>
        [SqlField]
        public int OpenDistribution { get; set; } = 0;


        /// <summary>
        /// 是否开启关注公众号，0：关闭，1：开启
        /// </summary>
        [SqlField]
        public int OpenOfficialAccount { get; set; } = 0;

        /// <summary>
        /// 开关设置模型
        /// </summary>
        public PlatStoreSwitchModel SwitchModel { get; set; } = new PlatStoreSwitchModel();
        
        /// <summary>
        /// 距离
        /// </summary>
        public string Distance { get; set; } = "0.00";

        /// <summary>
        /// 店铺被收藏数量
        /// </summary>
        public int FavoriteCount { get; set; }

        /// <summary>
        /// 店铺累计访客
        /// </summary>
        public int StoreUV { get; set; }

        /// <summary>
        /// 是否已经收藏了 0表示否 1表示已经收藏
        /// </summary>
        public int Favorited { get; set; }

        /// <summary>
        /// 所属类别名称
        /// </summary>
        public string CategoryName { get; set; }
        
        /// <summary>
        /// 店铺Logo 默认轮播图第一张
        /// </summary>
        public string StoreHeaderImg
        {
            get
            {
                if (!string.IsNullOrEmpty(Banners))
                {
                    return Banners.Split(',').FirstOrDefault();
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 店铺详情页面推荐商品 按销量降序 推荐4个
        /// </summary>
        public List<TjGoods> TjGoods { get; set; } = new List<TjGoods>();

        public StoreOwner storeOwner { get; set; } = new StoreOwner();

        /// <summary>
        /// 申请开通绑定的开通状态，-1未开通,0：待开通，1：已开通
        /// </summary>
        public int AppState { get; set; } = -1;
        public string AppId { get; set; }
        public int UserId { get; set; }

        /// <summary>
        /// 该店铺是否有优惠券
        /// </summary>
        public bool HaveCoupons { get; set; }


        /// <summary>
        /// 使用期限 之前按年 20180913后来改为月
        /// </summary>
        [SqlField]
        public int YearCount { get; set; } = 12;
        /// <summary>
        /// 使用费用  累计费用
        /// </summary>
        [SqlField]
        public int CostPrice { get; set; }


        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired
        {

            get
            {
                //TODO 上线改为月
                return DateTime.Now > AddTime.AddMonths(YearCount);
            }
        }

        /// <summary>
        /// 入驻时间
        /// </summary>
        public string AddTimeStr
        {

            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 过期时间
        /// </summary>
        public string ExpireTimeStr
        {

            get
            {
                //TODO 上线改为月
                return AddTime.AddMonths(YearCount).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 剩余天数
        /// </summary>
        public int BalanceDay
        {
            get
            {
                //TODO 上线改为月
             
                return AddTime.AddMonths(YearCount).Subtract(DateTime.Now).Days;
            }
        }
    }

    /// <summary>
    /// 店铺首页推荐商品模型
    /// </summary>
    public class TjGoods
    {
        public int Aid { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }

        public string PriceStr { get; set; }

        public int SaleCount { get; set; }

        public int TopState { get; set; }

    }

    public class StoreOwner
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int State { get; set; }
        /// <summary>
        /// 是否开启分销，1：开启，0：关闭
        /// </summary>
        public int IsOpenDistribution { get; set; } = 0;
    }


    public class PlatStoreSwitchModel
    {
        /// <summary>
        /// 储值支付
        /// </summary>
        public bool SaveMoneyPay { get; set; } = false;

        /// <summary>
        /// 产品类别级别 1表示1级只显示小类 样式按照小类样式 
        /// 2表示2级 按照二级类别样式显示 先显示大类的 然后再显示小类
        /// </summary>
        public int ProductCategoryLevel { get; set; } = 1;
        /// <summary>
        /// 运费计算方式（枚举：DeliveryFeeSumMethond）
        /// </summary>
        public int deliveryFeeSumMethond { get; set; }

        /// <summary>
        /// 启用新运费模板功能
        /// </summary>
        public bool enableDeliveryTemplate { get; set; } = true;
        
        /// <summary>
        /// 一物一码开关
        /// </summary>
        public bool ProductQrcode { get; set; } = false;

        /// <summary>
        /// 到店自取开关
        /// </summary>
        public bool SwitchReceiving { get; set; } = false;

        /// <summary>
        /// 退货地址
        /// </summary>
        public string ReturnAddres { get; set; } = string.Empty;

        /// <summary>
        /// 快速支付
        /// </summary>
        public bool QuikclyPay { get; set; } = false;
        public string StorePayQrcode { get; set; } = "";

    }

    /// <summary>
    /// 店铺提供的服务
    /// </summary>
    public class StoreServiceModel
    {
        public bool ServiceState { get; set; }

        public string ServiceName { get; set; }
    }
}
