using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Ent
{
    /// <summary>
    /// 提现记录申请表操作日志
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public  class DrawCashApplyLog
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
        public int appId { get; set; } = 0;

        /// <summary>
        /// 登录用户操作账号
        /// </summary>
        [SqlField]
        public string accountid { get; set; } = string.Empty;


       
        /// <summary>
        /// 新增时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        public string AddTimeStr
        {
            get
            {
                return AddTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }


        /// <summary>
        /// 操作描述
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 操作IP
        /// </summary>
        [SqlField]
        public string hostIP { get; set; } = string.Empty;

        
    }
}
