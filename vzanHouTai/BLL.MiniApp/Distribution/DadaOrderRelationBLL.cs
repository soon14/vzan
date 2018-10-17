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
    public class DadaOrderRelationBLL : BaseMySql<DadaOrderRelation>
    {
        #region 单例模式
        private static DadaOrderRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private DadaOrderRelationBLL()
        {

        }

        public static DadaOrderRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DadaOrderRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DadaOrderRelation GetModelOrder(int dataid, int orderid, int ordertype)
        {
            return GetModel($"orderid={orderid} and ordertype={ordertype} and dataid={dataid}");
        }
        public DadaOrderRelation GetModelUOrderNo(string orderid)
        {
            return GetModel($"uniqueorderno='{orderid}'");
        }
        public List<DadaOrderRelation> GetListByOrderIds(string orderids,int ordertype,int dataid)
        {
            return GetList($"orderid in({orderids}) and ordertype={ordertype} and dataid={dataid}");
        }
    }
}