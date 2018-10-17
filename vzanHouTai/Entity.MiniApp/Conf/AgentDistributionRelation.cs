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
    /// 代理分销关联表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentDistributionRelation
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 分销代理商ID
        /// </summary>
        [SqlField]
        public int AgentId { get; set; }

        /// <summary>
        /// 父级分销代理商ID
        /// </summary>
        [SqlField]
        public int ParentAgentId { get; set; }

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
        /// 代理商提交用户时间
        /// </summary>
        [SqlField]
        public DateTime SubmitTime { get; set; }
        
        /// <summary>
        /// -2：永久删除，-1：停用，0或1：正常
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;
        public string StateStr
        {
            get
            {
                switch (State)
                {
                    case -2: return "已删除";
                    case -1: return "停用";
                    default: return "正常";
                }
            }
        }
        /// <summary>
        /// 跟进状态，0：代理跟进，1：小未跟进
        /// </summary>
        [SqlField]
        public int FollowState { get; set; } = 0;
        public string FollowStateStr
        {
            get
            {
                return Enum.GetName(typeof(AgentDistributionFollowState), FollowState);
            }
        }
        /// <summary>
        /// 关联代理分销二维码表Id（agentqrcode）
        /// </summary>
        [SqlField]
        public int QrCodeId { get; set; } = 0;
        /// <summary>
        /// 跟进业务员ID
        /// </summary>
        [SqlField]
        public string FollowerId { get; set; }
        /// <summary>
        /// 开通类型，0：申请代理，1：开通小程序
        /// </summary>
        [SqlField]
        public int OpenType { get; set; }
        /// <summary>
        /// 是否开启同步数据，1：开启，0，不开启
        /// </summary>
        [SqlField]
        public int OpenSyncData { get; set; }
        public string OpenSyncDataStr { get { return OpenSyncData == 1 ? "已开启" : "已关闭"; } }

        /// <summary>
        /// 上级分销代理名称
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 分销代理登陆ID
        /// </summary>
        public string LoginId { get; set; }
        /// <summary>
        /// 分销代理名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 分销代理手机号
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 开单/预存次数
        /// </summary>
        public int OpenOrder { get; set; }
        /// <summary>
        /// 最后一次预存款
        /// </summary>
        public int LastDeposit { get; set; } = 0;
        public string LastDepositStr { get { return (LastDeposit * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 店铺总数
        /// </summary>
        public int StoreCount { get; set; }
        /// <summary>
        /// 屏蔽店铺数
        /// </summary>
        public int CloseSyncStoreCount { get; set; }
        /// <summary>
        /// 小程序总数
        /// </summary>
        public int AppCount { get; set; }
        /// <summary>
        /// 平台访问量
        /// </summary>
        public int PlatViewCount { get; set; }

        /// <summary>
        /// 来源客户
        /// </summary>
        public string SourceFrom { get; set; }
        /// <summary>
        /// 来源客户AccountdId
        /// </summary>
        public string UserAccountId { get; set; }
        public string Desc { get; set; }
    }
}
