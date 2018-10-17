using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Alading;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Utility;


namespace BLL.MiniApp.Alading
{
    public class AlaDingAppInfoBLL : BaseMySql<AlaDingAppInfo>
    {
        #region 单例模式
        private static AlaDingAppInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private AlaDingAppInfoBLL()
        {

        }

        public static AlaDingAppInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AlaDingAppInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /*
        阿拉丁平台账号：
        账号：18023453930
        密码：18023453930
        third_id : 561f3f35dc038c84000b5dd168f516e7
        third_secret : 9fc5e7966f2d46017b83a14d0a0119db
        */
        public static Dictionary<string, string> urlDic = new Dictionary<string, string> {
            { "GetCode","http://openapi.aldwx.com/Main/action/Oauth/Oauth/authorize"},
            { "GetToken","http://openapi.aldwx.com/Main/action/Oauth/Oauth/access_token"},
            { "Register","http://openapi.aldwx.com/Main/action/Appregister/Appregister/getApp"},
            { "RedirectUrl","https://wtapi.vzan.com/api/base/getCode"}
        };
        public static string third_id = "561f3f35dc038c84000b5dd168f516e7";
        public static string third_secret = "9fc5e7966f2d46017b83a14d0a0119db";
        public string accessTokenCacheKey = $"ald_accesstoken_{0}";

        public AlaDingAppInfo GetModelByAppId(string appId)
        {
            return GetModel($"appid='{appId}'");
        }

        /// <summary>
        /// 注册小程序的阿拉丁统计appkey
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string RegisterApp(AlaDingAppInfo model)
        {
            if (!WebSiteConfig.UseALaDing)
                return string.Empty;

            if (model == null || string.IsNullOrEmpty(model.AppId))
                return string.Empty;
            
            AlaDingAppInfo existsModel = base.GetModel($"appid='{model.AppId}'");
            if (existsModel != null)
                return existsModel.AppKey;

            string access_token = GetAccessToken(model.AppId);
            if (string.IsNullOrEmpty(access_token))
                return string.Empty;

            string registerStr = HttpHelper.PostData(urlDic["Register"], string.Join("&", new string[] {
                $"access_token={access_token}",
                $"app_name={model.Name}",
                $"app_logo={model.Logo}",
                $"user_id=点赞科技"
            }));
            RegisterResult registerResult = null;
            if(!string.IsNullOrEmpty(registerStr))
            {
                registerResult =Newtonsoft.Json.JsonConvert.DeserializeObject<RegisterResult>(registerStr);
            }
            if (registerResult != null && registerResult.code == "200")
                model.AppKey = registerResult.data.appkey;

            if (Convert.ToInt32(base.Add(model)) > 0)
                return model.AppKey;

            return string.Empty;

        }

        /// <summary>
        /// 获取access_token,如果缓存中已经存在，可以省略前两步
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public string GetAccessToken(string appId)
        {
            if (!WebSiteConfig.UseALaDing)
                return "";

            string code = string.Empty;
            string cacheKey = string.Format(accessTokenCacheKey, appId);
            string accessToken = RedisUtil.Get<string>(cacheKey);
            if (!string.IsNullOrEmpty(accessToken))
                return accessToken;

            //第一步
            //ReturnMsg codeResult = HttpHelper.PostData<ReturnMsg>(urlDic["GetCode"], string.Join("&", new string[] {
            //    $"response_type=code",
            //    $"client_id={third_id}",
            //    "state=1",
            //    $"redirect_uri={urlDic["RedirectUrl"]}"
            //}));

            //if (codeResult != null && codeResult.code == 1)
            //    code = codeResult.obj.ToString();

            ////第二步：通过code获取access_token，access_token有效期是一天，因为注册成功之后就保存到数据库了，不需要判断这个有效期
            //TokenResult tokenResult = HttpHelper.PostData<TokenResult>(urlDic["GetToken"], string.Join("&", new string[] {
            //    $"grant_type=authorization_code",
            //    $"client_id={third_id}",
            //    $"client_secret={third_secret}",
            //    $"code={code}",
            //    $"redirect_uri={urlDic["RedirectUrl"]}",
            //}));

            //第一步
            string codeStr = HttpHelper.PostData(urlDic["GetCode"], string.Join("&", new string[] {
                $"response_type=code",
                $"client_id={third_id}",
                "state=1",
                $"redirect_uri={urlDic["RedirectUrl"]}"
            }));
            ReturnMsg codeResult = null;
            if(!string.IsNullOrEmpty(codeStr))
            {
                codeResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnMsg>(codeStr);
            }

            if (codeResult != null && codeResult.code == 1)
                code = codeResult.obj.ToString();

            //第二步：通过code获取access_token，access_token有效期是一天，因为注册成功之后就保存到数据库了，不需要判断这个有效期
            string tokenStr = HttpHelper.PostData(urlDic["GetToken"], string.Join("&", new string[] {
                $"grant_type=authorization_code",
                $"client_id={third_id}",
                $"client_secret={third_secret}",
                $"code={code}",
                $"redirect_uri={urlDic["RedirectUrl"]}",
            }));
            TokenResult tokenResult = null;
            if (!string.IsNullOrEmpty(tokenStr))
            {
                tokenResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResult>(tokenStr);
            }

            if (tokenResult != null)
                return tokenResult.access_token;

            return string.Empty;
        }
    }

    /// <summary>
    /// 获取Token的返回值
    /// </summary>
    public class TokenResult
    {
        public string access_token { get; set; } = string.Empty;
        public int expires_in { get; set; } = 0;
        public string token_type { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;

        public string refresh_token { get; set; } = string.Empty;
    }

    /// <summary>
    /// 注册小程序的返回值
    /// </summary>
    public class RegisterResult
    {
        public string code { get; set; } = string.Empty;
        public string msg { get; set; } = string.Empty;

        public KeyData data { get; set; } = new KeyData();

        public string user_id { get; set; } = string.Empty;
    }
    public class KeyData
    {
        public string appkey { get; set; } = string.Empty;
    }
}
