using System;
using System.Collections.Generic;
using Entity.Base;
using Utility; 

namespace Entity.MiniApp
{
    /// <summary>
    /// 同城配置表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_CityCofig
    {
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 同城 Id
        /// </summary>
        [SqlField]
        public int CityInfoId { get; set; }

        /// <summary>
        /// 是否显示商户数量
        /// </summary>
        [SqlField]
        public bool IsShowStoreCount { get; set; }

        /// <summary>
        /// 是否显示帖子数量
        /// </summary>
        [SqlField]
        public bool IsShowPostCount { get; set; }

        /// <summary>
        /// 是否显示同城浏览量
        /// </summary>
        [SqlField]
        public bool IsShowViewCount { get; set; }

        /// <summary>
        /// 是否显示同城发帖动态
        /// </summary>
        [SqlField]
        public bool IsShowCityNoteOfAddPost { get; set; } = true;

        /// <summary>
        /// 是否显示同城店铺入驻动态
        /// </summary>
        [SqlField]
        public bool IsShowCityNoteOfAddStore { get; set; } = true;


        /// <summary>
        /// 是否显示红包入口
        /// </summary>
        [SqlField]
        public bool IsShowRedPacket { get; set; } = true;
        [SqlField]
        public string RedPacketConfig { get; set; }
        public AdSitConfig CRedPacketConfig => !string.IsNullOrEmpty(RedPacketConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(RedPacketConfig) : new AdSitConfig();
        /// <summary>
        /// 是否显示拼团入口
        /// </summary>
        [SqlField]
        public bool IsShowPingTuan { get; set; } = true;
        [SqlField]
        public string PingTuanConfig { get; set; }
        public AdSitConfig CPingTuanConfig => !string.IsNullOrEmpty(PingTuanConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(PingTuanConfig) : new AdSitConfig();

        /// <summary>
        /// 是否显示抢购入口
        /// </summary>
        [SqlField]
        public bool IsShowQiangGou { get; set; } = true;
        [SqlField]
        public string QiangGouConfig { get; set; }
        public AdSitConfig CQiangGouConfig => !string.IsNullOrEmpty(QiangGouConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(QiangGouConfig) : new AdSitConfig();

        /// <summary>
        /// 是否显示同城卡入口
        /// </summary>
        [SqlField]
        public bool IsShowTongChengKa { get; set; } = true;
        [SqlField]
        public string TongChengKaConfig { get; set; }
        public AdSitConfig CTongChengKaConfig => !string.IsNullOrEmpty(TongChengKaConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(TongChengKaConfig) : new AdSitConfig();

        /// <summary>
        /// 是否显示优惠券入口
        /// </summary>
        [SqlField]
        public bool IsShowDaiJinQuan { get; set; } = true;
        [SqlField]
        public string DaiJinQuanConfig { get; set; }
        public AdSitConfig CDaiJinQuanConfig => !string.IsNullOrEmpty(DaiJinQuanConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(DaiJinQuanConfig) : new AdSitConfig();

        /// <summary>
        /// 是否显示砍价入口
        /// </summary>
        [SqlField]
        public bool IsShowKanJia { get; set; } = true;
        [SqlField]
        public string KanJiaConfig { get; set; }
        public AdSitConfig CKanJiaConfig => !string.IsNullOrEmpty(KanJiaConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(KanJiaConfig) : new AdSitConfig();

        /// <summary>
        /// 是否显示竞拍入口
        /// </summary>
        [SqlField]
        public bool IsShowAuction { get; set; } = true;
        [SqlField]
        public string AuctionConfig { get; set; }
        public AdSitConfig CAuctionConfig => !string.IsNullOrEmpty(AuctionConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(AuctionConfig) : new AdSitConfig();

        /// <summary>
        /// 是否显示本地商家
        /// </summary>
        [SqlField]
        public bool IsShowLocalStores { get; set; } = true;

        /// <summary>
        /// 本地商家排序    0 : 最新入驻商家     1 : 最新置顶商家
        /// </summary>
        [SqlField]
        public int LocalStoresSort { get; set; } = 1;

        /// <summary>
        /// 轮播图配置（Json格式）
        /// </summary>
        [SqlField]
        public string CarouselConfig { get; set; }

        /// <summary>
        /// 是否关闭首页推荐板块，默认0开启，1关闭
        /// </summary>
        [SqlField]
        public bool ModelOnOrOff { get; set; } = true;


        /// <summary>
        /// 虚拟店铺数（好店首页配置）
        /// </summary>
        [SqlField]
        public int StoreCount_S { get; set; }
        /// <summary>
        /// 虚拟浏览数（好店首页配置）
        /// </summary>
        [SqlField]
        public int ViewCount_S { get; set; }
        /// <summary>
        /// 虚拟发布数（同城首页配置）
        /// </summary>
        [SqlField]
        public int PostCount { get; set; }
        /// <summary>
        /// 是否显示商户数量（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowStoreCount_S { get; set; }
        /// <summary>
        /// 是否显示浏览数量（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowViewCount_S { get; set; }
        /// <summary>
        /// 是否显示同城店铺入驻动态（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowCityNoteOfAddStore_S { get; set; } = true;

        /// <summary>
        /// 是否显示拼团入口（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowPingTuan_S { get; set; } = true;
        [SqlField]
        public string PingTuanConfig_S { get; set; }
        /// <summary>
        /// 拼团广告位JSON配置（好店首页配置）
        /// </summary>
        public AdSitConfig CPingTuanConfig_S => !string.IsNullOrEmpty(PingTuanConfig_S) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(PingTuanConfig_S) : new AdSitConfig();
        /// <summary>
        /// 是否显示抢购入口（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowQiangGou_S { get; set; } = true;
        [SqlField]
        public string QiangGouConfig_S { get; set; }
        /// <summary>
        /// 抢购广告位JSON配置（好店首页配置）
        /// </summary>
        public AdSitConfig CQiangGouConfig_S => !string.IsNullOrEmpty(QiangGouConfig_S) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(QiangGouConfig_S) : new AdSitConfig();

        /// <summary>
        /// 是否显示同城卡入口（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowTongChengKa_S { get; set; } = true;
        [SqlField]
        public string TongChengKaConfig_S { get; set; }
        /// <summary>
        /// 同城卡广告位JSON配置（好店首页配置）
        /// </summary>
        public AdSitConfig CTongChengKaConfig_S => !string.IsNullOrEmpty(TongChengKaConfig_S) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(TongChengKaConfig_S) : new AdSitConfig();

        /// <summary>
        /// 是否显示优惠券入口（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowDaiJinQuan_S { get; set; } = true;
        [SqlField]
        public string DaiJinQuanConfig_S { get; set; }
        /// <summary>
        /// 优惠券广告位JSON配置（好店首页配置）
        /// </summary>
        public AdSitConfig CDaiJinQuanConfig_S => !string.IsNullOrEmpty(DaiJinQuanConfig_S) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(DaiJinQuanConfig_S) : new AdSitConfig();

        /// <summary>
        /// 是否显示砍价入口（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowKanJia_S { get; set; } = true;
        [SqlField]
        public string KanJiaConfig_S { get; set; }
        /// <summary>
        /// 砍价广告位JSON配置（好店首页配置）
        /// </summary>
        public AdSitConfig CKanJiaConfig_S => !string.IsNullOrEmpty(KanJiaConfig_S) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(KanJiaConfig_S) : new AdSitConfig();

        [SqlField]
        public string AdvanceStoreConfig { get; set; }
        /// <summary>
        /// 高级版店铺入驻营销工具配置
        /// </summary>
        public AdvanceStoreConfig CAdvanceStoreConfig => !string.IsNullOrEmpty(AdvanceStoreConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdvanceStoreConfig>(AdvanceStoreConfig) : new AdvanceStoreConfig();

        /// <summary>
        /// 是否显示竞拍入口（好店首页配置）
        /// </summary>
        [SqlField]
        public bool IsShowAuction_S { get; set; } = true;
        [SqlField]
        public string AuctionConfig_S { get; set; }
        /// <summary>
        /// 砍价广告位JSON配置（好店首页配置）
        /// </summary>
        public AdSitConfig CAuctionConfig_S => !string.IsNullOrEmpty(AuctionConfig_S) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(AuctionConfig_S) : new AdSitConfig();

        /// <summary>
        /// 首页本地商家栏目样式
        /// </summary>
        [SqlField]
        public int CStoreStyleConfig { get; set; } = (int)IndexStoreStyle.横排;
        /// <summary>
        /// 好店首页，Tab列表排序
        /// </summary>
        [SqlField]
        public string CStoreTabSortString { get; set; }
        public List<AdTabListSortConfig> CStoreTabSortConfig =>
               !string.IsNullOrWhiteSpace(CStoreTabSortString) ?
               Utility.Serialize.SerializeHelper.DesFromJson<List<AdTabListSortConfig>>(CStoreTabSortString) :
               new List<AdTabListSortConfig>
               {
                   new AdTabListSortConfig { Type = StoreIndexTabSort.hot.ToString(),Sort = (int)StoreIndexTabSort.hot,Name="推荐" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.New.ToString() ,Sort = (int)StoreIndexTabSort.New,Name="新入" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.red.ToString() ,Sort = (int)StoreIndexTabSort.red,Name="抢红包" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.coupon.ToString() ,Sort = (int)StoreIndexTabSort.coupon,Name="优惠券" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.discount.ToString(),Sort = (int)StoreIndexTabSort.discount ,Name="同城卡" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.tuan.ToString() ,Sort = (int)StoreIndexTabSort.tuan,Name="拼团" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.qiang.ToString() ,Sort = (int)StoreIndexTabSort.qiang,Name="同城热卖" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.near.ToString(),Sort = (int)StoreIndexTabSort.near ,Name="附近" }
               };
        public List<AdTabListSortConfig> IStoreTabSortConfig =>
              !string.IsNullOrWhiteSpace(CStoreTabSortString) ?
              Utility.Serialize.SerializeHelper.DesFromJson<List<AdTabListSortConfig>>(CStoreTabSortString) :
              new List<AdTabListSortConfig>
              {
                   new AdTabListSortConfig { Type = StoreIndexTabSort.hot.ToString(),Sort = (int)StoreIndexTabSort.hot,Name="推荐" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.New.ToString() ,Sort = (int)StoreIndexTabSort.New,Name="新入" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.ipost.ToString() ,Sort = (int)StoreIndexTabSort.New,Name="资讯" }
              };
        /// <summary>
        /// 首页，聚合Tab列表排序
        /// </summary>
        [SqlField]
        public string FairTabSortString { get; set; }
        public List<AdTabListSortConfig> CFairTabSortConfig =>
               !string.IsNullOrWhiteSpace(FairTabSortString) ?
               Utility.Serialize.SerializeHelper.DesFromJson<List<AdTabListSortConfig>>(FairTabSortString) :
               new List<AdTabListSortConfig>
               {
                   new AdTabListSortConfig { Type = StoreIndexTabSort.store.ToString() ,Sort = 1,Name="推荐商家" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.post.ToString() ,Sort = 2,Name="便民信息" },
                   new AdTabListSortConfig { Type = StoreIndexTabSort.aggregate.ToString(),Sort = 3,Name="商家动态" }
               };

        /// <summary>
        /// 入驻邀请码状态 0代表关闭
        /// </summary>
        [SqlField]
        public int AdmissionCodeState { get; set; }

        /// <summary>
        /// 分享开关
        /// </summary>
        [SqlField]
        public string ShareConfig { get; set; }

        /// <summary>
        /// 店铺、帖子三个集合的分享开关
        /// </summary>
        public List<AdShareConfig> CShareConfigList => string.IsNullOrWhiteSpace(ShareConfig) ? new List<AdShareConfig>() : Utility.Serialize.SerializeHelper.DesFromJson<List<AdShareConfig>>(ShareConfig);

        /// <summary>
        /// 说明
        /// </summary>
        [SqlField]
        public string Description { get; set; }

        /// <summary>
        /// 是否启用独立域名砍价分享朋友圈
        /// </summary>
        [SqlField]
        public bool IsOpenShareToMoments { get; set; } = false;
        [SqlField]
        public bool IsShowZiXun { get; set; } = true;
        [SqlField]
        public string ZiXunConfig { get; set; }
        public AdSitConfig CZiXunConfig => !string.IsNullOrEmpty(ZiXunConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(ZiXunConfig) : new AdSitConfig();

        /// <summary>
        /// 导航图标 -- 首页
        /// </summary>
        [SqlField]
        public string NavigatorIndexConfig { get; set; }
        public AdSitConfig CNavigatorIndexConfig => !string.IsNullOrEmpty(NavigatorIndexConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(NavigatorIndexConfig) : new AdSitConfig();

        /// <summary>
        /// 导航图标 -- 社区
        /// </summary>
        [SqlField]
        public string NavigatorForumConfig { get; set; }
        public AdSitConfig CNavigatorForumConfig => !string.IsNullOrEmpty(NavigatorForumConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(NavigatorForumConfig) : new AdSitConfig();

        /// <summary>
        /// 导航图标 -- 发布
        /// </summary>
        [SqlField]
        public string NavigatorPublishConfig { get; set; }
        public AdSitConfig CNavigatorPublishConfig => !string.IsNullOrEmpty(NavigatorPublishConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(NavigatorPublishConfig) : new AdSitConfig();

        /// <summary>
        /// 导航图标 -- 好店
        /// </summary>
        [SqlField]
        public string NavigatorGoodStoreConfig { get; set; }
        public AdSitConfig CNavigatorGoodStoreConfig => !string.IsNullOrEmpty(NavigatorGoodStoreConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(NavigatorGoodStoreConfig) : new AdSitConfig();

        /// <summary>
        /// 导航图标 -- 个人中心
        /// </summary>
        [SqlField]
        public string NavigatorCenterConfig { get; set; }
        public AdSitConfig CNavigatorCenterConfig => !string.IsNullOrEmpty(NavigatorCenterConfig) ? Utility.Serialize.SerializeHelper.DesFromJson<AdSitConfig>(NavigatorCenterConfig) : new AdSitConfig();
    }

    /// <summary>
    /// 广告位Json配置
    /// </summary>
    [Serializable]
    public class AdSitConfig
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 文案 1
        /// </summary>
        public string Text1 { get; set; }
        /// <summary>
        /// 文案 2
        /// </summary>
        public string Text2 { get; set; }
        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImgUrl { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }


    public class AdCarouselConfig
    {
        /// <summary>
        /// 轮播图类型
        /// </summary>
        public int AnnouncementType { get; set; }
        /// <summary>
        /// 是否开启广告
        /// </summary>
        public int IsOpenBnAdv { get; set; }

        /// <summary>
        /// 每个类型的轮播图最多数
        /// </summary>
        public int BannerCount { get; set; } = 8;
    }

    public class AdTabListSortConfig
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
    }

    /// <summary>
    /// 分享开关
    /// </summary>
    public class AdShareConfig
    {
        public int ItemType { get; set; }
        public int Count { get; set; }
        public int State { get; set; }
    }

    /// <summary>
    /// 高级版店铺入驻营销工具配置
    /// </summary>
    public class AdvanceStoreConfig
    {
        /// <summary>
        /// 砍价
        /// </summary>
        public bool Bargain { get; set; } = true;
        /// <summary>
        /// 拼团
        /// </summary>
        public bool Group { get; set; } = true;
        /// <summary>
        /// 抢优惠
        /// </summary>
        public bool Coupon { get; set; } = true;
        /// <summary>
        /// 竞拍
        /// </summary>
        public bool Auction { get; set; } = true;
        /// <summary>
        /// 店铺红包
        /// </summary>
        public bool RedPacket { get; set; } = true;
        /// <summary>
        /// 同城卡
        /// </summary>
        public bool CityCar { get; set; } = true;
        /// <summary>
        /// 抽奖
        /// </summary>
        public bool Lottery { get; set; } = true;
    }

}
