using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Stores
{
    /// <summary>
    /// 小程序商城模板-商品信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreGoods
    {
        public StoreGoods() { }
        /// <summary>
        /// 商品id
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
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 商品多规格详情
        /// </summary>
        [SqlField]
        public string AttrDetail { get; set; }
        public List<StoreGoodsAttrDetail> GASDetailList => !string.IsNullOrEmpty(AttrDetail) ?  SerializeHelper.DesFromJson<List<StoreGoodsAttrDetail>>(AttrDetail) : new List<StoreGoodsAttrDetail>();
        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public int Price { get; set; }

        /// <summary>
        /// 价格 单位(元)
        /// </summary>
        public string PriceStr
        {
            get
            {
                return (Price * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public int OriginalPrice { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public string OriginalPriceStr { get { return (OriginalPrice * 0.01).ToString(); } }


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
        public int StoreId { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [SqlField]
        public int TypeId { get; set; }
        /// <summary>
        /// 类型说明
        /// </summary>
        public string TypeStr{ get; set; }
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
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 是否出售状态  上1/0下架   
        /// </summary>
        [SqlField]
        public int IsSell { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
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
        /// 销量
        /// </summary>
        [SqlField]
        public int salesCount { get; set; }

        public string showtime { get; set; } = string.Empty;
        public bool sel { get; set; } = false;

        /// <summary>
        /// 会员折扣
        /// </summary>
        public int discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public int discountPrice { get; set; }

        public string discountPricestr
        {
            get
            {
                return (discountPrice * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 商品真实的销量,仅商家后台可看到
        /// </summary>
        [SqlField]
        public int salesCount_real { get; set; }
    }

    /// <summary>
    /// 商品多规格详情Json配置
    /// </summary>
    [Serializable]
    public class StoreGoodsAttrDetail
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
        /// 价格 单位(元)
        /// </summary>
        public string priceStr
        {
            get
            {
                return (price * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 会员折扣
        /// </summary>
        public int discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public int discountPrice { get; set; }

        public string discountPricestr
        {
            get
            {
                return (discountPrice * 0.01).ToString("0.00");
            }
        }
    }
}
