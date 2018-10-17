using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.User
{
    /// <summary>
    /// 关注用户表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class WxUserinfo
    {
        public WxUserinfo() { }

        /// <summary>
        /// 主键
        /// </summary>
        [SqlField(IsAutoId = true, IsPrimaryKey = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 是否关注
        /// </summary>
        [SqlField]
        public int subscribe { get; set; } = 0;

        /// <summary>
        /// 获取用户openid
        /// </summary>
        [SqlField]
        public string openid { get; set; } = string.Empty;

        /// <summary>
        /// 获取用户unionid
        /// </summary>
        [SqlField]
        public string unionid { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        [SqlField]
        public string nickname { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        [SqlField]
        public int sex { get; set; } = 0;

        /// <summary>
        /// 城市
        /// </summary>
        [SqlField]
        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 省份
        /// </summary>
        [SqlField]
        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 国家
        /// </summary>
        [SqlField]
        public string country { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        [SqlField]
        public string headimgurl { get; set; } = string.Empty;

        /// <summary>
        /// 关注时间
        /// </summary>
        [SqlField]
        public string subscribe_time { get; set; } = string.Empty;

        /// <summary>
        /// 公众号Id
        /// </summary>
        [SqlField]
        public string serverid { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

    }
}