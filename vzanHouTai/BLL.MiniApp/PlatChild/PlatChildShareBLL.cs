using DAL.Base;
using Entity.MiniApp.PlatChild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.PlatChild
{
  public class PlatChildShareBLL : BaseMySql<PlatChildShare>
    {
        #region 单例模式
        private static PlatChildShareBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatChildShareBLL()
        {

        }

        public static PlatChildShareBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatChildShareBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PlatChildShare GetPlatChildShare(int aid)
        {
            return base.GetModel($"Aid={aid}");
        }

    }
}
