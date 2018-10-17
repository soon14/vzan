using Core.OpenWx;
using Entity.OpenWx;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Entity.OpenWx.MiniAppEnum;

namespace BLL.OpenWx
{
    public static class WXRequestCommandBLL
    {
        public static XDocument Init(Stream inputStream, PostModel postModel)
        {
            XDocument edocument = XmlUtility.Convert(inputStream);
            if (postModel != null && edocument != null && edocument.Root.Element("Encrypt") != null && !string.IsNullOrEmpty(edocument.Root.Element("Encrypt").Value))
            {
                //解密XML信息
                string postDataStr = edocument.ToString();

                WXBizMsgCrypt msgCrype = new WXBizMsgCrypt(postModel.Token, postModel.EncodingAESKey, postModel.AppId);

                string msgXml = null;
                int result = msgCrype.DecryptMsg(postModel.Msg_Signature, postModel.Timestamp, postModel.Nonce, postDataStr, ref msgXml);

                //判断result类型
                if (result != 0)
                {
                    return null;
                }
                return XDocument.Parse(msgXml);//完成解密
            }

            return edocument;
        }

        public static XDocument Init(PostModel postModel, XDocument postDataDocument)
        {
            //进行加密判断并处理
            var postDataStr = postDataDocument.ToString();
            XDocument decryptDoc = postDataDocument;

            if (postModel != null && postDataDocument.Root.Element("Encrypt") != null && !string.IsNullOrEmpty(postDataDocument.Root.Element("Encrypt").Value))
            {
                WXBizMsgCrypt msgCrype = new WXBizMsgCrypt(postModel.Token, postModel.EncodingAESKey, postModel.AppId);
                string msgXml = null;
                var result = msgCrype.DecryptMsg(postModel.Msg_Signature, postModel.Timestamp, postModel.Nonce, postDataStr, ref msgXml);

                if (postDataDocument.Root.Element("FromUserName") != null && !string.IsNullOrEmpty(postDataDocument.Root.Element("FromUserName").Value))
                {

                }

                decryptDoc = XDocument.Parse(msgXml);//完成解密
            }

            return decryptDoc;
        }

        /// <summary>
        /// 全网发布时处理文本发送
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public static string OnTextRequest(RequestMessageText requestMessage, PostModel postModel)
        {
            if (requestMessage.Content == "TESTCOMPONENT_MSG_TYPE_TEXT")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<xml>");
                sb.Append($"<ToUserName><![CDATA[{requestMessage.FromUserName}]]></ToUserName>");
                sb.Append($"<FromUserName><![CDATA[{requestMessage.ToUserName}]]></FromUserName>");
                sb.Append($"<CreateTime>{DateTimeHelper.GetTimeStamp(true)}</CreateTime>");
                sb.Append($"<MsgType><![CDATA[text]]></MsgType>");
                sb.Append($"<Content><![CDATA[TESTCOMPONENT_MSG_TYPE_TEXT_callback]]></Content>");
                sb.Append("</xml>");

                var timeStamp = DateTime.Now.Ticks.ToString();
                var nonce = DateTime.Now.Ticks.ToString();

                //必须要加密
                WXBizMsgCrypt msgCrype = new WXBizMsgCrypt(postModel.Token, postModel.EncodingAESKey, postModel.AppId);
                string finalResponseXml = null;
                msgCrype.EncryptMsg(sb.ToString().Replace("\r\n", "\n")/* 替换\r\n是为了处理iphone设备上换行bug */, timeStamp, nonce, ref finalResponseXml);//TODO:这里官方的方法已经把EncryptResponseMessage对应的XML输出出来了

                return finalResponseXml;
            }

            if (requestMessage.Content.StartsWith("QUERY_AUTH_CODE:"))
            {
                string openTicket = OpenPlatConfigBLL.SingleModel.GetComponentVerifyTicket();
                string query_auth_code = requestMessage.Content.Replace("QUERY_AUTH_CODE:", "");
                try
                {
                    OpenPlatConfig currentmodel = OpenPlatConfigBLL.SingleModel.getCurrentModel();
                    QueryAuthResult oauthResult = WxRequest.QueryAuth(currentmodel.component_access_token, currentmodel.component_Appid, query_auth_code);

                    //调用客服接口
                    string content = query_auth_code + "_from_api";

                    //Task.Run(() => {
                    //    Thread.Sleep(1000);
                    //    WxRequest.SendText(oauthResult.authorization_info.authorizer_access_token, requestMessage.FromUserName, content);
                    //});

                    WxRequest.SendText(oauthResult.authorization_info.authorizer_access_token, requestMessage.FromUserName, content);
                    return "";
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(typeof(WXRequestCommandBLL), ex);
                }
            }
            return "success";
        }

