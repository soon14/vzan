using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 菜品属性类型
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishYuDing
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        [SqlField]
        public string yuding_date { get; set; } = string.Empty;

        /// <summary>
        /// 时间
        /// </summary>
        [SqlField]
        public string yuding_time { get; set; } = string.Empty;

        /// <summary>
        /// 人数
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "预定人数输入错误")]
        public int yuding_renshu { get; set; } = 0;

        /// <summary>
        /// 联系人
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings =false, ErrorMessage = "请输入您的姓名")]
        public string yuding_name { get; set; } = string.Empty;

        /// <summary>
        /// 手机
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings =false,ErrorMessage = "请输入您的手机号码")]
        [RegularExpression(@"^1\d{10}$",ErrorMessage = "手机格式错误")]
        public string yuding_phone { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string yuding_info { get; set; } = string.Empty;

        /// <summary>
        /// 门店ID
        /// </summary>
        [SqlField]
        public int dish_id { get; set; } = 0;

        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 状态0=待处理，1=已处理
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
    }
}
