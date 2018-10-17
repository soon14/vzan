using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 物流接口返回结果（基础模型）
    /// </summary>
    public class DeliveryBase
    {
        /// <summary>
        /// 订阅商户号
        /// </summary>
        public string EBusinessID { get; set; }
        /// <summary>
        /// 查询是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 失败原因
        /// </summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// 物流订阅接收推送
    /// </summary>
    public class DeliverySubscribePush
    {
        /// <summary>
        /// 推送时间
        /// </summary>
        public string PushTime { get; set; }
        /// <summary>
        /// 订阅商户号
        /// </summary>
        public string EBusinessID { get; set; }
        /// <summary>
        /// 推送订单数
        /// </summary>
        public string Count { get; set; }
        /// <summary>
        /// 推送数据
        /// </summary>
        public List<DeliveryData> Data { get; set; }
    }

    /// <summary>
    /// 物流新增订阅结果
    /// </summary>
    public class DeliverySubscribeResult: DeliveryBase
    {
        public string UpdateTime { get; set; }
    }

    /// <summary>
    /// 物流接口返回数据
    /// </summary>
    public class DeliveryData : DeliveryBase
    {
        /// <summary>
        /// 物流订单号
        /// </summary>
        public string LogisticCode { get; set; }
        /// <summary>
        /// 物流公司代码
        /// </summary>
        public string ShipperCode { get; set; }
        /// <summary>
        /// 物流轨迹
        /// </summary>
        public List<DeliveryTrace> Traces { get; set; }
        /// <summary>
        /// 物流状态 : 0-无轨迹, 1-已揽收, 2-在途中, 3-签收, 4-问题件
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 用户自定义回传字段
        /// </summary>
        public string CallBack { get; set; }
        /// <summary>
        /// 订单预计到货时间
        /// </summary>
        public string EstimatedDeliveryTime { get; set; }
    }

    /// <summary>
    /// 物流轨迹
    /// </summary>
    public class DeliveryTrace
    {
        /// <summary>
        /// 轨迹对象
        /// </summary>
        public string AcceptStation { get; set; }
        /// <summary>
        /// 轨迹时间
        /// </summary>
        public string AcceptTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
