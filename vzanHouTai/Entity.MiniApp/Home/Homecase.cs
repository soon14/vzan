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
    ///Homenews:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Homecase
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 案例名称
        /// </summary>
        [SqlField]
        public string casename { get; set; }
        /// <summary>
        /// 图片链接
        /// </summary>
        [SqlField]
        public string ImgLink { get; set; } = string.Empty;
        /// <summary>
        /// 封面图片
        /// </summary>
        [SqlField]
        public string coverPath { get; set; } = string.Empty;
        /// <summary>
        /// 类型 对应项目请看下面 枚举：casetype
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 1;
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 二维码图片
        /// </summary>
        [SqlField]
        public string QrcodePath { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
        /// <summary>
        /// 小程序案例类型 即小程序模板id
        /// </summary>
        [SqlField]
        public int casetype { get; set; } = 0;
        /// <summary>
        /// 小程序模板名称
        /// </summary>
        public string tname { get; set; } = string.Empty;
        /// <summary>
        /// 小程序标签id
        /// </summary>
        [SqlField]
        public string tagids { get; set; } = string.Empty;
        /// <summary>
        /// 标签名称
        /// </summary>
        public string tagnames { get; set; } = string.Empty;

    }
    public enum caseType
    {
        /// <summary>
        /// 直播案例
        /// </summary>
        zbcase = 1,
        /// <summary>
        /// 同城案例
        /// </summary>
        tccase = 2,
        /// <summary>
        /// 社区案例
        /// </summary>
        sqcase = 3,
        /// <summary>
        /// 有约案例
        /// </summary>
        yycase = 4,
        /// <summary>
        /// 小程序广告
        /// </summary>
        xcxadv = 5,
        /// <summary>
        /// 小程序案例
        /// </summary>
        xcxcase = 6
    }
}
