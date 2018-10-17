using Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 代理返款记录表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentCaseBack
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 银行账号
        /// </summary>
        [SqlField]
        public string BankAccount { get; set; }
        /// <summary>
        /// 支付宝账号
        /// </summary>
        [SqlField]
        public string AlipayAccount { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        [SqlField]
        public string CourierNumber { get; set; }

        /// <summary>
        /// 发票类型：0：纸质发票（需要填写电子单号），1：电子发票（需要上传电子发票图片）
        /// </summary>
        [SqlField]
        public int Invoice { get; set; } = 0;

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
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Desc { get; set; }
        /// <summary>
        /// 电子发票图片
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }
        /// <summary>
        /// 关联表agentdistributionrelation的id
        /// </summary>
        [SqlField]
        public int AgentDistributionRelatioinId { get; set; }
        /// <summary>
        /// -2：永久删除，-1：失效，0：待确认，1：驳回，2：已确认，3：已打款(AgentCaseBackState)
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 业务人员
        /// </summary>
        [SqlField]
        public string Follower { get; set; }

        /// <summary>
        /// 返款凭证图片
        /// </summary>
        [SqlField]
        public string CaseBackImgUrl { get; set; }
        /// <summary>
        /// 参与抽成用户信息
        /// </summary>
        [SqlField]
        public string DUserInfo { get; set; }
        public List<DistributionUserInfo> DUserInfoStr { get { return !string.IsNullOrEmpty(DUserInfo) ? JsonConvert.DeserializeObject<List<DistributionUserInfo>>(DUserInfo) : new List<DistributionUserInfo>(); } }
        /// <summary>
        /// 小未抽成
        /// </summary>
        [SqlField]
        public double DZPercentage { get; set; }
        /// <summary>
        /// 返款类型，0：代理返款，1：商家返款
        /// </summary>
        [SqlField]
        public int DataType { get; set; }

        public string Level { get; set; }
        /// <summary>
        /// 代理ID
        /// </summary>
        public int AgentId { get; set; }
        public string AgentName { get; set; }
        /// <summary>
        /// 代理userid
        /// </summary>
        public string UseraccountId { get; set; }
        public int LastDeposit { get; set; }
        public string LastDepositStr { get { return (LastDeposit * 0.01).ToString("0.00"); } }
        public int QrCodeId { get; set; }
        public string LoginId { get; set; }
        public string StateStr { get { return Enum.GetName(typeof(AgentCaseBackState),State); } }
        /// <summary>
        /// 渠道
        /// </summary>
        public string SourceFrom { get; set; }
    }

    public class DistributionUserInfo
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public double Percentage { get; set; }
        public string LevelName { get; set; }
        public int RuleId { get; set; }
        public string RuleName { get; set; }
        public int NexPercentageId { get; set; }
    }
}
