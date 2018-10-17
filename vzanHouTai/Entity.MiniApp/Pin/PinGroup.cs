using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Pin
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinGroup
    {
        /// <summary>
        /// id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; } = 0;

        [SqlField]
        public int aid { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        [SqlField]
        public int goodsId { get; set; } = 0;

        /// <summary>
        /// 成团人数：满足拼团的人数
        /// </summary>
        [SqlField]
        public int groupCount { get; set; } = 2;

        /// <summary>
        /// 参团人数：参加拼团的人数
        /// </summary>
        [SqlField]
        public int entrantCount { get; set; } = 0;

        /// <summary>
        /// 完成拼团人数：交易完成的人数
        /// </summary>
        [SqlField]
        public int successCount { get; set; } = 0;

        /// <summary>
        /// 状态 -1：拼团失败   0：未成团    1：已成团  2：拼团成功 3:已返利 见PinEnums-GroupState
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        public string stateStr
        {
            get
            {
                return Enum.GetName(typeof(PinEnums.GroupState), state);
            }
        }
        /// <summary>
        /// 拼团开始时间
        /// </summary>
        [SqlField]
        public DateTime startTime { get; set; } = DateTime.Now;

        public string startTimeStr
        {
            get
            {
                return startTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 拼团结束时间
        /// </summary>
        [SqlField]
        public DateTime endTime { get; set; } = DateTime.Now;

        public string endTimeStr
        {
            get
            {
                return endTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 变化时间
        /// </summary>
        [SqlField]
        public DateTime changeTime { get; set; } = DateTime.Now;

        public string changeTimeStr
        {
            get
            {
                return changeTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}