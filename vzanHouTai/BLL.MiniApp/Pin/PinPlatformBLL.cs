using DAL.Base;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Pin
{
    public class PinPlatformBLL : BaseMySql<PinPlatform>
    {
        #region 单例模式
        private static PinPlatformBLL _singleModel;
        private static readonly object SynObject = new object();

        private PinPlatformBLL()
        {

        }

        public static PinPlatformBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PinPlatformBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PinPlatform GetModelByAid(int aid)
        {
            PinPlatform platform = null;
            if (aid <= 0)
            {
                return platform;
            }
            string sqlwhere = $" aid={aid}";
            platform = GetModel(sqlwhere);
            return platform;
        }
    }
}