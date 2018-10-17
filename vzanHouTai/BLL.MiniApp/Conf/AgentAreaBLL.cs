using DAL.Base;
using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Conf
{
    public class AgentAreaBLL : BaseMySql<AgentArea>
    {
        #region 单例模式
        private static AgentAreaBLL _singleModel;
        private static readonly object SynObject = new object();

        private AgentAreaBLL()
        {

        }

        public static AgentAreaBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AgentAreaBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
