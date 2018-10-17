using Core.MiniApp;
using DAL.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace User.MiniApp.Areas.DishAdmin.Models
{
    public class PayRecordModel
    {
        public int AId { get; set; } = 0;
        public int StoreId { get; set; } = 0;
        public int UserId { get; set; } = 0;
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// 支付金额（分）
        /// </summary>
        public double Money { get; set; } = 0d;

        /// <summary>
        /// 支付方式
        /// </summary>
        public string PayType { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;

        public string HeadImgUrl { get; set; } = string.Empty;
    }

    public class PayRecordBLL
    {
        #region 单例模式
        private static PayRecordBLL _singleModel;
        private static readonly object SynObject = new object();

        private PayRecordBLL()
        {

        }

        public static PayRecordBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new PayRecordBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public List<PayRecordModel> GetList(int aId, int storeId, int pageIndex, int pageSize, out int total, int payType = 0, DateTime? queryBegin = null, DateTime? queryEnd = null)
        {
            string selectSql = BuildSelectSql(aId, storeId);
            string whereSql = BuildWhereSql(payType, queryBegin, queryEnd);
            string orderSql = "order by t.addtime desc ";
            string pagerSql = $"limit {pageIndex * pageSize},{pageSize} ";
            string countSql = $@"SELECT count(0)
                                   FROM(
                                            SELECT aid,shop_id as storeid,user_id as userid,add_time as addtime, account_info as info,account_money as money,'储值支付' as paytype
                                              FROM DishCardAccountLog
                                             WHERE account_type=2 and aid={aId} and shop_id={storeId}  and account_info like '门店买单,%'
                                         UNION ALL
                                            SELECT MinisnsId as aid,CommentId as storeid,FuserId as userid,Addtime, ShowNote as info,payment_free as money,'微信支付' as paytype
                                              FROM citymorders where OrderType=3001017 and `Status`=1 and MinisnsId={aId} and CommentId={storeId}
                                        ) t
                                   WHERE 1=1 {whereSql}";

            total = Convert.ToInt32(SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, countSql));
            return DataHelper.ConvertDataTableToList<PayRecordModel>(SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"{selectSql} {whereSql} {orderSql} {pagerSql}").Tables[0]);
        }

        public DataTable GetTable(int aId, int storeId, int payType = 0, DateTime? queryBegin = null, DateTime? queryEnd = null)
        {
            string selectSql = BuildSelectSql(aId, storeId);
            string whereSql = BuildWhereSql(payType, queryBegin, queryEnd);
            string orderSql = "order by t.addtime desc ";
            return SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"{selectSql} {whereSql} {orderSql}").Tables[0];
        }

        public string BuildSelectSql(int aId,int storeId)
        {
            return $@"SELECT t.*,u.NickName,u.HeadImgUrl
                        FROM (
                                SELECT aid,shop_id as storeid,user_id as userid,add_time as addtime, account_info as info,account_money as money,'储值支付' as paytype 
                                  FROM DishCardAccountLog
                                 WHERE account_type=2 and aid={aId} and shop_id={storeId} and account_info like '门店买单,%'
                             UNION ALL
                                SELECT MinisnsId as aid,CommentId as storeid,FuserId as userid,Addtime, CONCAT(ShowNote,'，备注：',remark) as info,payment_free/100 as money,'微信支付' as paytype
                                  FROM citymorders where OrderType=3001017 and `Status`=1 and MinisnsId={aId} and CommentId={storeId}
                            ) t 
                  LEFT JOIN c_userinfo u on t.userid=u.Id
                      WHERE 1=1";
        }

        public string BuildWhereSql(int payType = 0, DateTime? queryBegin = null, DateTime? queryEnd = null)
        {
            string whereSql = string.Empty;
            if (queryBegin.HasValue)
            {
                whereSql += $" and t.addtime>='{queryBegin.Value.ToString("yyyy-MM-dd 00:00:00")}'";
            }
            if (queryEnd.HasValue)
            {
                whereSql += $" and t.addtime<='{queryEnd.Value.ToString("yyyy-MM-dd 23:59:59")}'";
            }
            if (payType == 1)
            {
                whereSql += $"and t.paytype='微信支付'";
            }
            if (payType == 2)
            {
                whereSql += $"and t.paytype='储值支付'";
            }
            return whereSql;
        }
    }
}