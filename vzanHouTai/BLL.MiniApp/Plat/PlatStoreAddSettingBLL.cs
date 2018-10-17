using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
   public class PlatStoreAddSettingBLL : BaseMySql<PlatStoreAddSetting>
    {
        #region 单例模式
        private static PlatStoreAddSettingBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStoreAddSettingBLL()
        {

        }

        public static PlatStoreAddSettingBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStoreAddSettingBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PlatStoreAddSetting GetPlatStoreAddSetting(int aid)
        {
            return base.GetModel($"Aid={aid}");
        }

    }
}
