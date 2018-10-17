using DAL.Base;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Dish
{
    public class DishQueueBLL : BaseMySql<DishQueue>
    {
        #region 单例模式
        private static DishQueueBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishQueueBLL()
        {

        }

        public static DishQueueBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishQueueBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 根据参数返回相应结果
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="effData">只查询有效数据(state = 1)</param>
        /// <param name="orderbyCols">排序语句</param>
        /// <returns></returns>
        public List<DishQueue> GetQueuesByParams(int aid, int storeId, bool effData = true, bool orderBySort = true)
        {
            string whereSql = $" aid = {aid} and storeId = {storeId}  ";

            if (effData)
            {
                whereSql += "and state >= 0 ";
            }
            List<DishQueue> modelList = GetList(whereSql);
            if (orderBySort)
            {
                modelList = modelList.OrderByDescending(t => t.q_order).ToList();
            }

            return modelList;
        }

        /// <summary>
        /// 检测是否存在同名队列名
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="effData">只查询有效数据(state = 1)</param>
        /// <param name="orderbyCols">排序语句</param>
        /// <returns></returns>
        public bool CheckExistQueueName(int id, int aId, int storeId, string q_name)
        {
            string whereSql = $" aId = @aId and storeId = @storeId and q_name = @q_name and id != @id  and state != -1 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@id", id));
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@q_name", q_name));

            return base.Exists(whereSql, mysqlParams.ToArray());
        }
    }
}