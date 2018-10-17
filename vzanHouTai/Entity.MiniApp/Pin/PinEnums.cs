using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Pin
{
    public class PinEnums
    {
        /// <summary>
        /// 订单状态
        /// </summary>
        public enum PinOrderState
        {
            申请售后=-5,
            已退款=-4,
            已删除 = -3,
            交易失败 = -2,
            交易取消 = -1,
            待支付 = 0,
            待发货 = 1,
            待收货 = 2,
            待自取 = 3,
            交易成功 = 4,//此状态相当于待评价
            已评价 = 5,
        }

        /// <summary>
        /// 支付方式
        /// </summary>
        public enum PayWay
        {
            微信支付 = 0,
            余额支付 = 1,
            线下支付 = 2,
        }

        /// <summary>
        /// 配送方式
        /// </summary>
        public enum SendWay
        {
            商家配送 = 0,
            到店自取 = 1,
            面对面交易 = 2
        }

        /// <summary>
        /// 拼团状态
        /// </summary>
        public enum GroupState
        {
            返利失败 = -2,
            拼团失败 = -1,
            未成团 = 0,
            已成团 = 1,
            拼团成功 = 2,
            已返利 = 3,
        }
        /// <summary>
        /// 付款状态
        /// </summary>
        public enum PayState
        {
            未付款 = 0,
            已付款 = 1,
            已退款 = 2,
        }
        /// <summary>
        /// 商品状态
        /// </summary>
        public enum GoodsState
        {

            上架 = 1,
            下架 = 0,
            删除 = -1
        }

        /// <summary>
        /// 商品审核状态
        /// </summary>
        public enum GoodsAuditState
        {
            审核通过 = 1,
            待审核 = 0,
            已拒绝 = -1,

        }

        /// <summary>
        /// 标杆状态
        /// </summary>
        public enum BiaoGanState
        {
            申请失败 = -1,
            未申请 = 0,
            申请中 = 1,
            申请成功 = 2,
        }
        /// <summary>
        /// 投诉处理状态
        /// </summary>
        public enum ComplaintState
        {
            未处理 = 0,
            协调中 = 1,
            已处理 = 2
        }
        public enum AgentState
        {
            未可用 = 0,
            正常 = 1,
            申请中 = 2
        }
        public enum RefundApplyState
        {
            删除=-2,
            申请失败=-1,
            待处理=0,
            申请成功=1,
            已重新发货=2,
            已退款=3
        }
    }
}