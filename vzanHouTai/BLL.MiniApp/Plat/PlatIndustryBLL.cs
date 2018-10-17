using DAL.Base;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Plat
{
    public class PlatIndustryBLL : BaseMySql<PlatIndustry>
    {
        #region 单例模式
        private static PlatIndustryBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatIndustryBLL()
        {

        }

        public static PlatIndustryBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatIndustryBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<PlatIndustry> GetListData(int state=0)
        {
            string sql = $"state>={state}";
            return base.GetList(sql);
        }

        public List<PlatIndustry> GetListByIds(string ids)
        {
            if(string.IsNullOrEmpty(ids))
            {
                return new List<PlatIndustry>();
            }

            return base.GetList($"id in ({ids})");
        }
    }
}
