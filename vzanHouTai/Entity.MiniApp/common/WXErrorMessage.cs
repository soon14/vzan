using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 微信接口返回的错误信息
    /// </summary>
    public class WXErrorMessage
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public string errcode { get; set; } = string.Empty;

        /// <summary>
        /// 错误提示
        /// </summary>
        public string errmsg { get; set; } = string.Empty;
    }
}
