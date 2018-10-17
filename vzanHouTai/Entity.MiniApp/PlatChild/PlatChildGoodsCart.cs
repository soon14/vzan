using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.PlatChild
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildGoodsCart
    {
        /// <summary>
        /// Id购物车
        /// </summary>
        [SqlField(IsPrimaryKey = true,IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商品id
        /// </summary>
        [SqlField]
        public int GoodsId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        [SqlField]
        public string GoodsName { get; set; }

        /// <summary>
        /// 商品订单id
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 规格ID
        /// </summary>
        [SqlField]
        public string SpecIds { get; set; }
        /// <summary>
        /// 商品规格属性
        /// </summary>
        [SqlField]
        public string SpecInfo { get; set; }
        //public List<PlatGoodsSpecItem> SpecItem { get { return string.IsNullOrEmpty(SpecInfo) ? new List<PlatGoodsSpecItem>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlatGoodsSpecItem>>(SpecInfo); } }
        /// <summary>
        /// 规格图片
        /// </summary>
        [SqlField]
        public string SpecImg { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        ///状态 -1 已删除  0 加入购物车  1已提交订单  2商品失效
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        
        /// <summary>
        /// 权限表Id
        /// </summary>
        [SqlField]
        public int AId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [SqlField]
        public int Count { get; set; }
        /// <summary>
        /// 价格(折后价）
        /// </summary>
        [SqlField]
        public int Price { get; set; }
        public string PriceStr { get { return (Price * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 打折
        /// </summary>
        [SqlField]
        public int Discount { get; set; } = 100;
        /// <summary>
        /// 原价（会员打折前的价格）
        /// </summary>
        [SqlField]
        public int OriginalPrice { get; set; } = 0;
        public string OriginalPriceStr { get { return (OriginalPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 立即购买，1：是，0：否
        /// </summary>
        [SqlField]
        public int GoToBuy { get; set; } = 0;
        public PlatChildGoods GoodsInfo { get; set; }

        [SqlField]
        public bool IsCommentting { get; set; } = false;
    }
    

    /// <summary>
    /// 购物车单条对象
    /// </summary>
    public class PlatGoodsSpecItem
    {
        /// <summary>
        /// 商品属性id
        /// </summary>
        public int SpecId { get; set; }
        /// <summary>
        /// 商品属性名称
        /// </summary>
        public string SpecName { get; set; }
    }
}
    