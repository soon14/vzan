using DAL.Base;
using Entity.MiniApp;
using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
//using Core.MiniSNS;
using Utility.AliOss;
using Microsoft.Ajax.Utilities;
using static Entity.MiniApp.C_Enums;
using Entity.MiniApp.MiappTribune;

namespace BLL.MiniApp.MiappTribune
{
    public class MiniappTribuneAttachmentBLL : BaseMySql<MiniappTribuneAttachment>
    {
        /// <summary>
        /// 图片列表key，itemid，itemtype
        /// </summary>
        public static string CImageKey = "miniAppcimagekey_{0}_{1}";
        /// <summary>
        /// 图片列表key，itemid，itemtype
        /// </summary>
        public static string CPostImageKey = "miniAppcpostimagekey_{0}";
        /// <summary>
        /// 店铺附件key，storeid
        /// </summary>
        public static string CStoreImageKey = "miniAppcstoreimagekey_{0}";
      

        public override object Add(MiniappTribuneAttachment model)
        {
            object o = base.Add(model);
            RemoveRedis(model.itemId, model.itemType);
            return o;
        }
        public override int Delete(int id)
        {
            //阿里云删除
            var attachment = GetModel(id);
            if (null == attachment)
                return 0;
            var delresult = base.Delete(id);
            if (delresult > 0)
            {
                //var ossKey = attachment.filepath.Replace("http://oss.vzan.cc/", "");
                //AliOSSHelper.deleteFile(ossKey);
                RemoveRedis(attachment.itemId, attachment.itemType);
            }
            return delresult;
        }
        public override bool Update(MiniappTribuneAttachment model)
        {
            bool b = base.Update(model); 
            RemoveRedis(model.itemId, model.itemType);
            return b;
        }
        public override bool Update(MiniappTribuneAttachment model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveRedis(model.itemId, model.itemType);
            return b;
        }

        /// <summary>
        /// 查找图片列表，目前最多10张
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public List<MiniappTribuneAttachment> GetListByCache(int itemId, int itemType,bool refresh=false)
        {
            string cacheKey = string.Format(CImageKey, itemId, itemType);
            List<MiniappTribuneAttachment> cattList = RedisUtil.Get<List<MiniappTribuneAttachment>>(cacheKey);
            if ((cattList == null || cattList.Count == 0) || refresh)
            {
                cattList = GetList($"itemId={itemId} and itemType={itemType}", 20, 1);
                RedisUtil.Set(cacheKey, cattList, TimeSpan.FromDays(1));
            }
            return cattList;
        }

        /// <summary>
        /// 查找帖子的图片。
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public List<MiniappTribuneAttachment> GetPostImageByCache(int postId)
        {
            string cacheKey = string.Format(CPostImageKey, postId);
            List<MiniappTribuneAttachment> cattList = RedisUtil.Get<List<MiniappTribuneAttachment>>(cacheKey);
            if (cattList == null || cattList.Count == 0)
            {
                cattList = GetList($"itemId={postId} and itemType in ({(int)C_Enums.AttachmentItemType.信息附件},{(int)C_Enums.AttachmentItemType.帖子详情图})", 25, 1);
                RedisUtil.Set(cacheKey, cattList, TimeSpan.FromDays(1));
            }
            return cattList;
        }
        

        /// <summary>
        /// 删除缓存 --通用
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        public void RemoveRedis(int itemId, int itemType)
        {
            RedisUtil.Remove(string.Format(CImageKey, itemId, itemType));
            //清楚店铺附件缓存
            if (itemType == 0 || itemType == 8 || itemType == 9)
            {
                RedisUtil.Remove(string.Format(CStoreImageKey, itemId));
            }
            if(itemType== (int)C_Enums.AttachmentItemType.帖子详情图 || itemType == (int)C_Enums.AttachmentItemType.信息附件)
            {
                RedisUtil.Remove(string.Format(CPostImageKey, itemId));
            }
          
        }

        /// <summary>
        /// 删除缓存  -- itemType = "12,13"
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        public void RemoveRedis(int itemId, string itemType)
        {
            RedisUtil.Remove(string.Format(CImageKey, itemId, itemType));
            RedisUtil.Remove(string.Format(CStoreImageKey, itemId));
        }

        public void DeleteImg(int id)
        {
            MiniappTribuneAttachment model = GetModel(id);
            if (model != null)
            {
                Delete(id);
                RemoveRedis(model.itemId, model.itemType);
            }
        }

