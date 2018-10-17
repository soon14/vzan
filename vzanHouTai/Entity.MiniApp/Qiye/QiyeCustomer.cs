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
    /// 企业客户信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class QiyeCustomer
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId 
        /// </summary>
        [SqlField]
        public int Aid { get; set; }
        
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; }

        /// <summary>
        /// 员工ID
        /// </summary>
        [SqlField]
        public int StaffId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string Desc { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string PhoneDesc { get; set; }
        /// <summary>
        /// 访问量
        /// </summary>
        [SqlField]
        public int PV { get; set; }

        /// <summary>
        /// 咨询次数
        /// </summary>
        public int AskCount { get; set; }
        /// <summary>
        /// 客户姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 客户电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 客户头像
        /// </summary>
        public string HeadImgUrl { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EmployeeName { get; set; }
        /// <summary>
        /// 订单量
        /// </summary>
        public int OrderCount { get; set; }
        /// <summary>
        /// 订单总金额
        /// </summary>
        public int OrderTotalPrice { get; set; }
        public string OrderTotalPriceStr { get { return (OrderTotalPrice * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 未读私信信息数量
        /// </summary>
        public int NoReadMessageCount { get; set; }
        /// <summary>
        /// 最后一条私信信息
        /// </summary>
        public string LastMsg { get; set; } = "";
        public int MsgType { get; set; }
    }
}
