using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 用户收货地址
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class UserAddress
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int id { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>
        [SqlField]
        public int userid { get; set; } = 0;

        /// <summary>
        /// 是否默认
        /// </summary>
        [SqlField]
        public int isdefault { get; set; } = 0;

        /// <summary>
        /// 联系人
        /// </summary>
        [SqlField]
        public string contact { get; set; } = string.Empty;

        /// <summary>
        /// 手机
        /// </summary>
        [SqlField]
        public string phone { get; set; } = string.Empty;

        /// <summary>
        /// 省
        /// </summary>
        [SqlField]
        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 市
        /// </summary>
        [SqlField]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 区/县
        /// </summary>
        [SqlField]
        public string district { get; set; } = string.Empty;

        /// <summary>
        /// 街道/详细地址
        /// </summary>
        [SqlField]
        public string street { get; set; } = string.Empty;

        /// <summary>
        /// 邮编
        /// </summary>
        [SqlField]
        public string zipcode { get; set; } = string.Empty;
        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updatetime { get; set; }
    }
}
