using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 会员升级规则
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipRule
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public string appId { get; set; } = string.Empty;

        /// <summary>
        /// 状态 0:正常 -1：删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 最小消费金额
        /// </summary>
        [SqlField]
        public int minMoney { get; set; } = 0;

        public string showminMoney
        {
            get
            {
                return (minMoney*0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 最大消费金额
        /// </summary>
        [SqlField]
        public int maxMoney { get; set; } = 0;
        
        public string showmaxMoney
        {
            get
            {
                return (maxMoney*0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; }
        [SqlField]
        public int levelid { get; set; } = 0;
        public VipLevel levelinfo { get; set; }

        /// <summary>
        /// 0表示按照消费金额  1表示按照储值金额
        /// </summary>
        [SqlField]
        public int RuleType { get; set; } = 0;


       

    }
}
