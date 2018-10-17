using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.MiniApp
{
    #region 微信请求类 RequestXML
    /// <summary>  
    /// 微信请求类  
    /// </summary>  
    [Serializable]
    public class RequestXML
    {
        private string _tousername =string.Empty;//消息接收方微信号，一般为公众平台账号微信号  
        private string _fromusername = string.Empty;// 消息发送方微信号 
        private string _createtime = string.Empty;//创建时间 
        private string _msgtype = string.Empty;// 信息类型 地理位置:location,文本消息:text,消息类型:image
        private string _content = string.Empty;//信息内容
        private string _picurl = string.Empty;// 图片链接，开发者可以用HTTP GET获取  
        private string _msgid = string.Empty;//消息Id
        private string _mediaid = string.Empty;//多媒体消息id
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _url = string.Empty;
        private string _format = string.Empty;
        private string _recognition = string.Empty;

        private string _event=string.Empty;
        private string _eventkey=string.Empty;
        private string _ticket=string.Empty;

        public string ToUserName
        {
            get { return _tousername; }
            set { _tousername = value; }
        }

        public string FromUserName
        {
            get { return _fromusername; }
            set { _fromusername = value; }
        }

        public string CreateTime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }

        public string MsgType
        {
            get { return _msgtype; }
            set { _msgtype = value; }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string PicUrl
        {
            get { return _picurl; }
            set { _picurl = value; }
        }

        public string MediaId
        {
            get { return _mediaid; }
            set { _mediaid = value; }
        }

        public string MsgId
        {
            get { return _msgid; }
            set { _msgid = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }

        public string Recognition
        {
            get { return _recognition; }
            set { _recognition = value; }
        }

        public string Event
        {
            get { return _event; }
            set { _event = value; }
        }

        public string EventKey
        {
            get { return _eventkey; }
            set { _eventkey = value; }
        }

        public string Ticket
        {
            get { return _ticket; }
            set { _ticket = value; }
        }
    }
    #endregion
}