using DAL.Base;
using Entity.MiniApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class DeliveryCompanyBLL : BaseMySql<DeliveryCompany>
    {
        #region 单例模式
        private static DeliveryCompanyBLL _singleModel;
        private static readonly object SynObject = new object();

        private DeliveryCompanyBLL()
        {

        }

        public static DeliveryCompanyBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DeliveryCompanyBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<DeliveryCompany> GetCompanys()
        {
            return GetList("Title != ''", 600, 1, "*");
        }
    }
}
