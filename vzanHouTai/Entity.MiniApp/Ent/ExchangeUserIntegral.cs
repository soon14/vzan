using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 用户在每个小程序对应的积分(订单确认收货后开始计算本次消费获赠积分)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ExchangeUserIntegral
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户ID 同一个微信号对每个小程序的用户Id都不同
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;
       
        /// <summary>
        /// 总积分
        /// </summary>
        [SqlField]
        public int integral { get; set; } = 0;


       

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }
        public string UpdateDateStr
        {
            get
            {
                return UpdateDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        

    }



}
