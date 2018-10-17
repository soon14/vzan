using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 推广分享 产生的客户  分销员-客户-产品-保护期
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class SalesManRecordUser
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        /// 所属分销员Id
        /// </summary>
        [SqlField]
        public int salesManId { get; set; } = 0;

        /// <summary>
        /// 推广记录Id
        /// </summary>
        [SqlField]
        public int recordId { get; set; } = 0;

        /// <summary>
        /// 用户Id
        /// </summary>
        [SqlField]
        public int userId { get; set; } = 0;

        /// <summary>
        /// 分销产品Id
        /// </summary>
        [SqlField]
        public int goodsId { get; set; } = 0;


        /// <summary>
        /// 保护期天数
        /// </summary>
        [SqlField]
        public int protected_time { get; set; } = 0;

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }
        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 更新时间 
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        public string UpdateTimeStr
        {
            get
            {
                return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 总成交金额 元为单位
        /// </summary>
        public string orderMoneyStr { get; set; }
        /// <summary>
        /// 总佣金 元为单位
        /// </summary>
        public string cpsMoneyStr { get; set; }

        /// <summary>
        /// 订单成交数量
        /// </summary>
        public int orderCount { get; set; }

        public string NickName { get; set; }

        public string ImgLogo { get; set; }


        /// <summary>
        /// 当前剩余保护期天数
        /// </summary>
        public int cur_protected_time { get; set; } = 0;



    }

    

}
