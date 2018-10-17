using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity.MiniApp;
using BLL.MiniApp;

namespace User.MiniApp.Controllers
{
    public class globalController : baseController
    {
        // GET: global
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 积分设置
        /// </summary>
        /// <returns></returns>
        [CheckLogin]
        public ActionResult socreset(int? Id)
        {
            if (!Id.HasValue)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            
            return View();
        }

    }
}