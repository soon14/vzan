using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 积分商城兑换积分规则
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class ExchangeRule
    {

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;
       
        /// <summary>
        /// 赠送积分
        /// </summary>
        [SqlField]
        public int integral { get; set; } = 0;


        /// <summary>
        /// 当选择部分商品消费赠送积分的时候 此字段为商品id集合以逗号分隔
        /// </summary>
        [SqlField]
        public string goodids { get; set; } = string.Empty;


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
        /// 积分赠送规则方式  0→全场商品消费赠送积分  1→ 部分商品消费赠送积分 2→储值
        /// </summary>
        [SqlField]
        public int ruleType { get; set; } = 0;

        /// <summary>
        /// 每笔消费满多少钱赠送积分
        /// </summary>
        [SqlField]
        public int price { get; set; } = 0;

        public string priceStr
        {
            get
            {

                return (price * 0.01).ToString();
            }
        }
       

        /// <summary>
        /// 状态 1:删除,0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;



        public List<PickGood> goodslist { get; set; } = new List<PickGood>();

        public int goodCount {
            get
            {
                if (goodids != null && !string.IsNullOrEmpty(goodids))
                    return goodids.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries).Length;
                return 0;
            }
        } 

    }

    public class PickGood
    {
        public int Id { get; set; }
        public string GoodsName { get; set; }
        public string ImgUrl { get; set; }
        public bool sel { get; set; }
        public string showtime { get; set; }
    }

}
