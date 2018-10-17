using DAL.Base;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
  public  class PinAgentChangeLogBLL : BaseMySql<PinAgentChangeLog>
    {
        #region 单例模式
        private static PinAgentChangeLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinAgentChangeLogBLL()
        {

        }

        public static PinAgentChangeLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinAgentChangeLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }

        #endregion

        public List<PinAgentChangeLog> GetListByAgentId(int agentId,int aid,out int totalCount, int pageIndex=1,int pageSize=10)
        {
            totalCount = 0;
            string strWhere = $"aid={aid} and agentid={agentId}";
            List<PinAgentChangeLog> list = base.GetList(strWhere, pageSize, pageIndex);
            if (list != null && list.Count > 0)
            {
                totalCount = base.GetCount(strWhere);
            }

            return list;

        }


    }
}
