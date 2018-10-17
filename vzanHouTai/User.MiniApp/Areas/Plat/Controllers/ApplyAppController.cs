using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Plat;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;
using User.MiniApp.Areas.Plat.Models;
using Utility.IO;

namespace User.MiniApp.Areas.Plat.Controllers
{
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
    public class ApplyAppController : User.MiniApp.Controllers.baseController
    {
        protected Return_Msg _returnData;
        
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.appId = aid;

            
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                _returnData.Msg = "还未开通小未平台模板";
                return Json(_returnData);
            }
            
            AccountRelation accountrelation = AccountRelationBLL.SingleModel.GetModelByAccountId(xcxrelation.AccountId.ToString());
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(xcxrelation.AccountId.ToString());

            ApplyAppView model = new ApplyAppView();
            model.Deposit = accountrelation.Deposit;
            model.IsAgent = agentinfo != null;
            model.ExitLog = AgentdepositLogBLL.SingleModel.ExitLogByacid(accountrelation.Id);
            model.AccountRId = accountrelation.Id;

            return View(model);
        }
        
        public ActionResult GetDataList()
        {
            _returnData = new Return_Msg();

            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int aid = Context.GetRequestInt("aid", 0);
            int openState = Context.GetRequestInt("openstate", -1);
            int xcxappState = Context.GetRequestInt("xcxappstate", -2);
            int dayLength = Context.GetRequestInt("daylength", 0);
            string customerName = Context.GetRequest("customername", string.Empty);
            string storeName = Context.GetRequest("storename", string.Empty);
            string loginId = Context.GetRequest("loginid", string.Empty);

            if (aid<=0)
            {
                _returnData.Msg = "参数错误";
                return Json(_returnData);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                _returnData.Msg = "还未开通小未平台模板";
                return Json(_returnData);
            }

            Agentinfo agentinfoModel = AgentinfoBLL.SingleModel.GetModelByAccoundId(xcxrelation.AccountId.ToString());

            int count = 0;
            List<PlatApplyApp> list = PlatApplyAppBLL.SingleModel.GetDataList(xcxrelation.AppId,dayLength, customerName, loginId,storeName, openState, xcxappState,aid, pageSize,pageIndex, ref count);

            XcxTemplate tempInfo = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小未平台子模版);
            if(tempInfo!=null && agentinfoModel!=null)
            {
                List<XcxTemplate> xcxList = XcxTemplateBLL.SingleModel.GetRealPriceTemplateList($" id in ({tempInfo.Id})", agentinfoModel.id);
                if(xcxList!=null && xcxList.Count>0)
                {
                    tempInfo = xcxList[0];
                }
            }
            _returnData.isok = true;
            _returnData.dataObj = new { data = list, count = count,tempinfo= tempInfo };

            return Json(_returnData);
        }
        
        public ActionResult OpenStore()
        {
            _returnData = new Return_Msg();

            int aid = Context.GetRequestInt("aid", 0);
            int id = Context.GetRequestInt("id",0);
            int type = Context.GetRequestInt("type", 0);
            int useLength = Context.GetRequestInt("uselength",0);
            XcxTemplate xcxtemplateModel = XcxTemplateBLL.SingleModel.GetModelByType((int)TmpType.小未平台子模版);
            if(xcxtemplateModel==null)
            {
                _returnData.Msg = "改模板还未上线";
                return Json(_returnData);
            }
            if (id<=0)
            {
                _returnData.Msg = "参数错误";
                return Json(_returnData);
            }

            PlatApplyApp platApplyAppModel = PlatApplyAppBLL.SingleModel.GetModel(id);
            if(platApplyAppModel==null)
            {
                _returnData.Msg = "数据过期，请刷新重试";
                return Json(_returnData);
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if(xcxrelation==null)
            {
                _returnData.Msg = "还未开通小未平台模板";
                return Json(_returnData);
            }

            try
            {
                if (type == 0)
                {
                    string msg = "";
                    _returnData.isok = PlatApplyAppBLL.SingleModel.OpenStore(platApplyAppModel, xcxrelation.AccountId.ToString(), useLength, xcxtemplateModel.Id, ref msg);
                    _returnData.Msg = _returnData.isok ? "开通成功" : (msg.Length <= 0 ? "开通失败" : msg);
                }
                else if (type == 1)
                {
                    string msg = "";
                    _returnData.isok = PlatApplyAppBLL.SingleModel.AddTimeLength(platApplyAppModel, xcxrelation.AccountId.ToString(), useLength, xcxtemplateModel.Id, ref msg);
                    _returnData.Msg = _returnData.isok ? "续期成功" : (msg.Length <= 0 ? "续期失败" : msg);
                }
                else
                {
                    _returnData.Msg = "无效类型";
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(),ex);
                _returnData.Msg = "系统繁忙";
            }
            

            return Json(_returnData);
        }

        /// <summary>
        /// 关闭或开启同步
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveXcxState()
        {
            _returnData = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            int state = Context.GetRequestInt("state", 0);
            if (id <= 0)
            {
                _returnData.Msg = "参数错误";
                return Json(_returnData);
            }

            PlatApplyApp model = PlatApplyAppBLL.SingleModel.GetModel(id);
            if (model == null)
            {
                _returnData.Msg = "数据过期，请刷新重试";
                return Json(_returnData);
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModel(model.StoreId);
            if(store==null)
            {
                _returnData.Msg = "店铺数据过期，请刷新重试";
                return Json(_returnData);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(store.Aid);
            if(xcxrelation==null)
            {
                _returnData.Msg = "店铺数据过期，请刷新重试";
                return Json(_returnData);
            }
            xcxrelation.State = state;
            _returnData.isok = XcxAppAccountRelationBLL.SingleModel.Update(xcxrelation,"state");
            _returnData.Msg = _returnData.isok ? "保存成功" : "保存失败";

            return Json(_returnData);
        }

        public ActionResult DepositLog(string act="list",int aid=0,int pageSize=10,int pageIndex=1,string name="",int type=0,string starttime="",string endtime="")
        {
            ViewBag.appId = aid;
            if (act=="list")
            {
                return View();
            }

            _returnData = new Return_Msg();
            
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                _returnData.Msg = "还未开通小未平台模板";
                return Json(_returnData);
            }
            
            AccountRelation accountRelation = AccountRelationBLL.SingleModel.GetModelByAccountId(xcxrelation.AccountId.ToString());
            int count = 0;
            List<AgentdepositLog> list = AgentdepositLogBLL.SingleModel.GetList(0,"",type,starttime,endtime,pageSize,pageIndex,out count,accountRelation.Id);
            _returnData.dataObj = new { data = list, count = count };
            _returnData.isok = true;
            return Json(_returnData);
        }
    }
}