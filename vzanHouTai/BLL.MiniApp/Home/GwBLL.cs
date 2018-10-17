using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Home;
using System;
using System.Net;
using Utility.AliOss;

namespace BLL.MiniApp.Home
{
    public  class GwBLL : BaseMySql<Gw>
    {
        #region 单例模式
        private static GwBLL _singleModel;
        private static readonly object SynObject = new object();

        private GwBLL()
        {

        }

        public static GwBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new GwBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public string UpLoadToAliOss(string url, string ext, string sourceUlr, string title)
        {
            string aliTempImgKey = string.Empty;
            if (!url.Contains("http"))
                return aliTempImgKey = $"{WebSiteConfig.cdnurl}content/default_qrcode.png"; //"http://j.vzan.cc/dz/content/default_qrcode.png";
            byte[] data = null;
            try
            {


                using (WebClient web = new WebClient())
                {
                    data = web.DownloadData(url);
                }
                var aliTempImgFolder = AliOSSHelper.GetOssImgKey(ext, false, out aliTempImgKey);
                var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, data, 1, "." + ext);
                if (!putResult)
                {
                    log4net.LogHelper.WriteInfo(typeof(AliOSSHelper), "下载图片上传失败！图片同步到Ali失败！");
                    //上传失败
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(typeof(AliOSSHelper), ex.Message + "---" + url + "---" + sourceUlr + "---" + title);
                return string.Empty;
            }
            return aliTempImgKey;
        }
    }
}
