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
    /// 小程序模板数据表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Moduls
    {
        public Moduls() { }
        /// <summary>
        /// 小程序ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商家名称Id
        /// </summary>
        [SqlField]
        public int appId { get; set; }
        /// <summary>
        /// 层级
        /// </summary>
        [SqlField]
        public int Level { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        [SqlField]
        public int Hidden { get; set; }
        /// <summary>
        /// 状态（0,1）
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 小图标地址
        /// </summary>
        [SqlField]
        public string LitleImgUrl { get; set; }
        /// <summary>
        /// 板块名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string Content { get; set; }
        /// <summary>
        /// 纯文本内容1
        /// </summary>
        [SqlField]
        public string Content2 { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 经纬度
        /// </summary>
        [SqlField]
        public string AddressPoint { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        [SqlField]
        public string mobile { get; set; }
        /// <summary>
        /// 模板颜色
        /// </summary>
        [SqlField]
        public string Color { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SqlField]
        public DateTime Lastdate { get; set; }
        public string LastdateStr { get { return Lastdate.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime Createdate { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; }
        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int ViewCount { get; set; }
        /// <summary>
        /// 视频链接
        /// </summary>
        [SqlField]
        public string VideoUrl { get; set; } = "";
        /// <summary>
        /// 视频类型，0：网络，1：本地
        /// </summary>
        [SqlField]
        public int VideoType { get; set; }
        /// <summary>
        /// 发展历程数据集
        /// </summary>
        public List<Development> MiniappdevelopmentList { get; set; }
    }
}
