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
    /// 储值消费记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishCardAccountLog
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int shop_id { get; set; } = 0;

        /// <summary>
        /// 消费内容
        /// </summary>
        [SqlField]
        public string account_info { get; set; } = string.Empty;

        /// <summary>
        /// 消费金额
        /// </summary>
        [SqlField]
        public double account_money { get; set; } = 0.00;

        /// <summary>
        /// 消费类型  1：充值/退款   2：消费
        /// </summary>
        [SqlField]
        public int account_type { get; set; } = 1;

        /// <summary>
        /// 消费时间
        /// </summary>
        [SqlField]
        public DateTime add_time { get; set; } = DateTime.Now;

        [SqlField]
        public int user_id { get; set; } = 0;

        /// <summary>
        /// 状态 -1：删除 0：待确认   1：正常 
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
    }
}
