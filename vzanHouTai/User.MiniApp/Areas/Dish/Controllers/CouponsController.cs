using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Areas.Dish.Controllers
{
    public class CouponsController : User.MiniApp.Controllers.baseController
    {
        // GET: Dish/coupons
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Edit()
        {
            return View();
        }
    }
}