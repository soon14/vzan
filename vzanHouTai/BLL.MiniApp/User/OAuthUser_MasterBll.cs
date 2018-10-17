using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace BLL.MiniApp
{
    public class OAuthUser_MasterBll : BaseMySql<OAuthUser_Master>
    {
        #region 单例模式
        private static OAuthUser_MasterBll _singleModel;
        private static readonly object SynObject = new object();

        private OAuthUser_MasterBll()
        {

        }

        public static OAuthUser_MasterBll SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new OAuthUser_MasterBll();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public override object Add(OAuthUser_Master model)
        {
            object o = null;
            try
            {
                o = base.Add(model);
                //论坛的总人数加1
                SqlMySql.ExecuteNonQuery(connName, CommandType.Text, $"update low_priority minisns set hprices=hprices+1 where id={model.MinisnsId}", null);
                RedisUtil.Remove(string.Format(MemCacheKey.MinisnsDetailKey, model.MinisnsId));
                //论坛的总人数加1
                return o;
            }
            catch (Exception ex)
            {
                OAuthUser_Master model2 = GetModelFromMaster(string.Format("openid='{0}' and minisnsId={1}", model.OpenId, model.MinisnsId));
                if (model2 != null && model2.Id > 0)
                {
                    return model2.Id;
                }
                log4net.LogHelper.WriteInfo(this.GetType(), "添加主表：" + ex.Message);
                return o;
            }
        }
        public OAuthUser_Master GetModelFromMaster(string strWhere)
        {
            OAuthUser_Master model = new OAuthUser_Master();
            string strSql = "select * from OAuthUser_Master where " + strWhere;
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