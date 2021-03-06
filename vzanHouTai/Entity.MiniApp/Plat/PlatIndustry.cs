﻿using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    /// 行业
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatIndustry
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        
        /// <summary>
        /// 行业名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        [SqlField]
        public int ParentId { get; set; } = 0;
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
    }
}
