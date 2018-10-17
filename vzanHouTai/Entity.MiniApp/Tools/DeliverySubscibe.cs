using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Tools
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DeliverySubscribe
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 推送时间
        /// </summary>
        [SqlField]
        public DateTime PushTime { get; set; }
        /// <summary>
        /// 推送ID（区分推送批次）
        /// </summary>
        [SqlField]
        public Guid SubscribeId { get; set; }
        /// <summary>
        /// 物流公司Code
        /// </summary>
        [SqlField]
        public string ShipperCode { get; set; }
        /// <summary>
        /// 物流订单号
        /// </summary>
        [SqlField]
        public string LogisticCode { get; set; }
        /// <summary>
        /// 跟踪轨迹
        /// </summary>
        [SqlField]
        public string Traces { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 订阅商户号
        /// </summary>
        [SqlField]
        public string EBusinessID { get; set; }
        /// <summary>
        /// 订阅查询结果
        /// </summary>
        [SqlField]
        public bool Success { get; set; }
        /// <summary>
        /// 失败原因
        /// </summary>
        [SqlField]
        public string Reason { get; set; }
        /// <summary>
        /// 用户自定义回传字段
        /// </summary>
        [SqlField]
        public string CallBack { get; set; }
        /// <summary>
        /// 预计送达时间
        /// </summary>
        [SqlField]
        public DateTime EstimatedDeliveryTime { get; set; }
        /// <summary>
        /// 是否已同步到本地订单：-1 = 失败，0 = 未同步，1 = 成功
        /// </summary>
        [SqlField]
        public int Sync { get; set; }
        /// <summary>
        /// 同步本地订单结果
        /// </summary>
        [SqlField]
        public string SyncResult { get; set; }
    }
}
