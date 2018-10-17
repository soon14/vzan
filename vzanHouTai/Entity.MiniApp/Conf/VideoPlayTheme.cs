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
    /// 大树视频主题表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VideoPlayTheme
    {

        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }

    }
}
