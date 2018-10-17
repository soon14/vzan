using AutoMapper;
using BLL.MiniApp;
using BLL.MiniApp.Dish;
using Core.MiniApp;
using Core.MiniApp.Common;
using Core.MiniApp.DTO;
using Core.MiniApp.WeiXin;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;
using Utility.AliOss;

namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("Common"), Route("{action}")]
    public class CommonController : BaseController
    {
        public static string webview_appid = ConfigurationManager.AppSettings["webview_appid"];
        public static string webview_appsecret = ConfigurationManager.AppSettings["webview_appsecret"];

        [NonAction]
        public string GetPageUrl()
        {
            string host = Request.Url.Host;
            string pageUrl = Request.Url.ToString().Split('#')[0];
            if (host.ToLower() == "wtapi.vzan.com")
            {
                pageUrl = pageUrl.Replace("http://", "https://");
            }
            return pageUrl;
        }

        [HttpGet]
        public ActionResult GetJSSDK(string url)
        {
            string token = WxHelper.GetToken(webview_appid, webview_appsecret, false);
            string appId = AccessTokenContainer.GetFirstOrDefaultAppId();
            string ticket = JsApiTicketContainer.GetJsApiTicket(webview_appid);
            string timestamp= JSSDKHelper.GetTimestamp();
            string nonceStr = JSSDKHelper.GetNoncestr();
            string signature = JSSDKHelper.GetSignature(ticket, nonceStr, timestamp, HttpUtility.UrlDecode(url));
            //string accesstoken = AccessTokenContainer.TryGetAccessToken(webview_appid, webview_appsecret);

            object SDK = new
            {
                AppId = webview_appid,
                Timestamp = int.Parse(timestamp),
                NonceStr = nonceStr,
                Signature = signature,
                Ticket = ticket,
                Url = url,
            };

            return ApiModel(isok: true, message: "获取成功", data: SDK);
        }
      
        [HttpGet]
        public ActionResult GetOAuthUrl()
        {
            string oauthUrl = string.Empty;
            string sessionGUID = string.Empty;
            string host = Request.Url.Host;

            if (WebSiteConfig.Environment == "dev")
            {
                //测试环境域名
                host = "testdz.vzan.com";
            }
            string success_callBack = $"http://{host}/Shop/Admin/WechatLogin/{{0}}";

            if (!WxOAuth.SingleModel.GetOAuthUrl(success_callBack, out oauthUrl, out sessionGUID))
            {
                return ApiModel(message: "接口生成失败[RedisError]");
            }

            return ApiModel(isok: true, message: "获取成功", data: new { OAuthUrl = oauthUrl, SessionGUID = sessionGUID, success_callBack });
        }

        public ActionResult SaveImg(string mediaId = "")
        {
            int times = 1;
            reGetToken:
            string token = WxHelper.GetToken(webview_appid, webview_appsecret, false);
            string url = string.Empty;
            var posturl = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(posturl);
            byte[] result = null;
            using (WebResponse response = req.GetResponse())
            {
                //在这里对接收到的页面内容进行处理 
                Stream stream = response.GetResponseStream();
                List<byte> bytes = new List<byte>();
                int temp = stream.ReadByte();
                while (temp != -1)
                {
                    bytes.Add((byte)temp);
                    temp = stream.ReadByte();
                }

                if (response.ContentType == "image/jpeg")
                {

                    result = bytes.ToArray();
                }
                else
                {
                    string errorResult = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
                    if (errorResult.IndexOf("40001") != -1 && times < 3)
                    {
                        WxHelper.GetToken(webview_appid, webview_appsecret, true);
                        times += 1;
                        goto reGetToken;
                    }
                }


            }
            string fileExtension = ".jpg";
            if (result != null && result.Length > 0)
            {
                string ossurl = AliOSSHelper.GetOssImgKey(fileExtension.Replace(".", ""), false, out url);
                bool putResult = AliOSSHelper.PutObjectFromByteArray(ossurl, result, 1, fileExtension);
                if (putResult)
                {
                    return ApiModel(isok: true, message: "上传成功", data: url);
                }
                else
                {
                    return ApiModel(message: "保存失败");
                }
            }

            return ApiModel(message: "上传失败");
        }
    }
}