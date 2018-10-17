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
    ///Homebkmenu:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Homebkmenu
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 菜单名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 归属
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 1;
       
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
    }
    public enum MenuType
    {
        /// <summary>
        /// 直播百科
        /// </summary>
        zbbk = 1,

        /// <summary>
        /// 同城百科
        /// </summary>
        tcbk = 2,

        /// <summary>
        /// 论坛百科
        /// </summary>
        ltbk = 3,

        /// <summary>
        /// 有约百科
        /// </summary>
        yybk = 4,
        /// <summary>
        /// 小程序问答
        /// </summary>
        miniapp = 5,
    }
}
