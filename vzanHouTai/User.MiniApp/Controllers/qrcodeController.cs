using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThoughtWorks.QRCode;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class qrcodeController : baseController
    {
        // GET: QrCode
        public FileResult GetQrCode(string code,int type=0)
        {
            string Token = WxHelper.GetToken(type);
            //log4net.LogHelper.WriteInfo(this.GetType(), $"Token:{Token}");
            // 20分钟不登陆二维码失效！
            
            
            //return Content(ReultCode);
            for(int i=0; i < 5; i++)
            {
                string ReultCode = WxHelper.CreateQrCodeResult(Token, 60 * 20, code);
                if (ReultCode.IndexOf("ticket") > -1)
                {
                    //log4net.LogHelper.WriteInfo(this.GetType(), ReultCode);
                    CreateQrCodeResult createqrcoderesult = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<CreateQrCodeResult>(ReultCode);
                    if (createqrcoderesult != null)
                    {
                        //string Logo = "http://j.vzan.cc/miniapp/img/green_logo.png";
                        string Logo = "http:" + WebSiteConfig.MiniappZyUrl + "/img/green_logo.jpg";
                        MemoryStream ms = new MemoryStream();
                        Bitmap bmp = QRCodeHelp.Instance.GetQrCodeImg(Logo, createqrcoderesult.url);
                        bmp.Save(ms, ImageFormat.Jpeg);
                        bmp.Dispose();
                        return File(ms.ToArray(), "image/jpg");
                    }
                }
            }
            return null;
        }

    }

    public class CreateQrCodeResult
    {
        public string ticket { get; set; }
        public int expire_seconds { get; set; }
        public string url { get; set; }
    }
}