using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 产品标签
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntGoodLabel
    {
        /// <summary>
        /// 产品标签
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 标签名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;
        /// <summary>
        /// 编辑中的排序
        /// </summary>
        public int editsort { get { return sort; } }

        /// <summary>
        /// 状态
        /// 1：正常，0:删除
        /// </summary>
        [SqlField]
        public int state { get; set; } =1;

        public bool sel { get; set; } = false;
    }
}
