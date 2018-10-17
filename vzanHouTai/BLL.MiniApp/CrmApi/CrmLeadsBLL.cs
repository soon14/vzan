using BLL.MiniApp.Conf;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.CrmApi;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.CrmApi
{
    public class CrmLeadsBLL : BaseMySql<CrmLeads>
    {
        #region 单例模式
        private static CrmLeadsBLL _singleModel;
        private static readonly object SynObject = new object();

        private CrmLeadsBLL()
        {

        }

        public static CrmLeadsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new CrmLeadsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public CrmLeads GetModelLeadId(int id)
        {
            return base.GetModel($"LeadId={id}");
        }
    }
}
