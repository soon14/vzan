using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Entity.MiniApp.Plat;

namespace Entity.MiniApp.PlatChild
{
    /// <summary>
    /// 产品信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildGoods
    {
        /// <summary>
        /// 产品
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户小程序ID  绑定的店铺小程序aid
        /// </summary>
        [SqlField]
        public int AId { get; set; } = 0;

        /// <summary>
        /// 产品名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 产品图片
        /// </summary>
        [SqlField]
        public string Img { get; set; } = string.Empty;


        /// <summary>
        /// 产品分类 可选择多个，逗号分隔
        /// </summary>
        [SqlField]
        public string Categorys { get; set; } = string.Empty;

        /// <summary>
        /// 产品分类，格式化显示
        /// </summary>
        public string CategorysStr { get; set; } = string.Empty;


        /// <summary>
        /// 库存 
        /// </summary>
        [SqlField]
        public int Stock { get; set; } = 0;
        /// <summary>
        /// 是否限制库存 默认不限库存
        /// </summary>
        [SqlField]
        public bool StockLimit { get; set; } = false;
        /// <summary>
        /// 产品标签
        /// </summary>
        [SqlField]
        public string Plabels { get; set; } = string.Empty;

        /// <summary>
        /// 产品标签-字符串拼接格式
        /// </summary>
        public string PlabelStr { get; set; } = string.Empty;


        public List<string> PlabelStr_Arry { get; set; }


        /// <summary>
        /// 规格 
        /// </summary>
        [SqlField]
        public string SpecName { get; set; } = string.Empty;
        /// <summary>
        /// 规格值
        /// </summary>
        [SqlField]
        public string SpecValue { get; set; } = string.Empty;
        /// <summary>
        /// 产品规格，详情 保存规格+价格+库存
        /// </summary>
        [SqlField]
        public string SpecDetail { get; set; } = string.Empty;


        /// <summary>
        /// 选择的规格 用于还原 
        /// </summary>
        [SqlField]
        public string PickSpec { get; set; } = string.Empty;

        /// <summary>
        /// 单买价 
        /// </summary>
        [SqlField]
        public float Price { get; set; } = 0;

        public int PriceFen { get { return Convert.ToInt32(Price * 100); } }



        /// <summary>
        /// 原价 
        /// </summary>
        [SqlField]
        public float OriginalPrice { get; set; } = 0;

        public string OriginalPriceStr { get { return OriginalPrice.ToString("#0.00"); } }
        


        /// <summary>
        /// 数量单位
        /// </summary>
        [SqlField]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 滚动图片
        /// </summary>
        [SqlField]
        public string Slideimgs { get; set; } = string.Empty;

        public string Slideimgs_fmt { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 录入时间
        /// </summary>
        [SqlField]
        public DateTime Addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime Updatetime { get; set; } = DateTime.Now;

        public string ShowUpdateTime
        {
            get
            {
                return Updatetime.ToString("yyyy-MM-dd HH:mm");
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 99;
        /// <summary>
        /// 状态
        /// 1：正常
        /// 0：删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;

        /// <summary>
        /// 0：下架
        /// 1：上架
        /// </summary>
        [SqlField]
        public int Tag { get; set; } = 1;



        /// <summary>
        /// 商品销量 - 虚拟销量(专业版供用户直接设定)
        /// </summary>
        [SqlField]
        public int VirtualSalesCount { get; set; } = 0;

        /// <summary>
        /// 商品销量 
        /// </summary>
        [SqlField]
        public int SalesCount { get; set; } = 0;

        public List<GoodsSpecDetail> GASDetailList => !string.IsNullOrEmpty(SpecDetail) ? JsonConvert.DeserializeObject<List<GoodsSpecDetail>>(SpecDetail) : new List<GoodsSpecDetail>();

        /// <summary>
        /// 会员折扣
        /// </summary>
        public int Discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public float DiscountPrice
        {
            get; set;
        }

        public string DiscountPricestr
        {
            get
            {
                return DiscountPrice.ToString("0.00").TrimEnd('0').TrimEnd('.');
            }
        }

        /// <summary>
        /// 运费模板ID
        /// </summary>
        [SqlField]
        public int TemplateId { get; set; } = 0;

        /// <summary>
        /// 重量（kg）
        /// </summary>
        [SqlField]
        public int Weight { get; set; } = 0;


        public StoreModel storeModel { get; set; }

        /// <summary>
        /// 是否同步到平台状态 0 没有同步 1同步
        /// </summary>

        public int Synchronized { get; set; } = 0;


        /// <summary>
        /// 最大价格 如果有规格 返回规格最大值 用于前端显示
        /// </summary>
        [SqlField]
        public float MaxPrice { get; set; }=0.00f;



        /// <summary>
        /// 0：不推荐
        /// 1：推荐
        /// </summary>
        [SqlField]
        public int TopState { get; set; } = 0;


        /// <summary>
        /// 店铺名称
        /// </summary>
        public string StoreName { get; set; }


        /// <summary>
        /// 现在所属平台大类名称
        /// </summary>
        public string CurFirstCategoryName { get; set; }
        /// <summary>
        /// 现在所属平台小类名称
        /// </summary>
        public string CurSecondCategoryName { get; set; }


        public StoreOwner storeOwner { get; set; } = new StoreOwner();
    }

    /// <summary>
    /// 该产品所属店铺信息
    /// </summary>
    public class StoreModel
    {
        public int StoreId { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public string Loction { get; set; }

        /// <summary>
        /// 地理位置 纬度
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// 地理位置 经度
        /// </summary>
        public double Lng { get; set; }
    }


    /// <summary>
    /// 商品多规格详情Json配置
    /// </summary>
    [Serializable]
    public class GoodsSpecDetail
    {
        /// <summary>
        /// 属性ids串
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public float Price { get; set; }


        public int PriceFen { get { return Convert.ToInt32(Price * 100); } }

       

        /// <summary>
        /// 数量
        /// </summary>
        public int Stock { get; set; }



        /// <summary>
        /// 会员折扣
        /// </summary>
        public int Discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public float DiscountPrice { get; set; }
       



        public string DiscountPricestr
        {
            get
            {
                return DiscountPrice.ToString("0.00");
            }
        }

        /// <summary>
        /// 规格图片 专业版
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public float OriginalPrice { get; set; }

    }
}
