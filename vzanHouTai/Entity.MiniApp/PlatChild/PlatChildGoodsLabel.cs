using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.PlatChild
{
    /// <summary>
    /// 产品标签
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatChildGoodsLabel
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
        public int AId { get; set; } = 0;
        /// <summary>
        /// 标签名称
        /// </summary>
        [SqlField]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 99;
        /// <summary>
        /// 编辑中的排序
        /// </summary>
        public int Editsort { get; set; }

        /// <summary>
        /// 状态
        /// 1：正常，0:删除
        /// </summary>
        [SqlField]
        public int State { get; set; } =1;

        public bool Sel { get; set; } = false;
    }
}
