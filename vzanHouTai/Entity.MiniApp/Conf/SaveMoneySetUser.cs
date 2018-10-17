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
    /// 用户储值账户表
    /// </summary>
    /// [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class SaveMoneySetUser
    {
        public SaveMoneySetUser() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 小程序appid
        /// </summary>
        [SqlField]
        public string AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public int UserId { get; set; }


        /// <summary>
        /// 账户金额
        /// </summary>
        [SqlField]
        public int AccountMoney { get; set; }

        /// <summary>
        /// 账户金额
        /// </summary>
        public string AccountMoneyStr { get { return (AccountMoney * 0.01).ToString("0.00"); } }
        /// <summary>
        /// 首次充值时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 支付密码
        /// 默认为空，当用户使用储值单独支付的时候提示用户设置支付密码
        /// </summary>
        public string PassWord { get; set; } = string.Empty;
        
        

    }
}
