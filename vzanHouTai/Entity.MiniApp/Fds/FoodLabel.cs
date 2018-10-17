using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Fds
{
    /// <summary>
    /// 小程序餐饮版-商品标签
    /// </summary>
    [Serializable]
    [SqlTable(Utility.dbEnum.MINIAPP)]
    public class FoodLabel
    {
        public FoodLabel() { }
        /// <summary>
        /// 属性ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 商家id
        /// </summary>
        [SqlField]
        public int FoodStoreId { get; set; }
        /// <summary>
        /// 标签名称
        /// </summary>
        [SqlField]
        public string LabelName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; }
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
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 0;

        public bool isCheck { get; set; }

    }
}
