using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Plat
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class PlatMsgReport
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
        public int Aid { get; set; }

        /// <summary>
        /// 举报者用户Id
        /// </summary>
        [SqlField]
        public int ReportcardId { get; set; }




        /// <summary>
        /// 被举报的信息ID
        /// </summary>
        [SqlField]
        public int MsgId { get; set; }


        /// <summary>
        /// 举报原因
        /// </summary>
        [SqlField]
        public string ReportReason { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 状态 0为正常 -1为删除
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 确认状态 0为未确认 1为已经确认
        /// </summary>
        [SqlField]
        public int ConfirmState { get; set; }



        public string AddTimeStr
        {

            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        /// <summary>
        /// 举报者用户昵称
        /// </summary>
        public string ReportUserName { get; set; }


        /// <summary>
        /// 被举报者用户昵称
        /// </summary>
        public string BeReportUserName { get; set; }

        /// <summary>
        /// 被举报者电话 发信息填写的电话
        /// </summary>
        public string BeReportMsgPhone { get; set; }

        /// <summary>
        /// 被举报信息的状态 0为无效 1为有效 -1为删除
        /// </summary>
        public int BeReportMsgState { get; set; }




    }
}
