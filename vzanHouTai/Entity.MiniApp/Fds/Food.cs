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
   public class Food
    {

        public Food() { }
        /// <summary>
        /// 餐饮店铺Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string FoodsName { get; set; } = string.Empty;

        [SqlField]
        public string Logo { get; set; } = string.Empty;

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;
        /// <summary>
        /// 省id
        /// </summary>
        [SqlField]
        public int Province { get; set; } = 0;
        /// <summary>
        /// 城市id
        /// </summary>
        [SqlField]
        public int CityCode { get; set; } = 0;
        /// <summary>
        /// 区域id
        /// </summary>
        [SqlField]
        public int AreaCode { get; set; } = 0;
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
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 餐馆公告
        /// </summary>
        [SqlField]
        public string Notice { get; set; } = string.Empty;

        /// <summary>
        /// 营业开始时间
        /// </summary>
        [SqlField]
        public string StartShopTime { get; set; } = "00:00";
        /// <summary>
        /// 营业结束时间
        /// </summary>
        [SqlField]
        public string EndShopTime { get; set; } = "23:59";

        /// <summary>
        /// 配送方式
        /// </summary>
        [SqlField]
        public string GiveWays {
            get { return Enum.GetName(typeof(miniAppOrderGetWay), DistributionWay); }
            set { value = Enum.GetName(typeof(miniAppOrderGetWay), DistributionWay); }
        }

        /// <summary>
        /// 配送方式，看枚举miniAppOrderGetWay
        /// </summary>
        [SqlField]
        public int DistributionWay { get; set; } = 0;
        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double Lng { get; set; } = 0.00f;
        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double Lat { get; set; } = 0.00f;
        /// <summary>
        /// 配送范围公里数
        /// </summary>
        [SqlField]
        public int DeliveryRange { get; set; }
        /// <summary>
        /// 营业日期(周几)
        /// </summary>
        [SqlField]
        public string OpenDateStr { get; set; } = "周一周二周三周四周五";

        /// <summary>
        /// 营业时间
        /// </summary>
        [SqlField]
        public string OpenTimeJson { get; set; } = "[{'StartTime':'08:00','EndTime':'20:00'},{'StartTime':'08:00','EndTime':'20:00'},{'StartTime':'08:00','EndTime':'20:00'}]";

        public List<FoodOpenTimeModel> getOpenTimeList => !string.IsNullOrEmpty(OpenTimeJson) ?  SerializeHelper.DesFromJson<List<FoodOpenTimeModel>>(OpenTimeJson) : new List<FoodOpenTimeModel>();

        /// <summary>
        /// 开启外卖功能 0:关闭  1:开启
        /// </summary>
        [SqlField]
        public int TakeOut { get; set; } = 1;

        /// <summary>
        /// 开启店内点餐
        /// </summary>
        [SqlField]
        public int TheShop { get; set; } = 1;

        /// <summary>
        /// 起送价
        /// </summary>
        [SqlField]
        public int OutSide { get; set; } = 0;
        /// <summary>
        /// 起送价(显示)
        /// </summary>
        public string OutSideStr { get { return (OutSide * 0.01).ToString("0.00"); } }


        /// <summary>
        /// 配送费
        /// </summary>
        [SqlField]
        public int ShippingFee { get; set; } = 0;

        /// <summary>
        /// 配送费
        /// </summary>
        public string ShippingFeeStr { get { return (ShippingFee * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 营业状态 1营业 0打烊
        /// </summary>
        public int openState
        {
            get
            { 
                string dayweekAsEngLish = DateTime.Now.DayOfWeek.ToString();
                string dayweek = "";

                switch (dayweekAsEngLish)
                {
                    case "Monday":
                        dayweek = "周一";
                        break;
                    case "Tuesday":
                        dayweek = "周二";
                        break;
                    case "Wednesday":
                        dayweek = "周三";
                        break;
                    case "Thursday":
                        dayweek = "周四";
                        break;
                    case "Friday":
                        dayweek = "周五";
                        break;
                    case "Saturday":
                        dayweek = "周六";
                        break;
                    case "Sunday":
                        dayweek = "周日";
                        break;
                }

                int dayNowTime = Convert.ToInt32(DateTime.Now.ToString("HH:mm").Replace(":", ""));

                if (OpenDateStr.Contains(dayweek) )
                {
                    int isOpen = 1;

                    //&& getOpenTimeList.Any(x => dayNowTime >= Convert.ToInt32(x.StartTime.Replace(":", ""))
                    //                                && dayNowTime <= Convert.ToInt32(x.EndTime.Replace(":", "")))
                    if (getOpenTimeList != null && getOpenTimeList.Count > 0)
                    {
                        var item = getOpenTimeList[0];
                        int startTime = Convert.ToInt32(item.StartTime.Replace(":", ""));
                        int endTime = Convert.ToInt32(item.EndTime.Replace(":", ""));
                        if (startTime > endTime)
                        {
                            //表示跨天
                            if (dayNowTime > startTime || dayNowTime < endTime)
                            {
                                isOpen = 1;
                            }
                            else
                            {
                                isOpen = 0;
                            }
                        }
                        else
                        {
                            //表示不跨天
                            if (!(dayNowTime >= startTime && dayNowTime <= endTime))
                            {
                                isOpen = 0;
                            }
                        }
                    }

                    return isOpen;
                }
               
                return 0;
            }
        }

        /// <summary>
        /// 自动接单
        /// </summary>
        [SqlField]
        public int AutoAcceptOrder { get; set; } = 0;

        [SqlField]
        public int PackinFee { get; set; } = 0;
        public string PackinFeeStr
        {
            get
            {
                return (PackinFee * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 功能入口
        /// </summary>
        [SqlField]
        public string configJson { get; set; } = "";

        /// <summary>
        /// 功能入口实体
        /// </summary>
        public FoodConfigModel funJoinModel => !string.IsNullOrEmpty(configJson) ?  SerializeHelper.DesFromJson<FoodConfigModel>(configJson) : new FoodConfigModel();
        
        /// <summary>
        /// 主店ID
        /// </summary>
        [SqlField]
        public int masterStoreId { get; set; }

        /// <summary>
        /// 上下架状态 1上架 0下架
        /// </summary>
        [SqlField]
        public int state { get; set;}

        /// <summary>
        /// 过期时间
        /// </summary>
        [SqlField]
        public DateTime overTime { get; set; }

        /// <summary>
        /// 管理员名称
        /// </summary>
        [SqlField]
        public string masterNickName { get; set; } = string.Empty;

        

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
        public int VoiceType { get; set; } =0;
        #endregion

        /// <summary>
        /// 开启线下支付 0:关闭 1:开启
        /// </summary>
        [SqlField]
        public int underlinePay { get; set; } = 1;
        /// <summary>
        /// 快跑者配送团队token
        /// </summary>
        public string KPZTeamToken { get; set; }
        /// <summary>
        /// 快跑者注册的手机号
        /// </summary>
        public string KPZPhone { get; set; }
        /// <summary>
        /// UU配送配置
        /// </summary>
        public UUCustomerRelation UUModel { get; set; }
    }


    public class FoodConfigModel
    {
        /// <summary>
        /// 会员卡
        /// </summary>
        public bool vipCard { get; set; } = false;

        /// <summary>
        /// 外卖
        /// </summary>
        public bool takeOut { get; set; } = false;
        /// <summary>
        /// 店内点餐
        /// </summary>
        public bool theShop { get; set; } = false;
        /// <summary>
        /// 储值有礼
        /// </summary>
        public bool saveMoney { get; set; } = false;

        /// <summary>
        /// 推荐好友
        /// </summary>
        public bool theShard { get; set; } = false;

        /// <summary>
        /// 门店图片显示方式   0不显示/1大图/2小图/3轮播
        /// </summary>
        public int pictureShowType { get; set; } = 0;
        /// <summary>
        /// 分享配置
        /// </summary>
        public FoodShare shareConfig { get; set; }

        /// <summary>
        /// 储值功能开关
        /// </summary>
        public bool canSaveMoneyFunction { get; set; } = true;

        /// <summary>
        /// 满减规则开关
        /// </summary>
        public bool discountRuleSwitch { get; set; } = false;

        /// <summary>
        /// 新用户首单立减
        /// </summary>
        public float newUserFirstOrderDiscountMoney { get; set; }
        
        /// <summary>
        /// 用户首单立减
        /// </summary>
        public float userFirstOrderDiscountMoney { get; set; }


        /// <summary>
        /// 排队开关
        /// </summary>
        public bool sortQueueSwitch { get; set; } = false;

        /// <summary>
        /// 下一个队列号
        /// </summary>
        public int sortNo_next { get; set; } = 1;

        /// <summary>
        /// 是否显示排队入口 - 小程序前端显示
        /// </summary>
        public bool sortQueueShowSwitch { get; set; } = false;


        /// <summary>
        /// 是否显示预约入口 - 小程序前端显示
        /// </summary>
        public bool reservationShowSwitch { get; set; } = false;

        public bool reservationSwitch { get; set; } = true;

        public bool reservationPrint { get; set; }
    }

    /// <summary>
    /// 分享配置模型
    /// </summary>
    public class FoodShare
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public int FoodId { get; set; } = 0;

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// 广告语
        /// </summary>
        public string ADTitle { get; set; } = string.Empty;

        /// <summary>
        /// 小程序码
        /// </summary>
        public string Qrcode { get; set; } = string.Empty;

        /// <summary>
        /// 分享图样式类别Id  总共7种
        /// </summary>
        public int StyleType { get; set; } = 0;


        /// <summary>
        /// 店铺Logo
        /// </summary>
        public List<object> Logo { get; set; } = new List<object>();

        /// <summary>
        /// 广告图
        /// </summary>
        public List<string> ADImg { get; set; } = new List<string>();




    }


    /// <summary>
    /// 营业时间模型
    /// </summary>
    public class FoodOpenTimeModel
    {
        public FoodOpenTimeModel() { }

        public string StartTime;

        public string EndTime;
    }

}
