using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 菜品属性类型
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishAttr
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 属性类型ID
        /// </summary>
        [SqlField]
        public int cat_id { get; set; } = 0;

        /// <summary>
        /// 类型名称
        /// </summary>
        public string cat_name { get; set; } = string.Empty;

        /// <summary>
        /// 属性名称
        /// </summary>
        [SqlField]
        public string attr_name { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SqlField]
        public string attr_values { get; set; } = string.Empty;


        public List<string> attr_values_arr
        {
            get {
                if (string.IsNullOrEmpty(attr_values))
                    return new List<string>();
                else
                    return attr_values.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(p=>p).ToList();
            }
        }
        /// <summary>
        /// 状态 1=正常，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;

        /// <summary>
        /// 用来显示
        /// </summary>
        public string attr_values_fmt
        {
            get
            {
                return this.attr_values?.Replace("\n", ",");
            }
        }
    }
}
