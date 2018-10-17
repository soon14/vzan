using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace BLL.MiniApp
{
    /// <summary>
    /// 执行事务的参数实体
    /// </summary>
    public class TransactionModel
    {
        private List<string> sqllist;
        private List<MySqlParameter[]> plist;
        public TransactionModel()
        {
            sqllist = new List<string>();
            plist = new List<MySqlParameter[]>();
        }
        public string[] sqlArray
        {
            get
            {
                return sqllist.ToArray();
            }
        }
        public MySqlParameter[][] ParameterArray
        {
            get
            {
                return plist.ToArray();
            }
        }

        public void Add(string sql, MySqlParameter[] pone = null)
        {
            sqllist.Add(sql);
            plist.Add(pone);
        }
        public void Add(string[] sqls)
        {
            foreach(string sql in sqls)
            {
                Add(sql);
            }
        }
    }
}
