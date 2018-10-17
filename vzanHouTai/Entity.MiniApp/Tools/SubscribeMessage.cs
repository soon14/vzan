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
    /// 订阅消息
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class SubscribeMessage
    {
        [SqlField(IsPrimaryKey =true,IsAutoId =true)]
        public int Id { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        [SqlField]
        public DateTime SendTime { get; set; }
        /// <summary>
        /// 发送内容
        /// </summary>
        [SqlField]
        public string SendContent { get; set; }
        /// <summary>
        /// 内容模板
        /// </summary>
        [SqlField]
        public int Template { get; set; }
        /// <summary>
        /// 小程序模板类型
        /// </summary>
        [SqlField]
        public int PageType { get; set; }
        /// <summary>
        /// 消息类型（0:小程序、1:公众号）
        /// </summary>
        [SqlField]
        public int ContentType { get; set; }
        /// <summary>
        /// 发送状态
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 订阅来源ID（产品ID、订单ID等）
        /// </summary>
        [SqlField]
        public int SourceId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 发送对象 OpenId
        /// </summary>
        [SqlField]
        public string OpenId { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        [SqlField]
        public string ErrorMsg { get; set; }
        ///// <summary>
        ///// 附加数据
        ///// </summary>
        //public string Attach { get; set; }
    }
}
