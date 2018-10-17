using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 新订单提示的店铺ID记录表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishHavingNewOrder
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField(IsPrimaryKey = true)]
        public int storeId { get; set; }
    }
}
