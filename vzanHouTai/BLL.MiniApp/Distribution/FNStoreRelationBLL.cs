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
    public class FNStoreRelationBLL : BaseMySql<FNStoreRelation>
    {
        #region 单例模式
        private static FNStoreRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private FNStoreRelationBLL()
        {

        }

        public static FNStoreRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FNStoreRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public FNStoreRelation GetModelByRid(int rid)
        {
            return GetModel($"rid={rid}");
        }
    }
}