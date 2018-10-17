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
    /// 活动
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishActivity
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 活动类型 1=代金券，2=满减
        /// </summary>
        [SqlField]
        public int q_type { get; set; } = 1;

        /// <summary>
        /// 状态 1=开启，0=关闭，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 开始时间
        /// </summary>
        [SqlField]
        public DateTime q_begin_time { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime q_end_time { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        [SqlField]
        public string q_name { get; set; } = string.Empty;

        /// <summary>
        /// 最低消费金额
        /// </summary>
        [SqlField]
        public double q_xiaofei_jiner { get; set; } = 0.00;

        /// <summary>
        /// 抵用金额
        /// </summary>
        [SqlField]
        public double q_diyong_jiner { get; set; } = 0.00;

        /// <summary>
        /// 发放数量 0=不限制
        /// </summary>
        [SqlField]
        public int q_limit_num { get; set; } = 0;

        /// <summary>
        /// 促销描述
        /// </summary>
        [SqlField]
        public string q_shuoming { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int q_order { get; set; } = 99;

        [SqlField]
        public DateTime add_time { get; set; } = DateTime.Now;

        [SqlField]
        public DateTime update_time { get; set; }
    }
}
