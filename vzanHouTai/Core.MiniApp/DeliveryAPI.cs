using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.MiniApp
{
    /// <summary>
    /// 快鸟物流接口
    /// </summary>
    public class DeliveryAPI
    {
        private static Encoding Format = Encoding.UTF8;

        /// <summary>
        /// 申请实时物流信息
        /// </summary>
        /// <param name="merchId">商户号</param>
        /// <param name="authKey">授权Key</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司Code</param>
        /// <returns></returns>
        public static object RequestRealTime(string merchId, string authKey, string deliveryNo, string companyCode)
        {
            string requestData = $"{{'OrderCode':'','ShipperCode':'{companyCode}','LogisticCode':'{deliveryNo}'}}";
            string dataSign = encrypt(requestData, authKey, "UTF-8");

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("RequestData", HttpUtility.UrlEncode(requestData, Format));
            param.Add("EBusinessID", merchId);
            //RequestType（申请动作）：1002 == 实时查询
            param.Add("RequestType", "1002");
            //DataType（返回数据类型）：2 == JSON
            param.Add("DataType", "2");
            param.Add("DataSign", HttpUtility.UrlEncode(dataSign, Format));

            string result = sendPost("http://api.kdniao.cc/Ebusiness/EbusinessOrderHandle.aspx", param);
            return result;
        }

        /// <summary>
        /// 申请订阅物流信息（需要配合回调地址）
        /// </summary>
        /// <param name="merchId">商户号</param>
        /// <param name="authKey">授权Key</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司Code</param>
        /// <param name="deliveryId">本地订单号（可为空）</param>
        /// <param name="callBack">自定义回调字段（可为空）</param>
        /// <returns></returns>
        public static object RequestSubscribe(string merchId, string authKey, string deliveryNo, string companyCode, string deliveryId = null, string callBack = null)
        {
            string requestData = $"{{'OrderCode':'','ShipperCode':'{companyCode}','LogisticCode':'{deliveryNo}'}}";
            string dataSign = encrypt(requestData, authKey, "UTF-8");

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("RequestData", HttpUtility.UrlEncode(requestData, Format));
            param.Add("EBusinessID", merchId);
            //RequestType（申请动作）：1008 == 订阅推送
            param.Add("RequestType", "1008");
            //DataType（返回数据类型）：2 == JSON
            param.Add("DataType", "2");
            param.Add("DataSign", HttpUtility.UrlEncode(dataSign, Format));
            //callBacb（自定义回调字段）
            param.Add("CallBack", callBack);
            param.Add("OrderCode", deliveryId);

            //log4net.LogHelper.WriteInfo(typeof(DeliveryAPI), Newtonsoft.Json.JsonConvert.SerializeObject(param));

            string result = sendPost("https://api.kdniao.com/api/dist", param);
            return result;
        }

        public bool CompareSign(string content,string key, string sign)
        {
            return string.Equals(sign, encrypt(content, key, "UTF-8"));
        }

        /// <summary>
        /// Post方式提交数据，返回网页的源代码
        /// </summary>
        /// <param name="url">发送请求的 URL</param>
        /// <param name="param">请求的参数集合</param>
        /// <returns>远程资源的响应结果</returns>
        private static string sendPost(string url, Dictionary<string, string> param)
        {
            string result = "";
            StringBuilder postData = new StringBuilder();
            if (param != null && param.Count > 0)
            {
                foreach (var p in param)
                {
                    if (postData.Length > 0)
                    {
                        postData.Append("&");
                    }
                    postData.Append(p.Key);
                    postData.Append("=");
                    postData.Append(p.Value);
                }
            }
            byte[] byteData = Encoding.GetEncoding("UTF-8").GetBytes(postData.ToString());
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = url;
                request.Accept = "*/*";
                request.Timeout = 30 * 1000;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
                request.Method = "POST";
                request.ContentLength = byteData.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(byteData, 0, byteData.Length);
                stream.Flush();
                stream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream backStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(backStream, Encoding.GetEncoding("UTF-8"));
                result = sr.ReadToEnd();
                sr.Close();
                backStream.Close();
                response.Close();
                request.Abort();
            }
            catch (Exception ex)
            {
                result = ex.Message;
                log4net.LogHelper.WriteError(typeof(DeliveryAPI), ex);
            }
            return result;
        }

        ///<summary>
        ///电商Sign签名
        ///</summary>
        ///<param name="content">内容</param>
        ///<param name="keyValue">Appkey</param>
        ///<param name="charset">URL编码 </param>
        ///<returns>DataSign签名</returns>
        private static string encrypt(string content,string encryptKey, string charset)
        {
            if (encryptKey != null)
            {
                return base64(MD5(content + encryptKey, charset), charset);
            }
            return base64(MD5(content, charset), charset);
        }

        ///<summary>
        /// 字符串MD5加密
        ///</summary>
        ///<param name="str">要加密的字符串</param>
        ///<param name="charset">编码方式</param>
        ///<returns>密文</returns>
        private static string MD5(string str, string charset)
        {
            byte[] buffer = Encoding.GetEncoding(charset).GetBytes(str);
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider check;
                check = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] somme = check.ComputeHash(buffer);
                string ret = "";
                foreach (byte a in somme)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("X");
                    else
                        ret += a.ToString("X");
                }
                return ret.ToLower();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// base64编码
        /// </summary>
        /// <param name="str">内容</param>
        /// <param name="charset">编码方式</param>
        /// <returns></returns>
        private static string base64(string str, string charset)
        {
            return Convert.ToBase64String(Encoding.GetEncoding(charset).GetBytes(str));
        }
    }
}
