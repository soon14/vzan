using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Home
{
    /// <summary>
    ///Homekeyword:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Homekeyword
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 关键词
        /// </summary>
        [SqlField]
        public string keyword { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
    }
}
