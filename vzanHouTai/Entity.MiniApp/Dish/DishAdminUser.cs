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
    /// 餐饮多门店配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishAdminUser
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int aid { get; set; }

        /// <summary>
        /// 门店id
        /// </summary>
        [SqlField]
        public int storeId { get; set; }

        /// <summary>
        /// 登录名
        /// </summary>
        [SqlField]
        public string login_username { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [SqlField]
        public string login_userpass { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }
    }
}
