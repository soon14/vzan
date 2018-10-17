using BLL.MiniApp.Stores;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Stores;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Ent
{
    public class ExchangePlayCardRelationBLL : BaseMySql<ExchangePlayCardRelation>
    {
        #region 单例模式
        private static ExchangePlayCardRelationBLL _singleModel;
        private static readonly object SynObject = new object();

        private ExchangePlayCardRelationBLL()
        {

        }

        public static ExchangePlayCardRelationBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ExchangePlayCardRelationBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion



        public ExchangePlayCardRelation GetExchangePlayCardRelation(int userId, int appId, out int resultCode)
        {
            bool blackout = false;//是否断签
            resultCode = 200;

            Store storeModel = StoreBLL.SingleModel.GetModelByAId(appId);
            if (storeModel == null || string.IsNullOrEmpty(storeModel.configJson))
            {
                resultCode = 404;//店铺配置信息没有找到
                return null;
            }
            ExchangePlayCardConfig exchangePlayCardConfig = new ExchangePlayCardConfig();
            storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);
            if (string.IsNullOrEmpty(storeModel.funJoinModel.ExchangePlayCardConfig))
            {
                resultCode = 403;//签到配置信息没有找到
                return null;
            }

            exchangePlayCardConfig = JsonConvert.DeserializeObject<ExchangePlayCardConfig>(storeModel.funJoinModel.ExchangePlayCardConfig);
            if (exchangePlayCardConfig.State == 0)
            {
                resultCode = 302;//签到开关关闭
                return null;
            }


            #region 表示空白从未打卡过
            ExchangePlayCardRelation exchangePlayCardRelation = base.GetModel($"userId={userId} and Aid={appId}");
            if (exchangePlayCardRelation == null)
            {
                exchangePlayCardRelation = new ExchangePlayCardRelation() { Aid = appId, UserId = userId, UpdateTime = DateTime.Now };
                for (int i = 0; i < 7; i++)
                {
                    exchangePlayCardRelation.listPlayCardLog.Add(new ExchangePlayCardLog()
                    {

                        Aid = appId,
                        UserId = userId,
                        AddTime = DateTime.Now.AddMinutes(i),
                        Played = false,
                        Points = exchangePlayCardConfig.DayGivePoints

                    });
                }

                return exchangePlayCardRelation;
            }
            #endregion

            ExchangePlayCardLog exchangePlayCardLog = ExchangePlayCardLogBLL.SingleModel.GetModel($"userId={userId} and Aid={appId}");
            if (exchangePlayCardLog != null)
            {
                //判断是否断签
                double day = CommondHelper.DateDiff(Convert.ToDateTime(exchangePlayCardLog.AddTime.ToString("yyyy-MM-dd")), Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))).TotalDays;
                if (day > 1)//大于1表示断签了隔了至少一天没有签到
                {
                    blackout = true;
                }
            }



            if (blackout)
            {
                //断签则将连续签到天数归零以及上次赠送积分的连续签到的天数归零

                exchangePlayCardRelation.ConnectDay = 0;
                exchangePlayCardRelation.LastConnectDay = 0;
                exchangePlayCardRelation.UpdateTime = DateTime.Now;
                if (!base.Update(exchangePlayCardRelation, "ConnectDay,LastConnectDay,UpdateTime"))
                {
                    resultCode = 500;//说明异常

                }

                for (int i = 0; i < 7; i++)
                {
                    exchangePlayCardRelation.listPlayCardLog.Add(new ExchangePlayCardLog()
                    {

                        Aid = appId,
                        UserId = userId,
                        AddTime = DateTime.Now.AddMinutes(i),
                        Played = false,
                        Points = exchangePlayCardConfig.DayGivePoints
                    });
                }

                return exchangePlayCardRelation;

            }

            //没有断签 返回连续签到信息
            int tempDay = exchangePlayCardRelation.ConnectDay;
            if (tempDay > 7)
            {
                tempDay= exchangePlayCardRelation.ConnectDay % 7;
            }
            exchangePlayCardRelation.listPlayCardLog = ExchangePlayCardLogBLL.SingleModel.GetList($"userId={userId} and Aid={appId}",  tempDay, 1, "*", "AddTime desc");
            if (tempDay<7)
            {
                exchangePlayCardRelation.listPlayCardLog= exchangePlayCardRelation.listPlayCardLog.OrderBy(x => x.AddTime).ToList();

                for (int i = 0; i < 7 - tempDay; i++)
                {
                    exchangePlayCardRelation.listPlayCardLog.Add(new ExchangePlayCardLog()
                    {

                        Aid = appId,
                        UserId = userId,
                        AddTime = DateTime.Now.AddMinutes(i),
                        Played = false,
                        Points = exchangePlayCardConfig.DayGivePoints
                    });
                }
            }
              ExchangePlayCardLog todayPlayCardLog = ExchangePlayCardLogBLL.SingleModel.GetModel($"userId={userId} and Aid={appId} and to_days(AddTime) = to_days(now())");
           //测试用 ExchangePlayCardLog todayPlayCardLog = _exchangePlayCardLogBLL.GetModel($"userId={userId} and Aid={appId} and AddTime>NOW()-INTERVAL 2 MINUTE");// 
            if (todayPlayCardLog != null)//2分钟之内
            {
                //今天已经签到
                exchangePlayCardRelation.TodayPlayCard = true;
            }


            return exchangePlayCardRelation;


        }

        public ExchangePlayCardRelation PlayCard(int userId, int appId, out int resultCode)
        {

            //插入打卡记录,根据规则进行积分更新相加,最后更新签到信息
            resultCode = 200;
            Store storeModel = StoreBLL.SingleModel.GetModelByAId(appId);
            if (storeModel == null || string.IsNullOrEmpty(storeModel.configJson))
            {
                resultCode = 500;//店铺配置信息没有找到
                return null;
            }
            ExchangePlayCardLog todayPlayCardLog = ExchangePlayCardLogBLL.SingleModel.GetModel($"userId={userId} and Aid={appId} and to_days(AddTime) = to_days(now())"); 
            if (todayPlayCardLog != null)
            {
                resultCode = 403;//今天已签到
                return null;
            }


            ExchangePlayCardConfig exchangePlayCardConfig = new ExchangePlayCardConfig();
            storeModel.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeModel.configJson);
            if (string.IsNullOrEmpty(storeModel.funJoinModel.ExchangePlayCardConfig))
            {
                resultCode = 404;//签到配置信息没有找到
                return null;
            }

            exchangePlayCardConfig = JsonConvert.DeserializeObject<ExchangePlayCardConfig>(storeModel.funJoinModel.ExchangePlayCardConfig);
            if (exchangePlayCardConfig.State == 0)
            {
                resultCode = 302;//签到开关关闭
                return null;
            }



            TransactionModel transactionModel = new TransactionModel();

            bool havePlayCardRelation = true;
            ExchangePlayCardRelation exchangePlayCardRelation = base.GetModel($"userId={userId} and Aid={appId}");
            if (exchangePlayCardRelation == null)
            {
                exchangePlayCardRelation = new ExchangePlayCardRelation();
                exchangePlayCardRelation.UserId = userId;
                exchangePlayCardRelation.Aid = appId;
                exchangePlayCardRelation.ConnectDay = 1;
                exchangePlayCardRelation.LastConnectDay = 0;
                exchangePlayCardRelation.UpdateTime = DateTime.Now;
                havePlayCardRelation = false;
                transactionModel.Add(base.BuildAddSql(exchangePlayCardRelation));
            }
            else
            {
                exchangePlayCardRelation.ConnectDay += 1;
                exchangePlayCardRelation.UpdateTime = DateTime.Now;
            }



            //获取签到送积分规则
            int curSumIntegral = 0;

            curSumIntegral += exchangePlayCardConfig.DayGivePoints;//每天签到送多少积分
            int tempDay = exchangePlayCardRelation.ConnectDay - exchangePlayCardRelation.LastConnectDay;
            if (tempDay < 0)
            {
                resultCode = 502;//签到天数错误
                return null;
            }
            ExchangePlayCardLog exchangePlayCardLog = new ExchangePlayCardLog();
            exchangePlayCardLog.AddTime = DateTime.Now;
            exchangePlayCardLog.Aid = appId;
            exchangePlayCardLog.UserId = userId;
            if (tempDay >= exchangePlayCardConfig.ConnectDay && havePlayCardRelation)
            {
                curSumIntegral += exchangePlayCardConfig.ConnectDayGivePoints;
                exchangePlayCardLog.Remark = $"满足连续签到{exchangePlayCardConfig.ConnectDayGivePoints}";
                exchangePlayCardRelation.LastConnectDay += exchangePlayCardConfig.ConnectDay;//记录上次连续赠送积分的天数断签后清零
            }
            if (curSumIntegral < 0)
            {
                resultCode = 503;//签到送积分计算错误
                return null;
            }



            exchangePlayCardLog.Points = curSumIntegral;
            exchangePlayCardLog.Remark += $"每天签到送{exchangePlayCardConfig.DayGivePoints}";

            transactionModel.Add(ExchangePlayCardLogBLL.SingleModel.BuildAddSql(exchangePlayCardLog));

            if (havePlayCardRelation)
            {
                transactionModel.Add(base.BuildUpdateSql(exchangePlayCardRelation, "ConnectDay,UpdateTime,LastConnectDay"));
            }

            ExchangeUserIntegral exchangeUserIntegral = ExchangeUserIntegralBLL.SingleModel.GetModel($"userId={userId}");
            if (exchangeUserIntegral != null)
            {
                exchangeUserIntegral.integral = exchangeUserIntegral.integral + curSumIntegral;
                exchangeUserIntegral.UpdateDate = DateTime.Now;

                transactionModel.Add(ExchangeUserIntegralBLL.SingleModel.BuildUpdateSql(exchangeUserIntegral, "integral,UpdateDate"));
            }
            else
            {
                //表示新增
                exchangeUserIntegral = new ExchangeUserIntegral { UserId = userId, integral = curSumIntegral, AddTime = DateTime.Now, UpdateDate = DateTime.Now };
                transactionModel.Add(ExchangeUserIntegralBLL.SingleModel.BuildAddSql(exchangeUserIntegral));

            }

            //积分变动日志
            transactionModel.Add(ExchangeUserIntegralLogBLL.SingleModel.BuildAddSql(new ExchangeUserIntegralLog
            {
                ruleId = -1,
                appId = appId,
                integral = curSumIntegral,
                ruleType = -1,
                userId = userId,
                actiontype = 0,
                curintegral = curSumIntegral,
                AddTime = DateTime.Now,
                UpdateDate = DateTime.Now,
                ordertype = 3
            }));

            if (!base.ExecuteTransactionDataCorect(transactionModel.sqlArray))
            {
                resultCode = -1;
                return null;
            }

            exchangePlayCardRelation = base.GetModel($"userId={userId} and Aid={appId}");

            exchangePlayCardRelation.TodayPlayCard = true;
            int tempDay2 = exchangePlayCardRelation.ConnectDay;
            if (tempDay2 > 7)
            {
                tempDay2 = exchangePlayCardRelation.ConnectDay % 7;
            }
            exchangePlayCardRelation.listPlayCardLog = ExchangePlayCardLogBLL.SingleModel.GetList($"userId={userId} and Aid={appId}", tempDay2, 1, "*", "AddTime desc");
            if (tempDay2 < 7)
            {
                exchangePlayCardRelation.listPlayCardLog= exchangePlayCardRelation.listPlayCardLog.OrderBy(x => x.AddTime).ToList();
                for (int i = 0; i < 7 - tempDay2; i++)
                {
                    exchangePlayCardRelation.listPlayCardLog.Add(new ExchangePlayCardLog()
                    {

                        Aid = appId,
                        UserId = userId,
                        AddTime = DateTime.Now.AddMinutes(i),
                        Played = false,
                        Points = exchangePlayCardConfig.DayGivePoints
                    });
                }
            }
            return exchangePlayCardRelation;

        }

        public List<UserPointsInfo> GetUserPointsInfoList(int appId, out int totalCount, int pageIndex = 1, int pageSize = 10, string nickName = "")
        {
            totalCount = 0;
            string sqlWhere = string.Empty;
            List<UserPointsInfo> list = new List<UserPointsInfo>();
            List<MySqlParameter> mysqlParameter = new List<MySqlParameter>();
            if (!string.IsNullOrEmpty(nickName))
            {
                sqlWhere = " where u.NickName like @userName";
                mysqlParameter.Add(new MySql.Data.MySqlClient.MySqlParameter("@userName", "%" + nickName + "%"));

            }
            string sql = $"SELECT u.NickName,u.HeadImgUrl ,userid,points,playcardnum from (SELECT userid, SUM(curintegral) as points,COUNT(Id) as playcardnum from exchangeuserintegrallog where appid = {appId} and ordertype = 3 GROUP BY userId) p LEFT JOIN c_userinfo u on u.id = p.userId  {sqlWhere}   limit {pageSize * (pageIndex - 1)},{pageSize}";
            DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, mysqlParameter.ToArray());
            if (ds.Tables == null || ds.Tables.Count <= 0)
            {
                return list;
            }


            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows.Count <= 0)
                return list;

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new UserPointsInfo()
                {
                    UserId = Convert.ToInt32(row["userid"]),
                    Name = row["NickName"] != DBNull.Value ? row["NickName"].ToString() : string.Empty,
                    HeaderImg = row["HeadImgUrl"] != DBNull.Value ? row["HeadImgUrl"].ToString() : string.Empty,
                    TotalPoints = Convert.ToInt32(row["points"]),
                    PlayCardNum = Convert.ToInt32(row["playcardnum"])
                });

            }


            sql = $"SELECT COUNT(userid) as number from (SELECT userid from exchangeuserintegrallog where appid = {appId} and ordertype = 3 GROUP BY userId) p LEFT JOIN c_userinfo u on u.id = p.userId {sqlWhere}";
            var obj = SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql, mysqlParameter.ToArray());
            if (obj != DBNull.Value)
            {
                totalCount = Convert.ToInt32(obj);
            }
            return list;



        }


    }
}
