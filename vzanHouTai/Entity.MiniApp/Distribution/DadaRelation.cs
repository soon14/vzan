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
    /// 权限与达达商户关联表
    /// </summary>
    public class DadaRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int rid { get; set; }
        /// <summary>
        /// 达达商户表ID
        /// </summary>
        [SqlField]
        public int merchantid { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }
        /// <summary>
        /// 状态：0：正常，-1：已删除
        /// </summary>
        [SqlField]
        public int state { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }
    }
}
