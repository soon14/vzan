using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class FNOrderRelationBLL : BaseMySql<FNOrderRelation>
    {
        #region 单例模式
        private static FNOrderRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private FNOrderRelationBLL()
        {

        }

        public static FNOrderRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FNOrderRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public FNOrderRelation GetModelOrder(int dataid, int orderid, int ordertype)
        {
            return GetModel($"orderid={orderid} and ordertype={ordertype} and dataid={dataid}");
        }
        public FNOrderRelation GetModelUOrderNo(string orderid)
        {
            return GetModel($"uniqueorderno={orderid}");
        }
        public List<FNOrderRelation> GetListByOrderIds(string orderids,int ordertype,int dataid)
        {
            return GetList($"orderid in({orderids}) and ordertype={ordertype} and dataid={dataid}");
        }
    }
}