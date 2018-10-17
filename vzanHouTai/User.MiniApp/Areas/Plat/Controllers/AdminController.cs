using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.User;
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
    public class AdminController : User.MiniApp.Controllers.baseController
    {
        
        // GET: 
        [MiniApp.Filters.RouteAuthCheck]
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("appId", 0);
            if (aid <= 0)
            {
                aid = Context.GetRequestInt("aid", 0);
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            
            AccountRelation accountRelation = AccountRelationBLL.SingleModel.GetModelByAccountId(xcxrelation.AccountId.ToString());

            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RedisUtil.Set<string>(string.Format(PlatStatisticalFlowBLL._redis_PlatVisiteTimeKey, aid), nowTime);
            return View();
        }
    }
}