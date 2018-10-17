using DAL.Base;

using Entity.MiniApp.Conf;
using System.Collections.Generic;

namespace BLL.MiniApp.Conf
{
    public class Xcxtemplate_PriceBLL : BaseMySql<Xcxtemplate_Price>
    {
        #region 单例模式
        private static Xcxtemplate_PriceBLL _singleModel;
        private static readonly object SynObject = new object();

        private Xcxtemplate_PriceBLL()
        {

        }

        public static Xcxtemplate_PriceBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new Xcxtemplate_PriceBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<Xcxtemplate_Price> GetListByAgentId(int agentid)
        {
            return GetList($"agentid={agentid}");
        }

        public Xcxtemplate_Price GetModelByAgentIdAndTid(int agentid, int tid, int VersionId = 0)
        {
            string sql = $"tid={tid} and agentid={agentid} and VersionId={VersionId}";
            return GetModel(sql);
        }
    }
}
