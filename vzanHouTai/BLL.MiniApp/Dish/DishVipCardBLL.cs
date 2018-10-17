using DAL.Base;
using Entity.MiniApp.Dish;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Dish
{
    public class DishVipCardBLL : BaseMySql<DishVipCard>
    {
        #region 单例模式
        private static DishVipCardBLL _singleModel;
        private static readonly object SynObject = new object();

        private DishVipCardBLL()
        {

        }

        public static DishVipCardBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DishVipCardBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion


        public List<DishVipCard> GetVipCardList(int storeId, int pageIndex, int pageSize, string nickname, string u_name, string u_phone, string number, out int recordCount)
        {
            recordCount = 0;
            string sql = "select a.*,b.telephone,b.nickname from dishvipcard a left join c_userinfo b on a.uid=b.id where ";
            string sqlwhere = $" shop_id={storeId}";
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(nickname))
            {
                parameters.Add(new MySqlParameter("@nickname", $"%{nickname}%"));
                sqlwhere += " and nickname like @nickname";
            }
            if (!string.IsNullOrEmpty(u_name))
            {
                parameters.Add(new MySqlParameter("@u_name", $"%{u_name}%"));
                sqlwhere += " and u_name like @u_name";
            }
            if (!string.IsNullOrEmpty(u_phone))
            {
                parameters.Add(new MySqlParameter("@u_phone", $"%{u_phone}%"));
                sqlwhere += " and telePhone like @u_phone";
            }
            if (!string.IsNullOrEmpty(number))
            {
                parameters.Add(new MySqlParameter("@number", $"%{number}%"));
                sqlwhere += " and number like @number";
            }
            sql += $"{sqlwhere} order by number asc limit {pageIndex * pageSize},{pageSize}";
            string countSql = $"select count(*) from dishvipcard a left join c_userInfo b on a.uid=b.id where {sqlwhere}";
            recordCount = GetCountBySql(countSql,parameters.ToArray());
            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql, parameters.ToArray()))
            {
                List<DishVipCard> list = new List<DishVipCard>();
                while (dr.Read())
                {
                    DishVipCard card = GetModel(dr);
                    card.nickname = dr["nickname"].ToString();
                    card.u_phone = dr["telephone"].ToString();
                    list.Add(card);
                }
                return list;
            }
        }

        public DishVipCard GetVipCardById_StoreId(int id, int storeId)
        {
            string sql = $"select a.*,b.telephone,b.nickname from dishvipcard a left join c_userinfo b on a.uid=b.id where a.id={id} and a.shop_id={storeId}";
            using (var dr = SqlMySql.ExecuteDataReader(connName, CommandType.Text, sql))
            {
                DishVipCard card = new DishVipCard();
                while (dr.Read())
                {
                    card = GetModel(dr);
                    card.nickname = dr["nickname"].ToString();
                    card.u_phone = dr["telephone"].ToString();
                }
                return card;
            }
        }

        public DishVipCard GetVipCardByStoreId_UId(int storeId,int uid)
        {
            DishVipCard card = null;
            if (uid <= 0 || storeId <= 0)
            {
                return card;
            }
            string sqlwhere = $"uid={uid} and shop_id={storeId}";
            card = GetModel(sqlwhere);
            return card;
        }

        public int GetCountByStoreId(int storeId)
        {
            string sqlwhere = $" shop_id={storeId}";
            return GetCount(sqlwhere);
        }



        /// <summary>
        /// 余额扣费
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool PayOrderByBalance(int aId, int storeId, int userId, double money, string info, ref string errMsg)
        {
            TransactionModel tran = new TransactionModel();

            //余额扣费
            DishVipCard vipCard = DishVipCardBLL.SingleModel.GetVipCardByStoreId_UId(storeId, userId);
            if (vipCard == null)
            {
                errMsg = "未成为本店会员,不支持余额";
                return false;
            }

            DishCardAccountLog log = new DishCardAccountLog();
            if (vipCard.account_balance < money)
            {
                errMsg = "余额不足,请充值后再支付";
                return false;
            }
            vipCard.account_balance -= money;
            tran.Add(DishVipCardBLL.SingleModel.BuildUpdateSql(vipCard, "account_balance"));
            DishCardAccountLogBLL.SingleModel.AddRecordLog(vipCard, 2, money, info, tran);

            return ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray);
        }

        public  DishVipCard GetModelByUid(int shop_id,int user_id)
        {
            string sqlwhere = $"shop_id ={shop_id} and uid={user_id}";
            return GetModel(sqlwhere);
        }
    }
}
