using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    //public class EleApiModel
    //{
    //    public string code { get; set; }
    //    public string msg { get; set; }
    //    public object data { get; set; }
    //}

    public class FNApiReponseModel<T>
    {
        public int code { get; set; }
        /// <summary>
        /// 响应返回吗，参考接口返回码
        /// </summary>
        public string app_id { get; set; }
        /// <summary>
        /// 错误编码，与code一致
        /// </summary>
        public T data { get; set; }
        /// <summary>
        /// 响应描述
        /// </summary>
        public int salt { get; set; }
        /// <summary>
        /// 响应状态，成功为"success"，失败为"fail"
        /// </summary>
        public string signature { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { get; set; }
    }

    /// <summary>
    /// 蜂鸟配送状态返回JSON数据
    /// </summary>
    public class FNAccepterOrderModel
    {
        /// <summary>
        /// 商户自己的订单号
        /// </summary>
        public string partner_order_code { get; set; }
        /// <summary>
        ///状态码 1：系统已接单，2：配送中，3：已送达，5：系统拒单/配送异常，20：已分配骑手，80：骑手已到店
        /// </summary>
        public int order_status { get; set; }
        /// <summary>
        /// 状态推送时间(毫秒)
        /// </summary>
        public long push_time { get; set; }
        /// <summary>
        /// 蜂鸟配送员姓名
        /// </summary>
        public string carrier_driver_name { get; set; }
        /// <summary>
        /// 蜂鸟配送员电话
        /// </summary>
        public string carrier_driver_phone { get; set; }
        /// <summary>
        /// 描述信息
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 错误编码
        /// </summary>
        public string error_code { get; set; }
        /// <summary>
        /// 定点次日达服务独有的字段: 微仓地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 定点次日达服务独有的字段: 微仓纬度
        /// </summary>
        public long latitude { get; set; }
        /// <summary>
        /// 定点次日达服务独有的字段: 微仓经度
        /// </summary>
        public long longitude { get; set; }
        /// <summary>
        /// 订单取消原因. 1:用户取消, 2:商家取消
        /// </summary>
        public string cancel_reason { get; set; }
    }

    /// <summary>
    /// 订单取消请求表单
    /// </summary>
    public class FNCancelOrderRequestModel
    {
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string partner_order_code { get; set; }
        /// <summary>
        /// 订单取消原因代码(2:商家取消)
        /// </summary>
        public int order_cancel_reason_code { get; set; } = 2;
        /// <summary>
        /// 订单取消编码（0:其他, 1:联系不上商户, 2:商品已经售完, 3:用户申请取消, 4:运力告知不配送 让取消订单, 5:订单长时间未分配, 6:接单后骑手未取件）
        /// </summary>
        public int order_cancel_code { get; set; }
        /// <summary>
        /// 订单取消描述（order_cancel_code为0时必填）
        /// </summary>
        public string order_cancel_description { get; set; }
        /// <summary>
        /// 订单取消时间（毫秒）
        /// </summary>
        public long order_cancel_time { get; set; }
    }
}
