using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.cityminiapp
{
    /// <summary>
    /// 同城模板 帖子 浏览量-收藏数量-分享数量统计
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityMsgViewFavoriteShare
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public int aid { get; set; }

      /// <summary>
      /// 帖子ID
      /// </summary>
        [SqlField]
        public int msgId { get; set; }


        /// <summary>
        /// 帖子总浏览量
        /// </summary>
        [SqlField]
        public int ViewCount { get; set; }


        /// <summary>
        /// 帖子总收藏量
        /// </summary>
        [SqlField]
        public int FavoriteCount { get; set; }

        /// <summary>
        /// 帖子总分享量
        /// </summary>
        [SqlField]
        public int ShareCount { get; set; }



        /// <summary>
        /// 帖子总点赞量
        /// </summary>
        [SqlField]
        public int DzCount { get; set; }


        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

      


    }
}
