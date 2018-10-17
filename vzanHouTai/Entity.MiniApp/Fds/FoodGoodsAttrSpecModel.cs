using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 用于餐饮版每个商品的规格属性时的连表查询返回数据
    /// </summary>
    public class FoodGoodsAttrSpecModel
    {
        /// <summary>
        /// 商品id
        /// </summary>
        public int foodGoodsId { get; set; } = 0;
        /// <summary>
        /// 规格id
        /// </summary>
        public int attrId { get; set; } = 0;
        /// <summary>
        /// 店铺id
        /// </summary>
        public int foodId { get; set; } = 0;
        /// <summary>
        /// 规格名称
        /// </summary>
        public string attrName { get; set; } = string.Empty;

        /// <summary>
        /// 规格状态
        /// </summary>
        public int attrState { get; set; } = 0;
        /// <summary>
        /// 属性id
        /// </summary>
        public int specId { get; set; } = 0;
        /// <summary>
        /// 属性名称
        /// </summary>
        public string specName { get; set; } = string.Empty;
        /// <summary>
        /// 属性状态
        /// </summary>
        public int specState { get; set; } = 0;
        public FoodGoodsAttr attr { get; set; }
        public FoodGoodsSpec spec { get; set; }

    }
}
