using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entity.MiniApp.Tools
{
    /// <summary>
    /// 添加拼团
    /// </summary>
    public class AddGroupModel
    {
        public string appId { get; set; }
        public int userId { get; set; }
        /// <summary>
        /// 拼团商品ID
        /// </summary>
        public int groupId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int num { get; set; }
        /// <summary>
        /// 是否团购
        /// </summary>
        public int isGroup { get; set; }
        /// <summary>
        /// 是否团长
        /// </summary>
        public int isGHead { get; set; }
        /// <summary>
        /// 参团ID
        /// </summary>
        public int gsid { get; set; }
        /// <summary>
        /// 用户拼团记录ID
        /// </summary>
        public int guid { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int payprice { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string addres { get; set; }
        /// <summary>
        /// 收货人电话
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string msg { get; set; }
    }
}