using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 推广分享 产生的订单
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SalesManRecordOrder
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        /// 推广记录Id
        /// </summary>
        [SqlField]
        public int salesManRecordId { get; set; } = 0;


      
        /// <summary>
        /// 订单Id
        /// </summary>
        [SqlField]
        public int orderId { get; set; } = 0;


        /// <summary>
        /// 购物车Id
        /// </summary>
        [SqlField]
        public int CarId { get; set; } = 0;

        /// <summary>
        /// 对外订单号
        /// </summary>
        [SqlField]
        public string orderNumber { get; set; } = string.Empty;

        /// <summary>
        /// 订单成交金额 分为单位
        /// </summary>
        [SqlField]
        public int orderMoney { get; set; }

        public string orderMoneyStr
        {

            get
            {
                return (orderMoney * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 产品佣金比例 分享推广时候的 随时可能变,这里记录的是分享推广时候的
        /// </summary>
        [SqlField]
        public double cps_rate { get; set; } = 0.00;


        /// <summary>
        /// 订单佣金 分为单位
        /// </summary>
        [SqlField]
        public int cpsMoney { get; set; }
        public string cpsMoneyStr
        {
            get
            {
                return (cpsMoney * 0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 推广订单结算 状态 
        /// 0表示人工已结算 1表示自动已结算
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }
        /// <summary>
        /// 新增时间str
        /// </summary>
       
        public string addTimeStr {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 分销员手机号码
        /// </summary>
        public string TelePhone { get; set; } = string.Empty;


        /// <summary>
        ///分销员昵称
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 备注信息
        /// </summary>
        [SqlField]
        public string Remark { get; set; }


     

        /// <summary>
        /// 结算状态 0表示已结算 -1表示未结算
        /// </summary>
        [SqlField]
        public int PayState { get; set; }
























































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































        public List<OrderGoodsDetail> listOrderGoodsDetail { get; set; } = new List<OrderGoodsDetail>();


       


    }


    public class OrderGoodsDetail
    {
        public string goodImgUrl { get; set; }
        public string goodname { get; set; }

        public double price { get; set; }

        public int Count { get; set; }

        public int cpsMoney { get; set; }

        public string cpsMoneyStr { get { return (cpsMoney * 0.01).ToString(); } }

        public string cps_rate { get; set; }

    }

    /// <summary>
    /// 业绩统计数据实体类
    /// </summary>
    public class OrderSum
    {
        /// <summary>
        /// 订单数量
        /// </summary>
        public int payOrderCount { get; set; } = 0;

        /// <summary>
        /// 订单总金额
        /// </summary>
        public string payOrderTotalPrice { get; set; } = "0.00";

        /// <summary>
        /// 订单总佣金金额
        /// </summary>
        public string payOrderTotalCpsMoney { get; set; } = "0.00"; 

    }



    /// <summary>
    /// 关系查询展示模型
    /// </summary>
    public class RelationViewModel
    {
        /// <summary>
        /// 买家名称
        /// </summary>
        public string orderUserName { get; set; }

        /// <summary>
        /// 分销员手机号码
        /// </summary>
        public string saleManTelephone { get; set; }

        /// <summary>
        /// 关系建立连接表示 其实是salesmanrecorduserId
        /// </summary>
        public int relationFlag { get; set; }

        public string relationConnectTime { get; set; }

        public string relationEndTime { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public string state { get; set; }= "无关联";

    }


}
