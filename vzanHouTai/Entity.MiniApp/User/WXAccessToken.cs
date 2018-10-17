using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    [Serializable]
    public class WXAccessToken
    {
        public string access_token { set; get; }
        public int expires_in { set; get; }

        public int errcode { get; set; } = 0;
        public string errmsg { get; set; }
    }
    [Serializable]
    public class Jsapi_ticket
    {
        public int errcode { get; set; } = 0;
        public string errmsg { get; set; }
        public string ticket { get; set; }
        public string expires_in { get; set; }
    }
    public class WeChatParam
    {

        #region 常用
        /// <summary>
        /// 微信OpenId
        /// </summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WeChatPublic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Debug { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
        public string Code { get; set; }
        #endregion

        #region 开放平台
        /// <summary>
        /// 微信后台推送的ticket，此ticket会定时推送
        /// </summary>
        public string ComponentVerifyTicket { get; set; }
        /// <summary>
        /// 授权方的刷新令牌
        /// </summary>
        public string AuthorizerRefreshToken { get; set; }
        /// <summary>
        /// 授权方appid
        /// </summary>
        public string AuthorizerAppid { get; set; }
        /// <summary>
        /// 第三方平台appid
        /// </summary>
        public string ComponentAppid { get; set; }
        /// <summary>
        /// 第三方平台appsecret
        /// </summary>
        public string ComponentAppsecret { get; set; }
        /// <summary>
        /// 授权code,会在授权成功时返回给第三方平台
        /// </summary>
        public string AuthorizationCode { get; set; }
        /// <summary>
        /// 第三方平台access_token。
        /// </summary>
        public string ComponentAccessToken { get; set; }
        #endregion

        #region 消息
        /// <summary>
        /// 
        /// </summary>
        public Guid? MsgId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SendCount { get; set; }
        /// <summary>
        /// 文本消息
        /// </summary>
        public const string TextMsg = "text";
        /// <summary>
        /// 图片消息
        /// </summary>
        public const string ImageMsg = "image";
        /// <summary>
        /// 语音消息
        /// </summary>
        public const string VoiceMsg = "voice";
        /// <summary>
        /// 视频消息
        /// </summary>
        public const string VideoMsg = "video";
        /// <summary>
        /// 音乐消息
        /// </summary>
        public const string MusicMsg = "music";
        /// <summary>
        /// 图文消息
        /// </summary>
        public const string NewsMsg = "news";
        /// <summary>
        /// 接收消息的用户的openid
        /// </summary>

        public string ToUser { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public string TemplateId { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public Dictionary<string, string> Data { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>

        public string MsgType { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>

        public object Text { get; set; }
        #endregion

        #region 自定义菜单
        /// <summary>
        /// 菜单json数据
        /// </summary>

        #endregion

        #region MediaParam
        /// <summary>
        ///     媒体文件上传后，获取时的唯一标识
        /// </summary>

        public string MediaId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FilePath { get; set; }
        #endregion
    }

    /// <summary>
    /// 服务端向客户端返回信息类
    /// 2008-01-15 新增
    /// </summary>
    [Serializable]
    public class RetMessage
    {
        private bool _Success = false;
        private object _Id = 0;
        private string _Info = "";
        private string _Redirect = "";
        private object _Data = new object();

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool Success
        {
            get { return _Success; }
            set { _Success = value; }
        }
        /// <summary>
        /// 信息类型
        /// </summary>
        public object Id
        {
            get { return _Id; }
            set { _Id = value; }
        }
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Info
        {
            get { return _Info; }
            set { _Info = value; }
        }
        public string Redirect
        {
            get { return _Redirect; }
            set { _Redirect = value; }
        }
        /// <summary>
        /// 数据集
        /// </summary>
        public object Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
    }
}
