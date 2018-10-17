using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 小程序请求 GetPhoneNumber 返回数据解密后的实体
    /// </summary>
    public class GetPhoneNumberReturnJson
    {

        /// <summary>
        /// 手机号码
        /// </summary>
        public string phoneNumber { get; set; }

        /// <summary>
        /// 带区号的手机号码(国外手机会有区号)
        /// </summary>
        public string purePhoneNumber { get; set; }

        /// <summary>
        /// countryCode
        /// </summary>
        public string countryCode { get; set; }


        /// <summary>
        /// watermark
        /// </summary>
        public watermark watermark { get; set; }
    }

    public class watermark
    {
        public string appid { get; set; }

        public string timestamp { get; set; }
    }
}
