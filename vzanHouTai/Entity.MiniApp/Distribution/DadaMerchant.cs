using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 注册商户body对象
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DadaMerchant
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 商户城市名称(如,上海)
        /// </summary>
        [SqlField]
        public string city_name { get; set; }
        /// <summary>
        /// 联系人姓名
        /// </summary>
        [SqlField]
        public string contact_name { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [SqlField]
        public string contact_phone { get; set; }
        /// <summary>
        /// 邮箱地址
        /// </summary>
        [SqlField]
        public string email { get; set; }
        /// <summary>
        /// 企业地址
        /// </summary>
        public string enterprise_address { get; set; }
        /// <summary>
        /// 企业全称
        /// </summary>
        [SqlField]
        public string enterprise_name { get; set; }
        /// <summary>
        /// 注册商户手机号,用于登陆商户后台
        /// </summary>
        [SqlField]
        public string mobile { get; set; }
        /// <summary>
        /// 达达商户号
        /// </summary>
        [SqlField]
        public string sourceid { get; set; }
        /// <summary>
        /// 门店编号
        /// </summary>
        public string origin_shop_id { get; set; }

        public List<DadaCity> CityList { get; set; }
    }
}
