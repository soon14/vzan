using DAL.Base;
using Entity.OpenWx;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace BLL.OpenWx
{
    public partial class XcxTemplateBLL : BaseMySql<XcxTemplate>
    {
        #region 单例模式
        private static XcxTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private XcxTemplateBLL()
        {

        }

        public static XcxTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new XcxTemplateBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public XcxTemplate GetTModel(int tid)
        {
            var model = GetModel(string.Format("TId={0} and State=0", tid));

            return model;
        }

        public XcxTemplate GetModelFromMaster(int tid)
        {
            XcxTemplate model = new XcxTemplate();
            string strSql = "select * from XcxTemplate where Id=" + tid;
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, strSql, null))
            {
                if (dr.Read())
                    model = GetModel(dr);
            }
            if (model.Id == 0)
                return null;
            return model;
        }
    }
}
