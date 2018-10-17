using BLL.MiniApp;
using Entity.MiniApp.Footbath;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Entity.MiniApp.Ent;
using Entity.MiniApp;
using Entity.MiniApp.User;
using Utility.IO;
using DAL.Base;
using MySql.Data.MySqlClient;
using log4net;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.User;

namespace User.MiniApp.Controllers
{
    public partial class footbathController : configController
    {
        // GET: footbath_ordermanager：足浴版小程序-订单管理管理

        #region 订单管理

        /// <summary>
        /// 订单管理视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Ordermanager()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            ViewBag.appId = appId;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            List<EntGoodType> roomList = EntGoodTypeBLL.SingleModel.GetRoomList(appId, storeModel.Id, (int)GoodProjectType.足浴版包间分类);
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            ViewBag.dateList = FootBathBLL.SingleModel.GetReservationTime(storeModel.switchModel.PresetTime);
            return View(roomList);
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string name = Context.GetRequest("name", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string servicetime = Context.GetRequest("servicetime", string.Empty);
            string ConfDate = Context.GetRequest("ConfDate", string.Empty);
            string serviceName = Context.GetRequest("serviceName", string.Empty);
            string roomName = Context.GetRequest("roomName", string.Empty);
            string jobNumber = Context.GetRequest("jobNumber", string.Empty);
            int payState = Context.GetRequestInt("payState", -1);
            int state = Context.GetRequestInt("state", -6);
            string OrderNum = Context.GetRequest("OrderNum", string.Empty);

            string roomIds = string.Empty;
            string serviceIds = string.Empty;
            string technicianIds = string.Empty;
            List<EntGoodsOrder> list = new List<EntGoodsOrder>();
            //搜索条件：包间
            if (!string.IsNullOrEmpty(roomName))
            {
                List<EntGoodType> roomList = EntGoodTypeBLL.SingleModel.GetRoomListByName(appId, storeModel.Id, (int)GoodProjectType.足浴版包间分类, roomName);
                if (roomList == null || roomList.Count <= 0)
                {
                    return Json(new { isok = true, recordCount = 0, list = list });
                }
                foreach (EntGoodType room in roomList)
                {
                    roomIds += room.id.ToString() + ",";
                }
                roomIds = roomIds.TrimEnd(',');
            }
            //搜索条件：服务项目
            if (!string.IsNullOrEmpty(serviceName))
            {
                List<EntGoods> serviceList = EntGoodsBLL.SingleModel.GetServiceByName(appId, (int)GoodsType.足浴版服务, serviceName);
                if (serviceList == null || serviceList.Count <= 0)
                {
                    return Json(new { isok = true, recordCount = 0, list = list });
                }
                serviceIds = string.Join(",", serviceList.Select(s => s.id).ToList());
            }
            //搜索条件：技师工号
            if (!string.IsNullOrEmpty(jobNumber))
            {
                List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetTechnicianListByJobNumeberOrWorkType(storeModel.Id, jobNumber, -1);
                if (technicianList == null || technicianList.Count <= 0)
                {
                    return Json(new { isok = true, recordCount = 0, list = list });
                }
                foreach (TechnicianInfo technician in technicianList)
                {
                    technicianIds += technician.id.ToString() + ",";
                }
                technicianIds = string.Join(",", technicianList.Select(t => t.id).ToList());
            }
            int recordCount = 0;
            list = EntGoodsOrderBLL.SingleModel.GetFootbathOrderList(appId, name, phone, servicetime, payState, state, roomIds, serviceIds, technicianIds, pageIndex, pageSize, (int)EntOrderType.订单, out recordCount, ConfDate, OrderNum);
            return Json(new { isok = true, recordCount = recordCount, list = list, ConfDate = ConfDate });
        }

