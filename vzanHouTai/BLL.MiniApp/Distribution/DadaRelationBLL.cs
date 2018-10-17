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
    public class DadaRelationBLL : BaseMySql<DadaRelation>
    {
        #region 单例模式
        private static DadaRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private DadaRelationBLL()
        {

        }

        public static DadaRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DadaRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public DadaRelation GetModelByRid(int rid,int storeid=0)
        {
            string sqlwhere = $"rid={rid} and state=0";
            if(storeid>0)
            {
                sqlwhere += $" and storeid={storeid}";
            }
            return GetModel(sqlwhere);
        }
    }
}