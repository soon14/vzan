using Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using static Entity.MiniApp.Pin.PinEnums;

namespace Entity.MiniApp.Pin
{
    /// <summary>
    /// 售后退款申请
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinRefundApply
    {
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int id { get; set; }
        /// <summary>
        /// 小程序id
        /// </summary>
        [SqlField]
        public int aId { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int storeId { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        [SqlField]
        public int userId { get; set; }
        /// <summary>
        /// 订单id
        /// </summary>
        [SqlField]
        public int orderId { get; set; } = 0;
        /// <summary>
        /// 服务单号(与订单外部单号一致）
        /// </summary>
        [SqlField]
        public string serviceNo { get; set; } = string.Empty;

        /// <summary>
        /// 售后类型 1:仅退款，2：退款退货，3：换货/维修
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        public string typeStr
        {
            get
            {
                return type == 1 ? "仅退款" : type == 2 ? "退款退货" : type == 3 ? "换货/维修" : "未知类型";
            }
        }
        /// <summary>
        /// 退货原因
        /// </summary>
        [SqlField]
        public string reason { get; set; } = string.Empty;
        /// <summary>
        /// 退货说明
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 商品件数
        /// </summary>
        [SqlField]
        public int count { get; set; } = 0;
        /// <summary>
        /// 退款金额
        /// </summary>
        [SqlField]
        public int money { get; set; } = 0;
        /// <summary>
        /// 图片凭证
        /// </summary>
        [SqlField]
        public string imgs { get; set; } = string.Empty;
        public List<string> imgList
        {
            get
            {
                return string.IsNullOrEmpty(imgs) ? new List<string>() : imgs.Split(',').ToList();
            }
        }

        /// <summary>
        /// 状态 0：申请中 ，-1：取消申请， 1：通过申请
        /// </summary>
        [SqlField]
        public int state { get; set; }
        public string stateStr
        {
            get
            {
                return Enum.GetName(typeof(RefundApplyState), state);
            }
        }
        /// <summary>
        /// 提交时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }

        public string updateTimeStr
        {
            get
            {
                return updateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 物流公司
        /// </summary>
        [SqlField]
        public string wuliuName { get; set; } = string.Empty;
        /// <summary>
        /// 物流单号
        /// </summary>
        [SqlField]
        public string wuliuOrder { get; set; } = string.Empty;
        /// <summary>
        /// 申请时订单状态
        /// </summary>
        [SqlField]
        public int orderState { get; set; }
        public PinGoodsOrder order { get; set; } = null;
        public PinStore store { get; set; }

    }
}
