using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BLL.MiniApp
{
    public class VideoAliMtsHelper
    {
        public class Output
        {
            public string OutputObject { get; set; } = string.Empty;
            public string TemplateId { get; set; } = string.Empty;

        }

        /// <summary>
        /// 响应内容格式
        /// </summary>
        public enum AliyunResponseFormat
        {
            Json,
            Xml
        }

        public class AliYunRequest
        {
            #region 公共参数
            public AliyunResponseFormat Format { get; set; } = AliyunResponseFormat.Json;
            public string Version { get; } = "2014-06-18";

            public string AccessKeyId { get; set; } = ConfigurationManager.AppSettings["AliyunAccessKeyId_MTS"];

            public string Signature { get; set; }

            public string SignatureMethod { get; } = "HMAC-SHA1";

            public string TimeStamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

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
                _parameters.Add(nameof(TimeStamp), TimeStamp);
            }
            public string GetUrl(string url)
            {
                ComputeSignature();
                return url + "?" +
                    string.Join("&", _parameters.Select(x => x.Key + "=" + HttpUtility.UrlEncode(x.Value)));
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
                char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
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
            public AliYunRequest(HttpMethod httpMethod, Dictionary<string, string> parameters)
            {
                _httpMethod = httpMethod;
                _parameters = parameters;
            }
           
        }
        public static string GetOssVideoKey(string format, bool istemp, out string url)
        {
            string ossurl = ConfigurationManager.AppSettings["OSSURL"];
            if (string.IsNullOrEmpty(ossurl))
            {
                log4net.LogHelper.WriteError(typeof(VideoAliMtsHelper), new Exception("请配置OSSURL"));
                url = string.Empty;
                return string.Empty;
            }
            if (!ossurl.EndsWith("/"))
            {
                ossurl += "/";
            }

            DateTime now = DateTime.Now;
            string dateTimePrefix = now.ToString("yyyy/M/d");
            string ossGuidName = Guid.NewGuid().ToString("N").ToLower();
            string InputName = string.Empty;
            //临时文件夹
            if (istemp)
            {
                InputName = $"temp/video/{format}/{dateTimePrefix}/{now.ToString("HHmmss")}{ossGuidName}.{format}";//string.Format("temp/image/{0}/{1}/{2}{3}.{4}", format, dateTimePrefix, now.ToString("HHmmss"), ossGuidName, format);
            }
            else
            {
                InputName = $"video/{format}/{dateTimePrefix}/{now.ToString("HHmmss")}{ossGuidName}.{format}";//string.Format("image/{0}/{1}/{2}{3}.{4}", format, dateTimePrefix, now.ToString("HHmmss"), ossGuidName, format);
            }

            url = string.Format("{0}{1}", ossurl, InputName);
            return InputName;
        }
        public static string GetUrlFromKey(string key)
        {
            string ossurl = ConfigurationManager.AppSettings["OSSURL"];
            return string.Format("{0}{1}", ossurl, key);
        }
        public static async Task<bool> SubMitVieoJob(string inputObjectName, string outputObjectName)
        {
            string ossurl = ConfigurationManager.AppSettings["OSSURL"];
            outputObjectName = outputObjectName.Replace(ossurl,"");
            List<Output> outputs = new List<Output>();
            outputs.Add(new Output()
            {
                OutputObject = outputObjectName,
                TemplateId = "S00000001-200000"//转封装模板mp4格式
            });
            var parameters = new Dictionary<string, string>()
            {
                { "Action", "SubmitJobs" },
                { "Input",JsonConvert.SerializeObject(new { Bucket="vzan-img",Location="oss-cn-hangzhou",Object=inputObjectName})},
                { "OutputBucket","vzan-img" },
                { "OutputLocation","oss-cn-hangzhou"},
                { "Outputs",JsonConvert.SerializeObject(outputs)},
                { "PipelineId","e0ce680461b74696b9d7ca718320eb0d"}
            };
            try
            {
                var request = new AliYunRequest(HttpMethod.Get, parameters);
                var url = request.GetUrl(ConfigurationManager.AppSettings["MTSURL"]);
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();

                    return !string.IsNullOrEmpty(content) && content.LastIndexOf("\"Success\":true") > -1;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(VideoAliMtsHelper), ex);
                return false;
            }

        }
        private class OutputFile
        {
            public string Bucket { get; set; }
            public string Location { get; set; }
            public string Object { get; set; }
        }
        public static async Task<bool> SubMitVieoSnapshotJob(string inputObjectName, string outputObjectName)
        {
            OutputFile output = new OutputFile();
            output.Bucket = "vzan-img";
            output.Location = "oss-cn-hangzhou";
            output.Object = outputObjectName;
            var parameters = new Dictionary<string, string>()
            {
                { "Action", "SubmitSnapshotJob" },
                { "Input",JsonConvert.SerializeObject(new { Bucket="vzan-img",Location="oss-cn-hangzhou",Object=inputObjectName})},
                { "SnapshotConfig",JsonConvert.SerializeObject(new { OutputFile=output,Time=8,Interval=1,Num=1})},
                { "PipelineId","e0ce680461b74696b9d7ca718320eb0d"}
            };
            try
            {
                var request = new AliYunRequest(HttpMethod.Get, parameters);
                var url = request.GetUrl(ConfigurationManager.AppSettings["MTSURL"]);
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();

                    return !string.IsNullOrEmpty(content) && content.LastIndexOf("\"Success\":true") > -1;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(VideoAliMtsHelper), ex);
                return false;
            }

        }

    }
}
