using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 付费内容支付记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PaidContentRecord
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int Aid { get; set; }
        [SqlField]
        public DateTime Addtime { get; set; }
        /// <summary>
        /// 记录标题
        /// </summary>
        [SqlField]
        public string Title { get; set; }
        /// <summary>
        /// 记录属性
        /// </summary>
        [SqlField]
        public string Attr { get; set; }
        /// <summary>
        /// 内容类型（枚举：PaidContentType）
        /// </summary>
        [SqlField]
        public int ContentType { get; set; }
        /// <summary>
        /// 付费内容ID
        /// </summary>
        [SqlField]
        public int ContentId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 支付ID（订单ID）
        /// </summary>
        [SqlField]
        public int PayId { get; set; }
        /// <summary>
        /// 支付状态：0=未支付，1=已支付
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 购买内容快照
        /// </summary>
        [SqlField]
        public string Snapshot { get; set; }
    }

    /// <summary>
    /// 购买内容快照
    /// </summary>
    public class PaidSnapShot
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public int OriginalAmout { get; set; }
        /// <summary>
        /// 支付
        /// </summary>
        public int PaidAmout { get; set; }
        /// <summary>
        /// 优惠信息
        /// </summary>
        public string DiscountInfo { get; set; }
        /// <summary>
        /// 内容类型
        /// </summary>
        public int ContentType { get; set; }
        /// <summary>
        /// 文章ID
        /// </summary>
        public int ArticleId { get; set; }
        /// <summary>
        /// 视频ID
        /// </summary>
        public string VideoURL { get; set; }
        /// <summary>
        /// 视频封面
        /// </summary>
        public string VideoCover { get; set; }
    }
}
