using DAL.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utility.AliOss;

namespace BLL.MiniApp
{
    public class C_AttachmentVideoBLL : BaseMySql<C_AttachmentVideo>
    {
        /// <summary>
        /// 视频列表key，itemid，itemtype
        /// </summary>
        public const string CVideoKey = "cvideokey_{0}_{1}";

        #region 单例模式
        private static C_AttachmentVideoBLL _singleModel;
        private static readonly object SynObject = new object();

        private C_AttachmentVideoBLL()
        {

        }

        public static C_AttachmentVideoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new C_AttachmentVideoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public override object Add(C_AttachmentVideo model)
        {
            object o = base.Add(model);
            RemoveRedis(model.itemId, model.itemType);
            return o;
        }

        /// <summary>
        /// 查找图片列表，目前最多10张
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public List<C_AttachmentVideo>  GetListByCache(int itemId,int itemType)
        {
            string cacheKey = string.Format(CVideoKey, itemId,itemType);
            List<C_AttachmentVideo> cattList = RedisUtil.Get<List<C_AttachmentVideo>>(cacheKey);
            if(cattList==null || cattList.Count==0)
            {
                cattList = GetList($"itemId={itemId} and itemType={itemType}", 10, 1);
                if(cattList != null && cattList.Count > 0)
                {
                    RedisUtil.Set(cacheKey, cattList, TimeSpan.FromDays(1));
                }
            }
            return cattList;
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        public void RemoveRedis(int itemId, int itemType)
        {
            RedisUtil.Remove(string.Format(CVideoKey, itemId, itemType));
        }

        public void DeleteImg(int id)
        {
            C_AttachmentVideo model = GetModel(id);
            if (model != null)
            {
                Delete(id);
                RemoveRedis(model.itemId, model.itemType);
            }
        }




        public bool UpdateVideoStaus(int statusValue, int vId)
        {
            string strSql = string.Format("update c_attachmentvideo set status={0} where id={1}", statusValue, vId);
            return base.ExecuteTransaction(strSql);
        }

        public override bool Update(C_AttachmentVideo model)
        {
            bool  b = base.Update(model);
            RemoveRedis(model.itemId, model.itemType);
            return b;
        }

        public override bool Update(C_AttachmentVideo model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RemoveRedis(model.itemId, model.itemType);
            return b;
        }


        /// <summary>
        /// 发帖成功后，视频处理，临时文件拷贝到正式文件， 如果要转码，需要进行转码
        /// 目前一个视频附件只能有一个
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemType"></param>
        /// <param name="videoKey"></param>
        /// <param name="cityInfoId"></param>
        public async Task<bool> HandleVideo(int itemId, int itemType, string videoKey_Org, int cityInfoId = 0)
        {
            var cVideo = GetModel($"itemId={itemId} and itemType={itemType}");
            if (cVideo == null)
            {
                cVideo = new C_AttachmentVideo();
            }
            cVideo.createDate = DateTime.Now;
            cVideo.itemId = itemId;
            cVideo.itemType = itemType;// (int)C_Enums.AttachmentVideoType.店铺动态视频;
            cVideo.sourceFilePath = videoKey_Org;
            cVideo.status = 1;
            cVideo.cityCode = cityInfoId;
            List<string> extents = new List<string> { "mp4", "ogg", "mpeg4", "webm", "MP4", "OGG", "MPEG4", "WEBM" };
            string fileType = "mp4";
            string[] keyExt = videoKey_Org.Split('.');
            if (keyExt.Length > 1)
            {
                fileType = keyExt[1];
            }

            //无需转换 , 直接COPY
            if (extents.Contains(fileType))
            {
                var videokey = videoKey_Org.Replace("temp/", "");
                //临时目录移动
                var moveResult = AliOSSHelper.CopyObect(videoKey_Org, videokey, ".mp4");

                cVideo.convertFilePath = VideoAliMtsHelper.GetUrlFromKey(videoKey_Org.Replace("temp/", ""));
                //AliOss视频copy成功
                if (true == moveResult)
                {
                    //获取封面图
                    var videoImgUrl = string.Empty;
                    var videoImgKey = AliOSSHelper.GetOssImgKey("jpg", false, out videoImgUrl);
                    await VideoAliMtsHelper.SubMitVieoSnapshotJob(videoKey_Org, videoImgKey);
                    cVideo.videoPosterPath = videoImgUrl;
                }
                else
                {
                    log4net.LogHelper.WriteInfo(typeof(VideoAttachmentBLL), "移动视频失败tempPath= " + videoKey_Org);
                }
            }
            else//需要转码
            {
                var regex = new System.Text.RegularExpressions.Regex(@"(?i)\.[\S]*");
                var convertUrl = VideoAliMtsHelper.GetUrlFromKey(regex.Replace(videoKey_Org.Replace("temp/", ""), ".mp4"));
                //提交给Ali转码
                var covertResult = await VideoAliMtsHelper.SubMitVieoJob(videoKey_Org, convertUrl);
                cVideo.convertFilePath = convertUrl;
                if (covertResult)
                {
                    //获取封面图
                    var videoImgUrl = string.Empty;
                    var videoImgKey = AliOSSHelper.GetOssImgKey("jpg", false, out videoImgUrl);
                    await VideoAliMtsHelper.SubMitVieoSnapshotJob(videoKey_Org, videoImgKey);
                    cVideo.videoPosterPath = videoImgUrl;
                }
                else
                {
                    log4net.LogHelper.WriteInfo(typeof(VideoAttachmentBLL), "提交转码视频失败tempPath= " + videoKey_Org);
                }
            }
            if (cVideo.id == 0)
            {
                Add(cVideo);
            }
            else
            {
                Update(cVideo);
            }
            return true;
        }

        /// <summary>
        /// 更新店铺动态视频所属店铺动态id以及转移视频和图片-蔡华兴
        /// </summary>
        /// <param name="videopath"></param>
        /// <param name="oldhvid"></param>
        /// <param name="strategyId"></param>
        /// <param name="videotype"></param>
        /// <returns></returns>
        public async Task<bool> HandleVideoLogicStrategyAsync(string videopath,int oldhvid,int strategyId,int videotype)
        {
            int result = 0;
            try
            {
                if (!string.IsNullOrEmpty(videopath))
                {
                    //清除老视频
                    if (oldhvid > 0)
                    {
                        C_AttachmentVideo cattV = GetModel(oldhvid);
                        if (cattV != null)
                        {
                            Delete(oldhvid);
                            RemoveRedis(cattV.itemId, cattV.itemType);//清除缓存
                        }
                    }
                    C_AttachmentVideo cVideo = new C_AttachmentVideo();
                    List<string> extents = new List<string> { "mp4", "ogg", "mpeg4", "webm", "MP4", "OGG", "MPEG4", "WEBM" };
                    string fileType = videopath.Substring(videopath.LastIndexOf('.') + 1);
                    
                    //判断是否需要转码
                    if (!extents.Contains(fileType))
                    {
                        var regex = new Regex(@"(?i)\.[\S]*");
                        var tempconverpath = regex.Replace(videopath.Replace("temp/", "").Replace(fileType, "mp4"), ".mp4");
                        cVideo.convertFilePath = VideoAliMtsHelper.GetUrlFromKey(tempconverpath);
                        
                        //转码
                        await VideoAliMtsHelper.SubMitVieoJob(videopath, cVideo.convertFilePath);

                        //获取封面图
                        var videoImgUrl = string.Empty;
                        var videoImgKey = AliOSSHelper.GetOssImgKey("jpg", false, out videoImgUrl);
                        await VideoAliMtsHelper.SubMitVieoSnapshotJob(tempconverpath, videoImgKey);

                        cVideo.itemId = strategyId;
                        cVideo.status = 1;
                        cVideo.createDate = DateTime.Now;
                        cVideo.itemType = videotype;//;
                        cVideo.sourceFilePath = videopath;//视频路径
                        cVideo.videoPosterPath = videoImgUrl;//封面

                        result = Convert.ToInt32(Add(cVideo));
                    }
                    else
                    {
                        var videokey = videopath.Replace("temp/", "");
                        //临时目录移动
                        var moveResult = AliOSSHelper.CopyObect(videopath, videokey, ".mp4");
                        if (!moveResult)
                        {
                            return false;
                        }
                        //AliOss视频copy成功
                        else
                        {
                            //获取封面图
                            var videoImgUrl = string.Empty;
                            var videoImgKey = AliOSSHelper.GetOssImgKey("jpg", false, out videoImgUrl);
                            await VideoAliMtsHelper.SubMitVieoSnapshotJob(videokey, videoImgKey);

                            cVideo.itemId = strategyId;
                            cVideo.status = 1;
                            cVideo.createDate = DateTime.Now;
                            cVideo.itemType = videotype;
                            cVideo.sourceFilePath = videokey;//视频路径
                            var regex = new Regex(@"(?i)\.[\S]*");
                            cVideo.convertFilePath = VideoAliMtsHelper.GetUrlFromKey(regex.Replace(videopath.Replace("temp/", ""), ".mp4"));
                            cVideo.videoPosterPath = videoImgUrl;//封面

                            result = Convert.ToInt32(Add(cVideo));
                            
                        }
                    }
                }
            }
            catch (Exception)
            {
               
            }

            return result>0;
        }
        /// <summary>
        /// 转码-蔡华兴
        /// </summary>
        /// <param name="videopath"></param>
        /// <param name="oldhvid"></param>
        /// <param name="strategyId"></param>
        /// <returns></returns>
        public async Task<Tuple<string,string>> GetConvertVideoPathAsync(string videopath)
        {
            string tempconverpath = string.Empty;
            string convertFilePath = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(videopath))
                {
                    C_AttachmentVideo cVideo = new C_AttachmentVideo();
                    List<string> extents = new List<string> { "mp4", "ogg", "mpeg4", "webm", "MP4", "OGG", "MPEG4", "WEBM" };
                    string fileType = videopath.Substring(videopath.LastIndexOf('.') + 1);

                    
                    //判断是否需要转码
                    if (!extents.Contains(fileType))
                    {
                        var regex = new Regex(@"(?i)\.[\S]*");
                        tempconverpath = regex.Replace(videopath.Replace(fileType, "mp4"), ".mp4");
                        convertFilePath = VideoAliMtsHelper.GetUrlFromKey(tempconverpath);
                        //转码
                        await VideoAliMtsHelper.SubMitVieoJob(videopath, convertFilePath);

                        //获取封面图
                        //var videoImgUrl = string.Empty;
                        //var videoImgKey = AliOSSHelper.GetOssImgKey("jpg", false, out videoImgUrl);
                        //VideoAliMtsHelper.SubMitVieoSnapshotJob(tempconverpath, videoImgKey);
                       
                    }
                    else
                    {
                        convertFilePath = VideoAliMtsHelper.GetUrlFromKey(videopath);
                    }
                }
            }
            catch (Exception )
            {

            }

            return Tuple.Create(convertFilePath, tempconverpath);
        }       
        
    }
}