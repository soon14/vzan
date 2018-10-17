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
    /// 队列
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishQueue
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
        /// 队列名称
        /// </summary>
        [SqlField]
        public string q_name { get; set; }

        /// <summary>
        /// 人数  
        /// </summary>
        [SqlField]
        public int q_renshu { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>
        [SqlField]
        public string q_qianzhui { get; set; }

        /// <summary>
        /// 提前通知的人数
        /// </summary>
        [SqlField]
        public string q_tongzhi_renshu { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int q_order { get; set; }

        /// <summary>
        /// 状态 1为启用 0为关闭 -1为删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 当前号码
        /// </summary>
        public int p_haoma { get; set; } = 0;


        /// <summary>
        /// 当前排队的人数
        /// </summary>
        public int pd_count { get; set; } = 0;

        /// <summary>
        /// 当前到号
        /// </summary>
        public string pd_this_num { get; set; } = string.Empty;

        /// <summary>
        /// 当前排队号
        /// </summary>
        [SqlField]
        public int q_curnumber { get; set; } = 0;
        
    }
}
