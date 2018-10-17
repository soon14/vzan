using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{

    /// <summary>
    /// 优惠规则
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DiscountRule
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int aId { set; get; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int storeId { get; set; }

        /// <summary>
        /// 满足金额(标示满足某金额触发优惠)   单位：分
        /// </summary>
        [SqlField]
        public float meetMoney { get; set; }

        /// <summary>
        /// 优惠金额   单位：分
        /// </summary>
        [SqlField]
        public float discountMoney { get; set; }

        /// <summary>
        /// 状态 0 为正常 -1 为删除
        /// </summary>
        [SqlField]
        public int state { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime createDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updateDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string updateDateStr
        {
            get
            {
                if (updateDate == null || updateDate == Convert.ToDateTime("0001-01-01 00:00:00"))
                {
                    return string.Empty;
                }
                else
                {
                    return updateDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }
    }
}
