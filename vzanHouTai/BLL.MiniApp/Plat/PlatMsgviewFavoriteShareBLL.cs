using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
   public class PlatMsgviewFavoriteShareBLL : BaseMySql<PlatMsgviewFavoriteShare>
    {
        #region 单例模式
        private static PlatMsgviewFavoriteShareBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMsgviewFavoriteShareBLL()
        {

        }

        public static PlatMsgviewFavoriteShareBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMsgviewFavoriteShareBLL();
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
        public PlatMsgviewFavoriteShare GetModelByMsgId(int aid,int msgId,int datatype)
        {
            return base.GetModel($"aid = {aid} and msgId={msgId} and datatype={datatype}");
        }

        public List<PlatMsgviewFavoriteShare> GetListByCardIds(int aid,string msgids,int datatype)
        {
            if(string.IsNullOrEmpty(msgids))
            {
                return new List<PlatMsgviewFavoriteShare>();
            }
            return base.GetList($"aid = {aid} and msgId in({msgids}) and datatype={datatype}");
        }
    }
}
