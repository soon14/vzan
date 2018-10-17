using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Entity.Base;
using Utility;


namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 购物车
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishShoppingCart
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        
        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 订单id
        /// </summary>
        [SqlField]
        public int order_id { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int user_id { get; set; } = 0;

        /// <summary>
        /// 商品id
        /// </summary>
        [SqlField]
        public int goods_id { get; set; } = 0;

        /// <summary>
        /// 商品名称
        /// </summary>
        [SqlField]
        public string goods_name { get; set; } = string.Empty;

        /// <summary>
        /// 商品编号 -暂不用
        /// </summary>
        [SqlField]
        public string goods_sn { get; set; } = string.Empty;

        /// <summary>
        /// 商品数量
        /// </summary>
        [SqlField]
        public int goods_number { get; set; } = 0;

        /// <summary>
        /// 商品金额
        /// </summary>
        [SqlField]
        public double goods_price { get; set; } = 0.00;

        /// <summary>
        /// 商品属性
        /// </summary>
        [SqlField]
        public string goods_attr { get; set; } = string.Empty;
        
        /// <summary>
        /// 属性id
        /// </summary>
        [SqlField]
        public string goods_attr_id { get; set; } = string.Empty;

        /// <summary>
        /// 是否退款 1是 0否
        /// </summary>
        [SqlField]
        public int is_tuikuan { get; set; } = 0;

        /// <summary>
        /// 商品图片
        /// </summary>
        [SqlField]
        public string goods_img { get; set; } = string.Empty;
    }


    public class cdataModel
    {
        public string goods_id { get; set; } = string.Empty;
        public string goods_name { get; set; } = string.Empty;
        public int cart_goods_number { get; set; } = 0;
        public double goods_price { get; set; } = 0.00d;
        public string goods_attr { get; set; } = string.Empty;
        public string goods_attr_id { get; set; } = string.Empty;
    }
}
