using BLL.MiniApp;
using System.Collections.Generic;
using System.Web.Mvc;
using System.IO;
using Core.MiniApp;
using System;
using Utility.AliOss;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using BLL.MiniApp.Tools;
using Entity.MiniApp.Tools;
using System.Data;
using System.Collections;
using System.Threading;
using Entity.MiniApp;
using BLL.MiniApp.Stores;
using BLL.MiniApp.Conf;
using Entity.MiniApp.Conf;
using BLL.MiniApp.Ent;
using Entity.MiniApp.Ent;
using DAL.Base;
using Utility;
using MySql.Data.MySqlClient;
using User.MiniApp.Filters;
using Newtonsoft.Json;
using log4net;
using Utility.IO;
using Entity.MiniApp.Fds;
using BLL.MiniApp.Fds;

namespace User.MiniApp.Controllers
{
    public class elemanageController : baseController
    {
        
        
        
        

        /// <summary>
        /// 实例化对象
        /// </summary>
        public elemanageController()
        {
            
            
            
            
        }

        public ActionResult Test()
        {
            string ip = WebHelper.GetIP();
            if (ip != "121.33.238.82")
            {
                return View("PageError", new Return_Msg() { Msg = ip + "IP不对!", code = "404" });
            }

            //EleApiConfig model = _eleapiconfigBll.GetModelByRedis(_eleapi._appId);

            return View();
        }

