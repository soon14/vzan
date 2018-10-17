using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 物流跟踪信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DeliveryFeedback
    {
        [SqlField(IsPrimaryKey = true, IsAutoId =true)]
        public int Id { get; set; }
        [SqlField]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }
        /// <summary>
        /// 订单类别
        /// </summary>
        [SqlField]
        public int OrderType { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        [SqlField]
        public string ContactName { get; set; }
        /// <summary>
        /// 收货人电话
        /// </summary>
        [SqlField]
        public string ContactTel { get; set; }
        /// <summary>
        /// 物流公司查询Code
        /// </summary>
        [SqlField]
        public string CompanyCode { get; set; }
        /// <summary>
        /// 物流公司名称
        /// </summary>
        [SqlField]
        public string CompanyTitle { get; set; }
        /// <summary>
        /// 物流订单号
        /// </summary>
        [SqlField]
        public string DeliveryNo { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 是否跟踪物流
        /// </summary>
        [SqlField]
        public bool IsTrack { get; set; }
        /// <summary>
        /// 跟踪信息（JSON String）
        /// </summary>
        [SqlField]
        public string FeedBack { get; set; } = string.Empty;
        /// <summary>
        /// 物流备注
        /// </summary>
        [SqlField]
        public string Mark { get; set; }
        /// <summary>
        /// 物流查询状态：枚举 DeliveryFeedState
        /// </summary>
        [SqlField]
        public int Status { get; set; }
        /// <summary>
        /// 物流接口错误原因
        /// </summary>
        [SqlField]
        public string Reason { get; set; } = string.Empty;
    }

    public class DeliveryUpdatePost
    {
        public int OrderId { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        [SqlField]
        public string ContactName { get; set; }
        /// <summary>
        /// 收货人电话
        /// </summary>
        [SqlField]
        public string ContactTel { get; set; }
        /// <summary>
        /// 物流公司查询Code
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string CompanyTitle { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 物流订单号
        /// </summary>
        public string DeliveryNo { get; set; }
        /// <summary>
        /// 商家配送
        /// </summary>
        public bool SelfDelivery { get; set; }
        /// <summary>
        /// 物流备注
        /// </summary>
        public string Remark { get; set; }
    }
}
