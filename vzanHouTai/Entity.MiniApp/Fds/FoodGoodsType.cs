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
    /// 小程序餐饮版-餐饮商品类型
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class FoodGoodsType
    {
        public FoodGoodsType() { }
        /// <summary>
        /// 类型ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        [SqlField]
        public string Name { get; set; }
        /// <summary>
        /// 餐饮店铺id
        /// </summary>
        [SqlField]
        public int FoodId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 状态 0 正常/-1 删除
        /// </summary>
        [SqlField]
        public int State { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int SortVal { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        [SqlField]
        public string LogImg { get; set; }
        
        /// <summary>
        /// <辅助字段>用于表示model是否有被选中
        /// </summary>
        public bool sel { get; set; }
    }
}
