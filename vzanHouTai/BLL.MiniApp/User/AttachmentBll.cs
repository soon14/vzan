using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.AliOss;

namespace BLL.MiniApp
{
    public class AttachmentBll : BaseMySql<Attachment>
    {
        #region 单例模式
        private static AttachmentBll _singleModel;
        private static readonly object SynObject = new object();

        private AttachmentBll()
        {

        }

        public static AttachmentBll SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AttachmentBll();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public override object Add(Attachment model)
        {
            #region 黄图检查
            Utility.AliOss.ImagePornResult imgResult = Utility.AliOss.AliyunApi.GetImageDetectionRate(model.filepath);
            if (imgResult != null && imgResult.Rate >= WebConfigBLL.ImageDetectionRate)
            {//超过这个分值的，直接全站拉黑
                if (0 != model.fid && 0 != model.userid)
                {
                    var usermodel = new OAuthUserBll(model.fid).GetUserByCache(model.userid);
                    log4net.LogHelper.WriteInfo(this.GetType(), $"黄图{model.filepath},拉黑：{usermodel.Id}");
                    new WholeBlackBLL().Add(usermodel.Id, 1, $"黄图自动拉黑{model.filepath}");
                }
            }
            //超过60分，帖子状态变为机器检测状态
            if (imgResult != null && imgResult.Rate >= 60)
            {
                model.imgwith = -1;
            }
            var score = Utility.Qcloud.QcloudAPI.FileDetection(model.filepath);
            if (score >= WebConfigBLL.ImageDetectionRate)
            {//超过这个分值的，直接全站拉黑
                if (0 != model.fid && 0 != model.userid)
                {
                    var usermodel = new OAuthUserBll(model.fid).GetUserByCache(model.userid);
                    log4net.LogHelper.WriteInfo(this.GetType(), $"黄图{model.filepath},拉黑：{usermodel.Id}");
                    new WholeBlackBLL().Add(usermodel.Id, 1, $"黄图自动拉黑{model.filepath}");
                }
            }
            //超过60分，帖子状态变为机器检测状态
            if (score >= 60)
            {
                model.imgwith = -1;
            }
            #endregion
            //log4net.LogHelper.WriteInfo(this.GetType(), imgResult.Rate.ToString());
            return base.Add(model);
        }
    }
}
