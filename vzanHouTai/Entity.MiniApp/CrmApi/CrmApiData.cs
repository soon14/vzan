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
    /// crm系统对接配置表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CrmApiData
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int PageIndex { get; set; }
        [SqlField]
        public int PageSize { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        [SqlField]
        public string CurrentTime { get; set; }
        [SqlField]
        public int TotalPageSize { get; set; }
    }
}
