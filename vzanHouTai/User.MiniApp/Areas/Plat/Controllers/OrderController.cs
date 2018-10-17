using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.Plat.Controllers
{
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
    public class OrderController : User.MiniApp.Controllers.baseController
    {
        
        
        
        public ActionResult Index(int aid=0, int orderType = 0)
        {
            ViewBag.appId = aid;
            return View();
        }
        public ActionResult GetDataList()
        {
            Return_Msg returnData = new Return_Msg();
            int aid = Context.GetRequestInt("aid", 0);
            int state = Context.GetRequestInt("state", -999);
            int getWay = Context.GetRequestInt("getway", -999);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int orderType = Context.GetRequestInt("ordertype", 1);
            string orderNum = Context.GetRequest("ordernum", "");
            string ladingCode = Context.GetRequest("ladingcode", "");
            string accepterName = Context.GetRequest("acceptername", "");
            string storeName = Context.GetRequest("storeName", "");
            string accepterTelephone = Context.GetRequest("acceptertelephone", "");

            ViewBag.tab = orderType;
            int count = 0;
            List<PlatChildGoodsOrder> list = PlatChildGoodsOrderBLL.SingleModel.GetDataList2(storeName, orderType, accepterTelephone, accepterName, ladingCode, orderNum, getWay, state, aid, pageSize, pageIndex, ref count);

            returnData.dataObj = new { list = list, count = count };
            returnData.isok = true;

            return Json(returnData);
        }
    }
}