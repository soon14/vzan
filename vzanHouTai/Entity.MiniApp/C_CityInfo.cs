using Entity.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 同城系统信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_CityInfo
    {
        public C_CityInfo() { }

        /// <summary>
        /// 同城ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        //[Required(ErrorMessage = "同城名称不能为空 !")]
        //[StringLength(20, ErrorMessage = "同城名称不能超过20个字 !")]
        public string CName { get; set; }
        /// <summary>
        /// 论坛 Minisns ID
        /// </summary>
        [SqlField]
        public int MiniSnsId { get; set; }

        /// <summary>
        /// 同城区域代码 
        /// </summary>
        [SqlField]
        public int AreaCode { get; set; }

        /// <summary>
        /// 同城区域代码
        /// </summary>
        [SqlField]
        public int CityInfoTag { get; set; }

        [SqlField]
        public DateTime CreateDate { get; set; }

        [SqlField]
        public DateTime EndDate { get; set; }
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 创建者OPENID
        /// </summary>
        [SqlField]
        public string OpenId { get; set; }

        /// <summary>
        /// 创建者UnionId
        /// </summary>
        [SqlField]
        public string UnionId { get; set; }


        /// <summary>
        /// 同城LOGO，分享用到
        /// </summary>
        [SqlField]
        public string LogoUrl { get; set; }
        /// <summary>
        /// 客服二维码
        /// </summary>
        [SqlField]
        public string CustomerCodeUrl { get; set; }
        /// <summary>
        /// 同城的类型，0：同城。1：行业版   枚举 CityInfoTypeEnum
        /// </summary>
        [SqlField]
        public int IsThirdTools { get; set; } = 0;
        /// <summary>
        /// 是否有子区域已被占用
        /// </summary>
        [SqlField]
        public int SubAreaOccupy { get; set; } = 0;


        /// <summary>
        /// 同城简介，分享出去时显示的详情
        /// </summary>
        [SqlField]
        public string Notes { get; set; }

        /// <summary>
        /// pv 浏览量，热度值
        /// </summary>
        [SqlField]
        public int ViewCount { get; set; }

        ///// <summary>
        ///// 同城现金   xxxxxxxxxxxxx 用不到的字段，暂时屏蔽 xxxxxxxxxxxxxxxxxxxxxxx xiaowei 2017-07-03 14:36:00
        ///// </summary>
        //[SqlField(IsUpdateRemove = true)]
        //public int Cash { get; set; } = 0;
        ///// <summary>
        ///// 同城历史现金
        ///// </summary>
        //[SqlField(IsUpdateRemove = true)]
        //public int HistoryCash { get; set; } = 0;
        /// <summary>
        /// 是否开启轮播图广告
        /// </summary>
        [SqlField]
        public int IsOpenBnAdv { get; set; } = 0;
        /// <summary>
        /// 轮播图最多张数
        /// </summary>
        [SqlField]
        public int BannerCount { get; set; } = 8;

        /// <summary>
        /// 同城过期提醒时间
        /// </summary>
        [SqlField]
        public string EnterUrl { get; set; }

        /// <summary>
        /// 以下为总后台展示数据用到的字段
        /// </summary>
        public string ForumName { get; set; }
        public string AreaName { get; set; }
        public string ForumLogo { get; set; }
        /// <summary>
        /// 发帖是否需要审核通过 0否，1是
        /// </summary>
        
        [SqlField]
        public int IsPostSH { get; set; }

        /// <summary>
        /// 同城功能开关配置JSON
        /// </summary>
        [SqlField]
        public string ConfigJson { get; set; }

        /// <summary>
        /// 店铺默认图片
        /// </summary>
        [SqlField]
        public string StoreDefImgUrl { get; set; } = "http://oss.vzan.cc/image/jpg/2016/12/2/154847faf8bfa75fd445f2b990702f09abf3aa.jpg";

        /// <summary>
        /// 论坛默认图片
        /// </summary>
        [SqlField]
        public string ForumDefImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// 发帖数
        /// </summary>
        [SqlField]
        public int PostCount { get; set; } = 0;

        /// <summary>
        /// 商家数
        /// </summary>
        [SqlField]
        public int StoreCount { get; set; } = 0;

        ///// <summary>
        ///// 是否绑定公众号
        ///// </summary>
        //[SqlField]
        //public CityInfoIsOpenBinding IsOpenBinding { get; set; }
      

        /// <summary>
        /// 同城功能开关配置JSON
        /// </summary>
        public CityConfig CCityConfig
        {
            get
            {
                if (!string.IsNullOrEmpty(ConfigJson))
                {
                    return  SerializeHelper.DesFromJson<CityConfig>(ConfigJson);
                    //return Newtonsoft.Json.JsonConvert.DeserializeObject<CityConfig>(ConfigJson);
                }
                return new CityConfig();
            }
        }

        /// <summary>
        /// 分销同城默认分成比例配置JSON
        /// </summary>
        [SqlField]
        public string FxConfigJson { get; set; }

        /// <summary>
        /// 分销同城默认分成配置JSON
        /// </summary>
        public FxCitySubConfig FxCCityConfig => !string.IsNullOrEmpty(FxConfigJson) ?  SerializeHelper.DesFromJson<FxCitySubConfig>(FxConfigJson) : new FxCitySubConfig();

        /// <summary>
        /// 分销邀请分享图片
        /// </summary>
        [SqlField]
        public string ShareFxImg { get; set; } = string.Empty;

        /// <summary>
        /// 城主设置第三方支付费率
        /// </summary>
        [SqlField]
        public double PayRate { get; set; } = 0;

        /// <summary>
        /// 城主设置店铺分销费率
        /// </summary>
        [SqlField]
        public int StoreFxRate { get; set; } = 0;

        /// <summary>
        /// 独立域名
        /// </summary>
        [SqlField]
        public string CusDomain { get; set; }
        /// <summary>
        /// appid ,用于js  sdk 
        /// </summary>
        [SqlField]
        public string AppId { get; set; }
        /// <summary>
        /// appsecret,用于js  sdk 
        /// </summary>
        [SqlField]
        public string AppSecret { get; set; }
        //行业版同城服务级别 0普通1中等2高级
        [SqlField]
        public int PriceLevel { get; set; } = 0;

        /// <summary>
        /// 非独代同城的区域
        /// </summary>
        [SqlField]
        public int RegionCode { get; set; }
    }

    [Serializable]
    public class CityConfig
    {
        /// <summary>
        /// 是否开启活动
        /// </summary>
        public bool Act { get; set; } = true;//是否开启活动
        /// <summary>
        /// 是否开启店铺
        /// </summary>
        public bool Store { get; set; } = true;//是否开启店铺功能
        /// <summary>
        /// 是否显示招聘求职页面推荐企业
        /// </summary>
        public bool SCN { get; set; } = true;//是否显示招聘求职页面推荐企业
        /// <summary>
        /// 是否显示招聘页面提示完善企业信息弹窗
        /// </summary>
        public bool SCC { get; set; } = true;//是否显示招聘页面提示完善企业信息弹窗
        /// <summary>
        /// 帖子是否显示浏览量
        /// </summary>
        public bool SC { get; set; } = true;

        /// <summary>
        /// 自定义链接的排序，true 默认后面
        /// </summary>
        public bool AS { get; set; } = true;

        /// <summary>
        /// 首页按钮每页的个数： 5,10,15,20...
        /// </summary>
        public int LNum { get; set; } = 10;

        /// <summary>
        /// 首页按钮每行的个数： 4, 5
        /// </summary>
        public int CNum { get; set; } = 5;

        /// <summary>
        /// 公众号推送开关
        /// </summary>
        public bool PU { get; set; } = true;

        /// <summary>
        /// 好店名称
        /// </summary>
        public string SN { get; set; } = "好店";
        /// <summary>
        /// 同城卡审核开关
        /// </summary>
        public bool SCcard { get; set; } = true;//新发布的同城卡折扣优惠是否需要审核
        /// <summary>
        /// 代金卷审核开关
        /// </summary>
        public bool SCcoupon { get; set; } = true;//新发布的代金卷优惠是否需要审核

        /// <summary>
        /// 自定义关注条名称
        /// </summary>
        public string AN { get; set; } = "更多精彩，长按关注";

        /// <summary>
        /// 切换帖子简版
        /// </summary>
        public bool PV { get; set; } = false;

        /// <summary>
        /// 人人分销开关
        /// </summary>
        public bool rrFx { get; set; } = false;

        /// <summary>
        /// 五一活动开关
        /// </summary>
        public bool CouponArt { get; set; } = false;
        /// <summary>
        /// 强制店铺上传营业执照
        /// </summary>
        public bool ForceShopBLicence { get; set; } = false;
        /// <summary>
        /// 浏览量统计方式，0列表页+1，1.详细页加+1
        /// </summary>
        public int ST { get; set; } = 0;

        /// <summary>
        /// 店铺分销开关
        /// </summary>
        public bool StoreFx { get; set; } = false;

        /// <summary>
        /// 论坛名称
        /// </summary>
        public string MN { get; set; }

        /// <summary>
        /// 论坛链接
        /// </summary>
        public string MU { get; set; }

        /// <summary>
        /// 是否开启新版红包
        /// </summary>
        public bool NR { get; set; } = true;

        /// <summary>
        /// 店铺发批发商城是否收费，默认收费， 140825  对该同城进行不收费配置
        /// </summary>
        public bool PF { get; set; } = true;
        /// <summary>
        /// 同城商铺默认手续费
        /// </summary>
        public double euro { get; set; } = 0d;

        /// <summary>
        /// 帖子刷新首次刷新免费：0，关闭，1 开启
        /// </summary>
        public int FF { get; set; } = 0;

        /// <summary>
        /// 长期拼车开关
        /// </summary>
        public bool LC { get; set; } = false;

    }

    [Serializable]
    public class FxCitySubConfig
    {
        /// <summary>
        /// 发帖分成利率
        /// </summary>
        public int AddPostRate { get; set; } = 0;
        /// <summary>
        /// 帖子置顶分成利率
        /// </summary>
        public int TopPostRate { get; set; } = 0;
        /// <summary>
        /// 店铺入驻分成利率
        /// </summary>
        public int AddStoreRate { get; set; } = 0;
        /// <summary>
        /// 店铺置顶分成利率
        /// </summary>
        public int TopStoreRate { get; set; } = 0;
        /// <summary>
        /// 店铺VIP分成利率
        /// </summary>
        public int VIPStoreRate { get; set; } = 0;
        /// <summary>
        /// 同城卡分成利率
        /// </summary>
        public int CityCardRate { get; set; } = 0;
        /// <summary>
        /// 企业置顶分成利率
        /// </summary>
        public int TopCompanyRate { get; set; } = 0;
        /// <summary>
        /// 轮播图付费分成利率
        /// </summary>
        public int AddCarouselRate { get; set; } = 0;
        /// <summary>
        /// 店铺认领分成利率
        /// </summary>
        public int ShopClaimUnitRate { get; set; } = 0;
        /// <summary>
        /// 申请分销分成利率
        /// </summary>
        public int ApplyCitySubRate { get; set; } = 0;
    }



    public class CityInfoSimple
    {
        public int CityInfoId { get; set; }

        public string CName { get; set; }


        public int AreaCode { get; set; }
    }
}
