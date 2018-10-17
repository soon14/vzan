using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Utility;
using Utility.AliOss;

namespace BLL.MiniApp
{
    public class C_AttachmentBLL : BaseMySql<C_Attachment>
    {
        /// <summary>
        /// 图片列表key，itemid，itemtype
        /// </summary>
        public static string CImageKey = "cimagekey_{0}_{1}";
        /// <summary>
        /// 图片列表key，itemid，itemtype
        /// </summary>
        public static string CPostImageKey = "cpostimagekey_{0}";
        /// <summary>
        /// 店铺附件key，storeid
        /// </summary>
        public static string CStoreImageKey = "C_Attachment_Store{0}";

        #region 单例模式
        private static C_AttachmentBLL _singleModel;
        private static readonly object SynObject = new object();

        private C_AttachmentBLL()
        {

        }

        public static C_AttachmentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new C_AttachmentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 添加图片到附件表
        /// </summary>
        /// <param name="Imgs"></param>
        /// <param name="attachmentItemType"></param>
        /// <param name="id"></param>
        public void AddImgList(string[] Imgs,int attachmentItemType,int id)
        {
       
                if (Imgs.Length > 0)
                {
                    foreach (var img in Imgs)
                    {
                        //判断上传图片是否以http开头，不然为破图-蔡华兴
                        if (!string.IsNullOrWhiteSpace(img) && img.IndexOf("http") == 0)
                        {
                            Add(new C_Attachment
                            {
                                itemId = id,
                                createDate = DateTime.Now,
                                filepath = img,
                                itemType = attachmentItemType,
                                thumbnail = img,
                                status = 0
                            });
                        }

                    }

                }
            
        }

        public override object Add(C_Attachment model)
        {
            object o = base.Add(model);
            RemoveRedis(model.itemId, model.itemType);
            return o;
        }

        //不再物理删除数据,而是把状态改为 -1
        public override int Delete(int id)
        {
            //阿里云删除 --阿里已经没有删除
            var attachment = GetModel(id);
            if (null == attachment)
                return 0;

            attachment.status = -1;
            bool sucess = base.Update(attachment, "status");
            //var delresult = base.Delete(id);
            if (sucess)
            {
                //var ossKey = attachment.filepath.Replace("http://oss.vzan.cc/", "");
                //AliOSSHelper.deleteFile(ossKey);
                RemoveRedis(attachment.itemId, attachment.itemType);
            }
            return 1;
        }

        /// <summary>
        /// 删除（状态改为-1）
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        public void DeleteALL(int itemId,int itemType)
        {
            string sql = $"update C_Attachment set status=-1 where itemId={itemId} and itemType={itemType} and status = 0";
            int result = SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, sql);
            if (result>0)
            {
                RemoveRedis(itemId, itemType);
            }
        }

        public override bool Update(C_Attachment model)
        {
            bool b = base.Update(model); 
            RemoveRedis(model.itemId, model.itemType);
            return b;
        }
        public override bool Update(C_Attachment model, string columnFields)
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
        public List<C_Attachment> GetListByCache(int itemId, int itemType,bool refresh=false)
        {
            string cacheKey = string.Format(CImageKey, itemId, itemType);
            List<C_Attachment> cattList = RedisUtil.Get<List<C_Attachment>>(cacheKey);
            if ((cattList == null || cattList.Count == 0) || refresh)
            {
                cattList = GetList($"itemId={itemId} and itemType={itemType} and status = 0", 50, 1);
                RedisUtil.Set(cacheKey, cattList, TimeSpan.FromDays(1));
            }
            return cattList;
        }
        

