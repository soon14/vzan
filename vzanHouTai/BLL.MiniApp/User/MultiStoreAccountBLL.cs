using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
/// <summary>
/// Member转移过来的
/// </summary>
namespace BLL.MiniApp
{
    public class MultiStoreAccountBLL : BaseMySql<MultiStoreAccount>
    {
        #region 单例模式
        private static MultiStoreAccountBLL _singleModel;
        private static readonly object SynObject = new object();

        private MultiStoreAccountBLL()
        {

        }

        public static MultiStoreAccountBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new MultiStoreAccountBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private static readonly string MULTISTOREACCOUNT_REDIS_KEY = "MULTISTOREACCOUNT_REDIS_KEY_{0}";
        

        /// <summary>
        /// 查找当前用户管理的所有账户(找出当前小程序商户号下当前用户管理的分店账号)
        /// </summary>
        /// <returns></returns>
        public List<MultiStoreAccount> GetListByMasterAccountId(Guid AccountId, int appId)
        {
            if (appId <= 0)
            {
                return null;
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (role == null)
            {
                return null;
            }
            PayCenterSetting userPaySetting = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(role.AppId);
            if (userPaySetting == null)
            {
                return null;
            }
            return GetListByMasterAccountId(AccountId, userPaySetting.Mch_id);
        }

        /// <summary>
        /// 获取管理员账户信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MultiStoreAccount GetMultiStoreAccountByCache(int Id)
        {
            if (Id <= 0)
            {
                return null;
            }

            MultiStoreAccount model = RedisUtil.Get<MultiStoreAccount>(string.Format(MULTISTOREACCOUNT_REDIS_KEY, Id));
            if (model == null)
            {
                model = base.GetModel(Id);
                RedisUtil.Set<MultiStoreAccount>(string.Format(MULTISTOREACCOUNT_REDIS_KEY, Id), model, TimeSpan.FromHours(6));
            }
            return model;
        }
        
        /// <summary>
        /// 查找对应账户
        /// </summary>
        /// <param name="merNo"></param>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public MultiStoreAccount GetModelByParams(string merNo,string loginName,string password)
        {
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@MerNo", merNo));
            sqlParams.Add(new MySqlParameter("@LoginName", loginName));
            sqlParams.Add(new MySqlParameter("@Password", password));
            try
            {
                return GetModel($" MerNo = @MerNo AND LoginName = @LoginName AND password = @Password And State = 1", sqlParams.ToArray());
            }
            catch (Exception)
            {
                return null ;
            }
        }

        /// <summary>
        /// 查找当前用户关联的所有账户
        /// </summary>
        /// <returns></returns>
        public List<MultiStoreAccount> GetListByAccountId(Guid AccountId, string MerNo = "")
        {
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@AccountId", AccountId));
            sqlParams.Add(new MySqlParameter("@MerNo", MerNo));

            return GetListByParam($" {(!string.IsNullOrWhiteSpace(MerNo) ? " MerNo = @MerNo  and " : "")} AccountId = @AccountId and State = 1 ", sqlParams.ToArray());
        }

        /// <summary>
        /// 查找当前用户管理的所有账户
        /// </summary>
        /// <returns></returns>
        public List<MultiStoreAccount> GetListByMasterAccountId(Guid AccountId, string MerNo = "")
        {
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@MasterAccountId", AccountId));
            sqlParams.Add(new MySqlParameter("@MerNo", MerNo));

            return GetListByParam($" {(!string.IsNullOrWhiteSpace(MerNo) ? " MerNo = @MerNo  and " : "")} MasterAccountId = @MasterAccountId and State >= 0 ", sqlParams.ToArray());
        }

        /// <summary>
        /// 查找当前商户号的所有未删除账户
        /// </summary>
        /// <returns></returns>
        public List<MultiStoreAccount> GetListByAppId(int appId, string MerNo = "")
        {
            List<MySqlParameter> sqlParams = new List<MySqlParameter>();
            sqlParams.Add(new MySqlParameter("@appId", appId));
            sqlParams.Add(new MySqlParameter("@MerNo", MerNo));

            return GetListByParam($" {(!string.IsNullOrWhiteSpace(MerNo) ? " MerNo = @MerNo  and " : "")} appId = @appId and State >= 0 ", sqlParams.ToArray());
        }


        /// <summary>
        /// 是否重复登录名
        /// </summary>
        /// <param name="multiStoreAccount"></param>
        /// <returns></returns>
        public bool repetitionLoginName(MultiStoreAccount multiStoreAccount)
        {
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@Id", multiStoreAccount.Id));
            mysqlParams.Add(new MySqlParameter("@AppId", multiStoreAccount.AppId));
            mysqlParams.Add(new MySqlParameter("@MerNo", multiStoreAccount.MerNo));
            mysqlParams.Add(new MySqlParameter("@MasterAccountId", multiStoreAccount.MasterAccountId));
            mysqlParams.Add(new MySqlParameter("@LoginName", multiStoreAccount.LoginName));

            //是否重复
            var isRepetition = Exists($" {(multiStoreAccount.Id > 0 ? " Id != @Id And " : "")} AppId = @AppId And MerNo = @MerNo And MasterAccountId = @MasterAccountId And State >= 0 AND LoginName = @LoginName", mysqlParams.ToArray());
            return isRepetition;
        }


        /// <summary>
        /// 查找当前小程序下的所有账户(找出当前小程序下的分店账号)
        /// </summary>
        /// <returns></returns>
        public List<MultiStoreAccount> GetListByAppId(int appId)
        {
            if (appId <= 0)
            {
                return null;
            }

            XcxAppAccountRelation role = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (role == null)
            {
                return null;
            }
            PayCenterSetting userPaySetting = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(role.AppId);
            if (userPaySetting == null)
            {
                return null;
            }
            return GetListByAppId(role.Id,userPaySetting.Mch_id);
        }


        #region 重写底层加入缓存,若需要其他方法,再行加入

        public override object Add(MultiStoreAccount model)
        {
            model.Id = Convert.ToInt32(base.Add(model));
            if (model.Id > 0)
            {
                RedisUtil.Set<MultiStoreAccount>(string.Format(MULTISTOREACCOUNT_REDIS_KEY, model.Id), model, TimeSpan.FromHours(6));
            }
            return model.Id;
        }

        public override bool Update(MultiStoreAccount model)
        {
            bool isSuccess = base.Update(model);
            if (isSuccess)
            {
                RedisUtil.Remove(string.Format(MULTISTOREACCOUNT_REDIS_KEY, model.Id));
                RedisUtil.Set<MultiStoreAccount>(string.Format(MULTISTOREACCOUNT_REDIS_KEY, model.Id), model, TimeSpan.FromHours(6));
            }
            return isSuccess;
        }

        #endregion
    }
}