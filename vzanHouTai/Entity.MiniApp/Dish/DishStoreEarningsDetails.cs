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
    /// 门店收益情况变动表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishStoreEarningsDetails
    {
        [SqlField(IsPrimaryKey = true,IsAutoId =true)]
        public int id { get; set; }
        
        /// <summary>
        /// 金额记录表Id
        /// </summary>
        [SqlField]
        public int seId { get; set; }

        /// <summary>
        /// 记录类型
        /// </summary>
        [SqlField]
        public int type { get; set; }

        /// <summary>
        /// 变动金额
        /// </summary>
        [SqlField]
        public double changeMoney { get; set; } = 0.00;

        /// <summary>
        /// 此时剩余金额
        /// </summary>
        [SqlField]
        public double surplusMoney { get; set; } = 0.00;

        /// <summary>
        /// 说明备注
        /// </summary>
        [SqlField]
        public string remark { get; set; }

        /// <summary>
        /// 操作日期
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }
    }
}
