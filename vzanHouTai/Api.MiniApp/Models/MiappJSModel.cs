using Entity.MiniApp.Conf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.MiniApp.Models
{
    public class MiappJSModel<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<T> data { get; set; }
    }

    /// <summary>
    /// 分类信息筛选条件  出售，求购，
    /// </summary>
    public class PostTypeFilter
    {
        public int TypeValue { get; set; }

        public string TypeText { get; set; }
    }
}