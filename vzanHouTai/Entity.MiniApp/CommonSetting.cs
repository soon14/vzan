using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 公共配置类,用于承接配置设定的存放.不参与数据库操作
    /// </summary>
    public class CommonSetting
    {
        public CommonSetting() { }

        /// <summary>
        /// 满减开关
        /// </summary>
        public bool discountRuleSwitch { get; set; } = false;
        /// <summary>
        /// 新用户首单立减金额
        /// </summary>
        public float newUserFirstOrderDiscountMoney { get; set; } = 0;
        /// <summary>
        /// 用户首单立减金额
        /// </summary>
        public float userFirstOrderDiscountMoney { get; set; } = 0;

        /// <summary>
        /// 排队开关
        /// </summary>
        public bool sortQueueSwitch { get; set; } = false;

        /// <summary>
        /// 下一个队列号码
        /// </summary>
        public int sortNo_next { get; set; } = 1;
    }
}
