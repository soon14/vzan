using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 秒杀活动（限时抢购）
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class FlashDeal
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int Aid { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 活动标题
        /// </summary>
        [SqlField]
        public string Title { get; set; }
        /// <summary>
        /// 广告图
        /// </summary>
        [SqlField]
        public string Banner { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        [SqlField]
        public DateTime Begin { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime End { get; set; }
        /// <summary>
        /// 数量限制（订单/X件）
        /// </summary>
        [SqlField]
        public int AmountLimit { get; set; }
        /// <summary>
        /// 订单数限制（物品/x单）
        /// </summary>
        [SqlField]
        public int OrderLimit { get; set; }
        /// <summary>
        /// 活动规则描述
        /// </summary>
        [SqlField]
        public string Description { get; set; }
        /// <summary>
        /// 活动状态（枚举：FlashDealState）
        /// </summary>
        [SqlField]
        public int State { get; set; } = (int)FlashDealState.已下架;
        public string StateStr
        {
            get
            {
                return Enum.GetName(typeof(FlashDealState), State);
            }
        }
        public int ItemCount { get; set; }
        /// <summary>
        /// 操作信息（手动下架、活动到期结束等）
        /// </summary>
        [SqlField]
        public string OperMsg { get; set; }

        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool sel { get; set; }

    }
}