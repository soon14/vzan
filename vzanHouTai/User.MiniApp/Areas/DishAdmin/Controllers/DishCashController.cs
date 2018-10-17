using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using User.MiniApp.Areas.DishAdmin.Filters;
using System.Web.Mvc;
using Entity.MiniApp.Dish;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    [LoginFilter]
    public class DishCashController : Controller
    {
        private readonly DishReturnMsg _result;

        public DishCashController()
        {
            _result = new DishReturnMsg();
        }


        // GET: DishAdmin/DishCash
        public ActionResult Index()
        {
            //ViewData[]
            return View();
        }

        public ActionResult Config()
        {

            return View();
        }

        public ActionResult Log()
        {
            return View();
        }
    }
}