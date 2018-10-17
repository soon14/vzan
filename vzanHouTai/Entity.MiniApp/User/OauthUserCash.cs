using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{/// <summary>
 /// 同城系统信息表
 /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class OauthUserCash
    {
        /// <summary>
        /// 同城用户ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int ID { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserID { get; set; }
        /// <summary>
        /// 微赞OpenID
        /// </summary>
        [SqlField]
        public string OpenId { get; set; }
        /// <summary>
        /// 对应公众号的OpenID
        /// </summary>
        [SqlField]
        public string ThirdOpenId { get; set; }
        /// <summary>
        /// 支付公众号AppID
        /// </summary>
        [SqlField]
        public string AppID { get; set; }
        /// <summary>
        /// 支付用户对应公众号的金额
        /// </summary>
        [SqlField(IsUpdateRemove = true)]
        public int Cash { get; set; } = 0;
        /// <summary>
        /// 支付用户对应公众号的金额
        /// </summary>
        [SqlField(IsUpdateRemove = true)]
        public int HistoryCash { get; set; } = 0;
    }
}
