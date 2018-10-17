using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    public class ServerHost : WxJsonResult
    {
        public string action { get; set; }
        /// <summary>
        /// request合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> requestdomain { get; set; }
        /// <summary>
        ///  socket合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> wsrequestdomain { get; set; }
        /// <summary>
        /// uploadFile合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> uploaddomain { get; set; }
        /// <summary>
        /// downloadFile合法域名，当action参数是get时不需要此字段。
        /// </summary>
        public List<string> downloaddomain { get; set; }

        /// <summary>
        /// 程序业务域名
        /// </summary>
        public List<string> webviewdomain { get; set; }
    }
}
