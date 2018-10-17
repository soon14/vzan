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
    public class DishQueueUpBLL : BaseMySql<DishQueueUp>
    {
        #region 单例模式
        private static DishQueueUpBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishQueueUpBLL()
        {

        }

        public static DishQueueUpBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishQueueUpBLL();
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
        public List<DishQueueUp> GetQueueUpByParams(int aid, int storeId, int state = 999, bool effData = true)
        {
            string whereSql = $" aid = {aid} and storeId = {storeId}  ";

            if (state != 999)
            {
                whereSql += $"and state = {state} ";
            }
            if (effData)
            {
                whereSql += "and state >= 0 ";
            }
            List<DishQueueUp> modelList = base.GetList(whereSql);

            return modelList;
        }

        /// <summary>
        /// 返回叫号的需要通知的队列记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="effData">只查询有效数据(state = 1)</param>
        /// <param name="orderbyCols">排序语句</param>
        /// <returns></returns>
        public List<DishQueueUp> GetCalls(int queueId,int renshu)
        {
            string whereSql = $" q_catid = {queueId} and state = 0  ";

            return base.GetList(whereSql,renshu,1);
        }

        /// <summary>
        /// 清空队列记录及重置队列排号
        /// </summary>
        /// <returns></returns>
        public bool ClearQueueUps()
        {
            List<string> clearSql = new List<string>();
            //clearSql.Add("Updatge DishQueueUp Set state = -1 where state = 0;");
            clearSql.Add("Delete From DishQueueUp;");
            clearSql.Add("Update DishQueue Set q_curnumber = 0;");

            return ExecuteTransaction(clearSql.ToArray());
        }

        /// <summary>
        /// 检测是某队列是否存在排队的人
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="queueId">队列ID</param>
        /// <returns></returns>
        public bool CheckExistQueueUps( int aId, int storeId, int queueId)
        {
            string whereSql = $" aId = @aId and storeId = @storeId and q_catid = @queueId and state = 0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@queueId", queueId));

            return base.Exists(whereSql, mysqlParams.ToArray());
        }


        /// <summary>
        /// 队列里还有多少人在排队
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public int GetCountByParams(int aId, int storeId, int queueId)
        {
            string whereSql = $" aId = @aId and storeId = @storeId and q_catid = @queueId and state = 0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@queueId", queueId));

            return base.GetCount(whereSql, mysqlParams.ToArray());
        }

        /// <summary>
        /// 当前用户排队model
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public DishQueueUp GetUserQueueUp(int aId, int storeId, int userId)
        {
            string whereSql = $" aId = @aId and storeId = @storeId  and user_Id = @userId and state = 0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@userId", userId));

            return base.GetModel(whereSql, mysqlParams.ToArray());
        }

        /// <summary>
        /// 当前到号人
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public DishQueueUp GetCurQueueUp(int aId, int storeId, int queueId)
        {
            string whereSql = $" aId = @aId and storeId = @storeId and q_catid = @queueId and state = 0 ";
            List<MySqlParameter> mysqlParams = new List<MySqlParameter>();
            mysqlParams.Add(new MySqlParameter("@aId", aId));
            mysqlParams.Add(new MySqlParameter("@storeId", storeId));
            mysqlParams.Add(new MySqlParameter("@queueId", queueId));

            return base.GetModel(whereSql, mysqlParams.ToArray());
        }

        /// <summary>
        /// 加入排队
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public bool AddQueueUp(DishQueueUp queueUp)
        {
            TransactionModel tran = new TransactionModel();
            tran.Add($" update DishQueue set q_curnumber = {queueUp.q_z_haoma} where id = {queueUp.q_catid} ");
            tran.Add(BuildAddSql(queueUp));

            return ExecuteTransactionDataCorect(tran.sqlArray,tran.ParameterArray);
        }
    }
}