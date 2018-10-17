using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.CrmApi
{
    /// <summary>
    /// 线索表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CrmLeads
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int LeadId { get; set; }
        [SqlField]
        public string Name { get; set; }
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public string StateContent { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        [SqlField]
        public DateTime UpdateTime { get; set; }
        [SqlField]
        public string Phone { get; set; }
    }
}
