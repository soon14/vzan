using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatStoreAddRules
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId   小程序
        /// </summary>
        [SqlField]
        public int Aid { get; set; }

        /// <summary>
        /// 置顶时间 月
        /// </summary>
        [SqlField]
        public int YearCount { get; set; } = 1;


        /// <summary>
        /// 置顶所需金额 分
        /// </summary>
        [SqlField]
        public int CostPrice { get; set; } = 0;

        public string CostPriceStr
        {
            get
            {
                return (CostPrice * 0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 规则更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        public string UpdateTimeStr
        {

            get
            {
                return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 状态 0→正常 -1 →删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr
        {

            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public string Unit
        {
            get
            {
                return "月";
            }
        }
    }
}
