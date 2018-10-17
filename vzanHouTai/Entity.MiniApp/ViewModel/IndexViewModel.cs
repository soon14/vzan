using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.ViewModel
{
    public class IndexViewModel
    {
        /// <summary>
        /// 代理信息
        /// </summary>
        public Agentinfo agentinfo { get; set; }
        /// <summary>
        /// 累计客户
        /// </summary>
        public int CustomerCount { get; set; }
        /// <summary>
        /// 累计售出模板
        /// </summary>
        public int TemplateCount { get; set; }
        /// <summary>
        /// 累计模板费用
        /// </summary>
        public int TemplateSum { get; set; }
    }
}