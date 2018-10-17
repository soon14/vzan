using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 分销员列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class SalesMan
    {
        /// <summary>
        /// 分销员列表
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        /// 用户ID 获取用户信息等以及也是充当用户标识用
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 用户手机号码
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 可提现金额 每次提现扣除后剩余可提现的金额
        /// </summary>
        [SqlField]
        public int useCash { get; set; } = 0;


        /// <summary>
        /// 可提现金额 每次提现扣除后剩余可提现的金额
        /// </summary>
        public string useCashStr
        {
            get
            {
                return (useCash * 0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 总提现金额 累计收益
        /// </summary>
        [SqlField]
        public int useCashTotal { get; set; } = 0;


        /// <summary>
        /// 累计收益=总体现金额(已经提现的)
        /// </summary>
        public string totalIncome {
            get
            {
                return (useCashTotal * 0.01).ToString("0.00");
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
                return UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 状态 -1被清退删除了，0待审核，1审核不通过，2审核通过
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;


        /// <summary>
        /// 上级分销员Id
        /// </summary>
        [SqlField]
        public int ParentSalesmanId { get; set; } = 0;

        /// <summary>
        /// 所属上级分销员昵称
        /// </summary>
        public string ParentSalesmanNickName { get; set; } = string.Empty;
        /// <summary>
        /// 所属上级分销员电话
        /// </summary>
        public string ParentSalesmanPhone { get; set; } = string.Empty;
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickName { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        public string headerImg { get; set; } = string.Empty;

        /// <summary>
        /// 当前店铺名称
        /// </summary>
        public string storeName { get; set; }

        /// <summary>
        /// 订单数量 该申请用户的购买数量
        /// </summary>
        public int orderCount{ get; set; } = 0;

        /// <summary>
        /// 订单付款成功的总金额 
        /// </summary>
        public string orderPrice { get; set; } =string.Empty;

        /// <summary>
        /// 该分销员 推广订单
        /// </summary>
        public int salesManOrderCount { get; set; } = 0;

        /// <summary>
        /// 该分销员 累计客户
        /// </summary>
        public int customerNumber { get; set; } = 0;
        /// <summary>
        /// 自动结算订单数量
        /// </summary>
        public int autoPayOrderCount { get; set; } = 0;

        /// <summary>
        /// 自动结算订单总金额
        /// </summary>
        public string autoPayOrderTotalPrice { get; set; }

        /// <summary>
        /// 自动结算订单总佣金金额
        /// </summary>
        public string autoPayOrderTotalCpsMoney { get; set; }

        /// <summary>
        /// 人工结算订单数量
        /// </summary>
        public int peoplePayOrderCount { get; set; } = 0;

        /// <summary>
        /// 人工结算订单总金额
        /// </summary>
        public string peoplePayOrderTotalPrice { get; set; }

        /// <summary>
        /// 人工结算订单总佣金金额
        /// </summary>
        public string peoplePayOrderTotalCpsMoney { get; set; }


    }
}
