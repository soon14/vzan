using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.OpenWx
{
    /// <summary>
    /// 公众号JSON返回结果（用于菜单接口等）
    /// </summary>
    [Serializable]
    public class WxJsonResult
    {
        public MiniAppEnum.ReturnCodeEnum errcode { get; set; }
        public string errmsg { get; set; }
    }
}
