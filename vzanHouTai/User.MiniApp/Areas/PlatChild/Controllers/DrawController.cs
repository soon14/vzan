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
using User.MiniApp.Areas.PlatChild.Filters;
using Utility;
using Utility.IO;

namespace User.MiniApp.Areas.PlatChild.Controllers
{
    [LoginFilter]
    public class DrawController : User.MiniApp.Controllers.baseController
    {
        protected Return_Msg _returnData;
        
        public ActionResult Index()
        {
            int aid = Context.GetRequestInt("aid", 0);
            ViewBag.appId = aid;
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(aid);
            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModel(store.MyCardId);
            if(myCard==null)
            {
                return Redirect("/base/PageError?type=2");
            }
            //用户提现账户
            PlatUserCash model = PlatUserCashBLL.SingleModel.GetModelByUserId(myCard.AId, myCard.UserId);
            if(model==null)
            {
                return Redirect("/base/PageError?type=2");
            }
            //平台提现设置
            PlatDrawConfig drawConfig = PlatDrawConfigBLL.SingleModel.GetModelByAId(myCard.AId);
            if (drawConfig == null)
            {
                drawConfig = new PlatDrawConfig();
            }
            model.Fee = drawConfig.Fee;
            if (string.IsNullOrEmpty(model.Name))
            {
                model.Name = "未绑定";
            }
            return View(model);
        }
        
        /// <summary>
        /// 保存提现账号
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="accountbank"></param>
        /// <param name="customername"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SaveBankInfo(string act="bank",int aid=0,string accountbank="",string customername = "",int id=0,int drawcashway=0)
        {
            _returnData = new Return_Msg();
            
            string column = "UpdateTime";
            if (id <= 0)
            {
                _returnData.Msg = "无效账户ID";
                return Json(_returnData);
            }
            if (act=="bank")
            {
                if (string.IsNullOrEmpty(customername))
                {
                    _returnData.Msg = "请输入客户账号";
                    return Json(_returnData);
                }
                if (string.IsNullOrEmpty(accountbank))
                {
                    _returnData.Msg = "请输入提现账号";
                    return Json(_returnData);
                }
                column += ",AccountBank,Name";
            }
            else
            {
                column += ",DrawCashWay";
            }
            
            PlatUserCash userCash = PlatUserCashBLL.SingleModel.GetModel(id);
            if(userCash==null)
            {
                _returnData.Msg = "无效账户";
                return Json(_returnData);
            }
            userCash.AccountBank = accountbank;
            userCash.Name = customername;
            userCash.DrawCashWay = drawcashway;
            userCash.UpdateTime = DateTime.Now;

            _returnData.isok = PlatUserCashBLL.SingleModel.Update(userCash, column);
            _returnData.Msg = _returnData.isok ? "保存成功" : "保存失败";
            return Json(_returnData);
        }
        
        /// <summary>
        /// 获取申请记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="state"></param>
        /// <param name="storename"></param>
        /// <param name="drawway"></param>
        /// <param name="drawstate"></param>
        /// <returns></returns>
        public ActionResult GetDataList(int aid=0,int plataid=0, int pageSize=10,int pageIndex=1,int state = -999, string storename = "",int drawway=-999,int drawstate=-999,int userid=0)
        {
            _returnData = new Return_Msg();
            
            int count = 0;
            List<DrawCashApply> list = DrawCashApplyBLL.SingleModel.GetPlatDrawCashApplys(ref count, plataid, state, drawway, drawstate,"","", storename, pageSize,pageIndex, userid);

            _returnData.dataObj = new { data = list, count = count };
            _returnData.isok = true;
            return Json(_returnData);
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id">提现账号id(PlatUserCash)</param>
        /// <param name="drawcashmoney">提现金额（分）</param>
        /// <returns></returns>
        public ActionResult ApplyDrawCash(int aid=0,int id=0,int drawcashmoney=0)
        {
            _returnData = new Return_Msg();
            
            string msg = DrawCashApplyBLL.SingleModel.ApplyDrawCash(aid,id,drawcashmoney);
            _returnData.isok = msg.Length <= 0;
            _returnData.Msg = msg.Length<=0?"申请成功":msg;
            return Json(_returnData);
        }
    }
}