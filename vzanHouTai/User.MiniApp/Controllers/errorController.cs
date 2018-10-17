using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Controllers
{
    public class errorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult error(string msg="")
        {
            ViewBag.msg = msg;
            return View();
        }

    }
}
