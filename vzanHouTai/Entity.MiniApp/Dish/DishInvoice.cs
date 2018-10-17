using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 发票
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishInvoice
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int userid { get; set; } = 0;

        /// <summary>
        /// 发票抬头
        /// </summary>
        [SqlField]
        public string fapiao_title { get; set; } = string.Empty;

        /// <summary>
        /// 税号
        /// </summary>
        [SqlField]
        public string fapiao_daima { get; set; } = string.Empty;

        /// <summary>
        /// 发票类型 1=单位，2=个人
        /// </summary>
        [SqlField]
        public int fapiao_leixing { get; set; } = 0;

        /// <summary>
        /// 状态 1=正常，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

    }
}
