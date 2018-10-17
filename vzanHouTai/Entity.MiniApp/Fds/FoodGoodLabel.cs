using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序餐饮版-商品标签
    /// </summary>
    [Serializable]
    [SqlTable(Utility.dbEnum.MINIAPP)]
    public class FoodGoodsLabel
    {
        public FoodGoodsLabel() { }
        /// <summary>
        /// 属性ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商家id
        /// </summary>
        [SqlField]
        public int FoodStoreId { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        [SqlField]
        public int GoodId { get; set; }


        /// <summary>
        /// 标签ID
        /// </summary>
        [SqlField]
        public int labelId { get; set; }

    }
}
