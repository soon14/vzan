using Entity.Base;
using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序餐饮版-商品信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoods
    {
        public FoodGoods() { }
        /// <summary>
        /// 商品Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }
        /// <summary>
        /// 商品名
        /// </summary>
        [SqlField]
        public string GoodsName { get; set; }
        /// <summary>
        /// 详情
        /// </summary>
        [SqlField]
        public string Description { get; set; }

        /// <summary>
        /// 商品多规格详情
        /// </summary>
        [SqlField]
        public string AttrDetail { get; set; }
        public List<FoodGoodsAttrDetail> GASDetailList => !string.IsNullOrEmpty(AttrDetail) ?  SerializeHelper.DesFromJson<List<FoodGoodsAttrDetail>>(AttrDetail) : new List<FoodGoodsAttrDetail>();

        /// <summary>
        /// 规格名
        /// </summary>
        public string AttrStr { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public int Price { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public string PriceStr { get { return (Price * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public int OriginalPrice { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public string OriginalPriceStr { get { return (OriginalPrice * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 总库存
        /// </summary>
        [SqlField]
        public int Inventory { get; set; }
        /// <summary>
        /// 剩余库存
        /// </summary>
        [SqlField]
        public int Stock { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int FoodId { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [SqlField]
        public string TypeId { get; set; } = string.Empty;

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }


        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int BrowseCount { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; }
        /// <summary>
        /// 状态 1正常 0/-1删除
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 是否出售状态  1上架/0下架 
        /// </summary>
        [SqlField]
        public int IsSell { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        public string showTime
        {
            get
            {
                return CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 运费模板id
        /// </summary>
        [SqlField]
        public string FreightIds { get; set; } = string.Empty;
        /// <summary>
        /// 内容简介
        /// </summary>
        [SqlField]
        public string Introduction { get; set; } = string.Empty;

        /// <summary>
        /// 是否热销 0表示否 1表示是
        /// </summary>
        [SqlField]
        public int IsHot { get; set; } = 0;

        /// <summary>
        /// 标签ID集合 格式如： ,1,2,3
        /// </summary>
        [SqlField]
        public string labelIdStr { get; set; } = string.Empty;

        /// <summary>
        /// 标签ID集合 格式如： ,1,2,3
        /// </summary>
        public string labelNameStr { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        [SqlField]
        public int salesCount { get; set; }

        /// <summary>
        /// 开启外卖 1开启 0关闭
        /// </summary>
        [SqlField]
        public int openTakeOut { get; set; }

        /// <summary>
        /// 开启点餐 1开启 0关闭
        /// </summary>
        [SqlField]
        public int openTheShop { get; set; }

        /// <summary>
        /// 产品类型：0：普通产品，1：拼团产品（EntGoodsType）
        /// </summary>
        [SqlField]
        public int goodtype { get; set; } = 0;

        /// <summary>
        /// 是否收取餐盒费 0否 1是
        /// </summary>
        [SqlField]
        public int isPackin { get; set; } = 0;

        //用来标示当前商品在购物车中的数量
        public int carCount { get; set; } = 0;

        /// <summary>
        /// 折后价
        /// </summary>
        public int discountPrice { get; set; }


        /// <summary>
        /// 折扣
        /// </summary>
        public int discount { get; set; }


        public string discountPricestr
        {
            get
            {
                return (discountPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 拼团折后价
        /// </summary>
        public int discountGroupPrice
        {
            get;set;
        }

        public bool sel { get; set; } = false;

        /// <summary>
        /// 拼团产品
        /// </summary>
        public EntGroupsRelation EntGroups { get; set; } = new EntGroupsRelation();

    }

    /// <summary>
    /// 商品多规格详情Json配置
    /// </summary>
    [Serializable]
    public class FoodGoodsAttrDetail
    {
        /// <summary>
        /// 属性ids串
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int price { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int count { get; set; }
       
        /// <summary>
        /// 价格
        /// </summary>
        public string priceStr { get { return (price * 0.01).ToString("0.00"); } }
        
        /// <summary>
        /// 会员折扣
        /// </summary>
        public int discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public int discountPrice { get; set; }

        /// <summary>
        /// 折后售价(元)
        /// </summary>
        public string discountPricestr
        {
            get
            {
                return (discountPrice * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 原价
        /// </summary>
        public int originalPrice { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public string originalPriceStr { get { return (originalPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 拼团价
        /// </summary>
        public int groupPrice { get; set; }

        /// <summary>
        /// 拼团价
        /// </summary>
        public string groupPriceStr { get { return (groupPrice * 0.01).ToString("0.00"); } }

        /// <summary>
        /// 折后拼团价
        /// </summary>
        public int discountGroupPrice { get; set; }

        /// <summary>
        /// 拼团价
        /// </summary>
        public string discountGroupPriceStr { get { return (discountGroupPrice * 0.01).ToString("0.00"); } }

    }
}
