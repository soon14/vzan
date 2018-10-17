using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatUserCashBLL : BaseMySql<PlatUserCash>
    {
        #region 单例模式
        private static PlatUserCashBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatUserCashBLL()
        {

        }

        public static PlatUserCashBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatUserCashBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PlatUserCash GetModelByUserId(int aid,long userid)
        {
            return base.GetModel($"aid={aid} and userid={userid} and state=0");
        }
    }
}
