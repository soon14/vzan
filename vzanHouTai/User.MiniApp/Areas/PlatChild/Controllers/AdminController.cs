using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.PlatChild.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.PlatChild.Controllers
{
    [LoginFilter]
    public class AdminController : User.MiniApp.Controllers.baseController
    {
        
        // GET: 
        public ActionResult Index()
        {

            
            
            int aid = Context.GetRequestInt("appId", 0);
            if (aid <= 0)
            {
                aid = Context.GetRequestInt("aid", 0);
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModel(store.MyCardId);
            if (myCard == null)
            {
                return Redirect("/base/PageError?type=2");
            }
            PlatUserCash model = PlatUserCashBLL.SingleModel.GetModelByUserId(myCard.AId, myCard.UserId);
            if (model == null)
            {
                model = new PlatUserCash();
                model.AddTime = DateTime.Now;
                model.UpdateTime = DateTime.Now;
                model.UserId = myCard.UserId;
                model.AId = myCard.AId;
                model.Id = Convert.ToInt32(PlatUserCashBLL.SingleModel.Add(model));
            }
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            RedisUtil.Set<string>(string.Format(PlatStatisticalFlowBLL._redis_PlatVisiteTimeKey, aid), nowTime);
            return View();
        }
    }
}