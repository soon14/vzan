using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BLL.MiniApp
{
    public class C_UserInfoBLL : BaseMySql<C_UserInfo>
    {
        /// <summary>
        /// 用户缓存key， openid
        /// </summary>
        private const string UserCacheKey = "vzan_cuserinfo_{0}";
        private const string _redisUserInfoSessionKey = "Dz_UserInfo_SessionKey_{0}";
        private static string _redisUserInfoModelKey = "UserInfoModelKey_{0}";

        #region 单例模式
        private static C_UserInfoBLL _singleModel;
        private static readonly object SynObject = new object();

        private C_UserInfoBLL()
        {

        }

        public static C_UserInfoBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new C_UserInfoBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        #region 缓存操作
        public void RemoveModel(int id)
        {
            RedisUtil.Remove(string.Format(_redisUserInfoModelKey, id));
        }

        public override C_UserInfo GetModel(int Id)
        {
            string key = string.Format(_redisUserInfoModelKey, Id);
            C_UserInfo model = RedisUtil.Get<C_UserInfo>(key);
            if (model != null)
                return model;

            model = base.GetModel(Id);
            if(model!=null)
            {
                RedisUtil.Set<C_UserInfo>(key,model,TimeSpan.FromHours(1));
            }

            return model;
        }

        /// <summary>
        /// 根据OPENID获取用户信息，包含了从基础表同步步骤
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="fromCache"></param>
        /// <returns></returns>
        public C_UserInfo GetModelFromCache(string openId, bool fromCache = true)
        {
            C_UserInfo cUser;
            string key = string.Format(UserCacheKey, openId);
            if (fromCache)
            {
                cUser = RedisUtil.Get<C_UserInfo>(key);
                if (cUser != null)
                    return cUser;
            }
            cUser = GetModelByOpenId(openId);
            if (cUser == null)
            {//新用户没有，尝试从基础用户表同步过来
                UserInfo baseUserInfo = UserInfoBLL.SingleModel.GetModelByOpenId(openId);
                if (baseUserInfo != null)
                {
                    cUser = RegisterByUserInfo(baseUserInfo);
                }
            }
            if (cUser != null)
            {
                RedisUtil.Set(key, cUser, TimeSpan.FromDays(1));
            }
            return cUser;
        }

        public C_UserInfo GetModelFromCacheByUnionid(string unionid, bool fromCache = true)
        {
            C_UserInfo cUser;
            string key = string.Format(UserCacheKey, unionid);
            if (fromCache)
            {
                cUser = RedisUtil.Get<C_UserInfo>(key);
                if (cUser != null)
                    return cUser;
            }
            cUser = GetModelByUnionId(unionid);
            if (cUser == null)
            {//新用户没有，尝试从基础用户表同步过来
                UserInfo baseUserInfo = UserInfoBLL.SingleModel.GetModelByUnionId(unionid);
                if (baseUserInfo != null)
                {
                    cUser = RegisterByUserInfo(baseUserInfo);
                }
            }
            if (cUser != null)
            {
                RedisUtil.Set(key, cUser, TimeSpan.FromDays(1));
            }
            return cUser;
        }

        public override bool Update(C_UserInfo model)
        {
            bool b = base.Update(model);
            RedisUtil.Remove(string.Format(UserCacheKey, model.OpenId));
            RemoveModel(model.Id);
            return b;
        }

        public override bool Update(C_UserInfo model, string columnFields)
        {
            bool b = base.Update(model, columnFields);
            RedisUtil.Remove(string.Format(UserCacheKey, model.OpenId));
            if (!string.IsNullOrEmpty(model.UnionId))
            {
                RedisUtil.Remove(string.Format(UserCacheKey, model.UnionId));
            }
            RemoveModel(model.Id);
            return b;
        }

        /// <summary>
        /// 刷新小程序端用户登录的sessionkey
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public bool RefleshUserInfoSessionKey(int userId, string sessionKey)
        {
            if (userId <= 0 || string.IsNullOrEmpty(sessionKey))
            {
                return false;
            }
            RedisUtil.Set<string>(string.Format(_redisUserInfoSessionKey, userId), sessionKey);
            return true;
        }

        /// <summary>
        /// 获取小程序端用户登录的sessionkey
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserInfoSessionKey(int userId)
        {
            if (userId <= 0)
            {
                return "";
            }
            return RedisUtil.Get<string>(string.Format(_redisUserInfoSessionKey, userId));
        }
        #endregion

        /// <summary>
        /// 防止触发唯一索引添加报的错
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override object Add(C_UserInfo model)
        {
            object o;
            try
            {
                o = base.Add(model);
            }
            catch
            {
                o = GetModelFromCache(model.OpenId).Id;
            }
            return o;
        }

        /// <summary>
        /// 验证手机号是否已绑定
        /// </summary>
        /// <param name="telePhone"></param>
        /// <param name="appid"></param>
        /// <returns></returns>
        public bool ExistsTelePhone(string telePhone, string appid = "")
        {
            List<MySqlParameter> param = new List<MySqlParameter>();

            string strWhere = $"TelePhone=@TelePhone and IsValidTelePhone =1 ";
            param.Add(new MySqlParameter("@TelePhone", telePhone));

            if (string.IsNullOrEmpty(appid))
            {
                strWhere += " and appId is null";
            }
            else
            {
                strWhere += $" and appId=@appId";
                param.Add(new MySqlParameter("@appId", appid));
            }
            return Exists(strWhere, param.ToArray());
        }

        public int GetCountByAppid(string appId)
        {
            int count = 0;
            if (string.IsNullOrEmpty(appId))
            {
                return count;
            }
            List<MySqlParameter> paramters = new List<MySqlParameter>();
            paramters.Add(new MySqlParameter("@appid", appId));
            string sqlwhere = "appid=@appid";
            count = GetCount(sqlwhere, paramters.ToArray());
            return count;
        }

        public C_UserInfo GetModelByUnionId(string unionId)
        {
            string strWhere = "UnionId=@UnionId";
            MySqlParameter[] param = { new MySqlParameter("@UnionId", unionId) };
            return base.GetModel(strWhere, param);
        }

        public C_UserInfo GetModelByAppId(string appid)
        {
            string strWhere = $"appid='{appid}'";
            List<C_UserInfo> list = base.GetList(strWhere, 1, 1, "", "id desc");
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return new C_UserInfo();
        }

        public C_UserInfo GetModelByOpenId(string openId)
        {
            string strWhere = "OpenId=@openId";
            MySqlParameter[] param = { new MySqlParameter("@openId", openId) };
            return base.GetModel(strWhere, param);
        }
        
        public C_UserInfo GetModelByAppId_OpenId(string appId, string openId)
        {
            string strWhere = " appId=@appId and OpenId=@openId ";
            MySqlParameter[] param = { new MySqlParameter("@appId", appId), new MySqlParameter("@openId", openId) };
            return base.GetModel(strWhere, param);
        }

        /// <summary>
        /// 从基础用户表注册用户信息
        /// </summary>
        /// <param name="baseUserInfo"></param>
        /// <returns></returns>
        public C_UserInfo RegisterByUserInfo(UserInfo baseUserInfo)
        {
            C_UserInfo cUser = new C_UserInfo
            {
                OpenId = baseUserInfo.openid,
                UnionId = baseUserInfo.unionid,
                NickName = baseUserInfo.nickname,
                HeadImgUrl = baseUserInfo.headimgurl
            };
            int sex;
            int.TryParse(baseUserInfo.sex, out sex);
            cUser.Sex = sex;
            cUser.Id = Convert.ToInt32(Add(cUser));
            return cUser;
        }

        /// <summary>
        /// 从小程序 注册用户
        /// </summary>
        /// <param name="baseUserInfo"></param>
        /// <returns></returns>
        public C_UserInfo RegisterByXiaoChenXun(C_UserInfo baseUserInfo)
        {
            if (string.IsNullOrEmpty(baseUserInfo?.OpenId))
            {
                return null;
            }
            C_UserInfo cUser = GetModelFromCache(baseUserInfo.OpenId);
            if (cUser != null)
                return cUser;
            cUser = new C_UserInfo
            {
                OpenId = baseUserInfo.OpenId,
                UnionId = baseUserInfo.UnionId,
                NickName = baseUserInfo.NickName,
                HeadImgUrl = baseUserInfo.HeadImgUrl,
                Sex = baseUserInfo.Sex,
                appId = baseUserInfo.appId,
                StoreId = baseUserInfo.StoreId,
                TelePhone = baseUserInfo.TelePhone,
                Address = baseUserInfo.Address,
                IsValidTelePhone = baseUserInfo.IsValidTelePhone,
            };
            cUser.Id = Convert.ToInt32(Add(cUser));
            return cUser;
        }

        public C_UserInfo GetModelByAppId_UserId(string appid, int userId)
        {
            string strWhere = $"id={userId} and appId=@appId";
            MySqlParameter[] param = new[] { new MySqlParameter("@appId", appid) };
            return base.GetModel(strWhere, param);
        }

        public C_UserInfo GetModelByTelephone_appid(string phone, string appid, bool requirePhoneValid = false)
        {
            C_UserInfo userInfo = null;
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(appid))
            {
                return userInfo;
            }
            string sqlwhere = $"TelePhone=@phone and appid=@appid";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@phone", phone));
            parameters.Add(new MySqlParameter("@appid", appid));
            if (requirePhoneValid)
            {
                sqlwhere += " and IsValidTelePhone=1 ";
            }
            userInfo = base.GetModel(sqlwhere, parameters.ToArray());
            return userInfo;
        }

        /// <summary>
        /// 获取客服信息
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public C_UserInfo GetKfInfo(string appId)
        {
            C_UserInfo kfInfo = null;
            if (string.IsNullOrEmpty(appId))
            {
                return kfInfo;
            }
            string sqlwhere = $"appid=@appid and userType ={(int)UserType.客服}";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@appid", appId));
            kfInfo = base.GetModel(sqlwhere, parameters.ToArray());
            return kfInfo;
        }

        public C_UserInfo GetModelByNickName(string appId, string nickName)
        {
            C_UserInfo userInfo = null;
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(nickName))
            {
                return userInfo;
            }
            string sqlwhere = $"appid=@appid and nickname=@nickname";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@appId", appId));
            parameters.Add(new MySqlParameter("@nickName", nickName));
            userInfo = base.GetModel(sqlwhere, parameters.ToArray());
            return userInfo;
        }

        /// <summary>
        /// 更新小程序用户信息
        /// </summary>
        /// <param name="apiUserInfo"></param>
        /// <param name="userInfo"></param>
        public C_UserInfo UpdateUserInfo(C_ApiUserInfo apiUserInfo, C_UserInfo userInfo)
        {
            if (apiUserInfo == null || userInfo == null)
                return userInfo;
            StringBuilder columns = new StringBuilder();
            //更新用户手机号
            if (!string.IsNullOrEmpty(apiUserInfo.phoneNumber) && apiUserInfo.phoneNumber != userInfo.TelePhone)
            {
                columns.Append("TelePhone,IsValidTelePhone,");
                userInfo.TelePhone = apiUserInfo.phoneNumber;
                userInfo.IsValidTelePhone = 1;
            }
            //更新用户昵称
            if (!string.IsNullOrEmpty(apiUserInfo.nickName) && apiUserInfo.nickName != userInfo.NickName)
            {
                columns.Append("NickName,");
                userInfo.NickName = apiUserInfo.nickName;
            }
            //更新用户头像
            if (!string.IsNullOrEmpty(apiUserInfo.avatarUrl) && apiUserInfo.avatarUrl != userInfo.HeadImgUrl)
            {
                columns.Append("HeadImgUrl,");
                userInfo.HeadImgUrl = apiUserInfo.avatarUrl;
            }
            if (!string.IsNullOrEmpty(apiUserInfo.unionId) && apiUserInfo.unionId != userInfo.UnionId)
            {
                columns.Append("UnionId,");
                userInfo.UnionId = apiUserInfo.unionId;
            }

            if (!string.IsNullOrEmpty(columns.ToString()))
            {
                base.Update(userInfo, columns.ToString().TrimEnd(','));
            }

            return userInfo;
        }

        /// <summary>
        /// 模糊查询昵称或手机号
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public List<C_UserInfo> GetListByName(string appId, string nickName)
        {
            if (string.IsNullOrEmpty(appId))
                return new List<C_UserInfo>();

            string strWhere = " appId=@appId and (NickName like @NickName or TelePhone like @telePhone)";
            MySqlParameter[] param = { new MySqlParameter("@appId", appId), new MySqlParameter("@NickName", $"%{nickName}%"), new MySqlParameter("@telePhone", $"%{nickName}%") };
            return base.GetListByParam(strWhere, param);
        }

        /// <summary>
        /// 根据用户昵称获取用户信息 通过appId进行过滤优化查询
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public List<C_UserInfo> GetUserListByNickName(string nickName, string appId = "")
        {
            string strWhere = string.Empty;
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(appId))
            {
                strWhere = $" appId=@appId and  ";
                parameters.Add(new MySqlParameter("@appId", appId));
            }
            strWhere += $"nickName like @nickName";

            parameters.Add(new MySqlParameter("@nickName", $"%{nickName}%"));

            // log4net.LogHelper.WriteInfo(this.GetType(), $"strWhere={strWhere};list={list.Count}");
            return GetListByParam(strWhere, parameters.ToArray());
        }

        /// <summary>
        /// 获取客服列表
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public List<C_UserInfo> GetKfListByPhone(string phone)
        {
            List<C_UserInfo> userList = null;
            if (string.IsNullOrEmpty(phone))
            {
                return userList;
            }
            string sqlwhere = $" TelePhone=@phone and userType={(int)UserType.客服}";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            parameters.Add(new MySqlParameter("@phone", phone));
            userList = GetListByParam(sqlwhere, parameters.ToArray());
            return userList;
        }

        public List<C_UserInfo> GetListByIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return new List<C_UserInfo>();
            return base.GetList($"id in ({ids})");
        }

        public List<C_UserInfo> GetListByAppIds(string telephone, string appids)
        {
            if (string.IsNullOrEmpty(telephone) || string.IsNullOrEmpty(appids))
                return new List<C_UserInfo>();

            return base.GetList($"TelePhone='{telephone}' and appId in ({appids})");
        }

    }
}