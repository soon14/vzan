using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 模板消息发送参数记录
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class TemplateMsg_UserLog
    { 
        public TemplateMsg_UserLog() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        [SqlField]
        public int OrderId { get; set; }


        ///// <summary>
        ///// AppId
        ///// </summary>
        //[SqlField]
        //public string AppId { get; set; }

        /// <summary>
        /// 模板主表Id
        /// </summary>
        [SqlField]
        public int TmId { get; set; }

        /// <summary>
        /// 用户模板记录表
        /// </summary>
        [SqlField]
        public int TmuId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string Form_Id { get; set; }

        /// <summary>
        /// 接受者的openid
        /// </summary>
        [SqlField]
        public string Open_Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public DateTime AddDate { get; set; }

        /// <summary>
        /// 是否发送过消息 0 未发送,1 已发送  --(-2 已不需发送,-1 取消发送,0 未发送,1 已发送)
        /// </summary>
        [SqlField]
        public int State { get; set; }

        /// <summary>
        /// 模板消息类型
        /// </summary>
        [SqlField]
        public int TmgType { get; set; }

        /// <summary>
        /// 模板类型归属(所属哪个小程序模板的消息)
        /// </summary>
        [SqlField]
        public int Ttypeid { get; set; }


    }
}
