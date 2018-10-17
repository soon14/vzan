using Entity.Base;
using System;
using System.Collections.Generic;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 产品分类
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class EntGoodType
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
        /// 编辑中的排序
        /// </summary>
        public int editsort { get { return sort; } }
        /// <summary>
        /// 状态 1:正常,0:删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;
        /// <summary>
        /// 分类类型：见枚举GoodProjectType
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;
        /// <summary>
        /// 规格：如足浴版包间分类可容纳数量
        /// </summary>
        [SqlField]
        public int count { get; set; } = 0;

        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int storeId { get; set; } = 0;


        /// <summary>
        /// 父级ID 专业版 行业版 产品分类为二级分类
        /// </summary>
        [SqlField]
        public int parentId { get; set; } = -1;

        public string parentName { get; set; } = string.Empty;

        /// <summary>
        /// 默认不选中
        /// </summary>
        public bool sel { get; set; } = false;

        /// <summary>
        /// 类别图片
        /// </summary>
        [SqlField]
        public string Img { get; set; } = string.Empty;

    }

    public class GoodTypeRelation
    {
        public EntGoodType FirstGoodType { get; set; }
        public List<EntGoodType> SecondGoodTypes { get; set; }
       

    }

}
