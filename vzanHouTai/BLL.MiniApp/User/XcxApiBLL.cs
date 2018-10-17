using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Newtonsoft.Json;
using System;
using Utility;
using DAL.Base;
using MySql.Data.MySqlClient;
using Core.MiniApp;
using Entity.MiniApp.Weixin;

namespace BLL.MiniApp
{
    public class XcxApiBLL
    {
        private readonly string _xcxapihosturl = "https://api.weixin.qq.com";
        private readonly string _redis_MiniappGongZhongToken = "MiniappGongZhongToken_{0}";
        private object _lockObj = new object();

        public int _openType = 1;//0：第一开放平台接口链接，1：第二开放平台接口链接
        private string OpenUrl
        {
            get
            {
                switch (_openType)
                {
                    case 0: return WebSiteConfig.XcxAPI;
                    case 1: return WebSiteConfig.XcxAPIDzOpen;
                    default: return "";
                }
            }
        }

        #region 单例模式
        private static XcxApiBLL _singleModel;
        private static readonly object SynObject = new object();

        private XcxApiBLL()
        {

        }

        public static XcxApiBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new XcxApiBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        #region url
        /// <summary>
        /// 获取授权路径
        /// </summary>
        /// <param name="areacode"></param>
        /// <returns></returns>
        public string getOpenImg(int areacode, string returnurl)
        {
            return $"{OpenUrl}getOpenImg?areacode={areacode}&returnurl={returnurl}";
        }
        /// <summary>
        /// 上传并提交审核代码
        /// </summary>
        /// <param name="username">商户小程序原始Id</param>
        /// <param name="appsr">商户小程序秘钥</param>
        /// <param name="wtitle">小程序首页显示标题</param>
        /// <param name="mc_id">商户号</param>
        /// <param name="mc_key">商户秘钥</param>
        /// <param name="tid">小程序模板id</param>
        /// <param name="isnewcode">是否重新上传</param>
        /// <returns></returns>
        public string Commit(string username, string appsr, string wtitle, string mc_id, string mc_key, int tid, int isnewcode = 0)
        {
            return $"{OpenUrl}Commit?username={username}&appsr={appsr}&wtitle={wtitle}&mc_id={mc_id}&mc_key={mc_key}&tid={tid}";
        }

        /// <summary>
        /// 上传并提交审核代码
        /// </summary>
        /// <param name="username">商户小程序原始Id</param>
        /// <param name="data">参数</param>
        /// <returns></returns>
        public string Commit(string username, object data)
        {
            var datajson = JsonConvert.SerializeObject(data);
            return $"{OpenUrl}Commit?username={username}&datajson={datajson}";
        }
        /// <summary>
        /// 上传代码
        /// </summary>
        /// <param name="username">商户小程序原始Id</param>
        /// <param name="data">参数</param>
        /// <returns></returns>
        public string CommitCode(string username, object data)
        {
            var datajson = JsonConvert.SerializeObject(data);
            return $"{OpenUrl}CommitCode?username={username}&datajson={datajson}";
        }

        /// <summary>
        /// 发布代码
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string ReleaseCode(string username)
        {
            return $"{OpenUrl}ReleaseCode?username={username}";
        }

        /// <summary>
        /// 获取体验二维码
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string GetQrcode(string username)
        {
            return $"{OpenUrl}getqrcode?username={username}";
        }
        
        /// <summary>
        /// 获取用户acesstoken
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string GetOpenAuthodModel(string username)
        {
            return $"{OpenUrl}GetOpenAuthodModel?username={username}";
        }
        /// <summary>
        /// 获取体验者列表
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string Memberauth(string username)
        {
            return $"{OpenUrl}GetMemberAuth?username={username}";
        }
        /// <summary>
        /// 设置体验者
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string Settester(string username, string tester)
        {
            return $"{OpenUrl}Settester?username={username}&tester={tester}";
        }
        /// <summary>
        /// 解除设置体验者
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string UnSettester(string username, string tester)
        {
            return $"{OpenUrl}UnSettester?username={username}&tester={tester}";
        }
        /// <summary>
        /// 设置服务器域名管理
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string UpdateServerHost(string username, object data)
        {
            var datajson = JsonConvert.SerializeObject(data);
            return $"{OpenUrl}UpdateServerHost?username={username}&datajson={datajson}";
        }
        /// <summary>
        /// 设置服务器业务域名管理
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <returns></returns>
        public string UpdateWebviewDomain(string username, string action, string datajson)
        {
            return $"{OpenUrl}UpdateWebviewDomain?username={username}&action={action}&datajson={datajson}";
        }
        /// <summary>
        /// code 换取 session_key
        /// </summary>
        /// <param name="appid">小程序appid</param>
        /// <param name="js_code">登录时获取的 code</param>
        /// <returns></returns>
        public string GetSessionKey(string appid, string js_code)
        {
            return $"{OpenUrl}GetSessionKey?appid={appid}&js_code={js_code}";
        }

