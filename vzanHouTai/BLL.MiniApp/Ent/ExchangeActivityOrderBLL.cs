using DAL.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Ent
{
    public class ExchangeActivityOrderBLL : BaseMySql<ExchangeActivityOrder>
    {
        #region 单例模式
        private static ExchangeActivityOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangeActivityOrderBLL()
        {

        }

        public static ExchangeActivityOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangeActivityOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion


        /// <summary>
        /// 获取用户兑换列表
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="strOrder"></param>
        /// <returns></returns>
        public List<ExchangeActivityOrder> GetJoinList(string where, int pageSize, int pageIndex, string strOrder)
        {
            var sql = $"select a.*,u.NickName,u.headimgurl from ExchangeActivityOrder a LEFT join C_UserInfo u on a.userId=u.Id  where {where} order by {strOrder} LIMIT {(pageIndex - 1) * pageSize},{pageSize}";

            using (var dr = SqlMySql.ExecuteDataReader(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql))
            {
                List<ExchangeActivityOrder> list = new List<ExchangeActivityOrder>();
                while (dr.Read())
                {
                    ExchangeActivityOrder activityOrder= GetModel(dr);
  
                    activityOrder.userLogo = dr["headimgurl"].ToString();
                    activityOrder.nickName = dr["NickName"].ToString();
                    list.Add(activityOrder);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取参与记录条数
        /// </summary>
        /// <param name="bid"></param>
        /// <returns></returns>
        public int GetJoinCount(string where)
        {
            var sql = $"select Count(a.Id) from  ExchangeActivityOrder a LEFT join C_UserInfo u on a.userId=u.Id where {where}";
            object obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
            if (obj != null)
            {
                return Convert.ToInt32(obj);
            }
            return 0;
        }


    }
}
