using BLL.OpenWx;
using DAL.Base;
using Entity.OpenWx;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace BLL.OpenWx
{
    public partial class XcxAppAccountRelationBLL : BaseMySql<XcxAppAccountRelation>
    {
        #region 单例模式
        private static XcxAppAccountRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private XcxAppAccountRelationBLL()
        {

        }

        public static XcxAppAccountRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new XcxAppAccountRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        public bool JieBang(string userId)
        {
            List<string> sqllist = new List<string>();
            OpenAuthorizerConfig openconfig = OpenAuthorizerConfigBLL.SingleModel.GetModel("user_name='" + userId + "'");
            if(openconfig!=null)
            {
                var xcxrelations = GetList($"AppId='{openconfig.appid}'");
                if (xcxrelations != null && xcxrelations.Count > 0)
                {
                    var ids = string.Join(",", xcxrelations.Select(s => s.Id).Distinct());
                    //log4net.LogHelper.WriteInfo(this.GetType(),ids);
                    //log4net.LogHelper.WriteInfo(this.GetType(), $"Update XcxAppAccountRelation set appId='' where id in ({ids})");
                    
                    if (base.ExecuteNonQuery($"Update XcxAppAccountRelation set appId='' where id in ({ids})") <= 0 && UserXcxTemplateBLL.SingleModel.UpdateUserTemplate(openconfig.appid,openconfig.minisnsid))
                    {
                        return false;
                    }
                }

                if(OpenAuthorizerConfigBLL.SingleModel.Delete(openconfig.id)<=0)
                {
                    return false;
                }
            }

            return true;
        }

        public int UpdateModelByAppId(string appid)
        {
            string sql = $"update XcxAppAccountRelation set appid='' where appid='{appid}'";
            return SqlMySql.ExecuteNonQuery(connName, CommandType.Text, sql, null);
        }
    }
}