        public ActionResult SaveOrderInfo()
        {
            #region 数据验证

            int appId = Context.GetRequestInt("appId", 0);
            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }
            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }
            FootBath storeModel = FootBathBLL.SingleModel.GetModelByAppId(appId);
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            string reservationTime = Context.GetRequest("ReservationTime", string.Empty);
            if (string.IsNullOrEmpty(reservationTime))
            {
                return Json(new { isok = false, msg = "请选择服务时间" });
            }

            string remark = Context.GetRequest("remark", string.Empty);
            if (remark.Length > 100)
            {
                return Json(new { isok = false, msg = "备注内容不能超过100字" });
            }
            int tid = Context.GetRequestInt("tid", 0);
            if (tid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙tid_null" });
            }
            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModel($"storeid={storeModel.Id} and state>=0 and id={tid}");
            if (technicianInfo == null)
            {
                return Json(new { isok = false, msg = "该技师不存在" });
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙orderId_null" });
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModelByOrderIdAndAid(orderId, appId, 0);
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙order_null" });
            }
            int state = Context.GetRequestInt("state", 0);
            if (state <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙state_null" });
            }

            EntGoodsCart cartInfo = EntGoodsCartBLL.SingleModel.GetModelByGoodsOrderId(orderInfo.Id, 1);
            if (cartInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙cart_null" });
            }
            TechnicianInfo beforeTechnicianInfo = TechnicianInfoBLL.SingleModel.GetModelById(cartInfo.technicianId);
            if (beforeTechnicianInfo == null)
            {
                return Json(new { isok = false, msg = "原订单技师不存在" });
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetServiceById(cartInfo.aId, cartInfo.FoodGoodsId);
            if (serviceInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙serviceInfo_null" });
            }

            #endregion 数据验证

            //假设没有修改技师
            if (technicianInfo.id == beforeTechnicianInfo.id)
            {
                //减少单数
                if ((orderInfo.State == (int)MiniAppEntOrderState.已完成 || orderInfo.State == (int)MiniAppEntOrderState.已超时) && (state != (int)MiniAppEntOrderState.已完成 && state != (int)MiniAppEntOrderState.已超时))
                {
                    technicianInfo.serviceCount--;
                    serviceInfo.salesCount--;
                }
                //增加单数
                else if ((orderInfo.State != (int)MiniAppEntOrderState.已完成 && orderInfo.State != (int)MiniAppEntOrderState.已超时) && (state == (int)MiniAppEntOrderState.已完成 || state == (int)MiniAppEntOrderState.已超时))
                {
                    technicianInfo.serviceCount++;
                    serviceInfo.salesCount++;
                }

                //同步技师工作状态
                if (state == (int)MiniAppEntOrderState.已完成 || state == (int)MiniAppEntOrderState.已超时)
                {
                    technicianInfo.state = (int)TechnicianState.空闲;
                }
                else if (state == (int)MiniAppEntOrderState.服务中)
                {
                    technicianInfo.state = (int)TechnicianState.上钟;
                }
                else if (state == (int)MiniAppEntOrderState.待服务)
                {
                    technicianInfo.state = (int)TechnicianState.空闲;
                }
            }
            else //前后技师不一致
            {
                //减少原先技师的接单数
                if ((orderInfo.State == (int)MiniAppEntOrderState.已完成 || orderInfo.State == (int)MiniAppEntOrderState.已超时) && (state != (int)MiniAppEntOrderState.已完成 && state != (int)MiniAppEntOrderState.已超时))
                {
                    beforeTechnicianInfo.serviceCount--;
                    serviceInfo.salesCount--;
                }
                //增加之后技师的接单数
                else if ((orderInfo.State != (int)MiniAppEntOrderState.已完成 && orderInfo.State != (int)MiniAppEntOrderState.已超时) && (state == (int)MiniAppEntOrderState.已完成 || state == (int)MiniAppEntOrderState.已超时))
                {
                    technicianInfo.serviceCount++;
                    serviceInfo.salesCount++;
                }
                //更换技师应该把接单数累计到更改后的技师上,减掉原技师接单数
                else if ((orderInfo.State == (int)MiniAppEntOrderState.已完成 || orderInfo.State == (int)MiniAppEntOrderState.已超时) && (state == (int)MiniAppEntOrderState.已完成 || state == (int)MiniAppEntOrderState.已超时))
                {
                    beforeTechnicianInfo.serviceCount--;
                    technicianInfo.serviceCount++;
                }

                //同步技师工作状态
                if (state == (int)MiniAppEntOrderState.已完成 || state == (int)MiniAppEntOrderState.已超时)
                {
                    technicianInfo.state = (int)TechnicianState.空闲;
                    beforeTechnicianInfo.state = (int)TechnicianState.空闲;
                }
                else if (state == (int)MiniAppEntOrderState.服务中)
                {
                    technicianInfo.state = (int)TechnicianState.上钟;
                    beforeTechnicianInfo.state = (int)TechnicianState.空闲;
                }
                else if (state == (int)MiniAppEntOrderState.待服务)
                {
                    //考虑如果被换过来的技师,其他项目还没完结,只是安排待服务,那么不能影响她原来的状态
                    //technicianInfo.state = (int)TechnicianState.休息中;
                    beforeTechnicianInfo.state = (int)TechnicianState.空闲;
                }
            }

            //修改之前的服务时间，如果修改成功要将此时间从已预订时间表里取消
            DateTime beforeTime = cartInfo.reservationTime;
            try
            {
                cartInfo.reservationTime = Convert.ToDateTime(reservationTime);
            }
            catch
            {
                return Json(new { isok = false, msg = "时间格式不合法" });
            }
            //ServiceTime dates = null;

            ServiceTime dates = ServiceTimeBLL.SingleModel.GetModel(storeModel.appId, technicianInfo.id, storeModel.Id, cartInfo.reservationTime.ToShortDateString());
            if (DateTime.Compare(beforeTime, cartInfo.reservationTime) != 0)
            {
                //验证服务时间是否已经被选
                if (dates != null && !string.IsNullOrEmpty(dates.time))
                {
                    List<string> list = dates.time.Split(',').ToList(); ;
                    if (list.Contains(cartInfo.reservationTime.ToString("HH:mm")))
                    {
                        return Json(new { isok = -1, msg = "这个点已经被预订了" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            orderInfo.Remark = remark;
            orderInfo.State = state;
            int beforeTid = cartInfo.technicianId;
            cartInfo.technicianId = tid;

            bool isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "remark,state") && EntGoodsCartBLL.SingleModel.Update(cartInfo, "roomNo,technicianId,reservationTime");
            if (isok)
            {
                if (DateTime.Compare(beforeTime, cartInfo.reservationTime) != 0 || cartInfo.technicianId != beforeTid)
                {
                    //修改成功后将选定的时间点添加到已服务时间表
                    ServiceTimeBLL.SingleModel.AddSelServiceTime(storeModel, cartInfo, storeModel.switchModel);
                    //取消已预订的技师服务时间
                    dates = ServiceTimeBLL.SingleModel.GetModel(storeModel.appId, beforeTid, storeModel.Id, beforeTime.ToShortDateString());
                    if (dates != null && !string.IsNullOrEmpty(dates.time))
                    {
                        List<string> timeList = dates.time.Split(',').ToList();
                        timeList.Remove(beforeTime.ToString("HH:mm"));
                        dates.time = string.Join(",", timeList);
                        ServiceTimeBLL.SingleModel.Update(dates, "time");
                    }
                }
                //前后技师不同时,才会去更新前者的状态
                if (beforeTechnicianInfo.id != technicianInfo.id)
                {
                    TechnicianInfoBLL.SingleModel.Update(beforeTechnicianInfo, "state,serviceCount");
                }
                TechnicianInfoBLL.SingleModel.Update(technicianInfo, "state,serviceCount");
                EntGoodsBLL.SingleModel.Update(serviceInfo, "salesCount");
            }
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg });
        }

        #endregion 订单管理
    }
}