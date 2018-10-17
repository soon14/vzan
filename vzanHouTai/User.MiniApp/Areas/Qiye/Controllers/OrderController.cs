using BLL.MiniApp;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Qiye;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Qiye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Qiye.Filters;
using Utility.IO;

namespace User.MiniApp.Areas.Qiye.Controllers
{
    [LoginFilter]
    public class OrderController : User.MiniApp.Controllers.baseController
    {

        public ActionResult Index(int aid=0)
        {
            ViewBag.appId = aid;
            return View();
        }
        
        public ActionResult GetDataList()
        {
            Return_Msg returnData = new Return_Msg();
            int aid = Context.GetRequestInt("aid",0);
            int state = Context.GetRequestInt("state", -999);
            int getWay = Context.GetRequestInt("getway", -999);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            string orderNum = Context.GetRequest("ordernum", "");
            string ladingCode = Context.GetRequest("ladingcode", "");
            string accepterName = Context.GetRequest("acceptername", "");
            string accepterTelephone = Context.GetRequest("acceptertelephone", "");
            
            int count = 0;
            List<QiyeGoodsOrder> list = QiyeGoodsOrderBLL.SingleModel.GetDataList(accepterTelephone, accepterName, ladingCode, orderNum, getWay, state, aid, pageSize, pageIndex, ref count);

            returnData.dataObj = new { list = list, count = count };
            returnData.isok = true;

            return Json(returnData);
        }

        /// <summary>
        /// 变更订单状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="oldState"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult UpdteOrderState()
        {
            Return_Msg returnData = new Return_Msg();

            int state = Context.GetRequestInt("state", 0);
            int oldState = Context.GetRequestInt("oldState", 0);
            int id = Context.GetRequestInt("orderId", 0);
            string attachData = Context.GetRequest("attachData", "");
            if (id<=0)
            {
                returnData.Msg = "参数错误";
                return Json(returnData);
            }

            if (!Enum.IsDefined(typeof(QiyeOrderState), state))
            {
                returnData.Msg = "无效状态,请刷新再试";
                return Json(returnData);
            }

            QiyeGoodsOrder order = QiyeGoodsOrderBLL.SingleModel.GetModel(id);
            if(order==null || order.State!= oldState)
            {
                returnData.Msg = "无效订单，请刷新再试";
                return Json(returnData);
            }

            ServiceResult result = new ServiceResult();
            string msg = "";
            switch (state)
            {
                case (int)QiyeOrderState.已取消://取消订单
                    QiyeGoodsOrderBLL.SingleModel.CancelOrder(order,ref msg);
                    returnData.Msg = msg;
                    break;
                case (int)QiyeOrderState.待收货:
                    QiyeGoodsOrderBLL.SingleModel.SendGoods(order, attachData, ref msg);
                    returnData.Msg = msg;
                    break;
                case (int)QiyeOrderState.已完成:
                    QiyeGoodsOrderBLL.SingleModel.ReceiptGoods(order, ref msg);
                    returnData.Msg = msg;
                    break;
                case (int)QiyeOrderState.退款中://退款
                    QiyeGoodsOrderBLL.SingleModel.ReturnOrder(order, ref msg);
                    returnData.Msg = msg;
                    break;
                case (int)QiyeOrderState.退款失败://退款
                    QiyeGoodsOrderBLL.SingleModel.ReturnOrderAgain(order, ref msg);
                    returnData.Msg = msg;
                    break;
                default:
                    returnData.Msg = "无效操作类型";
                    break;
            }

            returnData.isok = returnData.Msg.Length <=0;
            returnData.Msg = returnData.isok?"保存成功":"保存失败";
            
            return Json(returnData);
        }

        /// <summary>
        /// 修改订单
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateOrderMaterial()
        {
            Return_Msg returnData = new Return_Msg();
            string orderJson = Context.GetRequest("orderJson", string.Empty);

            if (string.IsNullOrWhiteSpace(orderJson))
            {
                returnData.Msg = "服务器繁忙,未接收到订单数据";
                return Json(returnData);
            }

            string columnstr = "Address,AccepterName,AccepterTelePhone";
            QiyeGoodsOrder order = null;//要修改成的订单数据
            QiyeGoodsOrder dbOrder = null; //数据库订单
            
            try
            {
                //订单
                order = Newtonsoft.Json.JsonConvert.DeserializeObject<QiyeGoodsOrder>(orderJson);    
                if (order == null || order.Id <= 0)
                {
                    returnData.Msg = $"订单数据出现异常";
                    return Json(returnData);
                }
            }
            catch (Exception)
            {
                returnData.Msg = "订单数据存在异常";
                return Json(returnData);
            }
            dbOrder = QiyeGoodsOrderBLL.SingleModel.GetModel(order.Id);
            if (dbOrder == null)
            {
                returnData.Msg = "订单数据有异常";
                return Json(returnData);
            }
            
            dbOrder.Address = order.Address;
            dbOrder.AccepterName = order.AccepterName;
            dbOrder.AccepterTelePhone = order.AccepterTelePhone;

            //微信支付,金额不等才去重新生成微信订单并关闭原有订单
            if (dbOrder.OrderId > 0 && dbOrder.BuyPrice != order.BuyPrice)
            {
                columnstr += ",OrderId";
                //关闭原微信订单
                string errorMsg = "";
                dbOrder.OrderId = new JsApiPay(HttpContext).updateWxOrderMoney(dbOrder.OrderId, order.BuyPrice, ref errorMsg);
                if (dbOrder.OrderId <= 0 || errorMsg.Length>0)
                {
                    returnData.Msg = errorMsg;
                    return Json(returnData);
                }

                columnstr += ",BuyPrice,ReducedPrice";
                dbOrder.ReducedPrice += dbOrder.BuyPrice - order.BuyPrice;//重新累计优惠金额
                dbOrder.BuyPrice = order.BuyPrice;
            }
            
            bool isSuccess = QiyeGoodsOrderBLL.SingleModel.Update(dbOrder, columnstr);
            if (isSuccess)
            {
                returnData.isok = true;
                returnData.Msg = "修改订单资料成功";
            }
            else
            {
                returnData.Msg = "修改订单资料失败";
            }
            return Json(returnData);
        }

        public ActionResult OrderEdit(int appid = 0)
        {
            int id = Context.GetRequestInt("id",0);
            if(id<=0)
            {
                return Redirect("/base/PageError?type=5");
            }
            ViewBag.appId = appid;
            QiyeGoodsOrder order = QiyeGoodsOrderBLL.SingleModel.GetModel(id);
            if(order==null)
            {
                return Redirect("/base/PageError?type=4");
            }
            return View(order);
        }

        /// <summary>
        /// 获取订单退款失败原因
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult GetOutOrderFailRemark(int aid=0,int orderId = 0)
        {
            Return_Msg returnData = new Return_Msg();
            try
            {
                QiyeGoodsOrder order = QiyeGoodsOrderBLL.SingleModel.GetModel(orderId);
                CityMorders ctiyMorder = new CityMordersBLL().GetModel(order.OrderId);
                ReFundResult outOrderRsult = RefundResultBLL.SingleModel.GetModelByTradeno(ctiyMorder.trade_no);

                if (outOrderRsult == null)
                {
                    returnData.Msg = "未知原因！";
                }
                else
                {
                    returnData.Msg  = outOrderRsult.err_code_des ?? outOrderRsult.return_msg;
                    returnData.isok = true;
                }
            }
            catch (Exception ex)
            {
                returnData.Msg = ex.Message;
            }
            return Json(returnData);
        }
    }
}