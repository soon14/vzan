using Entity.Base;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 餐饮多门店配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishTable
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int aId { get; set; }

        /// <summary>
        /// 门店id
        /// </summary>
        [SqlField]
        public int storeId { get; set; }

        /// <summary>
        /// 桌台名称
        /// </summary>
        [SqlField]
        public string table_name { get; set; }

        /// <summary>
        /// 人数  
        /// </summary>
        [SqlField]
        public int table_renshu { get; set; }

        /// <summary>
        /// 桌台排序
        /// </summary>
        [SqlField]
        public int table_sort { get; set; }

        /// <summary>
        /// 二维码路径
        /// </summary>
        [SqlField]
        public string table_qrcode { get; set; }

        /// <summary>
        /// 桌台状态 枚举：TableStateEnums
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
    }
}
