using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp.User;
using Entity.MiniApp; 
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Utility.AliOss;
using Utility;

namespace User.MiniApp.Handler
{
    /// <summary>
    /// UploadAudioHandler 的摘要说明
    /// </summary>
    public class UploadAudioHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string name = context.Request["name"];
            string serverid = "";
            var fid = Utility.IO.Context.GetRequestInt("minsnsId",0);
            var accid = Utility.IO.Context.GetRequest("accountId","");
            int voicetime = 0,voicetype = 0,convertstate=0;
            int.TryParse(context.Request["voicetime"], out voicetime);
            int.TryParse(context.Request["voicetype"], out voicetype);
            if (voicetype ==1) {  //1录音文件 0本地音频
                serverid="app_" + DateTime.Now.ToString(); //前端显示需要用到serverid
                convertstate = 1;  // 前端显示需要用到 标识已转换成功
            }
            Guid _accountId=Guid.Empty;
            try
            {
                if (string.IsNullOrEmpty(accid))
                    accid = CookieHelper .GetCookie("UserCookieNew");
                    _accountId = Guid.Parse(accid);
                
            }
            catch (Exception )
            {

                log4net.LogHelper.WriteInfo(this.GetType(), "guid转换失败，上传音频_accountId="+ accid);
                context.Response.Write(new JavaScriptSerializer().Serialize(new { result = 0, msg = "账号信息错误！" }));
            }
            /*如果有openId，发帖人用微信帐号 xiaowei 2015-10-27 11:21:54*/
            Account account = AccountBLL.SingleModel.GetModel(_accountId);
            if(null==account)
                context.Response.Write(new JavaScriptSerializer().Serialize(new { result = 0, msg = "账号信息错误！" }));
            OAuthUser artUser = null;
            int userid = 0;
            if (null != account && !string.IsNullOrEmpty(account.OpenId))
            {
                artUser = new OAuthUserBll(fid).GetUserByOpenId(account.OpenId, fid);
                userid = artUser.Id;
            }
            
            HttpPostedFile file = context.Request.Files[name];
            string time = string.Empty;
            string fileType = System.IO.Path.GetExtension(file.FileName).ToLower();
            List<string> extents = new List<string> { ".cda", ".wav", ".mp3", ".wma", ".ra", ".midi", ".ogg", ".ape", ".flac", ".aac", ".amr" };

            if (!extents.Contains(fileType))
            {
                context.Response.Write(new JavaScriptSerializer().Serialize(new { result = 0, msg = "音频格式不对" }));
            }
            int convertState = 1;
            var size =file.ContentLength;
            string aliTempImgKey = string.Empty;
            if (fileType != ".mp3")
            {
                convertState = -9;

            }
            var filePath = @"\\share3.vzan.cc\share\temp\pc" + DateTime.Now.ToFileTime().ToString() + fileType;
            file.SaveAs(filePath);
            var fileStream = file.InputStream;
            var byteData = new byte[size];
            if (null != fileStream)
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fileStream))
                {
                    byteData = br.ReadBytes(size);
                }
              
                //同步到AliOss
                //上传的目录
                var aliTempImgFolder = AliOSSHelper.GetOssVoiceKey(fileType.Replace(".",""), true, "voice/folder", out aliTempImgKey);
                var putResult = AliOSSHelper.PutObjectFromByteArray(aliTempImgFolder, byteData, 1, fileType);
                if (!putResult)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "语音同步到Ali失败！");
                }
            }// 上传的文件为空
            else
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "fileStream为null!");
                context.Response.Write(new JavaScriptSerializer().Serialize(new { error = aliTempImgKey, msg = "上传失败" }));
            }
          
            Voices voice = new Voices();
            voice.ServerId = "";
            voice.MessageText = "";
            voice.DownLoadFile = aliTempImgKey;
            voice.TransFilePath = aliTempImgKey;
            voice.UserId = userid;
            voice.FId = fid;
            voice.VoiceTime = voicetime;
            voice.VoiceType = voicetype;
            voice.ServerId = serverid;  //录音文件
            voice.ConvertState = convertstate; //1 已转换 0为转换
            voice.CreateDate = DateTime.Now;

            if (convertState == 1)//是mp3--获取音频信息必须保存到本地才能获得
            {
                HostFile.GetVoiceFromPath(ref voice, filePath);
            }
            else
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (string.IsNullOrEmpty(voice.SongName))
            {
                voice.SongName = file.FileName;
            }
            if (string.IsNullOrEmpty(voice.SongPic))
            {
                voice.SongPic="//j.vzan.cc/manager/images/yinping.jpg?v=0.1";
            }
            
            int voiceId = Convert.ToInt32(VoicesBll.SingleModel.Add(voice));
            if (voiceId > 0)
            {
                context.Response.Write(new JavaScriptSerializer().Serialize(new { result = 1, msg = "上传成功", time = voice.VoiceTime, url = aliTempImgKey, songpic=voice.SongPic, songname=voice.SongName, singer=voice.Singer, album=voice.Album, id = voiceId, createdate = voice.CreateDate.ToString("yyyy-MM-dd") }));
            }
            else
            {
                context.Response.Write(new JavaScriptSerializer().Serialize(new { result = 0, msg = "上传错误" }));
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