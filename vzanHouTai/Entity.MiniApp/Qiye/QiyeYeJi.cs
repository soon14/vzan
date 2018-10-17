using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Qiye
{
    /// <summary>
    /// 员工业绩统计
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyeYeJi
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId 
        /// </summary>
        public int Aid { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
        /// <summary>
        /// 部门ID
        /// </summary>
        public int DepartmentId { get; set; } = 0;

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// 名片浏览数
        /// </summary>
        public Int64 CardViewedCount { get; set; } = 0;

        /// <summary>
        /// 名片转发数
        /// </summary>
        public Int64 CardRepostCount { get; set; } = 0;

        /// <summary>
        /// 客户数
        /// </summary>
        public Int64 CustomerCount { get; set; } = 0;

        /// <summary>
        /// 客户点赞数
        /// </summary>
        public Int64 CustomerLikeCount { get; set; } = 0;

        /// <summary>
        /// 客户咨询数
        /// </summary>
        public Int64 CustomerConsultCount { get; set; } = 0;


        /// <summary>
        /// 官网引流量（从名片进入并点击到其他页面的所有访问次数）
        /// </summary>
        public Int64 PV { get; set; } = 0;

        /// <summary>
        /// 订单数量
        /// </summary>
        public Int64 OrderCount { get; set; } = 0;

        /// <summary>
        /// 销售额 单位分
        /// </summary>
        public decimal Sales { get; set; } = 0;
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; } = DateTime.Now;

    }
}
