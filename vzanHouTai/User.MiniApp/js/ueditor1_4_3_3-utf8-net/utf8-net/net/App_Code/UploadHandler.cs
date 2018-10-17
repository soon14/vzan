using Core.MiniApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Utility.AliOss;

/// <summary>
/// UploadHandler 的摘要说明
/// </summary>
public class UploadHandler : Handler
{

    public UploadConfig UploadConfig { get; private set; }
    public UploadResult Result { get; private set; }

    public UploadHandler(HttpContext context, UploadConfig config)
        : base(context)
    {
        this.UploadConfig = config;
        this.Result = new UploadResult() { State = UploadState.Unknown };
    }

    public override void Process()
    { 
        UploadImage();
        return;
        //byte[] uploadFileBytes = null;
        //string uploadFileName = null;

        //if (UploadConfig.Base64)
        //{
        //    uploadFileName = UploadConfig.Base64Filename;
        //    uploadFileBytes = Convert.FromBase64String(Request[UploadConfig.UploadFieldName]);
        //}
        //else
        //{
        //    var file = Request.Files[UploadConfig.UploadFieldName];
        //    uploadFileName = file.FileName;

        //    if (!CheckFileType(uploadFileName))
        //    {
        //        Result.State = UploadState.TypeNotAllow;
        //        WriteResult();
        //        return;
        //    }
        //    if (!CheckFileSize(file.ContentLength))
        //    {
        //        Result.State = UploadState.SizeLimitExceed;
        //        WriteResult();
        //        return;
        //    }

        //    uploadFileBytes = new byte[file.ContentLength];
        //    try
        //    {
        //        file.InputStream.Read(uploadFileBytes, 0, file.ContentLength);
        //    }
        //    catch (Exception)
        //    {
        //        Result.State = UploadState.NetworkError;
        //        WriteResult();
        //    }
        //}

        //Result.OriginFileName = uploadFileName;

        //var savePath = PathFormatter.Format(uploadFileName, UploadConfig.PathFormat);
        //var localPath = Server.MapPath(savePath);
        //try
        //{
        //    if (!Directory.Exists(Path.GetDirectoryName(localPath)))
        //    {
        //        Directory.CreateDirectory(Path.GetDirectoryName(localPath));
        //    }
        //    File.WriteAllBytes(localPath, uploadFileBytes);
        //    Result.Url = savePath;
        //    Result.State = UploadState.Success;
        //}
        //catch (Exception e)
        //{
        //    Result.State = UploadState.FileAccessError;
        //    Result.ErrorMessage = e.Message;
        //}
        //finally
        //{
        //    WriteResult();
        //}
    }
    public string domname = "upfile";
    private void UploadImage()
    {
        //var FileCollect = Request.Files[UploadConfig.UploadFieldName];
        HttpFileCollection FileCollect = System.Web.HttpContext.Current.Request.Files;
        string uploadFileName = "";
        int imgWidth, imgHeigth;
        string ext = string.Empty;
        if (FileCollect.Count > 0)
        {
            uploadFileName = FileCollect[domname].FileName;
            var size = FileCollect[domname].ContentLength;
            var fileStream = FileCollect[domname].InputStream;
            var byteData = new byte[size];
            if (null != fileStream)
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fileStream))
                {
                    byteData = br.ReadBytes(size);

                }
            }// 上传的文件为空
            else
            {
            }
            if (uploadFileName.Contains("."))
            {
                ext = uploadFileName.Split('.')[uploadFileName.Split('.').Length - 1];
            }
            else
            {
                Utility.ImgHelper.IsImgageType(byteData, "jpg", out ext);
            }
            //不是图片格式不让上传
            string[] ImgType = new string[] { "jpg", "jpeg", "gif", "png", "bmp" };
            string[] VodType = new string[] { "mp4", "avi", "rmvb", "avi", "rm" };
            if (ImgType.Contains<string>(ext.ToLower()))
            {
                Result.Url = uploadimg(byteData, out imgWidth, out imgHeigth, size, ext);
            }
            else if (VodType.Contains<string>(ext.ToLower()))
            {
                try
                {
                    string aliTempImgKey = string.Empty;
                    var aliTempImgFolder = AliOSSHelper.GetOssVoiceKey(ext.ToLower(), true, "video/folder", out aliTempImgKey);
                    var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteData, 1, "." + ext.ToLower());
                    Result.Url = aliTempImgKey;
                }
                catch (Exception ex)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), ex.Message);
                }
            }
        }
        Result.OriginFileName = uploadFileName;
        Result.State = UploadState.Success;
        WriteResult(); 
    }

    /// <summary>
    /// 传图片，电脑发帖是附件
    /// </summary>
    /// <param name="minisnsId"></param>
    /// <param name=domname></param>
    /// <param name="folder"></param>
    /// <param name="imageThumbnailUrl"></param>
    /// <param name="imgWidth"></param>
    /// <param name="imgHeigth"></param>
    /// <param name="imgSize"></param>
    /// <param name="postfix"></param>
    /// <param name="isWatermark"></param>
    /// <returns></returns>
    protected string uploadimg(byte[] byteData, out int imgWidth, out int imgHeigth, int imgSize, string postfix, bool istemp = true)
    {
        string msg = string.Empty;
        string BackUrl = "0";
        imgWidth = 0; imgHeigth = 0;
        MemoryStream imgStreamWithWaterMark = null;
        try
        {

            postfix = "." + postfix.ToLower();
            string aliTempImgKey = string.Empty;
            using (MemoryStream imgStream = new MemoryStream(byteData))
            {
                using (imgStreamWithWaterMark = imgStream)
                {
                    using (var image = new System.Drawing.Bitmap(imgStreamWithWaterMark))
                    {
                        imgWidth = image.Width; imgHeigth = image.Height;

                        if (postfix == "")
                        {
                            return "上传文件不能为空";//上传文件不能为空
                        }
                        List<string> typelist = new List<string>() { ".gif", ".jpg", ".jpeg", ".png", ".bmp" };

                        if (!typelist.Contains(postfix))
                        {
                            return "格式错误"; //格式错误！
                        }
                        if (imgSize > 20 * 1024 * 1024)
                        {
                            return "大小超出限制";//大小超出限制
                        }
                        if (imgSize >= 5 * 1024 * 1024 && postfix != ".gif")
                        {
                            imgStreamWithWaterMark = Utility.ImgHelper.CompressImage(new System.Drawing.Bitmap(imgStreamWithWaterMark), Convert.ToInt32(imgWidth * 0.6), Convert.ToInt32(imgHeigth * 0.6), 100);
                        }
                        if (null != imgStreamWithWaterMark)
                        {
                            //同步到AliOss
                            //上传的目录
                            var aliTempImgFolder = AliOSSHelper.GetOssImgKey(postfix.Replace(".", string.Empty), istemp, out aliTempImgKey);
                            var byteArray = imgStreamWithWaterMark.ToArray();
                            var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteArray, 1, postfix);
                            if (!putResult)
                            {
                                log4net.LogHelper.WriteInfo(this.GetType(), "图片同步到Ali失败！");
                            }
                        }
                        else
                        {
                            Exception ex = new Exception("AliOss同步文件失败imgStreamWithWaterMark为null");
                            log4net.LogHelper.WriteError(this.GetType(), ex);
                            return "{\"id\":\"-1\",\"imgUrl\":\"\"}";
                        }


                    }

                }


                BackUrl = aliTempImgKey;
            }


        }
        catch (Exception ex)
        {
            log4net.LogHelper.WriteError(this.GetType(), ex);
            return "0";
        }
        finally
        {
            if (null != imgStreamWithWaterMark)
                imgStreamWithWaterMark.Dispose();
        }
        return BackUrl;
    }

    private void WriteResult()
    {
        this.WriteJson(new
        {
            state = GetStateMessage(Result.State),
            url = Result.Url,
            title = Result.OriginFileName,
            original = Result.OriginFileName,
            error = Result.ErrorMessage
        });
    }

    private string GetStateMessage(UploadState state)
    {
        switch (state)
        {
            case UploadState.Success:
                return "SUCCESS";
            case UploadState.FileAccessError:
                return "文件访问出错，请检查写入权限";
            case UploadState.SizeLimitExceed:
                return "文件大小超出服务器限制";
            case UploadState.TypeNotAllow:
                return "不允许的文件格式";
            case UploadState.NetworkError:
                return "网络错误"; 
        }
        return "未知错误";
    }

    private bool CheckFileType(string filename)
    {
        var fileExtension = Path.GetExtension(filename).ToLower();
        return UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
    }

    private bool CheckFileSize(int size)
    {
        return size < UploadConfig.SizeLimit;
    }
}

public class UploadConfig
{
    /// <summary>
    /// 文件命名规则
    /// </summary>
    public string PathFormat { get; set; }

    /// <summary>
    /// 上传表单域名称
    /// </summary>
    public string UploadFieldName { get; set; }

    /// <summary>
    /// 上传大小限制
    /// </summary>
    public int SizeLimit { get; set; }

    /// <summary>
    /// 上传允许的文件格式
    /// </summary>
    public string[] AllowExtensions { get; set; }

    /// <summary>
    /// 文件是否以 Base64 的形式上传
    /// </summary>
    public bool Base64 { get; set; }

    /// <summary>
    /// Base64 字符串所表示的文件名
    /// </summary>
    public string Base64Filename { get; set; }
}

public class UploadResult
{
    public UploadState State { get; set; }
    public string Url { get; set; }
    public string OriginFileName { get; set; }

    public string ErrorMessage { get; set; }
}

public enum UploadState
{
    Success = 0,
    SizeLimitExceed = -1,
    TypeNotAllow = -2,
    FileAccessError = -3,
    NetworkError = -4,
    Unknown = 1,
}

