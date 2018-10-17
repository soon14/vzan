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
    public class Gw
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

            get { return Tag.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
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

        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;

        /// <summary>
        /// 所在页数
        /// </summary>
        [SqlField]
        public int Page { get; set; } = 1;



    }

    /// <summary>
    /// 91小程序实体数据集合
    /// </summary>
    public class List91
    {
        public bool isEnd = false;
        public List<Mini91Item> list = new List<Mini91Item>();

    }

    /// <summary>
    /// 91小程序单个实体
    /// </summary>
    public class Mini91Item
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string href { get; set; } = string.Empty;
        public string img { get; set; } = string.Empty;
        public string catId { get; set; } = string.Empty;
        public string cat { get; set; } = string.Empty;
    }





    /// <summary>
    /// 第9小程序 实体
    /// </summary>
    public class No9XiaoChenXun
    {
        public int status { get; set; } = 0;
        /// <summary>
        /// nomore表示没有数据 success 表示有数据
        /// </summary>
        public string info { get; set; } = string.Empty;
        public dataItem data { get; set; }
    }

    public class dataItem
    {
        public List<rowItem> rows { get; set; } = new List<rowItem>();
        public string total { get; set; } = string.Empty;
    }
    public class rowItem
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

    public class scoreItem
    {
        public string score { get; set; } = string.Empty;
        public int total { get; set; } = 0;
        public int num { get; set; } = 0;
        public string start1 { get; set; } = string.Empty;
        public string start2 { get; set; } = string.Empty;
        public string start3 { get; set; } = string.Empty;
        public string start4 { get; set; } = string.Empty;
        public string start5 { get; set; } = string.Empty;
        public double start_rate1 { get; set; } = 0;
        public double start_rate2 { get; set; } = 0;
        public double start_rate3 { get; set; } = 0;
        public double start_rate4 { get; set; } = 0;
        public double start_rate5 { get; set; } = 0;
    }




}
