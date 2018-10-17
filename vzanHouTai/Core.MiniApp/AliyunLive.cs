using Aliyun.OSS;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.MiniApp
{
    public class AliyunLive
    {
        private string BaseApi(Dictionary<string, string> param)
        {
            string url = string.Empty;
            param.Add("Format", "JSON");
            param.Add("Version", "2014-11-11");
            param.Add("AccessKeyId", "LTAI4G9R8oHXt8yf");
            param.Add("SignatureMethod", "HMAC-SHA1");
            param.Add("Timestamp", DateTime.Now.AddHours(-8).ToString("yyyy-MM-ddThh:mm:ssZ"));
            param.Add("SignatureVersion", "1.0");
            param.Add("SignatureNonce", Guid.NewGuid().ToString());
            string signature = Signature(param, out url);
            signature = percentEncode(signature);
            param.Add("Signature", signature);
            url += "&Signature=" + signature;
            return url;
        }

        /// <summary>
        /// 获取推流状态
        /// </summary>
        /// <param name="StreamName"></param>
        /// <returns></returns>
        public string DescribeLiveStreamStreamStatus(string StreamName)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", "DescribeLiveStreamStreamStatus");
            param.Add("StreamName", StreamName);
            param.Add("AppName", "live");
            param.Add("DomainName", "zhibo.vzan.cc");
            param.Add("RegionId", "cn-hangzhou");
            string api = BaseApi(param);
            try
            {
                string url = "http://cdn.aliyuncs.com?" + api;
                string res = HttpGet(url);
                Dictionary<string, string> StreamStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                if (StreamStatus != null && StreamStatus.Keys.Contains("StreamStatus"))
                {
                    return StreamStatus["StreamStatus"];
                }
                return null;
            }
            catch (Exception )
            {
                return null;
            }
        }
        /// <summary>
        /// 停止直播流
        /// </summary>
        /// <param name="StreamName"></param>
        /// <returns></returns>
        public string ForbidLiveStream(string StreamName)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", "ForbidLiveStream");
            param.Add("StreamName", StreamName);
            param.Add("AppName", "live");
            param.Add("DomainName", "zhibo.vzan.cc");
            param.Add("RegionId", "cn-hangzhou");
            param.Add("LiveStreamType", "publisher");
            string api = BaseApi(param);
            try
            {
                string url = "http://cdn.aliyuncs.com?" + api;
                string res = HttpGet(url);
                Dictionary<string, string> dicres = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                if (dicres != null && dicres.Keys.Contains("RequestId"))
                {
                    return dicres["RequestId"];
                }
                return null;
            }
            catch (Exception )
            {
                return null;
            }
        }

        /// <summary>
        /// 添加转码配置
        /// </summary>
        /// <param name="appname"></param>
        /// <returns></returns>
        public string AddLiveStreamTranscode(string appname)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", "AddLiveStreamTranscode");
            param.Add("App", appname);
            param.Add("Domain", "zhibo.vzan.cc");
            param.Add("Template", "hd");
            param.Add("Record", "yes");
            param.Add("Snapshot", "no");
            string api = BaseApi(param);
            try
            {
                string url = "http://cdn.aliyuncs.com?" + api;
                string res = HttpGet(url);
                Dictionary<string, string> dicres = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                if (dicres != null && dicres.Keys.Contains("RequestId"))
                {
                    return dicres["RequestId"];
                }
                return null;
            }
            catch (Exception )
            {
                return null;
            }
        }
        /// <summary>
        /// 删除转码设置
        /// </summary>
        /// <param name="appname"></param>
        /// <returns></returns>
        public string DeleteLiveStreamTranscode(string appname)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", "DeleteLiveStreamTranscode");
            param.Add("App", appname);
            param.Add("Domain", "zhibo.vzan.cc");
            param.Add("Template", "hd");
            string api = BaseApi(param);
            try
            {
                string url = "http://cdn.aliyuncs.com?" + api;
                string res = HttpGet(url);
                Dictionary<string, string> dicres = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                if (dicres != null && dicres.Keys.Contains("RequestId"))
                {
                    return dicres["RequestId"];
                }
                return null;
            }
            catch (Exception )
            {
                return null;
            }
        }
        /// <summary>
        /// 添加录制配置
        /// </summary>
        /// <param name="appname"></param>
        /// <returns></returns>
        public string AddLiveAppRecordConfig(string appname)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", "AddLiveAppRecordConfig");
            param.Add("AppName", appname);
            param.Add("DomainName", "zhibo.vzan.cc");
            param.Add("OssEndpoint", "oss-cn-hangzhou.aliyuncs.com");
            param.Add("OssBucket", "oss-vod");
            param.Add("OssObjectPrefix", "{AppName}/{StreamName}/{UnixTimestamp}_{Sequence}");
            string api = BaseApi(param);
            try
            {
                string url = "http://cdn.aliyuncs.com?" + api;
                string res = HttpGet(url);
                Dictionary<string, string> dicres = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                if (dicres != null && dicres.Keys.Contains("RequestId"))
                {
                    return dicres["RequestId"];
                }
                return null;
            }
            catch (Exception )
            {
                return null;
            }
        }

        public string DescribeLiveStreamsOnlineList()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Action", "DescribeLiveStreamsOnlineList");
            param.Add("AppName", "live");
            param.Add("DomainName", "zhibo.vzan.cc");
            string api = BaseApi(param);
            try
            {
                string url = "http://cdn.aliyuncs.com?" + api;
                string res = HttpGet(url);
                return res;
            }
            catch (Exception )
            {
                return null;
            }
        }


        private string Signature(Dictionary<string, string> param, out string url)
        {
            url = "http://cdn.aliyuncs.com";
            List<string> keyList = new List<string>(param.Keys);
            keyList.Sort();//排序
            string canonicalizedQueryString = "";
            for (int i = 0; i < keyList.Count; i++)
            {
                string key = keyList[i];
                canonicalizedQueryString += "&" + percentEncode(key) + "=" + percentEncode(param[key]);
            }
            //返回请求的URL
            url = canonicalizedQueryString.Substring(1);
            // 生成用于计算签名的字符串 stringToSign
            string stringToSign = "GET&%2F&" + percentEncode(canonicalizedQueryString.Substring(1));
            string access_key = "XtxUyYAcVo7IOsO79rGBLiXX3LQkFp" + "&";
            string signature = HmacSha1Sign(stringToSign, access_key);
            return signature;
        }
        private string percentEncode(string str)
        {
            str = UrlEncode(str);
            str = str.Replace("+", "%20");
            str = str.Replace("*", "%2A");
            str = str.Replace("%7E", "~");
            return str;
        }
        private string UrlEncode(string str)
        {
            str = System.Web.HttpUtility.UrlEncode(str);
            byte[] buf = Encoding.ASCII.GetBytes(str);
            for (int i = 0; i < buf.Length; i++)
                if (buf[i] == '%')
                {
                    if (buf[i + 1] >= 'a') buf[i + 1] -= 32;
                    if (buf[i + 2] >= 'a') buf[i + 2] -= 32;
                    i += 2;
                }
            return Encoding.ASCII.GetString(buf);
        }
        public string HmacSha1Sign(string text, string key)
        {
            Encoding encode = Encoding.UTF8;
            byte[] byteData = encode.GetBytes(text);
            byte[] byteKey = encode.GetBytes(key);
            HMACSHA1 hmac = new HMACSHA1(byteKey);
            CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);
            cs.Write(byteData, 0, byteData.Length);
            cs.Close();
            return Convert.ToBase64String(hmac.Hash);
        }
        public string HttpPost(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.GetEncoding("UTF-8");
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }
        public string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        } 
    }
}
