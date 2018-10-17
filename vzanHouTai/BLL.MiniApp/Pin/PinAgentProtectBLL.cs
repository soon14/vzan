using DAL.Base;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
    public class PinAgentProtectBLL : BaseMySql<PinAgentProtect>
    {
        #region 单例模式
        private static PinAgentProtectBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinAgentProtectBLL()
        {

        }

        public static PinAgentProtectBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinAgentProtectBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PinAgentProtect GetModelByUserId_State(int userId,int state=0)
        {
            string sqlwhere = $"userid={userId} and state={state}";
            return GetModel(sqlwhere);

        }
    }
}
