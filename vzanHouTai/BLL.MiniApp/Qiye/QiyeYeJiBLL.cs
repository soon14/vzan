using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.Qiye;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using BLL.MiniApp.Plat;

namespace BLL.MiniApp.Qiye
{
    public class QiyeYeJiBLL : BaseMySql<QiyeYeJi>
    {
        #region 单例模式
        private static QiyeYeJiBLL _singleModel;
        private static readonly object SynObject = new object();

        private QiyeYeJiBLL()
        {

        }

        public static QiyeYeJiBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new QiyeYeJiBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        public Tuple<List<QiyeYeJi>, int> GetListFromTable(int aId,int did, int pageIndex, int pageSize, string kw = "", int typeid = 0)
        {
            Tuple<string, List<MySqlParameter>> sqlResult = BuildSql(aId, did, pageIndex, pageSize, kw, typeid);
            DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, sqlResult.Item1, sqlResult.Item2?.ToArray()).Tables[0];
            int totalCount =Convert.ToInt32(SqlMySql.ExecuteScalar(dbEnum.MINIAPP.ToString(),CommandType.Text, sqlResult.Item1.Replace("*","count(0)"), sqlResult.Item2?.ToArray()));
            List<QiyeYeJi> list = DataHelper.ConvertDataTableToList<QiyeYeJi>(dt);
            QiyeYeJiBLL logBLL = new QiyeYeJiBLL();
            
            return Tuple.Create(list, totalCount);
        }

        /// <summary>
        /// 私有方法，拼接sql,返回sql和sqlparameter
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="did"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="kw"></param>
        /// <param name="typeid"></param>
        /// <returns></returns>
        private Tuple<string,List<MySqlParameter>> BuildSql(int aId, int did, int pageIndex, int pageSize, string kw = "", int typeid = 0)
        {
            List<MySqlParameter> parameters = null;
            StringBuilder sqlFilter = new StringBuilder();
            pageIndex = pageIndex - 1;
            if (pageIndex < 0)
                pageIndex = 0;

            if (!string.IsNullOrEmpty(kw))
            {
                if (parameters == null)
                    parameters = new List<MySqlParameter>();
                parameters.Add(new MySqlParameter("@kw", Utils.FuzzyQuery(kw)));
                sqlFilter.Append(" and (t.UserName like @kw or t.phone like @kw)  ");
            }
            if (did > 0)
            {
                sqlFilter.Append($" and t.departmentid={did}  ");
            }
            string timeRange = string.Empty;
            if (typeid == 1)
                timeRange = "and DATE_FORMAT(addtime,'%Y%m')=DATE_FORMAT(CURDATE() ,'%Y%m' )";

            string sqlfilter = $"aid={aId} {timeRange}";
            string sqlpager = pageSize <= 0 ? "" : $" limit {pageSize * pageIndex},{pageSize}";
            string sql = $@" SELECT * from (
SELECT e.id,e.`name` as UserName,e.Phone, e.departmentid,e.aid,d.`name` as DepartmentName,
(SELECT count(0) from qiyeuserfavoritemsg log where {sqlfilter} and log.msgid = e.id and log.actiontype = 3 and log.datatype = 3) as CardViewedCount,
(SELECT count(0) from qiyeuserfavoritemsg log where {sqlfilter} and log.msgid = e.id and log.actiontype = 6 and log.datatype = 3) as CardRepostCount,
(SELECT count(0) from qiyecustomer c where {sqlfilter} and c.staffid = e.id) as CustomerCount,
(SELECT count(0) from qiyeuserfavoritemsg log where {sqlfilter} and log.msgid = e.id and log.actiontype = 1 and log.datatype = 3) as CustomerLikeCount,
(SELECT count(0) from immessage msg where msg.tuserid=e.userid and msg.tuserid!=0) as CustomerConsultCount,
(SELECT count(0) from qiyegoodsorder o where {sqlfilter} and o.State=4 and o.userid in(SELECT userid from qiyecustomer c where c.aid ={aId} and c.state >=0 and c.staffid = e.id)) as OrderCount,
(SELECT convert(IFNULL(sum(BuyPrice), 0)/100,decimal(10,2)) from qiyegoodsorder o where {sqlfilter} and o.State=4 and o.userid in(SELECT userid from qiyecustomer c where c.aid ={aId} and c.state >=0 and c.staffid = e.id) and o.State = 4) as Sales

from qiyeemployee e LEFT JOIN qiyedepartment d on e.departmentid = d.id
) t where 1=1 and aid={aId} {sqlFilter} order by t.Sales desc {sqlpager}";
            return Tuple.Create(sql, parameters);
        }

        /// <summary>
        /// 供导出使用
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="did"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize">传入<0的数字表示导出全部</param>
        /// <param name="kw"></param>
        /// <param name="typeid"></param>
        /// <returns></returns>
        public DataTable GetDataTable(int aId, int did, int pageIndex, int pageSize, string kw = "", int typeid = 0)
        {
            Tuple<string, List<MySqlParameter>> sqlResult = BuildSql(aId, did, pageIndex, pageSize, kw, typeid);
            return SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, sqlResult.Item1, sqlResult.Item2?.ToArray()).Tables[0];
        }
    }
}
