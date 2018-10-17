using System;
using Newtonsoft.Json;
using Utility;
using static Entity.OpenWx.MiniAppEnum;
using Entity.OpenWx;
using System.Net;
using System.Web.Configuration;

namespace Core.OpenWx
{
    public static class  WxRequest
    {
        public readonly static string _component_Host = WebConfigurationManager.AppSettings["Component_Host"];
        public readonly static string _component_AppId = WebConfigurationManager.AppSettings["Component_Appid"];
        public readonly static string _component_Secret = WebConfigurationManager.AppSettings["Component_Secret"];
        public readonly static string _component_Token = WebConfigurationManager.AppSettings["Component_Token"];
        public readonly static string _component_EncodingAESKey = WebConfigurationManager.AppSettings["Component_EncodingAESKey"];

        #region url
        private static string _api_component_tokenUrl = "https://api.weixin.qq.com/cgi-bin/component/api_component_token";
        private static string _api_create_preauthcodeUrl = "https://api.weixin.qq.com/cgi-bin/component/api_create_preauthcode?component_access_token={0}";
        private static string _api_authorizer_tokenUrl = "https://api.weixin.qq.com/cgi-bin/component/api_authorizer_token?component_access_token={0}";
        private static string _queryAuthUrl = "https://api.weixin.qq.com/cgi-bin/component/api_query_auth?component_access_token={0}";
        private static string _getAuthorizerInfoUrl = "https://api.weixin.qq.com/cgi-bin/component/api_get_authorizer_info?component_access_token={0}";
        private static string URL_FORMAT = "https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={0}";

        private static string _getCategoryUrl = "https://api.weixin.qq.com/wxa/get_category?access_token={0}";
        private static string _commitUrl = "https://api.weixin.qq.com/wxa/commit?access_token={0}";
        private static string _getPageUrl = "https://api.weixin.qq.com/wxa/get_page?access_token={0}";
        private static string _submitAuditUrl = "https://api.weixin.qq.com/wxa/submit_audit?access_token={0}";
        private static string _releaseUrl = "https://api.weixin.qq.com/wxa/release?access_token={0}";
        private static string _getAuditstatusUrl = "https://api.weixin.qq.com/wxa/get_auditstatus?access_token={0}";
        private static string _getLatestAuditstatusUrl = "https://api.weixin.qq.com/wxa/get_latest_auditstatus?access_token={0}";
        private static string _undocodeAuditUrl = "https://api.weixin.qq.com/wxa/undocodeaudit?access_token={0}";
        private static string _versionBackUrl = "https://api.weixin.qq.com/wxa/revertcoderelease?access_token={0}";

        private static string _bindTesterUrl = "https://api.weixin.qq.com/wxa/bind_tester?access_token={0}";
        private static string _unbindTesterUrl = "https://api.weixin.qq.com/wxa/unbind_tester?access_token={0}";
        private static string _getMemberauthUrl = "https://api.weixin.qq.com/wxa/memberauth?access_token={0}";

        private static string _modifyDomainUrl = "https://api.weixin.qq.com/wxa/modify_domain?access_token={0}";
        private static string _setWebviewDomainUrl = "https://api.weixin.qq.com/wxa/setwebviewdomain?access_token={0}";

        private static string _getSessionKeyUrl = "https://api.weixin.qq.com/sns/component/jscode2session?appid={0}&js_code={1}&grant_type=authorization_code&component_appid={2}&component_access_token={3}";
        private static string _unbindUrl = "https://api.weixin.qq.com/cgi-bin/open/unbind?access_token={0}";

        private static string _getQrcodeUrl = "https://api.weixin.qq.com/wxa/get_qrcode?access_token={0}";
        private static string _tiaozhuanUrl = "https://mp.weixin.qq.com/cgi-bin/componentloginpage?component_appid={0}&pre_auth_code={1}&redirect_uri={2}";
        #endregion

        /// <summary>
        /// 获取平台token
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appSecret"></param>
        /// <param name="tiket"></param>
        /// <returns></returns>
        public static ComponentAccessTokenResult GetComonentToken(string appid,string appSecret,string tiket)
        {
            var datatoken = new
            {
                component_appid = appid,
                component_appsecret = appSecret,
                component_verify_ticket = tiket
            };
            
            string dataJson = JsonConvert.SerializeObject(datatoken);
            string result = HttpHelper.DoPostJson(_api_component_tokenUrl, dataJson);
            
            ComponentAccessTokenResult token = GetResultModel<ComponentAccessTokenResult>(result);

            return token;
        }

        /// <summary>
        /// 获取预授权码
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appSecret"></param>
        /// <param name="tiket"></param>
        /// <returns></returns>
        public static PreAuthCodeResult GetPreAuthCode(string appid, string accessToken)
        {
            string url =string.Format(_api_create_preauthcodeUrl, accessToken.AsUrlData());
            var data = new
            {
                component_appid = appid
            };

            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            PreAuthCodeResult model = GetResultModel<PreAuthCodeResult>(result);
            return model;
        }

