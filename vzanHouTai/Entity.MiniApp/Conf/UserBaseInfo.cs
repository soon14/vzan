using Entity.Base;
using System;
using Utility;

namespace Entity.MiniApp.Conf
{
    /// <summary>
    /// 用户基础表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class UserBaseInfo
    {

        public UserBaseInfo() { }

        public UserBaseInfo(WxUser _wxUser)
        {
            openid = _wxUser.openid;
            unionid = _wxUser.unionid;
            headimgurl = _wxUser.headimgurl;
            nickname = _wxUser.nickname;
            sex = _wxUser.sex.ToString();
            country = _wxUser.country;
            city = _wxUser.city;
            province = _wxUser.province;
            addtime = DateTime.Now;
            serverid = _wxUser.serverid;
    }

        /// <summary>
        /// 用户Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 用户的唯一标识
        /// </summary>
        [SqlField]
        public string openid { get; set; } = string.Empty;

        /// <summary>
        /// 用户昵称
        /// </summary>
        [SqlField]
        public string nickname { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        [SqlField]
        public string sex { get; set; } = "0";

        /// <summary>
        /// 省份
        /// </summary>
        [SqlField]
        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 城市
        /// </summary>
        [SqlField]
        public string city { get; set; } = string.Empty;

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
        /// unionid
        /// </summary>
        [SqlField]
        public string unionid { get; set; } = string.Empty;

        /// <summary>
        /// 消息提醒OpenId
        /// </summary>
        [SqlField]
        public string msgopenId { get; set; } = string.Empty;

        /// <summary>
        /// 暂时用不到
        /// </summary>
        [SqlField]
        public bool state { get; set; } = true;

        /// <summary>
        /// 公众号openid
        /// </summary>
        [SqlField]
        public string openid1 { get; set; } = string.Empty;

        /// <summary>
        /// 公众号openid
        /// </summary>
        [SqlField]
        public string openid2 { get; set; } = string.Empty;

        /// <summary>
        /// 公众号openid
        /// </summary>
        [SqlField]
        public string openid3 { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 用户组ID
        /// </summary>
        [SqlField]
        public int groupid { get; set; }
        /// <summary>
        /// 用户获取用户信息时判断是否关注
        /// </summary>
        [SqlField]
        public int subscribe { get; set; }
        /// <summary>
        /// 公众号id
        /// </summary>
        [SqlField]
        public string serverid { get; set; } = string.Empty;
    }
}
