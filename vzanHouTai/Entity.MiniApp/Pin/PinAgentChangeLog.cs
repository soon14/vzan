using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Pin
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class PinAgentChangeLog
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public int Aid { get; set; } = 0;

        [SqlField]
        public int AgentId { get; set; } = 0;

        /// <summary>
        /// 变更money  分为单位
        /// </summary>
        [SqlField]
        public int ChangeMoney { get; set; } = 0;

        public string ChangeMoneyStr
        {
            get
            {
                return (ChangeMoney * 0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 变更日志描述
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;


        /// <summary>
        /// 操作类型 0续费 1升级 2代理入驻
        /// </summary>
        [SqlField]
        public int ChangeType { get; set; } = 0;

        public string ChangeTypeStr
        {
            get
            {
                if (ChangeType == 1)
                {
                    return "升级";
                }else if (ChangeType ==2)
                {
                    return "入驻";
                }
                else
                {
                    return "续费";
                }
            }
        }

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;

        public string AddTimeStr
        {
            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:MM:ss");
            }

        }


    }
}
