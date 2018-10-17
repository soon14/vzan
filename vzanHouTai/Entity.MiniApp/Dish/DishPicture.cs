using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 餐饮多门店图片表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishPicture
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        [SqlField]
        public int aid { get; set; } = 0;
        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// 附件id
        /// </summary>
        [SqlField]
        public string img { get; set; } = string.Empty;

        /// <summary>
        /// 跳转链接
        /// </summary>
        [SqlField]
        public string url { get; set; } = string.Empty;

        public string url_fmt {
            get {
                if (string.IsNullOrEmpty(url))
                    return "";
                else
                    return "/pages/restaurant/restaurant-home-info/index?dish_id=" + url;
            }
        }
        /// <summary>
        /// 是否显示 0：不显示  1：显示
        /// </summary>
        [SqlField]
        public int is_show { get; set; } = 1;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int is_order { get; set; } = 99;

        /// <summary>
        /// 图片类型：0：轮播图
        /// </summary>
        [SqlField]
        public int type { get; set; } = 0;

        /// <summary>
        /// 状态 -1:删除 0:正常
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;
        public string addTimeStr
        {
            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 图片链接
        /// </summary>
        public string imgUrl { get; set; } = string.Empty;
    }
}
