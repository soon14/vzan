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
    /// 同城模板 用户收藏或者点赞的帖子
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityUserFavoriteMsg
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
        /// 用户Id
        /// </summary>
        [SqlField]
        public int userId { get; set; }

        /// <summary>
        /// 类型 0表示 收藏 1表示点赞
        /// </summary>
        [SqlField]
        public int actionType { get; set; }


        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

        /// <summary>
        /// 收藏状态 0表示正常 -1表示删除
        /// </summary>
        [SqlField]
        public int state { get; set; }

        /// <summary>
        /// 数据类型，0：帖子，1：商品，2：评论(PointsDataType)
        /// </summary>
        [SqlField]
        public int Datatype { get; set; }
    }
}
