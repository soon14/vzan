using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
	/// orders:付款订单--实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
    [SqlTable(dbEnum.MINIAPP, UseMaster = true)]
    public class PayCenterSetting
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 公众号Appid
        /// </summary>
        [SqlField]
        public string Appid { get; set; } = string.Empty;
        /// <summary>
        /// 商户号
        /// </summary>
        [SqlField]
        public string Mch_id { get; set; } = string.Empty;
        /// <summary>
        /// 商户秘钥
        /// </summary>
        [SqlField]
        public string Key { get; set; }
        /// <summary>
        /// 绑定类型，1：论坛，2：直播，3：同城，4：商城
        /// </summary>
        [SqlField]
        public int BindingType { get; set; }
        /// <summary>
        /// 对应的绑定ID
        /// </summary>
        [SqlField]
        public int BindingId { get; set; }
        /// <summary>
        /// 证书安装地址
        /// </summary>
        [SqlField]
        public string SSLCERT_PATH { get; set; }
        /// <summary>
        /// 证书密码
        /// </summary>
        [SqlField]
        public string SSLCERT_PASSWORD { get; set; }
        /// <summary>
        /// 状态，-1为关闭
        /// </summary>
        [SqlField]
        public int Status { get; set; }

        //支付费率
        public double PayRate { get; set; } = 0;
    }
}
