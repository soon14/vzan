using Entity.MiniApp.Conf ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.ViewModel
{
    public class VipViewModel
    {
        public VipConfig config { get; set; }

        public List<VipLevel> levelList { get; set; }

        /// <summary>
        /// 累计消费规则
        /// </summary>
        public List<VipRule> ruleList { get; set; }


        /// <summary>
        /// 累计充值规则
        /// </summary>
        public List<VipRule> SaveMoneyRuleList { get; set; }

    }

    public class MiniappVipInfo
    {
        public List<VipRelation> relationList { get; set; } = new List<VipRelation>();
        public int recordCount { get; set; } = 0;
        //客服人数
        public int kfCount { get; set; } = 0;
    }
}
