using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace User.MiniApp.Areas.Qiye.Model
{
    public class OfficialWebIndexModel
    {
        public int AId { get; set; }
        /// <summary>
        ///企业名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 企业简述
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 企业介绍
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 形象大图
        /// </summary>
        public string ImgUrls { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 企业地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 底部图片
        /// </summary>
        public string BottomImgUrl { get; set; }
    }
}