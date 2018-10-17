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
    public class DishSetting
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 入驻开关 0：关闭 1：开启
        /// </summary>
        [SqlField]
        public int dish_rz_open { get; set; } = 0;

        /// <summary>
        /// 入驻费用
        /// </summary>
        [SqlField]
        public double dish_rz_jiner { get; set; } = 0.00;

        /// <summary>
        /// 入驻说明
        /// </summary>
        [SqlField]
        public string dish_rz_shuoming { get; set; } = string.Empty;

        /// <summary>
        /// 提现自动审核  0：关闭 1：开启
        /// </summary>
        [SqlField]
        public int dish_auto_cash { get; set; } = 0;

        /// <summary>
        /// 支付自动到账  0：关闭 1：开启
        /// </summary>
        [SqlField]
        public int dish_auto_one_pay { get; set; } = 0;

        /// <summary>
        /// 最小提现金额（最低提现金额不能小于1元，建议填写整数，不填写为不限制）
        /// </summary>
        [SqlField]
        public double cash_limit_jiner { get; set; } = 0;

        /// <summary>
        /// 提现费率
        /// </summary>
        [SqlField]
        public double cash_fee { get; set; } = 0;

        /// <summary>
        /// 销量显示方式 0:最近30天    1:每月销量
        /// </summary>
        [SqlField]
        public int dish_xiaoliang_show_type { get; set; } = 0;

        /// <summary>
        /// 自有配送  0：关闭 1：开启
        /// </summary>
        [SqlField]
        public int dish_own_peisong_status { get; set; } = 0;

        /// <summary>
        /// 配送名称:
        /// </summary>
        [SqlField]
        public string dish_own_peisong_name { get; set; } = string.Empty;

        /// <summary>
        /// AppKey
        /// </summary>
        [SqlField]
        public string dish_own_peisong_key { get; set; } = string.Empty;

        /// <summary>
        /// Sign
        /// </summary>
        [SqlField]
        public string dish_own_peisong_sign { get; set; } = string.Empty;


        /// <summary>
        /// 是否开启店铺分享 0=关闭，1=开启
        /// </summary>
        [SqlField]
        public int dish_share { get; set; } = 0;

        /// <summary>
        /// 是否开启关注公众号
        /// </summary>
        [SqlField]
        public int dish_focus_wx { get; set; } = 0;
    }
}
