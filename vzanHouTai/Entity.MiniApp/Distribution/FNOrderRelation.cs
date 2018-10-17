using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 达达订单与所有订单的关联表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FNOrderRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 所有要跟蜂鸟配送平台关联的的订单的唯一订单号
        /// </summary>
        [SqlField]
        public string uniqueorderno { get; set; }
        /// <summary>
        /// 所有要跟蜂鸟关联的订单表ID，该订单ID有可能会重复
        /// </summary>
        [SqlField]
        public int orderid { get; set; }
        /// <summary>
        /// 看枚举TmpType
        /// </summary>
        [SqlField]
        public int ordertype { get; set; }
        [SqlField]
        /// <summary>
        /// 专业版存rid,电商和餐饮存店铺ID，各个模板通过该字段查找到相对应的订单
        /// </summary>
        public int dataid { get; set; }
    }
}