        /// <summary>
        /// 获取小程序token
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="token"></param>
        /// <param name="platAppid"></param>
        /// <param name="refreToken"></param>
        /// <returns></returns>
        public static RefreshAuthorizerTokenResult GetAuthoRizerToken(string appid, string token, string platAppid,string refreToken)
        {
            string url = string.Format(_api_authorizer_tokenUrl, token.AsUrlData());
            var data = new
            {
                component_appid = platAppid,
                authorizer_appid = appid,
                authorizer_refresh_token = refreToken
            };

            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            RefreshAuthorizerTokenResult model = GetResultModel<RefreshAuthorizerTokenResult>(result);
            return model;
        }

        /// <summary>
        /// 使用授权码换取公众号的授权信息
        /// </summary>
        /// <param name="componentAppId">服务开发方的appid</param>
        /// <param name="componentAccessToken">服务开发方的access_token</param>
        /// <param name="authorizationCode">授权code,会在授权成功时返回给第三方平台，详见第三方平台授权流程说明</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
        public static QueryAuthResult QueryAuth(string componentAccessToken, string componentAppId, string authorizationCode)
        {
            string url =
                string.Format(_queryAuthUrl, componentAccessToken.AsUrlData());

            var data = new
            {
                component_appid = componentAppId,
                authorization_code = authorizationCode
            };
            
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            QueryAuthResult model = GetResultModel<QueryAuthResult>(result);
            return model;
        }

        /// <summary>
        /// 获取授权方信息
        /// 注意：此方法返回的JSON中，authorization_info.authorizer_appid等几个参数通常为空（哪怕公众号有权限）
        /// </summary>
        /// <param name="componentAccessToken"></param>
        /// <param name="componentAppId"></param>
        /// <param name="authorizerAppId"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static GetAuthorizerInfoResult GetAuthorizerInfo(string componentAccessToken, string componentAppId, string authorizerAppId)
        {
            string url =string.Format(_getAuthorizerInfoUrl,componentAccessToken.AsUrlData());

            var data = new
            {
                component_appid = componentAppId,
                authorizer_appid = authorizerAppId,
            };

            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            GetAuthorizerInfoResult model = GetResultModel<GetAuthorizerInfoResult>(result);
            return model;
        }
        
        /// <summary>
        /// 获取跳转到授权链接
        /// </summary>
        /// <param name="component_Appid">小程序第三方平台appid</param>
        /// <param name="pre_auth_code">预授权码</param>
        /// <returns></returns>
        public static string ReturnUrl(string component_Appid, string pre_auth_code, string callbackurl)
        {
            var url =
           string.Format(_tiaozhuanUrl, component_Appid.AsUrlData(), pre_auth_code.AsUrlData(), callbackurl.AsUrlData());
            return url;
        }

        /// <summary>
        /// 发送文本信息
        /// </summary>
        /// <param name="accessTokenOrAppId"></param>
        /// <param name="openId"></param>
        /// <param name="content"></param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
        public static WxJsonResult SendText(string accessToken, string openId, string content, int timeOut = 10000)
        {
            string url = string.IsNullOrEmpty(accessToken) ? URL_FORMAT : string.Format(URL_FORMAT, accessToken.AsUrlData());
            var data = new
            {
                touser = openId,
                msgtype = "text",
                text = new
                {
                    content = content
                }
            };
            
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            WxJsonResult model = GetResultModel<WxJsonResult>(result);
            return model;
        }

        #region 代码管理
        /// <summary>
        /// 获取小程序类目
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static Category getCategory(string access_token)
        {
            string url = string.Format(_getCategoryUrl, access_token);
            string result = HttpHelper.GetData(url);
            Category model = GetResultModel<Category>(result);

            return model;
        }

        /// <summary>
        /// 提交小程序代码
        /// </summary>
        /// <param name="componentAccessToken">已授权用户的access_token</param>
        /// <param name="ext_json"></param>
        /// <returns></returns>
        public static OAuthAccessTokenResult Commit(string componentAccessToken, CommitModel ext_json)
        {
            string url = string.Format(_commitUrl, componentAccessToken);
            string dataJson = JsonConvert.SerializeObject(ext_json);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }

        /// <summary>
        /// 获取小程序的第三方提交代码的页面配置（仅供第三方开发者代小程序调用）
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult GetPage(string access_token)
        {
            string url = string.Format(_getPageUrl, access_token);

            string result = HttpHelper.GetData(url);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);

            return model;
        }

        /// <summary>
        /// 提交小程序代码审核
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult SubmitAudit(string access_token, object data)
        {
            string url = string.Format(_submitAuditUrl, access_token);
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }

