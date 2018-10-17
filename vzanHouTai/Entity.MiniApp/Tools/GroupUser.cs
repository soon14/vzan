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
    public class GroupUser
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 拼团活动id 商品id
        /// </summary>
        [SqlField]
        public int GroupId { get; set; }
        /// <summary>
        /// 团购id 参团id
        /// </summary>
        [SqlField]
        public int GroupSponsorId { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        [SqlField]
        public int BuyNum { get; set; }
        /// <summary>
        /// 购买价格
        /// </summary>
        [SqlField]
        public int BuyPrice { get; set; }

        public string BuyPriceStr
        {
            get
            {
                return (BuyPrice * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 是否团购  1-团购
        /// </summary>
        [SqlField]
        public int IsGroup { get; set; }
        /// <summary>
        /// 是否团长  1-团长
        /// </summary>
        [SqlField]
        public int IsGroupHead { get; set; }
        /// <summary>
        /// 优惠GUID  
        /// </summary>
        [SqlField]
        public string DiscountGuid { get; set; } = string.Empty;
        /// <summary>
        /// 购买的用户
        /// </summary>
        [SqlField]
        public int ObtainUserId { get; set; }
        /// <summary>
        /// 1已发货,0待发货，-1已收货,-2已退款 ,-3退款中 ，-4已过期,待支付 = 4, 取消支付 = 5,
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        public string StateStr
        {
            get
            {
                if (PState == 1 && EndDate > DateTime.Now)
                {
                    return "拼团中";
                }
                else if (PState == 2 || PState == 3)
                {
                    return Enum.GetName(typeof(MiniappPayState), State);
                }
                else
                {
                    return "成团失败";
                }
            }
        }
        /// <summary>
        /// 是否已到账 
        /// PayState
        /// </summary>
        [SqlField]
        public int PayState { get; set; }

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
        /// 订单的ID，用于退款使用
        /// </summary>
        [SqlField]
        public int OrderId { get; set; } = 0;

        /// <summary>
        /// 分享图片
        /// </summary>
        [SqlField]
        public string ShareImg { get; set; }

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
        /// 发货地址
        /// </summary>
        [SqlField]
        public string Address { get; set; }
        /// <summary>
        /// 收货电话
        /// </summary>
        [SqlField]
        public string Phone { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        [SqlField]
        public string UserName { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string CreateDateStr
        {
            get
            {
                return CreateDate.ToString("yyyy-MM-dd hh:mm:ss");
            }
        }
        /// <summary>
        /// 支付时间
        /// </summary>
        [SqlField]
        public DateTime PayTime { get; set; }
        public string PayTimeStr
        {
            get
            {
                return PayTime.ToString("yyyy-MM-dd hh:mm:ss");
            }
        }
        /// <summary>
        /// 发货时间
        /// </summary>
        [SqlField]
        public DateTime SendGoodTime { get; set; }
        public string SendGoodTimeStr
        {
            get
            {
                return SendGoodTime.ToString("yyyy-MM-dd hh:mm:ss");
            }
        }
        /// <summary>
        /// 成交时间
        /// </summary>
        [SqlField]
        public DateTime RecieveGoodTime { get; set; }
        public string RecieveGoodTimeStr
        {
            get
            {
                return RecieveGoodTime.ToString("yyyy-MM-dd hh:mm:ss");
            }
        }
        /// <summary>
        /// 店主留言
        /// </summary>
        [SqlField]
        public string StorerRemark { get; set; }
        /// <summary>
        /// 用户留言
        /// </summary>
        [SqlField]
        public string ShoperRemark { get; set; }
        /// <summary>
        ///支付类型，0微信支付，1储值支付
        /// </summary>
        [SqlField]
        public int PayType { get; set; }

        [SqlField]
        public string HeadImgUrl { get; set; }
        /// <summary>
        /// 内部订单号(也是对外订单号）
        /// </summary>
        [SqlField]
        public string OrderNo { get; set; }
        /// <summary>
        /// 小程序AppID
        /// </summary>
        [SqlField]
        public string AppId { get; set; }
        public int AId { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        [SqlField]
        public string Note { get; set; }


        //View 显示数据
        public string Name { get; set; }
        public int DiscountPrice { get; set; }
        public int UnitPrice { get; set; }
        public int OriginalPrice { get; set; }
        public int GroupSize { get; set; }
        public int HaveNum { get; set; }
        public DateTime StartUseTime { get; set; }
        public string ShopLogoUrl { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public int IsRefund { get; set; }
        /// <summary>
        /// -1:成团失败，1：开团成功，2拼团成功,3：单买商品
        /// </summary>
        public int PState { get; set; }
        public int GState { get; set; }
        public int MState { get; set; }
        public string GroupImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 商家地址
        /// </summary>
        public string StoreAddress { get; set; }
        /// <summary>
        /// 商家电话
        /// </summary>
        public string StoreTel { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// 商家Id
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 使用者昵称
        /// </summary>
        public string NickName { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public string ValidShowTime { get; set; }
        //是否过期
        public int IsExpire { get; set; }
        /// <summary>
        /// 小团开始时间
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 小团结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 团购名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 是否已评论
        /// </summary>
        [SqlField]
        public bool IsCommentting { get; set; } = false;
    }
}
