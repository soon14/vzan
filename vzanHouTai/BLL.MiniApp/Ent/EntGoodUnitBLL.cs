using DAL.Base;
using Entity.MiniApp.Ent;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Utility;

namespace BLL.MiniApp.Ent
{
    public class EntGoodUnitBLL : BaseMySql<EntGoodUnit>
    {
        #region 单例模式
        private static EntGoodUnitBLL _singleModel;
        private static readonly object SynObject = new object();

        private EntGoodUnitBLL()
        {

        }

        public static EntGoodUnitBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new EntGoodUnitBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public int GetEntGoodUnitId(int aid, string unitName)
        {
            string sql = $"SELECT id from EntGoodUnit where aid={aid} and name=@name and State=1";

            object id = SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),
                  CommandType.Text, sql,
                  new MySqlParameter[] { new MySqlParameter("@name", unitName) });
            if (id != DBNull.Value)
            {
                return Convert.ToInt32(id);
            }
            return 0;

        }
    }
}
