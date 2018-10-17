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
    public class UserRoleBLL : BaseMySql<UserRole>
    {
        #region 单例模式
        private static UserRoleBLL _singleModel;
        private static readonly object SynObject = new object();

        private UserRoleBLL()
        {

        }

        public static UserRoleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new UserRoleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        /// <summary>
        /// 是否用户拥有指定的权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="roleType"></param>
        /// <returns></returns>
        public bool HavingRole(Guid userId, RoleType roleType, int appId = 0, int storeId = 0)
        {
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@userId", userId));
            mysqlParams.Add(new MySqlParameter("@appId", appId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@roleType", (int)roleType));

            return Exists($" userId = @userId and IFNULL(appId,0) = @appId and IFNULL(storeId,0) = @storeId  and roleId = @roleType And State = 1 ", mysqlParams.ToArray());
        }


        /// <summary>
        /// 查找当前用户所有的权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="roleType"></param>
        /// <returns></returns>
        public List<UserRole> GetCurrentUserRoles(Guid userId,  int appId, int storeId = 0)
        {
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@userId", userId));

            string sqlWhereStr = $" userId = @userId And State = 1 ";
            sqlWhereStr += $" and IFNULL(appId,0) = @appId " ;
            mysqlParams.Add(new MySqlParameter("@appId", appId));

            sqlWhereStr += $" and (IFNULL(storeId,0) = @storeId  OR IFNULL(storeId,0) = 0) ";
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));

            return GetListByParam(sqlWhereStr, mysqlParams.ToArray());
        }

        /// <summary>
        /// 是否有用户有这个权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="roleType"></param>
        /// <returns></returns>
        public bool HavingRole(RoleType roleType, int appId = 0, int storeId = 0)
        {
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@appId", appId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@roleType", (int)roleType));

            return Exists($" IFNULL(appId,0) = @appId and IFNULL(storeId,0) = @storeId  and roleId = @roleType And State = 1 ", mysqlParams.ToArray());
        }

        /// <summary>
        /// 读取哪个用户有这个权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="roleType"></param>
        /// <returns>Account 表的 ID</returns>
        public Guid havingRoleAccountId(RoleType roleType, int appId = 0, int storeId = 0)
        {
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@appId", appId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@roleType", (int)roleType));

            UserRole role = GetModel($" IFNULL(appId,0) = @appId and IFNULL(storeId,0) = @storeId  and roleId = @roleType And State = 1 ", mysqlParams.ToArray());

            return role == null ? Guid.Empty : role.UserId;
        }

        /// <summary>
        /// 新增用户权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="roleType"></param>
        /// <param name="deleteOldRole">是否替换移除掉此处的旧权限</param>
        /// <returns>userRole 权限表 新增的ID</returns>
        public Int32 addRoler(Guid userId, RoleType roleType, int appId = 0, int storeId = 0,bool deleteOldRole = true)
        {
            //是否替换移除掉此处的旧权限
            if (deleteOldRole)
            {
                //此处的旧权限作废
                var deleteRoleSql = $" update userRole set State = 0 Where IFNULL(appId,0) = {appId} and IFNULL(storeId,0) = {storeId} and RoleId = {(int)roleType} ";
                SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, deleteRoleSql);
            }

            return Convert.ToInt32(Add(new UserRole() { UserId = userId, RoleId = (int)roleType, AppId = appId, StoreId = storeId, State = 1 ,CreateDate = DateTime.Now}));
        }

    }
}