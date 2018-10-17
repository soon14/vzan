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
    ///  评论列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityMsgComment
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
        /// 帖子Id
        /// </summary>
        [SqlField]
        public int MsgId { get; set; }


        /// <summary>
        /// 帖子主人
        /// </summary>
        [SqlField]
        public int ToUserId { get; set; }


        /// <summary>
        /// 用户Id  发出评论的用户
        /// </summary>
        [SqlField]
        public int FromUserId { get; set; }


        /// <summary>
        /// 用户评论文字内容
        /// </summary>
        [SqlField]
        public string CommentDetail { get; set; }



        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }


        public string AddTimeStr { get { return AddTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// 收藏状态 0表示正常 -1表示删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        public string NickName { get; set; } 
        public string HeaderImg { get; set; }

        public string ToNickName { get; set; }
        public string ToHeaderImg { get; set; }

        public string ShowTimeStr { get; set; }

        /// <summary>
        /// 评论对象详情 如果评论的是帖子 则是帖子详情前20字符
        /// </summary>
        public string MsgTxt { get; set; }
        public string MsgFirstImg { get; set; }

      


    }
}
