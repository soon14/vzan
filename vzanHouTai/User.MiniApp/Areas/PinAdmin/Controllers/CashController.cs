using BLL.MiniApp.Ent;
using BLL.MiniApp.Pin;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Pin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.PinAdmin.Filters;
using User.MiniApp.Model;
using BLL.MiniApp;

namespace User.MiniApp.Areas.PinAdmin.Controllers
{
    [LoginFilter]
    public class CashController : BaseController
    {
        /// <summary>
        /// 提现申请界面
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET: PinAdmin/Cash
        public ActionResult Index(int aId = 0, int storeId = 0)
        {
            if (aId <= 0 || storeId <= 0)
            {
                return Content("参数错误");
            }
            
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aId);
            if (platform == null)
            {
                return Content("平台信息错误");
            }
            PinStore store = (PinStore)Request.RequestContext.RouteData.Values["pinStore"];
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(store.userId);
            if (userInfo == null)
            {
                return Content($"用户信息错误");
            }
            
            PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
            
            int sumCash = DrawCashApplyBLL.SingleModel.GetSumCash(aId, store.userId);
            ViewBag.DrawCashSum = (sumCash * 0.01).ToString("0.00");
            ViewData["platform"] = platform;
            ViewData["userInfo"] = userInfo;
            ViewData["agent"] = agent;
            return View(store);
        }
        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="txway"></param>
        /// <param name="cash"></param>
        /// <param name="cashType">0:扫码提现  1：平台提现</param>
        /// <returns></returns>
        public ActionResult AddApply(int aId = 0, int storeId = 0, int txway = -1, int cash = 0, int cashType = (int)DrawCashApplyType.拼享惠平台交易)
        {
            if (aId <= 0 || storeId <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            
            PinPlatform platform = PinPlatformBLL.SingleModel.GetModelByAid(aId);
            if (platform == null)
            {
                result.msg = "平台信息错误";
                return Json(result);
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aId, storeId);
            if (store == null)
            {
                result.msg = "店铺信息错误";
                return Json(result);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(store.userId);
            if (userInfo == null)
            {
                result.msg = "用户信息错误";
                return Json(result);
            }
           
            
            if (cash < platform.minTxMoney)
            {
                result.msg = $"提现金额不可低于{platform.minTxMoneyStr}元";
                return Json(result);
            }
            string account = string.Empty;
            string accountName = string.Empty;
            switch (txway)
            {
                case 1://提现到银行卡
                    if (string.IsNullOrEmpty(store.bankcardName))
                    {
                        result.msg = "请设置银行卡账户名称";
                        return Json(result);
                    }
                    accountName = store.bankcardName;
                    if (string.IsNullOrEmpty(store.bankcardNum))
                    {
                        result.msg = "请设置银行卡账户";
                        return Json(result);
                    }
                    if (platform.toBank != 1)
                    {
                        result.msg = "暂时无法通过银行卡提现";
                        return Json(result);
                    }
                    account = store.bankcardNum;
                    break;
                case 0://提现到微信
                    if (platform.toWx != 1)
                    {
                        result.msg = "暂时无法通过微信提现";
                        return Json(result);
                    }
                    accountName = "微信账号";
                    account = userInfo.NickName;
                    break;
                default:
                    result.msg = "参数错误";
                    return Json(result);
            }
            
            int serviceFee = cash * platform.serviceFee / 1000;
            switch (cashType)
            {
                case (int)DrawCashApplyType.拼享惠平台交易:
                    if (cash > store.cash && cashType == (int)DrawCashApplyType.拼享惠平台交易)
                    {
                        result.msg = "超出可提现金额";
                        return Json(result);
                    }
                    double fee = cash * platform.serviceFee * 0.001;
                    serviceFee = (int)Math.Ceiling(fee);
                    result.code = DrawCashApplyBLL.SingleModel.PxhAddApply(store, userInfo.Id, account, accountName, cash, txway, serviceFee, (int)DrawCashApplyType.拼享惠平台交易);
                    break;
                case (int)DrawCashApplyType.拼享惠代理收益:
                    
                    PinAgent agent = PinAgentBLL.SingleModel.GetModelByUserId(userInfo.Id);
                    if (agent == null)
                    {
                        result.msg = "你还不是代理";
                        return Json(result);
                    }
                    if (cash * 1000 > agent.cash)
                    {
                        result.msg = "超出可提现金额";
                        return Json(result);
                    }
                    fee = cash * platform.agentServiceFee * 0.001;
                    serviceFee = (int)Math.Ceiling(fee);
                    result.code = DrawCashApplyBLL.SingleModel.PxhAddApply(agent, userInfo.Id, account, accountName, cash, txway, serviceFee);
                    break;
                case (int)DrawCashApplyType.拼享惠扫码收益:
                    if(cash > store.qrcodeCash && cashType == (int)DrawCashApplyType.拼享惠扫码收益)
                    {
                        result.msg = "超出可提现金额";
                        return Json(result);
                    }
                    fee = cash * platform.qrcodeServiceFee * 0.001;
                    serviceFee = (int)Math.Ceiling(fee);
                    result.code = DrawCashApplyBLL.SingleModel.PxhAddApply(store, userInfo.Id, account, accountName, cash, txway, serviceFee, (int)DrawCashApplyType.拼享惠扫码收益);
                    break;
                default:
                    result.msg = "参数错误";
                    return Json(result);
            }

            //result.code = drawCashApplyBLL.PxhAddApply(store, userInfo.Id, account, accountName, cash, txway, serviceFee, 0);
            result.msg = result.code == 1 ? "申请已提交" : "提交失败";
            return Json(result);
        }

        public ActionResult Record(int aid = 0, int storeId = 0, int pageIndex = 1, int pageSize = 15)
        {
            if (aid <= 0 || storeId <= 0)
            {
                return Content("参数错误");
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (store == null)
            {
                result.msg = "店铺信息错误";
                return Json(result);
            }
            
            ViewModel<DrawCashApply> vm = new ViewModel<DrawCashApply>();
            int recordCount = 0;
            vm.DataList = DrawCashApplyBLL.SingleModel.GetListByAid_UserId(aid, store.userId, pageIndex, pageSize, out recordCount);
            vm.TotalCount = recordCount;
            vm.aId = aid;
            vm.storeId = storeId;
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            return View(vm);
        }

        public ActionResult EditBankCard(int aid = 0, int storeId = 0, string code = "", string bankcardName = "", string bankcardNum = "")
        {
            if (string.IsNullOrEmpty(bankcardName))
            {
                result.msg = "请输入账户名称";
                return Json(result);
            }
            if (string.IsNullOrEmpty(bankcardNum))
            {
                result.msg = "请输入提现账号";
                return Json(result);
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (store == null)
            {
                result.msg = "店铺不存在！";
                return Json(result);
            }

            ReturnMsg data = ValidateSMS(store, code);
            if (data.code != 1)
            {
                return Json(data);
            }
            store.bankcardName = bankcardName;
            store.bankcardNum = bankcardNum;
            if (PinStoreBLL.SingleModel.Update(store, "bankcardName,bankcardNum"))
            {
                result.code = 1;
                result.msg = "修改成功";
            }
            else
            {
                result.msg = "修改失败";
            }
            return Json(result);
        }
    }
}