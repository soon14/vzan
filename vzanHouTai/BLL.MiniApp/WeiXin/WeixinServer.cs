using System;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Xml;
using DAL.Base;
using Entity.MiniApp;
using BLL.MiniApp;
using System.Web.Script.Serialization;
using Entity.MP;
using BLL.MP;
using Newtonsoft.Json;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using Utility;
using Core.MiniApp;
using BLL.MiniApp.Conf;
//using BLL.MiniApp.Conf;

namespace BLL.MiniApp
{
    /// <summary>  
    /// 微信公众平台操作类  
    /// </summary>  
    public class WeixinServer
    {
        static object state = new object();

        public WeixinServer() { }

        #region 验证消息真实性
        public void Auth(string ServerId)
        {
            string echoStr = HttpContext.Current.Request.QueryString["echoStr"];
            //log4net.LogHelper.WriteInfo(this.GetType(), "Auth---- " + echoStr);
            if (CheckSignature(ServerId))
            {
                if (!string.IsNullOrEmpty(echoStr))
                {
                    HttpContext.Current.Response.Write(echoStr);
                    HttpContext.Current.Response.End();
                }
            }
        }

        bool CheckSignature(string ServerId)
        {
            bool flage = false;
            if (!string.IsNullOrEmpty(ServerId))
            {
                string signature = HttpContext.Current.Request.QueryString["signature"];
                string timestamp = HttpContext.Current.Request.QueryString["timestamp"];
                string nonce = HttpContext.Current.Request.QueryString["nonce"];


                string[] ArrTmp = { "pengxuntest", timestamp, nonce };
                Array.Sort(ArrTmp);
                string tmpStr = string.Join(string.Empty, ArrTmp);
                tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
                tmpStr = tmpStr.ToLower();
                if (tmpStr == signature)
                    flage = true;
            }
            else
                flage = true;
            return flage;
        }

        #endregion

