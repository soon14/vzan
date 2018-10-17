using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Pin;
using BLL.MiniApp.Tools;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Pin;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using User.MiniApp.Areas.PinAdmin.Filters;
using User.MiniApp.Model;
using Utility;
using static Entity.MiniApp.Pin.PinEnums;

namespace User.MiniApp.Areas.PinAdmin.Controllers
{
    /// <summary>
    /// 评论，订单
    /// </summary>
    [LoginFilter]
    public class OrderController : BaseController
    {

        #region 评论
        public ActionResult Comments(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string kw = "")
        {
            pageIndex = pageIndex - 1;
            if (pageIndex  < 0)
                pageIndex = 0;

            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $" pc.state=1 and pc.aid={aId} and pc.storeid={storeId} ";

                ViewModel<PinComment> vm = new ViewModel<PinComment>();
                List<MySqlParameter> parameters = null;
                string sqlField = "pc.*,pg.name as GoodName,pg.img as GoodImgUrl";
                string sqlCount = "count(0)";
                string sql = $"from PinComment pc left join pingoods pg on pc.goodsid=pg.id where pc.aid={aId} and pc.storeid={storeId}";

                if (!string.IsNullOrEmpty(kw))
                {
                    sql += $" and pg.name like @kw  ";
                    parameters = new List<MySqlParameter>();
                    parameters.Add(new MySqlParameter("kw",Utils.FuzzyQuery(kw)));
                }

                DataTable dt = SqlMySql.ExecuteDataSet(dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT {sqlField} {sql} order by pc.id desc limit {pageSize * pageIndex},{pageSize}", parameters?.ToArray()).Tables[0];
                vm.DataList = DataHelper.ConvertDataTableToList<PinComment>(dt);

                string userIds = string.Join(",",vm.DataList.Select(s=>s.UserId).Distinct());
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByIds(userIds);
                vm.DataList?.ForEach(p =>
                {
                    C_UserInfo user = userInfoList?.FirstOrDefault(f=>f.Id == p.UserId);
                    if (user != null)
                    {
                        p.NickName = user.NickName;
                        p.UserPhoto = user.HeadImgUrl;
                    }
                });
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.TotalCount = PinCommentBLL.SingleModel.GetCountBySql($" select {sqlCount} {sql}",parameters?.ToArray());
                vm.aId = aId;
                vm.storeId = storeId;
                return View(vm);
            }
            else
            {
                //删除
                if (act == "del")
                {
                    if (id <= 0)
                        result.msg = "参数错误";
                    else
                    {
                        PinComment updateModel = PinCommentBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.State = -1;
                            bool updateResult = PinCommentBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                result.code = 1;
                                result.msg = "删除成功";
                            }
                            else
                                result.msg = "删除失败";
                        }
                        else
                            result.msg = "删除失败,对象不存在或已删除";
                    }

                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region 订单管理
        public ActionResult Index(int aid = 0, int storeId = 0, string orderId = "", string storeName = "", string goodsName = "", int orderState = -999, string consignee = "", string phone = "", int sendWay = -999, int groupState = -999, int groupId = 0, string startDate = "", string endDate = "", int pageIndex = 0, int pageSize = 20, string act = "")
        {
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            if (act == "getNewOrder")
            {
                bool isSuccess = pinGoodsOrderBLL.havingNewOrder(storeId, DateTime.Now.AddSeconds(-10)); //检查是否有新订单(10秒前到现在)

                result.code = isSuccess ? 1 : 0;
                result.msg = isSuccess ? "存在新订单" : "没有新订单";
                return Json(result);
            }
            if (aid <= 0 || storeId <= 0)
            {
                return Content($"参数错误 aid:{aid},storeId:{storeId}");
            }
            ViewModel<PinGoodsOrder> vm = new ViewModel<PinGoodsOrder>();
            int recordCount = 0;
            vm.DataList = pinGoodsOrderBLL.GetListByCondition(aid, storeId, orderId, storeName, goodsName, orderState, consignee, phone, sendWay, groupState, groupId, startDate, endDate, pageIndex, pageSize, out recordCount);
            if (vm.DataList != null && vm.DataList.Count > 0)
            {
                string groupIds = string.Join(",",vm.DataList.Select(s=>s.groupId).Distinct());
                List<PinGroup> pinGroupList = PinGroupBLL.SingleModel.GetListByIds(groupIds);

                vm.DataList.ForEach(order =>
                {
                    order.groupInfo = pinGroupList?.FirstOrDefault(f=>f.id == order.groupId);
                    if (order.sendway == (int)PinEnums.SendWay.到店自取)
                    {
                        PickPlace place = PickPlaceBLL.SingleModel.GetModel(Convert.ToInt32(order.address));
                        if (place != null)
                        {
                            order.storeName = place.name;
                            order.address = place.address;
                        }
                        else
                        {
                            order.address = string.Empty;
                        }
                    }
                });
            }
            vm.TotalCount = recordCount;
            vm.aId = aid;
            vm.storeId = storeId;
            ViewBag.OrderId = orderId;
            ViewBag.StoreName = storeName;
            ViewBag.goodsName = goodsName;
            ViewBag.OrderState = orderState;
            ViewBag.Consignee = consignee;
            ViewBag.Phone = phone;
            ViewBag.SendWay = sendWay;
            ViewBag.GroupState = groupState;
            ViewBag.GroupId = groupId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(vm);
        }
        /// <summary>
        /// 切换订单状态
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="state"></param>
        /// <param name="orderIds"></param>
        /// <param name="attachData">物流信息</param>
        /// <returns></returns>
        public ActionResult UpdateState(int aid = 0, int storeId = 0, int state = -999, string orderIds = "", string attachData = "")
        {
            if (aid <= 0 || storeId <= 0 || string.IsNullOrEmpty(orderIds))
            {
                result.msg = "参数错误";
                return Json(result);
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            List<PinGoodsOrder> orders = pinGoodsOrderBLL.GetListByIds(aid, storeId, orderIds, $"{(int)PinEnums.PinOrderState.交易取消},{(int)PinEnums.PinOrderState.交易失败},{(int)PinEnums.PinOrderState.交易成功}");
            if (orders == null || orders.Count <= 0)
            {
                result.msg = "找不到可操作订单";
                return Json(result);
            }

            switch (state)
            {
                case (int)PinEnums.PinOrderState.待发货:
                    result.code = pinGoodsOrderBLL.SendGoods(orders, attachData);
                    break;
                case (int)PinEnums.PinOrderState.交易成功:
                    result.code = pinGoodsOrderBLL.OrderSuccess(orders);
                    break;
                case (int)PinEnums.PinOrderState.交易取消:
                    result.code = pinGoodsOrderBLL.CancelOrder(orders);
                    break;
                default:
                    result.msg = "参数错误";
                    return Json(result);
            }
            result.msg = result.code == 1 ? "操作成功" : "操作失败";
            return Json(result);
        }


        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult Refund(int aid = 0, int storeId = 0, int orderId = 0)
        {
            if (aid <= 0 || storeId <= 0 || orderId <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            PinGoodsOrderBLL pinGoodsOrderBLL = new PinGoodsOrderBLL();
            PinGoodsOrder order = pinGoodsOrderBLL.GetModelByAid_StoreId_Id(aid, storeId, orderId);
            if (order == null)
            {
                result.msg = "订单错误";
                return Json(result);
            }
            string msg = string.Empty;
            result.code = pinGoodsOrderBLL.Refund(order, ref msg);
            result.msg = msg;
            return Json(result);
        }

        public ActionResult FaHuo(int aid = 0, int storeId = 0, int orderId = 0, string act = "")
        {
            if (string.IsNullOrEmpty(act))
            {
                PinGoodsOrder order = orderBLL.GetModelByAid_StoreId_Id(aid, storeId, orderId);
                if (order == null)
                {
                    return Content("订单不存在");
                }
                ViewModel<PinGoodsOrder> vm = new ViewModel<PinGoodsOrder>();
                vm.DataModel = order;
                vm.aId = aid;
                vm.storeId = storeId;
                ViewBag.companys = DeliveryCompanyBLL.SingleModel.GetCompanys();

                return View(vm);
            }
            else
            {
                return Json(result);
            }
        }
        #endregion
        #region 退换货
        public ActionResult RefundApplyList(int aid = 0, int storeId = 0, string consignee = "", string goodsName = "", string nickName = "", string phone = "", int sendWay = -999, string serviceNo = "", int state = -999, int type = -999, int pageIndex = 1, int pageSize = 20)
        {
            if (aid <= 0 || storeId <= 0)
            {
                return Content($"参数错误 aid:{aid},storeId:{storeId}");
            }
            ViewModel<PinRefundApply> vm = new ViewModel<PinRefundApply>();
            int recordCount = 0;
            vm.DataList = PinRefundApplyBLL.SingleModel.GetListByCondition(aid, storeId, goodsName, state, nickName, consignee, phone, sendWay, type, serviceNo, pageIndex, pageSize, out recordCount);
            if (vm.DataList != null && vm.DataList.Count > 0)
            {
                string orderIds = string.Join(",",vm.DataList.Select(s=>s.orderId).Distinct());
                List<PinGoodsOrder> pinGoodsOrderList = orderBLL.GetListByIds(orderIds);
                foreach (var item in vm.DataList)
                {
                    item.order = pinGoodsOrderList?.FirstOrDefault(f=>f.id == item.orderId);
                }
            }
            vm.storeId = storeId;
            vm.aId = aid;
            vm.TotalCount = recordCount;
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            ViewBag.GoodsName = goodsName;
            ViewBag.State = state;
            ViewBag.Consignee = consignee;
            ViewBag.Phone = phone;
            ViewBag.SendWay = sendWay;
            ViewBag.Type = type;
            ViewBag.ServiceNo = serviceNo;
            ViewBag.NickName = nickName;
            return View(vm);
        }
        /// <summary>
        /// 售后申请状态处理
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateRefundApplyState(int aid = 0, int storeId = 0, int state = 0, int id = 0)
        {
            if (aid <= 0 || storeId <= 0 || id <= 0)
            {
                result.msg = "参数错误";
                return Json(result);
            }
            PinRefundApply apply = PinRefundApplyBLL.SingleModel.GetModel(id);
            if (apply == null || apply.id == (int)RefundApplyState.删除)
            {
                result.msg = "申请不存在";
                return Json(result);
            }
            PinStore store = PinStoreBLL.SingleModel.GetModelByAid_Id(aid, storeId);
            if (store == null)
            {
                result.msg = "店铺信息错误";
                return Json(result);
            }
            string msg = string.Empty;
            bool status = false;
            apply.state = state;
            apply.updateTime = DateTime.Now;
            switch (state)
            {
                case (int)RefundApplyState.已退款:
                    status = orderBLL.RefundByApply(apply, ref msg);
                    break;
                case (int)RefundApplyState.已重新发货:
                case (int)RefundApplyState.申请失败:
                    status = PinRefundApplyBLL.SingleModel.ApplyFail(apply, ref msg);
                    break;
                case (int)RefundApplyState.删除:
                    status = PinRefundApplyBLL.SingleModel.Update(apply, "state,updatetime");
                    break;
                case (int)RefundApplyState.申请成功:
                    status = PinRefundApplyBLL.SingleModel.ApplySuccess(apply, store, ref msg);
                    break;
                default:
                    result.msg = "参数错误！";
                    return Json(result);
            }
            if (status)
            {
                result.code = 1;
                result.msg = "操作成功";
            }
            else
            {
                result.msg = "操作失败";
            }
            return Json(result);
        }
        #endregion
    }
}