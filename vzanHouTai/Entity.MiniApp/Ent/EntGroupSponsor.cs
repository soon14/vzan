using Entity.Base;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 拼团表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntGroupSponsor
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int RId { get; set; }
        /// <summary>
        /// 拼团产品关联表ID
        /// </summary>
        [SqlField]
        public int EntGoodRId { get; set; }
        /// <summary>
        /// 发起拼团的用户ID
        /// </summary>
        [SqlField]
        public int SponsorUserId { get; set; }
        /// <summary>
        /// 成团人数
        /// </summary>
        [SqlField]
        public int GroupSize { get; set; }
        /// <summary>
        /// 开团时间
        /// </summary>
        [SqlField]
        public DateTime StartDate { get; set; } = DateTime.Now;
        public string ShowStartTime { get; set; } = string.Empty;
        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string ShowEndTime { get; set; } = string.Empty;
        /// <summary>
        /// 4待付款，1开团成功，2团购成功，-1成团失败,-4已过期(GroupState)
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        [SqlField]
        public int Type { get; set; } = (int)TmpType.小程序专业模板;
        /// <summary>
        /// 团长头像
        /// </summary>
        public string UserImg { get; set; } = string.Empty;
        /// <summary>
        /// 该团已参加人数
        /// </summary>
        public int GroupNum { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string UserLogo { get; set; } = string.Empty;
        /// <summary>
        /// 参加过该拼团用户信息
        /// </summary>
        public List<object> GroupUserList { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public string OriginalPrice { get; set; } = string.Empty;
        /// <summary>
        /// 团购价
        /// </summary>
        public string GroupPrice { get; set; } = string.Empty;
        /// <summary>
        /// 团名称
        /// </summary>
        public string GroupName { get; set; } = string.Empty;
        /// <summary>
        /// 团产品图片
        /// </summary>
        public string GroupImage { get; set; } = string.Empty;
        /// <summary>
        /// 产品ID
        /// </summary>
        public int GoodId { get; set; } = 0;
    }
}
