using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Dish
{
    public class DishReturnMsg
    {
        /// <summary>
        /// 1：表示成功的
        /// 0：表示失败的
        /// 如需其他状态添加到下面
        /// </summary>
        public int code { get; set; } = 0;

        public string msg { get; set; } = string.Empty;

        public object obj { get; set; }
    }

    /// <summary>
    /// DishApi 使用的返回值对象
    /// </summary>
    public class DishApiReturnMsg
    {
        /// <summary>
        /// 0=失败,1=成功，2=登录失效或未登录,5=失败
        /// </summary>
        public int code { get; set; } = 0;
        
        /// <summary>
        /// 小程序端使用lodash.get获取，所有返回值都放到这个字段里
        /// </summary>
        public object info { get; set; }
    }
}
