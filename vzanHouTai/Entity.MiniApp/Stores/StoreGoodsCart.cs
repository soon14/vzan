using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Stores
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreGoodsCart
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 商品id
        /// </summary>
        [SqlField]
        public int GoodsId { get; set; }

        /// <summary>
        /// 商品信息
        /// </summary>
        public StoreGoods goodsMsg { get; set; }
        
        /// <summary>
        /// 商品订单id
        /// </summary>
        [SqlField]
        public int GoodsOrderId { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }
        /// <summary>
        /// 商品属性id串
        /// </summary>
        [SqlField]
        public string SpecIds { get; set; } = "";
        /// <summary>
        /// 商品规格属性
        /// </summary>
        [SqlField]
        public string SpecInfo { get; set; } = "";
        /// <summary>
        /// 数量
        /// </summary>
        [SqlField]
        public int Count { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public int Price { get; set; }
        /// <summary>
        /// 价格 单位(元)
        /// </summary>
        public string PriceStr { get { return (Price * 0.01).ToString("0.00"); } }
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
        /// 当前记录的 商品状态 0 正常   1 已下架    2 已删除
        /// </summary>
        [SqlField]
        public int GoodsState { get; set; } = 0;

        /// <summary>
        /// 打折
        /// </summary>
        public int discount { get; set; } = 100;

        /// <summary>
        /// 原价（会员打折前的价格）
        /// </summary>
        [SqlField]
        public int originalPrice { get; set; } = 0;

    }
}
