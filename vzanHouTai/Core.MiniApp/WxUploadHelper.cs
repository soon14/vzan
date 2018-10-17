using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Utility.AliOss;

namespace Core.MiniApp
{

    /// <summary>
    /// http辅助类
    /// </summary>
    public class WxUploadHelper
    {

        public static MemoryStream GetImgStream(string url)
        {
            using (WebClient web = new WebClient())
            {
                byte[] data = web.DownloadData(url);
                MemoryStream stream = new MemoryStream();
                stream.Write(data, 0, data.Length);
                return stream;
            }
        }

        /// <summary>
        /// 将另外的图片路径上传到我们服务器，返回我们服务器的图片路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isreload">是否获取了新路径</param>
        /// <returns></returns>
        public static string GetMyServerImgUrl(string url,ref bool isreload)
        {
            if(string.IsNullOrEmpty(url))
            {
                return "";
            }
            if(url.Contains("i.vzan.cc"))
            {
                return url;
            }

            string aliTempImgKey = "";
            using (WebClient web = new WebClient())
            {
                byte[] data = web.DownloadData(url);
                string aliTempImgFolder = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKey);
                if(AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, data, 1, ".jpg"))
                {
                    isreload = true;
                    return aliTempImgKey;
                }
            }

            return "";
        }
    }
}
