using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using Utility.AliOss;
using Utility;

namespace User.MiniApp.Handler
{
    /// <summary>
    /// UploadVideoHandler 的摘要说明
    /// </summary>
    public class UploadVideoHandler : IHttpHandler
    {
        /// <summary>
        /// 此方法由APP调用。不可删除
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            //因图文混排视频直接显示，设两个返回的临时视频地址
            var reVideoUrl = string.Empty;
            var rePostUrl = string.Empty;
            context.Response.ContentType = "text/html";
            string name = context.Request["name"];
            HttpPostedFile file = context.Request.Files[name];
            try
            {
                int minsnsId = 0;
                int.TryParse(context.Request["minsnsId"], out minsnsId);
                var tempurl = string.Empty;
                string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
                int flag = 0;//标志是否需要转换
                List<string> extents = new List<string> { ".mp4", ".ogg", ".mpeg4", ".webm", ".MP4", ".OGG", ".MPEG4", ".WEBM" };
                string posterUploadDirectory = Path.Combine(ConfigurationManager.AppSettings["ImgUploadUrl4"], "videoposter", "mp4", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString());

                var tempkey = VideoAliMtsHelper.GetOssVideoKey(fileExtension.Replace(".", ""), true, out tempurl);
                var byteData = new byte[file.ContentLength];
                var fileStream = file.InputStream;
                if (null != fileStream)
                {
                    using (System.IO.BinaryReader br = new System.IO.BinaryReader(fileStream))
                    {
                        byteData = br.ReadBytes(file.ContentLength);
                    }
                }
                if (null == byteData)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "视频获取byte[]失败！");
                    context.Response.Write(new JavaScriptSerializer().Serialize(new { result = 0, msg = "系统错误！请联系管理员." }));

                }
                var putrsult = AliOSSHelper.PutObjectFromByteArray(tempkey, byteData, 1, fileExtension.Replace(".", ""));
                if (extents.Contains(fileExtension))
                {

                    flag = 1;
                }

                VideoAttachment video = new VideoAttachment();
                video.CreateDate = DateTime.Now;
                video.VideoSize = (file.ContentLength / (1024 * 1024.0)).ToString("N2");//单位为M
                //无需转换
                if (1 == flag)
                {
                    video.Status = 1;
                    video.ConvertFilePath = VideoAliMtsHelper.GetUrlFromKey(tempkey.Replace("temp/", ""));

                }
                else
                {
                    var regex = new Regex(@"(?i)\.[\S]*");
                    video.ConvertFilePath = VideoAliMtsHelper.GetUrlFromKey(regex.Replace(tempkey.Replace("temp/", ""), ".mp4"));
                    video.Status = -2;//待转换
                }
                video.UserId = int.Parse(context.Request.Form["UId"] ?? "0");
                video.Fid = minsnsId;
                video.SourceFilePath = tempkey;
                int Vid = Convert.ToInt32(VideoAttachmentBLL.SingleModel.Add(video));
                context.Response.Write(new JavaScriptSerializer().Serialize(new { result = "ok", msg = "上传成功", Vid = Vid}));
       
            }
            catch (Exception ex)
            {
                  StringHelper.WriteOperateLog("error", ex.Message);
                context.Response.Write("");
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}