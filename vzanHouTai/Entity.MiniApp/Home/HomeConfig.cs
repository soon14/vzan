using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Home
{
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class HomeConfig
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 类型 0为官网全局配置
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        /// <summary>
        /// 图片链接
        /// </summary>
        [SqlField]
        public string config { get; set; } = string.Empty;

        public ConfigAttr attr { get; set; }
    }
    public class ConfigAttr
    {
        /// <summary>
        /// 客服1
        /// </summary>
        public string kf1 { get; set; } = string.Empty;
        /// <summary>
        /// 客服2
        /// </summary>
        public string kf2 { get; set; } = string.Empty;
        /// <summary>
        /// 招商电话
        /// </summary>
        public string zsTel { get; set; } = string.Empty;
        /// <summary>
        /// 招商手机
        /// </summary>
        public string zsPhone { get; set; } = string.Empty;
        /// <summary>
        ///全局标题
        /// </summary>
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// 全局描述
        /// </summary>
        public string description { get; set; } = string.Empty;
        /// <summary>
        /// 招商二维码
        /// </summary>
        public string zsQrCode { get; set; } = string.Empty;
    }
}
