using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Home
{
    /// <summary>
    /// 关键词类型
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class KeyWordType
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        
        [SqlField]
        public string Name { get; set; } = string.Empty;
        
        [SqlField]
        public int ParentId { get; set; } = 0;
        
        [SqlField]
        public int State { get; set; } = 0;
        
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}
