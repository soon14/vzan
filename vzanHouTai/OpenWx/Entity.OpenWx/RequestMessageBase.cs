using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entity.OpenWx.MiniAppEnum;

namespace Entity.OpenWx
{
    /// <summary>
    /// 所有Request和Response消息的基类
    /// </summary>
    public abstract class MessageBase
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public DateTime CreateTime { get; set; }
    }
    
    /// <summary>
    /// 请求消息
    /// </summary>
    public class RequestMessageBase : MessageBase
    {
        public string AppId { get; set; }
        public virtual RequestInfoType InfoType
        {
            get { return RequestInfoType.component_verify_ticket; }
        }
    }

    public class RequestMessageComponentVerifyTicket : RequestMessageBase
    {
        public override RequestInfoType InfoType
        {
            get { return RequestInfoType.component_verify_ticket; }
        }
        public string ComponentVerifyTicket { get; set; }
    }

    public class RequestMessageText : RequestMessageBase
    {
        public RequestMsgType MsgType
        {
            get { return RequestMsgType.TEXT; }
        }

        /// <summary>
        /// 文本消息内容
        /// </summary>
        public string Content { get; set; }
    }
}
