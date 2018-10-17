using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp
{
    [Serializable]
    [SqlTable(dbEnum.QLWL)]
    public class Minisns
    {
        public Minisns()
        { }
        #region Model
        private int _id;
        private Guid _accountId = new Guid();
        private string _logoUrl = string.Empty;
        private string _name = string.Empty;
        private string _notice = string.Empty;
        private int _state = 1;
        private DateTime _createdate = DateTime.Now;
        private string _appID = string.Empty;
        private string _appsecret = string.Empty;
        private int _TodayMaxScore = 0;
        private int _watermark = 0;

        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 论坛每日最多可得分数
        /// </summary>
        [SqlField]
        public int TodayMaxScore
        {
            set { _TodayMaxScore = value; }
            get { return _TodayMaxScore; }
        }

        /// <summary>
        /// 论坛水印  0关闭 1图片水印 2文字水印
        /// </summary>
        [SqlField]
        public int watermark
        {
            set { _watermark = value; }
            get { return _watermark; }
        }


        /// <summary>
        /// 对应 Account 表 ID
        /// </summary>
        [SqlField]
        public Guid AccountId
        {
            set { _accountId = value; }
            get { return _accountId; }
        }

        /// <summary>
        /// logo图片地址
        /// </summary>
        [SqlField]
        public string LogoUrl
        {
            set { _logoUrl = value; }
            get { return _logoUrl; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Name
        {
            set { _name = value; }
            get { return _name; }
        }

        /// <summary>
        /// 简介
        /// </summary>
        [SqlField]
        public string Notice
        {
            set { _notice = value; }
            get { return _notice; }
        }

        /// <summary>
        /// 状态 3代表付费阅读审核通过，2代表付费阅读不通过
        /// </summary>
        [SqlField]
        public int State
        {
            set { _state = value; }
            get { return _state; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime CreateDate
        {
            set { _createdate = value; }
            get { return _createdate; }
        }

        /// <summary>
        /// 公众号APPID
        /// </summary>
        [SqlField]
        public string appID
        {
            set { _appID = value; }
            get { return _appID; }
        }

        /// <summary>
        /// 公众号密钥
        /// </summary>
        [SqlField]
        public string appsecret
        {
            set { _appsecret = value; }
            get { return _appsecret; }
        }

        private int _clickcount = 0;
        /// <summary>
        /// 浏览数
        /// </summary>
        [SqlField]
        public int ClickCount
        {
            set { _clickcount = value; }
            get { return _clickcount; }
        }
        private string _forumDomain = string.Empty;
        /// <summary>
        /// 付费用户的独立域名
        /// </summary>
        [SqlField]
        public string forumDomain
        {
            get
            {
                if (string.IsNullOrEmpty(_forumDomain))
                {
                    _forumDomain =WebSiteConfig.DefaultForumDomain;

                }
                return _forumDomain;
            }
            set { _forumDomain = value; }
        }

        /// <summary>
        /// 是否参与互推  xxxxxx 去掉互推功能，用于论坛推送功能（==2 的时候）
        /// </summary>
        [SqlField]
        public int IsPush { get; set; } = 0;

        /// <summary>
        /// 吸引关注的图片
        /// </summary>
        [SqlField]
        public string AttentionImage { get; set; } = string.Empty;

        /// <summary>
        /// 关注的图片二维码
        /// </summary>
        [SqlField]
        public string QRCodeImage { get; set; } = string.Empty;

        /// <summary>
        /// 聊天站点ID
        /// </summary>
        [SqlField]
        public string SiteId { get; set; } = string.Empty;

        /// <summary>
        /// 是否开通聊天
        /// </summary>
        [SqlField]
        public int IsChat { get; set; } = 1;

        /// <summary>
        /// 排序方式：1，按发帖排序；2，按回帖时间排序,默认发帖排序
        /// </summary>
        [SqlField]
        public int OrderRule { get; set; } = 2;

        /// <summary>
        /// 显示时间类型：1、显示发帖时间；2、显示回帖时间，默认发帖时间
        /// </summary>
        [SqlField]
        public int ShowTimeType { get; set; } = 2;

        /// <summary>
        /// 时间显示方式：1，相对时间,2，绝对时间。默认相对时间
        /// </summary>
        [SqlField]
        public int ShowTimeRule { get; set; } = 1;
        /// <summary>
        /// 是否需要审核
        /// </summary>
        [SqlField]
        public int IsAudit { get; set; } = 0;
        /// <summary>
        /// 论坛现金
        /// </summary>
        [SqlField(IsUpdateRemove = true)]
        public int Cash { get; set; } = 0;
        /// <summary>
        /// 论坛历史现金
        /// </summary>
        [SqlField(IsUpdateRemove = true)]
        public int HistoryCash { get; set; } = 0;
        /// <summary>
        /// 是否开启打赏
        /// </summary>
        [SqlField]
        public int IsReward { get; set; } = 1;
        #region 时间显示

        /// <summary>
        /// 开通模板定制功能
        /// </summary>
        [SqlField]
        public string AnnouncementText { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [SqlField]
        public string AnnouncementLink { get; set; } = string.Empty;

        /// <summary>
        /// 发帖规则（1.强制选版块，0.无限制）
        /// </summary>
        [SqlField]
        public int PostRule { get; set; } = 0;

        /// <summary>
        /// 首页显示帖子规则，1.热帖 0.全部
        /// </summary>
        [SqlField]
        public int ShowArticleType { get; set; } = 0;
        /// <summary>
        /// 赏金提成比率（0~20）
        /// </summary>
        [SqlField]
        public int RewardPercent { get; set; } = 20;

        /// <summary>
        ///是否开通客服，1.开 0.关
        /// </summary>
        [SqlField]
        public int KFStatus { get; set; } = 0;
        /// <summary>
        ///是否开通悬赏
        /// </summary>
        [SqlField]
        public int IsGuerdon { get; set; } = 1;
        /// <summary>
        ///最低悬赏金额
        /// </summary>
        [SqlField]
        public int GuerdonLowestMoney { get; set; } = 100;
        /// <summary>
        ///提现用户ID
        /// </summary>
        [SqlField]
        public int DrawUserId { get; set; } = 0;


        /// <summary>
        /// 首页背景图
        /// </summary>
        [SqlField]
        public string BackMap { get; set; } = "http://j.vzan.cc/content/images/wxjt_02.jpg";

        //public ShowTimeRule ShowTimeRuleE
        //{
        //    get
        //    {
        //        if (ShowTimeRule == 2)
        //        {
        //            return Entity.MiniSNS.ShowTimeRule.Absolute;
        //        }
        //        else
        //        {
        //            return Entity.MiniSNS.ShowTimeRule.Relative;
        //        }
        //    }
        //}
        //public ShowTimeType ShowTimeTypeE
        //{
        //    get
        //    {
        //        if (ShowTimeType == 1)
        //        {
        //            return Entity.MiniSNS.ShowTimeType.CreaterTime;
        //        }
        //        else
        //        {
        //            return Entity.MiniSNS.ShowTimeType.ReplyTime;
        //        }
        //    }
        //}
        //public OrderRule OrderRuleE
        //{
        //    get
        //    {
        //        if (OrderRule == 1)
        //        {
        //            return Entity.MiniSNS.OrderRule.CreaterTime;
        //        }
        //        else
        //        {
        //            return Entity.MiniSNS.OrderRule.ReplyTime;
        //        }
        //    }
        //}
        /// <summary>
        ///发帖、评论时间间隔
        /// </summary>
        [SqlField]
        public int IsPubPay { get; set; } = 0;//发帖、评论时间间隔


        #region 不用了，拿掉这个功能 xxxxxxxxxx
        /// <summary>
        /// 同城c_cityinfo的AreaCode
        /// </summary>
        [SqlField]
        public double mprices { get; set; } = 0d;

        //论坛其他相关阅读配置
        [SqlField]
        public double dprices { get; set; }

        /// <summary>
        ///  论坛的用户总数量
        /// </summary>
        [SqlField]
        public double hprices { get; set; } = 0d;
        #endregion
        /// <summary>
        /// 是否开启用户付费发帖
        /// </summary>
        [SqlField]
        public bool IsOpenPayRead { get; set; } = false;
        [SqlField]
        public int ClassifyId { get; set; }
        public int ClassifyPId { get; set; }
        public int ClassifyPPId { get; set; }
        public string ClassifyText { get; set; }
        #endregion
        #endregion Model

        /// <summary>
        /// 应用Id
        /// </summary>
        [SqlField]
        public int applyId { get; set; } = 1;
        public int IsOpenBinding { get; set; } = 0;
        public int TotalCashLog { get; set; } = 0;//交易总金额
        /// <summary>
        /// 论坛类型，公开，加密，付费
        /// </summary>
        [SqlField]
        public int Type { get; set; }

        /// <summary>
        /// 是否能被授权打赏功能 ,读取配置中的 NearTypeId 字段 
        /// </summary>
        public int IsAuthReward { get; set; } = 0;
        public string GzhName { get; set; } = string.Empty;
        public string GzhId { get; set; } = string.Empty;
        public string GzhImg { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 公众号资料修改时间
        /// </summary>
        public DateTime EditTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 公众号认证状态
        /// </summary>
        public int GZHState { get; set; } = 0;
        public int LiveEntranceType { get; set; }
    }
}
