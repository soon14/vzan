using DAL.Base;
using Entity.MiniApp.Qiye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Qiye
{
   public class QiyeMsgviewFavoriteShareBLL : BaseMySql<QiyeMsgviewFavoriteShare>
    {
        #region 单例模式
        private static QiyeMsgviewFavoriteShareBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeMsgviewFavoriteShareBLL()
        {

        }

        public static QiyeMsgviewFavoriteShareBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeMsgviewFavoriteShareBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 根据帖子ID获取其浏览量-收藏量-分享量数据
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public QiyeMsgviewFavoriteShare GetModelByMsgId(int aid,int msgId,int dataType)
        {
            return base.GetModel($"aid = {aid} and msgId={msgId} and datatype={dataType}");
        }

        public List<QiyeMsgviewFavoriteShare> GetListByMsgIds(int aid,string msgIds,int dataType)
        {
            if(string.IsNullOrEmpty(msgIds))
            {
                return new List<QiyeMsgviewFavoriteShare>();
            }
            return base.GetList($"aid = {aid} and msgId in({msgIds}) and datatype={dataType}");
        }
    }
}
