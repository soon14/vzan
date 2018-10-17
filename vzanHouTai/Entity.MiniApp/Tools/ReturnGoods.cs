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
    public class ReturnGoods
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 退货类型（枚举：ReturnGoodsType）
        /// </summary>
        [SqlField]
        public int ReturnType { get; set; }
        /// <summary>
        /// 退货状态（枚举：ReutrnGoodsState）
        /// </summary>
        [SqlField]
        public int ReturnState { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }
        /// <summary>
        /// 退货商品（购物车ID）
        /// </summary>
        [SqlField]
        public string GoodsId { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        [SqlField]
        public int ReturnAmount { get; set; }
        /// <summary>
        /// 退货原因
        /// </summary>
        [SqlField]
        public string Reason { get; set; }
        /// <summary>
        /// 退货备注
        /// </summary>
        [SqlField]
        public string Remark { get; set; }
        /// <summary>
        /// 申请退货图片凭证
        /// </summary>
        [SqlField]
        public string UploadPics { get; set; }
        /// <summary>
        /// 拒绝退货理由
        /// </summary>
        [SqlField]
        public string RejectReason { get; set; }
    }

    public class ReturnGoodsPost
    {
        /// <summary>
        /// 申请日期
        /// </summary>
        public string ApplyTime { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// 退货商品
        /// </summary>
        public string GoodsId { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public int ReturnAmount { get; set; }
        /// <summary>
        /// 退货原因
        /// </summary>
        public string Reason { get; set; }
        /// <summary>
        /// 退货备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 申请退货图片凭证
        /// </summary>
        public string UploadPics { get; set; }
        /// <summary>
        /// 退货类型（枚举：ReturnGoodsType）
        /// </summary>
        public int ReturnType { get; set; }
    }
}