        /// <summary>
        /// 发布已通过审核的小程序（仅供第三方代小程序调用）
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult Release(string access_token)
        {
            string url = string.Format(_releaseUrl, access_token);
            var data = new { };
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }


        /// <summary>
        /// 小程序审核撤回
        /// 单个帐号每天审核撤回次数最多不超过1次，一个月不超过10次。
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult UndocodeAudit(string access_token)
        {
            string url = string.Format(_undocodeAuditUrl, access_token);

            string result = HttpHelper.GetData(url);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);

            return model;
        }

        /// <summary>
        /// 版本回退
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public static WxJsonResult VersionBack(string access_token)
        {
            string url = string.Format(_versionBackUrl, access_token);
            string result = HttpHelper.GetData(url);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);

            return model;
        }

        /// <summary>
        /// 获取提交小程序代码审核消息
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// /// <param name="access_token">提交审核时获得的审核id</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult GetAuditstatus(string access_token, long auditid)
        {
            string url = string.Format(_getAuditstatusUrl, access_token);
            var data = new { auditid = auditid };
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }

        /// <summary>
        /// 查询最新一次提交的审核状态（仅供第三方代小程序调用）
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult GetLatestAuditstatus(string access_token)
        {
            string url = string.Format(_getLatestAuditstatusUrl, access_token);

            string result = HttpHelper.GetData(url);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);

            return model;
        }
        #endregion

        #region 用户管理

        /// <summary>
        /// 添加体验者
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <param name="wechatid">微信号</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult BindTester(string access_token, string wechatid)
        {
            string url = string.Format(_bindTesterUrl, access_token);
            var data = new { wechatid = wechatid };
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }
        /// <summary>
        /// 解除体验者
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <param name="wechatid">微信号</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult UnbindTester(string access_token, string wechatid)
        {
            string url = string.Format(_unbindTesterUrl, access_token);
            var data = new { wechatid = wechatid };
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }

        /// <summary>
        /// 获取体验者列表
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult GetMemberauth(string access_token)
        {
            string url = string.Format(_getMemberauthUrl, access_token);
            var data = new { action = "get_experiencer" };
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }
        #endregion

        #region 服务器
        /// <summary>
        /// 修改服务器地址
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <param name="wechatid">微信号</param>
        /// <returns></returns>
        public static ServerHost ModifyDomain(string access_token, object data)
        {
            string url = string.Format(_modifyDomainUrl, access_token);
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            ServerHost model = GetResultModel<ServerHost>(result);
            return model;
        }

        /// <summary>
        /// 设置小程序业务域名
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <param name="wechatid">微信号</param>
        /// <returns></returns>
        public static ServerHost SetWebviewDomain(string access_token, object data)
        {
            string url = string.Format(_setWebviewDomainUrl, access_token);
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            ServerHost model = GetResultModel<ServerHost>(result);
            return model;
        }

        #endregion


        /// <summary>
        /// code 换取 session_key
        /// </summary>
        /// <param name="appid">小程序的AppID</param>
        /// <param name="js_code">登录时获取的 code</param>
        /// <param name="component_appid">第三方平台appid</param>
        /// <param name="component_access_token">第三方平台的component_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult GetSessionKey(string appid, string js_code, string componetappid, string componentaccesstoken)
        {
            string url = string.Format(_getSessionKeyUrl, appid, js_code, componetappid, componentaccesstoken);

            string result = HttpHelper.GetData(url);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);

            return model;
        }


        /// <summary>
        /// 解绑平台与小程序
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static OAuthAccessTokenResult Unbind(string access_token, string appid, string open_appid)
        {
            string url = string.Format(_unbindUrl, access_token);
            var data = new { appid = appid, open_appid = open_appid };
            string dataJson = JsonConvert.SerializeObject(data);
            string result = HttpHelper.DoPostJson(url, dataJson);
            OAuthAccessTokenResult model = GetResultModel<OAuthAccessTokenResult>(result);
            return model;
        }


        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="access_token">请使用第三方平台获取到的该小程序授权的authorizer_access_token</param>
        /// <returns></returns>
        public static HttpWebResponse GetQrcode(string access_token)
        {
            var url = string.Format(_getQrcodeUrl, access_token);
            return HttpGet(url);
        }

        public static T GetResultModel<T>(string result)
        {
            WxJsonResult errorResult = JsonConvert.DeserializeObject<WxJsonResult>(result);
            if (errorResult.errcode != ReturnCodeEnum.请求成功)
            {
                log4net.LogHelper.WriteInfo(typeof(WxRequest), errorResult.errcode.ToString());
            }
            T token = JsonConvert.DeserializeObject<T>(result);

            return token;
        }

        public static string AsUrlData(this string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";

            return Uri.EscapeDataString(data);
        }

        public static HttpWebResponse HttpGet(string url, int timeOut = 10000)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = timeOut;
            request.Proxy = null;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }
    }
}
