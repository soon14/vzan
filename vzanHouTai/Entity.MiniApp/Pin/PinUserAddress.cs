using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Entity.Base;
using Utility;
using System.ComponentModel.DataAnnotations;

namespace Entity.MiniApp.Pin
{

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinUserAddress
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int userId { get; set; } = 0;

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "收货人姓名不能为空")]
        [MaxLength(10,ErrorMessage ="收货人姓名不能超过10个字")]
        public string consignee { get; set; } = string.Empty;

        /// <summary>
        /// 手机
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "手机号码不能为空")]
        public string mobile { get; set; } = string.Empty;

        /// <summary>
        /// 省
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "请选择省份")]
        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 市
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "请选择城市")]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 区
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "请选择地区")]
        public string district { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "收货地址不能为空")]
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// 状态 1=正常，0=禁用,-1=已删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 是否默认地址 1=默认，0不默认
        /// </summary>
        [SqlField]
        public int isDefault { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }
    }
}