        /// <summary>
        /// 处理小程序代码审核回调
        /// </summary>
        public static void CommandXCXPublish(string returntype, XDocument postDataDocument)
        {
            string username = postDataDocument.Root.Element("ToUserName").Value;
            string reason = string.Empty;

            //小程序代码审核成功回调
            OpenAuthorizerInfo amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(username);
            if (amodel == null)
            {
                log4net.LogHelper.WriteInfo(typeof(WXRequestCommandBLL), "【" + returntype + "】" + username + "微信回调：找不到授权记录");
                return;
            }

            //小程序上传代码记录
            UserXcxTemplate userxcxtemplate = UserXcxTemplateBLL.SingleModel.GetModelByUserName(username);
            if (userxcxtemplate == null)
            {
                log4net.LogHelper.WriteInfo(typeof(WXRequestCommandBLL), "【" + returntype + "】" + username + "微信回调：找不到小程序上传代码记录");
                return;
            }

            //代码审核通过
            if (returntype == "weapp_audit_success")
            {
                //发布审核通过的小程序代码
                var cresult = WxRequest.Release(amodel.authorizer_access_token);

                userxcxtemplate.PreAuditId = userxcxtemplate.Auditid;
                //更改上传记录信息
                userxcxtemplate.State = cresult.errcode == 0 ? 3 : 4;
                userxcxtemplate.UpdateTime = DateTime.Now;
                userxcxtemplate.Reason = cresult.errcode == 0 ? "发布成功" : "发布失败" + cresult.errcode.ToString();
            }
            else
            {
                //更改上传记录信息
                userxcxtemplate.State = 1;
                userxcxtemplate.UpdateTime = DateTime.Now;
                userxcxtemplate.Reason = "审核不通过，" + postDataDocument.Root.Element("Reason").Value;
            }

            UserXcxTemplateBLL.SingleModel.Update(userxcxtemplate, "Reason,UpdateTime,State,PreAuditId");
        }

        /// <summary>
        /// 微信回调消息处理
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string CommandWXCallback(XDocument doc, PostModel postModel)
        {
            if (doc == null)
                return "success";

            RequestMessageBase requestMessage = new RequestMessageBase();
            try
            {
                if (doc.Root.Element("MsgType") == null && doc.Root.Element("InfoType") == null)
                {
                    return "success";
                }
                RequestMsgType msgType = RequestMsgType.EVENT;
                if (doc.Root.Element("MsgType") != null)
                {
                    msgType = (RequestMsgType)System.Enum.Parse(typeof(RequestMsgType), doc.Root.Element("MsgType").Value.ToUpper());
                }

                switch (msgType)
                {
                    case RequestMsgType.TEXT:
                        RequestMessageText requestMessageTest = EntityHelper.FillEntityWithXml(new RequestMessageText(), doc);
                        return OnTextRequest(requestMessageTest, postModel);
                    case RequestMsgType.EVENT:
                        if (doc.Root.Element("InfoType") == null)
                            break;

                        RequestInfoType infoType = (RequestInfoType)System.Enum.Parse(typeof(RequestInfoType), doc.Root.Element("InfoType").Value, true);

                        switch (infoType)
                        {
                            case RequestInfoType.component_verify_ticket:
                                RequestMessageComponentVerifyTicket tempModel = EntityHelper.FillEntityWithXml(new RequestMessageComponentVerifyTicket(), doc);

                                OpenPlatConfigBLL.SingleModel.ComponentVerifyTicket(tempModel.AppId, tempModel.ComponentVerifyTicket, tempModel.CreateTime);
                                break;
                            case RequestInfoType.unauthorized:
                                break;
                            case RequestInfoType.weapp_audit_success:
                            case RequestInfoType.weapp_audit_fail:
                                //小程序代码审核成功回调
                                CommandXCXPublish(infoType.ToString(), doc);
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(WXRequestCommandBLL), ex);
            }

            return "success";
        }
    }
}
