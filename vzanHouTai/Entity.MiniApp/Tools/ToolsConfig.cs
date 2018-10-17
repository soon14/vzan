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
    /// 小程序插件配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ToolsConfig
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 店铺ID
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        /// <summary>
        /// 是否开启砍价 0 表示关闭 1开启
        /// </summary>
        [SqlField]
        public int IsBargainOpen { get; set; } = 0;

    }
}
