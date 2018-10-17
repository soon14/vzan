using BLL.MiniApp.Dish;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Areas.DishAdmin.Filters;

namespace User.MiniApp.Areas.DishAdmin.Controllers
{
    [LoginFilter]
    public class ChartsController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 销售排行
        /// </summary>
        /// <returns></returns>
        public ActionResult SalesRank(int aId = 0, int storeId = 0,DateTime? q_begin_time = null,DateTime? q_end_time = null)
        {
            DataTable dtSalesReport = DishOrderBLL.SingleModel.GoodSalesCountReport(storeId, q_begin_time, q_end_time);

            ViewBag.goodMsgs = dtSalesReport.Rows.Count > 0 ? $"'{string.Join("','",dtSalesReport.AsEnumerable().Select(d => d["goods_name"]))}'" : null; 
            ViewBag.salesCounts = dtSalesReport.Rows.Count > 0 ? $"'{string.Join("','", dtSalesReport.AsEnumerable().Select(d => d["sales_count"]))}'" : null;
            ViewBag.aId = aId;
            ViewBag.storeId = storeId;
            ViewBag.q_begin_time = q_begin_time?.ToString("yyyy-MM-dd");
            ViewBag.q_end_time = q_end_time?.ToString("yyyy-MM-dd");

            return View(dtSalesReport);
        }

        /// <summary>
        /// 餐桌统计
        /// </summary>
        /// <returns></returns>
        public ActionResult DishTable(int aId = 0, int storeId = 0, DateTime? q_begin_time = null, DateTime? q_end_time = null)
        {
            DataTable dtSalesReport = DishOrderBLL.SingleModel.TableStatisticsReport(storeId, q_begin_time, q_end_time);
            
            ViewBag.aId = aId;
            ViewBag.storeId = storeId;
            ViewBag.q_begin_time = q_begin_time?.ToString("yyyy-MM-dd");
            ViewBag.q_end_time = q_end_time?.ToString("yyyy-MM-dd");
            return View(dtSalesReport);
        }

        /// <summary>
        /// 收入报表
        /// </summary>
        /// <returns></returns>
        public ActionResult IncomeReport(int aId = 0, int storeId = 0,int s_year = 2018,int s_month = 1)
        {
            DateTime startTime = Convert.ToDateTime($"{s_year}-{s_month}-01 00:00:00");
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);
            DataTable dtIncomeReport = DishOrderBLL.SingleModel.IncomeReport(storeId, startTime, endTime);
            ViewBag.aId = aId;
            ViewBag.storeId = storeId;
            ViewBag.s_year = s_year;
            ViewBag.s_month = s_month;
           
            return View(dtIncomeReport);
        }

    }
}