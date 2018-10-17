using System;
using DAL.Base;
using Entity.MiniApp.Footbath;


namespace BLL.MiniApp.Footbath
{
    public class FootbathXcxRelationBLL : BaseMySql<FootbathXcxRelation>
    {
        #region 单例模式
        private static FootbathXcxRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private FootbathXcxRelationBLL()
        {

        }

        public static FootbathXcxRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FootbathXcxRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 根据技师端aid获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FootbathXcxRelation GetModelByTechnicianAid(int tecahnicianAid)
        {
            if (tecahnicianAid <= 0)
            {
                return null;
            }
            return GetModel($" technicianAid={tecahnicianAid} and state>=0");
        }

        /// <summary>
        /// 根据客户端aid获取数据
        /// </summary>
        /// <param name="clientAid"></param>
        /// <returns></returns>
        public FootbathXcxRelation GetModelByClientAid(int clientAid)
        {
            if (clientAid <= 0)
            {
                return null;
            }
            return GetModel($"clientaid={clientAid} and state>=0");
        }
        /// <summary>
        /// 根据技师端aid，客户端aid，accountId获取数据
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="technicianAid"></param>
        /// <param name="clientAid"></param>
        /// <returns></returns>
        public FootbathXcxRelation GetModelByTechnicianAidAndClientAid(string accountId, int technicianAid, int clientAid)
        {
            return GetModel($"accountid='{accountId}' and technicianaid={technicianAid} and clientaid={clientAid}");
        }
        /// <summary>
        /// 根据技师端aid，客户端aid，accountId获取有效数据
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="technicianAid"></param>
        /// <param name="clientAid"></param>
        /// <returns></returns>
        public FootbathXcxRelation GetValidModel(string accountId, int technicianAid, int clientAid)
        {
            return GetModel($"accountid='{accountId}' and clientaid={clientAid} and technicianaid={technicianAid} and state>=0");
        }
    }
}
