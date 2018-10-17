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
    /// 预约点餐订单
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodReservation
    {
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField]
        public int FoodId { get; set; }
        /// <summary>
        /// 小程序模板ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }
        /// <summary>
        /// 座位ID
        /// </summary>
        [SqlField]
        public int TableId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 预约时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 就餐时间
        /// </summary>
        [SqlField]
        public DateTime DinnerTime { get; set; }
        /// <summary>
        /// 用餐人数
        /// </summary>
        [SqlField]
        public int Seats { get; set; }
        /// <summary>
        /// 预约备注
        /// </summary>
        [SqlField]
        public string Note { get; set; }
        /// <summary>
        /// 预约人名
        /// </summary>
        [SqlField]
        public string UserName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        [SqlField]
        public string Contact { get; set; }
        /// <summary>
        /// 订单状态 枚举： MiniAppEntOrderState
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 预约类型
        /// </summary>
        [SqlField]
        public int Type { get; set; }
        /// <summary>
        /// 点餐菜单快照
        /// </summary>
        [SqlField]
        public string Menu { get; set; }
        /// <summary>
        /// 获取点餐菜单快照
        /// </summary>
        /// <returns></returns>
        public List<ReserveMenu> GetMenu()
        {
            return !string.IsNullOrWhiteSpace(Menu) ? SerializeHelper.DesFromJson<List<ReserveMenu>>(Menu) : null;
        }
    }

    public class ReserveMenu
    {
        public int GoodId { get; set; }
        public int GoodName { get; set; }
        public int GoodCount { get; set; }
        public int GoodPrice { get; set; }
        public int GoodDiscount { get; set; }
        public int GoodOriginalPrice { get; set; }
        public int GoodOrderId { get; set; }
    }
}
