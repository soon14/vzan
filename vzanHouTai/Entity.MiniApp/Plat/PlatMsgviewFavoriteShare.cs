using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    ///  帖子 浏览量-收藏数量-分享数量统计
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatMsgviewFavoriteShare
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
        public int AId { get; set; }

      /// <summary>
      /// 帖子ID
      /// </summary>
        [SqlField]
        public int MsgId { get; set; }


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
        public DateTime AddTime { get; set; }
        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// 关注量
        /// </summary>
        [SqlField]
        public int FollowCount { get; set; }

        /// <summary>
        /// 私信量
        /// </summary>
        [SqlField]
        public int SiXinCount { get; set; }

        /// <summary>
        /// 数据类型，0：帖子，1：商品，2：评论，3：名片
        /// </summary>
        [SqlField]
        public int DataType { get; set; }
    }
}
