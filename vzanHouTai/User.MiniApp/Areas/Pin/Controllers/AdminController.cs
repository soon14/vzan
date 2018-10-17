using BLL.MiniApp;
using BLL.MiniApp.Pin;
using BLL.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.Pin;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Pin.Filters;
using Utility;

namespace User.MiniApp.Areas.Pin.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Index(int? aId = null)
        {
            if (aId.HasValue)
            {
                return Redirect($"/subAccount/index?appId={aId.Value}");
            }
            return new HttpStatusCodeResult(404);
        }

        public ActionResult AgentManager()
        {
            return View();
        }

        public ActionResult GetAgentList(int aid = 0, int pageIndex = 1, int pageSize = 10, int state = -999, string phone = "", string nickName = "", string fnickName = "", string fphone = "")
        {
            if (aid <= 0 || pageIndex <= 0 || pageSize <= 0)
            {
                result.code = 0;
                result.msg = "参数错误";
                return Json(result);
            }
            XcxAppAccountRelation xcxAppAccountRelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxAppAccountRelation == null)
            {
                result.code = 0;
                result.msg = "小程序不存在";
                return Json(result);
            }
            int recordCount = 0;
            
            
            List<PinStore> storeList = new List<PinStore>();

            List<PinAgent> agentList = PinAgentBLL.SingleModel.GetListByAid_State(xcxAppAccountRelation.AppId, aid, pageSize, pageIndex, out recordCount, phone:phone, nickName:nickName, fnickName:fnickName, fphone:fphone,state:-999);
            if (agentList != null && agentList.Count > 0)
            {
                foreach (var agent in agentList)
                {
                    PinStore store = PinStoreBLL.SingleModel.GetModelByAid_UserId(agent.aId, agent.userId);
                    if (store != null && store.state != -1)
                    {
                        store.goodsCount = PinGoodsBLL.SingleModel.GetCountByStoreId(store.id);
                        store.agentInfo = agent;
                        store.userId = agent.userId;
                    }
                    else
                    {
                        store = new PinStore() { storeName = "未开通店铺", userId = agent.userId };
                        store.agentInfo = agent;
                    }
                    store.agentFee = PinAgentBLL.SingleModel.GetAgentFee(store.agentInfo.id);
                    C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(agent.userId);
                    store.nickName = userInfo != null ? userInfo.NickName : string.Empty;
                    if (agent.fuserId > 0)
                    {
                        store.fuserInfo = C_UserInfoBLL.SingleModel.GetModel(agent.fuserId);
                    }
                    storeList.Add(store);
                }
            }


            result.code = 1;
            result.obj = new { list = storeList, recordCount };
            return Json(result);
        }
    }
}