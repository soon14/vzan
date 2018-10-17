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
    /// 门店收益表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishStoreEarnings
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int aId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int storeId { get; set; }

        /// <summary>
        /// 总收益
        /// </summary>
        [SqlField]
        public double money { get; set; } = 0.00;
        
        /// <summary>
        /// 最后操作日期
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }
    }
}
