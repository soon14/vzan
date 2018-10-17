using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Home
{
    public class QuestionViewModel
    {
        /// <summary>
        /// 分类列表
        /// </summary>
        public List<Homebkmenu> menuList { get; set; }

        /// <summary>
        /// 常见问题列表
        /// </summary>
        public List<Homenews> commonList { get; set; }

        /// <summary>
        /// 热门问题列表
        /// </summary>
        public List<Homenews> hotList { get; set; }

        /// <summary>
        /// 相关问题列表
        /// </summary>
        public List<Homenews> relevantList { get; set; }
        /// <summary>
        /// 详细新闻
        /// </summary>
        public Homenews news { get; set; }
        /// <summary>
        /// 广告位列表
        /// </summary>
        public List<Homecase> advList { get; set; }
    }
}
