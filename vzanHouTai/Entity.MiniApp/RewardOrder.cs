using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.QLWL, UseMaster = true)]
    public class RewardOrder
    {
        #region Model
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 打赏人
        /// </summary>
        [SqlField]
        public int rewardFromUserId { get; set; }
        /// <summary>
        /// 被打赏人
        /// </summary>
        [SqlField]
        public int rewardToUserId { get; set; }
        /// <summary>
        /// 打赏金额
        /// </summary>
        [SqlField]
        public int rewardMoney { get; set; }
        /// <summary>
        /// 社区ID
        /// </summary>
        [SqlField]
        public int minisnsId { get; set; }
        /// <summary>
        /// 文章id
        /// </summary>
        [SqlField]
        public int articleId { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int status { get; set; }
        /// <summary>
        /// 支付单号
        /// </summary>
        [SqlField]
        public string out_trade_no { get; set; }
        /// <summary>
        /// 用户提成百分比（默认70，范围：70~90）
        /// </summary>
        [SqlField]
        public int percent { get; set; }

        /// <summary>
        /// 打赏类型（0：文章，1：回复）
        /// </summary>
        [SqlField]
        public int rewardtype { get; set; }
        /// <summary>
        /// 评论id
        /// </summary>
        [SqlField]
        public int commentId { get; set; }
        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;

        /// <summary>
        /// IP地址
        /// </summary>
        [SqlField]
        public string userip { get; set; }
        /// <summary>
        /// 支付到公共号的appid
        /// </summary>
        [SqlField]
        public string appid { get; set; }
        /// <summary>
        /// 支付到的商户平台
        /// </summary>
        [SqlField]
        public string mch_id { get; set; }
        #endregion Model
    }
}
