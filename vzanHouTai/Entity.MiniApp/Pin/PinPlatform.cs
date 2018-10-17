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
    /// 平台设置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinPlatform
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 开启客服
        /// </summary>
        [SqlField]
        public int openKf { get; set; } = 0;
        /// <summary>
        /// 客服电话
        /// </summary>
        [SqlField]
        public string kfPhone { get; set; } = string.Empty;

        /// <summary>
        /// 客服微信号，供用户复制添加好友
        /// </summary>
        [SqlField]
        public string ServiceWeiXin { get; set; } = string.Empty;

        /// <summary>
        /// 免费体验天数
        /// </summary>
        [SqlField]
        public int freeDays { get; set; } = 1;

        /// <summary>
        /// 最低提现金额
        /// </summary>
        [SqlField]
        public int minTxMoney { get; set; } = 0;
        public string minTxMoneyStr
        {
            get
            {
                return (minTxMoney * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 处理期限
        /// </summary>
        [SqlField]
        public int dealDays { get; set; } = 5; 

        /// <summary>
        /// 平台交易提现手续费:百分比 10=1%
        /// </summary>
        [SqlField]
        public int serviceFee { get; set; } = 10;
        /// <summary>
        /// 店内扫码提现手续费:百分比 10=1%
        /// </summary>
        [SqlField]
        public int qrcodeServiceFee { get; set; } = 10;
        /// <summary>
        /// 代理收益提现手续费:百分比 10=1%
        /// </summary>
        [SqlField]
        public int agentServiceFee { get; set; } = 10;
        /// <summary>
        /// 提现到微信
        /// </summary>
        [SqlField]
        public int toWx { get; set; } = 1;
        /// <summary>
        /// 提现到银行卡
        /// </summary>
        [SqlField]
        public int toBank { get; set; } = 1;

        /// <summary>
        /// 代理费
        /// </summary>
        [SqlField]
        public int agentFee { get; set; } = 0;
        public string agentFeeStr
        {
            get
            {
                return (agentFee * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 代理提成 百分比 10=1%
        /// </summary>
        [SqlField]
        public int agentExtract { get; set; } = 400;
        /// <summary>
        /// 订单提成 百分比 10=1%
        /// </summary>
        [SqlField]
        public int orderExtract { get; set; } = 30;

        /// <summary>
        /// 直接分佣比例  10=1%
        /// 假设代理费1000 这里有二级关系，则 一级拿 1000*agentExtract*FirstExtract
        /// </summary>
        [SqlField]
        public int FirstExtract { get; set; } = 700;


        /// <summary>
        /// 间接分佣比例  10=1%
        /// </summary>
        [SqlField]
        public int SecondExtract { get; set; } = 300;


        /// <summary>
        /// 代理查询入口 开关 0→关闭 1→开启
        /// </summary>
        [SqlField]
        public int AgentSearchPort { get; set; } = 0;


        /// <summary>
        /// 入驻的入口 开关 0→关闭 1→开启
        /// </summary>
        [SqlField]
        public int AddStorePort { get; set; } = 0;


        [SqlField]
        public int agentProtectDays { get; set; } = 7;
         /// <summary>
         /// 自动完成交易时间
         /// </summary>
        [SqlField]
        public int orderSuccessDays { get; set; } = 7;
        /// <summary>
        /// 入驻宣传海报（富文本）
        /// </summary>
        [SqlField]
        public string poster { get; set; } = string.Empty;
        [SqlField]
        public string posterbk { get; set; } = string.Empty;


        /// <summary>
        /// 代理政策（富文本）
        /// </summary>
        [SqlField]
        public string AgentPolicy { get; set; } = string.Empty;


        /// <summary>
        /// 越级分佣比例 10%   10=1%
        /// </summary>
        [SqlField]
        public int JumpExtract { get; set; } = 100;
        /// <summary>
        /// 入驻开关 0：关闭 1：开启
        /// </summary>
        [SqlField]
        public int OpenRuzhu { get; set; } = 1;

    }
}