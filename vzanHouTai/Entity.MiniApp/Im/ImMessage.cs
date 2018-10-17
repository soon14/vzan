using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Im
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ImMessage
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public string appId { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int aId { get; set; } = 0;

        /// <summary>
        /// 门店id
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;
        /// <summary>
        /// 房间id
        /// </summary>
        [SqlField]
        public int roomId { get; set; } = 0;
        /// <summary>
        /// 发送者id
        /// </summary>
        [SqlField]
        public int fuserId { get; set; } = 0;
        /// <summary>
        /// 接收者id
        /// </summary>
        [SqlField]
        public int tuserId { get; set; } = 0;
        /// <summary>
        /// 状态 0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 消息内容
        /// </summary>
        [SqlField]
        public string msg { get; set; } = string.Empty;
        /// <summary>
        /// 发送时间
        /// </summary>
        [SqlField]
        public string sendDate { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public string createDate { get; set; } = string.Empty;
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public string updateDate { get; set; } = string.Empty;

        /// <summary>
        /// 用户类型
        /// </summary>
        [SqlField]
        public int tuserType { get; set; } = 0;
        /// <summary>
        /// 阅读状态 0未读 1已读
        /// </summary>
        [SqlField]
        public int isRead { get; set; } = 0;
        /// <summary>
        /// uuid
        /// </summary>
        [SqlField]
        public string ids { get; set; } = string.Empty;
        /// <summary>
        /// 消息类型 0文字 1图片
        /// </summary>
        [SqlField]
        public int msgType { get; set; } = 0;

        /// <summary>
        /// 名片姓名
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 名片头像
        /// </summary>
        public string CardImgUrl { get; set; }
        public string TUserImgUrl { get; set; }
        public string FUserImgUrl { get; set; }
        /// <summary>
        /// 未读私信信息数量
        /// </summary>
        public int NoReadMessageCount { get; set; }
        /// <summary>
        /// 最后一条私信信息
        /// </summary>
        public string LastMsg { get; set; }
        /// <summary>
        /// 客服姓名
        /// </summary>
        public string EmployeeName { get; set; }
        /// <summary>
        /// 客户备注
        /// </summary>
        public string Desc { get; set; }
        public string StoreName { get; set; }
        public string StoreLogImgUrl { get; set; }
    }
}
