using Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 秒杀用户支付记录
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class FlashDealPayment
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int Aid { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 活动ID
        /// </summary>
        [SqlField]
        public int DealId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 物品ID
        /// </summary>
        [SqlField]
        public int ItemId { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        [SqlField]
        public int BuyCount { get; set; }
        /// <summary>
        /// 支付订单MorderID
        /// </summary>
        [SqlField]
        public int PayOrderId { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        [SqlField]
        public int PayPrice { get; set; }
        /// <summary>
        /// 支付方式（枚举:miniAppBuyMode）
        /// </summary>
        [SqlField]
        public int PayWay { get; set; }
        /// <summary>
        /// 支付快照
        /// </summary>
        [SqlField]
        public string Snapshot { get; set; }
        /// <summary>
        /// 获取序列化支付快照
        /// </summary>
        public DealSnapshot GetSnapshot()
        {
            return JsonConvert.DeserializeObject<DealSnapshot>(Snapshot);
        }
    }

    /// <summary>
    /// 秒杀活动支付快照
    /// </summary>
    public class DealSnapshot : FlashDeal
    {
        /// <summary>
        /// 秒杀物品标题
        /// </summary>
        public string ItemTitle { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public int OrigPrice { get; set; }
        /// <summary>
        /// 支付方式文案
        /// </summary>
        public string PayWay { get; set; }
    }
}
