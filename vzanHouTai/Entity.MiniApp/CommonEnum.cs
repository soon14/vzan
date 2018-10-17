using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.MiniApp
{
    public enum OrderRule
    {
        CreaterTime=1,
        ReplyTime =2
    }
    public enum ShowTimeType
    {
        CreaterTime = 1,
        ReplyTime = 2
    }
    public enum ShowTimeRule
    {
        Relative = 1,//相对时间
        Absolute = 2//绝对时间
    }
}
