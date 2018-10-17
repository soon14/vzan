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
    /// 同城模板 用户消息
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityUserMsg
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
        /// 消息发送方用户Id  -1表示系统 其它表示用户
        /// </summary>
        [SqlField]
        public int fromUserId { get; set; }

       


        /// <summary>
        /// 消息接收方用户Id
        /// </summary>
        [SqlField]
        public int toUserId { get; set; }


        /// <summary>
        /// 消息内容
        /// </summary>
        [SqlField]
        public string msgBody { get; set; }

        /// <summary>
        /// 消息类别 0表示后台举报管理确认处理的消息 1表示用户点赞消息
        /// </summary>
        [SqlField]
        public int msgType { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime updateTime { get; set; }

        /// <summary>
        /// 状态 0为未读 1为已读
        /// </summary>
        [SqlField]
        public int state { get; set; }


        /// <summary>
        /// 小于1分钟显示刚刚,大于1分钟小于30分钟显示多少分钟前
        /// 小于1小时大于30分钟显示 1小时前
        /// 小于24小时大于1小时 显示多少小时前
        /// 大于1天小于一个月 显示多少天前
        /// 大于1一个月小于一年显示 多少个月前
        /// 大一年之后直接显示 具体时间
        /// </summary>
        public string addTimeStr { get; set; }
        

        /// <summary>
        /// 消息发送方用户昵称
        /// </summary>
        public string fromUserName { get; set; }


        /// <summary>
        /// 消息发送方用户头像
        /// </summary>
        public string fromUseImg { get; set; }

       




    }
}
