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
    /// 获取直播话题浏览量接口返回值表单
    /// </summary>
    public class VideoParam
    {
        public bool isok { get; set; }
        public string Msg { get; set; }
        public string code { get; set; }
        public List<TpcData> dataObj { get; set; }
    }

    public class TpcData
    {
        /// <summary>
        /// 直播话题ID
        /// </summary>
        public int tpid { get; set; }
        /// <summary>
        /// 浏览量
        /// </summary>
        public int viewcts { get; set; }
    }
}
