using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Plat;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Plat.Filters;
using Utility;
using Utility.IO;

namespace User.MiniApp.Areas.Plat.Controllers
{
    [LoginFilter]
    [MiniApp.Filters.RouteAuthCheck]
    public class DrawController : User.MiniApp.Controllers.baseController
    {
        protected Return_Msg _returnData;
        
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.appId = aid;
            return View();
        }
        
        public ActionResult GetDataList(int aid=0,int pageSize=10,int pageIndex=1,int state = -999, string storename = "",int drawway=-999,int drawstate=-999)
        {
            _returnData = new Return_Msg();
            
            int count = 0;
            List<DrawCashApply> list = DrawCashApplyBLL.SingleModel.GetPlatDrawCashApplys(ref count,aid,state, drawway, drawstate,"","", storename, pageSize,pageIndex);

            _returnData.dataObj = new { data = list, count = count };
            _returnData.isok = true;
            return Json(_returnData);
        }

        /// <summary>
        /// 变更提现申请记录状态
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateDrawCashApply(int aid=0,int state=0,string ids="")
        {
            _returnData = new Return_Msg();
            
            if (state != -1 && state != 1)
            {
                _returnData.Msg = "无效状态";
                return Json(_returnData);
            }
            
            if (!StringHelper.IsNumByStrs(',', ids))
            {
                _returnData.Msg = "非法操作";
                return Json(_returnData);
            }
            
            string msg = DrawCashApplyBLL.SingleModel.UpdatePlatDrawCashApply(ids, state, aid, dzaccount.Id.ToString());
            _returnData.isok = msg.Length <= 0;
            _returnData.Msg = _returnData.isok?"操作成功":msg;
            return Json(_returnData);
        }

        /// <summary>
        /// 银行账号确认提现
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ConfirmDrawCash(int aid=0,int id=0)
        {
            _returnData = new Return_Msg();
            
            if (id<=0)
            {
                _returnData.Msg = "id不能为0";
                return Json(_returnData);
            }

            DrawCashApply model = DrawCashApplyBLL.SingleModel.GetModel(id);
            if(model==null)
            {
                _returnData.Msg = "找不到提现申请数据";
                return Json(_returnData);
            }
             
            string msg = DrawCashApplyBLL.SingleModel.UpdatePlayDrawCashResult(1, model, "银行账号体现成功");
            _returnData.isok = msg.Length <= 0;
            _returnData.Msg = _returnData.isok ? "操作成功" : msg;
            return Json(_returnData);
        }

        public ActionResult Config()
        {
            
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.appId = aid;
            PlatDrawConfig model = PlatDrawConfigBLL.SingleModel.GetModelByAId(aid);
            if (model == null)
                model = new PlatDrawConfig();
            return View(model);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveInfo(int aid = 0, PlatDrawConfig model = null)
        {
            _returnData = new Return_Msg();

            if (model == null)
            {
                _returnData.Msg = "无效数据";
                return Json(_returnData);
            }

            
            PlatDrawConfig tempModel = PlatDrawConfigBLL.SingleModel.GetModelByAId(aid);
            if (tempModel == null)
                tempModel = new PlatDrawConfig();

            tempModel.CommandTime = model.CommandTime;
            tempModel.DrawCashWay = model.DrawCashWay;
            tempModel.Fee = model.Fee;
            tempModel.MinMoney = model.MinMoney;
            tempModel.UpdateTime = DateTime.Now;

            if (tempModel.Id == 0)
            {
                tempModel.AId = aid;
                tempModel.AddTime = DateTime.Now;
                _returnData.isok = Convert.ToInt32(PlatDrawConfigBLL.SingleModel.Add(tempModel)) > 0;
            }
            else
            {
                _returnData.isok = PlatDrawConfigBLL.SingleModel.Update(tempModel);
            }
            _returnData.Msg = _returnData.isok ? "保存成功" : "保存失败";
            return Json(_returnData);
        }
    }
}