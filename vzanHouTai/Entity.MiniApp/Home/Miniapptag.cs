using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Home
{
    /// <summary>
    /// 小程序案例标签
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Miniapptag
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;
        /// <summary>
        /// 小程序模板id
        /// </summary>
        [SqlField]
        public int tid { get; set; } = 0;
        /// <summary>
        /// 小程序模板名称
        /// </summary>
        public string tname { get; set; } = string.Empty;
        /// <summary>
        /// 标签名称
        /// </summary>
        [SqlField]
        public string tagname { get; set; } = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }
        public string AddTimeStr { get { return addtime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; }
        public string UpdateTimeStr { get { return updatetime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// 状态 -1:删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 代理商ID
        /// </summary>
        [SqlField]
        public int AgentId { get; set; } = 0;
        public bool SelTag { get; set; } = false;
    }
}
