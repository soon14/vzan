using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 页面设置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntSetting
    {
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true)]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 页面配置
        /// </summary>
        [SqlField]
        public string pages { get; set; } = string.Empty;
        /// <summary>
        /// 页面创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
        /// <summary>
        /// 页面更新时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; } = DateTime.Now;
        /// <summary>
        /// 产品类型
        /// </summary>
        public List<EntGoodType> goodtype { get; set; }

        public string tplname { get; set; } = string.Empty;

        /// <summary>
        /// 配置json string 串
        /// </summary>
        [SqlField]
        public string configJson { get; set; }

        /// <summary>
        /// 同步小未案例 0:不同步 1：同步
        /// </summary>
        [SqlField]
        public int syncmainsite { get; set; } = 0;
        /// <summary>
        /// 功能入口实体
        /// </summary>
        public EntConfigModel funJoinModel => !string.IsNullOrEmpty(configJson) ? Utility.SerializeHelper.DesFromJson<EntConfigModel>(configJson) : new EntConfigModel();

        /// <summary>
        /// 多门店使用的店铺Id
        /// </summary>
        public int storeId { get; set; }

        /// <summary>
        /// 门店所属总店Id
        /// </summary>
        public int HomeId { get; set; }

        /// <summary>
        /// 底部导航,我的导航项配置
        /// </summary>

        [SqlField]
        public string MeConfigJson { get; set; } = Newtonsoft.Json.JsonConvert.SerializeObject(new EntNavItem { name = "我的", icon = "icon-personal4-33", img = "" });

        public EntNavItem MeConfig
        {
            get
            {
                if (string.IsNullOrEmpty(MeConfigJson))
                    return new EntNavItem();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<EntNavItem>(MeConfigJson);
            }
        }

        //public List<EntPage> pageModels { get; set; }

    }
    public class EntPage
    {
        public List<object> coms { get; set; }
        public string def_name { get; set; } = string.Empty;
        public bool sel { get; set; } = false;
        public int selComIndex { get; set; } = 0;
        public int skin { get; set; } = 0;
        public string target { get; set; } = "_self";
        public string name { get; set; } = string.Empty;
        public string img { get; set; } = string.Empty;
    }

    public class EntConfigModel
    {
        /// <summary>
        /// 开启客户储值功能
        /// </summary>
        public bool canSaveMoneyFunction { get; set; } = false;
    }

    public class EntCom
    {
        public string type { get; set; }
        public string name { get; set; }
        public List<EntNavItem> navlist { get; set; }

        public string goodShowType { get; set; }

        public bool isShowGoodCatNav { get; set; } = false;
        public int GoodCatNavStyle { get; set; } = 4;
        public bool isShowGoodSearch { get; set; } = false;
        public bool isShowGoodPriceSort { get; set; } = false;
        public bool isShowFilter { get; set; } = false;
        public List<EntGoodType> goodCat { get; set; } = new List<EntGoodType>();
        public List<EntIndutypes> goodExtCat { get; set; } = new List<EntIndutypes>();
        public bool pickallgoodcat { get; set; } = false;

        public bool subscribeSwitch { get; set; } = false;

        public bool isShowPrice { get; set; } = true;
        public List<EntGoods> items { get; set; } = new List<EntGoods>();//产品详情

        public string title { get; set; }

        public int formstyle { get; set; } = 1;

    }

    public class EntNavItem
    {
        public string name { get; set; } = string.Empty;
        public string img { get; set; } = string.Empty;
        public int url { get; set; } = -1;
        public bool sel { get; set; } = false;
        public string icon { get; set; } = "";
        public string furl { get; set; } = "-1";//链接跳转
        public int urltype { get; set; } = -1;//-1=不跳转，0=跳转到页面，1=跳转到小程序，2=跳转链接功能，3=产品详情页，4=产品分类，5=拼团详情页，6=砍价详情页
        public string appid { get; set; } = string.Empty;
        public List<EntGoodType> itemstype { get; set; } = new List<EntGoodType>();//分类
        public List<EntGoods> items { get; set; } = new List<EntGoods>();//产品详情
        public string btnType { get; set; } = string.Empty;// ""=不显示|yuyue=预约|buy =购买
        public string path { get; set; } = string.Empty;
        public string target { get; set; } = "_self";
    }

    /// <summary>
    /// 轮播图选项
    /// </summary>
    public class EntSliderItem
    {
        public string furl { get; set; } = "-1";//链接跳转
        public string img { get; set; }
        public string target { get; set; } = "_blank";
        public int url { get; set; } = -1;
        public bool sel { get; set; } = false;
        public int urltype { get; set; } = -1;//-1=不跳转，0=跳转到页面，1=跳转到小程序，2=跳转链接功能，3=产品详情页，4=产品分类，5=拼团详情页，6=砍价详情页
        public string appid { get; set; }
        public List<EntGoodType> itemstype { get; set; } = new List<EntGoodType>();//分类
        public List<EntGoods> items { get; set; } = new List<EntGoods>();//产品详情
        public string btnType { get; set; } = string.Empty;// ""=不显示|yuyue=预约|buy =购买
        public string path { get; set; } = string.Empty;
    }
}
