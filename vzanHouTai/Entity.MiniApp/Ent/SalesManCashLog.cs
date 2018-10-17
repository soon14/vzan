using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 分销员佣金变化日志
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SalesManCashLog
    {
        
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; } = 0;

        /// <summary>
        /// 分销员Id
        /// </summary>
        [SqlField]
        public int SaleManId { get; set; } = 0;

        /// <summary>
        /// 佣金变化日志
        /// </summary>
        [SqlField]
        public string CashLog { get; set; } = string.Empty;

        /// <summary>
        /// 摘要备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


    }
}