        public void processRequest(string postStr, int id, string types = "")
        {


            try
            {
                Hashtable requestHT = ParseXml(postStr);
                RequestXML requestXML = new RequestXML();
                requestXML.ToUserName = (string)requestHT["ToUserName"];//开发者微信号
                requestXML.FromUserName = (string)requestHT["FromUserName"];//发送方帐号（一个OpenID）
                requestXML.CreateTime = (string)requestHT["CreateTime"];
                requestXML.MsgType = (string)requestHT["MsgType"];
                requestXML.EventKey = (string)requestHT["EventKey"];
                requestXML.Content = (string)requestHT["Content"];
                HttpContext.Current.Response.Output.Write("");

                if (string.IsNullOrEmpty(requestXML.ToUserName))
                    return;
                switch (requestXML.MsgType.ToLower())
                {
                    case "text":
                        this.GetWXContent(requestXML, id);
                        break;
                    case "image":

                        break;

                    case "link":

                        break;
                    case "event":
                        string eventType = (string)requestHT["Event"];
                        if (!string.IsNullOrEmpty(eventType))
                            this.ResponseMsg(requestHT, eventType, requestXML);
                        break;

                    case "voice":

                        break;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }

        /// <summary>
        /// 微信事件处理
        /// </summary>
        /// <param name="requestHT"></param>
        /// <param name="typeName"></param>
        /// <param name="requestXML"></param>
        /// <param name="Servermodel"></param>
        /// <param name="access_token"></param>
        private void ResponseMsg(Hashtable requestHT, string typeName, RequestXML requestXML)
        {
            try
            {
                switch (typeName.ToLower())
                {
                    case "subscribe"://订阅微信号
                        string WxMsg = PushWxMessages(requestXML);
                        HttpContext.Current.Response.Write(WxMsg);
                        if (WebSiteConfig.WxSerId != requestXML.ToUserName && WebSiteConfig.XWGS_WxSerId != requestXML.ToUserName)
                        {
                            return;
                        }
                        RegisterOAuthUser(requestXML);
                        IsAccountLogin(requestXML);
                        BindWeiXin(requestXML);
                        IsWXPFLogin(requestXML);//微信公众号托管平台
                        ResetPassWord(requestXML);
                        break;
                    case "unsubscribe"://用户取消关注时的事件推送
                        if (WebSiteConfig.WxSerId != requestXML.ToUserName && WebSiteConfig.XWGS_WxSerId != requestXML.ToUserName)
                        {
                            lock (state)
                            {
                                this.CancleSubscribe(requestXML);
                            }
                        }
                        break;
                    case "scan"://用户已关注时的事件推送
                        //获取二维码的数据然后写入转接的数据库 requestXML.EventKey 二维码的数据
                        if (WebSiteConfig.WxSerId != requestXML.ToUserName && WebSiteConfig.XWGS_WxSerId != requestXML.ToUserName)
                        {
                            return;
                        }
                        RegisterOAuthUser(requestXML);
                        IsAccountLogin(requestXML);
                        BindWeiXin(requestXML);
                        IsWXPFLogin(requestXML);//微信公众号托管平台
                        ResetPassWord(requestXML);
                        //PinBindWeiXin(requestXML);
                        break;
                    case "click"://点击菜单拉取消息时的事件推送
                        break;

                    case "view"://点击菜单跳转链接时的事件推送

                        break;
                    case "templatesendjobfinish":
                        SendMessageResult(requestXML);
                        break;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }
        /// <summary>
        /// 拼享惠商家绑定后台
        /// </summary>
        /// <param name="requestXML"></param>
        private void PinBindWeiXin(RequestXML xml)
        {
            try
            {
                if (xml == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(xml.FromUserName))
                {
                    return;
                }
                if (string.IsNullOrEmpty(xml.EventKey))
                {
                    return;
                }
                string key = xml.EventKey.Replace("qrscene_", "");
                Entity.MiniApp.LoginQrCode lcode = RedisUtil.Get<Entity.MiniApp.LoginQrCode>("wxbindSessionID:" + key);
                if (lcode == null)
                {
                    return;
                }
                lcode.OpenId = xml.FromUserName;
                RedisUtil.Set<Entity.MiniApp.LoginQrCode>("wxbindSessionID:" + key, lcode, TimeSpan.FromMinutes(3));
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }


        /// <summary>
        /// 将xml文件转换成Hashtable
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Hashtable ParseXml(string xml)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                XmlNode bodyNode = xmlDocument.ChildNodes[0];
                Hashtable ht = new Hashtable();
                if (bodyNode.ChildNodes.Count > 0)
                {
                    foreach (XmlNode xn in bodyNode.ChildNodes)
                    {
                        ht.Add(xn.Name, xn.InnerText);
                    }
                }
                return ht;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(WeixinServer), ex);
                return null;
            }
        }

        #region 互推注册获取ID
        /// <summary>
        /// www.vzan.com 扫码登录
        /// </summary>
        /// <param name="requestXML"></param>
        private void IsAccountLogin(RequestXML requestXML)
        {
            //扫码登陆
            if (requestXML == null)
            {
                return;
            }
            string qrscene = requestXML.EventKey;
            if (string.IsNullOrEmpty(qrscene))
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "微信扫码：qrscene为空");
                return;
            }
            qrscene = qrscene.Replace("qrscene_", "");
            Entity.MiniApp.LoginQrCode lcode = RedisUtil.Get<Entity.MiniApp.LoginQrCode>("SessionID:" + qrscene);
            if (lcode == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "微信扫码：lcode is null");
                return;
            }
            lcode.OpenId = requestXML.FromUserName;



