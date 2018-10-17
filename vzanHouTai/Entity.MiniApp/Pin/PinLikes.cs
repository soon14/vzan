using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using System.ComponentModel.DataAnnotations;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 我的收藏
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinLikes
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int userId { get; set; } = 0;

        /// <summary>
        /// 类型：0=产品，1=店铺,2=评价
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// type=0时 likeId=产品ID，type=1时 likeId=店铺ID，type=3时 likeid=订单评价ID
        /// </summary>
        [SqlField]
        public int likeId { get; set; } = 0;

        /// <summary>
        /// 快照
        /// 是产品时，保存产品收藏时的价格信息，如果价格有变动用于显示变动提示
        /// </summary>
        [SqlField]
        public string snapshot { get; set; } = string.Empty;

        /// <summary>
        /// 状态 1=正常，-1=取消收藏
        /// </summary>
        //[SqlField]
        //public int state { get; set; } = 1;

        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;


        public PinGoods likeGood = null;

        public PinStore likeStore = null;
    }
}
