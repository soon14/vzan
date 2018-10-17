using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 签到送积分设置
    /// </summary>
    public class ExchangePlayCardConfig
    {
        /// <summary>
        /// 0表示关闭 1表示开启
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 每天签到送多少积分
        /// </summary>
        public int DayGivePoints { get; set; }

        /// <summary>
        /// 连续签到天数
        /// </summary>
        public int ConnectDay { get; set; }

        /// <summary>
        /// 满足连续签到ConnectDay天送多少积分
        /// </summary>
        public int ConnectDayGivePoints { get; set; }

        /// <summary>
        /// 首页是否弹出 0表示不弹出 1表示弹出
        /// </summary>
        public int ShowPage { get; set; }


    }
}
