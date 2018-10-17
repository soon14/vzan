using Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Entity.MiniApp.Home
{
    /// <summary>
    /// 点赞官网 第三方小程序数据
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class RangeGw
    {
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        ///  9表示数据来源于第9小程序 91表示来源于91小程序 
        /// </summary>
        [SqlField]
        public int Type { get; set; } = 9;

        /// <summary>
        /// 小程序应用Id 在各自应用市场的Id 唯一标示
        /// </summary>
        [SqlField]
        public int appId { get; set; } = 0;

        /// <summary>
        /// 小程序名称
        /// </summary>
        [SqlField]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 标签
        /// </summary>
        [SqlField]
        public string Tag { get; set; } = string.Empty;

        public string[] Tags
        {

            get { return Tag.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries); }
        }

        /// <summary>
        /// 星星个数 评分
        /// </summary>
        [SqlField]
        public string startNum { get; set; } = string.Empty;

        /// <summary>
        /// 浏览次数
        /// </summary>
        [SqlField]
        public string ViewNumbers { get; set; } = "0";
        /// <summary>
        /// 二维码图片
        /// </summary>
        [SqlField]
        public string Qrcode { get; set; } = string.Empty;

        /// <summary>
        /// 小程序Logo
        /// </summary>
        [SqlField]
        public string Logo { get; set; } = string.Empty;

        /// <summary>
        /// 添加时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 所在页数
        /// </summary>
        [SqlField]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 排名
        /// </summary>
        [SqlField]
        public int RangeId { get; set; } = 0;


        /// <summary>
        /// 更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 更新前的浏览次数
        /// </summary>
        [SqlField]
        public string LastViewNumbers { get; set; } = "0";

    }


    /// <summary>
    /// 第9小程序排行榜 实体
    /// </summary>
    public class No9XiaoChenXunRange
    {
        public int status { get; set; } = 0;
        /// <summary>
        /// nomore表示没有数据 success 表示有数据
        /// </summary>
        public string info { get; set; } = string.Empty;
        public dataItemRange data { get; set; }
    }

    public class dataItemRange
    {
        public List<rowItemRange> rows { get; set; } = new List<rowItemRange>();
        public string total { get; set; } = string.Empty;
    }
    public class rowItemRange
    {
        public string id { get; set; } = "0";
        public string cid { get; set; } = "0";
        public string cname { get; set; } = string.Empty;
        public string rid { get; set; } = string.Empty;
        public string uid { get; set; } = string.Empty;
        public string uname { get; set; } = string.Empty;
        public string listorder { get; set; } = string.Empty;
        public string timelimit { get; set; } = string.Empty;
        public string starttime { get; set; } = string.Empty;
        public string endtime { get; set; } = string.Empty;
        public string create_time { get; set; } = string.Empty;
        public string update_time { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public appItem app { get; set; }
        /// <summary>
        /// 排行榜名次
        /// </summary>
        public int range { get; set; } = 0;
    }


    public class appItem
    {
        public string id { get; set; } = "0";
        public string uid { get; set; } = "0";
        public string title { get; set; } = string.Empty;
        public string cat_ids { get; set; } = string.Empty;
        public List<string> tags { get; set; } = new List<string>();
        public string keyword { get; set; } = string.Empty;
        public string descript { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;
        public string attr_imgs { get; set; } = string.Empty;
        public string qrcode { get; set; } = string.Empty;
        public string open_qrcode { get; set; } = string.Empty;
        public string author { get; set; } = string.Empty;
        public string requires { get; set; } = string.Empty;
        public string hits { get; set; } = string.Empty;
        public string listorder { get; set; } = string.Empty;
        public string choice { get; set; } = string.Empty;
        public string recommend { get; set; } = string.Empty;
        public string show { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string release_time { get; set; } = string.Empty;
        public string create_time { get; set; } = string.Empty;
        public string static_cloud { get; set; } = string.Empty;
        public List<string> cat_name { get; set; } = new List<string>();
        public List<string> imgs_thumb { get; set; } = new List<string>();
        public scoreItem score { get; set; }
        public object my_score { get; set; } = null;
        public int is_cloud { get; set; } = 0;
        public string url { get; set; } = string.Empty;
    }




}
