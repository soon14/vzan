using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 餐饮多门店配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishCategory
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int aId { get; set; } = 0;

        /// <summary>
        /// 门店id  app下的此列为0,门店下此列为dish表id
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 分类类型 枚举:DishEnum.CategoryEnums
        /// </summary>
        [SqlField]
        public int type { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        [SqlField]
        public string title { get; set; } = string.Empty;

        /// <summary>
        /// 图片地址
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;

        /// <summary>
        /// 是否显示 0不显示,1显示
        /// </summary>
        [SqlField]
        public int is_show { get; set; } = 1;

        /// <summary>
        /// 排序,数字越大越靠前
        /// </summary>
        [SqlField]
        public int is_order { get; set; } = 99;


        /// <summary>
        /// 状态 1正常 -1删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

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

        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string name_info { get; set; } = string.Empty;
    }
}
