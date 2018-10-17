using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml.Linq;
using BLL.OpenWx;
using Core.OpenWx;
using Entity.OpenWx;

namespace OpenWx.Controllers
{
    /// <summary>
    /// 第三方开放平台演示
    /// </summary>
    public class OpenController : Controller
    {
        private object _objlock = new object();
        
        /// <summary>
        /// 发起授权页的体验URL
        /// </summary>
        /// <returns></returns>
        public ActionResult OAuth()
        {
            OpenPlatConfig currentmodel = OpenPlatConfigBLL.SingleModel.getCurrentModel();
            string component_access_token = currentmodel.component_access_token;
            //获取预授权码
            var preAuthCode = WxRequest.GetPreAuthCode(currentmodel.component_Appid, component_access_token);
            if (preAuthCode == null)
            {
                return Content("获取预授权码失败");
            }

            string callbackUrl = $"{WxRequest._component_Host}/OpenOAuth/OpenOAuthCallback";//成功回调地址
            string url = WxRequest.ReturnUrl(currentmodel.component_Appid, preAuthCode.pre_auth_code, callbackUrl);
            return Redirect(url);
        }
        
        public ActionResult Callback(PostModel postModel)
        {
            //此处的URL格式类型为：http://weixin.senparc.com/Open/Callback/$APPID$， 在RouteConfig中进行了配置，你也可以用自己的格式，只要和开放平台设置的一致。
            if (postModel==null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "微信回调信息postModel为null");
                return Content("success");
            }
            
            postModel.Token = WxRequest._component_Token;
            postModel.EncodingAESKey = WxRequest._component_EncodingAESKey;//根据自己后台的设置保持一致
            postModel.AppId = WxRequest._component_AppId;//根据自己后台的设置保持一致
            XDocument doc = WXRequestCommandBLL.Init(Request.InputStream, postModel);//执行
            string result = WXRequestCommandBLL.CommandWXCallback(doc,postModel);
            return Content(result);
        }
        public ActionResult Callback2(PostModel postModel)
        {
            if (postModel == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "微信回调信息postModel为null");
                return Content("success");
            }

            try
            {
                postModel.Token = WxRequest._component_Token;
                postModel.EncodingAESKey = WxRequest._component_EncodingAESKey; //根据自己后台的设置保持一致
                postModel.AppId = WxRequest._component_AppId; //根据自己后台的设置保持一致
                var stream = Request.InputStream;

                var tempdoc = XmlUtility.Convert(stream);
                if (tempdoc == null)
                {
                    return Content("success");
                }
                var postDataDocument = XDocument.Parse(tempdoc.ToString());
                postDataDocument = WXRequestCommandBLL.Init(postModel, postDataDocument);

                if (postDataDocument != null)
                {
                    string returntype = postDataDocument?.Root?.Element("Event")?.Value;
                    switch (returntype)
                    {
                        case "weapp_audit_success":
                        case "weapp_audit_fail":
                            //小程序代码审核成功回调
                            WXRequestCommandBLL.CommandXCXPublish(returntype, postDataDocument);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "微信回调信息错误");
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }

            return Content("success");
        }

        /// <summary>
        /// 微信服务器会不间断推送最新的Ticket（10分钟一次），需要在此方法中更新缓存
        /// 授权事件接收URL
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult notice(PostModel postModel)
        {
            postModel.Token = WxRequest._component_Token;
            postModel.EncodingAESKey = WxRequest._component_EncodingAESKey;//根据自己后台的设置保持一致
            postModel.AppId = WxRequest._component_AppId;//根据自己后台的设置保持一致
            XDocument doc = WXRequestCommandBLL.Init(Request.InputStream, postModel);//执行
            WXRequestCommandBLL.CommandWXCallback(doc,postModel);

            return Content("success");
        }
    }
}
