using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    public class OpenAuthorizerConfigBLL : BaseMySql<OpenAuthorizerConfig>
    {
        #region 单例模式
        private static OpenAuthorizerConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private OpenAuthorizerConfigBLL()
        {

        }

        public static OpenAuthorizerConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new OpenAuthorizerConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public static readonly string XcxOpenAuthorizerConfigKey = "XcxOpenAuthorizerConfigKey_{0}";


        public List<OpenAuthorizerConfig> GetListByaccoundidAndRid(string accountid, int rid, int newtype = 0)
        {
            List<OpenAuthorizerConfig> info = GetList($"minisnsid='{accountid}' and RId={rid} and newtype={newtype}");
            return info;
        }

        public List<OpenAuthorizerConfig> GetListByaccoundidAndRid(string accountid, string rids, int newtype = 0)
        {
            List<OpenAuthorizerConfig> info = GetList($"minisnsid='{accountid}' and RId IN ({rids}) and newtype={newtype}");
            return info;
        }

        public List<OpenAuthorizerConfig> GetListByAppIds(string appids)
        {
            if (string.IsNullOrEmpty(appids))
                return new List<OpenAuthorizerConfig>();

            return base.GetList($"appid in ({appids})");
        }


        public OpenAuthorizerConfig GetModelByAppids(string appid,int rid=0,int newtype=0)
        {
            string sql = $"appid ='{appid}'";
            if(rid>0)
            {
                sql += $" and RId={rid}";
            }
            if (newtype > 0)
            {
                sql += $" and newtype={newtype}";
            }
            var info = GetModel(sql);

            return info;
        }
        public OpenAuthorizerConfig GetModelByAid(int aid)
        {
            string sql = $"RId={aid} and type=0 and newtype=0";
            return base.GetModel(sql);
        }

        /// <summary>
        /// 修改绑定模板授权用户accountid
        /// </summary>
        /// <param name="accountmodel"></param>
        /// <param name="customer"></param>
        /// <param name="rids"></param>
        /// <returns></returns>
        public bool UpdateBindUserInfo(string newuseraccountid, string olduseraccountid, string rids)
        {
            string sql = $"update openauthorizerconfig set minisnsid='{newuseraccountid}' where minisnsid='{olduseraccountid}' and rid in ({rids})";
            if(base.ExecuteNonQuery(sql)>-1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据小程序名称模糊匹配小程序Rid集合
        /// </summary>
        /// <returns></returns>
        public List<int> GetRidByAppName(string appName)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            List<int> listRid = new List<int>();
            parameters.Add(new MySqlParameter("@nick_name", $"%{appName}%"));
            string strWhere = " nick_name like @nick_name";
            List<OpenAuthorizerConfig> list = base.GetListByParam(strWhere,parameters.ToArray());
            if (list != null && list.Count > 0)
            {
                foreach(OpenAuthorizerConfig item in list)
                {
                    listRid.Add(item.RId);
                }
            }
            return listRid;
        }


    }
}
