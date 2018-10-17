using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DistributionApiConfig
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public string app_id { get; set; }

        [SqlField]
        public string access_token { get; set; }

        [SqlField]
        public long expire_time { get; set; }

        [SqlField]
        public int rid { get; set; }

        [SqlField]
        public DateTime addtime { get; set; }

        [SqlField]
        public DateTime updatetime { get; set; }

        [SqlField]
        public int state { get; set; }

        /// <summary>
        /// 刷新时间间隔，单位分
        /// </summary>
        [SqlField]
        public int refreshtime { get; set; }
    }
    
    public class DistributionApiModel
    {
        public int userid { get; set; }
        public int aid { get; set; }
        public int storeid { get; set; }
       /// <summary>
       /// 订单ID
       /// </summary>
        public int orderid { get; set; }
        /// <summary>
        /// 订单小类型（OrderTypeEnum）
        /// </summary>
        public int ordertype { get; set; }
        /// <summary>
        /// 订单价格
        /// </summary>
        public int buyprice { get; set; }
        /// <summary>
        /// 配送费
        /// </summary>
        public int fee { get; set; }
        /// <summary>
        /// 订单详情
        /// </summary>
        public string ordercontent{get;set;}
        /// <summary>
        /// 模板类型
        /// </summary>
        public int temptype { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string accepterName { get; set; }
        /// <summary>
        /// 收货人电话
        /// </summary>
        public string accepterTelePhone { get; set; }
        /// <summary>
        /// 收货人地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string cityname { get; set; }
        /// <summary>
        /// 收货人留言
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 收货人纬度
        /// </summary>
        public double lat { get; set; }
        /// <summary>
        /// 收货人经度
        /// </summary>
        public double lnt { get; set; }
        /// <summary>
        /// uu配送价格令牌
        /// </summary>
        public string price_token { get; set; }
        /// <summary>
        /// uu配送需要支付价格
        /// </summary>
        public string need_payprice { get; set; }
    }
    
    public class ShopInfo
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string ShopTelphone { get; set; }
        /// <summary>
        /// 店铺地址
        /// </summary>
        public string ShopAddress { get; set; }
        /// <summary>
        /// 店铺经纬度，lat+','+lnt
        /// </summary>
        public string ShopTag { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public string CountyName { get; set; }
    }
}
