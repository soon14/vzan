using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Home
{
    public class HomeViewModel
    {
        /// <summary>
        /// 首部海报
        /// </summary>
        public Homepic headImg { get; set; }
        public Homepic zbImg { get; set; }
        public Homepic tcImg { get; set; }
        public Homepic sqImg { get; set; }
        public Homepic yyImg { get; set; }
        public Homepic hmImg { get; set; }
        /// <summary>
        /// 中部轮播图
        /// </summary>
        public List<Homepic> bodylist { get; set; }
        /// <summary>
        /// 新闻列表
        /// </summary>
        public List<Homenews> newsList { get; set; }
        /// <summary>
        ///百科列表
        /// </summary>
        public List<Homenews> bknewsList { get; set; }
        /// <summary>
        /// 百科菜单
        /// </summary>
        public List<Homebkmenu> bkmenu { get; set; }
        /// <summary>
        /// 分部视图跳转类型
        /// </summary>
        public string Pagetype { get; set; } = string.Empty;
        /// <summary>
        /// 单条新闻
        /// </summary>
        public Homenews news { get; set; }
        /// <summary>
        /// 最新的直播百科
        /// </summary>
        public List<Homenews> newzb { get; set; }
        /// <summary>
        /// 最新的同城百科
        /// </summary>
        public List<Homenews> newtc { get; set; }
        /// <summary>
        /// 最新的有约百科
        /// </summary>
        public List<Homenews> newyy { get; set; }
        /// <summary>
        /// 最新的论坛百科
        /// </summary>
        public List<Homenews> newlt { get; set; }
        /// <summary>
        /// 浏览量最多的新闻资讯
        /// </summary>
        public List<Homenews> newnews { get; set; }
        /// <summary>
        /// 新闻总数
        /// </summary>
        public int TotalCount { get; set; } = 0;
        /// <summary>
        /// 直播案例
        /// </summary>
        public List<Homecase> zbCase { get; set; }
        /// <summary>
        /// 同城案例
        /// </summary>
        public List<Homecase> tcCase { get; set; }
        /// <summary>
        /// 社区案例
        /// </summary>
        public List<Homecase> sqCase { get; set; }
        /// <summary>
        /// 有约案例
        /// </summary>
        public List<Homecase> yyCase { get; set; }
        /// <summary>
        /// 基本信息配置
        /// </summary>
        public HomeConfig config { get; set; }

        public string keywords { get; set; }

    }
}
