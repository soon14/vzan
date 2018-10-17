using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    public class ReturnMsg
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

}
