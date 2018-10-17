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
    public class DadaCityBLL : BaseMySql<DadaCity>
    {
        #region 单例模式
        private static DadaCityBLL _singleModel;
        private static readonly object SynObject = new object();

        private DadaCityBLL()
        {

        }

        public static DadaCityBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DadaCityBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DadaCity GetModelName(string cityname)
        {
            return GetModel($"cityname='{cityname}'");
        }
        public DadaCity GetModelCode(string citycode)
        {
            return GetModel($"citycode='{citycode}'");
        }
    }
}