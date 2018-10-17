using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility; 

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 运费模板 
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodFreightTemplate
    {
        /// <summary>
        /// 模板Id
        /// </summary>
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
        public int FoodId { get; set; }

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
        /// 是否系统默认(现阶段规则,系统默认模板不允许删除)
        /// </summary>
        [SqlField]
        public int IsDefault { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string sum { get; set; }

    }
}
