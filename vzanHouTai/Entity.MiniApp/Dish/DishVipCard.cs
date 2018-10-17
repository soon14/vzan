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
    /// 会员卡信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishVipCard
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int shop_id { get; set; } = 0;

        /// <summary>
        /// 会员id
        /// </summary>
        [SqlField]
        public int uid { get; set; } = 0;

        /// <summary>
        /// 会员姓名
        /// </summary>
        [SqlField]
        public string u_name { get; set; } = string.Empty;

        /// <summary>
        /// 卡号
        /// </summary>
        [SqlField]
        public int number { get; set; } = 100000;

        /// <summary>
        /// 会员卡余额
        /// </summary>
        [SqlField]
        public double account_balance { get; set; } = 0.00d;

        [SqlField]
        public string status { get; set; } = "0";

        /// <summary>
        /// 领取时间
        /// </summary>
        [SqlField]
        public DateTime add_time { get; set; } = DateTime.Now;

        /// <summary>
        /// 会员昵称
        /// </summary>
        public string nickname { get; set; } = string.Empty;

        /// <summary>
        /// 会员号码
        /// </summary>
        public string u_phone { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        public string headImg { get; set; } = string.Empty;

        public int is_new { get; set; } = 0;
    }
}
