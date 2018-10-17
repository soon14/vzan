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
    public class DishTag
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

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
        /// 类型 打印标签=1
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// 状态 1=正常，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
    }
}
