using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 用户签到记录表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ExchangePlayCardLog
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; }


        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }


        /// <summary>
        /// 签到时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 签到月份.日期
        /// </summary>
        public string AddTimeDayStr {
            get
            {
                //TODO 上线为天测试为分钟
                // return $"{AddTime.ToString("MM")}.{AddTime.ToString("dd")}";
                return $"{AddTime.ToString("hh")}.{AddTime.ToString("mm")}";

            }
        }
        /// <summary>
        /// 是否打卡 从数据库里获取的都为true
        /// </summary>
        public bool Played { get; set; } = true;

        /// <summary>
        /// 本次签到所得积分
        /// </summary>
        [SqlField]
        public int Points { get; set; }

        /// <summary>
        /// 本次签到所得积分详情备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; }

    }
}
