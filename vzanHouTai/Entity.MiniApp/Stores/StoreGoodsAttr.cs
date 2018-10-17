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
    /// 小程序商城模板-商品规格
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class StoreGoodsAttr
    {
        public StoreGoodsAttr() { }
        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }
        /// <summary>
        /// 规格名称
        /// </summary>
        [SqlField]
        public string AttrName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }

        public List<StoreGoodsSpec> SpecList { get; set; }
    }
}
