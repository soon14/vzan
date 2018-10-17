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
    /// 用户模板记录表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class TemplateMsg_User
    {
        public TemplateMsg_User() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 各模板主表Id  --此列数据因现阶段不需使用,数据不全
        /// </summary>
        //[SqlField]
        public int TmpId { get; set; }

        /// <summary>
        /// 用户小程序AppId
        /// </summary>
        [SqlField]
        public string AppId { get; set; }

        /// <summary>
        /// 微信公众号的模板ID
        /// </summary>
        [SqlField]
        public string TemplateId { get; set; }

        /// <summary>
        /// 模板隶属于哪个类型的小程序
        /// </summary>
        [SqlField]
        public int Ttypeid { get; set; }

        /// <summary>
        /// 消息模板表Id (TemplateMsg 表ID)
        /// </summary>
        [SqlField]
        public int TmId { get; set; }

        /// <summary>
        /// 有哪些模板列,格式：[A,B,C]
        /// </summary>
        [SqlField]
        public string ColNums { get; set; }


        /// <summary>
        /// 状态: 是否开启 0为关闭,1为开启
        /// </summary>
        [SqlField]
        public int State { get; set; }


        /// <summary>
        /// 点击模板消息跳转的页面
        /// </summary>
        [SqlField]
        public string PageUrl { get; set; }

        /// <summary>
        /// 微信模板库Id
        /// </summary>
        [SqlField]
        public string TitleId { get; set; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// 模板消息类型
        /// </summary>
        [SqlField]
        public int TmgType { get; set; }
    }
}
