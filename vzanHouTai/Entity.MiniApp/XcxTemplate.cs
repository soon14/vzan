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
    /// 小程序模板表
    /// </summary>
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
        /// 小程序Id
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
        /// 小程序价格
        /// </summary>		
        [SqlField]
        public int Price { get; set; }
        public string PriceStr { get { return (Price * 0.01).ToString("0.00"); } }
        ///<summary>
        /// 小程序零售价格
        /// </summary>		
        [SqlField]
        public int RetailPrice { get; set; }
        ///<summary>
        /// 排序
        /// </summary>		
        [SqlField]
        public int Sort { get; set; }

        public string ShowPrice { get; set; }
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

        ///<summary>
        /// 类型
        /// </summary>		
        [SqlField]
        public int Type { get; set; }
        ///<summary>
        /// 链接地址
        /// </summary>		
        [SqlField]
        public string Link { get; set; }
        ///<summary>
        /// 项目类型：同城：1，小程序：2：直播：3，论坛：4，有约：5 枚举ProjectType
        /// </summary>		
        [SqlField]
        public int ProjectType { get; set; }
        ///<summary>
        /// 多门店每增加一门店所付价格
        /// </summary>		
        [SqlField]
        public int SPrice { get; set; }
        ///<summary>
        /// 每开通一个多门店附带开通几家分门店
        /// </summary>		
        [SqlField]
        public int SCount { get; set; }
        /// <summary>
        /// 小程序授权类型，1：第三方授权，0：个人授权
        /// </summary>
        [SqlField]
        public int AuthoAppType { get; set; } = 1;
        /// <summary>
        /// 小程序源码下载链接
        /// </summary>
        [SqlField]
        public string AppCodeUrl { get; set; }
        /// <summary>
        /// 开通多门店最小数量限制
        /// </summary>
        public int MinCount { get { return SCount; } }

        public int year { get; set; } = 1;
        public int sumprice { get; set; }
        /// <summary>
        /// 行业类型
        /// </summary>
        public string industr { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public int buycount { get; set; } = 1;
        /// <summary>
        /// 多门店购买数量
        /// </summary>
        public int storecount { get; set; } = 0;
        /// <summary>
        /// 添加时间
        /// </summary>
        public string AddTimeStr { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public string outtime { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string statename { get; set; }
        /// <summary>
        /// 是否免费使用，0：付费，1：免费
        /// </summary>
        public int FreeUse { get; set; }
        /// <summary>
        /// 是否开启试用，0：不试用，1：试用
        /// </summary>
        public int TestUse { get; set; }

        /// <summary>
        /// 专业版版本级别 基础版=3; 高级版=2; 尊享版=1; 旗舰版=0
        /// </summary>
        public int VersionId { get; set; }

        ///<summary>
        /// 0:不限制添加数量，大于0：限制添加的数量
        /// </summary>		
        [SqlField]
        public int LimitCount { get; set; }

    }
    /// <summary>
    /// 代理商后台分销商管理--小程序模板购买明细
    /// </summary>
    public class XcxTemplateDetail
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 购买数量
        /// </summary>
        public int Count { get; set; } = 0;
        /// <summary>
        /// 购买总额
        /// </summary>
        public int sum { get; set; } = 0;

        /// <summary>
        /// 显示总额
        /// </summary>
        public string showsum { get; set; } = "0.00";
    }
}
