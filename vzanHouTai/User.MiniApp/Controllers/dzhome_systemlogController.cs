using BLL.MiniApp;
using BLL.MiniApp.Home;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp.User;
using Entity.MiniApp;
using Entity.MiniApp.Home;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLL.MiniApp.Conf;
using System.Configuration;
using Utility;
using System.Data;
using Utility.IO;
using MySql.Data.MySqlClient;
using Entity.MiniApp.Conf;
using System.Text;
using User.MiniApp.Model;

namespace User.MiniApp.Controllers
{
    public partial class dzhomeController : baseController
    {
        public dzhomeController()
        {

        }

        public ActionResult GetNoReadSystempMessageList()
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            Member member = MemberBLL.SingleModel.GetMemberByAccountId(dzaccount.Id.ToString());
            if(member==null)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
            }

            List<SystemUpdateMessage> list = new List<SystemUpdateMessage>();
            try
            {
                list = SystemUpdateMessageBLL.SingleModel.GetSystemUpdateMessageList(dzaccount.Id.ToString(), member.CreationDate.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message, data = list }, JsonRequestBehavior.AllowGet);
            }
            
           
            return Json(new { isok=true,msg="",data= list },JsonRequestBehavior.AllowGet);
        }

        public ActionResult SystemMessageList()
        {
            int pageIndex = Context.GetRequestInt("pageIndex",1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int sysid = Context.GetRequestInt("sysid", 0);
            ViewBag.sysid = sysid;
            ViewBag.sysindex = -1;

            int count = 0;
            ViewModel<SystemUpdateMessage> viewmodel = new ViewModel<SystemUpdateMessage>();
            viewmodel.DataList = new List<SystemUpdateMessage>();
            if (dzaccount == null)
            {
                ViewBag.ishidden = true;
                List<SystemUpdateMessage> list = SystemUpdateMessageBLL.SingleModel.GetListByPage(pageSize, pageIndex, ref count);
                viewmodel.TotalCount = count;
                viewmodel.PageIndex = pageIndex;
                viewmodel.PageSize = pageSize;
                viewmodel.DataList = list;
                return View(viewmodel);
                //return Redirect("/dzhome/login");
            }
            //Member member = _memberBll.GetMemberByAccountId(dzaccount.Id.ToString());
            //if (member == null)
            //{
            //    return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
            //}
            
            //判断是否是代理商
            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(dzaccount.Id.ToString());
            if (agentinfo == null)
            {
                agentinfo = new Agentinfo();
            }


            List<XcxAppAccountRelation> relationlist = XcxAppAccountRelationBLL.SingleModel.GetListByaccountId(dzaccount.Id.ToString());
            string tids = "";
            if (relationlist != null && relationlist.Count > 0)
            {
                tids = string.Join(",", relationlist.Select(s => s.TId).Distinct());
            }

            viewmodel.DataList = SystemUpdateMessageBLL.SingleModel.GetAllSystemUpdateMessageList(tids, dzaccount.Id.ToString(), agentinfo.id,pageIndex,pageSize,ref count);
            
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;

            //设为已读
            if (sysid > 0)
            {
                SystemUpdateUserLogBLL.SingleModel.Readed(sysid, dzaccount.Id.ToString());
            }
            if(viewmodel.DataList!=null && viewmodel.DataList.Count>0 && sysid>0)
            {
                SystemUpdateMessage item = viewmodel.DataList.Where(w => w.Id == sysid).FirstOrDefault();
                if(item!=null)
                {
                    ViewBag.sysindex = viewmodel.DataList.IndexOf(item);
                }
                
            }

            return View(viewmodel);
        }

        public ActionResult AllRead()
        {
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            Member member = MemberBLL.SingleModel.GetMemberByAccountId(dzaccount.Id.ToString());
            if (member == null)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!", code = "500" });
            }
            try
            {
                if (!SystemUpdateUserLogBLL.SingleModel.AddSMUserLog(dzaccount.Id.ToString(), member.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")))
                {
                    return Json(new { isok = false, msg = "系统繁忙！" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 单条日志标为已读
        /// </summary>
        /// <returns></returns>
        public ActionResult Read()
        {
            int sysid = Context.GetRequestInt("sysid",0);
            if(sysid<=0)
            {
                return Json(new { isok = false, msg = "参数错误" }, JsonRequestBehavior.AllowGet);
            }

            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            try
            {
                if (!SystemUpdateUserLogBLL.SingleModel.Readed(sysid,dzaccount.Id.ToString()))
                {
                    return Json(new { isok = false, msg = "请刷新重试！" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = true, msg = "成功" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取更新日志
        /// </summary>
        public ActionResult GetUpdateLogList()
        {
            Return_Msg result = new Return_Msg();
            int pageSize = Context.GetRequestInt("pageSize",20);
            int pageIndex = Context.GetRequestInt("pageIndex",1);
            int count = 0;
            List<SystemUpdateMessage> list = SystemUpdateMessageBLL.SingleModel.GetListByPage(pageSize,pageIndex,ref count);

            result.isok = true;
            result.dataObj = new { Count =count, DataList=list } ;
            
            return Json(result);
        }
    }
}