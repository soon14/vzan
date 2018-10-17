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
    /// 代理信息表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class Agentinfo
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 代理accountid
        /// </summary>
        [SqlField]
        public string useraccountid { get; set; } = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updateitme { get; set; }

        /// <summary>
        /// 预存款
        /// </summary>
        [SqlField]
        public int deposit { get; set; } = 0;

        /// <summary>
        /// 用户状态 -2:逻辑删除,-1:停用，0:（暂未使用）,1:正常启用
        /// </summary>
        [SqlField]
        public int state { get; set; }

        /// <summary>
        /// 代理等级 0:代理商，1:分销代理商
        /// </summary>
        [SqlField]
        public int userLevel { get; set; } = 0;

        /// <summary>
        /// 代理名称
        /// </summary>
        [SqlField]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 代理配置
        /// </summary>
        [SqlField]
        public string configjson { get; set; } = string.Empty;
        /// <summary>
        /// 授权码（ID+注册日期）
        /// </summary>
        [SqlField]
        public string AuthCode { get; set; } = string.Empty;
        /// <summary>
        /// 代理分销规则ID
        /// </summary>
        [SqlField]
        public int AgentDistributionRuleId { get; set; } = 0;
        /// <summary>
        /// 最后一次预存款
        /// </summary>
        [SqlField]
        public int LastDeposit { get; set; } = 0;
        /// <summary>
        /// 是否开启分销代理，0：否，1：是
        /// </summary>
        public int IsOpenDistribution { get; set; } = 1;
        /// <summary>
        /// 开启分销管理
        /// </summary>
        [SqlField]
        public bool OpenFenXiao { get; set; } = false;
        /// <summary>
        /// 代理类型，0：普通代理，1：特别代理（只能开通免费版）
        /// </summary>
        [SqlField]
        public int AgentType { get; set; } = 0;
        /// <summary>
        /// 进入代理后台的密码，默认不用密码
        /// </summary>
        [SqlField]
        public string Password { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [SqlField]
        public DateTime OutTime { get; set; }
        public string OutTimeStr { get { return OutTime.ToString("yyyy-MM-dd HH:mm:ss"); } }
        /// <summary>
        /// 另外指定代理可以开通的模板类型id
        /// </summary>
        [SqlField]
        public string TemplateTypeId { get; set; }
        
        /// <summary>
        /// 分销代理是否可以继续添加分销商
        /// </summary>
        public bool CanOpenDistribution { get; set; }


        public int IsOpenAdv { get; set; } = 0;
        /// <summary>
        /// 是否是代理分销
        /// </summary>
        public bool IsAgentDistribution { get; set; }
        
        /// <summary>
        /// 消费总额
        /// </summary>
        public string sumcost { get; set; }


        /// <summary>
        /// 代理商网站Id
        /// </summary>
        public int webSiteId { get; set; } = 0;

        /// <summary>
        /// 网站状态 0→正常 -1停止
        /// </summary>
        public int webState { get; set; }
        public string WebStateStr
        {
            get
            {
                switch (webState)
                {
                    case 0: return "正常";
                    case -1: return "已经关闭";
                    default: return "未进行配置绑定";
                }
            }
        }

        /// <summary>
        /// 代理商域名  以后支持绑定多个则每个域名以分号分割
        /// </summary>
        public string domian { get; set; }


        /// <summary>
        /// 域名类型 0→自定义域名 1→小未程序二级域名
        /// </summary>
        public int domainType { get; set; }
        public string DomainTypeStr
        {
            get
            {
                switch (domainType)
                {
                    case 0: return "自定义域名";
                    case 1: return "二级域名";
                    default: return "未进行配置绑定";
                }
            }

        }

        public string loginid { get; set; }
        public string phone { get; set; }

        public string username { get; set; } = string.Empty;

        public string parentAgent { get; set; } = string.Empty;

        /// <summary>
        /// 代理区域数量
        /// </summary>
        public int areacount { get; set; } = 0;
        /// <summary>
        /// 代理区域名
        /// </summary>
        public string areaname { get; set; }
    }

    public class AgentConfig
    {
        /// <summary>
        /// 是否开启水印
        /// </summary>
        public int IsOpenAdv { get; set; } = 0;
        /// <summary>
        /// 标题
        /// </summary>
        public string LogoText { get; set; } = string.Empty;
        /// <summary>
        /// 标题
        /// </summary>
        public string LogoTitle { get; set; } = string.Empty;
        /// <summary>
        /// 域名
        /// </summary>
        public string LogoHost { get; set; } = string.Empty;
        /// <summary>
        /// 水印图片
        /// </summary>
        public string LogoImgUrl { get; set; } = string.Empty;
        public int isdefaul { get; set; } = 0;
        /// <summary>
        /// 是否开启推广计划
        /// </summary>
        public int OpenExtension { get; set; } = 0;
        /// <summary>
        /// 推广二维码ID
        /// </summary>
        public int QrcodeId { get; set; } = 0;
        /// <summary>
        /// 是否开启免费版可以自定义水印
        /// </summary>
        public int OpenFeeShuiying { get; set; } = 0;
    }
}
