using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;

namespace User.MiniApp.Controllers
{
    public class hardwareController : Controller
    {
        // GET: HardWare
        public ActionResult Index()
        {
            StringHelper.WriteOperateLog("编码器在线检测","我已经在线");
            String callbackFunName = Request["callbackparam"];
            return Content(callbackFunName + "([ { \"name\":\"John\"}])");
        }
    }
}