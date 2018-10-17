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
    public class Homenews
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string title { get; set; } = string.Empty;
        [SqlField]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string content { get; set; } = string.Empty;
        /// <summary>
        /// 类型
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
        /// 二级菜单id
        /// </summary>
        [SqlField]
        public int childNode { get; set; } = 0;
        /// <summary>
        /// 创建者
        /// </summary>
        [SqlField]
        public string creator { get; set; } = string.Empty;
        /// <summary>
        /// 封面图片
        /// </summary>
        [SqlField]
        public string ImgPath { get; set; } = string.Empty;//默认封面
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
        /// <summary>
        /// 浏览量
        /// </summary>
        [SqlField]
        public int count { get; set; } = 0;
        [SqlField]
        public string tags { get; set; } = string.Empty;
        /// <summary>
        /// 是否热门问题 0：否，1：是
        /// </summary>
        [SqlField]
        public int ishot { get; set; } = 0;
        /// <summary>
        /// 是否常见问题 0:否，1：是
        /// </summary>
        [SqlField]
        public int iscommon { get; set; } = 0;

    }

    /// <summary>
    /// 百科信息
    /// </summary>
    public class bknews
    {
        public int Id { get; set; } = 0;
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// 描述内容
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; } = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public int type { get; set; } = 0;
        /// <summary>
        /// 排序
        /// </summary>
        public int sort { get; set; } = 1;
        /// <summary>
        /// 状态
        /// </summary>
        public int state { get; set; } = 0;
        /// <summary>
        /// 二级菜单id
        /// </summary>
        public int childNode { get; set; } = 0;
        /// <summary>
        /// 创建者
        /// </summary>
        public string creator { get; set; } = string.Empty;
        /// <summary>
        /// 封面图片
        /// </summary>
        public string ImgPath { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 二级菜单名称
        /// </summary>
        public string nodeName { get; set; } = string.Empty;
        /// <summary>
        /// 浏览量
        /// </summary>
        public int count { get; set; } = 0;
    }

    public enum newsType
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
        /// 普通新闻
        /// </summary>
        news = 5,
        /// <summary>
        /// 直播新闻
        /// </summary>
        livenews = 6,
        /// <summary>
        /// 同城新闻
        /// </summary>
        citynews = 7,
        /// <summary>
        /// 有约新闻
        /// </summary>
        friendnews = 8,
        /// <summary>
        /// 社区新闻
        /// </summary>
        bbsnewws = 9,
        /// <summary>
        /// 小程序问答
        /// </summary>
        miniAPP = 10
    }
}