        //更新店铺时语音转mp3
        public async Task<string> SendVoiceToAliOss(int voiceId, int artId, int commentId, string content = "", bool needUpdateContent = false, bool isupdate = false)
        {
            string artcont = content;
            if (voiceId < 1)
                return string.Empty;
            MiniappTribuneAttachment model = GetModel(voiceId);
            if (model == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), "voiceId" + voiceId + "getmodel==null");
                return string.Empty;
            }
            model.itemId = artId;
            try
            {
                if (!string.IsNullOrEmpty(model.filepath))
                {
                    if (isupdate)
                    {
                        if (!model.filepath.Contains("temp"))//不是新上传的,停止操作！
                        {
                            log4net.LogHelper.WriteInfo(this.GetType(), "观察日志：修改文章停止旧语音移动。文章ID=" + artId + "语音路径" + model.filepath);
                            return string.Empty;
                        }
                    }
                    var bucket = ConfigurationManager.AppSettings["BucketName"];
                    int whichDomain = 1;
                    string TemplateId = string.Empty;
                    string PipelineId = string.Empty;
                    TemplateId = ConfigurationManager.AppSettings["VoiceTemplateId"] ?? "42d5aac40e6a50bf13045a40aeb83b6f";
                    PipelineId = ConfigurationManager.AppSettings["PipelineId"] ?? "4bc9dd15cb3d48e39e0824e19c41defb";
                    var finalVoiceKey = string.Empty;
                    var finalVoiceFolder = AliOSSHelper.GetOssVoiceKey("mp3", false, "voice/folder", out finalVoiceKey, whichDomain);
                    //上传的本地音频。并且不是mp3|| 微信语音
                    if (!string.IsNullOrEmpty(model.VoiceServerId))
                    {
                        //log4net.LogHelper.WriteInfo(this.GetType(), "model.DownLoadFile:"+ model.DownLoadFile+ "finalVoiceKey:" + finalVoiceKey + "bucket:" + bucket + "TemplateId:"+ TemplateId + "PipelineId:" + PipelineId );
                        //转换mp3
                        bool submitResult = await AliMTSHelper.SubmitJobs(model.filepath, finalVoiceKey, bucket, TemplateId, PipelineId, "", whichDomain);
                        if (submitResult)
                        {//转换成功。更新路径

                            model.thumbnail = finalVoiceKey;
                            //图文混排内容里的音频替换
                            if (needUpdateContent)
                            {
                                var voiceurl = model.filepath;
                                artcont = Utility.Text.CRegex.Replace(artcont, voiceurl, finalVoiceKey, 0);
                            }
                        }//音频转换失败
                        else
                        {
                            log4net.LogHelper.WriteInfo(this.GetType(), "语音给AliOSS转换格式失败！ID为" + model.id + "==" + model.filepath);
                        }
                    }//mp3移动
                    else
                    {
                        //本地音频mp3格式从temp 拷贝
                        Task<bool> moveResult = Task<bool>.Factory.StartNew(
                        () =>
                        {
                            return AliOSSHelper.CopyObect(model.thumbnail, finalVoiceKey);
                        }
                        );
                        ;
                        if (await moveResult)
                        {//移动成功。更新路径
                            model.thumbnail = finalVoiceKey;
                            //图文混排内容里的音频替换
                            if (needUpdateContent)
                            {
                                var voiceurl = model.filepath;
                                artcont = Utility.Text.CRegex.Replace(artcont, voiceurl, finalVoiceKey, 0);
                            }

                        }
                        // 移动失败
                        else
                        {
                            log4net.LogHelper.WriteInfo(this.GetType(), "本地音频AliOSS临时文件夹移动到正式文件夹失败！ID为" + model.id);
                        }

                    }
                }
                MySqlParameter[] param = new MySqlParameter[] { new MySqlParameter("@itemId",artId),
                new MySqlParameter("@filepath",model.filepath),
                new MySqlParameter("@thumbnail",model.thumbnail),
                new MySqlParameter("@id",model.id)
                };

                string sql = "update C_Attachment set itemId=@itemId,filepath=@filepath,thumbnail=@thumbnail where id=@id";
                SqlMySql.ExecuteNonQuery(Utility.dbEnum.SAS.ToString(), System.Data.CommandType.Text, sql, param);
                RedisUtil.Remove(string.Format(CImageKey, model.itemId, model.itemType));
                RemoveRedis(model.itemId, model.itemType);
                return artcont;
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception("voiceid" + model.id + "移动语音失败：" + ex.Message));
                return string.Empty;
            }


        }

        public bool UploadTask(string imgs, int itemId, C_Enums.AttachmentItemType itemType)
        {
            if (imgs.IsNullOrWhiteSpace())
                return false;

            var imgUrls = imgs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return imgUrls.Where(m => m.StartsWith("http://")).Select(img => new MiniappTribuneAttachment
            {
                itemId = itemId,
                itemType = (int)itemType,
                filepath = img,
                thumbnail = img,//Utils.GetAliImgThumbKey(img, 640, 360),
                createDate = DateTime.Now
            }).Select(attachment => Convert.ToInt32(Add(attachment)) > 0).FirstOrDefault();
        }


        /// <summary>
        /// 获取这个
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="typeIdList"></param>
        /// <returns></returns>
        public List<MiniappTribuneAttachment> GetAttachmentListByItemType(int itemId,string typeIdList)
        {
            string cacheKey = string.Format(CStoreImageKey, itemId + "_" + typeIdList);
            List<MiniappTribuneAttachment> cattList = RedisUtil.Get<List<MiniappTribuneAttachment>>(cacheKey);
            if (cattList == null || cattList.Count == 0)
            {
                cattList = GetList($"itemid={itemId} and itemtype in ({typeIdList})");
                RedisUtil.Set(cacheKey, cattList, TimeSpan.FromHours(12));
            }
            return cattList;
        }
      
    }
}