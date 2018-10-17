using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    [SqlTable(dbEnum.MINIAPP)]
    public class NavMenu
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public int ParentId { get; set; }
        [SqlField]
        public int AId { get; set; }
        /// <summary>
        /// 菜单功能类型（统计、产品、订单）
        /// </summary>
        [SqlField]
        public int Type { get; set; }
        /// <summary>
        /// 小程序模板类型
        /// </summary>
        [SqlField]
        public int PageType { get; set; }
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public string Name { get; set; }
        [SqlField]
        public string Url { get; set; }
        [SqlField]
        public string AppendPara { get; set; }
        [SqlField]
        public string Icon { get; set; }
        [SqlField]
        public int Sort { get; set; }
        [SqlField]
        public string Remark { get; set; }
    }
}
