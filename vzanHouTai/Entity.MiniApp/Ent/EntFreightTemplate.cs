using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Base;
using Utility;
using Entity.MiniApp;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 运费模板 
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntFreightTemplate
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 初阶数量
        /// </summary>
        [SqlField]
        public int BaseCount { get; set; }

        /// <summary>
        /// 初阶费用
        /// </summary>
        [SqlField]
        public int BaseCost { get; set; }

        /// <summary>
        /// 额外费用
        /// </summary>
        [SqlField]
        public int ExtraCost { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        [SqlField]
        public int IsDefault { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否有效
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 所属模板商家
        /// </summary>
        [SqlField]
        public int aId { get; set; }

        [SqlField]
        public int FullDiscount { get; set; }

        [SqlField]
        public string AreaCode { get; set; }

        public string AreaName { get; set; }

        public string sum { get; set; }

    }

    public class FreightInfo
    {
        public int Fee { get; set; }
        public bool IsVaild { get; set; }
        public string Msg { get; set; }
    }
}