        /// <summary>
        /// 查找店铺附件
        /// </summary>
        /// <param name="storeid"></param>
        /// <returns></returns>
        public List<C_Attachment> GetStoreImgByCache(int storeid)
        {
            string cacheKey = string.Format(CStoreImageKey, storeid);
            List<C_Attachment> cattList = RedisUtil.Get<List<C_Attachment>>(cacheKey);
            if (cattList == null || cattList.Count == 0)
            {
                cattList = GetList($"itemId={storeid} and itemType in (0,8,9) and status = 0 ", 50, 1);
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

        //不再物理删除数据,而是将状态改为-1
        public void DeleteImg(int id)
        {
            C_Attachment model = GetModel(id);
            if (model != null)
            {
                model.status = -1;
                base.Update(model, "status");
                RemoveRedis(model.itemId, model.itemType);
            }
        }

        //更新店铺时语音转mp3
        public async Task<string> SendVoiceToAliOss(int voiceId, int artId, int commentId, string content = "", bool needUpdateContent = false, bool isupdate = false)
        {
            string artcont = content;
            if (voiceId < 1)
                return string.Empty;
            C_Attachment model = GetModel(voiceId);
            if (model == null)
            {
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
                        //转换mp3
                        bool submitResult = await AliMTSHelper.SubmitJobs(model.filepath, finalVoiceKey, bucket, TemplateId, PipelineId, "", whichDomain);
                        if (submitResult)
                        {//转换成功。更新路径

                            model.thumbnail = finalVoiceKey;
                            //图文混排内容里的音频替换
                            if (needUpdateContent)
                            {
                                var voiceurl = model.filepath;
                                artcont =  CRegex.Replace(artcont, voiceurl, finalVoiceKey, 0);
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
                                artcont =  CRegex.Replace(artcont, voiceurl, finalVoiceKey, 0);
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
                SqlMySql.ExecuteNonQuery(Utility.dbEnum.MINIAPP.ToString(), System.Data.CommandType.Text, sql, param);
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
        //足浴版获取店铺图片
        public List<C_Attachment> GetFootbathStorePic(int storeId, int picType, string photoIds)
        {
            string sqlwhere = $"itemid={storeId} and itemtype={picType} and status = 0 ";
            if (!string.IsNullOrEmpty(photoIds))
            {
                sqlwhere += $" and id not in ({photoIds})";
            }
            return GetList(sqlwhere);
        }


        //更新店铺时语音转mp3
        public async Task<string> SendVoiceToAliOss(int itemId, int itemType, string sourceTempUrl)
        {
            if (itemId <= 0 || itemType <= 0 || string.IsNullOrEmpty(sourceTempUrl))
                return string.Empty;
            C_Attachment model = GetModelByType(itemId, itemType);
            if (model == null)
            {
                model = new C_Attachment();
                model.itemId = itemId;
                model.itemType = itemType;
                model.createDate = DateTime.Now;
                model.thumbnail = "";
            }
            model.filepath = sourceTempUrl;
            try
            {
                if (!model.filepath.Contains("temp"))//不是新上传的,停止操作！
                {
                    return string.Empty;
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
                    //转换mp3
                    bool submitResult = await AliMTSHelper.SubmitJobs(model.filepath, finalVoiceKey, bucket, TemplateId, PipelineId, "", whichDomain);
                    if (submitResult)
                    {//转换成功。更新路径
                        model.thumbnail = finalVoiceKey;
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
                        return AliOSSHelper.CopyObect(model.filepath ?? "", finalVoiceKey);
                    }
                    );
                    if (await moveResult)
                    {//移动成功。更新路径
                        model.thumbnail = finalVoiceKey;
                    }
                    // 移动失败
                    else
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "本地音频AliOSS临时文件夹移动到正式文件夹失败！ID为" + sourceTempUrl);
                        return "";
                    }

                }
                if (model.id == 0)
                {
                    Add(model);
                }
                else
                {
                    MySqlParameter[] param = new MySqlParameter[] { new MySqlParameter("@itemId",itemId),
                        new MySqlParameter("@filepath",model.filepath),
                        new MySqlParameter("@thumbnail",model.thumbnail),
                        new MySqlParameter("@id",model.id)
                    };
                    string sql = "update C_Attachment set itemId=@itemId,filepath=@filepath,thumbnail=@thumbnail where id=@id";
                    SqlMySql.ExecuteNonQuery(Utility.dbEnum.MINIAPP.ToString(), System.Data.CommandType.Text, sql, param);
                    RemoveRedis(model.itemId, model.itemType);
                }
                return "";
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), new Exception("voiceid" + model.id + "移动语音失败：" + ex.Message));
                return string.Empty;
            }
        }
        
        public bool UploadTask(string imgs, int itemId, AttachmentItemType itemType)
        {
            if (imgs.IsNullOrWhiteSpace())
                return false;

            var imgUrls = imgs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return imgUrls.Where(m => m.StartsWith("http://")).Select(img => new C_Attachment
            {
                itemId = itemId,
                itemType = (int)itemType,
                filepath = img,
                thumbnail = img,//Utility.AliOss.AliOSSHelper.GetAliImgThumbKey(img, 640, 360),
                createDate = DateTime.Now
            }).Select(attachment => Convert.ToInt32(Add(attachment)) > 0).FirstOrDefault();
        }


        /// <summary>
        /// 获取这个
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="typeIdList"></param>
        /// <returns></returns>
        public List<C_Attachment> GetAttachmentListByItemType(int itemId,string typeIdList)
        {
            string cacheKey = string.Format(CStoreImageKey, itemId + "_" + typeIdList);
            List<C_Attachment> cattList = RedisUtil.Get<List<C_Attachment>>(cacheKey);
            if (cattList == null || cattList.Count == 0)
            {
                cattList = GetList($"itemid={itemId} and itemtype in ({typeIdList}) and status = 0");
                RedisUtil.Set(cacheKey, cattList, TimeSpan.FromHours(12));
            }
            return cattList;
        }

        public C_Attachment GetModelByType(int itemId, int itemType)
        {
            return GetModel($"itemid={itemId} and itemtype ={itemType} and status = 0 ");
        }

    }
}