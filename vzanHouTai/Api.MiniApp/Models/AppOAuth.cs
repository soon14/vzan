using Core.MiniApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Net;
using Entity.MiniApp;
using Entity.MiniApp.User;
using BLL.MiniApp;
using DAL.Base;
using Entity.MiniApp.Pin;
using BLL.MiniApp.Pin;
using Entity.MiniApp.Weixin;
using Utility;

namespace Api.MiniApp.Models
{
    //errcode:500xxx
    /// <summary>
    /// APP 接口验证
    /// </summary>
    public class AppOAuth : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string uid = Utility.IO.Context.GetRequest("uid", string.Empty);
            string versionCode = Utility.IO.Context.GetRequest("versionCode", string.Empty);
            string deviceType = Utility.IO.Context.GetRequest("deviceType", string.Empty);
            string timestamp = Utility.IO.Context.GetRequest("timestamp", string.Empty);
            string sign = Utility.IO.Context.GetRequest("sign", string.Empty);

            VerifyModel vmodel = new VerifyModel()
            {
                uid = uid,
                versionCode = versionCode,
                deviceType = deviceType,
                timestamp = timestamp,
                sign = sign
            };

            if (!vmodel.verify())
            {
                string[] keys = System.Web.HttpContext.Current.Request.QueryString.AllKeys;
                string values = string.Empty;
                if (keys.Length > 0)
                {
                    foreach (var item in keys)
                    {
                        values += string.Format("{0}={1}&", item, System.Web.HttpContext.Current.Request.QueryString[item]);
                    }
                }
                //string p="str=" + vmodel.CombinParameters() + ",md5=" + vmodel.MD5(vmodel.CombinParameters()) + ",des=" + vmodel.Encrypt(vmodel.MD5(vmodel.CombinParameters())) + ",keys=" + string.Join(",", keys) + ",values=" + values;
                filterContext.Result = new JsonResult() { Data = new { isok = false, code = "-1", Msg = "非法请求" } };
            }
        }
    }
    /// <summary>
    /// 论坛 APP 接口验证
    /// </summary>
    public class MinisnsAppOAuth : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string uid = Utility.IO.Context.GetRequest("uid", string.Empty);
            string versionCode = Utility.IO.Context.GetRequest("versionCode", string.Empty);
            string deviceType = Utility.IO.Context.GetRequest("deviceType", string.Empty);
            string timestamp = Utility.IO.Context.GetRequest("timestamp", string.Empty);
            string sign = Utility.IO.Context.GetRequest("sign", string.Empty);

            VerifyModel vmodel = new VerifyModel()
            {
                uid = uid,
                versionCode = versionCode,
                deviceType = deviceType,
                timestamp = timestamp,
                sign = sign
            };

            if (!vmodel.verify())
            {
                BaseResult res = new BaseResult();
                res.result = false;
                res.msg = "参数无效";
                res.errcode = 50082;
                ContentResult Respone_result = new ContentResult();
                Respone_result.Content = JsonConvert.SerializeObject(res);
                filterContext.Result = Respone_result;
            }
        }
    }

    /// <summary>
    /// 签名验证
    /// </summary>
    public class VerifyModel
    {
        /// <summary>
        /// 微信unionid
        /// </summary>
        public string uid { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public string versionCode { get; set; } = string.Empty;

        /// <summary>
        /// 设备类型 android=1; ios=2
        /// </summary>
        public string deviceType { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳 
        /// </summary>
        public string timestamp { get; set; } = string.Empty;

        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; } = string.Empty;


        /// <summary>
        /// KEY
        /// </summary>
        private string sKey = "vzanlive";

        /// <summary>
        /// 
        /// </summary>
        private string pv = "32526978";

        /// <summary>
        /// 验证签名
        /// </summary>
        /// <returns></returns>
        public bool verify()
        {
            if (string.IsNullOrEmpty(versionCode) || string.IsNullOrEmpty(deviceType) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(sign))
                return false;

            return ComputeSign() == sign;
        }

        /// <summary>
        /// 拼接参数
        /// </summary>
        /// <returns></returns>
        public string CombinParameters()
        {
            string[] parameters = new string[] { "uid=" + uid, "versionCode=" + versionCode, "deviceType=" + deviceType, "timestamp=" + timestamp };
            Array.Sort(parameters);
            return string.Join("&", parameters.ToArray());
        }

        /// <summary>
        /// 计算签名
        /// </summary>
        /// <returns></returns>
        public string ComputeSign()
        {
            return Encrypt(MD5(CombinParameters()));
        }

        /// <summary>
        /// MD5 加密
        /// </summary>
        /// <param name="inputstr"></param>
        /// <returns></returns>
        public string MD5(string inputstr)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            //加密Byte[]数组   
            byte[] data = System.Text.Encoding.UTF8.GetBytes(inputstr);
            byte[] result = md5.ComputeHash(data);
            //将加密后的数组转化为字段   
            return BitConverter.ToString(result).Replace("-", "");
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="pToEncrypt"></param>
        /// <returns></returns>
        public string Encrypt(string pToEncrypt)
        {
            if (string.IsNullOrEmpty(pToEncrypt))
                return string.Empty;

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            //把字符串放到byte数组中
            //原来使用的UTF8编码，我改成Unicode编码了，不行
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);

            //建立加密对象的密钥和偏移量
            //使得输入密码必须输入英文文本
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(pv);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return BitConverter.ToString(ms.ToArray()).Replace("-", "");
        }
    }

    public class ApiOAuthParameter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string unionId = Utility.IO.Context.GetRequest("unionid", string.Empty);

            if (string.IsNullOrWhiteSpace(unionId))
            {
                filterContext.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new BaseResult() { result = false, msg = "unionid空值", errcode = -1 })
                };
            }
        }
    }

    /// <summary>
    /// 验证小程序appid
    /// </summary>
    public class ApiOAuthAppId : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string appId = Utility.IO.Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                filterContext.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new BaseResult() { result = false, msg = "appId参数错误", errcode = -1 })
                };

            }

            XcxAppAccountRelation r = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appId);
            if (r == null)
            {
                filterContext.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject(new BaseResult() { result = false, msg = "小程序未授权", errcode = -1 })
                };
            }
        }
    }

    public class ApiOAuthLogin : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string unionid = Utility.IO.Context.GetRequest("unionid", string.Empty);
            //if (new CheckUserSession().IsLogin(unionid)) { return; }
            if (string.IsNullOrWhiteSpace(unionid))
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new BaseResult()
                    {
                        result = false,
                        msg = "参数：unionid，为空",
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }
            //MySqlParameter[] userPara = { new MySqlParameter("@unionid", unionid), new MySqlParameter("@userStatus", ((int)Entity.MiniSNS.VZANCity.C_Enums.UserState.正常).ToString()) };
            //var IsLogin = new C_UserRoleBLL().GetCount($"unionid=@unionid and UserState=@userStatus", userPara) > 0 ? true : false;
            MySqlParameter[] userPara = { new MySqlParameter("@unionid", unionid) };
            var LoginUser = C_UserInfoBLL.SingleModel.GetModelFromCacheByUnionid(unionid);
            filterContext.Controller.ViewData.Add("LoginData", LoginUser);
            if (LoginUser == null)
            {
                filterContext.Result = new JsonResult()
                {
                    Data = new BaseResult()
                    {
                        result = false,
                        msg = "登陆信息异常"
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                base.OnActionExecuting(filterContext);
                return;
            }
        }
    }

    public class DecryptUserInfo
    {
        public string GetApiJsonStringnew(string js_code, string appid = "", string appsr = "")
        {
            ApiParameter.js_code = js_code;
            ApiParameter.appId = appid;
            if (string.IsNullOrEmpty(appsr))
            {
                if (!string.IsNullOrEmpty(appid))
                {
                    var umodel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(appid);
                    if (umodel != null && !string.IsNullOrEmpty(umodel.Appsr))
                    {
                        ApiParameter.secret = umodel.Appsr;
                    }
                }
                if (string.IsNullOrEmpty(ApiParameter.secret))
                {
                    ApiParameter.secret = appsr;
                }
            }
            else
            {
                ApiParameter.secret = appsr;
            }
            string StrJson = ApiParameter.GetJson();
            return StrJson;
        }
        public SeccessModel GetApiJsonStringnoappsr(string js_code, string appid)
        {
            JsonSerializerSettings setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            SeccessModel resultmodel = new SeccessModel();
            UserXcxTemplate umodel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(appid);
            if (umodel != null)
            {
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
                if(xcxrelation!=null)
                {
                    XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                }
                
                string url = XcxApiBLL.SingleModel.GetSessionKey(appid.Trim(), js_code);
                string result = HttpHelper.GetData(url);
                if (!string.IsNullOrEmpty(result))
                {
                    XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result, setting);
                    if (data != null && data.obj != null)
                    {
                        if (!string.IsNullOrEmpty(data.obj.session_key))
                        {
                            resultmodel.session_key = data.obj.session_key;
                            resultmodel.openid = data.obj.openid;
                            resultmodel.isok = 1;
                            resultmodel.msg = "OK";
                        }
                        else
                        {
                            resultmodel.isok = -1;
                            resultmodel.msg = data.msg;
                        }
                    }
                    else
                    {
                        resultmodel.isok = -1;
                        resultmodel.msg = "获取秘钥null" + result;
                    }
                }
                else
                {
                    resultmodel.isok = -1;
                    resultmodel.msg = result;
                }
            }
            else
            {
                resultmodel.isok = -1;
                resultmodel.msg = "没有上传记录";
            }

            return resultmodel;
        }

        public UserSession GetJscode2session(string js_code, string appid, string secret)
        {
            UserSession resultmodel = new UserSession();
            string url = XcxApiBLL.SingleModel.Jscode2sessionUrl(appid.Trim(), js_code, secret);

            string result = HttpHelper.GetData(url);
            if (!string.IsNullOrEmpty(result))
            {
                resultmodel = JsonConvert.DeserializeObject<UserSession>(result);
            }
            return resultmodel;
        }
    }


    public static class CheckLoginClass
    {
        private static readonly string _redis_miniappLoginSessionKey = "miniappLoginSessionKey_{0}";
        public static readonly string _redis_loginSessionOpenIdKey = "LoginSessionOpenIdKey_{0}";
        /// <summary>
        /// 用户登录/注册
        /// </summary>
        /// <param name="code">微信授权Code</param>
        /// <param name="iv">初始向量</param>
        /// <param name="data">加密数据</param>
        /// <param name="signature">加密签名</param>
        /// <returns>微信用户数据(Json)</returns>
        public static BaseResult CheckUserLoginNoappsr(int storeId, string code, string iv, string data, string appid, string signature, int isphonedata = 0, int needappsr = 0)
        {
            BaseResult result = new BaseResult();
            try
            {
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(iv) || string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(appid) )
                {
                    result.result = false;
                    result.msg = "参数缺省";
                    result.errcode = -1;
                    return result;
                }

                UserSession UserSession = new UserSession();
                UserSession.code = code;
                UserSession.vector = iv;
                UserSession.enData = data;

                JsonSerializerSettings setting = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                //判断是否用秘钥解密还是不需要
                if (needappsr == 0)
                {
                    //微信授权Code，调用接口获得session_key
                    SeccessModel JsonResult = new DecryptUserInfo().GetApiJsonStringnoappsr(code, appid);

                    if (JsonResult.isok < 0)
                    {
                        result.result = false;
                        result.msg = JsonResult.msg;
                        result.errcode = -1;
                        return result;
                    }
                    else
                    {
                        UserSession.session_key = JsonResult.session_key;
                    }
                }
                else
                {
                    //微信授权Code，调用接口获得session_key
                    string JsonResult = new DecryptUserInfo().GetApiJsonStringnew(code, appid);

                    UserSession sessionkey = JsonConvert.DeserializeObject<UserSession>(JsonResult, setting);
                    if (sessionkey == null || string.IsNullOrEmpty(sessionkey.session_key))
                    {
                        result.result = false;
                        result.msg = "登陆失败，获取秘钥失败";
                        result.errcode = -1;
                        result.obj = JsonResult;
                        return result;
                    }
                    UserSession.session_key = sessionkey.session_key;
                }


                //AES解密，委托参数session_key和初始向量
                UserSession.deData = AESDecrypt.Decrypt(UserSession.enData, UserSession.session_key, UserSession.vector);
                if(string.IsNullOrEmpty(UserSession.deData))
                {
                    result.result = false;
                    result.msg = "服务超时，请刷新重试";
                    result.errcode = -1;
                    return result;
                }
                C_ApiUserInfo userInfo = JsonConvert.DeserializeObject<C_ApiUserInfo>(UserSession.deData, setting);
                
                //保存用户会话
                //var SessionId = AESDecrypt.MD5(UserSession.session_key + UserInfo.unionId);
                C_UserInfo userinfopost = new C_UserInfo();
                //是否是用户手机数据
                if (isphonedata > 0)
                {
                    userinfopost = C_UserInfoBLL.SingleModel.GetModel(Convert.ToInt32(signature));
                    if (userinfopost == null)
                    {
                        result.result = false;
                        result.msg = "您还没注册";
                        result.errcode = -1;
                        result.obj = userInfo;
                        return result;
                    }

                    //TODO 一部分用户的店铺是在PC端注册的，没有关联c_userinfo,当用户在小程序端授权手机号的时候再做关联
                    //PinStoreBLL pinStoreBLL = new PinStoreBLL();
                    //PinStore pinStore = pinStoreBLL.GetStoreByPhone(userInfo.phoneNumber);
                    //if (pinStore != null)
                    //{
                    //    pinStore.userId = userinfopost.Id;
                    //    pinStoreBLL.Update(pinStore, "userId");

                    //    userinfopost.StoreId = pinStore.id;
                    //}

                    userinfopost.TelePhone = userInfo.phoneNumber;
                    userinfopost.IsValidTelePhone = 1;
                    
                    if (!C_UserInfoBLL.SingleModel.Update(userinfopost))
                    {
                        result.result = false;
                        result.msg = "保存用户手机号失败";
                        result.errcode = -1;
                        result.obj = userInfo;
                        return result;
                    }
                }
                else
                {
                    //返回sessionId
                    userinfopost = C_UserInfoBLL.SingleModel.GetModelFromCache(userInfo.openId);
                    if (userinfopost == null)
                    {
                        userinfopost = C_UserInfoBLL.SingleModel.RegisterByXiaoChenXun(new C_UserInfo() { NickName = userInfo.nickName, HeadImgUrl = userInfo.avatarUrl, UnionId = userInfo.unionId, appId = appid, OpenId = userInfo.openId, StoreId = storeId, Sex = int.Parse(userInfo.gender) ,Address = userInfo.country+"\\"+userInfo.province+"\\"+userInfo.city});
                    }
                    //else
                    //{
                    //    userinfopost.HeadImgUrl = string.IsNullOrEmpty(userInfo.avatarUrl) ? userinfopost.HeadImgUrl : userInfo.avatarUrl;
                    //    userinfopost.NickName = string.IsNullOrEmpty(userInfo.nickName) ? userinfopost.NickName : userInfo.nickName;
                    //    C_UserInfoBLL.SingleModel.Update(userinfopost, "HeadImgUrl,NickName");
                    //}
                }

                //获取登陆秘钥
                string loginsessionkey = GetLoginSessionKey(appid);
                if (loginsessionkey.Length == 0)
                {
                    result.result = false;
                    result.msg = "获取登陆秘钥超时";
                    result.errcode = -1;
                    return result;
                }

                //判断头像是否更改
                if (userInfo.avatarUrl != userinfopost.HeadImgUrl || userInfo.nickName != userinfopost.NickName || userInfo.unionId != userinfopost.UnionId)
                {
                    userinfopost.HeadImgUrl = string.IsNullOrEmpty(userInfo.avatarUrl) ? userinfopost.HeadImgUrl : userInfo.avatarUrl;
                    userinfopost.NickName = string.IsNullOrEmpty(userInfo.nickName) ? userinfopost.NickName : userInfo.nickName;
                    userinfopost.UnionId = string.IsNullOrEmpty(userInfo.unionId) ? userinfopost.UnionId : userInfo.unionId;
                    C_UserInfoBLL.SingleModel.Update(userinfopost, "HeadImgUrl,NickName,UnionId");
                }

                userInfo.userid = userinfopost.Id;
                userInfo.nickName = userinfopost.NickName;
                userInfo.avatarUrl = userinfopost.HeadImgUrl;
                userInfo.gender = userinfopost.Sex.ToString();
                userInfo.tel = userinfopost.TelePhone;
                userInfo.IsValidTelePhone = userinfopost.IsValidTelePhone;
                userInfo.openId = userinfopost.OpenId;
                userInfo.loginSessionKey = loginsessionkey;

                result.result = true;
                result.msg = "解密完成";
                result.errcode = -1;
                result.obj = userInfo;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.msg = "请求超时，请刷新重试";
                result.obj = ex;
                result.errcode = -1;
            }

            return result;
        }
        public static BaseResult WxLogin(string code, string appid, int needappsr = 0,int storeid=0)
        {
            BaseResult result = new BaseResult();
            try
            {
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(appid))
                {
                    result.result = false;
                    result.msg = "参数缺省";
                    result.errcode = -1;
                    return result;
                }

                UserSession UserSession = new UserSession();
                UserSession.code = code;

                //判断是否用秘钥解密还是不需要
                if (needappsr == 0 )//&& appid != "wxbb2fe3080d04c9b2")
                {
                    //微信授权Code，调用接口获得session_key
                    SeccessModel JsonResult = new DecryptUserInfo().GetApiJsonStringnoappsr(code, appid);
                    if (JsonResult.isok < 0)
                    {
                        result.result = false;
                        result.msg = JsonResult.msg;
                        result.obj = JsonResult;
                        result.errcode = -1;
                        return result;
                    }
                    else
                    {
                        UserSession.session_key = JsonResult.session_key;
                        UserSession.openid = JsonResult.openid;
                    }
                }
                else
                {
                    //微信授权Code，调用接口获得session_key
                    string JsonResult = new DecryptUserInfo().GetApiJsonStringnew(code, appid);
                    UserSession session = JsonConvert.DeserializeObject<UserSession>(JsonResult);
                    if (session == null || string.IsNullOrEmpty(session.session_key))
                    {
                        result.result = false;
                        result.msg = "登陆失败，获取秘钥失败";
                        result.errcode = -1;
                        result.obj = JsonResult;
                        return result;
                    }
                    UserSession.session_key = session.session_key;
                    UserSession.openid = session.openid;
                    string sessionkey = UserSession.session_key;
                    ////调用新接口获取用户Openid
                    //UserSession = new DecryptUserInfo().GetJscode2session(UserSession.code, appid, UserSession.session_key);

                }
                if (string.IsNullOrEmpty(UserSession.openid))
                {
                    result.result = false;
                    result.msg = "获取用户OpenId繁忙";
                    result.obj = UserSession;
                    result.errcode = -1;
                    return result;
                }
                
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, UserSession.openid);

                if (userInfo == null)
                {
                    userInfo = new C_UserInfo();
                    userInfo.StoreId = storeid;
                    userInfo.OpenId = UserSession.openid;
                    userInfo.UnionId = UserSession.unionid;
                    userInfo.appId = appid;
                    userInfo.Id = Convert.ToInt32(C_UserInfoBLL.SingleModel.Add(userInfo));
                }

                //获取登陆秘钥
                string loginsessionkey = GetLoginSessionKey(UserSession.session_key, userInfo.OpenId);
                if (loginsessionkey.Length == 0)
                {
                    result.result = false;
                    result.msg = "获取登陆秘钥超时";
                    result.errcode = -1;
                    return result;
                }
                userInfo.loginSessionKey = loginsessionkey;

                result.result = true;
                result.msg = "解密完成";
                result.errcode = 1;
                result.obj = userInfo;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.msg = "请求超时，请刷新重试";
                result.obj = ex;
                result.errcode = -1;
            }

            return result;
        }


        /// <summary>
        /// 获取登陆验证秘钥
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static string GetLoginSessionKey(string appid)
        {
            if (appid == null || appid.Length == 0)
            {
                return "";
            }

            //登陆验证秘钥
            string loginsessionkey = RedisUtil.Get<string>(string.Format(_redis_miniappLoginSessionKey, appid));
            if (loginsessionkey == null || loginsessionkey.Length == 0)
            {
                loginsessionkey = Guid.NewGuid().ToString();
                RedisUtil.Set<string>(string.Format(_redis_miniappLoginSessionKey, appid), loginsessionkey, TimeSpan.FromHours(12));
            }

            return loginsessionkey;
        }

        /// <summary>
        /// 获取登陆验证秘钥
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static string GetLoginSessionKey(string sessionkey, string openid)
        {
            if (sessionkey == null || sessionkey.Length == 0 || openid == null || openid.Length == 0)
            {
                return "";
            }
            UserSession usersession = new UserSession();
            usersession.session_key = sessionkey;
            usersession.openid = openid;
            //登陆验证秘钥
            string loginsessionkey = Guid.NewGuid().ToString();
            RedisUtil.Set<UserSession>(string.Format(_redis_loginSessionOpenIdKey, loginsessionkey), usersession, TimeSpan.FromHours(12));

            return loginsessionkey;
        }
    }

    public static class ApiParameter
    {
        public static string appId = "";
        public static string secret = "";
        public static List<AppSecretList> secretlist = new List<AppSecretList> {
            new AppSecretList() { appId = "wx238f1fdb91e27c56", secret = "3e0ad0785a863744da222055955a381d" },//同城优惠券
            //new AppSecretList() { appId = "wxbfa9d9b358118fa4", secret = "6c7127c583d86aa387285e11b5780273" },//同城分类信息
            new AppSecretList() { appId = "wx693acdb19992c5a5", secret = "6037603c00d584bfdd614c92bfa2753a" },//永平县
            new AppSecretList() { appId = "wxe897b65c4277aa59", secret = "50d514a957725681ca48858e4a116240" },//凌云网
            new AppSecretList() { appId = "wx03ab148eb45a0419", secret = "92fb629f856d46de4f2c0ee508dfa4bd" }//掌握太和
        };
        public static string grant_type { get { return "authorization_code"; } }
        public static string js_code { get; set; }
        public static string ApiUrl { get { return string.Format("https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type={3}", appId, secret, js_code, grant_type); } }
        public static string GetJson()
        {
            return Get(ApiUrl);
        }
        /// <summary>
        ///  获取网址HTML
        /// </summary>
        /// <param name="URL">网址 </param>
        /// <returns> </returns>
        public static string Get(string URL)
        {
            WebRequest wrt;
            wrt = WebRequest.Create(URL);
            wrt.Credentials = CredentialCache.DefaultCredentials;
            WebResponse wrp;
            wrp = wrt.GetResponse();
            string reader = new StreamReader(wrp.GetResponseStream(), Encoding.GetEncoding("gb2312")).ReadToEnd();
            try
            {
                wrt.GetResponse().Close();
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return reader;
        }
    }
    
    //public class AcceTokenModel: BaseModel
    //{
    //    public string acctoken { get; set; }
    //}
    public class BaseModel
    {
        public int isok { get; set; }
        public string msg { get; set; }
    }
    public class SeccessModel : BaseModel
    {
        public string StrJson { get; set; }
        public string session_key { get; set; }
        public string openid { get; set; }
    }

    public static class AESDecrypt
    {
        private static object _LockDecrypt = new object();
        /// <summary>
        /// 获取解密后的用户数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="iv"></param>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static C_ApiUserInfo GetUserInfo(string sessionKey,string iv,string data,ref string msg)
        {
            string decryptData = "";
            lock(_LockDecrypt)
            {
                decryptData = DecryptData(data, sessionKey, iv);
            }
            
            if (string.IsNullOrEmpty(decryptData))
            {
                msg = "解密失败，解密数据为空";
                return null;
            }
            //序列化解密数据
            C_ApiUserInfo apiUserInfo = JsonConvert.DeserializeObject<C_ApiUserInfo>(decryptData);

            return apiUserInfo;
        }

        /// <summary>
        /// 获取密钥
        /// </summary>
        //public static string Session_Key;
        //public static string vectorString;
        //默认密钥偏移向量 
        //private static byte[] vector = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        //private static byte[] vector = Convert.FromBase64String(vectorString);

        public static string Decrypt(string showText, string Session_Key, string vectorString)
        {
            byte[] vector = Convert.FromBase64String(vectorString);
            byte[] byteArray = Convert.FromBase64String(showText);
            byte[] keybitArray = Convert.FromBase64String(Session_Key);
            char[] cpara = new ASCIIEncoding().GetChars(keybitArray);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keybitArray;
            rDel.IV = vector;
            rDel.BlockSize = 128;
            rDel.Mode = CipherMode.CBC;//必须设置为ECB
            rDel.Padding = PaddingMode.PKCS7;//必须设置为PKCS7

            try
            {
                ICryptoTransform cTransform = rDel.CreateDecryptor();
                var resultBit = cTransform.TransformFinalBlock(byteArray, 0, byteArray.Length);
                var decrypt = Encoding.UTF8.GetString(resultBit).Replace("\0", "");
                rDel.Clear();
                return decrypt;
            }
            catch (Exception)
            {
                //log4net.LogHelper.WriteInfo(typeof(AppOAuth),"data: " +showText+"   key:  "+Session_Key+"  iv:  "+vectorString+";"+ex.Message);
            }

            return "";
        }
        
        /// <summary>  
        /// 根据微信小程序平台提供的解密算法解密数据  
        /// </summary>  
        /// <param name="encryptedData">加密数据</param>  
        /// <param name="iv">初始向量</param>  
        /// <param name="sessionKey">从服务端获取的SessionKey</param>  
        /// <returns></returns>  
        public static string DecryptData(string encryptedData, string sessionKey,string iv)
        {
            //创建解密器生成工具实例  
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            //设置解密器参数  
            aes.Mode = CipherMode.CBC;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;
            //格式化待处理字符串  
            byte[] byte_encryptedData = Convert.FromBase64String(encryptedData);
            byte[] byte_iv = Convert.FromBase64String(iv);
            byte[] byte_sessionKey = Convert.FromBase64String(sessionKey);

            aes.IV = byte_iv;
            aes.Key = byte_sessionKey;
            //根据设置好的数据生成解密器实例  
            ICryptoTransform transform = aes.CreateDecryptor();

            //解密  
            byte[] final = transform.TransformFinalBlock(byte_encryptedData, 0, byte_encryptedData.Length);
            
            //生成结果  
            return Encoding.UTF8.GetString(final);
        }
    }
}