using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 用户领取的优惠券
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishActivityUser
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }


        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int dish_id { get; set; } = 0;

        /// <summary>
        /// 领取时间
        /// </summary>
        [SqlField]
        public DateTime quan_add_time { get; set; } = DateTime.Now;

        /// <summary>
        /// 有效期开始时间
        /// </summary>
        [SqlField]
        public DateTime quan_begin_time { get; set; }

        /// <summary>
        /// 有效期结束时间
        /// </summary>
        [SqlField]
        public DateTime quan_end_time { get; set; }

        public string quan_end_time_fmt
        {
            get
            {
                return quan_end_time.ToString("yyyy-MM-dd");
            }
        }
        /// <summary>
        /// 优惠券ID
        /// </summary>
        [SqlField]
        public int quan_id { get; set; } = 0;

        /// <summary>
        /// 抵用金额
        /// </summary>
        [SqlField]
        public double quan_jiner { get; set; } = 0.00;

        /// <summary>
        /// 起用金额
        /// </summary>
        [SqlField]
        public double quan_limit_jiner { get; set; } = 0.00;

        /// <summary>
        /// 优惠券名称
        /// </summary>
        [SqlField]
        public string quan_name { get; set; } = string.Empty;

        /// <summary>
        /// 状态0=未使用，1=已使用
        /// </summary>
        [SqlField]
        public int quan_status { get; set; } = 0;

        /// <summary>
        /// 类型 1=店内优惠券，2=平台优惠券
        /// </summary>
        [SqlField]
        public int quan_type { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int user_id { get; set; } = 0;
    }
}
