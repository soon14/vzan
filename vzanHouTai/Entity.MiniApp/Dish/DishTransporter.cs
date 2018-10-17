using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 标签
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishTransporter
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 配送员姓名
        /// </summary>
        [SqlField]
        public string dm_name { get; set; } = string.Empty;

        /// <summary>
        /// 手机号码
        /// </summary>
        [SqlField]
        public string dm_mobile { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 状态 1=正常，0=禁用，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
    }
}
