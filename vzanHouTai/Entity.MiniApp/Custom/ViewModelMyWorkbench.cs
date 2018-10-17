using Entity.MiniApp.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.MiniApp.User;
using Entity.MiniApp.Conf;

namespace Entity.MiniApp
{
    public class ViewModelMyWorkbench
    {
        public Member _Member { get; set; }
        
        public Account _Account { get; set; }
        
        public List<Homenews> bknewsList { get; set; }
        
        public List<XcxAppAccountRelation> miniapplist { get; set; }
        /// <summary>
        /// 是否是代理商开的客户
        /// </summary>
        public bool IsAgentCustomer { get; set; } = false;
        /// <summary>
        /// 是否成为代理商的推广者
        /// </summary>
        public bool IsDistributioner { get; set; } = false;
        
        /// <summary>
        /// 代理信息
        /// </summary>
        public Agentinfo Agentinfo { get; set; }
        /// <summary>
        /// 模板集合
        /// </summary>
        public List<XcxTemplate> TemplateList{ get; set; }
        /// <summary>
        /// 是否有设置自定义小程序水印
        /// </summary>
        public int BottomLogoCount { get; set; }
        /// <summary>
        /// 自定义界面logo
        /// </summary>
        public string CustomLogo { get; set; }
    }
}
