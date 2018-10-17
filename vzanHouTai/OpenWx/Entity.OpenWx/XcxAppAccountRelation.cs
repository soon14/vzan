using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.OpenWx
{
    /// <summary>
    /// 小程序与用户关联表(权限表)
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class XcxAppAccountRelation
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 小程序模板Id
        /// </summary>
        [SqlField]
        public int TId { get; set; }
        /// <summary>
        ///  用户表Id
        /// </summary>
        [SqlField]
        public Guid AccountId { get; set; }
        /// <summary>
        /// 小程序AppId
        /// </summary>
        [SqlField]
        public string AppId { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 跳转链接
        /// </summary>
        [SqlField]
        public string Url { get; set; } = string.Empty;
        /// <summary>
        /// 使用时长（单位：年）
        /// </summary>
        [SqlField]
        public int TimeLength { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        [SqlField]
        public string Desc { get; set; } = string.Empty;
        /// <summary>
        /// 状态：默认0,-2：删除
        /// </summary>
        [SqlField]
        public int State { get; set; }
        [SqlField]
        public int agentId { get; set; }
        [SqlField]
        public int price { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [SqlField]
        public DateTime outtime { get; set; }
        /// <summary>
        /// 门店数量
        /// </summary>
        [SqlField]
        public int SCount { get; set; }

        /// <summary>
        /// 小程序模板名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 行业类型
        /// </summary>
        [SqlField]
        public string Industr { get; set; } = string.Empty;


        /// <summary>
        /// 版本级别Id 基础版 3  高级版 2 尊享版1 旗舰版0
        /// </summary>
        [SqlField]
        public int VersionId { get; set; } = 0;

        /// <summary>
        /// 商家版小程序对应userid
        /// </summary>
        [SqlField]
        public int storeMasterId { get; set; } = 0;

        /// <summary>
        /// 是否是体验版
        /// </summary>
        [SqlField]
        public bool IsExperience { get; set; } = false;

        /// <summary>
        /// 登陆该模板的密码
        /// </summary>
        [SqlField]
        public string Password { get; set; }
        /// <summary>
        /// 上传代码类型：0：手动上传，1：第三方上传
        /// </summary>
        [SqlField]
        public int AuthoAppType { get; set; }
        /// <summary>
        /// 第三方类型，0：第一个第三方平台，1：第二个第三方平台
        /// </summary>
        [SqlField]
        public int ThirdOpenType { get; set; } = 0;
        /// <summary>
        /// 小程序类型
        /// </summary>
        public int Type { get; set; }

        public string LoginId { get; set; }

        public string Phone { get; set; }
        /// <summary>
        /// 小程序名称
        /// </summary>
        public string XcxName { get; set; }
        /// <summary>
        /// 小程序上传状态 XcxTypeEnum枚举
        /// </summary>
        public int UploadState { get; set; }
        /// <summary>
        /// 小程序上传状态名称
        /// </summary>
        public string UploadStateName { get; set; }
    }

    public class XcxAppAccountRelationGroupInfo
    {
        public string TName { get; set; }
        public int TId { get; set; }
        public int Count { get; set; }
    }
}
