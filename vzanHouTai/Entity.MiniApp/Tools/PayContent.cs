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
    /// 付费内容
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PayContent
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        [SqlField]
        public int Amount { get; set; }
        /// <summary>
        /// 内容类型（枚举：PaidContentType）
        /// </summary>
        [SqlField]
        public int ContentType { get; set; }
        /// <summary>
        /// 免费/折扣选项
        /// </summary>
        [SqlField]
        public string Exclusive { get; set; }
        /// <summary>
        /// 文章ID
        /// </summary>
        [SqlField]
        public int ArticleId { get; set; }
        /// <summary>
        /// 视频文件URL
        /// </summary>
        [SqlField]
        public string VideoURL { get; set; } = string.Empty;
        /// <summary>
        /// 视频封面
        /// </summary>
        [SqlField]
        public string VideoCover { get; set; } = string.Empty;
        /// <summary>
        /// 音频文件URL
        /// </summary>
        [SqlField]
        public string AudioURL { get; set; } = string.Empty;
        ///// <summary>
        ///// 内容附件
        ///// </summary>
        //[SqlField]
        //public string AttachContent { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }
    }

    /// <summary>
    /// 付费内容支付信息
    /// </summary>
    public class PayContentPayment
    {
        /// <summary>
        /// 实付
        /// </summary>
        public int PayAmount { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public int OrgAmount { get; set; }
        /// <summary>
        /// 优惠额度
        /// </summary>
        public int DiscountAmount { get; set; }
        /// <summary>
        /// 优惠信息
        /// </summary>
        public string Info { get; set; }
    }
}
