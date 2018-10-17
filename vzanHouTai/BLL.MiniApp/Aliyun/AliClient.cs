using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace BLL.MiniApp.Aliyun
{
    /// <summary>
    /// 响应内容格式
    /// </summary>
    public enum AliyunResponseFormat
    {
        Json,
        Xml
    }

    public class AliClient
    {
        #region 公共参数
        public AliyunResponseFormat Format { get; set; } = AliyunResponseFormat.Json;
        public string Version { get; set; } = "2014-06-18";

        public string AccessKeyId { get; set; } = ConfigurationManager.AppSettings["AliyunAccessKeyId_MTS"];

        public string Signature { get; set; }

        public string SignatureMethod { get; } = "HMAC-SHA1";

        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        public string SignatureVersion { get; } = "1.0";

        public string SignatureNonce { get; } = Guid.NewGuid().ToString();
        #endregion

        //private string _url;
        private HttpMethod _httpMethod;
        public string AccessKeySecret { get; set; } = ConfigurationManager.AppSettings["AccessKeySecret_MTS"];


        private Dictionary<string, string> _parameters;

        private void BuildParameters()
        {
            _parameters.Add(nameof(Format), Format.ToString().ToUpper());
            _parameters.Add(nameof(Version), Version);
            _parameters.Add(nameof(AccessKeyId), AccessKeyId);
            _parameters.Add(nameof(SignatureVersion), SignatureVersion);
            _parameters.Add(nameof(SignatureMethod), SignatureMethod);
            _parameters.Add(nameof(SignatureNonce), SignatureNonce);
            _parameters.Add(nameof(Timestamp), Timestamp);
        }
        public void ComputeSignature()
        {
            BuildParameters();
            var canonicalizedQueryString = string.Join("&",
                _parameters.OrderBy(x => x.Key)
                .Select(x => PercentEncode(x.Key) + "=" + PercentEncode(x.Value)));

            var stringToSign = _httpMethod.ToString().ToUpper() + "&%2F&" + PercentEncode(canonicalizedQueryString);

            var keyBytes = Encoding.UTF8.GetBytes(AccessKeySecret + "&");
            var hmac = new HMACSHA1(keyBytes);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            Signature = Convert.ToBase64String(hashBytes);
            _parameters.Add(nameof(Signature), Signature);
        }
        private string PercentEncode(string value)
        {
            return UpperCaseUrlEncode(value)
                .Replace("+", "%20")
                .Replace("*", "%2A")
                .Replace("%7E", "~");
        }
        private static string UpperCaseUrlEncode(string s)
        {
            char[] temp = System.Web.HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }
        public AliClient(HttpMethod httpMethod, Dictionary<string, string> parameters)
        {
            _httpMethod = httpMethod;
            _parameters = parameters;
        }
        public string GetUrl(string url)
        {
            ComputeSignature();
            return url + "?" +
                string.Join("&", _parameters.Select(x => x.Key + "=" + System.Web.HttpUtility.UrlEncode(x.Value)));
        }
    }
}
