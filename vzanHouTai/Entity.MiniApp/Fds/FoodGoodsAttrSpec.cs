using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序商城模板-商品规格属性关系表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsAttrSpec
    {
        public FoodGoodsAttrSpec() { }
        /// <summary>
        /// 商品规格属性关系Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商品id
        /// </summary>
        [SqlField]
        public int FoodGoodsId { get; set; }
        /// <summary>
        /// 规格id
        /// </summary>
        [SqlField]
        public int AttrId { get; set; }
        /// <summary>
        /// 属性id
        /// </summary>
        [SqlField]
        public int SpecId { get; set; }
    }
}
