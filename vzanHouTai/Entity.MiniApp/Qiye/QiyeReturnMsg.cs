using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp.Qiye
{
    public class QiyeReturnMsg
    {
        /// <summary>
        /// 状态
        /// </summary>
        public bool isok { get; set; } = false;

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Msg { get; set; } = string.Empty;

        /// <summary>
        /// 返回错误编码
        /// </summary>
        public string code { get; set; } = "200";

        /// <summary>
        /// 数据
        /// </summary>
        public object dataObj { get; set; }
    }

    /// <summary>
    /// QiyeApi 使用的返回值对象
    /// </summary>
    public class QiyeApiReturnMsg
    {
        /// <summary>
        /// 0=失败,1=成功，2=登录失效或未登录
        /// </summary>
        public int code { get; set; } = 0;

        /// <summary>
        /// 小程序端使用lodash.get获取，所有返回值都放到这个字段里
        /// </summary>
        public object info { get; set; }
    }
}
