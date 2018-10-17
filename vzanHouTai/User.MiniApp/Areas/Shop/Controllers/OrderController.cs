using BLL.MiniApp;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using Core.MiniApp.Common;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;


namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("Order"), Route("{action}/{orderId?}")]
    [LoginFilter(storePara: "store", getAuthStore: true)]
    public class OrderController : BaseController
    {
        [HttpGet]
        public JsonResult List(DishStore store,
            int pageIndex = 1, int pageSize = 10,
            string orderNo = null, int orderState = -1, int orderType = 0, int payState = -1, int payWay = 0, int pickUp = -1,
            bool isToday = false, DateTime? start = null, DateTime? end = null)
        {
            if (pickUp == 1) //订单类型为自取
            {
                orderType = (int)DishEnums.OrderType.店内;
            }

            if (isToday)//是否今日订单
            {
                start = DateTime.Now.Date;
                end = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
            }

            List<DishOrder> orders = DishOrderBLL.SingleModel.GetOrders(store.aid, store.id, orderNo, orderState, orderType, payState, payWay, start, end, pageIndex, pageSize, is_ziqu: pickUp);
            int total = DishOrderBLL.SingleModel.GetOrdersCount(store.aid, store.id, orderNo, orderState, orderType, payState, payWay, start, end, is_ziqu: pickUp);

            object formatDTO = orders.Select(order =>
            {
                return new
                {
                    Id = order.id,
                    OrderNo = order.order_sn,
                    OrderDate = order.ctime.ToString(),
                    OrderStateEnum = order.order_status,
                    OrderState = order.order_status_txt,
                    OrderTypeEnum = order.order_type,
                    OrderType = order.order_type_txt,
                    UserName = order.user_name,
                    IsPickUp = order.is_ziqu == 1,
                    PickUpName = order.ziqu_username,
                    PickUpPhoneNo = order.ziqu_userphone,
                    ReceivingName = order.consignee,
                    ReceivingPhoneNo = order.mobile,
                    Address = order.address,
                    PayAmount = order.settlement_total_fee,
                    Table = order.order_table_id == "0" ? null : order.order_table_id,
                };
            });

            formatDTO = new { page = formatDTO, total };

            return ApiModel(isok: true, message: "获取成功", data: formatDTO);
        }

        [HttpGet]
        public JsonResult Detail(DishStore store, int? orderId)
        {
            if(!orderId.HasValue)
            {
                return ApiModel(message: "店铺ID不能为空");
            }
            DishOrder order = DishOrderBLL.SingleModel.GetModel(orderId.Value);
            if (order == null)
            {
                return ApiModel(message: "无效订单，数据不存在");
            }

            if (order.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }

            //订单商品
            List<DishShoppingCart> orderItem = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(order.id);
            //订单配送员
            List<DishTransporter> deliveryUnit = DishTransporterBLL.SingleModel.GetTransportersByparams(order.aId, order.storeId, true) ?? new List<DishTransporter>();

            object formatItem = orderItem?.Select(item => new
            {
                Id = item.goods_id,
                Name = item.goods_name,
                Img = item.goods_img,
                Price = item.goods_price,
                Spec = item.goods_attr,
                Amount = item.goods_number,
            });

            object formatOrder = new
            {
                Id = order.id,
                OrderNo = order.order_sn,
                OrderDate = order.ctime.ToString(),
                OrderStateEnum = order.order_status,
                OrderState = order.order_status_txt,
                OrderTypeEnum = order.order_type,
                OrderType = order.order_type_txt,

                PayStateEnum = order.pay_status,
                PayState = order.pay_status_txt,
                PayDate = order.pay_time.ToString(),
                PayAmount = order.order_amount * 0.01,
                PackingFee = order.dabao_fee * 0.01,
                NewUserDiscount = order.huodong_shou_jiner,
                Discount = order.huodong_quan_jiner,
                FullDiscount = order.huodong_manjin_jiner,

                UserName = order.user_name,
                IsPickUp = order.is_ziqu == 1,
                PickUpName = order.ziqu_username,
                PickUpPhoneNo = order.ziqu_userphone,
                ReceivingName = order.consignee,
                ReceivingPhoneNo = order.mobile,
                DeliveryName = order.peisong_user_name,
                DeliveryPhoneNo = order.peisong_user_phone,
                DeliveryFee = order.shipping_fee * 0.01,
                DeliveryStateEnum = order.peisong_status,
                DeliveryState = order.peisong_status_text,
                Address = order.address,

                Table = order.order_table_id == "0" ? null : order.order_table_id,
                Invoice = order.is_fapiao,
                InvoiceTitle = order.fapiao_text,
                InvoiceNo = order.fapiao_no,
                InvoiceType = order.fapiao_leixing_txt,
                UserRemark = order.post_info,
                StoreRemark = order.GetAttrbute().mark ?? "",
            };

            return ApiModel(isok: true, message: "获取成功", data: new { Order = formatOrder, Item = formatItem });
        }

        [HttpPost]
        public JsonResult UpdateState(DishStore store, string orderId, string state)
        {
            orderId = string.Join(",", orderId.ConvertToIntList(',').FindAll(id => id > 0));
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return ApiModel(message: "参数不能为空[orderId]");
            }
            if(string.IsNullOrWhiteSpace(state))
            {
                return ApiModel(message: "参数不能为空[state]");
            }

            List<DishOrder> orders = DishOrderBLL.SingleModel.GetListByIds(orderId);
            if (orders == null || orders.Count == 0)
            {
                return ApiModel(message: "无效订单，数据不存在");
            }

            if (orders.Exists(item => item.storeId != store.id))
            {
                return ApiModel(message: "非法操作");
            }

            bool success = false;
            int[] updateOrder = orders.Select(item => item.id).ToArray();
            switch (state)
            {
                case "pay":
                    success = DishOrderBLL.SingleModel.BtachModifyPayState(updateOrder, DishEnums.PayState.已付款); break;
                case "delete":
                    success = DishOrderBLL.SingleModel.BtachDelete(updateOrder); break;
                default:
                    Dictionary<string, DishEnums.OrderState> matchState = new Dictionary<string, DishEnums.OrderState>
                    {
                        { "confirm" , DishEnums.OrderState.已确认 },
                        { "complete" , DishEnums.OrderState.已完成 },
                        { "cancel",DishEnums.OrderState.已取消 } ,
                    };
                    DishEnums.OrderState newState;
                    success = matchState.TryGetValue(state, out newState) && DishOrderBLL.SingleModel.BtachModifyOrderState(updateOrder, newState);
                    break;
            }

            return ApiModel(isok: success, message: success ? "更新成功" : $"更新失败[{state}]");
        }

        [HttpPost,Route("Update/{doWhat}/{orderId?}")]
        public JsonResult Update(DishStore store, string doWhat, int? orderId, int? tableId, int? deliveryUnit)
        {
            if(!orderId.HasValue)
            {
                return ApiModel(message: "参数不能为空[orderId]");
            }

            DishOrder order = DishOrderBLL.SingleModel.GetModel(orderId.Value);
            if(order?.storeId != store.id)
            {
                return ApiModel(message: "无效订单");
            }

            bool success = false;
            switch (doWhat)
            {
                case "Table":
                    if(!tableId.HasValue)
                    {
                        return ApiModel(message: "未找到相关桌台");
                    }
                    DishTable table = DishTableBLL.SingleModel.GetModel(tableId.Value);
                    if (table == null || table.storeId != store.id)
                    {
                        return ApiModel(message: "未找到相关桌台");
                    }
                    order.order_table_id_zhen = table.id;
                    order.order_table_id = table.table_name;
                    success = DishOrderBLL.SingleModel.Update(order, "order_table_id,order_table_id_zhen");
                    break;
                case "PickUp":
                    order.peisong_status = (int)DishEnums.DeliveryState.待取货;
                    success = DishOrderBLL.SingleModel.Update(order, "peisong_status");
                    break;
                case "Cancel":
                    order.peisong_status = (int)DishEnums.DeliveryState.已取消;
                    success = DishOrderBLL.SingleModel.Update(order, "peisong_status");
                    break;
                case "Complete":
                    order.peisong_status = (int)DishEnums.DeliveryState.已完成;
                    success = DishOrderBLL.SingleModel.Update(order, "peisong_status");
                    break;
                case "Delivery":
                    if(!deliveryUnit.HasValue)
                    {
                        return ApiModel(message: "参数不能为空[deliveryUnit]");
                    }
                    DishTransporter transporter = DishTransporterBLL.SingleModel.GetModel(deliveryUnit.Value);
                    if (transporter == null)
                    {
                        return ApiModel(message: "未找到配送员资料");
                    }
                    order.peisong_open = 1;
                    order.peisong_status = (int)DishEnums.DeliveryState.配送中;
                    order.peisong_user_name = transporter.dm_name;
                    order.peisong_user_phone = transporter.dm_mobile;
                    success = DishOrderBLL.SingleModel.Update(order, "peisong_open,peisong_status,peisong_user_name,peisong_user_phone");
                    break;
            }

            return ApiModel(isok:success,message: success ? "操作成功" : "操作失败");
        }

        [HttpPost, Route("UpdateItem/{itemId?}")]
        public JsonResult UpdateItem(DishStore store, int? itemId, int count = 0)
        {
            if(!itemId.HasValue)
            {
                return ApiModel(message: "参数不能为空[itemId]");
            }

            DishShoppingCart item = DishShoppingCartBLL.SingleModel.GetModel(itemId.Value);
            if (item == null)
            {
                return ApiModel(message: "订单商品不存在");
            }

            DishOrder order = DishOrderBLL.SingleModel.GetModel(item.order_id);
            if (order.huodong_manjin_jiner > 0 && order.huodong_manjian_id == 0)
            {
                return ApiModel(message: "2018-8-13前享受满减的订单，不能修改");
            }

            List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetShoppingCart(item.aId, item.storeId, item.user_id, order.id) ?? new List<DishShoppingCart>();//购物车
            List<DishGood> goods = DishGoodBLL.SingleModel.GetGoodsByIds(carts.Select(c => c.goods_id)?.ToArray()); //购物车内商品详细资料
            if (goods == null || carts.Any(c => !goods.Any(g => c.goods_id == g.id)))
            {
                return ApiModel(message: "菜品不存在");
            }

            item.goods_number += count;
            if (item.goods_number < 0)
                item.goods_number = 0;

            if (item.goods_number <= 0 && carts.Count == 1)
            {
                return ApiModel(message: "订单至少要有一个菜品");
            }

            DishReturnMsg result = new DishReturnMsg();
            bool buildResult = DishOrderBLL.SingleModel.ReBuildOrder(order, store, item, carts, goods, result);

            return ApiModel(isok: buildResult, message: buildResult ? "设置成功" : "设置失败");
        }

        [HttpPost]
        public JsonResult Print(DishStore store, int? orderId, string printer = null)
        {
            if (!orderId.HasValue)
            {
                return ApiModel(message: "参数不能为空[orderId]");
            }

            DishOrder order = DishOrderBLL.SingleModel.GetModel(orderId.Value);
            try
            {
                PrinterHelper.DishPrintOrderByPrintType(order, 0, printer);
            }
            catch(Exception ex)
            {
                return ApiModel(message: ex.Message);
            }

            return ApiModel(isok: true, message: "操作成功");
        }

        [HttpPost]
        public JsonResult Remark(DishStore store, int? orderId, string remark = null)
        {
            if (!orderId.HasValue)
            {
                return ApiModel(message: "参数不能为空[orderId]");
            }

            DishOrder order = DishOrderBLL.SingleModel.GetModel(orderId.Value);
            DishOrderAttrbute orderAttr = order.GetAttrbute();
            orderAttr.mark = remark;
            order.attrbute = JsonConvert.SerializeObject(orderAttr);

            bool success = DishOrderBLL.SingleModel.Update(order, "attrbute");
            return ApiModel(isok: success, message: success ? "操作成功" : "操作失败");
        }

        [HttpPost]
        public JsonResult Refund(DishStore store, int? orderId, string remark = null)
        {
            if(!orderId.HasValue)
            {
                return ApiModel(message: "参数不能为空[orderId]");
            }
            if(DishOrderBLL.SingleModel.GetModel(orderId.Value)?.storeId != store.id)
            {
                return ApiModel(message: "非法操作");
            }
            DishReturnMsg result = new DishReturnMsg();
            DishOrderBLL.SingleModel.RefundOrderById(orderId.Value, result, remark);
            return ApiModel(isok: result.code == 1, message: result.msg, data: result.obj);
        }

        [HttpPost]
        public JsonResult RefundItem(DishStore store, string itemId)
        {
            if(string.IsNullOrWhiteSpace(itemId))
            {
                return ApiModel(message: "参数不能为空[orderId]");
            }

            int[] items = itemId.ConvertToIntList(',').ToArray();
            if(!DishShoppingCartBLL.SingleModel.GetCartsByIds(items).All(item => item.storeId == store.id))
            {
                return ApiModel(message: "非法操作");
            }

            DishReturnMsg result = new DishReturnMsg();
            DishOrderBLL.SingleModel.RefundOrderByCartIds(items, result);
            return ApiModel(isok: result.code == 1, message: result.msg, data: result.obj);
        }

        //[Route("{orderId?}/UpdateTable/{tableId?}")]
        //public JsonResult UpdateTable(DishStore store, int? orderId, int? tableId)
        //{
        //    if(!orderId.HasValue)
        //    {
        //        return ApiModel)()
        //    }
        //}
    }
}