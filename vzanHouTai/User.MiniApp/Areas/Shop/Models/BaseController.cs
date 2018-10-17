using Entity.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Areas.Shop.Models
{
    public class BaseController : Controller
    {
        public JsonResult ApiModel(bool isok = false, string message = null, string code = null, object data = null)
        {
            return Common.SingleModel.ApiModel(isok: isok, message: message, code: code, data: data);
        }
    }
}