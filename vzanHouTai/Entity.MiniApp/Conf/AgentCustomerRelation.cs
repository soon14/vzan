using Entity.Base;
using Entity.MiniApp;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 代理客户关系表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentCustomerRelation
    {
        /// <summary>
        /// Id
        /// </summary> 
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; }
        /// <summary>
        /// 代理id
        /// </summary>
        [SqlField]
        public int agentid { get; set; } = 0;
        /// <summary>
        /// 客户accountid
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
        public DateTime updatetime { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        [SqlField]
        public string username { get; set; } = string.Empty;
        /// <summary>
        /// 客户所在省份
        /// </summary>
        [SqlField]
        public int provincecode { get; set; } = 0;
        /// <summary>
        /// 客户所在城市
        /// </summary>
        [SqlField]
        public int citycode { get; set; } = 0;
        /// <summary>
        /// 客户所在地区
        /// </summary>
        [SqlField]
        public int areacode { get; set; } = 0;
        /// <summary>
        /// 客户所在行业
        /// </summary>
        [SqlField]
        public int industryid { get; set; } = 0;
        /// <summary>
        /// 客户企业规模
        /// </summary>
        [SqlField]
        public int companyscaleid { get; set; } = 0;
        /// <summary>
        /// 备注
        /// </summary>
        [SqlField]
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 账号状态 1开启 -1停用
        /// </summary>
        [SqlField]
        public int state { get; set; }
        /// <summary>
        /// 推广二维码ID
        /// </summary>
        [SqlField]
        public int QrcodeId { get; set; }
        /// <summary>
        /// 是否开启推广计划
        /// </summary>
        [SqlField]
        public int OpenExtension { get; set; }
    }
    /// <summary>
    /// 代理后台-用户管理-列表实体
    /// </summary>
    public class CustomerModel
    {
        public int id { get; set; }
        /// <summary>
        /// 代理id
        /// </summary>
        public int agentid { get; set; } = 0;
        /// <summary>
        /// 代理名称
        /// </summary>
        public string agentName { get; set; }
        /// <summary>
        /// 客户accountid
        /// </summary>
        public string useraccountid { get; set; } = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime addtime { get; set; }
        public string addtimeTostring { get; set; } = string.Empty;
        /// <summary>
        /// 登录账号
        /// </summary>
        public string LoginId { get; set; } = string.Empty;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string username { get; set; } = string.Empty;
        /// <summary>
        /// 账号状态
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 已开通模板名称
        /// </summary>
        public string templates { get; set; } = string.Empty;

        /// <summary>
        /// 模板总费用
        /// </summary>
        public int Totalcost { get; set; } = 0;
        /// <summary>
        /// 绑定手机号
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string pwd { get; set; }
        /// <summary>
        /// 已开通模板列表
        /// </summary>
        public List<XcxTemplate> sel_templateList { get; set; }
        /// <summary>
        /// 未开通模板列表
        /// </summary>
        public List<XcxTemplate> templateList { get; set; }
        /// <summary>
        /// 客户所在省份
        /// </summary>
        public int provincecode { get; set; } = 0;
        /// <summary>
        /// 客户所在城市
        /// </summary>
        public int citycode { get; set; } = 0;
        /// <summary>
        /// 客户所在地区
        /// </summary>
        public int areacode { get; set; } = 0;
        /// <summary>
        /// 客户所在行业
        /// </summary>
        public int industryid { get; set; } = 0;
        /// <summary>
        /// 客户企业规模
        /// </summary>
        public int companyscaleid { get; set; } = 0;
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; } = string.Empty;
        /// <summary>
        /// 购买模板价格
        /// </summary>
        public int price { get; set; } = 0;
        /// <summary>
        /// 显示价格
        /// </summary>
        public string showPrice { get; set; } = "0.00";
        /// <summary>
        /// 二维码SessionId
        /// </summary>
        public string SessionId { get; set; }

        public List<MsgAccount> MsgAccounts { get; set; }
    }

    
}