        /// <summary>
        /// 获取小程序服务类目
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string GetCategoryTyleListUrl(string username)
        {
            return $"{OpenUrl}GetCategoryTyleList?username={username}";
        }

        /// <summary>
        /// 撤回审核，单个帐号每天审核撤回次数最多不超过1次，一个月不超过10次。
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string UndocodeAuditUrl(string username)
        {
            return $"{OpenUrl}UndocodeAudit?username={username}";
        }

        /// <summary>
        /// code 换取 session_key
        /// </summary>
        /// <param name="appid">小程序appid</param>
        /// <param name="js_code">登录时获取的 code</param>
        /// <returns></returns>
        public string Jscode2sessionUrl(string appid, string js_code, string secret, string granttype = "authorization_code")
        {
            return $"https://api.weixin.qq.com/sns/jscode2session?appid={appid}&secret={secret}&js_code={js_code}&grant_type={granttype}";
        }

        /// <summary>
        /// 概况趋势
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public string GetDailySumMaryTrend(string access_token)
        {
            return $"{_xcxapihosturl}/datacube/getweanalysisappiddailysummarytrend?access_token={access_token }";
        }

        /// <summary>
        /// 访问趋势(日趋势)
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public string GetDailyVisitTrend(string access_token)
        {
            return $"{_xcxapihosturl}/datacube/getweanalysisappiddailyvisittrend?access_token={access_token }";
        }
        /// <summary>
        /// 访问趋势(周趋势)
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public string GetWeeklyVisitTrend(string access_token)
        {
            return $"{_xcxapihosturl}/datacube/getweanalysisappidweeklyvisittrend?access_token={access_token}";
        }
        /// <summary>
        /// 访问趋势(月趋势)
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public string GetMonthlyVisitTrend(string access_token)
        {
            return $"{_xcxapihosturl}/datacube/getweanalysisappidmonthlyvisittrend?access_token={access_token}";
        }

        /// <summary>
        /// 获取公众号token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string GetGongZhonTokenUrl(string appId, string secret)
        {
            return $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={secret}";
        }
        #endregion

        #region 方法
        /// <summary>
        /// 获取小程序统计数据
        /// </summary>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public Return_Msg GetDataInfo<T>(string url, DateTime startTime, DateTime endTime)
        {
            Return_Msg msg = new Return_Msg();

            //获取概括起始日期，只能获取一天内的数据 
            string datestr = startTime.ToString("yyyy-MM-dd");
            string endtimestr = endTime.ToString("yyyy-MM-dd");
            //end_date允许设置的最大值为昨日
            object postData = new { begin_date = datestr, end_date = endtimestr + " 23:59:59" };
            string dataJson = JsonConvert.SerializeObject(postData);
            string result = HttpHelper.DoPostJson(url, dataJson);
            //log4net.LogHelper.WriteInfo(this.GetType(), result + "   url:"+ url + datestr+ endtimestr);
            T gkresult = JsonConvert.DeserializeObject<T>(result);

            msg.isok = true;
            msg.dataObj = gkresult;
            return msg;
        }

