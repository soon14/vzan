using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.PlatChild
{
    /// <summary>
    /// 分享配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildShare
    {
        /// <summary>
        /// 分享配置
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int Aid { get; set; } = 0;
        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string StoreName { get; set; } = string.Empty;


        /// <summary>
        /// 店铺Logo
        /// </summary>
        [SqlField]
        public string StoreLogo { get; set; } = string.Empty;

        /// <summary>
        /// 广告图
        /// </summary>
        [SqlField]
        public string ADImg { get; set; } = string.Empty;


        /// <summary>
        /// 广告语
        /// </summary>
        [SqlField]
        public string ADTitle { get; set; } = string.Empty;

        /// <summary>
        /// 小程序码
        /// </summary>
        [SqlField]
        public string Qrcode { get; set; } = string.Empty;


        /// <summary>
        /// 分享图样式类别Id  总共6种
        /// </summary>
        [SqlField]
        public int StyleType { get; set; } = 0;


        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }



    }
}
