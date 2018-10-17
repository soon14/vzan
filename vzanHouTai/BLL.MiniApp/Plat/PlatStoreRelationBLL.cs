using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{

    /// <summary>
    /// 
    /// </summary>
   public class PlatStoreRelationBLL : BaseMySql<PlatStoreRelation>
    {
        #region 单例模式
        private static PlatStoreRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStoreRelationBLL()
        {

        }

        public static PlatStoreRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStoreRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 从PlatStoreRelation获取本平台入驻的店铺不包括同步过来的
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public PlatStoreRelation GetPlatStoreRelationOwner(int aid,int storeId)
        {
            return base.GetModel($"Aid={aid} and StoreId={storeId}");
        }

        public List<PlatStoreRelation> GetListPlatStoreRelation(string ids)
        {
            return base.GetList($"Id in({ids})");
        }


    }
}
