using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using static Entity.MiniApp.Pin.PinEnums;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 平台代理信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinAgent
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        [SqlField]
        public int aId { get; set; } = 0;
        /// <summary>
        /// 上级代理uid
        /// </summary>
        [SqlField]
        public int fuserId { get; set; } = 0;
        /// <summary>
        /// 代理uid
        /// </summary>
        [SqlField]
        public int userId { get; set; } = 0;
        /// <summary>
        ///状态 0:未可用  1：正常  2:申请中
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        public string stateStr
        {
            get
            {
                return Enum.GetName(typeof(AgentState), state);
            }
        }
        /// <summary>
        /// 代理总收益  1000=1分
        /// </summary>
        [SqlField]
        public long money { get; set; }
        public string moneyStr
        {
            get
            {
                return (money * 0.01 * 0.001).ToString("0.00");
            }
        }
        /// <summary>
        /// 可提现金额 1=0.001分
        /// </summary>
        [SqlField]
        public long cash { get; set; }
        public string cashStr
        {
            get
            {
                return (cash * 0.01 * 0.001).ToString("0.00");
            }
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;
        public string addTimeStr
        {
            get
            {
                return addTime.ToShortDateString();
                //return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 代理等级
        /// </summary>
        [SqlField]
        public int AgentLevel { get; set; }

        /// <summary>
        /// 代理期限 年为单位 默认1年
        /// </summary>
        [SqlField]
        public int AgentTime { get; set; } = 1;
        /// <summary>
        /// 是否过期
        /// </summary>
        public bool IsExpired
        {

            get
            {
                //TODO 上线改为月
                return DateTime.Now > addTime.AddYears(AgentTime);
            }
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public string ExpireTimeStr
        {

            get
            {
                
                return addTime.AddYears(AgentTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }


        /// <summary>
        /// 代理用户信息
        /// </summary>
        public C_UserInfo userInfo { get; set; }

        public string AgentLevelName { get; set; }
        /// <summary>
        /// 上级代理用户信息
        /// </summary>
        public PinAgentDec pinAgentDec { get; set; }

        /// <summary>
        /// 入驻总的花费,包含入驻,续费,升级
        /// </summary>
        public string TotalAgentMoney { get; set; } = "0.00";


      

        /// <summary>
        /// 直接代理发展关系
        /// </summary>
        public AgentDistributionDetail First { get; set; }
       
        /// <summary>
        /// 间接代理发展关系
        /// </summary>
        public AgentDistributionDetail Second { get; set; }

        /// <summary>
        /// 当前等级续费时间与金钱关系集合
        /// </summary>
        public List<object> AgentLevelTimeList { get; set; } = new List<object>();
    }

    /// <summary>
    /// 发展的客户关系
    /// </summary>
    public class AgentDistributionDetail
    {
        /// <summary>
        /// 代理分佣
        /// </summary>
        public string AgentSumMoney { get; set; }
        /// <summary>
        /// 邀请代理数量
        /// </summary>
        public int AgentCount { get; set; }

        /// <summary>
        /// 发展商户订单交易分润
        /// </summary>
        public string OrderSum { get; set; }
        /// <summary>
        /// 邀请商户数量
        /// </summary>
        public int StoreCount { get; set; }
    }


    public class PinAgentDec
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public int AgentLevel { get; set; }

        public string AgentLevelName { get; set; }
        public string Phone { get; set; }
    }


}
