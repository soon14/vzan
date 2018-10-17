using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 代理收入记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinAgentIncomeLog
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;
        /// <summary>
        /// 获得收益代理id
        /// </summary>
        [SqlField]
        public int agentId { get; set; } = 0;
        [SqlField]
        public int userId { get; set; } = 0;
        /// <summary>
        /// 收入来源：0代理分成，1商家分成
        /// </summary>
        [SqlField]
        public int source { get; set; } = 0;
        /// <summary>
        /// 来源用户id
        /// </summary>
        [SqlField]
        public int sourceUid { get; set; } = 0;
        /// <summary>
        /// 收入 1=0.001分
        /// </summary>
        [SqlField]
        public int income { get; set; } = 0;
        public string incomeStr
        {
            get
            {
                return (income * 0.001 * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 收入前金额 1=0.001分
        /// </summary>
        [SqlField]
        public long beforeMoney { get; set; } = 0;
        public string beforeMoneyStr
        {
            get
            {
                return (beforeMoney * 0.001 * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 收入后金额 1=0.001分
        /// </summary>
        [SqlField]
        public long afterMoney { get; set; } = 0;
        public string afterMoneyStr
        {
            get
            {
                return (afterMoney * 0.001 * 0.01).ToString("0.00");
            }
        }
        [SqlField]
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;
        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public C_UserInfo userInfo { get; set; }
        public PinStore storeInfo { get; set; }

        /// <summary>
        /// 提成来源 0表示下级 1表示下下级
        /// 默认为0 原来的就只有下级
        /// </summary>
        [SqlField]
        public int ExtractType { get; set; } = 0;
    }
}
