using DAL.Base;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Footbath;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.MiniApp.Footbath
{
    public class ServiceTimeBLL : BaseMySql<ServiceTime>
    {
        #region 单例模式
        private static ServiceTimeBLL _singleModel;
        private static readonly object SynObject = new object();

        private ServiceTimeBLL()
        {

        }

        public static ServiceTimeBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new ServiceTimeBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 从这一天算起为单周
        /// </summary>
        public static readonly string default_date = "2017-12-11";

        /// <summary>
        /// 足浴  -  当前时间是否为有效的可下单服务时间
        /// </summary>
        /// <param name="curOrderId">排除当前订单ID</param>
        /// <param name="curServiceTime">要查询的服务时间</param>
        /// <param name="curTechnicianId">技师的ID</param>
        /// <returns></returns>
        public bool IsEffTime(int curOrderId,string curServiceTime,int curTechnicianId)
        {
            bool isEffTime = SqlMySql.ExecuteNonQuery(connName, System.Data.CommandType.Text, $@"select 0 from entgoodscart a
                                                                                left JOIN entgoodsorder b on a.goodsorderid = b.id
                                                                                where reservationTime = '{curServiceTime}' and technicianId = {curTechnicianId} and a.state = 1 and b.state >= 0 and Id  != {curOrderId}") <= 0;
            return !isEffTime;
        }



        /// <summary>
        /// 获取预订技师时间列表
        /// </summary>
        /// <param name="shopTime"></param>
        /// <param name="timeInterval"></param>
        /// <param name="dates"></param>
        /// <returns></returns>
        public object GetTimeTable(SwitchModel switchModel, int days, int appId, int storeId, int tid)
        {
            DateTime date = DateTime.Now.AddDays(days);
            bool isSingleWeek = IsSingleWeek(date);
            string sqlwhere = $"aid={appId} and state>=0 and tid={tid}";
            if (isSingleWeek)
            {
                sqlwhere += " and daytype=0";
            }
            else
            {
                sqlwhere += " and daytype=1";
            }
            //获取技师当天排班情况
            Rota rota = RotaBLL.SingleModel.GetModel(sqlwhere);
            if (rota == null)
            {
                return null;
            }
            List<object> TimeTable = new List<object>();
            string worktimeStr = string.Empty;
            switch (Convert.ToInt32(date.DayOfWeek))
            {
                //周日
                case 0:
                    if (!switchModel.Sunday)//店铺周日没有营业
                        return null;
                    worktimeStr = rota.sunday;
                    break;
                //周一
                case 1:
                    if (!switchModel.Monday)//店铺周一没有营业
                        return null;
                    worktimeStr = rota.monday;
                    break;
                //周二
                case 2:
                    if (!switchModel.Tuesday)//店铺周二没有营业
                        return null;
                    worktimeStr = rota.tuesday;
                    break;
                //周三
                case 3:
                    if (!switchModel.Wensday)//店铺周三没有营业
                        return null;
                    worktimeStr = rota.wensday;
                    break;
                //周四
                case 4:
                    if (!switchModel.Thursday)//店铺周四没有营业
                        return null;
                    worktimeStr = rota.thursday;
                    break;
                //周五
                case 5:
                    if (!switchModel.Friday)//店铺周五没有营业
                        return null;
                    worktimeStr = rota.friday;
                    break;
                //周六
                case 6:
                    if (!switchModel.Saturday)//店铺周六没有营业
                        return null;
                    worktimeStr = rota.saturday;
                    break;
            }
            WorkTimeState workState = RotaBLL.SingleModel.GetDayState(worktimeStr);
            List<string> timeList = new List<string>();
            //早班
            if (workState.morning)
            {
                string startTime = switchModel.morningShift.Split('-')[0];
                DateTime start = Convert.ToDateTime(startTime);
                string endTime = switchModel.morningShift.Split('-')[1];
                DateTime end = Convert.ToDateTime(endTime);
                while (DateTime.Compare(end, start) > 0)
                {
                    timeList.Add(start.ToString("HH:mm"));
                    start = start.AddMinutes(switchModel.TimeInterval);
                    //TimeTable.Add(new { time = start.ToString("HH:mm"), state = false });
                }
            }
            //中班
            if (workState.noon)
            {
                string startTime = switchModel.noonShift.Split('-')[0];
                DateTime start = Convert.ToDateTime(startTime);
                string endTime = switchModel.noonShift.Split('-')[1];
                DateTime end = Convert.ToDateTime(endTime);
                while (DateTime.Compare(end, start) > 0)
                {
                    timeList.Add(start.ToString("HH:mm"));
                    start = start.AddMinutes(switchModel.TimeInterval);
                }
            }
            //晚班
            if (workState.evening)
            {
                string startTime = switchModel.eveningShift.Split('-')[0];
                DateTime start = Convert.ToDateTime(startTime);
                string endTime = switchModel.eveningShift.Split('-')[1];
                DateTime end = Convert.ToDateTime(endTime);
                while (DateTime.Compare(end, start) > 0)
                {
                    timeList.Add(start.ToString("HH:mm"));
                    start = start.AddMinutes(switchModel.TimeInterval);
                }
            }
            //去除已被选定的服务时间
            ServiceTime dates = GetModel($"aid={appId} and storeid={storeId} and tid={tid} and date='{DateTime.Now.AddDays(days).ToShortDateString()}'");
            if (dates != null)
            {
                List<string> list = dates.time.Split(',').ToList(); ;
                timeList = timeList.Except(list).ToList();
            }
            foreach (var timestr in timeList)
            {
                DateTime time = Convert.ToDateTime(timestr);
                //如果是今天，要去掉今天已经过去的时间
                if (DateTime.Compare(time, DateTime.Now) > 0 || days > 0)
                {
                    var obj = new
                    {
                        time = timestr,
                        state = false
                    };
                    TimeTable.Add(obj);
                }
            }
            return TimeTable;
        }

        /// <summary>
        /// 判断是否单周，不是则为双周
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsSingleWeek(DateTime date)
        {
            bool result = false;
            DateTime FirstWeek = Convert.ToDateTime(default_date);
            TimeSpan dayCount = date - FirstWeek;
            int count = dayCount.Days + 1;

            count = Convert.ToInt32(Math.Ceiling(count / 7.0));
            result = count % 2 > 0;
            return result;
        }

        /// <summary>
        /// 客户预订成功后将选定的时间点添加到已服务时间表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="goodsCart"></param>
        public void AddSelServiceTime(FootBath store, EntGoodsCart goodsCart, SwitchModel switchModel = null)
        {
            ServiceTime serviceTimeModel = GetModel($"aid={store.appId} and storeid={store.Id} and tid={goodsCart.technicianId} and date='{goodsCart.reservationTime.ToShortDateString()}'");
            if (serviceTimeModel == null)
            {
                serviceTimeModel = new ServiceTime();
                serviceTimeModel.aid = store.appId;
                serviceTimeModel.storeId = store.Id;
                serviceTimeModel.tid = goodsCart.technicianId;
                serviceTimeModel.date = Convert.ToDateTime(goodsCart.reservationTime.ToShortDateString());
                serviceTimeModel.time = goodsCart.reservationTime.ToString("HH:mm");//SelTimeStr(goodsCart.reservationTime, goodsCart.goodsMsg.ServiceTime, switchModel.TimeInterval);
                serviceTimeModel.createTime = DateTime.Now;
                Add(serviceTimeModel);
            }
            else
            {
                serviceTimeModel.time += "," + goodsCart.reservationTime.ToString("HH:mm"); //SelTimeStr(goodsCart.reservationTime, goodsCart.goodsMsg.ServiceTime, switchModel.TimeInterval);
                serviceTimeModel.date = Convert.ToDateTime(goodsCart.reservationTime.ToShortDateString());
                serviceTimeModel.createTime = DateTime.Now;
                MySqlParameter[] arr = null;
                string strSql = BuildUpdateSql(serviceTimeModel, "time,createtime", out arr);
                Update(serviceTimeModel, "time,createtime,date");
            }

        }

        public string SelTimeStr(DateTime start, int timeLength, int TimeInterval)
        {
            List<string> timeList = new List<string>();
            timeList.Add(start.ToString("HH:mm"));
            DateTime end = start.AddMinutes(timeLength);
            while (DateTime.Compare(end, start) > 0)
            {
                start = start.AddMinutes(TimeInterval);
                timeList.Add(start.ToString("HH:mm"));
            }
            return string.Join(",", timeList);
        }
        /// <summary>
        /// 根据技师id，服务时间获取服务时间段
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="technicianId"></param>
        /// <param name="storeId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public ServiceTime GetModel(int aId, int technicianId, int storeId, string date)
        {
            string sqlwhere = $"aid={aId} and tid={technicianId} and storeid={storeId} and date='{date}'";
            return GetModel(sqlwhere);
        }
        /// <summary>
        /// 根据时间获取服务时间
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="storeId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public ServiceTime GetModelByDate(int aId, int storeId, string date)
        {
            return GetModel($"aid={aId} and storeid={storeId} and date='{date}'");
        }

        /// <summary>
        /// 验证服务时间是否已经被选
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="id"></param>
        /// <param name="technicianId"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool ValidChoosed(int aId, int storeId, int technicianId, DateTime date)
        {
            bool result = false;
            ServiceTime dates = GetModel($"aid={aId} and tid={technicianId} and storeid={storeId} and date='{date.ToShortDateString()}'");
            if (dates != null && !string.IsNullOrEmpty(dates.time))
            {
                List<string> list = dates.time.Split(',').ToList(); ;
                if (list.Contains(date.ToString("HH:mm")))
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 根据日期和技师id获取服务时间
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="technicianId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public ServiceTime GetModelByDate_Tid(int aId, int storeId, int technicianId, string date)
        {
            return GetModel($"aid={aId} and tid={technicianId} and storeid={storeId} and date='{date}'");
        }
    }
}
