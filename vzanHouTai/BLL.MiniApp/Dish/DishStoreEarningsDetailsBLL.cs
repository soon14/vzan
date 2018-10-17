using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishStoreEarningsDetailsBLL : BaseMySql<DishStoreEarningsDetails>
    {
        #region 单例模式
        private static DishStoreEarningsDetailsBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishStoreEarningsDetailsBLL()
        {

        }

        public static DishStoreEarningsDetailsBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishStoreEarningsDetailsBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public double GetEarningsByDate(int storeId,DateTime startDate, DateTime endDate)
        {
            double sum = 0.00;
            
            DishStoreEarnings earnings = DishStoreEarningsBLL.SingleModel.GetModelByStoreId(storeId);
            if (earnings == null) return sum;
            string sql = $"select sum(changeMoney) from DishStoreEarningsDetails where seId={earnings.id} and addTime between @startDate and @endDate";
            List<MySqlParameter> parames = new List<MySqlParameter>();
            parames.Add(new MySqlParameter("@startDate", startDate));
            parames.Add(new MySqlParameter("@endDate", endDate));
            sum = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parames.ToArray()) == DBNull.Value ? 0 : Convert.ToDouble(SqlMySql.ExecuteScalar(connName, CommandType.Text, sql, parames.ToArray()));
            return sum; 
        }

        /// <summary>
        /// 获取门店总收益
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public double GetEarnings(int storeId)
        {
            double sum = 0.00;
            if (storeId <= 0)
            {
                return sum;
            }
            
            DishStoreEarnings earnings = DishStoreEarningsBLL.SingleModel.GetModelByStoreId(storeId);
            if (earnings == null) return sum;
            sum = earnings.money;
            return sum;
        }
    }
}