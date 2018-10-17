using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class FullReduction
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 满减订单金额
        /// </summary>
        [SqlField]
        public int OrderMoney { get; set; }

        public string OrderMoneyStr
        {
            get
            {
                return (OrderMoney * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 优惠金额
        /// </summary>
        [SqlField]
        public int ReducetionMoney { get; set; }
        public string ReducetionMoneyStr
        {
            get
            {
                return (ReducetionMoney * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        [SqlField]
        public DateTime StartTime { get; set; }

        public string StartTimeStr {
            get
            {

                return StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public string EndTimeStr
        {
            get
            {

                return EndTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        [SqlField]
        public DateTime EndTime { get; set; }


        /// <summary>
        /// 状态 0正常 -1删除
        /// </summary>
        [SqlField]
        public int IsDel { get; set; }


        /// <summary>
        /// 使用次数
        /// </summary>
        [SqlField]
        public int UseCount { get; set; }


        /// <summary>
        /// 是否与会员折扣互斥 0表示否 1表示是
        /// </summary>
        [SqlField]
        public int VipDiscount { get; set; }



        public int State
        {
            get
            {
                if(DateTime.Compare(StartTime, DateTime.Now) < 0&& DateTime.Compare(EndTime, DateTime.Now) > 0)
                {
                    return 1;//有效时间内 
                }else if(DateTime.Compare(StartTime, DateTime.Now) > 0)
                {
                    return 0;//活动未开始
                }
                else
                {
                    return -1;//活动已结束
                }
            }
        }

        public string StateStr
        {
            get
            {
                string s = "进行中";
                switch (State)
                {
                    case 0:
                        s = "活动未开始";
                        break;
                    case -1:
                        s = "活动已结束";
                        break;
                }
                return s;
            }
        }








    }
}
