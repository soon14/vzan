using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Im
{
    /// <summary>
    /// 消息列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ImContact
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
        /// 用户id
        /// </summary>
        [SqlField]
        public int fuserId { get; set; } = 0;
        /// <summary>
        /// 对话者id
        /// </summary>
        [SqlField]
        public int tuserId { get; set; } = 0;
        /// <summary>
        /// 状态 0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 用户类型 ：0：普通用户（c_user） 1:技师
        /// </summary>
        [SqlField]
        public int fuserType { get; set; } = 0;
        /// <summary>
        /// 附加数据
        /// </summary>
        [SqlField]
        public string extra { get; set; } = string.Empty;
        /// <summary>
        /// 对话者头像
        /// </summary>
        public string tuserHeadImg { get; set; } = string.Empty;
        /// <summary>
        /// 对话者昵称
        /// </summary>
        public string tuserNicename { get; set; } = string.Empty;
        /// <summary>
        /// 最近一条聊天消息
        /// </summary>
        public ImMessage message { get; set; }
        
        public DateTime newDate { get; set; } = DateTime.Now;

        public int unReadCount { get; set; } = 0;
    }
    public class ImContactListExtra
    {
        /// <summary>
        /// 发送者头像
        /// </summary>
        public string fHeadImg { get; set; } = string.Empty;
       /// <summary>
       /// 接收者头像
       /// </summary>
        public string tHeadImg { get; set; } = string.Empty;
        /// <summary>
        /// 发送者昵称
        /// </summary>
        public string fNickName { get; set; } = string.Empty;
        /// <summary>
        /// 接收者昵称
        /// </summary>
        public string tNickName { get; set; } = string.Empty;
    }
}
