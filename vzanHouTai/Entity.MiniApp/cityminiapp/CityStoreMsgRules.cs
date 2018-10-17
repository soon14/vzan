using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.cityminiapp
{
    /// <summary>
    /// 同城版-信息置顶规则表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class CityStoreMsgRules
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int aid { get; set; }

        /// <summary>
        /// 置顶时间天数
        /// </summary>
        [SqlField]
        public int exptimeday { get; set; } = 0;


        /// <summary>
        /// 置顶所需金额 分
        /// </summary>
        [SqlField]
        public int price { get; set; } = 0;

        public string priceStr
        {
            get
            {
                return (price * 0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 规则更新时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }

        public string updateTimeStr
        {

            get
            {
                return updateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 状态 0→正常 -1 →删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

    }
}
