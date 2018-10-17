using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Pin.Filters;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using BLL.MiniApp.Pin;

namespace User.MiniApp.Areas.Pin.Controllers
{
    [LoginFilter]
    public class BaseController : Controller
    {
        public readonly ReturnMsg result;
        
        
        
        
        
        
        public readonly List<FunModel> funList;

        public BaseController()
        {
            result = new ReturnMsg();
            
            
            
            
            
            
            funList = new List<FunModel>
            {
                new FunModel(1,"拼团商品",""),
                new FunModel(2,"入驻申请","")
            };
        }
    }
}