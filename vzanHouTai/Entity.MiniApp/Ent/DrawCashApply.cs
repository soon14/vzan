using Entity.Base;
using Entity.MiniApp.Pin;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 提现记录申请表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class DrawCashApply
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; } = 0;
        /// <summary>
        /// 小程序appId 字符串
        /// </summary>
        [SqlField]
        public string appId { get; set; } =string.Empty;

        /// <summary>
        /// 企业支付需要的订单号 唯一  当企业支付失败 返回系统繁忙，请稍后再试。需要重新用该单号否则可能造成重复支付等资金风险
        /// </summary>
        [SqlField]
        public string partner_trade_no { get; set; } = "";
        /// <summary>
        /// 申请提现用户Id
        /// </summary>
        [SqlField]
        public long userId { get; set; } = 0;
        /// <summary>
        /// 提现前金额
        /// </summary>
        [SqlField]
        public int BeforeApplyMoney { get; set; } = 0;
        public string BeforeApplyMoneyStr
        {
            get
            {
                return (BeforeApplyMoney * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 申请提现金额
        /// </summary>
        [SqlField]
        public int applyMoney { get; set; } = 0;
        public string applyMoneyStr
        {
            get
            {
                return (applyMoney * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 实际提现金额 单位为分
        /// </summary>
        [SqlField]
        public int cashMoney { get; set; } = 0;
        /// <summary>
        /// 提现金额 单位为元
        /// </summary>
        public string cashMoneyStr
        {
            get
            {
                return (cashMoney * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 手续费
        /// </summary>
        [SqlField]
        public int serviceMoney { get; set; }
        public string serviceMoneyStr
        {
            get
            {
                return (serviceMoney * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 申请时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        public string AddTimeStr
        {
            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        public string UpdateTimeStr
        {
            get
            {
                return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss")== "0001-01-01 00:00:00"?"": UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 提现时间
        /// </summary>
        [SqlField]
        public DateTime DrawTime { get; set; }

        public string DrawTimeStr
        {
            get
            {
                return DrawTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 审核状态 -1审核不通过，0待审核，1审核通过 枚举MiniAppEnum.ApplyState
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 审核状态Str
        /// </summary>
        public string stateStr
        {

            get
            {
                if (state == -1)
                {
                    return "审核不通过";
                }
                else if (state == 1)
                {
                    return "审核通过";
                }
                else
                {

                    return "待审核";

                }
            }
        }
        /// <summary>
        /// 提现状态 -1 提现失败,0未开始提现(没有加入提现退列表),1提现中,2提现成功 枚举MiniAppEnum.DrawCashState
        /// </summary>
        [SqlField]
        public int drawState { get; set; } = 0;
        public string drawStateStr {

            get
            {
                return Enum.GetName(typeof(DrawCashState), drawState);
                //if (drawState == -1)
                //{
                //    return "提现失败";
                //}else if (drawState == 2)
                //{
                //    return "提现成功";
                //}else if (drawState==1)
                //{
                //    return "提现中";
                //}
                //else
                //{
                //    return "未开始提现";
                //}
            }
        }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 操作IP
        /// </summary>
        [SqlField]
        public string hostIP { get; set; } = string.Empty;
        /// <summary>
        /// 申请提现类别 0默认分销收益提现 1：普通提现 2:平台店铺提现（DrawCashApplyType） ，3拼享惠提现
        /// </summary>
        [SqlField]
        public int applyType { get; set; } = 0;
        /// <summary>
        /// 剩余提现金额
        /// </summary>
        [SqlField]
        public string useCash { get; set; } = "";
        /// <summary>
        /// 推广订单总数
        /// </summary>
        [SqlField]
        public int orderCount { get; set; } = 0;
        /// <summary>
        /// 推广订单总额
        /// </summary>
        [SqlField]
        public string orderTotalMoney { get; set; } = "";
        /// <summary>
        /// 推广订单佣金总额
        /// </summary>
        [SqlField]
        public string orderTotalCpsMoney { get; set; } = "";
        /// <summary>
        /// 分销员昵称
        /// </summary>
        public string nickName { get; set; } = "";
        /// <summary>
        /// 分销员手机号码
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// 提现去向账户
        /// </summary>
        [SqlField]
        public string account { get; set; } = string.Empty;
        /// <summary>
        /// 提现去向账户名称
        /// </summary>
        [SqlField]
        public string accountName { get; set; } = string.Empty;
        /// <summary>
        /// 提现方式 MiniAppEnum.DrawCashWay
        /// </summary>
        [SqlField]
        public int drawCashWay { get; set; } = 0;
        public string DrawCashWayStr { get { return Enum.GetName(typeof(DrawCashWay), drawCashWay); } }

        public PinStore pinStore { get; set; }

        [SqlField]
        public int OrderId { get; set; }
    }
}
