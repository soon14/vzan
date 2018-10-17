using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Helper;
using BLL.MiniApp.Pin;
using BLL.MiniApp.User;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Pin;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Pin.Filters;
using User.MiniApp.Model;
using static Entity.MiniApp.Pin.PinEnums;

namespace User.MiniApp.Areas.Pin.Controllers
{
    public class MainController : BaseController
    {
        [RouteAuthCheck]
        public ActionResult Index(int? appId = null, AuthInfo authInfo = null)
        {
            List<NavMenu> navmenu = NavMenuBLL.SingleModel.GetListByAId(appId.Value);
            return View(Tuple.Create(navmenu ?? new List<NavMenu>(), authInfo ?? new AuthInfo()));
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyList(int aid = 0, int pageIndex = 1, int pageSize = 15)
        {
            if (aid <= 0)
            {
                return Content("参数错误");
            }
            
            ViewModel<DrawCashApply> vm = new ViewModel<DrawCashApply>();
            int recordCount = 0;
            vm.DataList = DrawCashApplyBLL.SingleModel.GetListByAid(aid, pageIndex, pageSize, out recordCount);
            if (vm.DataList != null && vm.DataList.Count > 0)
            {
                string userIds = string.Join(",",vm.DataList.Select(s=>s.userId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                vm.DataList.ForEach(apply =>
                {
                    apply.userId = apply.appId == WebSiteConfig.GongZhongAppId ? apply.OrderId : apply.userId;
                    apply.pinStore = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, apply.userId);
                    C_UserInfo userInfo = userInfoList?.FirstOrDefault(f=>f.Id == apply.userId);

                    apply.nickName = userInfo != null ? $"{userInfo.NickName} {userInfo.TelePhone}" : "用户信息错误";
                });
            }
            vm.PageSize = pageSize;
            vm.PageIndex = pageIndex;
            vm.TotalCount = recordCount;
            vm.aId = aid;
            return View(vm);
        }
        /// <summary>
        /// 提现审核
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult UpdateState(int aid = 0, int id = 0, int state = -999)
        {
            if (aid <= 0 || id <= 0 || state == -999)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            
            DrawCashApply apply = DrawCashApplyBLL.SingleModel.GetModelByAid_Id(aid, id);
            if (apply == null)
            {
                result.msg = "申请不存在";
                return Json(result);
            }
            if (apply.drawState != (int)DrawCashState.未开始提现 && apply.state != (int)ApplyState.删除)
            {
                result.msg = "该申请已开始提现处理";
                return Json(result);
            }
            if (state == (int)ApplyState.删除)
            {
                if (apply.state != (int)ApplyState.审核不通过)
                {
                    result.msg = "无法删除，只有审核不通过的才能删除";
                    return Json(result);
                }
            }
            
            TransactionModel tran = new TransactionModel();
            //if (apply.state == (int)ApplyState.审核通过 && apply.pinStore.agentId > 0 && apply.applyType == (int)DrawCashApplyType.拼享惠平台交易)//如果是通过审核改为其他状态，要将代理提成减掉
            //{
            //    if (!pinAgentBLL.ReturnStoreIncome(apply, tran))
            //    {
            //        result.msg = "操作失败，代理金额回撤失败";
            //        return Json(result);
            //    }
            //}
            if (state == (int)ApplyState.审核不通过)
            {
                apply.state = (int)ApplyState.审核不通过;
                apply.UpdateTime = DateTime.Now;
                switch (apply.applyType)
                {
                    case (int)DrawCashApplyType.拼享惠平台交易:
                    case (int)DrawCashApplyType.拼享惠扫码收益:
                        result.code = DrawCashApplyBLL.SingleModel.ReturnPinStoreCash(apply, tran) ? 1 : 0;
                        log4net.LogHelper.WriteInfo(this.GetType(),Newtonsoft.Json.JsonConvert.SerializeObject(tran));
                        result.code = DrawCashApplyBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
                        break;
                    case (int)DrawCashApplyType.拼享惠代理收益:
                        result.code = DrawCashApplyBLL.SingleModel.ReturnPinAgentCash(apply, tran) ? 1 : 0;
                        result.code = DrawCashApplyBLL.SingleModel.ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray) ? 1 : 0;
                        break;
                    case (int)DrawCashApplyType.拼享惠用户返现:
                        result.msg = DrawCashApplyBLL.SingleModel.UpdatePxhUserDrawCashResult(0, apply, "", state);
                        result.code = result.msg.Length <= 0 ? 1 : 0;
                        if (result.msg.Length>0)
                        {
                            return Json(result);
                        }
                        break;
                }

            }
            else if (state == (int)ApplyState.审核通过)
            {
                apply.state = (int)ApplyState.审核通过;
                apply.UpdateTime = DateTime.Now;
                apply.drawState = (int)DrawCashState.提现中;
                if(apply.applyType!= (int)DrawCashApplyType.拼享惠用户返现)
                {
                    apply.userId = apply.appId == WebSiteConfig.GongZhongAppId ? apply.OrderId : apply.userId;
                    apply.pinStore = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, apply.userId);
                    if (apply.pinStore == null)
                    {
                        result.msg = "门店不存在";
                        return Json(result);
                    }
                }
                
                result.code = DrawCashApplyBLL.SingleModel.Update(apply, "state,updatetime,drawState") ? 1 : 0;
            }
            else if (state == 2)//人工提现
            {
                apply.state = (int)ApplyState.审核通过;
                apply.UpdateTime = DateTime.Now;
                apply.drawState = (int)DrawCashState.人工提现;
                apply.userId = apply.appId == WebSiteConfig.GongZhongAppId ? apply.OrderId : apply.userId;
                apply.pinStore = PinStoreBLL.SingleModel.GetModelByAid_UserId(apply.Aid, apply.userId);
                if (apply.pinStore == null)
                {
                    result.msg = "门店不存在";
                    return Json(result);
                }
                result.code = DrawCashApplyBLL.SingleModel.Update(apply, "state,updatetime,drawState") ? 1 : 0;
                if (result.code == 1)
                {
                    PinAgentBLL.SingleModel.AddStoreIncome(apply);
                }
            }
            else
            {
                apply.state = state;
                apply.UpdateTime = DateTime.Now;
                result.code = DrawCashApplyBLL.SingleModel.Update(apply, "state,updatetime") ? 1 : 0;
            }
            result.msg = result.code == 1 ? "操作成功" : "操作失败";
            return Json(result);
        }
        public ActionResult ComplaintList(int aid = 0, int pageIndex = 1, int pageSize = 15, string outTradeNo = "",string userName="", string storeName = "", int orderState = -999, DateTime? startDate = null, DateTime? endDate = null, int state = -999,int sId = -999)
        {
            ViewModel<PinComplaint> model = new ViewModel<PinComplaint>();
            
            int recordCount = 0;
            model.DataList = PinComplaintBLL.SingleModel.GetListByCondition(aid, pageIndex, pageSize, outTradeNo, userName, sId, storeName, orderState, startDate, endDate, state, out recordCount);
            if (model.DataList != null && model.DataList.Count > 0)
            {
                PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
                string userIds = string.Join(",",model.DataList.Select(s=>s.userId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);

                string orderIds = string.Join(",",model.DataList.Select(s=>s.orderId).Distinct());
                List<PinGoodsOrder> pinGoodsOrderList = pinGoodsOrderBLL.GetListByIds(orderIds);

                string groupIds = string.Join(",",pinGoodsOrderList?.Select(s=>s.groupId).Distinct());
                List<PinGroup> pinGroupList = PinGroupBLL.SingleModel.GetListByIds(groupIds);

                string storeIds = string.Join(",",model.DataList.Select(s=>s.storeId).Distinct());
                List<PinStore> pinStoreList = PinStoreBLL.SingleModel.GetListByIds(storeIds);

                model.DataList.ForEach(item =>
                {
                    item.userInfo = userInfoList?.FirstOrDefault(f=>f.Id == item.userId);
                    item.order = pinGoodsOrderList?.FirstOrDefault(f => f.id == item.orderId);
                    if (item.order!=null&&item.order.groupId > 0)
                    {
                        item.order.groupInfo = pinGroupList?.FirstOrDefault(f=>f.id == item.order.groupId);
                    }
                    item.store = pinStoreList?.FirstOrDefault(f=>f.id == item.storeId);
                });
            }
            ViewBag.aid = aid;
            ViewBag.outTradeNo = outTradeNo;
            ViewBag.storeName = storeName;
            ViewBag.orderState = orderState;
            if (sId != -999)
            {
                ViewBag.storeId = sId;
            }
            string startStr = string.Empty;
            if (startDate != null)
            {
                startStr = ((DateTime)startDate).ToString("yyyy-MM-dd");
            }
            string endStr = string.Empty;
            if (endDate != null)
            {
                endStr = ((DateTime)endDate).ToString("yyyy-MM-dd");
            }
            ViewBag.startDate = startStr;
            ViewBag.endDate = endStr;
            ViewBag.state = state;
            ViewBag.userName = userName;
            model.TotalCount = recordCount;
            model.PageSize = pageSize;
            model.PageIndex = pageIndex;
            return View(model);
        }

        /// <summary>
        /// 保存协调记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="id"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public ActionResult SaveComplaintRecord(int aid=0,int id=0,string record = "")
        {
            if (aid <= 0 || id <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            if (string.IsNullOrEmpty(record))
            {
                result.msg = "协调记录不能为空";
                return Json(result);
            }
            
            PinComplaint complaint = PinComplaintBLL.SingleModel.GetModelByAid_Id(aid,id);
            if (complaint == null)
            {
                result.msg = "申诉不存在";
                return Json(result);
            }
            if (complaint.state == (int)ComplaintState.未处理)
            {
                complaint.state = (int)ComplaintState.协调中;
            }
            complaint.record = record;
            if (PinComplaintBLL.SingleModel.Update(complaint, "state,record"))
            {
                result.code = 1;
                result.msg = "保存成功";
            }
            else
            {
                result.msg = "保存失败";
            }
            return Json(result);
        }

        public ActionResult SendComplaintResult(int aid,int id=0,string resultMsg = "")
        {
            if(aid <= 0 || id <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            if (string.IsNullOrEmpty(resultMsg))
            {
                result.msg = "处理结果不能为空";
                return Json(result);
            }
            if (resultMsg.Length > 100)
            {
                result.msg = "处理结果不能超过100字";
                return Json(result);
            }
            
            PinComplaint complaint = PinComplaintBLL.SingleModel.GetModelByAid_Id(aid, id);
            if (complaint == null)
            {
                result.msg = "申诉不存在";
                return Json(result);
            }
            complaint.state = (int)ComplaintState.已处理;
            complaint.result = resultMsg;
            if (PinComplaintBLL.SingleModel.Update(complaint, "state,result"))
            {
                result.code = 1;

                //发给用户通知
                object orderData = TemplateMsg_Miniapp.PinGetTemplateMessageData(SendTemplateMessageTypeEnum.拼享惠发送申诉结果通知, complaint);
                TemplateMsg_Miniapp.SendTemplateMessage(complaint.userId, SendTemplateMessageTypeEnum.拼享惠发送申诉结果通知, TmpType.拼享惠, orderData);
                result.msg = "发送成功";
            }
            else
            {
                result.msg = "发送失败";
            }
            return Json(result);
        }

        /// <summary>
        /// 拼单返现
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderNum"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult GroupApplyList(int aid = 0, int pageIndex = 1, int pageSize = 15, string orderNum = "",DateTime? startDate = null, DateTime? endDate = null, int state = -999)
        {
            string startStr = string.Empty;
            if (startDate != null)
            {
                startStr = ((DateTime)startDate).ToString("yyyy-MM-dd");
            }
            string endStr = string.Empty;
            if (endDate != null)
            {
                endStr = ((DateTime)endDate).ToString("yyyy-MM-dd");
            }

            ViewModel<DrawCashApply> model = new ViewModel<DrawCashApply>();
            int recordCount = 0;
            
            List<DrawCashApply> list = DrawCashApplyBLL.SingleModel.GetListPxhUserDrawCash(aid,state,orderNum, startStr, endStr,pageIndex,pageSize,ref recordCount);
            model.DataList = list;
            model.TotalCount = recordCount;
            model.PageSize = pageSize;
            model.PageIndex = pageIndex;

            ViewBag.aid = aid;
            ViewBag.orderNum = orderNum;
            ViewBag.startDate = startStr;
            ViewBag.endDate = endStr;
            ViewBag.state = state;

            return View(model);
        }

        /// <summary>
        /// 导出拼单返现记录
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="orderNum"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="state"></param>
        public void ExportGroupDrawCaseExcel(int aid = 0, string orderNum = "", DateTime? startDate = null, DateTime? endDate = null, int state = -999)
        {
            string startStr = string.Empty;
            if (startDate != null)
            {
                startStr = ((DateTime)startDate).ToString("yyyy-MM-dd");
            }
            string endStr = string.Empty;
            if (endDate != null)
            {
                endStr = ((DateTime)endDate).ToString("yyyy-MM-dd");
            }
            List<int> listBId = new List<int>();
            int recordCount = 0;
            
            List<DrawCashApply> list = DrawCashApplyBLL.SingleModel.GetListPxhUserDrawCash(aid, state, orderNum, startStr, endStr, 1, 1, ref recordCount,true);
            
            if (list==null || list.Count <= 0)
            {
                Response.Write("没有数据");
                return;
            }

            DataTable table = new DataTable();
            table.Columns.AddRange(new[]
            {
                        new DataColumn("订单号"),
                        new DataColumn("下单时间"),
                        new DataColumn("申请时间"),
                        new DataColumn("返现金额(元)"),
                        new DataColumn("返现状态"),
                        new DataColumn("流水号"),
                        new DataColumn("返现时间"),
            });
            int export = Utility.IO.Context.GetRequestInt("export", 0);
            if (list != null && list.Count > 0)
            {
                foreach (DrawCashApply item in list)
                {
                    DataRow row = table.NewRow();
                    row["订单号"] = item.nickName;
                    row["下单时间"] = item.accountName;
                    row["申请时间"] = item.AddTimeStr;
                    row["返现金额(元)"] = item.applyMoneyStr;
                    row["返现状态"] = item.drawStateStr;
                    row["流水号"] = item.partner_trade_no;
                    row["返现时间"] = item.DrawTimeStr;
                    table.Rows.Add(row);
                }
            }
            if (table.Rows.Count <= 0)
            {
                table.Rows.Add(table.NewRow());
            }

            ExcelHelper<C_BargainUser>.Out2Excel(table, "拼单返现记录"); //导出
        }
    }
}