using BLL.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using User.MiniApp.Model;

namespace User.MiniApp.Controllers
{
    public class mgrController : Controller
    {
        public mgrController()
        {
        }
        public ActionResult log(int PageIndex = 0, int PageSize = 15, string Host = "", string Level = "ERROR", string kw = "")
        {

            ViewModel<log4netsyslog> vm = new ViewModel<log4netsyslog>();
            string filter = string.Empty;
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            filter += " 1=1 ";
            if (!string.IsNullOrEmpty(Host))
            {
                filter += " and Host=@Host ";
                parameters.Add(new MySqlParameter("@Host", Host));
            }
            if (!string.IsNullOrEmpty(Level))
            {
                filter += " and Level=@Level ";
                parameters.Add(new MySqlParameter("@Level", Level));
            }
            if (!string.IsNullOrEmpty(kw))
            {
                filter += $" and ( msgbody like @kw or msgexception like @kw ) ";
                parameters.Add(new MySqlParameter("@kw", Core.MiniApp.Utils.FuzzyQuery(kw)));
            }

            vm.DataList = log4netsyslogBLL.SingleModel.GetListByParam(filter, parameters?.ToArray(), PageSize, PageIndex, "*", " id desc");
            vm.PageSize = PageSize;
            vm.PageIndex = PageIndex;
            vm.TotalCount = log4netsyslogBLL.SingleModel.GetCount(filter, parameters?.ToArray());
            return View(vm);
        }
        public ActionResult logmgr(string act = "")
        {
            //只保留最近七天
            if (act == "delall")
            {
                log4netsyslogBLL.SingleModel.Delete($"CreateDate<'{DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            else if (act == "sel")
            {
                int id = Utility.IO.Context.GetRequestInt("id", 0);
                log4netsyslog model = new log4netsyslog();
                if (id > 0)
                {
                    model = log4netsyslogBLL.SingleModel.GetModel(id);
                }
                return Json(new { isok = true, msg = (model.Level == "INFO" ? model.MsgBody : model.MsgException) });
            }
            return Json(new { isok = true });
        }


        public ActionResult sql(string sql, string bfsql)
        {
            testSqlModel model = new testSqlModel();
            model.sql = sql;
            model.bfsql = bfsql;
            if (string.IsNullOrEmpty(sql))
            {
                return View(model);
            }
            try
            {
                DataSet ds = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql);
                if (ds.Tables.Count > 0)
                    model.table = ds.Tables[0];
                return View(model);
            }
            catch (Exception ex)
            {
                model.msg = ex.Message;
                return View(model);
            }
        }

        public ActionResult charts(string act = "", string tpl = "", DateTime? stime = null, DateTime? etime = null)
        {

            DateTime now = DateTime.Now;
            if (stime == null)
                stime = DateTime.Now.AddDays(-7);
            if (etime == null)
                etime = new DateTime(now.Year, now.Month, now.Day);
            ViewBag.stime = stime.Value.ToString("yyyy-MM-dd");
            ViewBag.etime = etime.Value.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(act))
            {
                return View();
            }
            else
            {




                ChartsDataModel model = new ChartsDataModel();
                if (tpl == "all")
                {
                    ChartsLineModel lineModel = new ChartsLineModel();
                    lineModel.TId = "all";
                    lineModel.Name = "全部";

                    string sql = $@"
SELECT 'all' as 模板, SUM(o.payment_free) / 100 as 总金额,DATE_FORMAT(Addtime, '%Y-%m-%d') as 日期 from citymorders o
   where OrderType >= 3001001 and OrderType<= 3001026  and `Status`= 1 and o.addtime between '{stime.Value.ToString("yyyy-MM-dd 00:00:00")}' and '{etime.Value.ToString("yyyy-MM-dd 23:59:59")}'
and MinisnsId not in(SELECT id from xcxappaccountrelation WHERE AccountId='20d5ea85-b0b3-4135-b250-617054be406b' and id<>6933593)
group by  日期 ";

                    DataTable dt = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql).Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            model.Labels.Add(item["日期"].ToString());
                            lineModel.Datas.Add(Convert.ToDouble(item["总金额"]));
                        }
                        model.LinList.Add(lineModel);
                    }

                }
                else
                {
                    TimeSpan ts = etime.Value - stime.Value;
                    int days = 1;
                    if (ts.Days <= 1)
                        days = 1;
                    else
                        days = ts.Days;
                    for (int i = days; i >= 1; i--)
                    {
                        model.Labels.Add(DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd"));
                    }

                    Dictionary<string, string> tplDic = new Dictionary<string, string>
                    {
                        { "pro","3001006,3001010,3001015,3001020"},//订单,储值买单，微信买单，内容付费
                        { "dish","3001016,3001017,3001019"},//订单，微信买单，会员充值
                        { "food","3001002"},//订单
                        { "store","3001001"},//订单
                        { "pin","3001022"},
                        { "platform","3001025"},
                        { "mutistore","3001009"}
                    };
                    Dictionary<string, string> tplNameDic = new Dictionary<string, string>
                    {
                        { "pro","专业版"},
                        {"dish","智慧餐厅" },
                        {"food","餐饮版" },
                        {"store","电商版" },
                        {"pin","拼享惠" },
                        {"platform","平台" },
                        {"mutistore","多门店" },
                    };
                    List<string> tplSearch = new List<string>();
                    string[] kv = tpl.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < kv.Length; i++)
                    {
                        if (tplDic.ContainsKey(kv[i]))
                        {
                            ChartsLineModel lineModel = new ChartsLineModel();
                            lineModel.TId = kv[i];
                            lineModel.Name = tplNameDic[kv[i]];
                            string sql = $@"
SELECT 'all' as 模板, SUM(o.payment_free) / 100 as 总金额,DATE_FORMAT(Addtime, '%Y-%m-%d') as 日期 from citymorders o
   where OrderType in({tplDic[kv[i]]})  and `Status`= 1 and o.addtime between '{stime.Value.ToString("yyyy-MM-dd 00:00:00")}' and '{etime.Value.ToString("yyyy-MM-dd 23:59:59")}'
and MinisnsId not in(SELECT id from xcxappaccountrelation WHERE AccountId='20d5ea85-b0b3-4135-b250-617054be406b' and id<>6933593)
group by  日期 ";

                            DataTable dt = SqlMySql.ExecuteDataSet(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, sql).Tables[0];
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow item in dt.Rows)
                                {
                                    lineModel.Datas.Add(Convert.ToDouble(item["总金额"]));
                                }

                            }
                            model.LinList.Add(lineModel);
                        }
                    }







                }

                return Json(model);
            }

        }
    }

    public class ChartsDataModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartsLineModel> LinList { get; set; } = new List<ChartsLineModel>();
    }
    public class ChartsLineModel
    {
        public string TId { get; set; } = "";
        public string Name { get; set; } = "";
        public List<double> Datas { get; set; } = new List<double>();
    }
}