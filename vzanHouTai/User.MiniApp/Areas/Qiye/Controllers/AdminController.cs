using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Qiye;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Qiye;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Qiye.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.Qiye.Controllers
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

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            
            AccountRelation accountrelation = AccountRelationBLL.SingleModel.GetModelByAccountId(xcxrelation.AccountId.ToString());

            QiyeStore qiyeStore = QiyeStoreBLL.SingleModel.GetModelByAId(aid);
            if (qiyeStore == null)
            {
                qiyeStore = new QiyeStore();
                qiyeStore.Aid = aid;
                qiyeStore.AddTime = DateTime.Now;
                qiyeStore.UpdateTime = DateTime.Now;
                qiyeStore.SwitchModel = new QiyeStoreSwitchModel();
                qiyeStore.SwitchConfig = JsonConvert.SerializeObject(qiyeStore.SwitchModel);

                int id = Convert.ToInt32(QiyeStoreBLL.SingleModel.Add(qiyeStore));
                if (id <= 0)
                {
                    return View("PageError", new QiyeReturnMsg() { Msg = "初始化数据失败!", code = "500" });
                }
            }


            return View();
        }
    }
}