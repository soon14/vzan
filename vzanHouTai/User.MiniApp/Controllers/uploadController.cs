using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;
using Utility;
using Utility.AliOss;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class uploadController : Controller
    {
        private readonly static string _checkWebViewHost = WebSiteConfig.CheckWebViewHost;
        public ActionResult UploadImg()
        {
            Stream imgStreamWithWaterMark = null;
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files[0];
                int needWarterMark = Context.GetRequestInt("awm", 0);
                int cityid = Context.GetRequestInt("cityid", 0);
                Stream filestrem = file.InputStream;
                string ext = string.Empty;
                if (null != filestrem && file.ContentLength > 0)
                {
                    //需要添加水印
                    //if (1 == needWarterMark)
                    //{
                    //    var text = string.Empty; var city = new C_CityInfoBLL().GetModel(cityid);
                    //    if (null != city)
                    //    {
                    //        text = city.CName;
                    //    } //添加水印
                    //    imgStreamWithWaterMark = StreamHelpers.AddTextWatermark(filestrem, text);
                    //    if (null == imgStreamWithWaterMark)
                    //        imgStreamWithWaterMark = filestrem;//添加水印失败就不添加水印,继续保存
                    //}
                    //else
                    //{
                    imgStreamWithWaterMark = filestrem;
                    //}


                }// 上传的文件为空
                else
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "UploadImg fileStream为null!");
                    return Json(new { Success = false, Msg = "上传图片失败", Path = "" }, JsonRequestBehavior.AllowGet);
                }
                imgStreamWithWaterMark.Position = 0;//保证流可读
                byte[] byteData = StreamHelpers.ReadFully(imgStreamWithWaterMark);
                Utility.ImgHelper.IsImgageType(byteData, "jpg", out ext);
                //不是图片格式不让上传
                string[] ImgType = new string[] { "jpg", "jpeg", "gif", "png", "bmp" };
                if (!ImgType.Contains<string>(ext.ToLower()))
                {
                    return Json(new { Success = false, Msg = "不支持的格式,目前只支持jpg，jpeg，gif，png，bmp", Path = "" }, JsonRequestBehavior.AllowGet);

                }

                string aliTempImgKey = SaveImageToAliOSS(byteData);
                return Json(new { Success = true, Msg = "上传图片成功", Path = aliTempImgKey }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(GetType(), ex);
                return Json(new { Success = false, Msg = "上传图片失败", Path = "" }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                if (null != imgStreamWithWaterMark)
                {
                    imgStreamWithWaterMark.Dispose();
                }
            }
        }
        public ActionResult Image()
        {
            HttpFileCollection FileCollect = System.Web.HttpContext.Current.Request.Files;
            string ext = string.Empty; Stream imgStreamWithWaterMark = null;
            int cityid = Context.GetRequestInt("cityid", 0);
            if (FileCollect.Count > 0)
            {
                try
                {
                    var file = FileCollect["upfile"];
                    Stream filestrem = file.InputStream;
                    if (null != filestrem)
                    {
                        //var text = string.Empty; var city = new C_CityInfoBLL().GetModel(cityid);
                        //if (null != city)
                        //{
                        //    text = city.CName;
                        //} //添加水印
                        //imgStreamWithWaterMark = StreamHelpers.AddTextWatermark(filestrem, text);
                        //if (null == imgStreamWithWaterMark)
                        imgStreamWithWaterMark = filestrem;//添加水印失败就不添加水印,继续保存

                    }// 上传的文件为空
                    else
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "fileStream为null!");
                        return Json(new { state = "error", msg = "fileStream null" }, "text/x-json");
                    }
                    imgStreamWithWaterMark.Position = 0;//保证流可读
                    byte[] byteData = StreamHelpers.ReadFully(imgStreamWithWaterMark);
                    Utility.ImgHelper.IsImgageType(byteData, "jpg", out ext);
                    //不是图片格式不让上传
                    string[] ImgType = new string[] { "jpg", "jpeg", "gif", "png", "bmp" };
                    if (!ImgType.Contains<string>(ext.ToLower()))
                    {
                        return Json(new { state = "error", msg = "不支持的格式" }, "text/x-json");
                    }

                    if (null != byteData)
                    {
                        //同步到AliOss
                        string format = "." + ext;
                        string aliImgKey;
                        string aliTempImgFolder = AliOSSHelper.GetOssImgKey(ext, false, out aliImgKey);

                        bool putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteData, 1, format);
                        if (!putResult)
                        {
                            log4net.LogHelper.WriteInfo(this.GetType(), "图片同步到Ali失败！");
                        }

                        return Json(new
                        {
                            originalName = file.FileName,
                            name = "",
                            url = aliImgKey,
                            size = file.ContentLength,
                            state = "SUCCESS",
                            type = format
                        }, "text/x-json");
                    }
                    else
                    {
                        return Json(new { state = "error", msg = "byteData null" }, "text/x-json");
                    }

                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteError(this.GetType(), ex);
                    return Json(new { state = "error", msg = "runtime error" }, "text/x-json");

                }
                finally
                {
                    if (null != imgStreamWithWaterMark)
                        imgStreamWithWaterMark.Dispose();
                }
            }
            else
            {
                return Json(new { state = "error", msg = "no data to upload" }, "text/x-json");
            }

        }

        public string SaveImageToAliOSS(byte[] byteArray)
        {
            string aliTempImgKey = string.Empty;
            string aliTempImgFolder = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKey);
            AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteArray, 1, ".jpg");
            return aliTempImgKey;
        }

        #region 上传文件-蔡华兴
        public ActionResult UploadFile()
        {
            try
            {
                HttpContext context = System.Web.HttpContext.Current;
                if (context.Request.Files.Count > 0)
                {
                    HttpFileCollection files = context.Request.Files;
                    string fileName = "";
                    foreach (string key in files)
                    {
                        HttpPostedFile file = files[key];

                        if (!string.IsNullOrEmpty(key) && key.Substring(key.LastIndexOf(".")).Contains("zip"))
                        {
                            fileName = @"\\share.vzan.cc\file\dzcert\" + key;
                            file.SaveAs(fileName);
                        }
                        else
                        {
                            return Json(new { isok = false, msg = "必须上传zip文件" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return Json(new { isok = false, msg = "请选择文件" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = true, msg = "上传文件成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = "上传文件失败" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 上传微信的验证文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadWxFile()
        {
            try
            {
                HttpContext context = System.Web.HttpContext.Current;
                if (context.Request.Files.Count > 0)
                {
                    HttpFileCollection files = context.Request.Files;
                    string fileName = "";
                    foreach (string key in files)
                    {
                        HttpPostedFile file = files[key];
                        if (file.ContentLength > 1024 * 2)
                        {
                            return Json(new { isok = true, msg = "必须上传微信验证txt文件" });
                        }
                        if (!string.IsNullOrEmpty(key) && key.Substring(key.LastIndexOf(".")).Contains("txt"))
                        {
                            fileName = @"https://wtApi.vzan.com/" + file.FileName;
                            file.SaveAs(fileName);
                        }
                        else
                        {
                            return Json(new { isok = true, msg = "必须上传txt文件" });
                        }
                    }
                    return Json(new { isok = true, msg = "上传文件成功", txtURL = fileName });
                }
                else
                {

                }

                return Json(new { isok = true, msg = "上传文件成功" });
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = "上传文件失败" + ex.Message });
            }
        }

        /// <summary>
        /// 上传文本文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadCheckDomainFile()
        {
            Return_Msg resultData = new Return_Msg();
            HttpContext context = System.Web.HttpContext.Current;
            HttpFileCollection files = context.Request.Files;
            if (files.Count <= 0)
            {
                resultData.Msg = "没有找到要上传的文件";
                return Json(resultData);
            }
            HttpPostedFile file = files[0];
            if (string.IsNullOrEmpty(file.FileName) || !file.FileName.Substring(file.FileName.LastIndexOf(".")).Contains("txt"))
            {
                resultData.Msg = "必须上传txt文件";
                return Json(resultData);
            }
            if(file.ContentLength>50)
            {
                resultData.Msg = "上传文件不能大于50字节";
                return Json(resultData);
            }

            try
            {
                
                BinaryReader br = new BinaryReader(file.InputStream);
                byte[] postArray = br.ReadBytes((int)file.InputStream.Length);

                //文件写入
                FileStream fs = System.IO.File.Open(_checkWebViewHost + file.FileName, FileMode.Create);
                fs.Write(postArray, 0, postArray.Length);
                fs.Close();

                resultData.isok = true;
                resultData.Msg = "上传成功";
            }
            catch (Exception ex)
            {
                resultData.Msg = ex.Message;
            }
            return Json(resultData);
        }

        #endregion

        #region 上传音频文件-蔡华兴
        public ActionResult UploadVoice()
        {
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files[0];
                if (file.InputStream.Length == 0)
                {
                    return Json(new { isok = false, msg = "上传失败，音频大小不能空", Path = "" }, JsonRequestBehavior.AllowGet);
                }
                if ((file.InputStream.Length / 1024.0 / 1024.0) > 10)
                {
                    return Json(new { isok = false, msg = "上传失败，音频文件不能超过10M", Path = "" }, JsonRequestBehavior.AllowGet);
                }
                if (!file.FileName.Substring(file.FileName.LastIndexOf(".")).Contains("mp3"))
                {
                    return Json(new { isok = false, msg = "上传失败，音频格式必须是MP3", Path = "" }, JsonRequestBehavior.AllowGet);
                }


                int storeid = int.Parse(Context.GetRequest("storeid", "0"));
                int voicetype = Context.GetRequestInt("voicetype", (int)AttachmentItemType.小程序语音);

                byte[] byteData = new byte[file.InputStream.Length];
                file.InputStream.Position = 0;
                file.InputStream.Read(byteData, 0, byteData.Length);
                file.InputStream.Close();

                //将下载的语音放到AliOss临时文件夹
                string voiceAliOssKey = string.Empty;
                //上传目录
                string voiceAliOssFolder = AliOSSHelper.GetOssVoiceKey("mp3", true, "voice/folder", out voiceAliOssKey);
                bool putResult = AliOSSHelper.PutObjectFromByteArray(voiceAliOssFolder, byteData);

                if (!putResult)// 未能成功同步到AliOss
                {
                    return Json(new { isok = false, msg = "上传失败", Path = "" }, JsonRequestBehavior.AllowGet);
                }

                List<C_Attachment> attachlist = C_AttachmentBLL.SingleModel.GetListByCache(storeid, voicetype);
                if (storeid > 0 && attachlist != null && attachlist.Count > 0)
                {
                    C_AttachmentBLL.SingleModel.RemoveRedis(storeid, voicetype);
                    return Json(new { isok = true, msg = "上传成功", Path = voiceAliOssKey, voiceId = attachlist[0].id }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    C_Attachment attach = new C_Attachment();
                    attach.itemId = 0;
                    attach.itemType = voicetype;
                    attach.filepath = voiceAliOssKey;
                    attach.createDate = DateTime.Now;
                    attach.VoiceServerId = Guid.NewGuid().ToString();
                    attach.thumbnail = voiceAliOssKey;

                    int voiceId = Convert.ToInt32(C_AttachmentBLL.SingleModel.Add(attach));
                    if (voiceId > 0)
                    {
                        return Json(new { isok = true, msg = "上传成功", Path = voiceAliOssKey, voiceId = voiceId }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { isok = false, msg = "系统错误，请重新尝试", Path = "" });
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "系统错误，请重新尝试", Path = "" });
            }
        }

        [HttpPost]
        public ActionResult UploadVoiceOnly()
        {
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files[0];
                if (file.InputStream.Length == 0)
                {
                    return Json(new { isok = false, msg = "上传失败，音频大小不能空", Path = "" });
                }
                if ((file.InputStream.Length / 1024.0 / 1024.0) > 10)
                {
                    return Json(new { isok = false, msg = "上传失败，音频文件不能超过10M", Path = "" });
                }
                if (!file.FileName.Substring(file.FileName.LastIndexOf(".")).Contains("mp3"))
                {
                    return Json(new { isok = false, msg = "上传失败，音频格式必须是MP3", Path = "" });
                }

                byte[] byteData = new byte[file.InputStream.Length];
                file.InputStream.Position = 0;
                file.InputStream.Read(byteData, 0, byteData.Length);
                file.InputStream.Close();

                //将下载的语音放到AliOss临时文件夹
                string voiceAliOssKey = string.Empty;
                //上传目录
                string voiceAliOssFolder = AliOSSHelper.GetOssVoiceKey("mp3", true, "voice/folder", out voiceAliOssKey);
                bool putResult = AliOSSHelper.PutObjectFromByteArray(voiceAliOssFolder, byteData);

                if (!putResult)// 未能成功同步到AliOss
                {
                    return Json(new { isok = false, msg = "上传失败", Path = "" });
                }

                return Json(new { isok = true, msg = "上传成功", Path = voiceAliOssKey });
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "系统错误，请重新尝试", Path = "" });
            }
        }

        #endregion

        #region 上传视频文件-蔡华兴
        [HttpPost]
        public async Task<ActionResult> UploadVedioAsync(int id)
        {
            try
            {
                var file = System.Web.HttpContext.Current.Request.Files[0];
                if (file.InputStream.Length == 0)
                {
                    return Json(new { isok = false, msg = "上传失败，视频大小不能空", Path = "" }, JsonRequestBehavior.AllowGet);
                }
                if (file.InputStream.Length > 104857600)
                {
                    return Json(new { isok = false, msg = "上传失败，视频文件不能超过100M", Path = "" }, JsonRequestBehavior.AllowGet);
                }

                string fileType = System.IO.Path.GetExtension(file.FileName);
                byte[] byteData = new byte[file.InputStream.Length];

                string tempurl = string.Empty;
                string tempkey = VideoAliMtsHelper.GetOssVideoKey(fileType.Replace(".", ""), true, out tempurl);
                Stream fileStream = file.InputStream;
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
                    return Json(new { isok = false, msg = "系统错误！请联系管理员." });
                }
                bool putrsult = AliOSSHelper.PutObjectFromByteArray(tempkey, byteData, 1, fileType.ToLower());
                Tuple<string, string> pathDic = await C_AttachmentVideoBLL.SingleModel.GetConvertVideoPathAsync(tempkey);
                return Json(new { isok = true, msg = "上传成功", videoconvert = pathDic.Item2, Vid = 0, soucepath = pathDic.Item1 });
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "系统错误，请重新尝试", Path = "" });
            }
        }
        #endregion


        #region 阿里云OSS上传
        /// <summary>
        /// 初始化上传
        /// </summary>
        /// <param name="type">文件类型</param>
        /// <returns></returns>
       // [HttpGet]
        public JsonResult InitUpload(string type)
        {
            DateTime dt = DateTime.Now.AddMinutes(10).AddHours(-8);
            string expire = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string accessid = "LTAI4G9R8oHXt8yf";
            string host = "http://vzan-img.oss-cn-hangzhou.aliyuncs.com";
            long timestamp = (dt.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            string dir = "upload/" + type + "/" + DateTime.Now.ToString("yyyyMMdd") + "/";
            long contentLength = 1024 * 1024 * 1024 * 10L;
            string policy = "{\"expiration\":\"" + expire +
                            "\",\"conditions\":[[\"content-length-range\",0," + contentLength + "],[\"starts-with\",\"$key\",\"" +
                            dir + "\"]]}";
            string base64Policy = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(policy));
            string signature = new AliyunLive().HmacSha1Sign(base64Policy, "XtxUyYAcVo7IOsO79rGBLiXX3LQkFp");

            //文件名称不需要后续名，这里取不到后缀名
            string key = Guid.NewGuid().ToString("N");

            var token = new
            {
                accessid = accessid,
                host = host,
                policy = base64Policy,
                signature = signature,
                expire = timestamp,
                dir = dir,
                key = key
            };
            return Json(token, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 阿里云OSS大文件上传 初始化
        /// </summary>
        /// <returns></returns>
        public ActionResult InitiateMultipartUpload()
        {
            string newfilename = Guid.NewGuid().ToString("N") + ".mp4";
            InitiateMultipartUploadResult InitUpload = new AliyunMultipartUpload().InitiateMultipartUpload(newfilename);
            if (InitUpload != null)
            {
                return Json(new { isok = true, NewFileName = newfilename, UploadId = InitUpload.UploadId, Date = InitUpload.Date, Authorization = InitUpload.Authorization });
            }
            return Json(new { isok = false, msg = "初始化失败！" });
        }


        #endregion
    }
}