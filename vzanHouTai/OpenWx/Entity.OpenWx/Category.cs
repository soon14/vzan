using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    /// <summary>
    /// 授权小程序帐号的可选类目
    /// </summary>
    public class Category:WxJsonResult
    {
        /// <summary>
        /// 可填选的类目列表
        /// </summary>
        public List<CategoryList> category_list { get; set; }
    }
    /// <summary>
    /// 可选类目
    /// </summary>
    public class CategoryList
    {
        /// <summary>
        /// 一级类目名称
        /// </summary>
        public string first_class { get; set; }
        /// <summary>
        /// 二级类目名称
        /// </summary>
        public string second_class { get; set; }
        /// <summary>
        /// 三级类目名称
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
    }
}
