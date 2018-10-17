using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.User
{
    public class AuthRoleBLL:BaseMySql<AuthRole>
    {
        #region 单例模式
        private static AuthRoleBLL _singleModel;
        private static readonly object SynObject = new object();

        private AuthRoleBLL()
        {

        }

        public static AuthRoleBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AuthRoleBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public AuthRole UserLogin(int appId, string loginName, string password)
        {
            try
            {
                MySqlParameter[] paras = new MySqlParameter[]{
                                 new MySqlParameter("@AppId",appId),
                                 new MySqlParameter("@LoginName",loginName),
                                 new MySqlParameter("@Password",DESEncryptTools.GetMd5Base32(password))
                };
                return GetModel("LoginName = @LoginName AND Password = @PassWord AND aId = @AppId AND State != -1", paras);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool UpdateLoginTime(AuthRole admin)
        {
            admin.LastLogin = DateTime.Now;
            return Update(admin, "LastLogin");
        }

        public bool CheckLoginName(int aId, string loginName)
        {
            string whereSql = BuildWhereSql(aId: aId, loginName: loginName);
            return GetCount(whereSql) == 0;
        }

        public AuthRole GetByAId(int aId, int roleId)
        {
            AuthRole role = GetModel(roleId);
            if(role?.AId != aId)
            {
                role = null;
            }
            return role;
        }

        public List<AuthRole> GetListByAId(int aId, int pageIndex = 1, int pageSize = 999)
        {
            string whereSql = BuildWhereSql(aId: aId);
            return GetList(whereSql, pageSize, pageIndex);
        }

        public int GetCountByAId(int aId)
        {
            string whereSql = BuildWhereSql(aId: aId);
            return GetCount(whereSql);
        }

        public List<AuthRole> GetListByGroupId(int groupId)
        {
            string whereSql = BuildWhereSql(groupId: groupId);
            return GetList(whereSql);
        }

        public AuthInfo GetMasterAuth(int pageType, string authName = null , string accessUrl = null)
        {
            List<NavMenu> allMenu = NavMenuBLL.SingleModel.GetListByPageType(pageType);
            List<AuthMenu> authMenu = new List<AuthMenu>();
            allMenu.ForEach(item =>
            {
                authMenu.Add(new AuthMenu { ItemId = item.Id, Read = true, Write = true });
            });
            AuthInfo authInfo = new AuthInfo
            {
                AuthMenu = authMenu,
                MasterAccess = true,
                AuthName = authName,
                AllMenu = allMenu,
                CurrRoute = accessUrl,
            };
            return authInfo;
        }

        public AuthInfo GetAuthMenuByRole(int pageType, int roleId, string accessUrl = null)
        {
            AuthRole role = GetModel(roleId);
            AuthGroup group = AuthGroupBLL.SingleModel.GetModel(role.GroupId);
            AuthInfo authInfo = new AuthInfo
            {
                AuthMenu = group.GetAuthMenu(),
                AuthName = role.Name,
                AuthAdmin = role,
                AllMenu = role != null ? NavMenuBLL.SingleModel.GetListByPageType(pageType) : new List<NavMenu>(),
                CurrRoute = accessUrl,
            };
            return authInfo;
        }

        public AuthInfo GetAppMasterAuth(int pageType = (int)TmpType.小程序专业模板, string authName = null, string accessUrl = null)
        {
            List<NavMenu> allMenu = NavMenuBLL.SingleModel.GetListByPageType(pageType);
            List<AuthMenu> authMenu = new List<AuthMenu>();
            allMenu?.ForEach(item =>
            {
                authMenu.Add(new AuthMenu { ItemId = item.Id, Read = true, Write = true });
            });
            AuthInfo authInfo = new AuthInfo
            {
                AuthMenu = authMenu,
                MasterAccess = true,
                AuthName = authName,
                AllMenu = allMenu,
                CurrRoute = accessUrl,
            };
            return authInfo;
        }

        public AuthInfo GetAppMenuByRole(AuthRole role, int pageType = (int)TmpType.小程序专业模板, string accessUrl = null)
        {
            if (role == null)
                return new AuthInfo();

            AuthGroup group = AuthGroupBLL.SingleModel.GetModel(role.GroupId);
            if (group == null)
                return new AuthInfo();

            AuthInfo authInfo = new AuthInfo
            {
                AuthMenu = group.GetAuthMenu(),
                AuthName = role.Name,
                AuthAdmin = role,
                AllMenu = NavMenuBLL.SingleModel.GetListByPageType(pageType) ?? new List<NavMenu>(),
                CurrRoute = accessUrl,
            };
            return authInfo;
        }

        public bool DeleteRole(int aId,int roleId)
        {
            AuthRole role = GetModel(roleId);
            if(role.AId != aId)
            {
                return false;
            }
            role.State = -1;
            return Update(role, "State");
        }

        public bool DeleteRole(List<AuthRole> roles)
        {
            if(roles == null || roles.Count == 0)
            {
                return true;
            }
            TransactionModel tran = new TransactionModel();
            roles.ForEach(role => 
            {
                role.State = -1; tran.Add(BuildUpdateSql(role, "State"));
            });
            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public string BuildWhereSql(int? aId = null,int? groupId = null, string ids = null, string loginName = null, bool isGetDelete = false)
        {
            List<string> whereSql = new List<string>();
            if(aId.HasValue)
            {
                whereSql.Add($"AId = {aId.Value}");
            }
            if(groupId.HasValue)
            {
                whereSql.Add($"GroupId = {groupId.Value}");
            }
            if(!isGetDelete)
            {
                whereSql.Add($"State != -1");
            }
            if(!string.IsNullOrWhiteSpace(loginName))
            {
                whereSql.Add($"LoginName = '{loginName}'");
            }
            if(!string.IsNullOrWhiteSpace(ids))
            {
                whereSql.Add($"Id IN ({ids})");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
