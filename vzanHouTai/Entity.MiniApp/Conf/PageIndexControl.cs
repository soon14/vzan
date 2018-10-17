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
    public class PageIndexControl
    {

        public PageIndexControl() { }
        /// <summary>
        /// ID
        /// </summary>
        [SqlField(IsPrimaryKey = true, IsAutoId = true)]
        public int Id { get; set; }

        /// <summary>
        /// RelationId 权限表Id
        /// </summary>
        [SqlField]
        public int RelationId { get; set; } = 0;


        /// <summary>
        /// 页面标题
        /// </summary>
        [SqlField]
        public string PageTitle { get; set; } = string.Empty;

        /// <summary>
        /// 首页控件配置信息
        /// </summary>
        [SqlField]
        public string JsonMsg { get; set; } = string.Empty;


        /// <summary>
        /// 配置更新时间
        /// </summary>
        [SqlField]
        public DateTime UpdateDate { get; set; } = DateTime.Now;
    }


    /// <summary>
    /// 控件项目
    /// </summary>
    public class ControlItem
    {
        public string title = string.Empty;
        public string type { get; set; } = string.Empty;
        public List<object> items { get; set; }
        public int sys_order { get; set; } = 0;

    }




    /// <summary>
    /// 关注公众号组件
    /// </summary>
    public class OfficialAccountItem
    {
        /// <summary>
        /// 0 表示关闭,1表示开启
        /// </summary>
        public int open { get; set; } = 0;
    }


    /// <summary>
    /// 广告图控件
    /// </summary>
    public class ADItem
    {
        public string unitid { get; set; } = string.Empty;
    }


    /// <summary>
    /// 轮播图子控件
    /// </summary>
    public class bannerItem
    {
        public string imgurl { get; set; } = string.Empty;
        public string link { get; set; } = string.Empty;
        public int sort { get; set; } = 0;
    }


    /// <summary>
    /// 图片控件
    /// </summary>
    public class imgItem
    {
        public string imgPath { get; set; } = string.Empty;
    }

 
    /// <summary>
    /// 编辑器控件
    /// </summary>
    public class editorItem
    {
        public string txt { get; set; } = string.Empty;
    }

    /// <summary>
    /// 视频控件
    /// </summary>
    public class videoItem
    {
        public string videoUrl { get; set; } = string.Empty;
        public string videoCover { get; set; } = string.Empty;
        public int isAutoPlay { get; set; } = 0;
    }
    /// <summary>
    /// 背景音乐控件
    /// </summary>
    public class AudioItem
    {
        public string bmgPath { get; set; } = string.Empty;
       
    }

    /// <summary>
    /// 表单项
    /// </summary>
    public class formObjItem
    {
        public string title { get; set; } = string.Empty;
        public List<formItem> items { get; set; }
    }
    /// <summary>
    /// 表单子项目 输入框 时机选择框 单选框
    /// </summary>
    public class formItem
    {
        public string type { get; set; } = string.Empty;
        public string filedName { get; set; } = string.Empty;

        /// <summary>
        /// 在表单的索引位置
        /// </summary>
        public int sys_index { get; set; } = 0;
    }


    public class phoneItem
    {
        public string phoneTitle { get; set; } = string.Empty;
        public string phoneBtnTitle { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
      
    }


    /// <summary>
    /// 地图控件
    /// </summary>
    public class MapItem
    {
        public string mapAddr { get; set; } = string.Empty;
        public string mapLat { get; set; } = string.Empty;
        public string mapLng { get; set; } = string.Empty;
        public int zoom { get; set; } = 15;

    }


}
