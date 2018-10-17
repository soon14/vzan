using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Plat;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
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
    public class SyncDataController : User.MiniApp.Controllers.baseController
    {
        protected Return_Msg _returnData;
        protected AgentDistributionRelationBLL _agentDistributionRelationBLL;
      
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.appId = aid;
            return View();
        }

  
        public ActionResult GetDataList()
        {
            _agentDistributionRelationBLL = new AgentDistributionRelationBLL();
            _returnData = new Return_Msg();

            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int aid = Context.GetRequestInt("aid", 0);
            int opensyncData = Context.GetRequestInt("opensyncdata", 0);
            string dname = Context.GetRequest("dname", string.Empty);
            string aname = Context.GetRequest("aname", string.Empty);

            if(aid<=0)
            {
                _returnData.Msg = "参数错误";
                return Json(_returnData);
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if(xcxrelation==null)
            {
                _returnData.Msg = "模板不存在";
                return Json(_returnData);
            }
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(xcxrelation.AccountId.ToString());
            if(agentinfo==null)
            {
                _returnData.Msg = "您还未成为代理商";
                return Json(_returnData);
            }

            int count = 0;
            List<AgentDistributionRelation> list = _agentDistributionRelationBLL.GetSysnDataList(dname,aname, agentinfo.id, opensyncData,pageSize,pageIndex, ref count);

            _returnData.isok = true;
            _returnData.dataObj = new { data = list, count = count };

            return Json(_returnData);
        }
        
        /// <summary>
        /// 关闭或开启同步
        /// </summary>
        /// <returns></returns>

        public ActionResult OpenSyncData()
        {
            _agentDistributionRelationBLL = new AgentDistributionRelationBLL();
            _returnData = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            int opensyncState = Context.GetRequestInt("opensyncstate", 0);
            if (id <= 0)
            {
                _returnData.Msg = "参数错误";
                return Json(_returnData);
            }

            AgentDistributionRelation model = _agentDistributionRelationBLL.GetModel(id);
            if (model == null)
            {
                _returnData.Msg = "数据过期，请刷新重试";
                return Json(_returnData);
            }

            model.UpdateTime = DateTime.Now;
            model.OpenSyncData = opensyncState;
            _returnData.isok = _agentDistributionRelationBLL.Update(model, "OpenSyncData,UpdateTime");
            _returnData.Msg = _returnData.isok ? "保存成功" : "保存失败";

            return Json(_returnData);
        }
    }
}