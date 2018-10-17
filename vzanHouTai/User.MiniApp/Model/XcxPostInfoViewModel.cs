using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entity.MiniApp;

namespace User.MiniApp.Model
{
    /// <summary>
    /// 小程序分类信息
    /// </summary>
    public class XcxPostInfoViewModel
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public int tid { get; set; }
        /// <summary>
        /// 小程序Id
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string mc_id { get; set; }
        /// <summary>
        /// 商户秘钥
        /// </summary>
        public string mc_key { get; set; }
        /// <summary>
        /// 广告图
        /// </summary>
        public string imgurl { get; set; }
        /// <summary>
        /// 小程序秘钥
        /// </summary>
        public string appsr { get; set; }

        public List<OpenAuthorizerConfig> XUserList { get; set; }
    }
}