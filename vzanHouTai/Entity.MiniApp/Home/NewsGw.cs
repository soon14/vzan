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
    /// 点赞官网新闻资讯
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class NewsGw
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;


        /// <summary>
        /// 类别 0→资讯  1 →深度观点
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 0;

        /// <summary>
        /// 新闻名称
        /// </summary>
        [SqlField]
        public string Title { get; set; } = string.Empty;


        /// <summary>
        /// 新闻简介
        /// </summary>
        [SqlField]
        public string Introduce { get; set; } = string.Empty;


        /// <summary>
        /// 链接
        /// </summary>
        [SqlField]
        public string NewsURL { get; set; } = string.Empty;

        /// <summary>
        /// 内容
        /// </summary>
        [SqlField]
        public string NewsContent { get; set; } = string.Empty;

      

        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 状态 0→删除  1 →发布
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;

        /// <summary>
        /// 排序 数字越大越靠前
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 1;

        /// <summary>
        /// 封面图片
        /// </summary>
        [SqlField]
        public string ImgPath { get; set; } = string.Empty;//默认封面

    }

}
