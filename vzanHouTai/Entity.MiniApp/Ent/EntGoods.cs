using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 小程序企业版-产品信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntGoods
    {
        /// <summary>
        /// 产品
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;

        /// <summary>
        /// 产品名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 产品图片
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;
        public string img_fmt { get; set; } = string.Empty;

        /// <summary>
        /// 是否显示价格
        /// </summary>
        [SqlField]
        public bool showprice { get; set; } = true;

        /// <summary>
        /// 产品分类 可选择多个，逗号分隔
        /// </summary>
        [SqlField]
        public string ptypes { get; set; } = string.Empty;

        /// <summary>
        /// 拓展产品参数 可选择多个，逗号分隔，(足浴版：用于区分是否送花订单)
        /// </summary>
        [SqlField]
        public string exttypes { get; set; } = string.Empty;

        /// <summary>
        /// 产品扩展分类ID
        /// </summary>
        [SqlField]
        public string exttypesstr { get; set; } = string.Empty;
        /// <summary>
        /// 参数组合
        /// </summary>
        public List<IndutypesItem> IndutypeList { get; set; }

        /// <summary>
        /// 产品分类，格式化显示
        /// </summary>
        public string ptypestr { get; set; } = string.Empty;

        /// <summary>
        /// 库存 （足浴版服务项目作假销量基数）
        /// </summary>
        [SqlField]
        public int stock { get; set; } = 0;
        /// <summary>
        /// 是否限制库存 默认不限库存
        /// </summary>
        [SqlField]
        public bool stockLimit { get; set; } = true;
        /// <summary>
        /// 产品标签
        /// </summary>
        [SqlField]
        public string plabels { get; set; } = string.Empty;

        /// <summary>
        /// 产品标签-数组格式
        /// </summary>
        public List<string> plabelstr_array { get; set; } = new List<string>();

        /// <summary>
        /// 产品标签-字符串拼接格式
        /// </summary>
        public string plabelstr { get; set; } = string.Empty;

        /// <summary>
        /// 规格 （足浴版服务流程）
        /// </summary>
        [SqlField]
        public string specificationkeys { get; set; } = string.Empty;
        /// <summary>
        /// 规格值
        /// </summary>
        [SqlField]
        public string specification { get; set; } = string.Empty;
        /// <summary>
        /// 产品规格，详情 保存规格+价格+库存
        /// </summary>
        [SqlField]
        public string specificationdetail { get; set; } = string.Empty;
        /// <summary>
        /// 选择的规格对象，用于编辑的时候还原
        /// </summary>
        [SqlField]
        public string pickspecification { get; set; } = string.Empty;
        /// <summary>
        /// 价格
        /// </summary>
        [SqlField]
        public float price { get; set; } = 0;

        public string priceStr { get { return price.ToString("0.00"); } }


        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public float originalPrice { get; set; }


        /// <summary>
        /// 数量单位
        /// </summary>
        [SqlField]
        public string unit { get; set; } = string.Empty;

        /// <summary>
        /// 滚动图片
        /// </summary>
        [SqlField]
        public string slideimgs { get; set; } = string.Empty;

        public string slideimgs_fmt { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// 录入时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; } = DateTime.Now;

        public string showUpdateTime
        {
            get
            {
                return updatetime.ToString("yyyy-MM-dd HH:mm");
            }
        }

        /// <summary>
        /// 是否显示产品参数，默认False
        /// </summary>
        public bool showExtTypes { get; set; } = false;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;
        /// <summary>
        /// 状态
        /// 1：正常
        /// 0：删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 0：下架
        /// 1：上架
        /// </summary>
        [SqlField]
        public int tag { get; set; } = 1;

        /// <summary>
        /// 是否选中，前端辅助字段
        /// </summary>
        public bool sel { get; set; } = false;
        public bool enable { get; set; } = true;

        /// <summary>
        /// 商品销量 - 虚拟销量(专业版供用户直接设定)
        /// </summary>
        [SqlField]
        public int virtualSalesCount { get; set; } = 0;

        /// <summary>
        /// 商品销量 （足浴版用户实际销量）
        /// </summary>
        [SqlField]
        public int salesCount { get; set; } = 0;

        //public List<EntGoodsAttrDetail> GASDetailList => !string.IsNullOrEmpty(specificationdetail) ? SerializeHelper.DesFromJson<List<EntGoodsAttrDetail>>(specificationdetail) : new List<EntGoodsAttrDetail>();
        public List<EntGoodsAttrDetail> GASDetailList => !string.IsNullOrEmpty(specificationdetail) ?JsonConvert.DeserializeObject<List<EntGoodsAttrDetail>>(specificationdetail,new JsonSerializerSettings() { MaxDepth=Int32.MaxValue}) : new List<EntGoodsAttrDetail>();

        /// <summary>
        /// 服务时长 -->  20171124 暂用于 足浴
        /// </summary>
        [SqlField]
        public int ServiceTime { get; set; } = 0;


        /// <summary>
        /// 会员折扣
        /// </summary>
        public int discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public float discountPrice
        {
            get;set;
        }

        public string discountPricestr
        {
            get
            {
                return discountPrice.ToString("0.00").TrimEnd('0').TrimEnd('.');
            }
        }

        #region 拼团
        /// <summary>
        /// 产品类型：0：普通产品，1：拼团产品
        /// </summary>
        [SqlField]
        public int goodtype { get; set; } = (int)EntGoodsType.普通产品;

        /// <summary>
        /// 拼团折后售价
        /// </summary>
        public float discountGroupPrice
        {
            get; set;
        }

        public string discountGroupPricestr
        {
            get
            {
                return discountGroupPrice.ToString("0.00").TrimEnd('0',',');
            }
        }
        public int storeId { get; set; } = 0;
        /// <summary>
        /// 拼团产品
        /// </summary>
        public EntGroupsRelation EntGroups { get; set; } = new EntGroupsRelation();
        #endregion

        //分店商品ID
        public int subGoodId { get; set; } = 0;
      
        /// <summary>
        /// 是否属于分销商品 0→不参与 1→参与
        /// </summary>
        [SqlField]
        public int isDistribution { get; set; } = 0;

        /// <summary>
        /// 是否默认店铺佣金比例 0→默认 1→自定义
        /// </summary>
        [SqlField]
        public int isDefaultCps_Rate { get; set; } = 0;

        /// <summary>
        /// 佣金比例
        /// </summary>
        [SqlField]
        public double cps_rate { get; set; } = 0.00;

        /// <summary>
        /// 推广状态更新时间
        /// </summary>
        [SqlField]
        public DateTime distributionTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 用户是否预约过此产品
        /// </summary>
        public bool isSubscribe { get; set; } = false;

        /// <summary>
        /// 外卖 0开启  -1关闭
        /// </summary>
        [SqlField]
        public int isTakeout { get; set; } = 0;

        /// <summary>
        /// 是否需要餐盒 0否  1是  
        /// </summary>
        [SqlField]
        public int isPackin { get; set; } = 0;

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

        /// <summary>
        /// 产品编号（用户填写：管理搜索用途）
        /// </summary>
        [SqlField]
        public string StockNo { get; set; } = string.Empty;

        /// <summary>
        /// 来源 0后台添加，1表示Excel导入
        /// </summary>
        [SqlField]
        public int FromSource { get; set; } = 0;


        /// <summary>
        /// 每日库存 
        /// 当天该产品销量大于该值,提示“今天已卖完”，第二天自动恢复设置库存重新计算。
        /// </summary>
        [SqlField]
        public int DayStock { get; set; } = 0;



    }


    /// <summary>
    /// 商品多规格详情Json配置
    /// </summary>
    [Serializable]
    public class EntGoodsAttrDetail
    {
        /// <summary>
        /// 属性ids串
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public float price { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public float originalPrice { get; set; }
        /// <summary>
        /// 拼团价
        /// </summary>
        public float groupPrice { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int stock { get; set; }

        ///// <summary>
        ///// 价格
        ///// </summary>
        //public string priceStr { get { return (price * 0.01).ToString("0.00"); } }



        /// <summary>
        /// 会员折扣
        /// </summary>
        public int discount { get; set; } = 100;
        /// <summary>
        /// 折后售价
        /// </summary>
        public float discountPrice { get; set; }
        /// <summary>
        /// 折后拼团价
        /// </summary>
        public float discountGroupPrice { get; set; }

        public string discountPricestr
        {
            get
            {
                return discountPrice.ToString("0.00");
            }
        }


        /// <summary>
        /// 规格图片 专业版
        /// </summary>
        public string imgUrl { get; set; }

       


    }

    /// <summary>
    /// 商品多规格组Json配置
    /// </summary>
    [Serializable]
    public class pickspecification
    {
        public int id { get; set; }
        public int parentid { get; set; }
        public string name { get; set; }
        public List<pickspecification> items { get; set; }
    }
}
