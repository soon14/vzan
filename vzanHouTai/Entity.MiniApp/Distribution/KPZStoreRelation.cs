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
    /// 权限与快跑者商户关联表
    /// </summary>
    public class KPZStoreRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; }
        /// <summary>
        /// 店铺ID，多门店备用
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }
        /// <summary>
        ///手机号码
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 团队token
        /// </summary>
        [SqlField]
        public string TeamToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int State { get; set; }
    }
}
