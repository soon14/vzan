using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntGoodsCart
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
        public int FoodGoodsId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        [SqlField]
        public string GoodName { get; set; }

        /// <summary>
        /// 商品信息
        /// </summary>
        public EntGoods goodsMsg { get; set; }


        /// <summary>
        /// 商品价格是否改动 0正常 -1失效
        /// </summary>
        public int PriceState { get; set; }


        /// <summary>
        /// 商品规格值状态 0正常 -1失效
        /// </summary>
        public int SpecificationState { get; set; }

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
        /// 商品规格图片
        /// </summary>
        [SqlField]
        public string SpecImg { get; set; }


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

        public string priceStr { get { return (Price * 0.01).ToString("0.00"); }}

        /// <summary>
        /// 价格(折前价）  未打折的现价后台填写的
        /// </summary>
        [SqlField]
        public int NotDiscountPrice { get; set; }

        public string NotDiscountPriceStr { get { return (NotDiscountPrice * 0.01).ToString("0.00"); } }



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
        ///状态 -1 已删除  0 加入购物车  1已提交订单,2立即购买
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        ///状态 0正常 1下架 2删除 (3 多门店售罄,4 多门店库存不足)
        /// </summary>
        [SqlField]
        public int GoodsState { get; set; } = 0;
        
        /// <summary>
        /// 小程序表Id
        /// </summary>
        [SqlField]
        public int aId { get; set; }



        /// <summary>
        /// 打折
        /// </summary>
        public int discount { get; set; } = 100;



        /// <summary>
        /// 原价（会员打折前的价格）
        /// </summary>
        [SqlField]
        public int originalPrice { get; set; } = 0;

        
        public string originalPriceStr { get { return (originalPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 包间号
        /// </summary>
        [SqlField]
        public int roomNo { get; set; } = 0;

        /// <summary>
        /// 足浴 - 包间名/ 多门店 - 商品名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;
        /// <summary>
        /// 预订时间
        /// </summary>
        [SqlField]
        public DateTime reservationTime { get; set; }

        public string showReservationTime
        {
            get
            {
                return reservationTime.ToString("yyyy-MM-dd HH:mm");
            }
        }
        /// <summary>
        /// 足浴版技师id
        /// </summary>
        [SqlField]
        public int technicianId { get; set; } = 0;

        public string technicianName { get; set; } = "未分配";
        /// <summary>
        /// 附加属性
        /// </summary>
        [SqlField]
        public string extraConfig { get; set; } = string.Empty;

        public ExtraConfig footBathConfig { get; set; }

        /// <summary>
        /// 分店商品信息表ID
        /// </summary>
        [SqlField]
        public int SubGoodId { get; set; } = 0;

        public double discountPrice = 0;


        /// <summary>
        /// 购物车对应的产品佣金比例
        /// </summary>
        [SqlField]
        public double cps_rate { get; set; } = 0.00;
        /// <summary>
        /// 购物车所属分销员-产品关系记录
        /// </summary>
        [SqlField]
        public int salesManRecordUserId { get; set; } = 0;

        /// <summary>
        /// 购物车所属推广记录Id
        /// </summary>
        [SqlField]
        public int recordId { get; set; } = 0;
        /// <summary>
        /// 购物车类型（枚举：EntGoodCartType）
        /// </summary>
        [SqlField]
        public int type { get; set; }

        /// <summary>
        /// 是否已评论
        /// </summary>
        [SqlField]
        public bool IsCommentting { get; set; } = false;

        /// <summary>
        /// 立即购买，1：是，0：否
        /// </summary>
        [SqlField]
        public int GoToBuy { get; set; } = 0;
    }

    /// <summary>
    /// 购物车产品分销对象
    /// </summary>
    public class CpsRateCar
    {
        public double cps_rate { get; set; } = 0.00;
        public int salesManRecordUserId { get; set; } = 0;
        public int recordId { get; set; } = 0;
    }

    /// <summary>
    /// 购物车单条对象
    /// </summary>
    public class EntFoodGoodCarItem
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
        public int Stock { get; set; }

    }
    /// <summary>
    /// 足浴版购物车额外属性实体
    /// </summary>
    public class ExtraConfig
    {
        /// <summary>
        /// 是否锁定技师
        /// </summary>
        public bool lockTechnician { get; set; } = false;
    }
}
    