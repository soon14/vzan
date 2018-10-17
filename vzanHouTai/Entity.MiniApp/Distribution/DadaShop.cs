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
    /// <summary>
    /// 门店body对象
    /// </summary>
    public class DadaShop
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 门店名称
        /// </summary>
        [SqlField]
        public string station_name { get; set; } = string.Empty;
        /// <summary>
        /// 门店编码,可自定义,但必须唯一;若不填写,则系统自动生成
        /// </summary>
        [SqlField]
        public string origin_shop_id { get; set; } = string.Empty;
        /// <summary>
        /// 区域名称(如,浦东新区)
        /// </summary>
        [SqlField]
        public string area_name { get; set; } = string.Empty;
        /// <summary>
        /// 门店地址
        /// </summary>
        [SqlField]
        public string station_address { get; set; } = string.Empty;
        /// <summary>
        /// 联系人姓名
        /// </summary>
        [SqlField]
        public string contact_name { get; set; } = string.Empty;
        /// <summary>
        /// 城市名称(如,上海)
        /// </summary>
        [SqlField]
        public string city_name { get; set; } = string.Empty;
        /// <summary>
        /// 业务类型(食品小吃-1,饮料-2,鲜花-3,文印票务-8,便利店-9,水果生鲜-13,同城电商-19, 医药-20,蛋糕-21,酒品-24,小商品市场-25,服装-26,汽修零配-27,数码-28,小龙虾-29, 其他-5)
        /// </summary>
        [SqlField]
        public int business { get; set; }
        /// <summary>
        /// 门店经度
        /// </summary>
        [SqlField]
        public double lng { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;
        /// <summary>
        /// 门店纬度
        /// </summary>
        [SqlField]
        public double lat { get; set; }
        /// <summary>
        /// 联系人身份证
        /// </summary>
        [SqlField]
        public string id_card { get; set; } = string.Empty;
        /// <summary>
        /// 达达商家app账号(若不需要登陆app,则不用设置)
        /// </summary>
        [SqlField]
        public string username { get; set; } = string.Empty;
        /// <summary>
        /// 达达商家app密码(若不需要登陆app,则不用设置)
        /// </summary>
        [SqlField]
        public string password { get; set; } = string.Empty;
        /// <summary>
        /// 达达商户表ID
        /// </summary>
        [SqlField]
        public int merchantid { get; set; }
    }
}
