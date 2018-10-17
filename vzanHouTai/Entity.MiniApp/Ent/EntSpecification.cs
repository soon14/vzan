using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 产品规格
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class EntSpecification
    {
        /// <summary>
        /// 产品
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        
        /// <summary>
        /// ID
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;
        
        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;
        
        /// <summary>
        /// 图片
        /// </summary>
        [SqlField]
        public string imgurl { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID
        /// </summary>
        [SqlField]
        public int parentid { get; set; } = 0;

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
        /// 状态 1正常 0删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        public bool sel { get; set; } = false;

        /// <summary>
        /// 价格
        /// </summary>
        public float price { get; set; } = 0;
        /// <summary>
        /// 库存
        /// </summary>
        public int stock { get; set; } = 0;
    }
}
