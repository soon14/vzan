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
    /// 关键词申购记录
    /// </summary>
    [SqlTable(dbEnum.MINIAPP)]
    public class ApplyKeyWord
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public decimal Price { get; set; } = 0;

        [SqlField]
        public string Phone { get; set; } = string.Empty;
        
        [SqlField]
        public int KeyWordId { get; set; } = 0;
        public string KeyWordName { get; set; }

        /// <summary>
        /// 0：未成功，1：成功
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
    }
}