        /// <summary>
        /// 不通过第三方授权平台获取小程序token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public GongZhongToken GetGongZhongToken(string appId)
        {
            string key = string.Format(_redis_MiniappGongZhongToken, appId);
            GongZhongToken model = RedisUtil.Get<GongZhongToken>(key);
            if (model == null)
                model = new GongZhongToken();

            if (string.IsNullOrEmpty(appId))
            {
                model.errmsg = "公众号token：appid不能为空";
                model.errcode = "-1";
                return model;
            }
            
            UserXcxTemplate userxcxModel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(appId);
            if (userxcxModel == null)
            {
                model.errmsg = "公众号token:上传记录为空";
                model.errcode = "-1";
                return model;
            }
            if (string.IsNullOrEmpty(userxcxModel.Appsr))
            {
                model.errmsg = "公众号token:小程序密码为空";
                model.errcode = "-1";
                return model;
            }

            //token为空或者超过100分钟，重新获取token
            if (string.IsNullOrEmpty(model.access_token) || model.UpdateTime < DateTime.Now.AddMinutes(100))
            {
                lock (_lockObj)
                {
                    model = RedisUtil.Get<GongZhongToken>(key);
                    if (model == null)
                        model = new GongZhongToken();

                    if (string.IsNullOrEmpty(model.access_token) || model.UpdateTime < DateTime.Now.AddMinutes(100))
                    {
                        string url = GetGongZhonTokenUrl(userxcxModel.AppId, userxcxModel.Appsr);
                        string resultStr = HttpHelper.GetData(url);
                        if (string.IsNullOrEmpty(resultStr))
                        {
                            model.errmsg = "公众号token：获取公众号token失败";
                            model.errcode = "-1";
                            return model;
                        }

                        GongZhongToken resultData = JsonConvert.DeserializeObject<GongZhongToken>(resultStr);
                        if (resultData == null)
                        {
                            model.errmsg = "公众号token:获取token为null";
                            model.errcode = "-1";
                            return model;
                        }
                        if (resultData.errcode == "0")
                        {
                            resultData.AppId = userxcxModel.AppId;
                            resultData.AddTime = model.AddTime;
                            resultData.UpdateTime = DateTime.Now;
                            RedisUtil.Set<GongZhongToken>(key, resultData);
                            model = resultData;
                        }
                        else
                        {
                            log4net.LogHelper.WriteInfo(this.GetType(), "公众号token:" + JsonConvert.SerializeObject(resultData));
                            model.errmsg = "公众号token:获取token失败";
                            model.errcode = "-1";
                            return model;
                        }
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 第三方授权获取小程序token
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public GetAccessTokenMsg GetThirthPlatToken(string appId)
        {
            GetAccessTokenMsg model = new GetAccessTokenMsg();
            model.obj = new AccessTokenModel();

            OpenAuthorizerConfig openConfig = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(appId);
            if (openConfig == null)
            {
                model.msg = "第三方授权：模板还没授权";
                model.isok = -1;
                return model;
            }

            string url = GetOpenAuthodModel(openConfig.user_name);
            string resultStr = HttpHelper.GetData(url);
            if (string.IsNullOrEmpty(resultStr))
            {
                model.isok = -1;
                model.msg = "第三方授权：获取token为空";
                return model;
            }
            model = JsonConvert.DeserializeObject<GetAccessTokenMsg>(resultStr);
            if (model == null || model.obj == null)
            {
                model.isok = -1;
                model.msg = "第三方授权：获取token失败";
                return model;
            }

            if (model.obj.errcode != 0 || string.IsNullOrEmpty(model.obj.access_token))
            {
                model.isok = -1;
                model.msg = $"第三方授权：{model.obj.errmsg}";
                return model;
            }

            return model;
        }

        public bool GetToken2(string appId, int authoType, ref string token)
        {
            //兼容第三方，先获取第三方token,如果获取失败，则判断是否为个人授权
            //个人授权则通过小程序密钥获取token
            GetAccessTokenMsg tModel = GetThirthPlatToken(appId);

            if (tModel != null && tModel.obj != null && tModel.isok != -1)
            {
                token = tModel.obj.access_token;
            }
            else
            {
                if (authoType == 0)
                {
                    GongZhongToken gModel = GetGongZhongToken(appId);
                    if (gModel != null && gModel.errcode == "0")
                    {
                        token = gModel.access_token;
                    }
                    else
                    {
                        token = gModel?.errmsg;
                        return false;
                    }
                }
                else
                {
                    token = tModel?.msg;
                    return false;
                }
            }
            return true;
        }
        public bool GetToken3(string appId, int authoType, ref string token)
        {
            //兼容第三方，先获取第三方token,如果获取失败，则判断是否为个人授权
            //个人授权则通过小程序密钥获取token
            GongZhongToken gModel = GetGongZhongToken(appId);
            if (gModel != null && gModel.errcode == "0")
            {
                token = gModel.access_token;
            }
            else
            {
                GetAccessTokenMsg tModel = GetThirthPlatToken(appId);
                if (tModel != null && tModel.obj != null && tModel.isok != -1)
                {
                    token = tModel.obj.access_token;
                }
                else
                {
                    token = tModel?.msg;
                    return false;
                }
            }

            return true;
        }
        public bool GetToken(XcxAppAccountRelation xcxrelation, ref string token)
        {
            //兼容第三方，先获取第三方token,如果获取失败，则判断是否为个人授权
            //个人授权则通过小程序密钥获取token
            switch (xcxrelation.AuthoAppType)
            {
                case 0:
                    GongZhongToken gModel = GetGongZhongToken(xcxrelation.AppId);
                    if (gModel != null && gModel.errcode == "0")
                    {
                        token = gModel.access_token;
                    }
                    else
                    {
                        token = gModel?.errmsg;
                        return false;
                    }
                    break;
                case 1:
                    _openType = xcxrelation.ThirdOpenType;
                    GetAccessTokenMsg tModel = GetThirthPlatToken(xcxrelation.AppId);
                    if (tModel != null && tModel.obj != null && tModel.isok != -1)
                    {
                        token = tModel.obj.access_token;
                    }
                    else
                    {
                        token = tModel?.msg;
                        return false;
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// 获取小程序sessionkey
        /// </summary>
        /// <param name="type">0:通过第三方平台，1：通过个人</param>
        /// <param name="js_code"></param>
        /// <param name="appid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public UserSession GetAppSessionInfo(int type,string js_code, string appid, ref string msg)
        {

            UserSession session = new UserSession();
            if (string.IsNullOrEmpty(appid))
            {
                msg = "appid不能为空";
                return session;
            }
            if (string.IsNullOrEmpty(js_code))
            {
                msg = "code不能为空";
                return session;
            }

            if(type==0)
            {
                return GetAppSessionInfoByThirdPlat(js_code,appid,ref msg);
            }
            else
            {
                return GetAppSessionInfoByAppsr(js_code, appid, ref msg);
            }
        }

        /// <summary>
        /// 小程序登陆通过个人获取sessionkey
        /// </summary>
        /// <param name="js_code"></param>
        /// <param name="appid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public UserSession GetAppSessionInfoByAppsr(string js_code, string appid,ref string msg)
        {
            UserSession session = new UserSession();
            UserXcxTemplate userXcxModel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(appid);
            if (userXcxModel == null || string.IsNullOrEmpty(userXcxModel.Appsr))
            {
                msg = "密钥不能为空";
                return session;
            }

            string url = Jscode2sessionUrl(appid,js_code, userXcxModel.Appsr);
            string resultJson = CommonCore.HttpGet(url);
            if(string.IsNullOrEmpty(resultJson))
            {
                msg = "获取小程序sessionkey返回值为空";
                return session;
            }

            session = JsonConvert.DeserializeObject<UserSession>(resultJson);
            if(session.errcode== "40029")
            {
                msg = "不可用的code，原因是开发工具添加项目时填写的appid跟app.js里的appid不一致";
                return session;
            }
            if (session.errcode == "40125")
            {
                msg = "不可用的秘钥，原因是手动发布的小程序后台没有保存秘钥，或者秘钥被重置过不可";
                return session;
            }
            if (string.IsNullOrEmpty(session.session_key))
            {
                msg = "登陆失败，获取sessionkey失败";
            }

            return session;
        }

        /// <summary>
        /// 小程序登陆通过第三方平台获取密钥
        /// </summary>
        /// <param name="js_code"></param>
        /// <param name="appid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public UserSession GetAppSessionInfoByThirdPlat(string js_code, string appid, ref string msg)
        {
            UserSession session = new UserSession();
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (xcxrelation != null)
            {
                _openType = xcxrelation.ThirdOpenType;
            }

            string url = GetSessionKey(appid.Trim(), js_code);
            string resultJson = HttpHelper.GetData(url);
            if (string.IsNullOrEmpty(resultJson))
            {
                msg = "获取小程序sessionkey返回值为空";
                return session;
            }
            XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(resultJson);
            if (data == null || data.obj == null)
            {
                msg = "获取秘钥null" + resultJson;
                return session;
            }

            if (!string.IsNullOrEmpty(data.obj.session_key))
            {
                session.session_key = data.obj.session_key;
                session.openid = data.obj.openid;
            }
            else
            {
                msg = data.msg;
            }
            return session;
        }
        #endregion
    }
}
