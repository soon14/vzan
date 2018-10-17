using System;
using Entity.MiniApp;
using System.Collections;
using System.Net;
using Utility.AliOss;
using System.Collections.Generic; 
using System.IO;

namespace Core.MiniApp
{
    public class CityCommonUtils
    {
        /// <summary>
        /// 下载添加文章编辑器粘帖的图片
        /// </summary>
        /// <param name="minsnsId"></param>
        /// <param name="userId"></param>
        /// <param name="articleId"></param>
        /// <param name="content"></param>
        public static bool downloadImgs(int cityid, ref string content, ArrayList Imglist)
        {
            content = content.Replace("!important","");
            foreach (string imgpath in Imglist)
            {
                if (string.IsNullOrEmpty(imgpath))
                {
                    continue;
                }
                if (imgpath.IndexOf(WebSiteConfig.ImageUrl) > -1)
                {
                    continue;
                }
                string newurl = DownloadImage(cityid, imgpath);
                if (!string.IsNullOrEmpty(newurl) && "notpic" != newurl)
                {
                    content = content.Replace(imgpath, newurl);
                }
                else
                {
                    if (string.IsNullOrEmpty(newurl))
                    {
                        //log4net.LogHelper.WriteInfo(typeof(MinisnsCore), "newurl:下载失败！");
                    }

                }
            }
            return true;

        }
        public static string DownloadImage(int cityid, string imgUrl)
        {

            try
            {
                string postfix = string.Empty, filepath = string.Empty;
                if (imgUrl.Contains("http://mmbiz.qpic.cn"))
                {
                    if (imgUrl.Contains("?"))
                    {
                        int start = imgUrl.IndexOf("?");
                        imgUrl = imgUrl.Substring(0, start);
                    }
                }
                //上传原图的路径
                WebRequest fileWebRequest = null;
                try
                {
                    fileWebRequest = WebRequest.Create(imgUrl);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                //捕捉路径问题的返回错误
                MemoryStream container = null;
                Stream imgStreamWithWaterMark = null;
                try
                {
                    using (WebResponse fileWebResponse = fileWebRequest.GetResponse())
                    {
                        container = new MemoryStream();
                        fileWebResponse.GetResponseStream().CopyTo(container);
                        container.Position = 0;//保证流可读
                        if (null != container && container.Length > 0)
                        {
                            string text = string.Empty;
                            //var city = new C_CityInfoBLL().GetModel(cityid);
                            //if (null != city)
                            //{
                            //    text = city.CName;
                            //} //添加水印
                            //imgStreamWithWaterMark = Utility.IO.StreamHelpers.AddTextWatermark(container, text);
                            if (null == imgStreamWithWaterMark)
                                imgStreamWithWaterMark = container;//添加水印失败就不添加水印,继续保存
                            imgStreamWithWaterMark.Position = 0;//保证流可读
                            byte[] byteData = Utility.IO.StreamHelpers.ReadFully(imgStreamWithWaterMark);
                            string ext;
                            Utility.ImgHelper.IsImgageType(byteData, "jpg", out ext);
                            string format = "." + ext;
                            List<string> typelist = new List<string>() { ".gif", ".jpg", ".jpeg", ".png", ".bmp" ,".riff"};
                            if (!typelist.Contains(format))
                            {
                                return "notpic"; //格式错误！
                            }
                            if (ext == "riff")// 公众号文章图片格式
                                ext = "jpg";
                            string aliImgUrl = string.Empty; string aliOssImgName = string.Empty; string aliOssImgUrl = string.Empty;
                            aliOssImgName = AliOSSHelper.GetOssImgKey(ext, false, out aliOssImgUrl);
                            bool retult = AliOSSHelper.PutObjectFromByteArray(aliOssImgName, byteData, 1);
                            if (retult)
                                return aliOssImgUrl;
                            else
                                return "";

                        }
                        else
                        {
                            log4net.LogHelper.WriteInfo(typeof(CityCommonUtils), "自动抓取图片DownloadImage fileWebResponse.GetResponseStream()  为null");
                        }

                    }

                }
                catch (Exception ex)
                {

                    log4net.LogHelper.WriteError(typeof(Utility.Module), ex);
                    return "notpic"; // 不是图片
                }
                finally {
                    if (null != container)
                        container.Dispose();
                    if (null != imgStreamWithWaterMark)
                        imgStreamWithWaterMark.Dispose();
                }

            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(Utility.Module), ex);
                //跟踪一下为什么说URI格式错误
                log4net.LogHelper.WriteInfo(typeof(Utility.Module), imgUrl);
            }
            return "";
        }
    }
}
