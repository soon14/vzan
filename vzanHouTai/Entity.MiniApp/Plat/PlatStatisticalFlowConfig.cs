using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    /// 小程序统计数据配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStatisticalFlowConfig
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        [SqlField]
        public int State { get; set; } = 0;
        [SqlField]
        public string AppId { get; set; }
        [SqlField]
        public int AId { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 同步开始时间
        /// </summary>
        [SqlField]
        public DateTime StartTime { get; set; }
        public string StartTimeStr { get { return StartTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 同步结束时间
        /// </summary>
        [SqlField]
        public DateTime EndTime { get; set; }
        public string EndTimeStr { get { return EndTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
    }
}
