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
    public class DadaOrderRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 所有要跟达达平台关联的的订单的唯一订单号
        /// </summary>
        [SqlField]
        public string uniqueorderno { get; set; }
        /// <summary>
        /// 所有要跟达达关联的订单表ID，该订单ID有可能会重复
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
        /// 权限表ID
        /// </summary>
        public int dataid { get; set; }
    }
}
