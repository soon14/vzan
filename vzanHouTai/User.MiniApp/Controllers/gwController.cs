using BLL.MiniApp;
using BLL.MiniApp.Home;
using Entity.MiniApp;
using Entity.MiniApp.Home;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace User.MiniApp.Controllers
{
    public class MiniAppGwController:gwController
    {

    }
    public class gwController : baseController
    {
        /// <summary>
        /// 实例化对象
        /// </summary>
        public gwController()
        {
            
        }
        
        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Index(int appId=0, int pageIndex = 1, int pageSize = 100)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "不能随便跨账号越权!", code = "403" });
            }
            
            string strWhere = string.Empty;
            string AppTitle = Utility.IO.Context.GetRequest("keys", string.Empty);
            if (!string.IsNullOrEmpty(AppTitle))
                strWhere = $"Title like '%{AppTitle}%'";

            ViewBag.Keys = AppTitle;
            ViewBag.Id = appId;
            ViewBag.pageSize = pageSize;
            List<Gw> list = GwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex);
            int TotalCount = GwBLL.SingleModel.GetCount(strWhere);
            ViewBag.TotalCount = TotalCount;
            return View(list);
        }
        
        public ActionResult RangeList(int appId=0, int pageIndex = 1, int pageSize = 100)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }

            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限表里没有该数据!", code = "500" });
            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "不能随便跨账号越权!", code = "403" });
            }
            
            string strWhere = string.Empty;
            string AppTitle = Utility.IO.Context.GetRequest("keys", string.Empty);
            if (!string.IsNullOrEmpty(AppTitle))
                strWhere = $"Title like '%{AppTitle}%'";

            ViewBag.Keys = AppTitle;
            ViewBag.Id = appId;
            ViewBag.pageSize = pageSize;
            List<RangeGw> list = RangeGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", " RangeId");
            int TotalCount = RangeGwBLL.SingleModel.GetCount(strWhere);
            ViewBag.TotalCount = TotalCount;
            return View(list);
        }
        
        /// <summary>
        /// 新闻资讯列表
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult NewsList(int appId=0, int Type = 0, int pageIndex = 1, int pageSize = 20)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "不能随便跨账号越权!", code = "403" });
            }
            
            string strWhere = string.Empty;
            string NewsTitle = Utility.IO.Context.GetRequest("keys", string.Empty);
            strWhere = $"State>0 and Type={Type}";
            if (!string.IsNullOrEmpty(NewsTitle))
                strWhere += $" and Title like '%{NewsTitle}%'";

            ViewBag.Keys = NewsTitle;
            ViewBag.Id = appId;
            ViewBag.Type = Type;
            ViewBag.pageSize = pageSize;
            List<NewsGw> list = NewsGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex);
            int TotalCount = NewsGwBLL.SingleModel.GetCount(strWhere);
            ViewBag.TotalCount = TotalCount;
            return View(list);
        }
        
        /// <summary>
        /// 新闻资讯修改或者新增
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddOrEditNews(int appId=0, int Id = 0, int Type = 0)
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return View("PageError", new Return_Msg() { Msg = "权限表里没有该数据!", code = "403" });
            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "不能随便跨账号越权!", code = "403" });
            }
            
            NewsGw model = new NewsGw();

            //默认新增
            ViewBag.Id = appId;
            model.Type = Type;
            if (Id > 0)
            {
                //表示修改
                model = NewsGwBLL.SingleModel.GetModel(Id);
                if (model == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "要修改的数据不存在!", code = "500" });
                }
            }
            return View(model);
        }


        /// <summary>
        /// 新闻资讯修改或者新增
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddOrEditNews(int appId, NewsGw news)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" });
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "权限表里没有该数据" });
            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return Json(new { isok = false, msg = "不能随便跨账号越权" });
            }

            if (umodel.Id != 281)
            {
                return Json(new { isok = false, msg = "你不是管理员不能查看" });
            }

            if (news == null || news.Title.Length > 30 || news.Title.Length <= 0)
            {
                return Json(new { isok = false, msg = "标题不能为空并且长度为30字符内" });
            }
            if (news.Introduce.Length > 100)
            {
                return Json(new { isok = false, msg = "简介长度为100字符内" });
            }

            if (news.Id > 0)
            {
                //表示修改
                if (NewsGwBLL.SingleModel.Update(news))
                {
                    return Json(new { isok = true, msg = "修改成功!" });
                }
                else
                {
                    return Json(new { isok = false, msg = "修改异常!请联系客服" });
                }
            }
            else
            {
                if (Convert.ToInt32(NewsGwBLL.SingleModel.Add(news)) > 0)
                {
                    return Json(new { isok = true, msg = "新增成功!" });
                }
                else
                {
                    return Json(new { isok = false, msg = "新增异常!请联系客服" });
                }
            }
        }

        /// <summary>
        /// 删除新闻
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelMiniAppNews(int appId, int Id)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" });
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "权限表里没有该数据" });
            }

            if (umodel.AccountId != dzaccount.Id)
            {
                return Json(new { isok = false, msg = "不能随便跨账号越权" });
            }

            if (umodel.Id != 281)
            {
                return Json(new { isok = false, msg = "你不是管理员不能删除" });
            }

            NewsGw model = NewsGwBLL.SingleModel.GetModel(Id);
            if (model == null)
            {
                return Json(new { isok = false, msg = "数据不存在!" });
            }
            model.State = 0;
            if (NewsGwBLL.SingleModel.Update(model))
            {
                return Json(new { isok = true, msg = "删除成功!" });
            }
            else
            {
                return Json(new { isok = false, msg = "删除异常!" });
            }

        }


        public ActionResult ExceptionLog(int appId=0, int pageIndex = 1, int pageSize = 20)
        {
            if (dzaccount == null)
            {
                return Json(new { isok = false, msg = "请先登录" });
            }

            if (appId <= 0)
            {
                return Json(new { isok = false, msg = "系统繁忙appid_null" });
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
            if (umodel == null)
            {
                return Json(new { isok = false, msg = "权限表里没有该数据" });
            }
            
            string strWhere = string.Empty;
            string act = Utility.IO.Context.GetRequest("act", "");
            string start = Utility.IO.Context.GetRequest("start", string.Empty);
            string end = Utility.IO.Context.GetRequest("end", string.Empty);
            int TotalCount = 0;
            List<CommandExceptionLog> list = new List<CommandExceptionLog>();
            if (act == "search")
            {
                string AppId = Utility.IO.Context.GetRequest("keys", string.Empty);
                if (!string.IsNullOrEmpty(AppId))
                    strWhere = $"AppId like '%{AppId}%'";


                if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
                {
                    start = Convert.ToDateTime(start).ToString("yyyy-MM-dd HH:mm:ss");
                    end = Convert.ToDateTime(end).ToString("yyyy-MM-dd 23:59:59");
                    if (!string.IsNullOrEmpty(strWhere))
                    {
                        strWhere += $" and AddTime between @start and @end ";
                    }
                    else
                    {
                        strWhere = $" AddTime between @start and @end ";
                    }
                }

                list = CommandExceptionLogBLL.SingleModel.GetListByParam(strWhere, new MySqlParameter[] {
                        new MySqlParameter("@start",start),
                        new MySqlParameter("@end",end)
                    }, pageSize, pageIndex, "*", " AddTime desc ");
                TotalCount = CommandExceptionLogBLL.SingleModel.GetCount(strWhere, new MySqlParameter[] {
                        new MySqlParameter("@start",start),
                        new MySqlParameter("@end",end)
                    });
            }
            else
            {
                list = CommandExceptionLogBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", " id desc ");
                TotalCount = CommandExceptionLogBLL.SingleModel.GetCount(strWhere);
            }
            ViewBag.pageSize = pageSize;
            ViewBag.TotalCount = TotalCount;
            ViewBag.appId = appId;
            ViewBag.Id = appId;

            return View(list == null ? new List<CommandExceptionLog>() : list);
        }
    }
}