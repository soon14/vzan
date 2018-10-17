using Entity.Base;
using Entity.MiniApp.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    /// <summary>
    /// 用户收藏 1表示点赞,2关注，3看过，4私信 记录
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatUserFavoriteMsg
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
        /// 用户Id
        /// </summary>
        [SqlField]
        public int UserId { get; set; }

        /// <summary>
        /// 类型 0表示 收藏 1表示点赞,2关注，3看过，4私信（PointsActionType）
        /// </summary>
        [SqlField]
        public int ActionType { get; set; }


        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        //public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        public string AddTimeStr { get; set; }

        /// <summary>
        /// 收藏状态 0表示正常 -1表示删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 数据类型，0：帖子，1：商品，2：评论(PointsDataType)
        /// </summary>
        [SqlField]
        public int Datatype { get; set; }
        public PlatMyCard MyCardModel { get; set; } = new PlatMyCard();
        /// <summary>
        /// 私信消息
        /// </summary>
        public ImMessage ImMessage { get; set; }
        public int Count { get; set; }
    }
}