            //扫描小未公司公众号
            if (requestXML.ToUserName == "gh_6014346f8435")
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), "微信扫码5：" + Newtonsoft.Json.JsonConvert.SerializeObject(requestXML));
                lcode.WxUser = WxHelper.GetWxUserInfo("gh_6014346f8435", requestXML.FromUserName);
            }
            //扫描小未科技公众号
            else
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), $"微信扫码4：" + Newtonsoft.Json.JsonConvert.SerializeObject(requestXML));
                lcode.WxUser = WxHelper.GetWxUserInfo(WxHelper.GetToken(), requestXML.FromUserName);
            }
            lcode.IsLogin = true;
            RedisUtil.Set<Entity.MiniApp.LoginQrCode>("SessionID:" + qrscene, lcode, TimeSpan.FromMinutes(1));
        }

        #endregion



        /// <summary>
        /// 取消关注
        /// </summary>
        /// <param name="xml"></param>
        private void CancleSubscribe(RequestXML xml)
        {
            if (!string.IsNullOrEmpty(xml.FromUserName))
            {
                new wxuserinfoBLL().Delete(string.Format("serverid='{0}' and openid='{1}'", xml.ToUserName, xml.FromUserName));
            }
        }



        //绑定微信
        private void BindWeiXin(RequestXML xml)
        {
            try
            {
                if (xml == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(xml.FromUserName))
                {
                    return;
                }
                if (string.IsNullOrEmpty(xml.EventKey))
                {
                    return;
                }
                string key = xml.EventKey.Replace("qrscene_", "");
                Entity.MiniApp.LoginQrCode lcode = RedisUtil.Get<Entity.MiniApp.LoginQrCode>("bindwxid:" + key);
                if (lcode == null)
                {
                    return;
                }
                lcode.OpenId = xml.FromUserName;
                RedisUtil.Set<Entity.MiniApp.LoginQrCode>("bindwxid:" + key, lcode, TimeSpan.FromMinutes(3));
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }


        /// <summary>
        /// 处理文本消息
        /// </summary>
        private void GetWXContent(RequestXML requestXML, int id)
        {
            try
            {
                if (requestXML == null)
                    return;
                if (string.IsNullOrEmpty(requestXML.Content) || string.IsNullOrEmpty(requestXML.ToUserName))
                    return;

                string strXml = string.Empty;
                string where = string.Format("Types='WxAmAway' and ServerId='{0}' and Keywords='{1}'", requestXML.ToUserName, requestXML.Content);
                WxAmAway wxaway = new WxAmAwayBLL().GetModel(where);
                List<WxNews> ls = null;
                if (wxaway == null)
                {
                    wxaway = new WxAmAway();
                }
                ls = new WxNewsBLL().GetList("IsShow=1 and groupid='" + wxaway.GroupId + "'");
                List<WXMesage> msgList = new List<WXMesage>();
                if (ls == null || ls.Count == 0)
                {
                    string config = System.Configuration.ConfigurationManager.AppSettings["wxsearchapi"];
                    string api = string.Format(config, id, HttpUtility.UrlEncode(requestXML.Content));
                    string json = HttpHelper.GetData(api);
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    List<Message> list = jss.Deserialize<List<Message>>(json);
                    if (list != null && list.Count > 0)
                    {
                        foreach (Message model in list)
                        {
                            WXMesage msg = new WXMesage();
                            msg.Title = FilterHandler.FilterHtml(model.bestFragment);//图文标题 
                            msg.Description = FilterHandler.FilterHtml(model.bestFragment);//图文简介
                            msg.Url = model.wapUrl;//图文推送连接
                            msg.PicUrl = MessageBLL.GetPicUrl(model);//图文背景
                            msg.Title = msg.Title.Length >= 36 ? msg.Title.Substring(0, 36) : msg.Title;
                            msg.Description = msg.Description.Length >= 36 ? msg.Description.Substring(0, 36) : msg.Description;
                            if (string.IsNullOrEmpty(msg.PicUrl))
                            {
                                msg.PicUrl = model.headImgUrl;
                            }
                            msg.MsgType = "news";
                            if (!string.IsNullOrEmpty(msg.Title))
                            {
                                msgList.Add(msg);
                            }
                        }
                        if (msgList.Count > 0)
                        {
                            msgList = msgList.OrderByDescending(p => p.PicUrl).Take(7).ToList();
                            msgList.Add(new WXMesage() { Title = "查看更多", Url = WebSiteConfig.WsqUrl + "/f/s-" + id });
                        }
                    }
                    else
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "json转换失败" + msgList.Count);
                    }
                }
                foreach (WxNews model in ls)
                {
                    WXMesage msg = new WXMesage();
                    string imgpath = string.Empty;
                    if (model.PathFile != null && model.PathFile.IndexOf("http://") > -1)
                    {
                        imgpath = string.IsNullOrEmpty(model.PathFile) ? "" : model.PathFile;
                    }
                    else
                    {
                        imgpath = string.IsNullOrEmpty(model.PathFile) ? "" : WebSiteConfig.SourceContent + model.PathFile;
                    }
                    msg.Content = model.Content;
                    msg.MsgType = model.MsgType;
                    msg.Title = model.Title;//图文标题 
                    msg.Url = model.Urls;//图文推送连接
                    msg.PicUrl = imgpath;//图文背景
                    msg.Title = msg.Title.Length >= 36 ? msg.Title.Substring(0, 36) : msg.Title;
                    msg.Description = model.Description.Length >= 36 ? model.Description.Substring(0, 36) : model.Description;
                    msg.MediaId = model.mediaUrl;
                    msgList.Add(msg);
                }
                //log4net.LogHelper.WriteInfo(this.GetType(), "msgList.Count=>" + msgList.Count);
                if (msgList.Where(p => p.MsgType == "news").Count() > 0)//图文
                {
                    strXml = AmAwayApi.GetNewsXml(requestXML.FromUserName, requestXML.ToUserName, msgList.Take(10).ToList());
                }
                if (msgList.Where(p => p.MsgType == "text").Count() > 0)//文本
                {
                    strXml = AmAwayApi.GetTextXml(requestXML.FromUserName, requestXML.ToUserName, msgList[0].Content);
                }
                if (msgList.Where(p => p.MsgType == "image").Count() > 0)//图片
                {
                    strXml = AmAwayApi.GetImageXml(requestXML.FromUserName, requestXML.ToUserName, msgList[0].MediaId);
                }
                if (msgList.Where(p => p.MsgType == "audio").Count() > 0)//语音
                {
                    strXml = AmAwayApi.GetVoiceXml(requestXML.FromUserName, requestXML.ToUserName, msgList[0].MediaId);
                }
                if (msgList.Where(p => p.MsgType == "video").Count() > 0)//视频
                {
                    strXml = AmAwayApi.GetVideoXml(requestXML.FromUserName, requestXML.ToUserName, msgList[0].MediaId, msgList[0].Title, msgList[0].Description);
                }
                if (!string.IsNullOrEmpty(strXml))
                {
                    HttpContext.Current.Response.Output.Write(strXml);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }

        private string PushWxMessages(RequestXML requestXML)
        {
            string strXml = string.Empty;
            return strXml;
        }

        //重置密码
        private void ResetPassWord(RequestXML requestXML)
        {
            if (requestXML == null)
            {
                return;
            }
            string EventKey = requestXML.EventKey;
            if (string.IsNullOrEmpty(EventKey))
            {
                return;
            }
            EventKey = EventKey.Replace("qrscene_", "");
            Dictionary<string, string> dic = RedisUtil.Get<Dictionary<string, string>>("resetpassword:" + EventKey);
            if (dic == null)
            {
                return;
            }
            string openId = dic["OpenId"];
            if (openId != requestXML.FromUserName)
            {
                return;
            }
            RedisUtil.Set<object>("resetpassword:" + EventKey, new { OpenId = requestXML.FromUserName, IsScan = 1 }, TimeSpan.FromHours(3));
        }

        private void IsWXPFLogin(RequestXML requestXML)
        {
            //扫码登陆
            if (requestXML == null)
            {
                return;
            }
            string qrscene = requestXML.EventKey;
            if (string.IsNullOrEmpty(qrscene))
            {
                return;
            }
            qrscene = qrscene.Replace("qrscene_", "");
            Entity.MiniApp.LoginQrCode lcode = RedisUtil.Get<Entity.MiniApp.LoginQrCode>("wxpf:" + qrscene);
            if (lcode == null)
            {
                return;
            }
            lcode.OpenId = requestXML.FromUserName;
            lcode.IsLogin = true;
            RedisUtil.Set<Entity.MiniApp.LoginQrCode>("wxpf:" + qrscene, lcode, TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// 添加新用户到基础表
        /// </summary>
        /// <param name="xml"></param>
        public void RegisterOAuthUser(RequestXML xml)
        {
            try
            {
                if (xml == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(xml.FromUserName))
                {
                    return;
                }
                
                //log4net.LogHelper.WriteInfo(this.GetType(), $"扫码事件推送：{JsonConvert.SerializeObject(xml)}");
                UserBaseInfo umodel = UserBaseInfoBLL.SingleModel.GetModelByOpenId(xml.FromUserName, xml.ToUserName);
                if (umodel == null)
                {
                    WeiXinUser wx = WxHelper.GetWxUserInfo(WxHelper.GetToken(), xml.FromUserName);
                    if (wx != null && !string.IsNullOrEmpty(wx.openid))
                    {
                        umodel = new UserBaseInfo();
                        umodel.headimgurl = wx.headimgurl;
                        umodel.nickname = wx.nickname;
                        umodel.openid = wx.openid;
                        umodel.unionid = wx.unionid;
                        umodel.country = wx.country;
                        umodel.sex = wx.sex;
                        umodel.city = wx.city;
                        umodel.province = wx.province;
                        umodel.serverid = xml.ToUserName;
                        UserBaseInfoBLL.SingleModel.Add(umodel);
                    }
                }
                else
                {
                    WeiXinUser wx = WxHelper.GetWxUserInfo(WxHelper.GetToken(), xml.FromUserName);
                    if (wx != null && !string.IsNullOrEmpty(wx.openid) && umodel.headimgurl != wx.headimgurl)
                    {
                        umodel.headimgurl = wx.headimgurl;
                        UserBaseInfoBLL.SingleModel.Update(umodel, "headimgurl");
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"报错扫码事件推送：{JsonConvert.SerializeObject(ex)}");
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
        }

        private void SendMessageResult(RequestXML requestXML)
        {
            log4net.LogHelper.WriteInfo(this.GetType(), $"模板消息发送结果：{JsonConvert.SerializeObject(requestXML)}");
        }

    }
}