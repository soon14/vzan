using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
   public class PlatMsgConfBLL : BaseMySql<PlatMsgConf>
    {
        #region 单例模式
        private static PlatMsgConfBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatMsgConfBLL()
        {

        }

        public static PlatMsgConfBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatMsgConfBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PlatMsgConf GetMsgConf(int aid)
        {
            return base.GetModel($"aid={aid}");
        }
    }
}
