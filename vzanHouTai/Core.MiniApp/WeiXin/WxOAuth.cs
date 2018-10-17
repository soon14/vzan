using DAL.Base;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.MiniApp.WeiXin
{
    public class WxOAuth
    {
        #region 单例模式
        private static WxOAuth _singleModel;
        private static readonly object SynObject = new object();

        private WxOAuth()
        {

        }

        public static WxOAuth SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new WxOAuth();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static string AppId = ConfigurationManager.AppSettings["webview_appid"];
        public static string AppSecret = ConfigurationManager.AppSettings["webview_appsecret"];

        /// <summary>
        /// 这个是构造回调网页所需要的前缀， 一定要加上http:// 
        /// </summary>
        public string OAuthDomain { get; set; } = "https://wtapi.vzan.com";
        public string OAuthRoute { get; set; } = "/OAuth/CallBack";
        public string OAuthCallBackUrl { get { return $"{OAuthDomain}/{OAuthRoute}"; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="successCallBack">比如：http://HostName/Shop/Admin/WechatLogin/{sessionGUID}</param>
        /// <param name="oauthUrl"></param>
        /// <param name="sessionGUID"></param>
        /// <returns></returns>
        public bool GetOAuthUrl(string successCallBack,  out string oauthUrl, out string sessionGUID)
        {
            oauthUrl = "";
            sessionGUID = Guid.NewGuid().ToString();
            if (!WriteToken(sessionGUID) || !WriteSuccessCallBack(sessionGUID, string.Format(successCallBack,sessionGUID)))
            {
                return false;
            }
            oauthUrl = OAuthApi.GetAuthorizeUrl(AppId, OAuthCallBackUrl, sessionGUID, Senparc.Weixin.MP.OAuthScope.snsapi_userinfo);
            return true;
        }

        public OAuthAccessTokenResult GetAccessToken(string code)
        {
            return OAuthApi.GetAccessToken(AppId, AppSecret, code);
        }

        public OAuthUserInfo GetOAuthUserInfo(OAuthAccessTokenResult accessToken)
        {
            return OAuthApi.GetUserInfo(accessToken.access_token, accessToken.openid);
        }

        public bool WriteSuccessCallBack(string guid, string successCallBack)
        {
            return RedisUtil.Set($"{guid}_callBack", successCallBack, TimeSpan.FromMinutes(5));
        }

        public string GetSuccessCallBack(string guid)
        {
            return RedisUtil.Get<string>($"{guid}_callBack");
        }

        public bool WriteToken(string guid)
        {
            return RedisUtil.Set($"{guid}_wxToken", true, TimeSpan.FromMinutes(5));
        }

        public bool CheckToken(string guid)
        {
            return RedisUtil.Get<bool>($"{guid}_wxToken");
        }

        public bool ExpireToken(string guid)
        {
            return RedisUtil.Set($"{guid}_wxToken", false, TimeSpan.FromSeconds(1));
        }

        public bool WriteOAuthInfo(string guid, OAuthUserInfo oauthInfo)
        {
            return RedisUtil.Set($"{guid}_oauth", oauthInfo, TimeSpan.FromMinutes(10));
        }

        public OAuthUserInfo GetOAuthInfo(string guid)
        {
            return RedisUtil.Get<OAuthUserInfo>($"{guid}_oauth");
        }
    }
}
