using Entity.Base;
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
    /// 客户投诉
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PinComplaint
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;
        /// <summary>
        /// 订单id
        /// </summary>
        [SqlField]
        public int orderId { get; set; } = 0;
        /// <summary>
        /// 订单流水
        /// </summary>
        [SqlField]
        public string orderOutTradeNo { get; set; } = string.Empty;
        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 投诉者id
        /// </summary>
        [SqlField]
        public int userId { get; set; } = 0;
        /// <summary>
        /// 被投诉店铺id
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;
        /// <summary>
        /// 处理状态 0:未处理  1：协调中  2：已处理 PinEnum.ComplaintState
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        public string stateStr
        {
            get
            {
                return Enum.GetName(typeof(ComplaintState), state);
            }
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;
        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 协调记录
        /// </summary>
        [SqlField]
        public string record { get; set; } = string.Empty;
        /// <summary>
        /// 处理结果
        /// </summary>
        [SqlField]
        public string result { get; set; } = string.Empty;
        /// <summary>
        /// 图片说明
        /// </summary>
        [SqlField]
        public string pictures { get; set; } = string.Empty;
        public List<string> picList
        {
            get
            {
                return string.IsNullOrEmpty(pictures) ? new List<string>() : pictures.Split(';').ToList();
            }
        }
        /// <summary>
        /// 文字说明
        /// </summary>
        [SqlField]
        public string content { get; set; } = string.Empty;
        /// <summary>
        /// 投诉问题
        /// </summary>
        [SqlField]
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string remark { get; set; }
        /// <summary>
        /// 最后处理时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; } = DateTime.Now;
        /// <summary>
        /// 联系电话
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;
        /// <summary>
        /// 被投诉店铺信息
        /// </summary>
        public PinStore store { get; set; } = new PinStore();
        /// <summary>
        /// 投诉人信息
        /// </summary>
        public C_UserInfo userInfo { get; set; } = new C_UserInfo();
        /// <summary>
        /// 投诉订单信息
        /// </summary>
        public PinGoodsOrder order { get; set; } = new PinGoodsOrder();
    }
}
