using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Core.MiniApp
{
    public class AliyunMultipartUpload
    {
        public InitiateMultipartUploadResult InitiateMultipartUpload(string filename)
        {
            string url = "http://mp4-vod.oss-cn-hangzhou.aliyuncs.com/"+ filename + "?uploads";
            DateTime dt = DateTime.Now.AddHours(-8);
            string _expire = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string text = "POST\n" + _expire + "\n/mp4-vod/";
            string Signature = HmacSha1Sign(text, "XtxUyYAcVo7IOsO79rGBLiXX3LQkFp");
            string Base64 = Convert.ToBase64String(Encoding.Default.GetBytes(Signature));
            string _authorization = "OSS LTAI4G9R8oHXt8yf:" + Base64;
            var send = new { Date = _expire, Authorization = _authorization };
            string post = Newtonsoft.Json.JsonConvert.SerializeObject(send);
            try
            {
                string xml = HttpPost(url, post);
                if (string.IsNullOrEmpty(xml))
                {
                    return null;
                }
                Hashtable requestHT = ParseXml(xml);
                InitiateMultipartUploadResult model = new InitiateMultipartUploadResult();
                model.Bucket = (string)requestHT["Bucket"];
                model.Key = (string)requestHT["Key"];
                model.UploadId = (string)requestHT["UploadId"];
                model.Date = _expire;
                model.Authorization = _authorization;
                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public CompleteMultipartUploadResult CompleteMultipartUpload(string filename,string _uploadId, List<ETags> list)
        {
            string url = "http://mp4-vod.oss-cn-hangzhou.aliyuncs.com/"+ filename + "?uploadId=" + _uploadId;
            DateTime dt = DateTime.Now.AddHours(-8);
            string _expire = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string text = "POST\n" + _expire + "\n/mp4-vod/";
            string Signature = HmacSha1Sign(text, "XtxUyYAcVo7IOsO79rGBLiXX3LQkFp");
            string Base64 = Convert.ToBase64String(Encoding.Default.GetBytes(Signature));
            string _authorization = "OSS LTAI4G9R8oHXt8yf:" + Base64;
            var send = new { Date = _expire, Authorization = _authorization };
            string post = Newtonsoft.Json.JsonConvert.SerializeObject(send);
            try
            {
                string pxml = CompleteMultipartUploadXml(list);
                post += pxml;
                string xml = HttpPost(url, post);
                if (string.IsNullOrEmpty(xml))
                {
                    return null;
                }
                Hashtable requestHT = ParseXml(xml);
                CompleteMultipartUploadResult model = new CompleteMultipartUploadResult();
                model.Location = (string)requestHT["Location"];
                model.Bucket = (string)requestHT["Bucket"];
                model.Key = (string)requestHT["Key"];
                model.ETag = (string)requestHT["ETag"];
                return model;
            }
            catch (Exception)
            {
                return null;
            }
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
        /// <summary>
        /// 将xml文件转换成Hashtable
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public Hashtable ParseXml(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode bodyNode = xmlDocument.ChildNodes[1];
            Hashtable ht = new Hashtable();
            if (bodyNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode xn in bodyNode.ChildNodes)
                {
                    ht.Add(xn.Name, xn.InnerText);
                }
            }
            return ht;
        }
        private string CompleteMultipartUploadXml(List<ETags> list)
        {
            StringBuilder strhtml = new StringBuilder();
            strhtml.Append("<CompleteMultipartUpload>");
            foreach (ETags item in list)
            {
                strhtml.Append("<Part>");
                strhtml.Append("<PartNumber>" + item.PartNumber + "</PartNumber>");
                strhtml.Append("<ETag>" + item.ETag + "</ETag>");
                strhtml.Append("</Part>");
            }
            strhtml.Append("</CompleteMultipartUpload> ");
            return strhtml.ToString();
        }
    }
    public class InitiateMultipartUploadResult
    {
        /// <summary>
        /// mp4-vod
        /// </summary>
        public string Bucket { get; set; }
        /// <summary>
        /// multipart.data
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 213CAB99DA5C409BA1B16C3F320BD251
        /// </summary>
        public string UploadId { get; set; }
        public string Date { get; set; }
        public string Authorization { get; set; }
    }
    public class CompleteMultipartUploadResult
    {
        public string Location { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }
        public string ETag { get; set; }
    }
    public class ETags
    {
        public int PartNumber { get; set; }
        public string ETag { get; set; }
    }
}
