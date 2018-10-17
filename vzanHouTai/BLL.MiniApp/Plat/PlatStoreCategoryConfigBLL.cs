using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Plat
{
    public class PlatStoreCategoryConfigBLL : BaseMySql<PlatStoreCategoryConfig>
    {
        #region 单例模式
        private static PlatStoreCategoryConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStoreCategoryConfigBLL()
        {

        }

        public static PlatStoreCategoryConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStoreCategoryConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public PlatStoreCategoryConfig GetModelByAid(int aid)
        {
            return base.GetModel($"Aid={aid}");
        }
    }
}
