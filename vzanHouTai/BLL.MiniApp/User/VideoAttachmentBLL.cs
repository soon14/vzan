using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using Utility.AliOss;

namespace BLL.MiniApp
{
    public class VideoAttachmentBLL : BaseMySql<VideoAttachment>
    {
        #region 单例模式
        private static VideoAttachmentBLL _singleModel;
        private static readonly object SynObject = new object();

        private VideoAttachmentBLL()
        {

        }

        public static VideoAttachmentBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new VideoAttachmentBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
