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
    /// 同城用户信息表，不轻易使用该表的 自增长  Id 字段
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class C_UserInfo
    {
        public C_UserInfo() { }

        /// <summary>
        ///
        /// </summary>
        [SqlField(IsPrimaryKey =true,IsAutoId =true)]
        public int Id { get; set; }
        
        [SqlField]
        public string OpenId { get; set; }
        [SqlField]
        public string WzLiveOpenId { get; set; }
        [SqlField]
        public string MsgOpenId { get; set; }
        [SqlField]
        public string UnionId { get; set; }
        [SqlField]
        public string NickName { get; set; }
        [SqlField]
        public string HeadImgUrl { get; set; }

        [SqlField]
        public int Sex { get; set; }

        [SqlField]
        public string TelePhone { get; set; }
        /// <summary>
        /// 电话是否已验证
        /// </summary>
        [SqlField]
        public  int IsValidTelePhone { get; set; } =  0;
        [SqlField]
        public string Address { get; set; }
        [SqlField]
        public int AreaCode { get; set; }

        /// <summary>
        /// 个人中心版本   0个人中心 1商家中心
        /// C_Enums.UserCenterVersion
        /// </summary>
        [SqlField]
        public int StoreId { get; set; }

        /// <summary>
        /// 现金
        /// </summary>
        [SqlField(IsUpdateRemove = true)]
        public int Cash { get; set; } = 0;
        /// <summary>
        /// 历史金额
        /// </summary>
        [SqlField(IsUpdateRemove = true)]
        public int HistoryCash { get; set; } = 0;

        /// <summary>
        /// 小程序appId
        /// </summary>
        [SqlField]
        public string appId { get; set; }
        /// <summary>
        /// 直播间id
        /// </summary>
        [SqlField]
        public int zbSiteId { get; set; } = 0;

        /// <summary>
        /// 用户类型 见枚举UserType
        /// </summary>
        [SqlField]
        public int userType { get; set; } = 0;

        [SqlField]
        public DateTime AddTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 登陆验证码（缓存半天,Guid.NewGuid()）
        /// </summary>
        public string loginSessionKey { get; set; }
        /// <summary>
        /// 解密时获取的sessionke
        /// </summary>
        public string SessionKey { get; set; }

        /// <summary>
        /// 用户备注信息
        /// </summary>
        [SqlField]
        public string Remark { get; set; }
    }

    public class UserManage
    {
        /// <summary>
        /// ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImgUrl { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string state { get; set; } = "";
        /// <summary>
        /// 关注公众号
        /// </summary>
        public string follow { get; set; } = "";
        /// <summary>
        /// 禁止发帖
        /// </summary>
        public string Jgpost { get; set; } = "";
        public string OpenId { get; set; }
        /// <summary>
        /// 禁止抢红包
        /// </summary>
        public string redpacketblack { get; set; } = "";
        /// <summary>
        /// 身份
        /// </summary>
        public string role { get; set; } = "";
        /// <summary>
        /// 警告
        /// </summary>
        public string jg { get; set; } = "";

        /// <summary>
        /// 店员店铺地址
        /// </summary>
        public string StoreURL { get; set; } = "";
    }
}
