using DAL.Base;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.cityminiapp
{
   public class CityMaterialsBLL : BaseMySql<CityMaterials>
    {
        #region 单例模式
        private static CityMaterialsBLL _singleModel;
        private static readonly object SynObject = new object();

        private CityMaterialsBLL()
        {

        }

        public static CityMaterialsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CityMaterialsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<CityMaterials> getListByaid(int aid,out int totalCount, int pageSize=10,int pageIndex=1,string orderWhere="addTime desc",int storeId = 0)
        {
            string strWhere = $"aid={aid} and state=0 and storeId = {storeId} ";
            totalCount = base.GetCount(strWhere);
            return base.GetList(strWhere,pageSize,pageIndex,"*",orderWhere);
        }

        public List<CityMaterials> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<CityMaterials>();

            string sqlwhere = $" id in ({ids})";
            return GetList(sqlwhere);
        }
    }
}
