using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.PinAdmin.Filters;

namespace User.MiniApp.Areas.PinAdmin.Controllers
{
    public class ConfigController : BaseController
    {
        /// <summary>
        /// 店铺配置
        /// 申请提现
        /// 运费模板
        /// </summary>
        /// <returns></returns>
        [LoginFilter]
        public ActionResult Index()
        {
            return View();
        }
    }
}