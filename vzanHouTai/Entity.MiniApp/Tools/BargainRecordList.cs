using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class BargainRecordList
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 砍价商品id 商品id
        /// </summary>
        [SqlField]
        public int BId { get; set; }

        /// <summary>
        /// 领取人砍价商品人的id也就是被帮助者
        /// </summary>
        [SqlField]
        public int BUId { get; set; }
        /// <summary>
        /// 帮砍金额
        /// </summary>
        [SqlField]
        public int Amount { get; set; }

        /// <summary>
        /// 帮砍金额 单位 元
        /// </summary>
        public string AmountStr { get { return (Amount * 0.01).ToString("0.00"); } }
        /// <summary>
        /// <summary>
        /// 砍价人Id
        /// </summary>
        [SqlField]
        public int BargainUserId { get; set; }
        /// <summary>
        /// 砍价人昵称
        /// </summary>
        [SqlField]
        public string BargainUserName { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 帮砍者IP地址
        /// </summary>
        [SqlField]
        public string IpAddress { get; set; }

        /// <summary>
        /// 用户微信头像
        /// </summary>
        public string UserLogo { get; set; } = string.Empty;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
