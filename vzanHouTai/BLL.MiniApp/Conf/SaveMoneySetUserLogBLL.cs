using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL.MiniApp.Conf
{
    public class SaveMoneySetUserLogBLL : DAL.Base.BaseMySql<SaveMoneySetUserLog>
    {
        #region 单例模式
        private static SaveMoneySetUserLogBLL _singleModel;
        private static readonly object SynObject = new object();

        private SaveMoneySetUserLogBLL()
        {

        }

        public static SaveMoneySetUserLogBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new SaveMoneySetUserLogBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public SaveMoneySetUserLog getModelByAppId(int Id, string AppId)
        {
            return GetModel($" Id = {Id} and AppId = '{AppId}' ");
        }
        public SaveMoneySetUserLog getModelByOrderId(int OrderId)
        {
            return GetModel($" OrderId = {OrderId} ");
        }
        public List<SaveMoneySetUserLog> getListByUserId(int userInfoId,string appId)
        {
            return GetList($" UserId = {userInfoId} and AppId = '{appId}' and State = 1 ");
        }
        public List<SaveMoneySetUserLog> GetListByUserId(int userInfoId, string appId,int pageSize,int pageIndex)
        {
            List<SaveMoneySetUserLog> list = new List<SaveMoneySetUserLog>();
            string sql = $@"select * from (select id,appid,userid,moneysetuserid,moneysetid,type,beforemoney,aftermoney,changemoney,changenote,createdate,state,orderid,storeid from SaveMoneySetUserLog where appid='{appId}' and userid={userInfoId} and state=1
                UNION
                select id,appid,fuserid userid,0 moneysetuserid,0 moneysetid,-2 type,0 beforemoney,0 aftermoney,payment_free changemoney, shownote changenote,addtime createdate, status state,0 orderid,storeid from citymorders where appid = '{appId}' and fuserid = {userInfoId} and status = 1 and OrderType={(int)ArticleTypeEnum.MiniappWXDirectPay}
                ) d ORDER BY d.createdate desc LIMIT { (pageIndex - 1) * pageSize},{ pageSize}";
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReaderMaster(connName, CommandType.Text, sql, null))
            {
                while (dr.Read())
                {
                    SaveMoneySetUserLog amodel = base.GetModel(dr);
                    list.Add(amodel);
                }
            }

            return list;
        }
        /// <summary>
        /// 会员充值记录金额
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <returns></returns>
        public int GetVipPayLog(string appId, string startdate, string enddate,int storeid=-1)
        {
            //购买记录
            var sql = $@"select sum(changemoney) from SaveMoneySetUserLog where appid = '{appId}' and state=0 and createdate>='{startdate}' and createdate<='{enddate}'";
            //多门店需要判断哪个门店
            if(storeid >= 0)
            {
                sql += " and storeId="+storeid;
            }
            var result = SqlMySql.ExecuteScalar(connName, CommandType.Text, sql);
            int sum = 0;
            if (DBNull.Value != result)
            {
                sum = Convert.ToInt32(result);
            }

            return sum;
        }
    }

    public class SaveMoneySetUserLogViewBLL : DAL.Base.BaseMySql<SaveMoneySetUserLogView>
    {

        public List<SaveMoneySetUserLogView> getListByCondition(string AppId, DateTime? changDateStart, DateTime? changDateEnd, string nickName = "", int changType = -999, string changeNote = "",int pageIndex = 1,int pageSize = 10)
        {
            string whereStr = @"select c.nickname,c.headimgurl,l.BeforeMoney,l.changemoney,l.AfterMoney,l.Type,
					                        case l.Type when 0 then '充值' when -1 then '消费' when 2 then '后台手动修改' ELSE '退款' END as TypeStr,
				                        l.ChangeNote,l.CreateDate 
                                from SaveMoneySetUserLog l
                                LEFT JOIN c_userinfo c on l.userid = c.id";

            List<string> whereList = new List<string>();
            List<MySql.Data.MySqlClient.MySqlParameter> paramList = new List<MySql.Data.MySqlClient.MySqlParameter>();

            whereList.Add($" where l.State = 1 and l.AppId = '{AppId}' ");
            if (changDateStart != null)
            {
                whereList.Add($"  l.CreateDate >= '{changDateStart?.ToString("yyyy-MM-dd 00:00:00")}' ");
            }
            if (changDateEnd != null)
            {
                whereList.Add($"  l.CreateDate <= '{changDateEnd?.ToString("yyyy-MM-dd 23:59:59")}' ");
            }
            if (!string.IsNullOrWhiteSpace(nickName))
            {
                whereList.Add($"  c.nickname = @nickName ");
                paramList.Add(new MySql.Data.MySqlClient.MySqlParameter("@nickName", nickName));
            }
            if (changType != -999)
            {
                whereList.Add($"  l.type = {changType} ");
            }
            if (!string.IsNullOrWhiteSpace(changeNote))
            {
                whereList.Add($"  l.changeNote like @changeNote ");
                paramList.Add(new MySql.Data.MySqlClient.MySqlParameter("@changeNote", "%" + changeNote + "%"));
            }
            

            string sqlStr = whereStr + string.Join(" and ", whereList) + " order by l.id desc " + $" limit {(pageIndex - 1) * pageSize},{pageSize} ";
            List <SaveMoneySetUserLogView> List = new List<SaveMoneySetUserLogView>();
            
            
            using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, System.Data.CommandType.Text, sqlStr, paramList.ToArray()))
            {
                while(dr.Read())
                {
                   var model = GetModel(dr);
                   List.Add(model);
                }
            }

            return List;
        }



        public int getCountByCondition(string AppId, DateTime? changDateStart, DateTime? changDateEnd, string nickName = "", int changType = -999, string changeNote = "", int pageIndex = 1, int pageSize = 10)
        {
            string whereStr = @"select count(*)
                                from SaveMoneySetUserLog l
                                LEFT JOIN c_userinfo c on l.userid = c.id";

            List<string> whereList = new List<string>();
            List<MySql.Data.MySqlClient.MySqlParameter> paramList = new List<MySql.Data.MySqlClient.MySqlParameter>();

            whereList.Add($" where l.State = 1 and l.AppId = '{AppId}' ");
            if (changDateStart != null)
            {
                whereList.Add($"  l.CreateDate >= '{changDateStart?.ToString("yyyy-MM-dd 00:00:00")}' ");
            }
            if (changDateEnd != null)
            {
                whereList.Add($"  l.CreateDate <= '{changDateEnd?.ToString("yyyy-MM-dd 23:59:59")}' ");
            }
            if (!string.IsNullOrWhiteSpace(nickName))
            {
                whereList.Add($"  c.nickname = @nickName ");
                paramList.Add(new MySql.Data.MySqlClient.MySqlParameter("@nickName", nickName));
            }
            if (changType != -999)
            {
                whereList.Add($"  l.type = {changType} ");
            }
            if (!string.IsNullOrWhiteSpace(changeNote))
            {
                whereList.Add($"  l.changeNote like @changeNote ");
                paramList.Add(new MySql.Data.MySqlClient.MySqlParameter("@changeNote", "%" + changeNote + "%"));
            }
            var count = Convert.ToInt32(SqlMySql.ExecuteScalar(connName, System.Data.CommandType.Text, whereStr + string.Join(" and ", whereList), paramList.ToArray()));

            //List<MiniAppSaveMoneySetUserLogView> List = new List<MiniAppSaveMoneySetUserLogView>();
            //using (MySqlDataReader dr = SqlMySql.ExecuteDataReader(connName, System.Data.CommandType.Text, whereStr + string.Join(" and ", whereList), null))
            //{
            //    if (dr.Read())
            //    {
            //        var model = GetModel(dr);
            //        List.Add(model);
            //    }
            //}

            return count;
        }
    }
}
