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
    /// 小程序时间发展历程
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Development
    {
        public Development(){}
        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商家Id
        /// </summary>
        [SqlField]
        public int appId { get; set; }
        /// <summary>
        /// 层级
        /// </summary>
        [SqlField]
        public int Level { get; set; }
        /// <summary>
        /// 状态（0,1）
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string Content { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SqlField]
        public DateTime Lastdate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime Createdate { get; set; }

        /// <summary>
        /// 年
        /// </summary>
        [SqlField]
        public string Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        [SqlField]
        public string Month { get; set; }
    }
}
