using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Entity.Base;
using Utility;
using System.ComponentModel.DataAnnotations;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 地址
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishUserAddress
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int userid { get; set; } = 0;

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "收货人姓名不能为空")]
        public string consignee { get; set; } = string.Empty;

        /// <summary>
        /// 性别 0=未知， 1=男,2=女
        /// </summary>
        [SqlField]
        public int u_sex { get; set; } = 0;

        /// <summary>
        /// 手机
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "电话不能为空")]
        public string mobile { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        [SqlField]
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// 国家
        /// </summary>
        [SqlField]
        public string country { get; set; } = string.Empty;

        /// <summary>
        /// 省
        /// </summary>
        [SqlField]
        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 市
        /// </summary>
        [SqlField]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 区
        /// </summary>
        [SqlField]
        public string district { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "收货地址不能为空")]
        public string address { get; set; } = string.Empty;

        /// <summary>
        /// 补充说明
        /// </summary>
        [SqlField]
        public string buchong { get; set; } = string.Empty;

        /// <summary>
        /// 纬度
        /// </summary>
        [SqlField]
        public double u_lat { get; set; } = 0d;

        /// <summary>
        /// 经度
        /// </summary>
        [SqlField]
        public double u_lng { get; set; } = 0d;

        /// <summary>
        /// 是否默认地址 1=默认，0不默认
        /// </summary>
        [SqlField]
        public int is_default { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime update_time { get; set; }

        /// <summary>
        /// 状态 1=正常，0=禁用,-1=已删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
    }
}
