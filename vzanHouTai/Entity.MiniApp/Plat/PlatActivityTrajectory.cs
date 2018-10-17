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
    /// 活动轨迹
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatActivityTrajectory
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public long MyUserId { get; set; }
        [SqlField]
        public string MyImgUrl { get; set; }
        [SqlField]
        public string MyName { get; set; }

        [SqlField]
        public long OtherUserId { get; set; }
        [SqlField]
        public string OtherImgUrl { get; set; }
        [SqlField]
        public string OtherName { get; set; }
        /// <summary>
        /// 类型 0表示 收藏 1表示点赞,2关注，3看过，4私信（PointsActionType）
        /// </summary>
        [SqlField]
        public int ActionType { get; set; }
        public string ActionTypeStr { get { return Enum.GetName(typeof(PointsActionType), ActionType); } }

        /// <summary>
        /// 数据类型，0：帖子，1：商品，2：评论(PointsDataType)
        /// </summary>
        [SqlField]
        public int Datatype { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        [SqlField]
        public int AId { get; set; } = 0;
        [SqlField]
        public string AppId { get; set; }
        [SqlField]
        public int DataId { get; set; } = 0;

        [SqlField]
        public string DataImgUrl { get; set; }
        [SqlField]
        public string DataContent { get; set; }
        [SqlField]
        public string DataComment { get; set; }
        /// <summary>
        /// 帖子ID
        /// </summary>
        public int MsgId { get; set; }
        public int MyCardId { get; set; }
        public int OtherCardId { get; set; }
        
    }
}
