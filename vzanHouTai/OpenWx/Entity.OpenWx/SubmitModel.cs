using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    /// <summary>
    /// 提交小程序数据
    /// </summary>
    public class SubmitModel
    {
        /// <summary>
        /// 提交审核项的一个列表（至少填写1项，至多填写5项）
        /// </summary>
        public List<ItemListModel> item_list { get; set; }
    }
    /// <summary>
    /// 提交审核项的一个列表（至少填写1项，至多填写5项）
    /// </summary>
    public class ItemListModel
    {
        /// <summary>
        /// 小程序的页面，可通过“获取小程序的第三方提交代码的页面配置”接口获得
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 小程序的标签，多个标签用空格分隔，标签不能多于10个，标签长度不超过20
        /// </summary>
        public string tag { get; set; }
        /// <summary>
        /// 一级类目名称，可通过“获取授权小程序帐号的可选类目”接口获得
        /// </summary>
        public string first_class { get; set; }
        /// <summary>
        /// 二级类目(同上)         
        /// </summary>
        public string second_class { get; set; }
        /// <summary>
        /// 三级类目(同上)
        /// </summary>
        public string third_class { get; set; }

        /// <summary>
        /// 一级类目的ID编号
        /// </summary>
        public int first_id { get; set; }
        /// <summary>
        ///  二级类目的ID编号
        /// </summary>
        public int second_id { get; set; }
        /// <summary>
        ///  三级类目的ID编号
        /// </summary>
        public int third_id { get; set; }
        /// <summary>
        /// 小程序页面的标题,标题长度不超过32
        /// </summary>
        public string title { get; set; }
    }
}
