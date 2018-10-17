using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 资讯分类
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class EntNewsType
    {
        /// <summary>
        /// 产品分类
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 用户小程序ID
        /// </summary>
        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 分类名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int sort { get; set; } = 99;
        /// <summary>
        /// 状态 1:正常,0:删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
        /// <summary>
        /// 默认不选中
        /// </summary>
        public bool sel { get; set; } = false;

        /// <summary>
        /// 所属上级分类
        /// </summary>
        [SqlField]
        public int ParentId { get; set; } = 0;

        /// <summary>
        /// 所属上级类别名称
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; }


    }
}
