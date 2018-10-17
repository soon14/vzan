using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Stores
{
    /// <summary>
    /// 小程序商城模板-商品属性
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreGoodsSpec
    {
        public StoreGoodsSpec() { }
        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 规格id
        /// </summary>
        [SqlField]
        public int AttrId { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        [SqlField]
        public string SpecName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }
    }
}
