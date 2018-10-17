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
    /// 模板记录表
    /// </summary>
    /// [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class TemplateMsg
    {
       
        public TemplateMsg() { }

        /// <summary>
        /// 
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }


        /// <summary>
        /// 模板的标题Id
        /// </summary>
        [SqlField]
        public string TitileId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string TitleName { get; set; }


        /// <summary>
        /// 链接地模板隶属于哪个类型的小程序
        /// </summary>
        [SqlField]
        public int Ttypeid { get; set; }

        /// <summary>
        /// 有哪些模板列,格式：[A,B,C]
        /// </summary>
        [SqlField]
        public string ColNums { get; set; }

        /// <summary>
        /// 模板列 文字说明
        /// </summary>
        [SqlField]
        public string ColNumsRemark { get; set; }

        /// <summary>
        /// 点击模板消息跳转的页面
        /// </summary>
        [SqlField]
        public string PageUrl { get; set; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 是否供用户选择使用
        /// </summary>
        [SqlField]
        public int State { get; set; } = 1;


        /// <summary>
        /// 模板启用状态
        /// </summary>
        public int openState { get; set; } = 0;

        /// <summary>
        /// 示例链接
        /// </summary>
        [SqlField]
        public string egUrl { get; set; }

        /// <summary>
        /// 模板消息类型
        /// </summary>
        [SqlField]
        public int TmgType { get; set; }
        
    }
}
