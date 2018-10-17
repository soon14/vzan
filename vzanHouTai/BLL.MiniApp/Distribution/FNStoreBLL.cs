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
    public class FNStoreBLL : BaseMySql<FNStore>
    {
        #region 单例模式
        private static FNStoreBLL _singleModel;
        private static readonly object SynObject = new object();

        private FNStoreBLL()
        {

        }

        public static FNStoreBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FNStoreBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public FNStore GetModelByMId(int merchantid)
        {
            return GetModel($"distritionid={merchantid}");
        }
        
    }
}