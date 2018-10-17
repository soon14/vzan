using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Dish.Filters;

namespace User.MiniApp.Areas.PlatBusiness.Controllers
{
    //[LoginFilter]
    public class AdminController : User.MiniApp.Controllers.baseController
    {
        // GET: 
        //[LoginFilter]
        public ActionResult Index()
        {
            return View();
        }
    }
}