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
    /// 同城模板 信息举报管理
    /// </summary>

    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CityMsgReport
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
        /// 举报者用户Id
        /// </summary>
        [SqlField]
        public int reportUserId { get; set; }

       


        /// <summary>
        /// 被举报的信息ID
        /// </summary>
        [SqlField]
        public int msgId { get; set; }


        /// <summary>
        /// 举报原因
        /// </summary>
        [SqlField]
        public string reportReason { get; set; }

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
        /// 状态 0为正常 -1为删除
        /// </summary>
        [SqlField]
        public int state { get; set; }

        /// <summary>
        /// 确认状态 0为未确认 1为已经确认
        /// </summary>
        [SqlField]
        public int confirmState { get; set; }



        public string addTimeStr
        {

            get
            {
                return addTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 举报者用户昵称
        /// </summary>
        public string reportUserName { get; set; }


        /// <summary>
        /// 被举报者用户昵称
        /// </summary>
        public string beReportUserName { get; set; }

        /// <summary>
        /// 被举报者电话 发信息填写的电话
        /// </summary>
        public string beReportMsgPhone { get; set; }

        /// <summary>
        /// 被举报信息的状态 0为无效 1为有效 -1为删除
        /// </summary>
        public int beReportMsgState { get; set; }




    }
}
