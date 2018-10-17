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
    /// 区域表 - 百度
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_Area
    {
        public C_Area() { }

        /// <summary>
        /// 区域代码
        /// </summary>
        [SqlField(IsPrimaryKey = true)]
        public int Code { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        [SqlField]
        public int ParentCode { get; set; }

        /// <summary>
        /// 级别 看 AreaLevel枚举 
        /// </summary>
        [SqlField]
        public int Level { get; set; }

        /// <summary>
        /// 区域名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 拼音
        /// </summary>
        [SqlField]
        public string PingYin { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 0;
    }
}
