using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    /// <summary>
    /// 基类序列化
    /// </summary>
    [Serializable]
    public class M_Base
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string types = string.Empty;      
   
    }

    public class Return_Msg
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
        public string code { get; set; } = string.Empty;

        /// <summary>
        /// 数据
        /// </summary>
        public object dataObj { get; set; }
    }

    public class Result_Msg
    {             

        /// <summary>
        /// 返回消息
        /// </summary>
        public string msg { get; set; } = string.Empty;

        /// <summary>
        /// 返回错误编码
        /// </summary>
        public int err { get; set; } = 0;

        /// <summary>
        /// 数据
        /// </summary>
        public int Id { get; set; } = 0;
    }
}
