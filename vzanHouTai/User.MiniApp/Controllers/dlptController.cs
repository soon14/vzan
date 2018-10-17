using BLL.MiniApp.Home; 
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Home; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace User.MiniApp.Controllers
{
    public class dlptController : Controller
    {
        
        
        protected Return_Msg msg = new Return_Msg();
        
        
        // GET: DLPT
        public ActionResult Index()
        {
            return View();
        }

        [CheckLoginMethod("abou_xiao")]
        public ActionResult abou_xiao()
        {

          ViewBag.listRangTotal = RangeGwBLL.SingleModel.GetList(string.Empty, 6, 1, "*", " cast(viewNumbers as SIGNED INTEGER) desc");
          ViewBag.listRangWeekAdd = RangeGwBLL.SingleModel.GetList($"addtime>'{DateTime.Now.AddDays(-3)}' and addtime<'{DateTime.Now.AddDays(3)}'", 6, 1, "*", "cast(viewNumbers as SIGNED INTEGER) desc");
          ViewBag.listRangWeekTop = RangeGwBLL.SingleModel.GetList(string.Empty, 6, 1, "*", "(cast(viewNumbers as SIGNED INTEGER)- cast(LastViewNumbers as SIGNED INTEGER)) desc");
            return View();
        }
        [CheckLoginMethod("about_US")]
        public ActionResult about_US()
        {
            return View();
        }
        [CheckLoginMethod("agency")]
        public ActionResult agency()
        {
            return View();
        }
        [CheckLoginMethod("miapp")]
        public ActionResult miapp()
        {
            return View();
        }
        [CheckLoginMethod("shop")]
        public ActionResult shop(int pageIndex = 1, int pageSize = 12)
        {
            string strWhere = string.Empty;
            ViewBag.pageSize = pageSize;
            List<Gw> list = GwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex);
            int TotalCount = GwBLL.SingleModel.GetCount(strWhere);
            ViewBag.TotalCount = TotalCount;
            return View(list==null?new List<Gw>():list);

          
        }
        [CheckLoginMethod("miappnew")]
        public ActionResult miappnew(int Type = 0, int pageIndex = 1, int pageSize = 5)
        {
            string strWhere = $"State>0 and Type={Type}";
            
            ViewBag.Type = Type;
            ViewBag.pageSize = pageSize;
            string sortWhere = "addtime desc";
            if (Type > 0)
                sortWhere = "sort desc,addtime desc";
            List<NewsGw> list = NewsGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex,"*", sortWhere);
            int TotalCount = NewsGwBLL.SingleModel.GetCount(strWhere);
            ViewBag.TotalCount = TotalCount;
            return View(list==null?new List<NewsGw>():list);
        }

        public ActionResult more_detail(int id)
        {
            NewsGw model = NewsGwBLL.SingleModel.GetModel(id);
            if (model == null)
                return Content("数据不存在!");
            return View(model);
        }

        public ActionResult more_total(int pageIndex = 1, int pageSize = 10,int type=0)
        {
         
            string strWhere = string.Empty;
            string order = "cast(viewNumbers as SIGNED INTEGER) desc";
            ViewBag.pageSize = pageSize;
            ViewBag.Type = type;
            if (type == 1)
            {
                //本周上升
                order = "(cast(viewNumbers as SIGNED INTEGER)- cast(LastViewNumbers as SIGNED INTEGER)) desc";
            }

            if (type == 2)
            {
                //本周新增
                strWhere = $"addtime>'{DateTime.Now.AddDays(-3)}' and addtime<'{DateTime.Now.AddDays(3)}'";
            }
            ViewBag.pageIndex = pageIndex;
            ViewBag.StartRang = (pageIndex-1)*pageSize;
            List<RangeGw> list = RangeGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", order);
            int TotalCount = RangeGwBLL.SingleModel.GetCount(strWhere);
            if (TotalCount == 0 && type == 2)
            {
                strWhere = string.Empty;
                order = "cast(viewNumbers as SIGNED INTEGER) desc";
                list = RangeGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", order);
                TotalCount = RangeGwBLL.SingleModel.GetCount(strWhere);
            }
            ViewBag.TotalCount = TotalCount;
            return View(list==null?new List<RangeGw>():list);
           
        }

        [CheckLoginMethod("question")]
        public ActionResult question()
        {
            ViewBag.title = Utility.IO.Context.GetRequest("title", string.Empty);
            ViewBag.childnode = Utility.IO.Context.GetRequestInt("childnode", 0);
            QuestionViewModel viewmodel = new QuestionViewModel();
            viewmodel.menuList = HomebkmenuBLL.SingleModel.GetList($" type={(int)MenuType.miniapp} and parentId=0");
            viewmodel.hotList = HomenewsBLL.SingleModel.GetList($" type={(int)newsType.miniAPP} and ishot=1  and state=1", 5, 1, "*", "sort desc,addtime desc");
            viewmodel.commonList = HomenewsBLL.SingleModel.GetList($" type={(int)newsType.miniAPP} and iscommon=1  and state=1", 4, 1, "*", "sort desc,addtime desc");
            viewmodel.advList = HomecaseBLL.SingleModel.GetList($"type={(int)caseType.xcxadv} and state=1",1,1,"*", "sort desc,addtime desc");
            return View(viewmodel);
        }

        public ActionResult getQuestionList()
        {
            int pageindex = Utility.IO.Context.GetRequestInt("pageindex", 1);
            int pagesize = Utility.IO.Context.GetRequestInt("pagesize", 10);
            int childnode = Utility.IO.Context.GetRequestInt("childnode", 0);
            string title = Utility.IO.Context.GetRequest("title", string.Empty);
            string sqlwhere = $"type ={ (int)newsType.miniAPP} and state = 1";
            if (childnode > 0)
            {
                sqlwhere += $" and childnode={childnode}";
            }
            if (!string.IsNullOrEmpty(title))
            {
                sqlwhere += $" and title like '%{title}%'";
            }
            List<Homenews> list = HomenewsBLL.SingleModel.GetList(sqlwhere, pagesize, pageindex,"*","sort desc,addtime desc");
            List<object> objlist = null;
            if (list != null && list.Count > 0)
            {
                objlist = new List<object>();
                foreach (Homenews news in list)
                {

                    string[] taglist;
                    if (!string.IsNullOrEmpty(news.tags))
                    {
                        taglist = news.tags.Split(',');
                    }
                    else
                    {
                        taglist = null;
                    }
                    object obj = new
                    {
                        taglist = taglist,
                        id = news.Id,
                        title = news.title,
                        Description = news.Description
                    };
                    objlist.Add(obj);
                }
            }
            msg.isok = true;
            msg.dataObj = objlist;
            return Json(msg);
        }

        [CheckLoginMethod("QuestionContent")]
        public ActionResult QuestionContent()
        {
            QuestionViewModel viewmodel = new QuestionViewModel();
            int id = Utility.IO.Context.GetRequestInt("id", 0);
            if (id <= 0)
            {
                return View(viewmodel);
            }
            viewmodel.news = HomenewsBLL.SingleModel.GetModel($" id={id} and  type={(int)newsType.miniAPP}");
            if (viewmodel.news != null)
            {
                ViewBag.childnode = viewmodel.news.childNode;
                viewmodel.news.count++;
                HomenewsBLL.SingleModel.Update(viewmodel.news);
            }
            viewmodel.menuList = HomebkmenuBLL.SingleModel.GetList($" type={(int)MenuType.miniapp} and parentId=0");
            viewmodel.hotList = HomenewsBLL.SingleModel.GetList($" type={(int)newsType.miniAPP} and ishot=1  and state=1", 5, 1, "*", "sort desc,id desc");
            viewmodel.commonList = HomenewsBLL.SingleModel.GetList($" type={(int)newsType.miniAPP} and iscommon=1  and state=1", 4, 1, "*", "sort desc,id desc");
            if (viewmodel.news != null && !string.IsNullOrEmpty(viewmodel.news.tags))
            {
                List<string> taglist = viewmodel.news.tags.Split(',').ToList();
                string sqlwhere = $" type={(int)newsType.miniAPP} and state=1 and id !={id}";
                string likestr = string.Empty;
                foreach (string tag in taglist)
                {
                    likestr += $"tags like '%{tag}%' or ";
                }
                likestr = "(" + likestr.Substring(0, likestr.Length - 4) +")";
                sqlwhere += $" and {likestr}";
                viewmodel.relevantList = HomenewsBLL.SingleModel.GetList(sqlwhere, 5, 1, "*", "sort desc,id desc");
               // ViewBag.str = sqlwhere+"||"+JsonConvert.SerializeObject(viewmodel.relevantList);
            }
            viewmodel.advList = HomecaseBLL.SingleModel.GetList($"type={(int)caseType.xcxadv} and state=1", 1, 1, "*", "sort desc,id desc");
            return View(viewmodel);
        }
        /// <summary>
        /// 提交用户咨询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SendUserAdvisory()
        {
            string name = Utility.IO.Context.GetRequest("username", string.Empty);
            string phone = Utility.IO.Context.GetRequest("phone", string.Empty);
            int datasource = Utility.IO.Context.GetRequestInt("source", 0);
            int type = Utility.IO.Context.GetRequestInt("type", 5);
            if (string.IsNullOrEmpty(name))
            {
                msg.Msg = "请输入您的称呼";
                return Json(msg,JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(phone))
            {
                msg.Msg = "请输入您的手机号码";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            if (!Regex.IsMatch(phone, @"^[1]+[3-9]+\d{9}$"))
            {
                msg.Msg = "手机格式不正确";
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            Hfeedback model = new Hfeedback()
            {
                name = name,
                phone = phone,
                datasource = datasource,
                type = type,
                addtime=DateTime.Now
            };
            msg.isok = Convert.ToInt32(HfeedbackBLL.SingleModel.Add(model)) > 0;
            msg.Msg = msg.isok ? "发送成功" : "发送失败";
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
    }
}