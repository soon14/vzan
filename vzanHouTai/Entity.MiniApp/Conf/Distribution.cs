using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 分销商信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Distribution
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;
        /// <summary>
        /// 代理商 代理id
        /// </summary>
        [SqlField]
        public int parentAgentId { get; set; } = 0;
        /// <summary>
        /// 分销代理 代理id
        /// </summary>
        [SqlField]
        public int AgentId { get; set; } = 0;
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime modifyDate { get; set; }

        /// <summary>
        /// 用户状态 -1:停用，1:启用 （此状态与D_Agentinfo表state同步）
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 分销商名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 分销商 accountid
        /// </summary>
        [SqlField]
        public string useraccountid { get; set; } = string.Empty;
        
        [SqlField]
        public int type { get; set; }
    }


    /// <summary>
    /// 代理后台-分销商管理-列表实体
    /// </summary>
    public class DistributionModel
    {
        /// <summary>
        /// 分销商账号
        /// </summary>
        public string LoginId { get; set; } = string.Empty;

        /// <summary>
        /// 代理商
        /// </summary>
        public int parentAgentId { get; set; } = 0;

        /// <summary>
        /// 分销代理 代理id
        /// </summary>
        public int AgentId { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime addtime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime modifyDate { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public int state { get; set; } = 0;

        /// <summary>
        /// 分销商名称
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 预存款
        /// </summary>
        public int deposit { get; set; } = 0;
        /// <summary>
        /// 显示预存款
        /// </summary>
        public string showDeposit { get; set; } = string.Empty;
        /// <summary>
        /// 已创建客户数量
        /// </summary>
        public int createCustomerCount { get; set; } = 0;

        /// <summary>
        /// 已购买模板数量
        /// </summary>
        public int buyTemplateCount { get; set; } = 0;
        /// <summary>
        /// 小程序模板列表 序列化
        /// </summary>
        public string templateList { get; set; } = string.Empty;

        /// <summary>
        /// 已创建客户列表
        /// </summary>
        public List<CustomerModel> customerList { get; set; }
        /// <summary>
        /// 模板购买列表
        /// </summary>
        public List<XcxTemplateDetail> detailList { get; set; }
    }
}
