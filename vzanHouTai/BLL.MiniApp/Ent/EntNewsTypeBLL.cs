using DAL.Base;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntNewsTypeBLL : BaseMySql<EntNewsType>
    {
        #region 单例模式
        private static EntNewsTypeBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntNewsTypeBLL()
        {

        }

        public static EntNewsTypeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntNewsTypeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        
        public int GetCountByType(int aid,int parentId)
        {
           return base.GetCount($"aid={aid} and parentId={parentId}");
        }
        
        public int InitiType(int appId, int parentId)
        {
            string sql = $"update EntNewsType set parentId={parentId} where aid={appId} and Id<>{parentId}";
            return DAL.Base.SqlMySql.ExecuteNonQuery(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql);

        }

        public string GetEntNewsTypeName(string ptypes)
        {
            string sql = $"SELECT GROUP_CONCAT(`name`) from EntNewsType where FIND_IN_SET(id,@ptypes)";
            return DAL.Base.SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                CommandType.Text, sql,
                new MySqlParameter[] { new MySqlParameter("@ptypes", ptypes) }).ToString();
        }

        public List<EntNewsType> GetListData(int aid,int pageIndex,int pageSize,bool isParentData)
        {
            string sqlWhere = $"aid={aid} and state=1";
            if(isParentData)
            {
                sqlWhere += " and ParentId=0";
            }
            else
            {
                sqlWhere += " and ParentId<>0";
            }
            return base.GetList(sqlWhere, pageSize, pageIndex, "*", "id asc");
        }
    }
}
