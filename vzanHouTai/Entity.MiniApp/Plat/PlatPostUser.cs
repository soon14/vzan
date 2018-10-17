using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Plat
{
   public class PlatPostUser
    {
        /// <summary>
        /// PlatMyCard 关联Id 用于关联注册用户
        /// </summary>
        public int PlatMyCardId { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 用户发电话号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 用户发布信息总数量
        /// </summary>
        public int  PostMsgCount { get; set; }

        /// <summary>
        /// 用户发布的置顶信息总数量
        /// </summary>
        public int TopMsgCount { get; set; }

        /// <summary>
        /// 用户发布的置顶信息所花费的总金额
        /// </summary>
        public string PostMsgPrice { get; set; } = "0.00";


        /// <summary>
        /// 用户注册时间
        /// </summary>
        public string AddTimeStr { get; set; }
      

    }

    public class PlatPostUserMsgCountViewMolde
    {
        /// <summary>
        /// 用户发布信息总数量
        /// </summary>
        public int PostMsgCount { get; set; }

        /// <summary>
        /// 用户发布的置顶信息总数量
        /// </summary>
        public int TopMsgCount { get; set; }

        /// <summary>
        /// 用户发布的置顶信息所花费的总金额
        /// </summary>
        public string PostMsgPrice { get; set; } = "0.00";

    }
}
