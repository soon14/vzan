using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp
{
    public partial class RefundResultBLL : BaseMySql<ReFundResult>
    {
        #region 单例模式
        private static RefundResultBLL _singleModel;
        private static readonly object SynObject = new object();

        private RefundResultBLL()
        {

        }

        public static RefundResultBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new RefundResultBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public ReFundResult GetModelByTradeno(string tradeno)
        {
            return base.GetModel($" transaction_id = '{tradeno}' and retype = 1");
        }
        public int GetSumMoney(string tradenos)
        {
            string sql = $"SELECT SUM(refund_fee) from ReFundResult where transaction_id in ({tradenos}) and result_code='SUCCESS'";

            var obj = SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, sql, null);
            if (obj != DBNull.Value)
            {
                return Convert.ToInt32(obj);
            }

            return 0;
        }
        public List<ReFundResult> Getinfos(string tradenos)
        {
            List<ReFundResult> list = new List<ReFundResult>();
            string sql = $"SELECT * from ReFundResult where transaction_id in ({tradenos}) and result_code='SUCCESS'";

            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    var model = GetModel(dr);
                    list.Add(model);
                }
            }

            return list;
        }
    }
}