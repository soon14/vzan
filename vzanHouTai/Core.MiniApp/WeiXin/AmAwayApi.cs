using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.MiniApp
{
    /// <summary>
    /// 被动回复接口
    /// </summary>
   public class AmAwayApi
    {
        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <param name="content"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
       public static string GetTextXml(string openId, string serverId, string content)
       {
           string xml = string.Empty;
           if (!string.IsNullOrEmpty(content))
           {
               xml = string.Format(@"<xml>
                           <ToUserName><![CDATA[{0}]]></ToUserName>
                           <FromUserName><![CDATA[{1}]]></FromUserName>
                           <CreateTime>{2}</CreateTime>
                           <MsgType><![CDATA[text]]></MsgType>
                           <Content><![CDATA[{3}]]></Content>
                           </xml>", openId, serverId, WxUtils.GetWeixinDateTime(DateTime.Now), content);
           }
           return xml;
       }

        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <param name="mediaId"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
       public static string GetImageXml(string openId, string serverId, string mediaId)
        {
            string xml = string.Empty;
            if (!string.IsNullOrEmpty(mediaId))
            {
                xml = string.Format(@"<xml>
                                    <ToUserName><![CDATA[{0}]]></ToUserName>
                                    <FromUserName><![CDATA[{1}]]></FromUserName>
                                    <CreateTime>{2}</CreateTime>
                                    <MsgType><![CDATA[image]]></MsgType>
                                    <Image>
                                    <MediaId><![CDATA[{3}]]></MediaId>
                                    </Image>
                                    </xml>", openId, serverId, WxUtils.GetWeixinDateTime(DateTime.Now), mediaId);
            }
            return xml;
        }

        /// <summary>
        /// 发送语音消息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <param name="mediaId"></param>
        /// <returns></returns>
       public static string GetVoiceXml(string openId, string serverId, string mediaId)
       {
           string xml = string.Empty;
           if (!string.IsNullOrEmpty(mediaId))
           {
               xml = string.Format(@"<xml>
                                    <ToUserName><![CDATA[{0}]]></ToUserName>
                                    <FromUserName><![CDATA[{1}]]></FromUserName>
                                    <CreateTime>{2}</CreateTime>
                                    <MsgType><![CDATA[voice]]></MsgType>
                                    <Voice>
                                    <MediaId><![CDATA[{3}]]></MediaId>
                                    </Voice>
                                    </xml>", openId, serverId, WxUtils.GetWeixinDateTime(DateTime.Now), mediaId);
           }
           return xml;
       }

        /// <summary>
        /// 发送视频消息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <param name="mediaId"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
       public static string GetVideoXml(string openId, string serverId, string mediaId, string title, string description)
        {
            string xml = string.Empty;
            if (!string.IsNullOrEmpty(mediaId))
            {
               xml = string.Format(@"<xml>
                                  <ToUserName><![CDATA[{0}]]></ToUserName>
                                  <FromUserName><![CDATA[{1}]]></FromUserName>
                                  <CreateTime>{2}</CreateTime>
                                  <MsgType><![CDATA[video]]></MsgType>
                                  <Video>
                                  <MediaId><![CDATA[{3}]]></MediaId>
                                  <Title><![CDATA[{4}]]></Title>
                                  <Description><![CDATA[{5}]]></Description>
                                  </Video> 
                                  </xml>", openId, serverId, WxUtils.GetWeixinDateTime(DateTime.Now), mediaId, title, description);
            }
            return xml;
        }

        /// <summary>
        /// 发送音乐消息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <param name="title">音乐标题（非必须）</param>
        /// <param name="description">音乐描述（非必须）</param>
        /// <param name="musicUrl">音乐链接</param>
        /// <param name="hqMusicUrl">高品质音乐链接，wifi环境优先使用该链接播放音乐</param>
        /// <param name="thumbMediaId">视频缩略图的媒体ID</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
       public static string GetMusicXml(string openId, string serverId, string title, string description,
                                    string musicUrl, string hqMusicUrl, string thumbMediaId)
        {
            string xml = string.Empty;
            if (!string.IsNullOrEmpty(musicUrl))
            {
               xml = string.Format(@"<xml>
                                      <ToUserName><![CDATA[{0}]]></ToUserName>
                                      <FromUserName><![CDATA[{1}]]></FromUserName>
                                      <CreateTime>{2}</CreateTime>
                                      <MsgType><![CDATA[music]]></MsgType>
                                      <Music>
                                      <Title><![CDATA[{3}]]></Title>
                                      <Description><![CDATA[{4}]]></Description>
                                      <MusicUrl><![CDATA[{5}]]></MusicUrl>
                                      <HQMusicUrl><![CDATA[{6}]]></HQMusicUrl>
                                      <ThumbMediaId><![CDATA[{7}]]></ThumbMediaId>
                                      </Music>
                                      </xml>", openId, serverId, WxUtils.GetWeixinDateTime(DateTime.Now),title,description,musicUrl,hqMusicUrl,thumbMediaId);
            }
            return xml;

          
        }

        /// <summary>
        /// 发送图文消息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <param name="articles"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
        public static string GetNewsXml(string openId, string serverId,List<WXMesage> articles)
        {
            StringBuilder strhtml = new StringBuilder();
            if (articles != null &&articles.Count > 0)
            {
                strhtml.Append("<xml>");
                strhtml.Append("<ToUserName><![CDATA[" + openId + "]]></ToUserName>");
                strhtml.Append("<FromUserName><![CDATA[" + serverId + "]]></FromUserName>");
                strhtml.Append("<CreateTime>" + WxUtils.GetWeixinDateTime(DateTime.Now) + "</CreateTime>");
                strhtml.Append("<MsgType><![CDATA[news]]></MsgType>");
                strhtml.Append("<ArticleCount>" + articles.Count + "</ArticleCount>");
                strhtml.Append("<Articles>");
                foreach (WXMesage item in articles)
                {
                    strhtml.Append("<item>");
                    strhtml.Append("<Title><![CDATA[" + item.Title + "]]></Title>");
                    strhtml.Append("<Description><![CDATA[" + item.Description + "]]></Description>");
                    strhtml.Append("<PicUrl><![CDATA[" + item.PicUrl + "]]></PicUrl>");
                    strhtml.Append("<Url><![CDATA[" + item.Url + "]]></Url>");
                    strhtml.Append("</item>");
                }
                strhtml.Append("</Articles>");
                strhtml.Append("</xml> ");
            }
            return strhtml.ToString();
        }
    }
}
