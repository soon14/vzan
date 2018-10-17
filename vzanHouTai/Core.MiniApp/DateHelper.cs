using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.MiniApp
{
    public static class DateHelper
    {
        /// <summary>  
        /// 得到本周第一天(以星期一为第一天)  
        /// </summary>  
        /// <param name="dateTime"></param>  
        /// <returns></returns>  
        public static DateTime GetWeekFirstDayMon(DateTime dateTime)
        {
            //星期一为第一天  
            int weeknow = Convert.ToInt32(dateTime.DayOfWeek);

            //因为是以星期一为第一天，所以要判断weeknow等于0时，要向前推6天。  
            weeknow = (weeknow == 0 ? (7 - 1) : (weeknow - 1));
            int daydiff = (-1) * weeknow;

            //本周第一天  
            string FirstDay = dateTime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(FirstDay);
        }

        /// <summary>  
        /// 得到本周最后一天(以星期天为最后一天)  
        /// </summary>  
        /// <param name="dateTime"></param>  
        /// <returns></returns>  
        public static DateTime GetWeekLastDaySun(DateTime dateTime)
        {
            //星期天为最后一天  
            int weeknow = Convert.ToInt32(dateTime.DayOfWeek);
            weeknow = (weeknow == 0 ? 7 : weeknow);
            int daydiff = (7 - weeknow);

            //本周最后一天  
            string LastDay = dateTime.AddDays(daydiff).ToString("yyyy-MM-dd");
            return Convert.ToDateTime(LastDay);
        }

        public static string GetDateTimeFormat(DateTime date)
        {
            string datestr = "";
            int day = GetDays(DateTime.Now,date);
            switch(day)
            {
                case 0:
                    datestr = "今天 " + date.ToString("HH:mm:ss");
                    break;
                case 1:
                    datestr = "昨天 " + date.ToString("HH:mm:ss");
                    break;
                default:
                    datestr = date.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
            }

            return datestr;
        }

        public static DateTime GetMonthFirstDay(DateTime dateTime)
        {
            DateTime firstDate = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDate;
        }

        public static DateTime GetMonthLastDay(DateTime dateTime)
        {
            DateTime firstDate = new DateTime(dateTime.Year, dateTime.Month, 1);
            DateTime lastDate = firstDate.AddMonths(1).AddDays(-1);
            return lastDate;
        }

        /// <summary>
        /// 获取两个时间的间隔天数
        /// </summary>
        /// <returns></returns>
        public static int GetDays(DateTime date,DateTime date2)
        {
            DateTime bigDate = date;
            DateTime smallDate = date2;
            if (date<date2)
            {
                bigDate = date2;
                smallDate = date;
            }
            int days = 0;
            TimeSpan bts = new TimeSpan(bigDate.Ticks);
            TimeSpan sts = new TimeSpan(smallDate.Ticks);
            int bday = bts.Days;
            int sday = sts.Days;
            //相差天数
            days = bday- sday;
            return days;
        }

        /// <summary>
        /// 获取两个时间的间隔
        /// </summary>
        /// <param name="date"></param>
        /// <param name="date2"></param>
        /// <param name="type">0：年，1：月，2：天，3：时，4：分，5：秒</param>
        /// <returns></returns>
        public static long GetDataCount(DateTime date, DateTime date2,int type)
        {
            DateTime bigDate = date;
            DateTime smallDate = date2;
            if (date < date2)
            {
                bigDate = date2;
                smallDate = date;
            }
            //TimeSpan bts = new TimeSpan(bigDate.Ticks);
            //TimeSpan sts = new TimeSpan(smallDate.Ticks);

            long counts = 0;
            //long bCounts = 0;
            //long sCounts = 0;
            //switch (type)
            //{
            //    case 0:
            //        break;
            //    case 1:
            //        break;
            //    case 2:
            //        bCounts = Convert.ToInt64(bts.TotalDays);
            //        sCounts = Convert.ToInt64(sts.TotalDays);
            //        break;
            //    case 3:
            //        bCounts = Convert.ToInt64(bts.TotalHours);
            //        sCounts = Convert.ToInt64(sts.TotalHours);
            //        break;
            //    case 4:
            //        bCounts = Convert.ToInt64(bts.TotalMinutes);
            //        sCounts = Convert.ToInt64(sts.TotalMinutes);
            //        break;
            //    case 5:
            //        bCounts = bts.Seconds;
            //        sCounts = sts.Seconds;
            //        break;
            //}

            //相差间隔
            //counts = bCounts - sCounts;
            counts = Convert.ToInt64(bigDate.Subtract(smallDate).TotalSeconds);
            
            return counts;
        }

        /// <summary>
        /// 秒转化为天小时分秒字符串
        /// 0:天，1：时，2：分，3：秒
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static long[] FormatSeconds(long seconds)
        {
            long[] dateArray = new long[4] { 0,0,0,0};
            if (seconds > 60)
            {
                long second = seconds % 60;
                long min = seconds / 60;
                dateArray[3] = second;
                dateArray[2] = min;
                if (min > 60)
                {
                    min = (seconds / 60) % 60;
                    long hour = (seconds / 60) / 60;
                    
                    dateArray[2] = min;
                    dateArray[1] = hour;
                    //if (hour > 24)
                    //{
                    //    hour = ((seconds / 60) / 60) % 24;
                    //    long day = (((seconds / 60) / 60) / 24);
                    //    dateArray[1] = hour;
                    //    dateArray[0] = day;
                    //}
                }
            }

            return dateArray;
        }
    }
}
