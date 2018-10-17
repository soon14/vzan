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
    /// 小程序统计数据
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStatisticalFlow
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 打开次数
        /// </summary>
        [SqlField]
        public int Sessioncnt { get; set; }
        /// <summary>
        /// 访问次数
        /// </summary>
        [SqlField]
        public int VisitPV { get; set; }
        /// <summary>
        /// 访问人数
        /// </summary>
        [SqlField]
        public int VisitUV { get; set; }
        /// <summary>
        /// 新用户数
        /// </summary>
        [SqlField]
        public int VisitUVNew { get; set; }
        /// <summary>
        /// 人均停留时长 (浮点型，单位：秒)
        /// </summary>
        [SqlField]
        public double StayTimeUV { get; set; }
        /// <summary>
        /// 次均停留时长 (浮点型，单位：秒)
        /// </summary>
        [SqlField]
        public double StayTimeSession { get; set; }
        /// <summary>
        /// 平均访问深度 (浮点型)
        /// </summary>
        [SqlField]
        public double VisitDepth { get; set; }
        /// <summary>
        /// 时间： 如： "20170313"
        /// </summary>
        [SqlField]
        public string RefDate { get; set; }
        /// <summary>
        /// 统计类型，0：小程序日访问趋势
        /// </summary>
        [SqlField]
        public int DataType { get; set; }
        [SqlField]
        public int AId { get; set; } = 0;
        [SqlField]
        public string AppId { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = "";
    }

    /// <summary>
    /// 数据雷达
    /// </summary>
    public class PlatRadarReportModel
    {
        /// <summary>
        /// 我的名片浏览量
        /// </summary>
        public int MyCardViewCount { get; set; }
        /// <summary>
        /// 我的名片点赞量
        /// </summary>
        public int MyCardDzCount { get; set; }
        /// <summary>
        /// 我的名片关注量
        /// </summary>
        public int MyCardFollowCount { get; set; }
        /// <summary>
        /// 我的名片访客量
        /// </summary>
        public int MyCardVisitorCount { get; set; }
        /// <summary>
        /// 私信
        /// </summary>
        public int MyCardSiXinCount { get; set; }
        /// <summary>
        /// 我的名片转发量
        /// </summary>
        public int MyCardShareCount { get; set; }

        /// <summary>
        /// 关注
        /// </summary>
        public int FollowCount { get; set; }
        /// <summary>
        /// 粉丝
        /// </summary>
        public int FanCount { get; set; }
        /// <summary>
        /// 互相关注
        /// </summary>
        public int MutualFollowCount { get; set; }

        /// <summary>
        /// 店铺浏览量
        /// </summary>
        public int StoreViewCount { get; set; }
        /// <summary>
        /// 店铺收藏量
        /// </summary>
        public int StoreFavoriteCount { get; set; }
        /// <summary>
        /// 店铺访客量
        /// </summary>
        public int StoreVisitorCount { get; set; }
    }
}
