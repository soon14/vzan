using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.OpenWx
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class XcxTemplate
    {
        ///<summary>
        /// auto_increment
        /// </summary>		
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        ///<summary>
        /// 小程序模板名称
        /// </summary>		
        [SqlField]
        public string TName { get; set; }
        ///<summary>
        /// 第三方平台上小程序模板Id
        /// </summary>		
        [SqlField]
        public int TId { get; set; }
        ///<summary>
        /// 小程序封面
        /// </summary>		
        [SqlField]
        public string TImgurl { get; set; }
        ///<summary>
        /// 提交代码时版本号，初始为v1.0.0
        /// </summary>		
        [SqlField]
        public string Version { get; set; }
        ///<summary>
        /// 提交小程序的描述
        /// </summary>		
        [SqlField]
        public string Desc { get; set; }
        ///<summary>
        /// 添加时间
        /// </summary>		
        [SqlField]
        public DateTime AddTime { get; set; }

        ///<summary>
        /// 更新小程序模板时间
        /// </summary>		
        [SqlField]
        public DateTime UpdateTime { get; set; }

        ///<summary>
        /// 提交小程序审核的配置：页面地址
        /// </summary>		
        [SqlField]
        public string Address { get; set; }

        ///<summary>
        /// 提交小程序审核的配置：标签
        /// </summary>		
        [SqlField]
        public string Tag { get; set; }

        ///<summary>
        /// 提交小程序审核的配置：第一分类
        /// </summary>		
        [SqlField]
        public string First_class { get; set; }

        ///<summary>
        /// 提交小程序审核的配置：第二分类
        /// </summary>		
        [SqlField]
        public string Second_class { get; set; }

        ///<summary>
        /// 提交小程序审核的配置：第三分类
        /// </summary>		
        [SqlField]
        public string Third_class { get; set; }
        ///<summary>
        /// 提交小程序审核的配置：标题
        /// </summary>		
        [SqlField]
        public string Title { get; set; }

        ///<summary>
        /// 状态
        /// </summary>		
        [SqlField]
        public int State { get; set; }

    }
}
