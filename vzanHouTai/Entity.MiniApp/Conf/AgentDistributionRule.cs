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
    /// 代理分销规则表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentDistributionRule
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 分销级别，目前有：一级分销，二级分销
        /// </summary>
        [SqlField]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 界限值
        /// </summary>
        [SqlField]
        public int LimitValue { get; set; }

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
        /// 分销规则类型
        /// </summary>
        [SqlField]
        public int Type { get; set; }
        /// <summary>
        /// 抽成
        /// </summary>
        [SqlField]
        public double Percentage { get; set; }
        /// <summary>
        /// -2：永久删除，-1：停用，0或1：启用
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;

        /// <summary>
        /// 下一级分销
        /// </summary>
        [SqlField]
        public int NexPercentageId { get; set; }
    }
}
