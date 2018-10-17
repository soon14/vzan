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
    /// 日志需要表达出：谁在什么模板哪个功能中做了什么操作
    /// 这几个字段必填：（UserId，UserName）（TemplateName，ModuleName）（StoreId,AId，Info）
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class CommonLog
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int UserId { get; set; } = 0;

        /// <summary>
        /// 用户名称
        /// </summary>
        [SqlField]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 模板名称
        /// </summary>
        [SqlField]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 模块名称
        /// </summary>
        [SqlField]
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型
        /// </summary>
        [SqlField]
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// 日志信息
        /// </summary>
        [SqlField]
        public string Info { get; set; } = string.Empty;

        [SqlField]
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 记录时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 日志类型 Info=一般消息,Error=错误日志
        /// </summary>
        [SqlField]
        public string LogType { get; set; } = "Info";

        /// <summary>
        /// 门店ID 
        /// </summary>
        [SqlField]
        public int StoreId { get; set; } = 0;

        [SqlField]
        public int AId { get; set; } = 0;

        /// <summary>
        /// 跟模块名称相关，对应模块的ID，例如 模块是产品这里保存产品ID
        /// </summary>
        [SqlField]
        public int AttachId { get; set; } = 0;
    }

    /// <summary>
    /// 模块名称
    /// </summary>
    public enum CommonLogModule
    {
        None=0,
        产品=1,
        拼团=2,
        砍价=3,
        秒杀=4,
        订单=5,
        打印=6,
        模板消息=7,
        会员=8
    }

    /// <summary>
    /// 动作名称
    /// </summary>
    public enum CommonLogActionType
    {
        None=0,
        添加=1,
        删除=2,
        修改=3,
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum CommonLogType
    {
        None=0,
        Info=1,
        Error=2
    }
}
