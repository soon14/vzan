using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Dish
{
    public static class DishEnums
    {
        /// <summary>
        /// 分类表类型枚举
        /// </summary>
        public enum CategoryEnums
        {
            店铺分类 = 1,
            菜品分类 = 2,//餐饮多门店菜品
        }

        /// <summary>
        /// 桌台状态枚举
        /// </summary>
        public enum TableStateEnums
        {
            已删除 = -1,
            空闲 = 0,
            已开台 = 1,
            已下单 = 2,
            已支付 = 3,
        }

        public enum QueueUpEnums
        {
            已删除 = -1,
            排队中 = 0,
            已入号 = 1,
            已取消 = 2,
            已过号 = 3,
        }

        /// <summary>
        /// 订单类型
        /// </summary>
        public enum OrderType
        {
            店内 = 1,
            外卖 = 2,
            //店内自提 = 3,
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public enum OrderState
        {
            未确认 = 0,
            已确认 = 1,
            已完成 = 2,
            已取消 = 3,
        }

        /// <summary>
        /// 支付状态
        /// </summary>
        public enum PayState
        {
            未付款 = 0,
            已付款 = 1,
            已退款 = 2,
        }

        /// <summary>
        /// 支付方式
        /// </summary>
        public enum PayMode
        {
            微信支付 = 1,
            线下支付 = 2,
            货到支付 = 3,
            余额支付 = 4,
        }

        /// <summary>
        /// 配送状态
        /// </summary>
        public enum DeliveryState
        {
            已取消 = -1,
            待商家确认 = 0,
            待取货 = 1,
            配送中 = 2,
            已完成 = 3,
        }

        /// <summary>
        /// 配送类型
        /// </summary>
        public enum DeliveryType
        {
            商家配送 = 1,
            达达配送 = 2,
            快跑者配送 = 4,
        }


        public enum EarningsType
        {
            支付 = 1,
            退款 = 2,
            提现 = 3,
        }

        public enum TagType
        {
            打印 = 1
        }
    }
}
