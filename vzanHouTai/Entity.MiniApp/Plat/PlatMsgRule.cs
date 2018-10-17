using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    /// 信息置顶规则表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class PlatMsgRule
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
        public int Aid { get; set; }

        /// <summary>
        /// 置顶时间天数
        /// </summary>
        [SqlField]
        public int ExptimeDay { get; set; } = 0;


        /// <summary>
        /// 置顶所需金额 分
        /// </summary>
        [SqlField]
        public int Price { get; set; } = 0;

        public string PriceStr
        {
            get
            {
                return (Price * 0.01).ToString("0.00");
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

    }
}
