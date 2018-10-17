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
    ///Homepic:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Homepic
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 图片名称
        /// </summary>
        [SqlField]
        public string pictureName { get; set; } = string.Empty;
        /// <summary>
        /// 图片路径
        /// </summary>
        [SqlField]
        public string picturePath { get; set; } = string.Empty;
        /// <summary>
        /// 图片类型
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        /// <summary>
        /// 跳转链接
        /// </summary>
        [SqlField]
        public string url { get; set; } = string.Empty;
        /// <summary>
        /// 图片状态
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 图片状态
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 0;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
    }
    public enum imgType
    {
        /// <summary>
        /// 首页头部轮播图片
        /// </summary>
        headerImg = 1,

        /// <summary>
        /// 首页中部轮播图片
        /// </summary>
        bodyImg = 2,

        /// <summary>
        /// 直播海报
        /// </summary>
        zbImg=3,

        /// <summary>
        /// 同城海报
        /// </summary>
        tcImg=4,

        /// <summary>
        /// 社区海报
        /// </summary>
        sqImg=5,

        /// <summary>
        /// 有约海报
        /// </summary>
        yyImg=6,
        /// <summary>
        /// 首部海报
        /// </summary>
        headbg=7,
        /// <summary>
        /// 花猫海报
        /// </summary>
        hmImg=8
        
    }
}
