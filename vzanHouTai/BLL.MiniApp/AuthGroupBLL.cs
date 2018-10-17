using BLL.MiniApp.User;
using DAL.Base;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp
{
    public class AuthGroupBLL : BaseMySql<AuthGroup>
    {
        #region 单例模式
        private static AuthGroupBLL _singleModel;
        private static readonly object SynObject = new object();

        private AuthGroupBLL()
        {

        }

        public static AuthGroupBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new AuthGroupBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public bool CheckGroupName(string groupName, int aId, int roleId)
        {
            string whereSql = BuildWhereSql(groupName: groupName, aId: aId);
            return Exists($"{whereSql} AND ID != {roleId}");
        }

        public AuthGroup GetByAId(int aId, int groupId)
        {
            AuthGroup group = GetModel(groupId);
            if(group?.Aid != aId)
            {
                group = null;
            }
            return group;
        }

        public List<AuthGroup> GetListByAId(int aId, int pageIndex = 1, int pageSize = 999)
        {
            string whereSql = BuildWhereSql(aId: aId);
            return GetList(whereSql, pageSize, pageIndex);
        }

        public int GetCountByAId(int aId)
        {
            string whereSql = BuildWhereSql(aId: aId);
            return GetCount(whereSql);
        }

        public bool DeleteGroup(int aId, int groupId)
        {
            AuthGroup group = GetModel(groupId);
            if(group.Aid != aId)
            {
                return false;
            }

            List<AuthRole> groupRoles = AuthRoleBLL.SingleModel.GetListByGroupId(group.Id);

            group.State = -1;
            return Update(group, "State") && AuthRoleBLL.SingleModel.DeleteRole(groupRoles);
        }

        public string BuildWhereSql(int? aId = null,string groupName = null, bool isGetDelete = false)
        {
            List<string> whereSql = new List<string>();
            if(aId.HasValue)
            {
                whereSql.Add($"AId = {aId.Value}");
            }
            if(!isGetDelete)
            {
                whereSql.Add($"State != -1");
            }
            if(!string.IsNullOrWhiteSpace(groupName))
            {
                whereSql.Add($"Name = '{groupName}'");
            }
            return string.Join(" AND ", whereSql);
        }
    }
}
