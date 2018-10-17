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
    /// 用户提现信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatUserCash
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public long UserId { get; set; }
        /// <summary>
        /// 绑定平台的AID
        /// </summary>
        [SqlField]
        public int AId { get; set; } = 0;
        /// <summary>
        /// 可提现金额（分）
        /// </summary>
        [SqlField]
        public int UseCash { get; set; } = 0;
        public string UseCashStr { get { return (UseCash * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 已提现金额（分）
        /// </summary>
        [SqlField]
        public int UseCashTotal { get; set; } = 0;
        public string UseCashTotalStr { get { return (UseCashTotal*0.01).ToString("0.00"); } }
        /// <summary>
        /// 手续费
        /// </summary>
        [SqlField]
        public int ServerCash { get; set; }
        public string ServerCashStr { get { return (ServerCash * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 实际到账金额
        /// </summary>
        public string InfactCashStr { get { return ((UseCashTotal- ServerCash) * 0.01).ToString("0.00"); } }
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 提现方式 MiniAppEnum.DrawCashWay
        /// </summary>
        [SqlField]
        public int DrawCashWay { get; set; }
        /// <summary>
        /// 银行账号
        /// </summary>
        [SqlField]
        public string AccountBank { get; set; } = "";
        public string AccountBankStr { get { return (string.IsNullOrEmpty(AccountBank) ?"空": (AccountBank.Substring(0, 4) + "****" + AccountBank.Substring(AccountBank.Length - 3, 3))); } }
        /// <summary>
        /// 账号名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = "";
        /// <summary>
        /// 是否开启分销，1：开启，0：关闭
        /// </summary>
        [SqlField]
        public int IsOpenDistribution { get; set; } = 0;
        /// <summary>
        /// 提现手续费
        /// </summary>
        public int Fee { get; set; } = 0;
    }
}
