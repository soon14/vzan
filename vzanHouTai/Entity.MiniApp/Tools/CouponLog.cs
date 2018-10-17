using Entity.Base;
using Entity.MiniApp.Stores;
using Entity.MiniApp;
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
    public class CouponLog
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 优惠券ID
        /// </summary>
        [SqlField]
        public int CouponId { get; set; } = 0;

        /// <summary>
        /// 商品ID
        /// </summary>
        [SqlField]
        public int GoodsId { get; set; } = 0;
        /// <summary>
        /// 订单ID
        /// </summary>
        [SqlField]
        public int OrderId { get; set; } = 0;
        /// <summary>
        /// 支付类型
        /// </summary>
        [SqlField]
        public int PayType { get; set; } = 0;

        
        /// <summary>
        /// 优惠券名称
        /// </summary>
        [SqlField]
        public string CouponName { set; get; }
        // <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { set; get; } = 0;
        /// <summary>
        /// 领取时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { set; get; }
        /// <summary>
        /// 使用时间
        /// </summary>
        [SqlField]
        public DateTime UseTime { set; get; }
        /// <summary>
        /// 开始有效期
        /// </summary>
        [SqlField]
        public DateTime StartUseTime { set; get; }
        public string StartUseTimeStr { get { return StartUseTime.ToString("yyyy.MM.dd"); } }
        /// <summary>
        /// 结束有效期
        /// </summary>
        [SqlField]
        public DateTime EndUseTime { set; get; }
        public string EndUseTimeStr { get { return EndUseTime.ToString("yyyy.MM.dd"); } }
        /// <summary>
        /// 状态：0:未使用，1：已使用，2：已过期，3：未开始，4：未满足条件
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 分享的订单ID，如果为0，则存自己下单的单号
        /// </summary>
        [SqlField]
        public int FromOrderId { get; set; } = 0;
        public string Money { get; set; } = string.Empty;
        public string Money_fmt { get; set; } = string.Empty;
        
        public int LimitMoney { get; set; } = 0;
        public string LimitMoneyStr { get { return (LimitMoney * 0.01).ToString("0.00"); } }
        public int CouponWay { get; set; } = 0;
        /// <summary>
        /// 指定商品ID
        /// </summary>
        public string GoodsIdStr { get; set; } = string.Empty;

        public int ValType { get; set; } = 0;
        
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool CanUse { get; set; } = false;
        [SqlField]
        public int StoreId { get; set; }
        public string StoreName { get; set; }

        /// <summary>
        /// 是否与会员折扣互斥 0默认不互斥 1互斥
        /// </summary>
      
        public int discountType { get; set; } = 0;

        [SqlField]
        public int AId { get; set; } = 0;
    }
}