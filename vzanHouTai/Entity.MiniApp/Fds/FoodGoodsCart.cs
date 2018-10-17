using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsCart
    {
        /// <summary>
        /// Id购物车
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 商品id
        /// </summary>
        [SqlField]
        public int FoodGoodsId { get; set; }

        /// <summary>
        /// 商品信息
        /// </summary>
        public FoodGoods goodsMsg { get; set; }

        /// <summary>
        /// 商品订单id
        /// </summary>
        [SqlField]
        public int GoodsOrderId { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int FoodId { get; set; }
        /// <summary>
        /// 商品属性id串
        /// </summary>
        [SqlField]
        public string SpecIds { get; set; }
        /// <summary>
        /// 商品规格属性
        /// </summary>
        [SqlField]
        public string SpecInfo { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [SqlField]
        public int Count { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public int Price { get; set; } = 0;
        /// <summary>
        /// 用户id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        ///状态 -1 已删除  0 加入购物车  1已提交订单
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        ///状态 0正常 1下架 2删除
        /// </summary>
        [SqlField]
        public int GoodsState { get; set; } = 0;


        /// <summary>
        /// 折后价
        /// </summary>
        public int discountPrice { get; set; }


        /// <summary>
        /// 折扣
        /// </summary>
        public int discount { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public int originalPrice { get; set; }

        [SqlField]
        public int Type { get; set; }

        /// <summary>
        /// 商品图片（下单时保存快照）
        /// </summary>
        [SqlField]
        public string GoodImg { get; set; }
        /// <summary>
        /// 商品图片（下单时保存快照）
        /// </summary>
        [SqlField]
        public string GoodName { get; set; }

        /// <summary>
        /// 是否已评论
        /// </summary>
        public bool IsCommentting { get; set; } = false;
    }

    /// <summary>
    /// 购物车单条对象
    /// </summary>
    public class FoodGoodCarItem
    {
       
        /// <summary>
        /// 商品id
        /// </summary>
        public int FoodGoodsId { get; set; }
        /// <summary>
        /// 商品属性id串
        /// </summary>
        public string SpecIds { get; set; }
        /// <summary>
        /// 商品属性串
        /// </summary>
        public string SpecInfo { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

    }
}
