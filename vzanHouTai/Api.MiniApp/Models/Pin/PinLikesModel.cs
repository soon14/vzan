using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.MiniApp.Models.Pin
{
    /// <summary>
    /// 用于接收小程序提交的添加，修改个人收藏类
    /// </summary>
    public class PinLikesModel
    {
        /// <summary>
        /// 添加的时候给0，修改的时候给当前对象的ID
        /// </summary>
        public int id { get; set; } = 0;

        /// <summary>
        /// 小程序id 通过getAid获取
        /// </summary>
        public int aId { get; set; } = 0;

        /// <summary>
        /// 收藏类型，0=产品，1=店铺,2=评价
        /// </summary>
        public int type { get; set; } = 0;

        /// <summary>
        /// 收藏ID
        /// </summary>
        public int likeId { get; set; } = 0;

        //public int state { get; set; } = 1;

    }
}