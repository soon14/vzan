using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    /// <summary>
    /// 获取第三方平台access_token
    /// </summary>
    [Serializable]
    public class ComponentAccessTokenResult
    {
        /// <summary>
        /// 第三方平台access_token
        /// </summary>
        public string component_access_token { get; set; }
        /// <summary>
        /// 有效期
        /// </summary>
        public int expires_in { get; set; }
    }
}
