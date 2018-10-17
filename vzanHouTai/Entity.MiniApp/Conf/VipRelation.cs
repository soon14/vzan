using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 会员表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VipRelation
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 小程序appid
        /// </summary>
        [SqlField]
        public string appId { get; set; } = string.Empty;

        /// <summary>
        /// 客户id
        /// </summary>
        [SqlField]
        public int uid { get; set; } = 0;

        /// <summary>
        /// 会员权限id
        /// </summary>
        [SqlField]
        public int levelid { get; set; } = 0;

        /// <summary>
        /// 状态 0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }

        public string showaddtime
        {
            get
            {
                return addtime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 消费总额
        /// </summary>
        [SqlField]
        public int PriceSum { get; set; } = 0;

        public string pricestr
        {
            get
            {
                return (PriceSum * 0.01).ToString("0.00");
            }
        }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; }

        public string showupdatetime
        {
            get
            {
                return updatetime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 会员等级信息
        /// </summary>
        public VipLevel levelInfo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string username { get; set; } = string.Empty;
        /// <summary>
        /// 客户头像
        /// </summary>
        public string headimgurl { get; set; } = string.Empty;
        /// <summary>
        /// 会员等级名称
        /// </summary>
        public string levelName { get; set; } = string.Empty;
        /// <summary>
        /// 储值余额
        /// </summary>
        public int AccountMoney { get; set; } = 0;

        public string AccountMoneystr
        {
            get
            {
                return (AccountMoney*0.01).ToString("0.00");
            }
        }


        /// <summary>
        /// 会员备注信息
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 微信会卡包会员卡标识
        /// </summary>
        public string WxVipCode { get; set; } = "未领取";


        /// <summary>
        /// 用户手机号码
        /// </summary>
        public string TelePhone { get; set; }

        /// <summary>
        /// 用户类型 
        /// </summary>
        public int userType { get; set; }

        public string userTypeStr
        {
            get
            {
                return Enum.GetName(typeof(UserType), userType);
            }
        }

        public string reservation { get; set; }


        public double SaveMoneySum { get; set; }

        public string SaveMoneySumStr {
            get
            {
                return SaveMoneySum.ToString("0.00");
            }
        }
    }
}
