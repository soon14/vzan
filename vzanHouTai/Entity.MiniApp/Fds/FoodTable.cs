using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Fds
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
   public class FoodTable
    {

        public FoodTable() { }
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 店铺Id
        /// </summary>
        [SqlField]
        public int FoodId { get; set; } = 0;
        /// <summary>
        /// 桌号
        /// </summary>
        [SqlField]
        public string Scene { get; set; } = string.Empty;

        /// <summary>
        /// 二维码
        /// </summary>
        [SqlField]
        public string ImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 状态
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.MinValue;
        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
        

    }
}
