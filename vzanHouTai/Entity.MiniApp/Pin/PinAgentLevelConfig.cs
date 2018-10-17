using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 代理等级设置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinAgentLevelConfig
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 等级
        /// </summary>
        [SqlField]
        public int LevelId { get; set; } = 0;

        /// <summary>
        /// 小程序aid
        /// </summary>
        [SqlField]
        public int Aid { get; set; } = 0;

        /// <summary>
        /// 等级名称
        /// </summary>
        [SqlField]
        public string LevelName { get; set; } = string.Empty;


        /// <summary>
        /// 代理入驻费用 分为单位
        /// </summary>
        [SqlField]
        public int AgentFee { get; set; } = 0;
        public string AgentFeeStr
        {
            get
            {
                return (AgentFee * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 代理提成(自推代理) 百分比 10=1%
        /// </summary>
        [SqlField]
        public int AgentExtract { get; set; } = 400;

        public string AgentExtractStr
        {
            get
            {
                return (AgentExtract * 0.1).ToString("0.0");
            }
        }
        /// <summary>
        /// 订单提成(自推商家) 百分比 10=1%
        /// </summary>
        [SqlField]
        public int OrderExtract { get; set; } = 30;

        public string OrderExtractStr
        {
            get
            {
                return (OrderExtract * 0.1).ToString("0.0");
            }
        }

        /// <summary>
        /// 渠道推代理分佣比例  10=1%
        /// </summary>
        [SqlField]
        public int SecondAgentExtract { get; set; } = 20;
        public string SecondAgentExtractStr
        {
            get
            {
                return (SecondAgentExtract * 0.1).ToString("0.0");
            }
        }

        /// <summary>
        /// 渠道推商家分佣比例  10=1%
        /// </summary>
        [SqlField]
        public int SecondOrderExtract { get; set; } = 3;
        public string SecondOrderExtractStr
        {
            get
            {
                return (SecondOrderExtract * 0.1).ToString("0.0");
            }
        }

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }


        /// <summary>
        /// 状态 0 正常 -1删除
        /// </summary>
        [SqlField]
        public int State { get; set; }





    }
}
