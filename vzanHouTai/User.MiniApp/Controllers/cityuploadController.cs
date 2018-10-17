using BLL.MiniApp; 
using Core.MiniApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.AliOss;

namespace User.MiniApp.Controllers
{
    public class CityUploadController : Controller
    {
        public ActionResult Image()
        {
            HttpFileCollection FileCollect = System.Web.HttpContext.Current.Request.Files;
            string ext = string.Empty; Stream imgStreamWithWaterMark=null;
            int cityid = Utility.IO.Context.GetRequestInt("cityid", 0);
            if (FileCollect.Count > 0)
            {
                try
                {
                    HttpPostedFile file = FileCollect["upfile"];
                    var filestrem = file.InputStream;
                    if (null != filestrem)
                    {
                        //var text = string.Empty; var city = new C_CityInfoBLL().GetModel(cityid);
                        //if (null != city)
                        //{
                        //    text = city.CName;
                        //} //添加水印
                        //imgStreamWithWaterMark = Utility.IO.StreamHelpers.AddTextWatermark(filestrem, text);
                        //if (null == imgStreamWithWaterMark)
                            imgStreamWithWaterMark = filestrem;//添加水印失败就不添加水印,继续保存

                    }// 上传的文件为空
                    else
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "fileStream为null!");
                        return Json(new { state = "error", msg = "fileStream null" }, "text/x-json");
                    }
                    imgStreamWithWaterMark.Position = 0;//保证流可读
                    var byteData = Utility.IO.StreamHelpers.ReadFully(imgStreamWithWaterMark);
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
                        var format = "." + ext;
                        string aliImgKey;
                        var aliTempImgFolder = AliOSSHelper.GetOssImgKey(ext, false, out aliImgKey);

                        var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteData, 1, format);
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

    }
}