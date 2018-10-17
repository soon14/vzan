using Entity.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    ///Homenews:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentinfoCase
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 案例名称
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "案例名称不能为空")]
        [MinLength(length: 1, ErrorMessage = "案例名称最少1个字")]
        [MaxLength(length: 50, ErrorMessage = "案例名称最多100个字")]
        public string CaseName { get; set; }
        /// <summary>
        /// 图片链接
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 封面图片
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "封面图片不能为空")]
        public string CoverUrl { get; set; } = string.Empty;
        /// <summary>
        /// 类型 对应项目请看下面 枚举：casetype
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 0;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 1;
        /// <summary>
        /// 状态,-1:已删除，0：已下架，1：已上架
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        public string StateStr { get { return State == 1 ? "已上架" : "已下架"; } }
        /// <summary>
        /// 二维码图片
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "二维码图片不能为空")]
        public string QrCodeUrl { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public string UpdateTimeStr { get { return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 小程序模板ID
        /// </summary>
        [SqlField]
        public int TId { get; set; } = 0;
        /// <summary>
        /// 小程序模板名称
        /// </summary>
        public string TName { get; set; } = string.Empty;
        /// <summary>
        /// 小程序标签id
        /// </summary>
        [SqlField]
        public string TagIds { get; set; } = string.Empty;
        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagNames { get; set; } = string.Empty;
        /// <summary>
        /// 代理商ID
        /// </summary>
        [SqlField]
        public int AgentId { get; set; }
    }
}
