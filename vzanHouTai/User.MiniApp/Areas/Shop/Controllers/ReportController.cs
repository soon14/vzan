using BLL.MiniApp;
using BLL.MiniApp.Dish;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.Shop.Filters;
using User.MiniApp.Areas.Shop.Models;
using Utility;


namespace User.MiniApp.Areas.Shop.Controllers
{
    [RouteArea("Shop"), RoutePrefix("Report"), Route("{action}/{storeId?}")]
    [LoginFilter(storePara: "store")]
    public class ReportController : BaseController
    {
        [HttpGet]
        public JsonResult Today(DishStore store)
        {
            DishOrderBLL dishOrderBLL = DishOrderBLL.SingleModel;
            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime endDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            //今日余额支付收入
            double todayAccountIncome = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.余额支付);
            //今日微信支付收入
            double todayWxIncome = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.微信支付);
            //今日微信买单收入
            double todayWxCheckout = dishOrderBLL.GetWxMdIncomeByDate(store.id, startDate, endDate);
            //今日现金支付收入
            double todayCashIncom = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.线下支付);
            //今日余额买单收入
            double todayAccountCheckout = DishCardAccountLogBLL.SingleModel.GetAccountSumByDate(store.id, startDate, endDate) - todayAccountIncome;
            todayAccountCheckout = Math.Round(todayAccountCheckout, 2);
            //今日货到付款收入
            double todayCashDelivery = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.货到支付);
            //今日手动确认收入
            double todayAccounting = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate, 0);
            //今日总收入
            double todayIncome = dishOrderBLL.GetEarningsByDate(store.id, startDate, endDate) + todayWxCheckout + todayAccountCheckout;

            //今日微信支付订单数
            int WxOrder = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.微信支付);
            //今日现金支付订单数
            int todayCashOrder = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.线下支付);
            //今日货到付款订单数
            int todayCashDeliveryOrder = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.货到支付);
            //今日手动确认订单数
            int todayAccountingOrder = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, 0);
            //今日余额支付订单数
            int todayAccountOrder = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate, (int)DishEnums.PayMode.余额支付);
            //今日微信买单订单数
            int todayWXCheckoutOrder = dishOrderBLL.GetWxMdCountByDate(store.id, startDate, endDate);
            //今日余额买单订单数
            int todayAccountCheckoutOrder = DishCardAccountLogBLL.SingleModel.GetCountByDate(store.id, startDate, endDate) - todayAccountOrder;
            //今日订单=订单表总数+微信买单订单数+余额买单订单数
            int todayOrder = dishOrderBLL.GetSaleCountByDate(store.id, startDate, endDate) + todayWXCheckoutOrder + todayAccountCheckoutOrder;


            //微信支付总收入
            double wxIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.微信支付);
            //现金支付总收入
            double cashIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.线下支付);
            //货到付款总收入
            double cashDeliveryincome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.货到支付);
            //手动确认总收入
            double accountingIncom = dishOrderBLL.GetEarnings(store.id, 0);
            //余额支付总收入
            double accountIncome = dishOrderBLL.GetEarnings(store.id, (int)DishEnums.PayMode.余额支付);
            //微信买单收入
            double wxCheckout = dishOrderBLL.GetWxMdIncomeByDate(store.id, startDate, endDate, false);
            //余额买单收入
            double accountCheckout = DishCardAccountLogBLL.SingleModel.GetAccountSumByDate(store.id, startDate, endDate, false) - accountIncome;
            accountCheckout = Math.Round(accountCheckout, 2);
            //总收入=订单总收入+微信买单收入+余额买单收入
            double onlineIncome = dishOrderBLL.GetEarnings(store.id) + wxCheckout + accountCheckout;

            //总订单数=订单表总数+微信买单总数+余额消费记录总数-余额支付订单数
            int totalOrder = dishOrderBLL.GetSaleCount(store.id) + dishOrderBLL.GetWxMdCountByDate(store.id, startDate, endDate, false) + DishCardAccountLogBLL.SingleModel.GetCountByDate(store.id, startDate, endDate) - dishOrderBLL.GetSaleCount(store.id, (int)DishEnums.PayMode.余额支付);
            //商品数
            int productCount = DishGoodBLL.SingleModel.GetCountByStoreId(store.id);
            //餐桌数
            int tablesCount = DishTableBLL.SingleModel.GetCountByStoreId(store.id);
            //打印机数
            int printerCount = DishPrintBLL.SingleModel.GetCountByStoreId(store.id);

            object formatDTO = new
            {
                TodayIncome = new
                {
                    Wx = $"{todayWxIncome }元",
                    WxCheckout = $"{todayWxCheckout }元",
                    Cash = $"{todayCashIncom }元",
                    CashDelivery = $"{todayCashDelivery }元",
                    Account = $"{todayAccountIncome }元",
                    AccountCheckout = $"{todayAccountCheckout }元",
                    Accounting = $"{todayAccounting }元",
                    Total = $"{todayIncome }元",
                },
                TodayOrder = new
                {
                    Wx = $"{WxOrder}单",
                    WXCheckout = $"{todayWXCheckoutOrder}单",
                    Cash = $"{todayCashOrder}单",
                    CashDelivery = $"{todayCashDeliveryOrder}单",
                    Account = $"{todayAccountOrder}单",
                    AccountCheckout = $"{todayAccountCheckoutOrder}单",
                    Accounting = $"{todayAccountingOrder}单",
                    Total = $"{todayOrder}单",
                },
                TotalIncome = new
                {
                    Wx = $"{ wxIncome }元",
                    Cash = $"{cashIncome }元",
                    CashDelivery = $"{cashDeliveryincome }元",
                    Account = $"{accountIncome }元",
                    AccountCheckout = $"{accountCheckout }元",
                    Accounting = $"{accountingIncom }元",
                    WxCheckout = $"{wxCheckout }元",
                },
                Total = new
                {
                    Order = totalOrder,
                    Prnter = printerCount,
                    Table = tablesCount,
                    Product = productCount,
                    Income = onlineIncome ,
                },
            };

            return ApiModel(isok: true, message: "获取成功", data: formatDTO);
        }

        [HttpGet]
        public JsonResult Income(DishStore store, int year = 0, int month = 0)
        {
            if(year == 0)
            {
                year = DateTime.Now.Year;
            }
            if (month == 0)
            {
                month = DateTime.Now.Month;
            }

            DateTime startTime = Convert.ToDateTime($"{year}-{month}-01 00:00:00");
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);
            DataTable incomeTable = DishOrderBLL.SingleModel.IncomeReport(store.id, startTime, endTime);

            object DTO = new
            {
                TotalIncome = incomeTable.Compute("sum(total_total_fee)+sum(wx_maidan_total_fee)+sum(ye_maidan_total_fee)-sum(ye_total_fee)", null),
                //WechatIncome = incomeTable.Compute("sum(wx_total_fee)", null),
                //CashIncome = incomeTable.Compute("sum(xj_total_fee)", null),
                //AccountIncome = incomeTable.Compute("sum(ye_total_fee)", null),
                TotalOrder = incomeTable.Compute("sum(order_count)", null),
                //DineInOrder = incomeTable.Compute("sum(dn_order_count)", null),
                //TakeoutOrder = incomeTable.Compute("sum(wm_order_coun)", null),
                //WechatPayOrder = incomeTable.Compute("sum(wx_order_count)", null),
                //CashOrder = incomeTable.Compute("sum(xj_order_count)", null),
                //AccountOrder = incomeTable.Compute("sum(ye_order_count)", null),
                //WechatCheckoutOrder = incomeTable.Compute("sum(ye_order_count)", null),
                Daily = incomeTable.Rows.Cast<DataRow>().Select(item =>
                new
                {
                    Date = item["order_date"],
                    Income = item["total_total_fee"],
                    Order = item["order_count"],
                }),
            };
            //<th width="80">@(Model.Compute("sum(wx_maidan_order_count)+sum(ye_maidan_order_count)-sum(ye_order_count)", ""))</th>
            //<th width="60">@(Model.Compute("sum(wx_maidan_order_count)", ""))/@(Model.Compute("sum(wx_maidan_total_fee)", ""))</th>
            //<th width="60">
            //    @{
            //        object sum2 = Model.Compute("sum(ye_maidan_total_fee)-sum(ye_total_fee)", "");
            //        string sum2Result = "0.00";
            //        if (!Convert.IsDBNull(sum2))
            //        {
            //            sum2Result= Convert.ToDouble(sum2).ToString("F");
            //        }
            //    }
            //    @(Model.Compute("sum(ye_maidan_order_count)-sum(ye_order_count)", ""))/@(sum2Result)
            //</th>
            return ApiModel(isok: true, message: "获取成功", data: DTO);
        }

        [HttpGet]
        public ActionResult Table(DishStore store, DateTime? begin = null, DateTime? end = null)
        {
            DataTable tableIncome = DishOrderBLL.SingleModel.TableStatisticsReport(store.id, begin, end);
            
            object DTO = tableIncome.Rows.Cast<DataRow>().Select(item =>
            new
            {
                Name = item["table_name"],
                OrderCount = item["order_count"],
                TotalPay = item["all_total_fee"],
                WechatPay = item["wx_total_fee"],
                OtherPay = item["else_total_fee"],
            });
            return ApiModel(isok: true, message: "获取成功", data: DTO);
        }

        [HttpGet]
        public ActionResult Sale(DishStore store, DateTime? begin, DateTime? end)
        {
            DataTable saleReport = DishOrderBLL.SingleModel.GoodSalesCountReport(store.id, begin, end);

            object DTO = new
            {
                columns = new string[] { "商品" , "销量"  },
                rows = saleReport.AsEnumerable().Select(item => new { 商品 = item["goods_name"], 销量 = item["sales_count"] })
            };

            return ApiModel(isok: true, message: "获取成功", data: DTO);
        }
    }
}