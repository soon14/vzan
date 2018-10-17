using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class AgentDistributionRuleBLL : BaseMySql<AgentDistributionRule>
    {
        #region 单例模式
        private static AgentDistributionRuleBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentDistributionRuleBLL()
        {

        }

        public static AgentDistributionRuleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentDistributionRuleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
