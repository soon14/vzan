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
    public class PinGroupLog
    {
        /// <summary>
        /// id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; } = 0;

        /// <summary>
        /// 成团id
        /// </summary>
        [SqlField]
        public int groupId { get; set; } = 0;

        /// <summary>
        /// 状态 0：正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 当前团状态
        /// </summary>
        [SqlField]
        public int groupState { get; set; } = 0;

        /// <summary>
        /// 日志信息
        /// </summary>
        [SqlField]
        public string logMsg { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        public string addtimeStr
        {
            get
            {
                return addtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}