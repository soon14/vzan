using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class ExchangePlayCardRelation
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; }


        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }

        /// <summary>
        /// 连续签到天数  每次断签则归零,否则累加
        /// </summary>
        [SqlField]
        public int ConnectDay { get; set; }

        /// <summary>
        /// 上一次连续签到赠送积分的累计天数 断签后清零
        /// 由于连续签到天数是累加的,但是达到条件后会赠送积分
        /// 所以这里需要记录下来,上次赠送积分的天数,如果中途没有断签下次赠送积分需要减掉
        /// </summary>
        [SqlField]
        public int LastConnectDay { get; set; }


        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 连续签到记录
        /// </summary>
        public List<ExchangePlayCardLog> listPlayCardLog = new List<ExchangePlayCardLog>();


       

        /// <summary>
        /// 今天是否签到了
        /// </summary>
        public bool TodayPlayCard { get; set; } = false;


    }


    /// <summary>
    /// 签到会员展示
    /// </summary>
    public class UserPointsInfo
    {
        public int UserId { get; set; }
        public string Name { get; set; }

        public string HeaderImg { get; set; }

        public int TotalPoints { get; set; }

        /// <summary>
        /// 累计总签到数量
        /// </summary>
        public int PlayCardNum { get; set; }

    }





  

}
