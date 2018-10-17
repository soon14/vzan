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
    /// 区域表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class AreaRegion
    {
        /// <summary>
        /// 区域Code
        /// </summary>
        [SqlField(IsPrimaryKey = true)]
        public int AreaCode { get; set; } = 0;

        /// <summary>
        /// 区域名
        /// </summary>
        [SqlField]
        public string AreaName { get; set; } = string.Empty;

        /// <summary>
        /// 父类Code
        /// </summary>
        [SqlField]
        public int ParentCode { get; set; } = 0;

        /// <summary>
        /// 是否推荐
        /// </summary>
        [SqlField]
        public int IsVouch { get; set; } = 0;
    }


    /// <summary>
    /// 区域表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class FAreaRegion
    {
        /// <summary>
        /// 区域Code
        /// </summary>
        [SqlField(IsPrimaryKey = true)]
        public long AreaCode { get; set; } = 0;

        /// <summary>
        /// 区域名
        /// </summary>
        [SqlField]
        public string AreaName { get; set; } = string.Empty;

        /// <summary>
        /// 父类Code
        /// </summary>
        [SqlField]
        public long ParentCode { get; set; } = 0;

        /// <summary>
        /// 是否推荐
        /// </summary>
        [SqlField]
        public int IsVouch { get; set; } = 0;
    }
}
