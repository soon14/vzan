using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    /// <summary>
    /// 小程序操作异常信息
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CommandExceptionLog
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 小程序版本
        /// </summary>
        [SqlField]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 来源
        /// </summary>
        [SqlField]
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息
        /// </summary>
        [SqlField]
        public string ExceptionMsg { get; set; } = string.Empty;


        /// <summary>
        /// 发生时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 发送Email是否成功 0→失败 1成功
        /// </summary>
        [SqlField]
        public int IsSend { get; set; } = 0;


        /// <summary>
        /// 服务器站点
        /// </summary>
        [SqlField]
        public string WebAddress { get; set; } = string.Empty;



    }

    /// <summary>
    /// 发送邮件结果
    /// </summary>
    public class SendEmailResult
    {
        /// <summary>
        /// 发送成功 返回 数据库记录ID 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 发送结果
        /// </summary>
        public string msg { get; set; }
    }

}
