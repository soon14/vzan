using BLL.MiniApp;
using BLL.MiniApp.Conf;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Utility;
using Entity.MiniApp.User;
using BLL.MiniApp.Footbath;
using Entity.MiniApp.Footbath;
using System.Data;
using System.Text;
using BLL.MiniApp.User;
using Utility.IO;
using Entity.MiniApp.Fds;
using BLL.MiniApp.Fds;
using User.MiniApp.Model;
using System.IO;
using ThoughtWorks.QRCode;
using System.Drawing.Imaging;
using System.Drawing;
using BLL.MiniApp.FunList;
using Entity.MiniApp.FunctionList;

namespace User.MiniApp.Controllers
{
    public class customerdistributionController : baseController
    {
        protected Return_Msg msg;
        
        protected readonly AgentDistributionRelationBLL _agentDistributionRelationBLL;
        
        
        
        

        /// <summary>
        /// 实例化对象
        /// </summary>
        public customerdistributionController()
        {
            
            _agentDistributionRelationBLL = new AgentDistributionRelationBLL();
            
            
            
            
        }
        
        public ActionResult CustomerDistributionUser()
        {
            if (dzaccount == null)
            {
                return View("PageError", new Return_Msg() { Msg = "登陆过期，请重新登陆!", code = "500" });
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string loginid = Context.GetRequest("loginid", "");
            string agentname = Context.GetRequest("agentname", "");
            string starttime = Context.GetRequest("starttime", "");
            string endtime = Context.GetRequest("endtime", "");
            int openstate = Context.GetRequestInt("openstate", 999);
            int opentype = Context.GetRequestInt("opentype", 999);


            int count = 0;
            ViewModel<AgentDistributionRelation> viewmodel = new ViewModel<AgentDistributionRelation>();
            List<AgentCustomerRelation> customerRelatin = AgentCustomerRelationBLL.SingleModel.GetListByAccountId(dzaccount.Id.ToString());
            if (customerRelatin != null && customerRelatin.Count > 0)
            {
                string qrcodeids = string.Join(",",customerRelatin.Where(w=>w.QrcodeId>0)?.Select(s=>s.QrcodeId));
                viewmodel.DataList = _agentDistributionRelationBLL.GetCustomerDistributionRelationList(openstate, opentype,starttime, endtime, agentname, loginid, qrcodeids, pageIndex, pageSize, ref count);
            }
            
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.Loginid = loginid;
            ViewBag.AgentName = agentname;
            ViewBag.OpenState = openstate;
            ViewBag.OpenType = opentype;

            return View(viewmodel);
        }


        public ActionResult CustomerCaseBackList()
        {
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            string starttime = Context.GetRequest("starttime", "");
            string endtime = Context.GetRequest("endtime", "");
            string soucefrom = Context.GetRequest("soucefrom", "");
            string agentname = Context.GetRequest("agentname", "");

            if (dzaccount == null)
            {
                return View("PageError", new Return_Msg() { Msg = "登陆过期，请重新登陆!", code = "500" });
            }
            
            int count = 0;
            ViewModel<AgentCaseBack> viewmodel = new ViewModel<AgentCaseBack>();
            List<AgentCustomerRelation> customerrelationlist = AgentCustomerRelationBLL.SingleModel.GetListByAccountId(dzaccount.Id.ToString());
            if (customerrelationlist != null && customerrelationlist.Count > 0)
            {
                string agentids = string.Join(",",customerrelationlist.Select(s=>s.agentid));
                viewmodel.DataList = AgentCaseBackBLL.SingleModel.GetAgentCaseBackList(agentname, soucefrom, starttime, endtime, agentids, pageIndex, pageSize, ref count, true, 1);
            }
                
            viewmodel.TotalCount = count;
            viewmodel.PageIndex = pageIndex;
            viewmodel.PageSize = pageSize;
            ViewBag.starttime = starttime;
            ViewBag.endtime = endtime;
            
            return View(viewmodel);
        }
        public ActionResult SaveAgentBack()
        {
            msg = new Return_Msg();
            int id = Context.GetRequestInt("id",0);
            string bankaccount = Context.GetRequest("bankaccount","");
            string aliaccount = Context.GetRequest("aliaccount", "");
            if(dzaccount==null)
            {
                msg.Msg = "登陆过期，请重新登陆！";
                return Json(msg);
            }
            if(id<=0)
            {
                msg.Msg = "参数错误";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(bankaccount))
            {
                msg.Msg = "请输入银行账号";
                return Json(msg);
            }
            if (string.IsNullOrEmpty(aliaccount))
            {
                msg.Msg = "请输入支付宝账号";
                return Json(msg);
            }

            List<AgentCustomerRelation> customerRelatioin = AgentCustomerRelationBLL.SingleModel.GetListByAccountId(dzaccount.Id.ToString());
            if(customerRelatioin==null || customerRelatioin.Count<=0)
            {
                msg.Msg = "权限不足";
                return Json(msg);
            }

            string qrcodeids = string.Join(",",customerRelatioin.Where(w=>w.QrcodeId>0)?.Select(s=>s.QrcodeId).Distinct());
            AgentDistributionRelation agentrelationmodel = _agentDistributionRelationBLL.GetModel(id);
            if (agentrelationmodel == null || !qrcodeids.Contains(agentrelationmodel.QrCodeId.ToString()))
            {
                msg.Msg = "操作权限不足";
                return Json(msg);
            }

            AgentCaseBack agentcasebackgmodel = new AgentCaseBack();
            agentcasebackgmodel.AgentDistributionRelatioinId = id;
            agentcasebackgmodel.AddTime = DateTime.Now;
            agentcasebackgmodel.UpdateTime = DateTime.Now;
            agentcasebackgmodel.Invoice = 0;
            agentcasebackgmodel.AlipayAccount = aliaccount;
            agentcasebackgmodel.ImgUrl = "";
            agentcasebackgmodel.BankAccount = bankaccount;
            agentcasebackgmodel.CourierNumber = "";
            agentcasebackgmodel.State = 0;
            agentcasebackgmodel.DataType = 1;
            //分销用户信息
            List<DistributionUserInfo> duserinfo = _agentDistributionRelationBLL.GetDistributionUserInfo(id);
            if (duserinfo != null && duserinfo.Count > 0)
            {
                agentcasebackgmodel.DUserInfo = JsonConvert.SerializeObject(duserinfo);
            }

            agentcasebackgmodel.Id = Convert.ToInt32(AgentCaseBackBLL.SingleModel.Add(agentcasebackgmodel));

            msg.isok = agentcasebackgmodel.Id > 0;
            msg.Msg = msg.isok ? "保存成功" : "保存失败";
            foreach (AgentCustomerRelation item in customerRelatioin)
            {
                AgentCaseBackBLL.SingleModel.RemoveCache(item.agentid);
            }

            return Json(msg);
        }
    }
}