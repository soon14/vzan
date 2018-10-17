using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Conf
{
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]

    ///小程序首页控件配置
    public class PageShare
    {

        public PageShare() { }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 小程序Id
        /// </summary>
        [SqlField]
        public int RelationId { get; set; } = 0;


        /// <summary>
        /// 分享标题
        /// </summary>
        [SqlField]
        public string ShareTitle { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        [SqlField]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Logo
        /// </summary>
        [SqlField]
        public string Logo { get; set; } = string.Empty;


        /// <summary>
        /// 摘要
        /// </summary>
        [SqlField]
        public string Remark { get; set; } = string.Empty;


        /// <summary>
        /// 广告图片
        /// </summary>
        [SqlField]
        public string ADImg { get; set; } = string.Empty;

        /// <summary>
        /// 是否开启分享
        /// </summary>
        [SqlField]
        public int IsOpen { get; set; } = 0;

        /// <summary>
        /// 配置更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// 小程序码路径
        /// </summary>
        [SqlField]
        public string QrCodeImg { get; set; } = string.Empty;

        /// <summary>
        /// 小程序自定义分享卡片路径
        /// </summary>
        [SqlField]
        public string ShareImg { get; set; } = string.Empty;

    }

    public class XCXResultBaseModel
    {
        public int isok { get; set; } = 0;
        public string msg { get; set; } = string.Empty;

        /// <summary>
        /// 日期
        /// </summary>
        public string ref_date { get; set; }
        
    }

    public class GetAccessTokenMsg : XCXResultBaseModel
    {
        public string token { get; set; }
        public AccessTokenModel obj { get; set; }
    }

    public class AccessTokenModel
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int isok { get; set; }
        public string msg { get; set; }
        public int errcode { get; set; }
        public string errmsg { get; set; }

    }

    public class XCXDataModel<T>
    {
        public List<T> list { get; set; }
    }

    /// <summary>
    /// 概括数据
    /// </summary>
    public class GKResultModel : XCXResultBaseModel
    {
        /// <summary>
        /// 累计用户数
        /// </summary>
        public string visit_total { get; set; }
        /// <summary>
        /// 转发次数
        /// </summary>
        public string share_pv { get; set; }
        /// <summary>
        /// 转发人数
        /// </summary>
        public string share_uv { get; set; }
    }
    /// <summary>
    /// 访问数据
    /// </summary>
    public class FWResultModel : XCXResultBaseModel
    {
        /// <summary>
        /// 打开次数
        /// </summary>
        public string session_cnt { get; set; }
        /// <summary>
        /// 访问次数
        /// </summary>
        public string visit_pv { get; set; }
        /// <summary>
        /// 访问人数
        /// </summary>
        public string visit_uv { get; set; }
        /// <summary>
        /// 新用户数
        /// </summary>
        public string visit_uv_new { get; set; }
        /// <summary>
        /// 人均停留时长 (浮点型，单位：秒)
        /// </summary>
        public string stay_time_uv { get; set; }
        /// <summary>
        /// 次均停留时长 (浮点型，单位：秒)
        /// </summary>
        public string stay_time_session { get; set; }
        /// <summary>
        /// 平均访问深度 (浮点型)
        /// </summary>
        public string visit_depth { get; set; }
    }
}
