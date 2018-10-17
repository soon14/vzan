using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序餐饮版-商品规格
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsAttr
    {
        public FoodGoodsAttr() { }
        /// <summary>
        /// 规格ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        [SqlField]
        public int FoodId { get; set; }
        /// <summary>
        /// 规格名称
        /// </summary>
        [SqlField]
        public string AttrName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; }

        public List<FoodGoodsSpec> SpecList { get; set; }


        public bool sel { get; set; }

    }


    /// <summary>
    /// 小程序餐饮版-商品属性
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsSpec
    {
        public FoodGoodsSpec() { }
        /// <summary>
        /// 属性ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 规格id
        /// </summary>
        [SqlField]
        public int AttrId { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        [SqlField]
        public string SpecName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }


        public bool sel { get; set; } = false;
        
    }

}
