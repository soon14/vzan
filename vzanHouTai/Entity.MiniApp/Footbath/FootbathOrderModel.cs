using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.Footbath
{
    public class FootbathOrderModel
    {
        /// <summary>
        /// 小程序id
        /// </summary>
        public int aid { get; set; } = 0;
        /// <summary>
        /// 店铺id
        /// </summary>
        public int storeId { get; set; } = 0;
        /// <summary>
        /// 订单id
        /// </summary>

        public int orderId { get; set; } = 0;
        /// <summary>
        /// 客户名称
        /// </summary>
        public string customerName { get; set; } = string.Empty;
        /// <summary>
        /// 客户联系电话
        /// </summary>

        public string phone { get; set; } = string.Empty;
        /// <summary>
        /// 客户性别
        /// </summary>

        public int sex { get; set; } = 0;
        /// <summary>
        /// 服务项目id
        /// </summary>

        public int serviceId { get; set; } = 0;
        /// <summary>
        /// 服务项目名称
        /// </summary>
        public string serviceName { get; set; } = string.Empty;

        /// <summary>
        /// 技师id
        /// </summary>
        public int technicianId { get; set; } = 0;

        /// <summary>
        /// 技师名称
        /// </summary>
        public string technicianName { get; set; } = string.Empty;

        /// <summary>
        /// 购物车额外属性
        /// </summary>
        public string extraConfig { get; set; } = string.Empty;

        /// <summary>
        /// 购物车额外属性实体
        /// </summary>
        public ExtraConfig ExtraModel { get; set; }

        /// <summary>
        /// 包间名称
        /// </summary>
        public string roomName { get; set; } = string.Empty;

        /// <summary>
        /// 服务时间
        /// </summary>
        public DateTime serviceTime { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 费用
        /// </summary>
        public int Cost { get; set; } = 0;

        /// <summary>
        /// 支付方式
        /// </summary>
        public int payWay { get; set; }

        /// <summary>
        /// 支付方式名称
        /// </summary>
        public string payWayName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        public int state { get; set; }

        /// <summary>
        /// 状态名称
        /// </summary>
        public string stateName { get; set; }
    }

}