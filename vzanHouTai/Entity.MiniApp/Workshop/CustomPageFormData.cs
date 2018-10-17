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
    public class CustomPageFormData
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int pageid { get; set; } = 0;
        [SqlField]
        public int userid { get; set; } = 0;
        [SqlField]
        public DateTime addtime  { get; set; } = DateTime.Now;
        public string addtime_fmt
        {
            get { return addtime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
        /// <summary>
        /// 用json格式来保存提交的表单内容
        /// </summary>
        [SqlField]
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        [SqlField]
        public int state { get; set; } = 0;

    }
}
