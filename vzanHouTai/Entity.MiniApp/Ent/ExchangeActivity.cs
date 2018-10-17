using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 积分商城活动
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ExchangeActivity
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;
        /// <summary>
        /// 活动名称
        /// </summary>
        [SqlField]
        public string activityname { get; set; } = string.Empty;

        /// <summary>
        /// 活动图片
        /// </summary>
        public string activityimg { get; set; } = string.Empty;

        /// <summary>
        /// 缩略图750*750
        /// </summary>
        public string activityimg_fmt { get; set; } = string.Empty;

        /// <summary>
        /// 缩略图750*750
        /// </summary>
        public List<string> imgs_fmt { get; set; } = new List<string>();
        public List<string> imgs { get; set; } = new List<string>();
        /// <summary>
        /// 所需积分
        /// </summary>
        [SqlField]
        public int integral { get; set; } = 0;

        /// <summary>
        /// 库存
        /// </summary>
        [SqlField]
        public int stock { get; set; } = 0;

        /// <summary>
        /// 每个人可兑换的礼品数量
        /// </summary>
        [SqlField]
        public int perexgcount { get; set; } = 1;

        /// <summary>
        /// 兑换开始时间
        /// </summary>
        [SqlField]
        public DateTime startdate { get; set; }

        public string startdateStr
        {
            get
            {
                return startdate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 兑换结束时间
        /// </summary>
        [SqlField]
        public DateTime enddate { get; set; }

        public string enddateStr
        {
            get
            {
                return enddate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }
        public string UpdateDateStr
        {
            get
            {
                return UpdateDate.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        [SqlField]
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// 积分兑换方式  0→积分兑换  1→ 微信支付+积分兑换
        /// </summary>
        [SqlField]
        public int exchangeway { get; set; } = 0;

        /// <summary>
        /// 微信支付+积分兑换方式所需的金额
        /// </summary>
        [SqlField]
        public int price { get; set; } = 0;

        public string priceStr
        {
            get
            {

                return (price * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 运费
        /// </summary>
        [SqlField]
        public int freight { get; set; } = 0;
        public string freightStr
        {
            get
            {

                return (freight * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        ///小程序模板类型 22表示专业版 6表示电商版
        /// </summary>
        [SqlField]
        public int apptype { get; set; } = 22;

        /// <summary>
        /// 状态 1:关闭,0:开启
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 是否删除 1:删除,0:正常
        /// </summary>
        [SqlField]
        public int isdel { get; set; } = 0;

        /// <summary>
        /// 排序字段
        /// </summary>
        [SqlField]
        public int SortNumber { get; set; }

        public bool IsShowEditSort = false;

        /// <summary>
        /// 原价
        /// </summary>
        [SqlField]
        public int originalPrice { get; set; } = 0;

        public string originalPriceStr
        {
            get
            {

                return (originalPrice * 0.01).ToString("0.00");
            }
        }




    }


    public class UserIntegralDetail
    {
        public int UserId { get; set; }

        public string NickName { get; set; }

        public string Avatar { get; set; }

        public int TotalPoints { get; set; }

        /// <summary>
        /// 消费获得的积分
        /// </summary>
        public int FromConsumPoints { get; set; }

        /// <summary>
        /// 储值获得的积分
        /// </summary>
        public int FromSavmeMoneyPoints { get; set; }

        /// <summary>
        /// 累计总签到数量
        /// </summary>
        public int PlayCardNum { get; set; }

        /// <summary>
        /// 累计签到积分
        /// </summary>
        public int FromPlayCardPoints { get; set; }


        /// <summary>
        /// 已兑换的积分
        /// </summary>
        public int CostTotalPoints { get; set; }

        /// <summary>
        /// 后台手动增加的积分
        /// </summary>
        public int AdminAddTotalPoints { get; set; }

        /// <summary>
        /// 后台手动减少的积分
        /// </summary>
        public int AdminSubTotalPoints { get; set; }
    }


}
