using DAL.Base;
using Entity.MiniApp.Plat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Plat
{
    public class PlatStatisticsBLL : BaseMySql<PlatStatistics>
    {
        #region 单例模式
        private static PlatStatisticsBLL _singleModel;
        private static readonly object SynObject = new object();

        private PlatStatisticsBLL()
        {

        }

        public static PlatStatisticsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PlatStatisticsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public List<PlatStatistics> GetCountList(string aids)
        {
            List<PlatStatistics> list = new List<PlatStatistics>();
            if(string.IsNullOrEmpty(aids))
            {
                return list;
            }
            string sql = $"select count(*) count,aid from platstatistics where aid in ({aids}) group by aid";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql , null))
            {
                while (dr.Read())
                {
                    PlatStatistics model = base.GetModel(dr);
                    if(dr["count"]!=DBNull.Value)
                    {
                        model.Count = Convert.ToInt32(dr["count"]);
                    }
                    list.Add(model);
                }
            }

            return list;
        }
    }
}
