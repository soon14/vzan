using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 分享时候触发 分享成功后将state状态变为1表示可用
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class SalesManRecord
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
        /// 分销员记录表ID
        /// </summary>
        [SqlField]
        public int salesManId { get; set; } = 0;


        /// <summary>
        /// 分销产品ID
        /// </summary>
        [SqlField]
        public int salesmanGoodsId { get; set; } = 0;


        /// <summary>
        /// 分享产品时候小程序的分享设置  该设置随时可能会变 所以这里记录当时分享的出去时候的设置
        /// </summary>
        [SqlField]
        public string configStr { get; set; } = string.Empty;


        public ConfigModel configModel { get; set; }

        /// <summary>
        /// 产品佣金比例 分享推广时候的 随时可能变,这里记录的是分享推广时候的
        /// </summary>
        [SqlField]
        public double cps_rate { get; set; } = 0.00;


        /// <summary>
        /// 该记录是否可用 0不可用 1可用
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

       

        /// <summary>
        /// 分享推广时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

      

    }



}
