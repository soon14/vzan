using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.MiniApp.Model;
using Entity.MiniApp;
using BLL.MiniApp;
using BLL.MiniApp.Pin;
using Entity.MiniApp.Pin;
namespace User.MiniApp.Areas.Pin.Controllers
{
    public class TongJiController : BaseController
    {
        // GET: Pin/TongJi
        public ActionResult Index(string act = "", int aId = 0, DateTime? q_begin_time = null, DateTime? q_end_time = null)
        {
            PinGoodsOrderBLL goodsOrderBLL = new PinGoodsOrderBLL();
            string sql = $"SELECT sum(money) as money,DATE_FORMAT(addtime,'%Y-%m-%d') as addtime from pingoodsorder where state>0 and ordertype=0 and aid={aId}  ";
            string whereSql = "";
            string groupSql = "group by DATE_FORMAT(addtime,'%Y-%m-%d') order by DATE_FORMAT(addtime,'%Y-%m-%d') desc";
            if (act == "7day")
            {
                sql = sql + groupSql + " limit 7 ";
            }
            else if (act == "30day")
            {
                sql = sql + groupSql + " limit 30 ";
            }
            else
            {
                if (q_end_time > q_begin_time)
                {
                    whereSql = $" and addtime BETWEEN '{q_begin_time?.ToString("yyyy-MM-dd")} 00:00:00' and '{q_end_time?.ToString("yyyy-MM-dd")} 23:59:59' ";
                }
                sql= sql + whereSql+ groupSql + " limit 30 ";
            }
            List<PinGoodsOrder> list = goodsOrderBLL.GetListBySql(sql);
            if (list != null && list.Count > 0)
            {
                ViewBag.NameList = string.Join(",", list.Select(p => $"'{p.addtime.ToString("yyyy-MM-dd")}'"));
                ViewBag.ValueList = string.Join(",", list.Select(p => p.money/100));
            }
            return View();
        }
    }
}