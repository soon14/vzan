using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 菜品属性类型
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishAttrType
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 属性类型名称
        /// </summary>
        [SqlField]
        public string cat_name { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SqlField]
        public int enabled { get; set; } = 1;

        /// <summary>
        /// 状态 1=正常，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 属性个数
        /// </summary>
        public long attrCount { get; set; } = 0;
    }
}
