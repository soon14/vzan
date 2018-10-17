//using BLL.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Reflection;
using Entity.MiniApp.User;
using Utility;
using System.Security.Cryptography;
using Entity.MiniApp.Conf;

namespace Core.MiniApp
{
    public class WxHelper
    {
        public static string HttpGet(string Url, int timeOut = 6000)
        {
            try
            {
                return HttpHelper.GetData(Url, timeOut);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(WxHelper), new Exception(ex.Message + " 请求地址出错：" + Url));
                return "";
            }
        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开    
            return true;
        }

        public static string Post(string xml, string url, bool isUseCert, PayCenterSetting setting, int timeout = 20)
        {
            // System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接
            string result = ""; //返回结果
            HttpWebRequest request = null;
            Stream reqStream = null;
            try
            {
                //设置最大连接数
                ServicePointManager.DefaultConnectionLimit = 10000;
                //设置https验证方式
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = timeout * 1000;
                //设置POST的数据类型和长度
                request.ContentType = "text/xml";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                request.ContentLength = data.Length;

                //是否使用证书
                if (isUseCert)
                {
                    //string path = HttpContext.Current.Request.PhysicalApplicationPath;
                    if (setting == null || setting.Appid == WxPayConfig.APPID)
                    {
                        X509Certificate2 cert = new X509Certificate2(WxPayConfig.SSLCERT_PATH, WxPayConfig.SSLCERT_PASSWORD);
                        request.ClientCertificates.Add(cert);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(setting.SSLCERT_PATH)) //安装路劲没有设置就用默认目录
                        {
                            int index = WxPayConfig.SSLCERT_PATH.LastIndexOf(@"\");
                            setting.SSLCERT_PATH = WxPayConfig.SSLCERT_PATH.Substring(0, index + 1) + setting.Mch_id + "\\" + WxPayConfig.SSLCERT_PATH.Substring(index + 1);
                        }
                        if (string.IsNullOrEmpty(setting.SSLCERT_PASSWORD)) //证书密码没有设置就用默认商户号
                        {
                            setting.SSLCERT_PASSWORD = setting.Mch_id;
                        }
                        X509Certificate2 cert = new X509Certificate2(setting.SSLCERT_PATH, setting.SSLCERT_PASSWORD);
                        request.ClientCertificates.Add(cert);
                    }
                }

                //往服务器写入数据
                using (reqStream = request.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    // reqStream.Close();
                }
                //获取服务端返回
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                //获取服务端返回数据
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = sr.ReadToEnd().Trim();

                    //sr.Close();
                }
            }
            catch (System.Threading.ThreadAbortException e)
            {
                log4net.LogHelper.WriteError(typeof(System.Threading.ThreadAbortException), e);
                System.Threading.Thread.ResetAbort();
            }
            catch (WebException e)
            {
                log4net.LogHelper.WriteError(typeof(WebException), e);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    log4net.LogHelper.WriteInfo(typeof(WebException), "StatusCode : " + ((HttpWebResponse)e.Response).StatusCode.ToString());
                    log4net.LogHelper.WriteInfo(typeof(WebException), "StatusDescription : " + ((HttpWebResponse)e.Response).StatusCode.ToString());
                }
                throw new WxPayException(e.ToString());
            }
            catch (Exception e)
            {
                log4net.LogHelper.WriteError(typeof(Exception), e);
                throw new WxPayException(e.ToString());
            }
            finally
            {

                if (request != null)
                {
                    request.Abort();
                }
            }
            return result;
        }

        /// <summary>
        /// 微赞，接入了转发器的，直接返回 VZAN的server Id
        /// </summary>
        /// <returns></returns>
        public static string GetToken(int type = 0)
        {
            string token = WebSiteConfig.JsSDKSerId;
            switch (type)
            {
                case 0://小未科技公众号
                    token = WebSiteConfig.JsSDKSerId;
                    //if (!WebConfigBLL.UseTransponder)
                    //{
                    //    token = GetToken(WebSiteConfig.JsSDKAppId, WebSiteConfig.JsSDKAppSecret, false);
                    //}
                    break;
                case 1://小未公司公众号
                    token = WebSiteConfig.XWGS_JsSDKSerId;
                    //if (!WebConfigBLL.UseTransponder)
                    //{
                    //    token = GetToken(WebSiteConfig.JsSDKAppId, WebSiteConfig.JsSDKAppSecret, false);
                    //}
                    break;
            }


            return token;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="AppID"></param>
        /// <param name="AppSecret"></param>
        /// <param name="Reflesh">是否更新缓存</param>
        /// <returns></returns>
        public static string GetToken(string AppID, string AppSecret, bool Reflesh)
        {
            string key = "access_token_" + AppID;
            WXAccessToken wxToken = null;
            if (!Reflesh)
            {
                wxToken = RedisUtil.Get<WXAccessToken>(key);
                if (wxToken != null && !string.IsNullOrEmpty(wxToken.access_token))
                {
                    return wxToken.access_token;
                }
            }
            string req_url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={AppID}&secret={AppSecret}";
            string result = HttpHelper.GetData(req_url, 5000);
            if (string.IsNullOrEmpty(result))
            {
                return string.Empty;
            }
            wxToken = JsonConvert.DeserializeObject<WXAccessToken>(result);
            if (wxToken.errcode == 0 && !string.IsNullOrEmpty(wxToken.access_token))
            {
                RedisUtil.Set(key, wxToken, TimeSpan.FromMinutes(60));
                return wxToken.access_token;
            }
            else
            {
                log4net.LogHelper.WriteInfo(typeof(WxHelper), "GetToken失败result: " + result);
                return string.Empty;
            }
        }

        public static string CreateQrCodeResult(string accessToken, int expireSeconds, object sceneId)
        {
            //var urlFormat = string.Format("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}", accessToken);
            string urlFormat = string.Format(WxSysConfig.QrCode_Url(accessToken), accessToken);
            //log4net.LogHelper.WriteInfo(typeof(WxSysConfig), urlFormat);

            object data = null;
            if (expireSeconds > 0)
            {
                data = new
                {
                    expire_seconds = expireSeconds,
                    action_name = "QR_SCENE",
                    action_info = new
                    {
                        scene = new
                        {
                            scene_id = sceneId
                        }
                    }
                };
            }
            else
            {
                data = new
                {
                    action_name = "QR_LIMIT_SCENE",
                    action_info = new
                    {
                        scene = new
                        {
                            scene_id = sceneId
                        }
                    }
                };
            }

            JavaScriptSerializer jss = new JavaScriptSerializer();
            string json = jss.Serialize(data);
            return DoPostJson(urlFormat, json);
        }


        /// <summary>
        ///  构造模拟远程HTTP的POST请求，POST数据Json字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static string DoPostJson(string url, string jsonData)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=utf-8";
            using (Stream dataStream = request.GetRequestStream())
            {
                byte[] paramBytes = ASCIIEncoding.UTF8.GetBytes(jsonData);
                dataStream.Write(paramBytes, 0, paramBytes.Length);
                dataStream.Close();
            }
            HttpWebResponse res;
            try
            {
                res = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse)ex.Response;
            }
            StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// 公众号获取微信用户基本信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static WeiXinUser GetWxUserInfo(string token, string openid)
        {
            WeiXinUser model = new WeiXinUser();
            //string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", token, openid);
            string url = string.Format(WxSysConfig.User_infoURL(token), token, openid);

            string jsonstr = HttpGet(url);
            try
            {
                model = new JavaScriptSerializer().Deserialize<WeiXinUser>(jsonstr);
                model.serverid = token;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(typeof(WxHelper), "GetWxUserInfo出错:" + url + "===" + ex.Message);
                return null;
            }
            return model;
        }


        /// <summary>
        /// Post提交数据 返回str
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller">控制器</param>
        /// <param name="action">方法名</param>
        /// <param name="reqParameter">参数</param>
        /// <returns></returns>
        public static string PostWebApi(string url, Dictionary<string, string> reqParameter)
        {
            string result = string.Empty;
            WebClient webClent = new WebClient();
            try
            {
                webClent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                webClent.Encoding = Encoding.UTF8;
                string strJson = string.Empty;
                if (reqParameter != null && reqParameter.Count > 0)
                {
                    foreach (KeyValuePair<string, string> keys in reqParameter)
                    {
                        strJson += "&" + keys.Key + "=" + keys.Value;
                    }
                    strJson = strJson.TrimStart('&');
                }
                result = webClent.UploadString(url, "POST", strJson);
            }
            catch
            {
            }
            finally
            {
                webClent.Dispose();
            }
            return result;
        }
        

        public static string AESDecrypt(string encryptedData, string AesKey, string AesIV)
        {
            try
            {

                //判断是否是16位 如果不够补0
                //text = tests(text);
                //16进制数据转换成byte
                byte[] encryptedData_ = Convert.FromBase64String(encryptedData);  // strToToHexByte(text);
                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Key = Convert.FromBase64String(AesKey); // Encoding.UTF8.GetBytes(AesKey);
                rijndaelCipher.IV = Convert.FromBase64String(AesIV);// Encoding.UTF8.GetBytes(AesIV);
                rijndaelCipher.Mode = CipherMode.CBC;
                rijndaelCipher.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
                byte[] plainText = transform.TransformFinalBlock(encryptedData_, 0, encryptedData.Length);
                string result = Encoding.Default.GetString(plainText);
                //int index = result.LastIndexOf('>');
                //result = result.Remove(index + 1);
                return result;
            }
            catch (Exception ex)
            {
                return null;

            }
        }


        /// <summary>
        /// 通过调用该接口查询卡券开放的类目ID，类目会随业务发展变更，请每次用接口去查询获取实时卡券类目。
        /// </summary>
        /// <param name="ak"></param>
        /// <param name="AppID">点赞小程序后台对应的Appid也就是权限Id</param>
        /// <returns></returns>
        public static CategoryResult GetMiniAppCategory(string ak, int AppID)
        {
            string key = "MiniAppCategory" + AppID;
            CategoryResult _CategoryResult = RedisUtil.Get<CategoryResult>(key);
            if (_CategoryResult == null)
            {
                string jsonStr = WxHelper.HttpGet($"https://api.weixin.qq.com/card/getapplyprotocol?access_token={ak}");
                _CategoryResult = JsonConvert.DeserializeObject<CategoryResult>(jsonStr);
                if (_CategoryResult.errcode == 0 && _CategoryResult.errmsg == "ok")
                {
                    RedisUtil.Set(key, _CategoryResult, TimeSpan.FromDays(30));
                }
            }


            return _CategoryResult;
        }


        /// <summary>
        /// 时间转时间戳10位
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static long TimeToUnix(DateTime t)
        {

            TimeSpan ts = t - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long timeStamp = Convert.ToInt64(ts.TotalSeconds);     //获取时间戳
            return timeStamp;
        }

        /// <summary>
        /// 10位数时间戳转时间字符串
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixToTime(long unixTimeStamp)
        {
            DateTime dtStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            long lTime = long.Parse(unixTimeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
    }
}