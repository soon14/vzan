using Entity.MiniApp.Ent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.Footbath
{
    public class FootbathGiftModel
    {
        /// <summary>
        /// 技师工号
        /// </summary>
        public string jobNumber { get; set; } = string.Empty;
        /// <summary>
        /// 技师姓名
        /// </summary>
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 顾客微信昵称 
        /// </summary>

        public string nickName { get; set; } = string.Empty;
        /// <summary>
        /// 性别
        /// </summary>
        public int sex { get; set; } = 0;
        public string sexStr
        {
            get
            {
                switch (sex)
                {
                    case 1: return "女";
                    case 2: return "男";
                    default: return "未知";
                }
            }
        }
        /// <summary>
        /// 支付金额
        /// </summary>

        public int price { get; set; } = 0;
        public string priceStr
        {
            get
            {
                return (price * 0.01).ToString("0.00");
            }
        }
        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; }
        public string createTimeStr { get
            {
                return createTime.ToString("yyyy-MM-dd HH:mm:ss");
            } }

    }

}