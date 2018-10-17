using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_BargainUser
    {
        public string Qrcode_Logo { get; set; }
        public string Qrcode_Url { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 砍价商品id 商品id
        /// </summary>
        [SqlField]
        public int BId { get; set; }
        /// <summary>
        /// 名称标题
        /// </summary>
        [SqlField]
        public string BName { get; set; }
        /// <summary>
        /// 领取人id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }
        /// <summary>
        /// 当前价格
        /// </summary>
        [SqlField]
        public int CurrentPrice { get; set; }
        /// <summary>
        /// 帮砍人数
        /// </summary>
        [SqlField]
        public int HelpNum { get; set; }
        /// <summary>
        /// 购买时间
        /// </summary>
        [SqlField]
        public DateTime BuyTime { get; set; } = Convert.ToDateTime("3000-01-01 01:01:01");
        /// <summary>
        /// 订单的ID，用于退款使用
        /// </summary>
        [SqlField]
        public int OrderId { get; set; } = 0;
        /// <summary>
        /// 核销GUID  
        /// </summary>
        [SqlField]
        public string BargainGuid { get; set; } = string.Empty;
        /// <summary>
        /// 核销时间
        /// </summary>
        [SqlField]
        public DateTime ValidTime { get; set; }
        /// <summary>
        /// 核销人 OpenId
        /// </summary>
        [SqlField]
        public string ValidUserOpenId { get; set; }
        /// <summary>
        /// 核销人昵称
        /// </summary>
        [SqlField]
        public string ValidUserNickName { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        [SqlField]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        [SqlField]
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        /// <summary>
        /// 0未付款，1已购买,2已核销 
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 手动核销劵号
        /// </summary>
        [SqlField]
        public int ValidNumber { get; set; }

        /// <summary>
        /// 核销的二维码
        /// </summary>
        [SqlField]
        public string QrCodeUrl { get; set; }

        /// <summary>
        /// 是否到账
        /// </summary>
        [SqlField]
        public int PayState { get; set; }

        /// <summary>
        /// 联系人姓名
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [SqlField]
        public string TelePhone { get; set; }
        /// <summary>
        /// 是否已发送消息提醒
        /// </summary>
        [SqlField]
        public int SendInfo { get; set; }
        /// <summary>
        /// 是否已发送消息提醒 过期使用剩余24小时提醒
        /// </summary>
        [SqlField]
        public int SendInfo2 { get; set; }

        //View 显示数据
        public int OriginalPrice { get; set; }
        public int FloorPrice { get; set; }
        public int ReduceMax { get; set; }
        public int ReduceMin { get; set; }
        public int IntervalHour { get; set; }
        public string ShopLogoUrl { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; }
        /// <summary>
        /// 商家
        /// </summary>
        //public C_Store Store { get; set; }

        public bool IsConcern { get; set; } = false;
        public List<C_Attachment> DescImgList { get; set; }
        /// <summary>
        /// 使用者昵称
        /// </summary>
        public string NickName { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public string ValidShowTime { get; set; }
        public string ValidShowDate { get; set; }
        //是否过期
        public int IsExpire { get; set; }

        public int usercount { get; set; }
        public C_UserInfo LoginUser { get; set; }
        //public C_Bargain Bargain { get; set; }
        /// <summary>
        /// 红包
        /// </summary>
        /// 是否有红包
        public int IsRed { get; set; } = 0;
        /// 当前剩余余额 (单位：分）
        public int Amount_current { get; set; } = 0;
        /// 红包总个数
        public int TotalCount { get; set; } = 0;
        //当前已抢红包人数
        public int Count_current { get; set; } = 0;

    }
}
