using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.AliOss;

namespace Core.MiniApp
{
    /// <summary>
    /// 
    /// </summary>
    public static class HostFile
    {
        public static void GetVoiceFromPath(ref Voices voice, string path)
        {
            Image img = null;
            FileInfo mp3File = null;
            Image waterPic = null;
            try
            {
                mp3File = new FileInfo(path);
                Mp3Lib.Mp3File mp3file = new Mp3Lib.Mp3File(mp3File);
                Id3Lib.TagHandler taginfo = new Id3Lib.TagHandler(mp3file.TagModel);
                voice.Singer = taginfo.Artist; // 歌手。
                voice.Album = taginfo.Album; // 专辑。
                voice.SongName = taginfo.Title; // 歌名。
                                                //log4net.LogHelper.WriteInfo(typeof(Voices), string.Format("{0}:{1}:{2}", taginfo.Artist, taginfo.Album, taginfo.Title));
                                                //ShellClass sh = new ShellClass();
                                                //Folder dir = sh.NameSpace(Path.GetDirectoryName(path));
                                                //FolderItem item = dir.ParseName(Path.GetFileName(path));

                //voice.VoiceTime = string.IsNullOrEmpty(dir.GetDetailsOf(item, 27)) ? 0 : GetTimeSecond(dir.GetDetailsOf(item, 27)); // 获取歌曲时长。
                //voice.Singer = dir.GetDetailsOf(item, 13); // 歌手。
                //voice.Album = dir.GetDetailsOf(item, 14); // 专辑。
                //voice.SongName = dir.GetDetailsOf(item, 21); // 歌名。
                img = taginfo.Picture;
                if (img != null)
                {
                    //string ImgUploadUrl = System.IO.Path.Combine(ConfigurationManager.AppSettings["ImgUploadUrl2"], "mp3", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString());
                    //string ImageUrl = ConfigurationManager.AppSettings["ImageUrl2"] + "mp3/" + DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString();
                    Guid guid = Guid.NewGuid();
                    string fileName = guid.ToString() + ".jpg";
                    //if (!Directory.Exists(ImgUploadUrl))
                    //{
                    //    Directory.CreateDirectory(ImgUploadUrl);
                    //}
                    // string savepath = Path.Combine(ImgUploadUrl, fileName);
                    //if (!File.Exists(savepath))
                    //{
                    //    img.Save(savepath);
                    //}
                    //封面图,传到OSS
                    string aliTempImgKey = string.Empty;
                    string songPic = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKey);
                    byte[] imgBytes = Utility.ImgHelper.ImageToBytes(img);
                    bool putResult = AliOSSHelper.PutObjectFromByteArray(songPic, imgBytes, 1, ".jpg");
                    voice.SongPic = songPic;// ImageUrl + "/" + fileName; //歌曲图片

                    //带播放水印的图片,传到OSS
                    waterPic = Utility.ImgHelper.GetWaterPic(img);
                    string aliTempImgKeySharePic = string.Empty;
                    string SharePic = AliOSSHelper.GetOssImgKey("jpg", false, out aliTempImgKeySharePic);
                    byte[] imgBytesSharePic = Utility.ImgHelper.ImageToBytes(waterPic);
                    bool putResultSharePic = AliOSSHelper.PutObjectFromByteArray(SharePic, imgBytesSharePic, 1, ".jpg");
                    voice.SharePic = songPic;
                    //Image waterPic = GetWaterPic(img);
                    //  if (!string.IsNullOrEmpty(waterPic))
                    //  {
                    //      voice.SharePic = ImageUrl + "/" + waterPic; //分享图片
                    //  }
                }
                if (mp3File != null)
                {
                    //删除MP3文件
                    mp3File.Delete();
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(typeof(Mp3Lib.Mp3File), ex);
            }
            finally
            {
                if (img != null)
                {
                    img.Dispose();
                }
                if (waterPic != null)
                {
                    waterPic.Dispose();
                }
            }
        }
    }
}
