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
using MySql.Data.MySqlClient;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Helper;
using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Footbath;
using BLL.MiniApp.User;

namespace User.MiniApp.Controllers
{
    public partial class footbathController : configController
    {
        // GET: footbath_reservemanager:足浴版小程序-预订管理
        #region 预订管理
        /// <summary>
        /// 预订配置 界面
        /// </summary>
        /// <returns></returns>
        public ActionResult ReserveSetting()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            return View(storeModel);
        }
        /// <summary>
        /// 保存预订配置
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SaveReserveSetting(FootBath info)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "系统繁忙auth_null" });
            }

            if (info == null)
            {
                return Json(new { isok = false, msg = "参数错误" });
            }

            if (info.appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙id_null" });
            }

            XcxAppAccountRelation appAcountRelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(info.appId, dzaccount.Id.ToString());
            if (appAcountRelation == null)
            {
                return Json(new { isok = false, msg = "系统繁忙relation_null" });
            }

            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={info.appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            if (info.switchModel.TimeInterval != 30 && info.switchModel.TimeInterval != 60)
            {
                return Json(new { isok = false, msg = "系统繁忙TimeInterval_error" });
            }
            storeModel.switchModel.TimeInterval = info.switchModel.TimeInterval;
            if (info.switchModel.PresetTime < 1 || info.switchModel.PresetTime > 15)
            {
                return Json(new { isok = false, msg = "预订时间只能在1~15天之间" });
            }
            storeModel.switchModel.PresetTime = info.switchModel.PresetTime;
            storeModel.switchModel.AdvancePayment = info.switchModel.AdvancePayment;
            storeModel.switchModel.PresetTechnician = info.switchModel.PresetTechnician;
            storeModel.switchModel.WriteSex = info.switchModel.WriteSex;
            storeModel.switchModel.WriteDesc = info.switchModel.WriteDesc;
            storeModel.switchModel.AutoLock = info.switchModel.AutoLock;
            storeModel.SwitchConfig = JsonConvert.SerializeObject(storeModel.switchModel);
            storeModel.UpdateDate = DateTime.Now;
            bool isok = FootBathBLL.SingleModel.Update(storeModel, "switchconfig,updatedate");
            string msg = isok ? "保存成功" : "保存失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 预订列表（界面）
        /// </summary>
        /// <returns></returns>
        public ActionResult ReserveRecord()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "数据不存在!", code = "500" });
            }
            List<EntGoodType> roomList = EntGoodTypeBLL.SingleModel.GetRoomList(appId, storeModel.Id,(int)GoodProjectType.足浴版包间分类);
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            ViewBag.dateList = FootBathBLL.SingleModel.GetReservationTime(storeModel.switchModel.PresetTime);
            return View(roomList);

        }

        /// <summary>
        /// 获取预订列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetReserveRecord()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string name = Context.GetRequest("name", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string servicetime = Context.GetRequest("servicetime", string.Empty);
            string serviceName = Context.GetRequest("serviceName", string.Empty);
            string roomName = Context.GetRequest("roomName", string.Empty);
            string jobNumber = Context.GetRequest("jobNumber", string.Empty);
            int payState = Context.GetRequestInt("payState", -1);
            int state = Context.GetRequestInt("state", -6);

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
                roomIds = string.Join(",", roomList.Select(t => t.id).ToList());
            }
            //搜索条件：服务项目
            if (!string.IsNullOrEmpty(serviceName))
            {
                
                List<EntGoods> serviceList = EntGoodsBLL.SingleModel.GetServiceByName(appId, (int)GoodsType.足浴版服务,serviceName);
                   
                if (serviceList == null || serviceList.Count <= 0)
                {
                    return Json(new { isok = true, recordCount = 0, list = list });
                }
                serviceIds = string.Join(",", serviceList.Select(t => t.id).ToList());
            }
            //搜索条件：技师工号
            if (!string.IsNullOrEmpty(jobNumber))
            {
                
                List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetTechnicianListByJobNumeberOrWorkType(storeModel.Id, jobNumber,-1);
                if (technicianList == null || technicianList.Count <= 0)
                {
                    return Json(new { isok = true, recordCount = 0, list = list });
                }
                technicianIds = string.Join(",", technicianList.Select(t => t.id).ToList());
            }
            int recordCount = 0;
            list = EntGoodsOrderBLL.SingleModel.GetFootbathOrderList(appId, name, phone, servicetime, payState, state, roomIds, serviceIds, technicianIds, pageIndex, pageSize, (int)EntOrderType.预约订单, out recordCount);
            List<ServiceTime> timeList = ServiceTimeBLL.SingleModel.GetList();//测试时的数据校验
            return Json(new { isok = true, recordCount = recordCount, list = list, timeList = timeList });
        }
        /// <summary>
        /// 取消预订
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateState()
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
            FootBath storeModel = FootBathBLL.SingleModel.GetModel($"appId={appId}");
            if (storeModel == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            int id = Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙orderId_null" });
            }
            int state = Context.GetRequestInt("state", -7);
            if (state <= -7)
            {
                return Json(new { isok = false, msg = "系统繁忙state_null" });
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModelByAidAndId(appId,id,1);
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙model_null" });
            }
            //是否是要直接取消的订单<未支付的订单取消订单都是直接取消>
            bool isCanncleOrder = orderInfo.PayDate == Convert.ToDateTime("0001-01-01 00:00:00");
            if (isCanncleOrder)
            {
                orderInfo.State = (int)MiniAppEntOrderState.已取消;
            }
            else
            {
                orderInfo.State = state;
            }
            EntGoodsCart cart = EntGoodsCartBLL.SingleModel.GetModelByGoodsOrderId(orderInfo.Id);
            if (cart == null)
            {
                return Json(new { isok = false, msg = "服务项目不存在" });
            }
            ServiceTime serviceTime = ServiceTimeBLL.SingleModel.GetModel(storeModel.appId,cart.technicianId,storeModel.Id, cart.reservationTime.ToShortDateString());
            //取消已预订的技师服务时间
            if (serviceTime != null && !string.IsNullOrEmpty(serviceTime.time))
            {
                List<string> timeList = serviceTime.time.Split(',').ToList();
                timeList.Remove(cart.reservationTime.ToString("HH:mm"));
                serviceTime.time = string.Join(",", timeList);
            }
            bool isok = false;

            if (isCanncleOrder)
            {
                isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "State");
            }
            else
            {
                isok = EntGoodsOrderBLL.SingleModel.outOrder(appAcountRelation.AppId, orderInfo, serviceTime);
            }

            #region 取消预约通知 模板消息
            if (isok && !isCanncleOrder)
            {
                object objData = TemplateMsg_Miniapp.FootbathGetTemplateMessageData(orderInfo, SendTemplateMessageTypeEnum.足浴预约取消通知);
                TemplateMsg_Miniapp.SendTemplateMessage(orderInfo.UserId, SendTemplateMessageTypeEnum.足浴预约取消通知, TmpType.小程序足浴模板, objData);

            }
            #endregion

            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 获取技师服务时间表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetServiceTimeTable()
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
            //获取可做此服务的技师
            int serviceId = Context.GetRequestInt("serviceId", 0);
            if (serviceId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙serviceId_null" });
            }
            EntGoods serviceInfo = EntGoodsBLL.SingleModel.GetServiceById(storeModel.appId,serviceId,1);
            if (serviceInfo == null)
            {
                return Json(new { isok = -1, msg = "该服务已下架" }, JsonRequestBehavior.AllowGet);
            }
            List<TechnicianInfo> technicianList = TechnicianInfoBLL.SingleModel.GetListByServiceId(storeModel.Id,serviceInfo.id);


            //获取技师的服务时间表
            int tid = Context.GetRequestInt("tid", 0);
            if (tid <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙tid_null" });
            }
            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModelById(tid);
            if (technicianInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙technicianInfo_null" });
            }
            storeModel.switchModel = JsonConvert.DeserializeObject<SwitchModel>(storeModel.SwitchConfig);
            List<object> timeList = new List<object>();
            for (int i = 0; i <= storeModel.switchModel.PresetTime; i++)
            {
                object timeTable = ServiceTimeBLL.SingleModel.GetTimeTable(storeModel.switchModel, i, storeModel.appId, storeModel.Id, technicianInfo.id);
                timeList.Add(timeTable);
            }
            return Json(new { isok = true, timeList = timeList, technicianList = technicianList });
        }
        /// <summary>
        /// 保存更改
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveReserve()
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
            TechnicianInfo technicianInfo = TechnicianInfoBLL.SingleModel.GetModelById(tid);
            if (technicianInfo == null)
            {
                return Json(new { isok = false, msg = "该技师不存在" });
            }
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙orderId_null" });
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModelByOrderIdAndAid(orderId,appId,1);
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙order_null" });
            }
            #endregion
            orderInfo.Remark = remark;
            orderInfo.State = (int)MiniAppEntOrderState.待服务;
            EntGoodsCart cartInfo = EntGoodsCartBLL.SingleModel.GetModelByGoodsOrderId(orderInfo.Id,1);
            if (cartInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙cart_null" });
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
            //验证服务时间是否已经被选
            ServiceTime dates = ServiceTimeBLL.SingleModel.GetModelByDate(storeModel.appId,storeModel.Id, cartInfo.reservationTime.ToShortDateString());
            if (dates != null && !string.IsNullOrEmpty(dates.time))
            {
                List<string> list = dates.time.Split(',').ToList(); ;
                if (list.Contains(cartInfo.reservationTime.ToString("HH:mm")))
                {
                    return Json(new { isok = -1, msg = "这个点已经被预订了" }, JsonRequestBehavior.AllowGet);
                }
            }
            //cartInfo.roomNo = roomNo;
            int beforeTid = cartInfo.technicianId;
            cartInfo.technicianId = tid;
            bool isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "remark,state") && EntGoodsCartBLL.SingleModel.Update(cartInfo, "roomNo,technicianId,reservationTime");
            if (isok)
            {
                if (DateTime.Compare(beforeTime, cartInfo.reservationTime) != 0 || cartInfo.technicianId!= beforeTid)
                {
                    //取消已预订的技师服务时间
                    dates = ServiceTimeBLL.SingleModel.GetModel(storeModel.appId, beforeTid,storeModel.Id,beforeTime.ToShortDateString());
                    if (dates != null && !string.IsNullOrEmpty(dates.time))
                    {
                        List<string> timeList = dates.time.Split(',').ToList();
                        timeList.Remove(beforeTime.ToString("HH:mm"));
                        dates.time = string.Join(",", timeList);
                        ServiceTimeBLL.SingleModel.Update(dates, "time");
                    }
                    //修改成功后将选定的时间点添加到已服务时间表
                    dates = ServiceTimeBLL.SingleModel.GetModel(storeModel.appId, cartInfo.technicianId, storeModel.Id, beforeTime.ToShortDateString());
                    ServiceTimeBLL.SingleModel.AddSelServiceTime(storeModel, cartInfo, storeModel.switchModel);
                }
            }
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg });
        }

        /// <summary>
        /// 开单
        /// </summary>
        /// <returns></returns>
        public ActionResult createOrder()
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
            int orderId = Context.GetRequestInt("orderId", 0);
            if (orderId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙orderId_null" });
            }
            EntGoodsOrder orderInfo = EntGoodsOrderBLL.SingleModel.GetModelByOrderIdAndAid(orderId,appId,1);
            if (orderInfo == null)
            {
                return Json(new { isok = false, msg = "系统繁忙order_null" });
            }
            #endregion

            if (orderInfo.State == (int)MiniAppEntOrderState.已超时)
            {
                orderInfo.State = (int)MiniAppEntOrderState.服务中;
            }
            orderInfo.OrderType = 0;
            orderInfo.ConfDate = DateTime.Now;
            bool isok = EntGoodsOrderBLL.SingleModel.Update(orderInfo, "State,ordertype,ConfDate") && VipRelationBLL.SingleModel.updatelevel(orderInfo.UserId, "footbath");
            #region 预约成功通知 模板消息
            if (isok)
            {
                object objData = TemplateMsg_Miniapp.FootbathGetTemplateMessageData(orderInfo, SendTemplateMessageTypeEnum.足浴预约成功通知);
                TemplateMsg_Miniapp.SendTemplateMessage(orderInfo.UserId, SendTemplateMessageTypeEnum.足浴预约成功通知, TmpType.小程序足浴模板, objData);
            }
            #endregion
            string msg = isok ? "操作成功" : "操作失败";
            return Json(new { isok = isok, msg = msg });
        }
        #endregion

    }
}