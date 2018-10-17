using BLL.MiniApp;
using BLL.MiniApp.Plat;
using Entity.MiniApp;
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
    public class MyCardController : User.MiniApp.Controllers.baseController
    {
        protected Return_Msg _returnData;
        
        
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.appId = aid;
            return View();
        }
        
        public ActionResult GetMyCardDataList()
        {
            _returnData = new Return_Msg();
            int pageSize = Context.GetRequestInt("pageSize",10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int aid = Context.GetRequestInt("aid", 0);
            int storeState = Context.GetRequestInt("storestate", 0);
            string name = Context.GetRequest("name",string.Empty);
            string loginId = Context.GetRequest("loginid", string.Empty);
            string nickname = Context.GetRequest("nickname", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string startTime = Context.GetRequest("starttime",string.Empty);
            string endTime = Context.GetRequest("endtime", string.Empty);

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if(xcxrelation==null)
            {
                _returnData.Msg = "无效模板";
                return Json(_returnData);
            }
            int count = 0;
            List<PlatMyCard> list = PlatMyCardBLL.SingleModel.GetMyCardDataList(xcxrelation.AppId,aid, ref count,name, phone, pageSize, pageIndex,loginId,storeState);
            _returnData.dataObj = new { data = list, count = count };

            return Json(_returnData);
        }
        
        public ActionResult CardInfo()
        {
            int id = Context.GetRequestInt("id", 0);
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.AppId = aid;
            if (id<=0)
            {
                return Redirect("/base/PageError?type=7");
            }

            PlatMyCard model = PlatMyCardBLL.SingleModel.GetMyCardData(id,aid);

            return View(model);
        }

        /// <summary>
        /// 保存虚拟人气
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveFictitiousCount()
        {
            _returnData = new Return_Msg();
            int id = Context.GetRequestInt("id", 0);
            int fictitiousCount = Context.GetRequestInt("fictitiouscount", 0);
            if (id <= 0)
            {
                return Redirect("/base/PageError?type=7");
            }

            PlatMyCard model = PlatMyCardBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                _returnData.Msg = "数据过期，请刷新重试";
                return Json(_returnData);
            }

            model.UpdateTime = DateTime.Now;
            model.FictitiousCount = fictitiousCount;
            _returnData.isok = PlatMyCardBLL.SingleModel.Update(model, "FictitiousCount,UpdateTime");
            _returnData.Msg = _returnData.isok ? "保存成功" : "保存失败";

            return Json(_returnData);
        }

        /// <summary>
        /// 名片屏蔽或显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditeCardState(int id=0,int state=0)
        {
            _returnData = new Return_Msg();
            if(id<=0)
            {
                _returnData.Msg = "无效参数";
                return Json(_returnData);
            }
            PlatMyCard model = PlatMyCardBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                _returnData.Msg = "无效数据，请刷新重试";
                return Json(_returnData);
            }
            model.State = state;
            model.UpdateTime = DateTime.Now;
            _returnData.isok = PlatMyCardBLL.SingleModel.Update(model,"state,updatetime");
            _returnData.Msg = _returnData.isok ? "保存成功" : "保存失败";

            return Json(_returnData);
        }
    }
}