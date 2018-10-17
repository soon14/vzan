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
    public class FNStore
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 门店编号(支持数字、字母的组合)
        /// </summary>
        [SqlField]
        public string chain_store_code { get; set; }
        /// <summary>
        /// 门店名称(支持汉字、符号、字母的组合)
        /// </summary>
        [SqlField]
        public string chain_store_name { get; set; }
        /// <summary>
        /// 门店联系信息(手机号或座机或400)
        /// </summary>
        [SqlField]
        public string contact_phone { get; set; }
        /// <summary>
        /// 门店地址(支持汉字、符号、字母的组合)
        /// </summary>
        [SqlField]
        public string address { get; set; }
        /// <summary>
        /// 坐标属性（1:腾讯地图, 2:百度地图, 3:高德地图）
        /// </summary>
        [SqlField]
        public int position_source { get; set; }
        /// <summary>
        /// 门店经度(数字格式, 包括小数点, 取值范围0～180)
        /// </summary>
        [SqlField]
        public string longitude { get; set; }
        /// <summary>
        /// 门店纬度(数字格式, 包括小数点, 取值范围0～90)
        /// </summary>
        [SqlField]
        public string latitude { get; set; }
        /// <summary>
        /// 配送服务(1:蜂鸟配送, 2:蜂鸟优送, 3:蜂鸟快送)
        /// </summary>
        [SqlField]
        public int service_code { get; set; }
        /// <summary>
        /// 商户配置表ID
        /// </summary>
        [SqlField]
        public int distritionid { get; set; }
    }
}
