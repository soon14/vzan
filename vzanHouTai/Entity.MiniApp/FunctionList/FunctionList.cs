using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.FunctionList
{
    /// <summary>
    /// 版本功能清单列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FunctionList
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 小程序模板类别
        /// </summary>
        [SqlField]
        public int TemplateType { get; set; } = 0;

        /// <summary>
        /// 版本Id  对应XcxAppAccountRelation里的industr
        /// </summary>
        [SqlField]
        public int VersionId { get; set; } = 0;

        /// <summary>
        /// 小程序版本类型名称 基础版 高级版 尊享版 旗舰版
        /// </summary>
        [SqlField]
        public string VersionName { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public int Price { get; set; }



        [SqlField]
        public string PageConfig { get; set; } = string.Empty;

        [SqlField]
        public string ComsConfig { get; set; } = string.Empty;

        [SqlField]
        public string ProductMgr { get; set; } = string.Empty;

        [SqlField]
        public string StoreConfig { get; set; } = string.Empty;


        [SqlField]
        public string NewsMgr { get; set; } = string.Empty;

        [SqlField]
        public string MessageMgr { get; set; } = string.Empty;

        [SqlField]
        public string MarketingPlugin { get; set; } = string.Empty;

        [SqlField]
        public string OperationMgr { get; set; } = string.Empty;

        [SqlField]
        public string FuncMgr { get; set; } = string.Empty;

        public PageConfig PageConfigModel { get; set; } = new PageConfig();
        public ComsConfig ComsConfigModel { get; set; } = new ComsConfig();
        public ProductMgr ProductMgrModel { get; set; } = new ProductMgr();
        public StoreConfig StoreConfigModel { get; set; } = new StoreConfig();
        public NewsMgr NewsMgrModel { get; set; } = new NewsMgr();
        public MessageMgr MessageMgrModel { get; set; } = new MessageMgr();
        public MarketingPlugin MarketingPluginModel { get; set; } = new MarketingPlugin();
        public OperationMgr OperationMgrModel { get; set; } = new OperationMgr();
        public FuncMgr FuncMgrModel { get; set; } = new FuncMgr();




        /// <summary>
        /// 增加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

    }


    public class VersionType
    {
        public int VersionId { get; set; }
        public string VersionName { get; set; }
        public string VersionPrice { get; set; }

        public int LimitCount { get; set; }
    }




    /// <summary>
    /// 自定义页面
    /// </summary>
    public class PageConfig
    {
        /// <summary>
        /// 自定义页面最大数量
        /// </summary>
        public int PageMaxCount { get; set; } = 100;
    }


    /// <summary>
    /// 组件配置
    /// </summary>
    public class ComsConfig
    {
        public int Bottomnav { get; set; } = 1;
        public int Imgnav { get; set; } = 1;
        public int Slider { get; set; } = 1;
        public int Goodlist { get; set; } = 1;
        public int Good { get; set; } = 1;
        public int Form { get; set; } = 1;
        public int Richtxt { get; set; } = 1;
        public int Video { get; set; } = 0;
        public int Bgaudio { get; set; } = 0;
        public int Map { get; set; } = 1;
        public int Img { get; set; } = 1;
        public int News { get; set; } = 0;
        public int AD { get; set; } = 1;
        public int FlashDeal { get; set; } = 1;
       
      
        public int Share { get; set; } = 0;
        public int Live { get; set; } = 0;
        public int Cutprice { get; set; } = 0;
        public int Joingroup { get; set; } = 0;
        public int Entjoingroup { get; set; } = 0;
        public int Coupons { get; set; } = 0;
        public int Spacing { get; set; } = 0;
        public int MagicCube { get; set; } = 0;
        public int Search { get; set; } = 0;
        public int ContactShopkeeper { get; set; } = 1;
      
    }

    /// <summary>
    /// 商品管理
    /// </summary>
    public class ProductMgr
    {
        public int ProductMaxCount { get; set; } = 10000;
        public int ProductType { get; set; } = 1;
        public int ProductParas { get; set; } = 1;
        public int ProductSpecification { get; set; } = 1;
        public int ProductLabel { get; set; } = 1;
        public int ProductReservation { get; set; } = 1;
        public int ProductBuy { get; set; } = 1;
    }

    /// <summary>
    /// 店铺配置
    /// </summary>
    public class StoreConfig
    {
        public int FreightTemplate { get; set; } = 0;
        public int SwitchReceiving { get; set; } = 0;
    }


    /// <summary>
    /// 资讯管理
    /// </summary>
    public class NewsMgr
    {
        public int NewsContent { get; set; } = 0;
        public int NewsType { get; set; } = 0;
    }

    /// <summary>
    /// 模板消息管理
    /// </summary>
    public class MessageMgr
    {
        public int TemplateMessage { get; set; } = 0;
    }


    /// <summary>
    /// 营销插件
    /// </summary>
    public class MarketingPlugin
    {
        public int ReductionCard { get; set; } = 0;
    }

    /// <summary>
    /// 运营管理
    /// </summary>
    public class OperationMgr
    {
        public int VIP { get; set; } = 0;
        public int SaveMoney { get; set; } = 0;
        public int Integral { get; set; } = 0;
        public int Distribution { get; set; } = 0;
    }

    /// <summary>
    /// 功能管理
    /// </summary>
    public class FuncMgr
    {
        public int IM { get; set; } = 0;
        public int ReserveShopping { get; set; } = 0;
        public int SortShopping { get; set; } = 0;
        public int StoreCode { get; set; } = 0;
        public int ProductQrcodeSwitch { get; set; } = 0;
        public int JumpMiniApp { get; set; } = 0;
        public int CashOnDelivery { get; set; } = 0;
    }



}
