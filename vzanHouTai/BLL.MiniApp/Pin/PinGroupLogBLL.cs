using DAL.Base;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
    public class PinGroupLogBLL : BaseMySql<PinGroupLog>
    {
        #region 单例模式
        private static PinGroupLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinGroupLogBLL()
        {

        }

        public static PinGroupLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinGroupLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}