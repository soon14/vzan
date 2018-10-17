using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序商城模板-商品订单日志表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsOrderLog
    {
        public FoodGoodsOrderLog() { }
        /// <summary>
        /// id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商品订单id
        /// </summary>
        [SqlField]
        public int GoodsOrderId { get; set; }
        /// <summary>
        /// 用户openid
        /// </summary>
        [SqlField]
        public string UserId { get; set; }
        /// <summary>
        /// 日志详情
        /// </summary>
        [SqlField]
        public string LogInfo { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        public string UserName { get; set; }
    }
}
