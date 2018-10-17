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
    /// 代理商网站信息配置
    /// </summary>
    [Serializable]
    [SqlTable(dbEnum.MINIAPP)]
    public class AgentWebSiteInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }
        /// <summary>
        /// 代理accountid
        /// </summary>
        [SqlField]
        public string userAccountId { get; set; } = string.Empty;


        /// <summary>
        /// 代理商域名  以后支持绑定多个则每个域名以分号分割
        /// </summary>
        [SqlField]
        public string domian { get; set; } = string.Empty;
       

        /// <summary>
        /// 域名类型 0→自定义域名 1→小未程序二级域名
        /// </summary>
        [SqlField]
        public int domainType { get; set; } = 0;


        /// <summary>
        /// SEO配置信息
        /// </summary>
        [SqlField]
        public string seoConfig { get; set; } = string.Empty;
        /// <summary>
        /// SEO配置信息视图模型
        /// </summary>
        public SeoConfigModel seoConfigModel { get; set; } = new SeoConfigModel();

        /// <summary>
        /// 内容页面信息
        /// </summary>
        [SqlField]
        public string pageMsgConfig { get; set; } = string.Empty;

        /// <summary>
        /// 内容页面信息视图模型
        /// </summary>
        public PageMsgConfigModel pageMsgConfigModel { get; set; } = new PageMsgConfigModel();

        /// <summary>
        /// 创建时间
        /// </summary>
        [SqlField]
        public DateTime addtime { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改时间
        /// </summary>
        [SqlField]
        public DateTime updateitme { get; set; } = DateTime.Now;

        /// <summary>
        /// 网站状态 0→正常 -1停止
        /// </summary>
        [SqlField]
        public int webState { get; set; } = 0;


        /// <summary>
        /// 网站是否创建 0→没有 1已经创建
        /// </summary>
        [SqlField]
        public int isCreate { get; set; } = 0;


    }


    /// <summary>
    /// 代理商页面Seo信息配置
    /// </summary>
    public class SeoConfigModel
    {
        /// <summary>
        /// 公司Logo
        /// </summary>
        public string logo { get; set; } = string.Empty;

        /// <summary>
        /// icon
        /// </summary>
        public string iconImg { get; set; } = string.Empty;

        /// <summary>
        /// 公司名称
        /// </summary>
        public string companyName { get; set; } = string.Empty;
        /// <summary>
        /// 网站描述也就是页面描述
        /// </summary>
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// 网站关键词优化 可优化搜索引擎结果，请用英文逗号","分隔
        /// </summary>
        public string keyWords { get; set; } = string.Empty;

        /// <summary>
        /// 网站备案号
        /// </summary>
        public string ICPNumber { get; set; } = string.Empty;

    }


    /// <summary>
    /// 页面内容信息配置模型
    /// </summary>
    public class PageMsgConfigModel
    {
        /// <summary>
        /// 轮播图每张图片以逗号分隔
        /// </summary>
        public string bannerImgs { get; set; } = string.Empty;

        /// <summary>
        /// 移动端轮播图每张图片以逗号分隔
        /// </summary>
        public string MobileBannerImgs { get; set; } = string.Empty;

        /// <summary>
        /// 视频开关
        /// </summary>
        public bool videoSwitch { get; set; } = false;

        /// <summary>
        /// 自定义模块列表
        /// </summary>
        public List<CustomModel> listCustomModel { get; set; } = new List<CustomModel>();

        /// <summary>
        /// 联系电话
        /// </summary>
        public string telephone { get; set; } = string.Empty;
        /// <summary>
        /// 联系邮箱
        /// </summary>
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// 联系地址
        /// </summary>
        public string address { get; set; } = string.Empty;
    }



    /// <summary>
    /// 自定义模块
    /// </summary>
    public class CustomModel
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public string modelName { get; set; } = string.Empty;
        /// <summary>
        /// 模块描述
        /// </summary>
        public string modelDescription { get; set; } = string.Empty;
        /// <summary>
        /// 模块展示图 如果后期改完多图则以逗号分隔即可
        /// </summary>
        public string modelBanners { get; set; } = string.Empty;
        /// <summary>
        /// 模块内容
        /// </summary>
        public string modelContent { get; set; } = string.Empty;
    }








}
