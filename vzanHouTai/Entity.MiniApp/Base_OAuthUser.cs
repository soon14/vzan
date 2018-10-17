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
    /// --用户信息表实体类
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Base_OAuthUser
    {
        public Base_OAuthUser()
        { }
        #region Model
        private int _id;
        //private string _openid;
        private string _nickname = string.Empty;
        //private string _sex = string.Empty;
        //private string _province = string.Empty;
        //private string _city = string.Empty;
        //private string _country = string.Empty;
        private string _headimgurl = string.Empty;
        //private string _privilege = string.Empty;
        private int _state = 1;
        //private DateTime _createdate = DateTime.Now;
        //private int _minisnsId = 0;
        //private int _userlevel = 0;
        ////private int _Cash = 0;
        private int _AggregateScore = 0;
        //private int _integrateScore = 0;
        //private double _Longitude = 0d;//经度
        //private double _Latitude = 0d;//纬度
        //private string _email =string.Empty;
        //private string _phone = string.Empty;
        //private string _password = string.Empty;

        //[SqlField]
        //public string unionid { get; set; } = string.Empty;//微信用户Unionid
        //[SqlField]
        //public string msgopenid { get; set; } = string.Empty;//关注公众号消息推送OpenId

        //[SqlField]
        //public string msgtips { get; set; } = string.Empty;//消息提醒功能设置

        /// <summary>
        /// --用户的唯一标识
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 用户总积分
        /// </summary>
        [SqlField]
        public int AggregateScore
        {
            set { _AggregateScore = value; }
            get { return _AggregateScore; }
        }


        /// <summary>
        /// openid
        /// </summary>
        //[SqlField]
        //public string Openid
        //{
        //    set { _openid = value; }
        //    get { return _openid; }
        //}
        /// <summary>
        /// --用户昵称
        /// </summary>
        [SqlField]
        public string Nickname
        {
            set { _nickname = value; }
            get { return _nickname; }
        }
        /// <summary>
        /// --用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        /// </summary>
        //[SqlField]
        //public string Sex
        //{
        //    set { _sex = value; }
        //    get { return _sex; }
        //}
        /// <summary>
        /// -用户个人资料填写的省份
        /// </summary>
        //[SqlField]
        //public string Province
        //{
        //    set { _province = value; }
        //    get { return _province; }
        //}
        /// <summary>
        /// --用户个人资料填写的城市  
        /// </summary>
        //[SqlField]
        //public string City
        //{
        //    set { _city = value; }
        //    get { return _city; }
        //}
        /// <summary>
        /// --国家，如中国为CN
        /// </summary>
        //[SqlField]
        //public string Country
        //{
        //    set { _country = value; }
        //    get { return _country; }
        //}
        /// <summary>
        /// --用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空
        /// </summary>
        [SqlField]
        public string Headimgurl
        {
            set { _headimgurl = value; }
            get { return _headimgurl; }
        }
        /// <summary>
        ///  --用户特权信息，json 数组，如微信沃卡用户为（chinaunicom）其实这个格式称不上JSON，只是个单纯数组
        /// </summary>
        //[SqlField]
        //public string Privilege
        //{
        //    set { _privilege = value; }
        //    get { return _privilege; }
        //}
        /// <summary>
        ///  --状态
        /// </summary>
        [SqlField]
        public int State
        {
            set { _state = value; }
            get { return _state; }
        }
        /// <summary>
        /// --创建时间
        /// </summary>
        //[SqlField]
        //public DateTime CreateDate
        //{
        //    set { _createdate = value; }
        //    get { return _createdate; }
        //}

        /// <summary>
        /// 所属社区
        /// </summary>
        //[SqlField]
        //public int MinisnsId
        //{
        //    set { _minisnsId = value; }
        //    get { return _minisnsId; }
        //}

        /// <summary>
        /// 用户级别（1.管理员。0.普通会员）
        /// </summary>
        //[SqlField]
        //public int UserLevel
        //{
        //    set { _userlevel = value; }
        //    get { return _userlevel; }
        //}

        //private bool _sysnc = true;
        ///// <summary>
        ///// 是否需要同步
        ///// </summary>
        //[SqlField]
        //public bool Sysnc
        //{
        //    set { _sysnc = value; }
        //    get { return _sysnc; }
        //}

        /// <summary>
        /// 用户发帖的数量
        /// </summary>
        //[SqlField]
        //public int ArticleCount { get; set; } = 0;


        //[SqlField]
        //public int CommentCount { get; set; } = 0;
        /// <summary>
        /// 现金余额
        /// </summary>
        //[SqlField]
        //public int Cash
        //{
        //    get; set;
        //} = 0;
        /// <summary>
        /// 历史现金
        /// </summary>
        //[SqlField]
        //public int HistoryCash
        //{
        //    get; set;
        //} = 0;
        /// <summary>
        /// 现金余额
        /// </summary>
        //[SqlField]
        //public int CCash
        //{
        //    get; set;
        //} = 0;
        /// <summary>
        /// 历史现金
        /// </summary>
        //[SqlField]
        //public int CHistoryCash
        //{
        //    get; set;
        //} = 0;
        /// <summary>
        /// 历史打赏赏金
        /// </summary>
        //[SqlField]
        //public int OutCash
        //{
        //    get; set;
        //} = 0;
        /// <summary>
        /// 历史悬赏赏金
        /// </summary>
        //[SqlField]
        //public int GuerdonOutCash
        //{
        //    get; set;
        //} = 0;
        /// <summary>
        /// 用户评论的数量
        /// </summary>
        //public int PraiseCount { get; set; } = 0;
        //public bool IsWholeAdmin
        //{
        //    get; set;
        //} = false;

        /// <summary>
        /// 经度
        /// </summary>
        //public double Longitude
        //{
        //    get { return _Longitude; }
        //    set { _Longitude = value; }
        //}
        /// <summary>
        /// 纬度
        /// </summary>
        //public double Latitude
        //{
        //    get { return _Latitude; }
        //    set { _Latitude = value; }
        //}

        /// <summary>
        /// 用户积分
        /// </summary>
        //[SqlField]
        //public int IntegrateScore
        //{
        //    get{return _integrateScore;}
        //    set{_integrateScore = value;}
        //}

        /// <summary>
        /// 邮箱
        /// </summary>
        //[SqlField]
        //public string Email
        //{
        //    get{return _email;}
        //    set{_email = value;}
        //}

        /// <summary>
        /// 手机
        /// </summary>
        //[SqlField]
        //public string Phone
        //{
        //    get{return _phone;}
        //    set{_phone = value;}
        //}

        /// <summary>
        /// 密码
        /// </summary>
        //[SqlField]
        //public string Password
        //{
        //    get{return _password;}
        //    set{ _password = value;}
        //}

        //public bool IsSign { get; set; } = false;
        #endregion Model
    }
}
