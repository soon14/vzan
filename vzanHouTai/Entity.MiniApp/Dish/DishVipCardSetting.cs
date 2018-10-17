using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Dish
{

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishVipCardSetting
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int uid { get; set; } = 0;

        [SqlField]
        public int aid { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 是否开启会员卡
        /// </summary>
        [SqlField]
        public int card_open_status { get; set; } = 0;

        /// <summary>
        /// 会员卡说明
        /// </summary>
        [SqlField]
        public string card_info { get; set; } = string.Empty;

        /// <summary>
        /// 充值规则
        /// </summary>
        [SqlField]
        public string rechargeConfigJson { get; set; } = string.Empty;

        public List<RechargeConfig> rechargeConfig { get; set; } = new List<RechargeConfig>();
    }

    public class RechargeConfig
    {
        public double rc_man { get; set; } = 0.00d;

        public double rc_song { get; set; } = 0.00d;
        public bool is_select { get; set; } = false;
    }
}
