using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Areas.Dish.Controllers
{
    public class CashController : User.MiniApp.Controllers.baseController
    {
        // GET: Dish/Cash
        public ActionResult Index()
        {
            return View();
        }
    }
}