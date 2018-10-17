using Entity.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 公用运费模板功能（新版）
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DeliveryTemplate
    {
        [SqlField(IsPrimaryKey = true,IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 父模板ID
        /// </summary>
        [SqlField]
        public int ParentId { get; set; } = 0;

        /// <summary>
        /// 状态：0：正常，-1：删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 模板名
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 首单位（首数量、首重量）
        /// </summary>
        [SqlField]
        public int Base { get; set; }

        /// <summary>
        /// 首价格（续数量、续重量）
        /// </summary>
        [SqlField]
        public int BaseCost { get; set; }

        /// <summary>
        /// 续单位（续数量、续重量）
        /// </summary>
        [SqlField]
        public int Extra { get; set; }

        /// <summary>
        /// 续价格（续数量、续重量）
        /// </summary>
        [SqlField]
        public int ExtraCost { get; set; }

        /// <summary>
        /// 单位类型（件数、kg）
        /// </summary>
        [SqlField]
        public int UnitType { get; set; }

        /// <summary>
        /// 包邮（1：是，0：否）
        /// </summary>
        [SqlField]
        public int IsFree { get; set; }

        /// <summary>
        /// 运费满减值（大于此值免邮费）
        /// </summary>
        //[SqlField]
        //public int Discount { get; set; }

        /// <summary>
        /// 运费满减开关
        /// </summary>
        //[SqlField]
        //private bool EnableDiscount { get; set; }

        /// <summary>
        /// 配送区域 AreaCode 数组
        /// </summary>
        [SqlField]
        public string DeliveryRegion { get; set; }

        [SqlField]
        public string DeliveryRegionSub { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 模板关联商品数
        /// </summary>
        public int ApplyCount { get; set; }
    }

    public class DeliveryDiscount
    {
        /// <summary>
        /// 满足运费优惠
        /// </summary>
        public bool HasDiscount { get; set; }
        /// <summary>
        /// 优惠价格
        /// </summary>
        public int DiscountPrice { get; set; }
        /// <summary>
        /// 优惠信息
        /// </summary>
        public string DiscountInfo { get; set; }
    }

    /// <summary>
    /// 运费结算结果
    /// </summary>
    public class DeliveryFeeResult
    {
        /// <summary>
        /// 是否在配送范围
        /// </summary>
        public bool InRange { get; set; }
        /// <summary>
        /// 配送金额
        /// </summary>
        public int Fee { get; set; }
        /// <summary>
        /// 配送信息（如：商品A不在配送区域内）
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回ID（产品ID）
        /// </summary>
        public int ErrorId { get; set; }
    }

    /// <summary>
    /// 运费计算模型
    /// </summary>
    public class DeliveryFeeSum
    {
        /// <summary>
        /// 首单位
        /// </summary>
        public int Base { get; set; }
        /// <summary>
        /// 首价格
        /// </summary>
        public int BaseCost { get; set; }
        /// <summary>
        /// 续单位
        /// </summary>
        public int Extra { get; set; }
        /// <summary>
        /// 续价格（续数量、续重量）
        /// </summary>
        public int ExtraCost { get; set; }
        /// <summary>
        /// 购买单位数（件数、kg）
        /// </summary>
        public int BuyUnit { get; set; }
        /// <summary>
        /// 补余单位（如：购买300g，不足1kg，追加700kg）
        /// </summary>
        public int PadUnit { get; set; }
        /// <summary>
        /// 单位类型（件数、kg）
        /// </summary>
        public int UnitType { get; set; }
    }

    /// <summary>
    /// 运费计算产品模型
    /// </summary>
    public class DeliveryProduct
    {
        /// <summary>
        /// 产品ID（用途：返回运费结果模型DeliveryFeeResult，ErrorID字段）
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 产品名称（用途：返回运费结果模型DeliveryFeeResult，Message字段：XXX商品超出配送范围）
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 产品配送数量（用途：结算运费规则）
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 产品重量（用途：结算运费规则，可为0）
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// 产品金额（用途：满减运费）
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// 选择运费模板（用途：关联绑定的运费模板，结算运费）
        /// </summary>
        public int TemplateId { get; set; }
    }
}
