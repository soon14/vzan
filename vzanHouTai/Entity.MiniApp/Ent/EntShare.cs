using Entity.Base;
using Entity.MiniApp;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 分享配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class EntShare
    {
        /// <summary>
        /// 分享配置
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 如果是行业高级版则保存为 用户小程序授权权限表Id 如果是电商版则保存店铺Id  
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 店铺名称
        /// </summary>
        [SqlField]
        public string StoreName { get; set; } = string.Empty;

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
        /// 类型 0,表示行业高级版 1表示电商版
        /// </summary>
        [SqlField]
        public int ShareType { get; set; } = 0;

        /// <summary>
        /// 是否开启分享
        /// </summary>
        [SqlField]
        public int IsOpen { get; set; } = 0;
        /// <summary>
        /// 分享图样式类别Id  总共7种
        /// </summary>
        [SqlField]
        public int StyleType { get; set; } = 0;

        /// <summary>
        /// 店铺Logo
        /// </summary>
        public List<C_Attachment> Logo { get; set; } 

        /// <summary>
        /// 广告图
        /// </summary>
        public List<C_Attachment> ADImg { get; set; }

    }
}
