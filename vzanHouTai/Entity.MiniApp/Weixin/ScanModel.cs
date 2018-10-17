using Senparc.Weixin.MP.AdvancedAPIs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 微信扫码数据
    /// </summary>
    public class ScanModel
    {
        /// <summary>
        /// 是否已使用：0未使用，1已使用
        /// </summary>
        public int isUse { get; set; } = 0;
        public UserInfoJson userInfo { get; set; }

    }
}
