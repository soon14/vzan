using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 店铺二维码
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntStoreCode
    {
        /// <summary>
        /// 产品标签
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 权限表ID
        /// </summary>
        [SqlField]
        public int AId { get; set; } = 0;
        /// <summary>
        /// 二维码链接
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 参数
        /// </summary>
        [SqlField]
        public string Scene { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        [SqlField]
        public int State { get; set; }=1;
        /// <summary>
        /// 扫码次数
        /// </summary>
        [SqlField]
        public int ScanCount { get; set; }
        /// <summary>
        /// 下单次数
        /// </summary>
        [SqlField]
        public int OrderCount { get; set; }
    }
}
