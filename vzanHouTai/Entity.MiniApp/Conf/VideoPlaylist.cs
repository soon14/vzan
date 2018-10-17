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
    /// 加密播放视频列表
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class VideoPlayList
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        [SqlField]
        public int VideoId { get; set; } = 0;

        /// <summary>
        /// 密码
        /// </summary>
        [SqlField]
        public string Password { get; set; } = string.Empty;

        public bool IsPassword { get; set; } = false;

        [SqlField]
        public string Desc { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 播放地址
        /// </summary>
        [SqlField]
        public string PlayUrl { get; set; } = string.Empty;

        /// <summary>
        /// 组ID
        /// </summary>
        [SqlField]
        public int GroupId { get; set; } = 0;

        /// <summary>
        /// 排序
        /// </summary>
        [SqlField]
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 标题
        /// </summary>
        [SqlField]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 封面
        /// </summary>
        [SqlField]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// 状态：0：正常，-1：删除
        /// </summary>
        [SqlField]
        public int State { get; set; } = 0;

        /// <summary>
        /// 类型：0：公开课，1：内部培训
        /// </summary>
        [SqlField]
        public int TypeId { get; set; } = 0;

        /// <summary>
        /// 主讲人名称
        /// </summary>
        [SqlField]
        public string UserName { get; set; } = string.Empty;
        /// <summary>
        /// 视频开播时间
        /// </summary>
        [SqlField]
        public DateTime PlayTime { get; set; }
        /// <summary>
        /// 视频开播时间
        /// </summary>
        public string PlayTimeStr { get { return PlayTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        /// 播放浏览量
        /// </summary>
        [SqlField]
        public int playcount { get; set; }
        public string playcountformat { get { return (playcount >= 10000 ? (playcount / 10000.0).ToString("0.00").TrimEnd('0').TrimEnd('.')+"万" : playcount.ToString()); } }
        /// <summary>
        /// 直播ID
        /// </summary>
        [SqlField]
        public int zbid { get; set; }
        /// <summary>
        /// 0：入门必读，1：关于小程序，2：选择开发商必读，3：深度解读，4：代理商必读
        /// </summary>
        [SqlField]
        public int Theme { get; set; }
        public string ThemeName { get; set; }
    }
}
