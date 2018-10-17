using BLL.MiniApp.Conf;
using Entity.MiniApp.Conf;
using System.Collections.Generic;
using System.Web.Mvc;
using User.MiniApp.Areas.Dish.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.Dish.Controllers
{
    [LoginFilter]
    public class AdminController : User.MiniApp.Controllers.baseController
    {
        // GET: Dish/Home
        //[LoginFilter]
        public ActionResult Index()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId == 0)
                appId = Context.GetRequestInt("aid", 0);
            string AccountId = Core.MiniApp.Utils.GetBuildCookieId("dz_UserCookieNew").ToString();
            
            //获取用户是否有设置自定义水印
            ViewBag.BottomLogoCount = ConfParamBLL.SingleModel.GetCustomConfigCount(AccountId, "'agentcustomlogo'");
            ViewBag.CustomLogo = "";
            if (appId > 0)
            {
                List<ConfParam> confparamlist = ConfParamBLL.SingleModel.GetListByRId(appId, "'agentsystemlogo'");
                ViewBag.CustomLogo = confparamlist==null || confparamlist.Count<=0?"": confparamlist[0].Value;//"https://gss0.bdstatic.com/5bVWsj_p_tVS5dKfpU_Y_D3/res/r/image/2017-09-27/297f5edb1e984613083a2d3cc0c5bb36.png";
            }
            return View();
        }
    }
}