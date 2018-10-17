using DAL.Base;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
   public class CityMsgViewFavoriteShareBLL : BaseMySql<CityMsgViewFavoriteShare>
    {
        #region 单例模式
        private static CityMsgViewFavoriteShareBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityMsgViewFavoriteShareBLL()
        {

        }

        public static CityMsgViewFavoriteShareBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityMsgViewFavoriteShareBLL();
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
        public CityMsgViewFavoriteShare getModelByMsgId(int msgId)
        {
            return base.GetModel($"msgId={msgId}");
        }

        public List<CityMsgViewFavoriteShare> GetListByMsgIds(string msgIds)
        {
            if (string.IsNullOrEmpty(msgIds))
                return new List<CityMsgViewFavoriteShare>();

            return base.GetList($"msgId in ({msgIds}");
        }
    }
}
