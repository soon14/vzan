using BLL.MiniApp.Dish;
using BLL.MiniApp.Helper;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.DishAdmin.Filters;
using User.MiniApp.Model;
using User.MiniApp.Areas.DishAdmin.Models;
using DAL.Base;
using Core.MiniApp;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    [LoginFilter]
    public class OrderController : Controller
    {
        private readonly DishReturnMsg _result;
        
        
        public OrderController()
        {
            _result = new DishReturnMsg();
            
            
        }

        /// <summary>
        /// 订单页
        /// </summary>
        /// <param name="act"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <param name="order_sn"></param>
        /// <param name="order_status"></param>
        /// <param name="order_type"></param>
        /// <param name="pay_status"></param>
        /// <param name="pay_id"></param>
        /// <param name="q_begin_time"></param>
        /// <param name="q_end_time"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="isToday"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult Index(string act = "", int aId = 0, int storeId = 0, string order_sn = "", int order_status = -1, int order_type = 0, int pay_status = -1, int pay_id = 0, DateTime? q_begin_time = null, DateTime? q_end_time = null, int pageIndex = 1, int pageSize = 10, int isToday = 0, int[] ids = null)
        {
            int is_ziqu = 0;
            if (order_type == 3) //订单类型为自取
            {
                order_type = 1;
                is_ziqu = 1;
            }

            if (isToday == 1)//是否今日订单
            {
                q_begin_time = DateTime.Now.Date;
                q_end_time = DateTime.Now.Date;
            }

            if (string.IsNullOrWhiteSpace(act))
            {
                ViewModel<DishOrder> vm = new ViewModel<DishOrder>();
                vm.DataList = DishOrderBLL.SingleModel.GetOrders(aId, storeId, order_sn, order_status, order_type, pay_status, pay_id, q_begin_time, q_end_time, pageIndex, pageSize, is_ziqu: is_ziqu);
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.TotalCount = DishOrderBLL.SingleModel.GetOrdersCount(aId, storeId, order_sn, order_status, order_type, pay_status, pay_id, q_begin_time, q_end_time, is_ziqu: is_ziqu);

                ViewBag.Tables = DishTableBLL.SingleModel.GetTableByParams(aId, storeId, true) ?? new List<DishTable>();

                ViewBag.aId = aId;
                ViewBag.storeId = storeId;
                ViewBag.order_sn = order_sn;
                ViewBag.order_status = order_status;
                ViewBag.order_type = order_type;
                ViewBag.pay_status = pay_status;
                ViewBag.pay_id = pay_id;
                ViewBag.q_begin_time = q_begin_time;
                ViewBag.q_end_time = q_end_time;
                ViewBag.isToday = isToday;
                ViewBag.printer = DishPrintBLL.SingleModel.GetListBySql($"select * from dishprint where state >-1 and aid={aId} and storeid={storeId}");
                return View(vm);
            }
            else
            {
                bool isSuccess = false;
                if (act == "export")
                {
                    DishOrderBLL.SingleModel.ExportOrders(aId, storeId, order_sn, order_status, order_type, pay_status, pay_id, q_begin_time, q_end_time, is_ziqu: is_ziqu);
                    return null;
                }
                else if (act == "batchConfirm" || act == "batchCancel" || act == "batchComplete")
                {
                    DishEnums.OrderState state = DishEnums.OrderState.已确认;
                    switch (act)
                    {
                        case "batchConfirm":
                            state = DishEnums.OrderState.已确认;
                            break;
                        case "batchCancel":
                            state = DishEnums.OrderState.已取消;
                            break;
                        case "batchComplete":
                            state = DishEnums.OrderState.已完成;
                            break;
                        default:
                            _result.code = 0;
                            _result.msg = "操作失败,违规的状态值";
                            return Json(_result);
                    }

                    if (ids?.Length > 0)
                    {
                        isSuccess = DishOrderBLL.SingleModel.BtachModifyOrderState(ids, state);
                    }
                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result);
                }
                else if (act == "batchDelete")
                {
                    if (ids?.Length > 0)
                    {
                        isSuccess = DishOrderBLL.SingleModel.BtachDelete(ids);
                    }
                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result);
                }
                else if (act == "batchPay")
                {
                    if (ids?.Length > 0)
                    {
                        isSuccess = DishOrderBLL.SingleModel.BtachModifyPayState(ids, DishEnums.PayState.已付款);
                    }
                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result);
                }
                else if (act == "getNewOrder")
                {
                    isSuccess = DishOrderBLL.SingleModel.havingNewOrder(storeId, DateTime.Now.AddSeconds(-10)); //检查是否有新订单(10秒前到现在)

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "存在新订单" : "没有新订单";
                    return Json(_result);
                }
            }
            return Json(_result);
        }


        /// <summary>
        /// 评论管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Comments(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20)
        {
            
            //显示
            if (string.IsNullOrEmpty(act))
            {
                string filterSql = $"state=1 and aid={aId} and storeid={storeId}";
                ViewModel<DishComment> vm = new ViewModel<DishComment>();
                vm.DataList = DishCommentBLL.SingleModel.GetListBySql($"select * from dishcomment where {filterSql} order by id desc limit {pageIndex * pageSize},{pageSize}");
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.TotalCount = DishCommentBLL.SingleModel.GetCount(filterSql);
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
                        _result.msg = "参数错误";
                    else
                    {
                        DishComment updateModel = DishCommentBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishCommentBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,评论不存在或已删除";
                    }

                }
            }
            return Json(_result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 预定管理
        /// </summary>
        /// <returns></returns>
        public ActionResult YuDing(string act = "", int id = 0, int aId = 0, int storeId = 0, int pageIndex = 0, int pageSize = 20, string sortData = "")
        {
            //显示
            if (string.IsNullOrEmpty(act))
            {
                ViewModel<DishYuDing> vm = new ViewModel<DishYuDing>();
                string sqlFilter = $"dish_id={storeId}";
                vm.DataList = DishYuDingBLL.SingleModel.GetList(sqlFilter, pageSize, pageIndex, "*", "id desc");
                vm.PageIndex = pageIndex;
                vm.PageSize = pageSize;
                vm.TotalCount = DishYuDingBLL.SingleModel.GetCount(sqlFilter);
                return View(vm);
            }
            else
            {
                //删除
                if (act == "del")
                {
                    if (id <= 0)
                        _result.msg = "参数错误";
                    else
                    {
                        DishYuDing updateModel = DishYuDingBLL.SingleModel.GetModel(id);
                        if (updateModel != null)
                        {
                            updateModel.state = -1;
                            bool updateResult = DishYuDingBLL.SingleModel.Update(updateModel);
                            if (updateResult)
                            {
                                _result.code = 1;
                                _result.msg = "删除成功";
                            }
                            else
                                _result.msg = "删除失败";
                        }
                        else
                            _result.msg = "删除失败,预定不存在";
                    }

                }
            }
            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 订单资料
        /// </summary>
        /// <param name="order"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public ActionResult Info(DishOrder order, string act = "", int psy_id = 0, string printer = null, string orderMark = null)
        {
            if (order.id <= 0)
            {
                _result.code = 0;
                _result.msg = "未找到有效的订单标识";
                return Json(_result, JsonRequestBehavior.AllowGet);
            }


            if (string.IsNullOrWhiteSpace(act))
            {
                EditModel<DishOrder> model = new EditModel<DishOrder>();

                model.DataModel = DishOrderBLL.SingleModel.GetModel(order.id) ?? new DishOrder();
                if (model.DataModel.id > 0)
                {
                    model.DataModel.carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(model.DataModel.id) ?? new List<DishShoppingCart>();
                }
                model.aId = model.appId = order.aId;
                model.storeId = order.storeId;

                ViewBag.dishTransporters = DishTransporterBLL.SingleModel.GetTransportersByparams(model.aId, model.storeId, true) ?? new List<DishTransporter>();
                return View(model);
            }
            else
            {
                bool isSuccess = false;
                DishOrder dbOrder = DishOrderBLL.SingleModel.GetModel(order.id);
                if (dbOrder == null)
                {
                    _result.code = 0;
                    _result.msg = "未找到相关订单";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }

                if (act == "changeTable")
                {
                    DishTable table = DishTableBLL.SingleModel.GetModel(order.order_table_id_zhen);
                    if (table == null)
                    {
                        _result.code = 0;
                        _result.msg = "未找到相关桌台";
                        return Json(_result, JsonRequestBehavior.AllowGet);
                    }

                    dbOrder.order_table_id_zhen = table.id;
                    dbOrder.order_table_id = table.table_name;

                    isSuccess = DishOrderBLL.SingleModel.Update(dbOrder, "order_table_id,order_table_id_zhen");

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "queren")
                {
                    dbOrder.peisong_status = (int)DishEnums.DeliveryState.待取货;
                    isSuccess = DishOrderBLL.SingleModel.Update(dbOrder, "peisong_status");

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "quxiao")
                {
                    dbOrder.peisong_status = (int)DishEnums.DeliveryState.已取消;
                    isSuccess = DishOrderBLL.SingleModel.Update(dbOrder, "peisong_status");

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "wancheng")
                {
                    dbOrder.peisong_status = (int)DishEnums.DeliveryState.已完成;
                    isSuccess = DishOrderBLL.SingleModel.Update(dbOrder, "peisong_status");

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "peisong")
                {
                    if (psy_id <= 0)
                    {
                        _result.code = 0;
                        _result.msg = "配送员标识错误";
                        return Json(_result, JsonRequestBehavior.AllowGet);
                    }
                    DishTransporter transporter = DishTransporterBLL.SingleModel.GetModel(psy_id);
                    if (transporter == null)
                    {
                        _result.code = 0;
                        _result.msg = "未找到配送员资料";
                        return Json(_result, JsonRequestBehavior.AllowGet);
                    }
                    dbOrder.peisong_open = 1;
                    dbOrder.peisong_status = (int)DishEnums.DeliveryState.配送中;
                    dbOrder.peisong_user_name = transporter.dm_name;
                    dbOrder.peisong_user_phone = transporter.dm_mobile;

                    isSuccess = DishOrderBLL.SingleModel.Update(dbOrder, "peisong_open,peisong_status,peisong_user_name,peisong_user_phone");

                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "print") //打印
                {
                    PrinterHelper.DishPrintOrderByPrintType(dbOrder, 0, printer);
                    _result.code = 1;
                    _result.msg = "操作成功";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "mark")
                {
                    DishOrderAttrbute orderAttr = dbOrder.GetAttrbute();
                    orderAttr.mark = orderMark;
                    dbOrder.attrbute = JsonConvert.SerializeObject(orderAttr);
                    isSuccess = DishOrderBLL.SingleModel.Update(dbOrder, "attrbute");
                    _result.code = isSuccess ? 1 : 0;
                    _result.msg = isSuccess ? "操作成功" : "操作失败";
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 订单菜品资料 -退款页
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderGoods(string act = "", int aId = 0, int storeId = 0, int orderId = 0, int[] cartIds = null,string refundReason = "")
        {
            if (string.IsNullOrWhiteSpace(act))
            {
                ViewModel<DishShoppingCart> model = new ViewModel<DishShoppingCart>();
                model.DataList = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(orderId, true) ?? new List<DishShoppingCart>();

                model.aId = aId;
                model.storeId = storeId;
                return View(model);
            }
            else
            {
                if (act == "refundCarts")
                {
                    DishOrderBLL.SingleModel.RefundOrderByCartIds(cartIds, _result);
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
                else if (act == "refundOrder")
                {
                    DishOrderBLL.SingleModel.RefundOrderById(orderId, _result, refundReason);
                    return Json(_result, JsonRequestBehavior.AllowGet);
                }
            }

            _result.code = 0;
            _result.msg = "访问路径错误,无此页面";
            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 买单记录
        /// </summary>
        /// <param name="act"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult PayRecord(string act = "", int aId = 0, int storeId = 0, int pageIndex = 0,
            int pageSize = 20,
            DateTime? q_begin_time = null,
            DateTime? q_end_time = null,
            int payType = 0)
        {
            pageIndex = pageIndex - 1;
            if (pageIndex < 0)
                pageIndex = 0;
            ViewModel<PayRecordModel> vm = new ViewModel<PayRecordModel>();
            int total;
            vm.DataList = PayRecordBLL.SingleModel.GetList(aId, storeId, pageIndex, pageSize, out total, payType, q_begin_time, q_end_time);
            vm.PageIndex = pageIndex;
            vm.PageSize = pageSize;
            vm.TotalCount = total;

            ViewBag.q_begin_time = q_begin_time;
            ViewBag.q_end_time = q_end_time;
            if (act == "export")
            {
                DataTable dt = PayRecordBLL.SingleModel.GetTable(aId, storeId, payType, q_begin_time, q_end_time);
                DataTable dtOrders = new DataTable();
                dtOrders.Columns.Add("支付时间");
                dtOrders.Columns.Add("支付详情");
                dtOrders.Columns.Add("支付金额");
                dtOrders.Columns.Add("支付方式");
                dtOrders.Columns.Add("用户昵称");


                DataRow drOrders;
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        drOrders = dtOrders.NewRow();

                        drOrders["支付时间"] = item["addtime"];
                        drOrders["支付详情"] = item["info"];
                        drOrders["支付金额"] = item["money"];
                        drOrders["支付方式"] = item["paytype"];
                        drOrders["用户昵称"] = item["NickName"];

                        dtOrders.Rows.Add(drOrders);
                    }
                }

                //加默认列让这个表一定有数据
                dtOrders.Rows.Add(dtOrders.NewRow());
                ExcelHelper<DishOrder>.Out2Excel(dtOrders, $"{DateTime.Now.ToString("yyyy-MM-dd")}_买单记录"); //导出
                return null;
            }
            return View(vm);
        }

        /// <summary>
        /// 减少或删除未支付订单里的菜品
        /// </summary>
        /// <param name="act"></param>
        /// <param name="aId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeDish(string act = "", int aId = 0, int storeId = 0, int orderId = 0, int cartId = 0, int count = 0)
        {
            #region 验证
            if (orderId <= 0)
            {
                _result.msg = "非法请求";
                return Json(_result);
            }
            DishOrder order = DishOrderBLL.SingleModel.GetModel(orderId);
            DishStore store = DishStoreBLL.SingleModel.GetModel(storeId);
            if (store == null)
            {
                _result.msg = "门店不存在";
                return Json(_result);
            }
            if (order == null)
            {
                _result.msg = "订单不存在";
                return Json(_result);
            }
            if (order.pay_status != (int)DishEnums.PayState.未付款)
            {
                _result.msg = "只有未支付的订单才能修改";
                return Json(_result);
            }
            #endregion
            DishShoppingCart cartModel = DishShoppingCartBLL.SingleModel.GetModel(cartId);
            if (cartModel == null)
            {
                _result.msg = "对象不存在或已删除";
                return Json(_result);
            }
            if (order.huodong_manjin_jiner > 0&&order.huodong_manjian_id==0)
            {
                _result.msg = "2018-8-13前享受满减的订单，不能修改";
                return Json(_result);
            }
            

            List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetShoppingCart(cartModel.aId, cartModel.storeId, cartModel.user_id, order.id) ?? new List<DishShoppingCart>();//购物车
            List<DishGood> goods = DishGoodBLL.SingleModel.GetGoodsByIds(carts.Select(c => c.goods_id)?.ToArray()); //购物车内商品详细资料
            if (goods == null || carts.Any(c => !goods.Any(g => c.goods_id == g.id)))
            {
                _result.msg = "菜品不存在";
                return Json(_result);
            }
            cartModel.goods_number += count;
            if (cartModel.goods_number < 0)
                cartModel.goods_number = 0;

            if (cartModel.goods_number <= 0 && carts.Count ==1)
            {
                _result.msg = "订单至少要有一个菜品。";
                return Json(_result);
            }

            bool buildResult = DishOrderBLL.SingleModel.ReBuildOrder(order, store, cartModel, carts, goods, _result);
            _result.code = buildResult ? 1 : 0;
            _result.msg = buildResult ? "设置成功" : "设置失败";
            return Json(_result);
        }
    }
}