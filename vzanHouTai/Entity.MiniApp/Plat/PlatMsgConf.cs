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
    /// 信息审核配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class PlatMsgConf
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; }


        /// <summary>
        /// 审核设置 发帖时是否需要审核
        /// 0 → 不需要审核  1 →先审核后发布 2 →先发布后审核
        /// </summary>
        [SqlField]
        public int ReviewSetting { get; set; } = 0;//不需要审核 默认

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }


    }
}
