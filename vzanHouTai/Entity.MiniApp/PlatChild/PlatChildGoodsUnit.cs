using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.PlatChild
{
   /// <summary>
   /// 产品数量单位
   /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildGoodsUnit
    {
        /// <summary>
        /// 产品标签
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int Aid { get; set; } = 0;
        /// <summary>
        /// 单位名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 99;
        [SqlField]
        public int State { get; set; } = 1;
    }
}
