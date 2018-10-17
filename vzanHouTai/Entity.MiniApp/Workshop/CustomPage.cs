using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Workshop
{
    /// <summary>
    /// 小未工坊
    /// 自定义页面
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CustomPage
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int uid { get; set; } = 0;
        [SqlField]
        public DateTime addtime  { get; set; } = DateTime.Now;
        public string addtime_fmt
        {
            get { return addtime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
        [SqlField]
        public DateTime updatetime { get; set; } = DateTime.Now;
        public string updatetime_fmt
        {
            get { return updatetime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
        /// <summary>
        /// 用json格式来保存自定义页面的内容
        /// </summary>
        [SqlField]
        public string content { get; set; } = string.Empty;

        [SqlField]
        public int viewcount { get; set; } = 0;
        /// <summary>
        /// 页面状态
        /// -1：删除
        /// 0：草稿
        /// 1：发布
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;
        /// <summary>
        /// 分享二维码
        /// </summary>
        [SqlField]
        public string qrcode { get; set; } = string.Empty;
    }
}
