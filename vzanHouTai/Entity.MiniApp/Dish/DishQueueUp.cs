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
    public class DishQueueUp
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
        /// 队列Id
        /// </summary>
        [SqlField]
        public int q_catid { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        [SqlField]
        public string cate_name { get; set; }

        /// <summary>
        /// 用户Id  
        /// </summary>
        [SqlField]
        public int user_Id { get; set; }

        /// <summary>
        /// 人数  
        /// </summary>
        [SqlField]
        public int q_renshu { get; set; }

        /// <summary>
        /// 手机号码  
        /// </summary>
        [SqlField]
        public string q_phone { get; set; }

        /// <summary>
        /// 排队号码  
        /// </summary>
        [SqlField]
        public string q_haoma { get; set; }

        /// <summary>
        /// 子号码  
        /// </summary>
        [SqlField]
        public int q_z_haoma { get; set; }

        /// <summary>
        /// 是否已经通知  1 已通知,0 未通知
        /// </summary>
        [SqlField]
        public int q_is_tongzhi { get; set; }

        /// <summary>
        /// 排队时间
        /// </summary>
        [SqlField]
        public DateTime q_addtime { get; set; }

        public string q_addtime_str
        {
            get
            {
                return q_addtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 状态 枚举：DishEnums.QueueUpEnums
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
    }
}
