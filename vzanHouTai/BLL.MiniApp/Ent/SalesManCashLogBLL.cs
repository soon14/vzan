using DAL.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Ent
{
    public class SalesManCashLogBLL : BaseMySql<SalesManCashLog>
    {
        #region 单例模式
        private static SalesManCashLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private SalesManCashLogBLL()
        {

        }

        public static SalesManCashLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SalesManCashLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
    }
}
