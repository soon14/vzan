using DAL.Base;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
   public class CityStoreBannerBLL : BaseMySql<CityStoreBanner>
    {
        #region 单例模式
        private static CityStoreBannerBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityStoreBannerBLL()
        {

        }

        public static CityStoreBannerBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityStoreBannerBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public CityStoreBanner getModelByaid(int aid)
        {
            return base.GetModel($"aid={aid}");
        }

    }
}
