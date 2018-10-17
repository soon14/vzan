using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    /// <summary>
    /// 用户足迹跟踪
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class UserTrack
    {
        [SqlField(IsAutoId =true, IsPrimaryKey = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 跟踪来源ID（入口二维码ID、商品ID）
        /// </summary>
        [SqlField]
        public int TrackId { get; set; }
        /// <summary>
        /// 跟踪类型（二维码、商品），枚举：UserTrackType
        /// </summary>
        [SqlField]
        public int Type { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        [SqlField]
        public string Attrbute { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 状态，枚举：UserTrackState
        /// </summary>
        [SqlField]
        public int State { get; set; }
    }
}
