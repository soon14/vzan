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
    /// 代理商预存款流水表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentdepositLog
    {
        public AgentdepositLog(int tid,int cost,Agentinfo agnetinfo,string log,string accountId,int type)
        {
            this.tid = tid;
            this.type = type;
            this.beforeDeposit = agnetinfo.deposit;
            this.agentid = agnetinfo.id;
            this.cost = cost;
            this.afterDeposit = agnetinfo.deposit - cost;
            this.costdetail = log;
            this.customerid = accountId;
            this.addtime = DateTime.Now;
        }
        public AgentdepositLog()
        {

        }
        /// <summary>
        /// 自增id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;
        /// <summary>
        /// 代理商id
        /// </summary>
        [SqlField]
        public int agentid { get; set; } = 0;
        /// <summary>
        /// 普通用户id
        /// </summary>
        [SqlField]
        public int acid { get; set; } = 0;

        /// <summary>
        /// 模板id
        /// </summary>
        [SqlField]
        public int tid { get; set; } = 0;
        /// <summary>
        /// 变更类型 1:充值  2:开通客户模板  3:分销商开通客户模板  4代理商续费  5分销商续费  6开通新门店  8代理商升级专业版版本  9分销商升级专业版版本 10变更智慧餐厅门店 11分销商变更智慧餐厅门店 12开启水印 13普通用户开通模板 14普通用户续费模板 15普通用户充值
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        public string typename { get { return Enum.GetName(typeof(AgentDepositLogType), type); } }

        /// <summary>
        /// 购买模板用户id
        /// </summary>
        [SqlField]
        public string customerid { get; set; } = string.Empty;
        /// <summary>
        /// 变更金额
        /// </summary>
        [SqlField]
        public int cost { get; set; } = 0;
        /// <summary>
        /// 变更金额显示
        /// </summary>
        public string showcost { get { return (cost * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 变更前的金额
        /// </summary>
        [SqlField]
        public int beforeDeposit { get; set; } = 0;
        /// <summary>
        /// 变更前的金额显示
        /// </summary>
        public string showbeforeDeposit { get { return (beforeDeposit * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 变更后的金额
        /// </summary>
        [SqlField]
        public int afterDeposit { get; set; } = 0;
        /// <summary>
        /// 变更后的金额显示
        /// </summary>
        public string showafterDeposit { get { return (afterDeposit * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 添加时间（变更时间）
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }
        /// <summary>
        /// 添加时间显示
        /// </summary>
        public string showaddtime { get { return addtime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 变更原因
        /// </summary>
        [SqlField]
        public string costdetail { get; set; } = string.Empty;
        /// <summary>
        /// 购买数量
        /// </summary>
        [SqlField]
        public int templateCount { get; set; } = 0;

        /// <summary>
        /// 权限ID
        /// </summary>
        [SqlField]
        public int Rid { get; set; } = 0;
        /// <summary>
        /// 过期时间（变更时间）
        /// </summary>
        [SqlField]
        public DateTime OutTime { get; set; }
    }
}
