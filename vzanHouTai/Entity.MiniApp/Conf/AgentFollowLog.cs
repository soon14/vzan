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
    /// 代理跟进记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentFollowLog
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 关联表agentdistributionrelation的id
        /// </summary>
        [SqlField]
        public int AgentDistributionRelatioinId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Desc { get; set; } = string.Empty;
        /// <summary>
        /// -2：永久删除，-1：停用，0或1：启用
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;
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
        /// 0：代理自己记录，1：小未业务记录
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 0;
        /// <summary>
        /// 来源，0：小未数据，1：crm爱克系统数据
        /// </summary>
        [SqlField]
        public int SouceFrom { get; set; } = 0;
        /// <summary>
        /// 作者
        /// </summary>
        [SqlField]
        public string Writer { get; set; } = string.Empty;
    }
}
