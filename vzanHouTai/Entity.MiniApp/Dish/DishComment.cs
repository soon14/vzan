using Entity.Base;
using System;
using Utility;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Entity.MiniApp.Dish
{
    /// <summary>
    /// 订单评论
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class DishComment
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        [SqlField]
        public int oId { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int uId { get; set; } = 0;

        [SqlField]
        public int aId { get; set; } = 0;

        [SqlField]
        public int storeId { get; set; } = 0;

        /// <summary>
        /// 评分 1-5
        /// </summary>
        [SqlField]
        public int star { get; set; } = 5;

        /// <summary>
        /// 评价内容
        /// </summary>
        [SqlField]
        [Required(AllowEmptyStrings = false, ErrorMessage = "评价内容不能为空")]
        [MinLength(length:8,ErrorMessage ="评价不能少于8个字")]
        public string content { get; set; } = string.Empty;

        /// <summary>
        /// 状态 1=显示，-1=删除
        /// </summary>
        [SqlField]
        public int state { get; set; } = 1;

        [SqlField]
        public DateTime addTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 图片，多图用分号;分割
        /// </summary>
        [SqlField]
        public string imgs { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        [SqlField]
        public string nickName { get; set; } = string.Empty;
        public List<string> imgsList
        {
            get
            {
                return string.IsNullOrEmpty(imgs) ? new List<string>() : imgs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        public string headImg { get; set; } = string.Empty;
    }
}
