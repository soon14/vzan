using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class UUCustomerRelationBLL : BaseMySql<UUCustomerRelation>
    {
        #region 单例模式
        private static UUCustomerRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private UUCustomerRelationBLL()
        {

        }

        public static UUCustomerRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UUCustomerRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public UUCustomerRelation GetModelByAid(int aid,int storeId,int state)
        {
            string sqlWhere = $"aid = {aid} and state={state}";
            if(storeId>0)
            {
                sqlWhere += $" and storeid={storeId}";
            }
            return base.GetModel(sqlWhere);
        }
    }
}