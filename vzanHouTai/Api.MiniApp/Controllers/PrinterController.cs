using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Api.MiniApp.Controllers
{
    public class PrinterController : Controller
    {
        // GET: Printer
        public ActionResult Notify()
        {
           // string msg = JsonConvert.ToString(HttpContext.Request.Url);
            return Content(HttpContext.Request.QueryString.ToString());
        }
        /// <summary>
        ///  构造模拟远程HTTP的POST请求，POST数据Json字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public string DoPostJson(string url, string jsonData)
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
    }
}