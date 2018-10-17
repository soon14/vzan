using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp
{
    public partial class UserXcxTemplateBLL : BaseMySql<UserXcxTemplate>
    {
        #region 单例模式
        private static UserXcxTemplateBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserXcxTemplateBLL()
        {

        }

        public static UserXcxTemplateBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserXcxTemplateBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        public bool ExitName(string name, string appId)
        {
            string sqlWhere = "name=@name and appid<>@appid ";
            List<MySqlParameter> parms = new List<MySqlParameter>();
            parms.Add(new MySqlParameter("@appid", appId));
            parms.Add(new MySqlParameter("@name", name));
            return base.Exists(sqlWhere, parms.ToArray());
        }

        public List<UserXcxTemplate> GetListByAppIds(string appIds)
        {
            if (string.IsNullOrEmpty(appIds))
                return new List<UserXcxTemplate>();

            return base.GetList($"appid in ({appIds})");
        }
        
        public UserXcxTemplate GetModelByAppId(string appid)
        {
            var model = GetModel(string.Format("AppId='{0}'", appid));

            return model;
        }

        /// <summary>
        /// 获取小程序中正在审核中的记录
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public List<UserXcxTemplate> GetMiniappSubmitList(int dayLength)
        {
            string sql = $"state in ({(int)XcxTypeEnum.审核中}) and auditid>0 and tuserid is not null and tuserid <>'' and updatetime<'{DateTime.Now.AddDays(dayLength)}'";
            return base.GetList(sql,200,1);
        }
    }
}
