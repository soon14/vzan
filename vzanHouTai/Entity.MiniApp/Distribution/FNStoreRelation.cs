using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    /// <summary>
    /// 权限与蜂鸟店铺关联表
    /// </summary>
    public class FNStoreRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int rid { get; set; }
        /// <summary>
        /// 蜂鸟门店表ID
        /// </summary>
        [SqlField]
        public int fnstoreid { get; set; }
    }
}