        /// <summary>
        /// 添加订单
        /// </summary>
        /// <returns></returns>
        public ActionResult AddOrder()
        {
            string orderno = Context.GetRequest("orderno", string.Empty);
            FNOrder order = new FNOrder();
            order.chain_store_code = "01";
            order.goods_count = 4;
            //order.invoice = "xxx有限公司";
            //order.is_agent_payment = 0;
            //order.is_invoiced = 1;
            order.items_json = new List<elegood>() { new elegood() {
                //item_id= "fresh0001",
                item_name= "苹果",
                item_quantity= 5,
                item_price= 10.00f,
                item_actual_price= 9.50f,
                //item_size= 1,
                //item_remark="苹果，轻放",
                //is_need_package= 1,
                //is_agent_purchase= 0,
                //agent_purchase_price=10.00f
            },
            new elegood() {
                //item_id= "fresh0002",
                item_name= "香蕉",
                item_quantity= 1,
                item_price= 20.00f,
                item_actual_price= 19.00f,
                //item_size= 2,
                //item_remark="香蕉，轻放",
                //is_need_package= 1,
                //is_agent_purchase= 0,
                //agent_purchase_price=10.00f
            }
            };
            order.notify_url = FNApi._callbackurl;
            //order.notify_url = "http://123.100.120.22:8090";

            order.order_actual_amount = 48.00f;
            order.order_add_time = Convert.ToInt64(FNApi.SingleModel.GetTimeStamp(90));
            //order.order_payment_method = 1;
            //order.order_payment_status = 1;
            //order.order_remark = "用户备注";
            order.order_total_amount = 50.00f;
            //order.order_type = 1;
            order.order_weight = 1f;
            order.partner_order_code = orderno;
            //order.partner_remark = "商户备注信息";
            order.receiver_info = new FNReceiverInfo()
            {
                receiver_name = "李明",
                receiver_primary_phone = "13900000000",
                //receiver_second_phone = "13911111111",
                receiver_address = "广东省广州市天河区林和东路149号之八(林和东公交车站步行140米近广州东站广州体育学院)",
                receiver_longitude = 113.32,
                receiver_latitude = 23.14501,
                //position_source = 1
            };
            //order.require_payment_pay = 50.00f;
            //order.require_receive_time = Convert.ToInt64(_eleapi.GetTimeStamp(90));
            //order.serial_number = "";
            order.transport_info = new FNStoreInfo()
            {
                transport_name = "01测试",
                transport_address = "广东省广州市天河区林和东路149号之八(林和东公交车站步行140米近广州东站广州体育学院)",
                transport_longitude = 113.32,
                transport_latitude = 23.14501,
                //position_source = 1,
                transport_tel = "18718463809",
                //transport_remark = "备注"
            };
            string url = FNApi._addorderapi;
            string result = FNApi.SingleModel.UseFNApi(order, url);
            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryOrder()
        {
            string orderno = Context.GetRequest("orderno", string.Empty);
            object data = new { partner_order_code = orderno };
            string url = FNApi._queryorderapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelOrder()
        {
            string orderno = Context.GetRequest("orderno", string.Empty);
            object data = new FNCancelOrderRequestModel()
            {
                partner_order_code = orderno,
                order_cancel_reason_code = 2,
                order_cancel_code = 0,
                order_cancel_description = "货品不新鲜",
                order_cancel_time = Convert.ToInt64(FNApi.SingleModel.GetTimeStamp()),
            };
            string url = FNApi._cancelorderapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加门店
        /// </summary>
        /// <returns></returns>
        public ActionResult AddStore()
        {
            object data = new
            {
                chain_store_code = "A001",
                chain_store_name = "门店一",
                contact_phone = "13611581190",
                address = "上海市",
                position_source = 3,
                longitude = "109.690773",
                latitude = "19.91243",
                service_code = "1"
            };
            string url = FNApi._addstoreapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);
            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询门店
        /// </summary>
        /// <returns></returns>
        public ActionResult GetEleApiStoreInfo()
        {
            object data = new { chain_store_name = new List<string> { "01测试" } };
            string url = FNApi._querystoreapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);
            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 查询配送服务
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryDelivery()
        {
            string lat = Context.GetRequest("lat", string.Empty);
            string lnt = Context.GetRequest("lnt", string.Empty);
            object data = new
            {
                chain_store_code = "01",
                position_source = 3,
                receiver_longitude = lnt,
                receiver_latitude = lat
            };
            string url = FNApi._querydeliveryapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);
            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelFNOrder()
        {
            string orderid = Context.GetRequest("orderid", string.Empty);
            string orderno = Context.GetRequest("orderno", string.Empty);
            int cancelcode = Context.GetRequestInt("cancel_reason_id", 0);
            string cancel_reason = Context.GetRequest("cancel_reason", string.Empty);

            object data = new FNCancelOrderRequestModel()
            {
                partner_order_code = orderno,
                order_cancel_reason_code = 2,
                order_cancel_code = cancelcode,
                order_cancel_description = cancel_reason,
                order_cancel_time = Convert.ToInt64(FNApi.SingleModel.GetTimeStamp()),
            };
            string url = FNApi._cancelorderapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);
            if(string.IsNullOrEmpty(result))
            {
                return Json(new { isok = false, msg = "取消订单失败" }, JsonRequestBehavior.AllowGet);
            }
            
            FNApiReponseModel<object> reposemodel = JsonConvert.DeserializeObject<FNApiReponseModel<object>>(result);
            if(reposemodel==null || reposemodel.code!=200)
            {
                return Json(new { isok = false, msg = reposemodel==null ?"接口异常":reposemodel.msg }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                FNOrder order = FNOrderBLL.SingleModel.GetModelByOrderNo(orderno);
                if(order == null)
                {
                    return Json(new { isok = false, msg = "找不到蜂鸟订单" }, JsonRequestBehavior.AllowGet);
                }

                order.state = (int)FNOrderEnum.已取消;
                order.updatetime = DateTime.Now;
                order.order_cancel_reason_code = 2;
                order.order_cancel_code = cancelcode;
                order.order_cancel_description = cancel_reason;
                order.order_cancel_time = FNApi.SingleModel.GetTimeStamp().ToString();
                bool success = FNOrderBLL.SingleModel.Update(order, "state,updatetime,order_cancel_reason_code,order_cancel_code,order_cancel_description,order_cancel_time");

                if(!success)
                {
                    return Json(new { isok = false, msg = "修改蜂鸟订单状态失败" }, JsonRequestBehavior.AllowGet);
                }

                FoodGoodsOrder foodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel($" Id = {orderid} ");
                if (foodGoodOrder == null)
                {
                    return Json(new { isok = false, msg = "蜂鸟配送：找不到订单" }, JsonRequestBehavior.AllowGet);
                }
                //退款接口 abel
                if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                {
                    success = FoodGoodsOrderBLL.SingleModel.outOrder(foodGoodOrder, foodGoodOrder.State);
                }
                else if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                {
                    SaveMoneySetUser userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(foodGoodOrder.UserId) ?? new SaveMoneySetUser();
                    success = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(foodGoodOrder, userSaveMoney, foodGoodOrder.State);
                }
            }

            return Json(new { isok = true, msg = result }, JsonRequestBehavior.AllowGet);
        }
    }
}