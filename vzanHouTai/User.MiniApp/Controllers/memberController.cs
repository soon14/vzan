using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;

using Utility;
using Utility.MemberLogin; 
using BLL.MiniApp;
using Core.MiniApp;
using Entity.MiniApp;
using DAL.Base; 
using BLL.MiniApp.Home;
using Entity.MiniApp.Home;
using Entity.MiniApp.Friend;
using Utility.AliOss; 
using System.Configuration;
using Newtonsoft.Json; 
using Entity.MiniApp.Conf;
using BLL.MiniApp.Conf;
using Entity.MiniApp.User;

namespace User.MiniApp.Controllers
{
    public class memberController : baseController
    {
        private CommonCore commonCore = new CommonCore();
        private MemberBLL bllMember = new MemberBLL(); 
        //private ProductCore coreProduct = new ProductCore(); 
        protected Return_Msg msg = new Return_Msg();
        private  OpenAuthorizerConfigBLL _openauthorizerconfigBll=new  OpenAuthorizerConfigBLL();
        private AgentdepositLogBLL _agentdepositlogBll = new AgentdepositLogBLL();
        
         
        [HttpPost]
        public ContentResult SaveHeadImg(string imgName)
        {
            string strWhere = "AccountId='" + dzuserId + "'";
            Member member = bllMember.GetModel(strWhere);
            member.Avatar = imgName;
            if (bllMember.Update(member))
            {
                try
                {
                    return Content("");
                }
                catch (Exception )
                {
                    return Content("");
                }
            }
            return Content("");
        }

         

        [CheckLogin]
        public ActionResult PerfectData()
        {
            Account account = AccountBLL.SingleModel.GetModel(dzuserId);
            ViewBag.LoginId = account.LoginId;
            ViewBag.IsUpdateId = account.IsUpdateId;
            ViewBag.Title = "用户资料";
            ViewBag.Path = ViewBag.Title;
            ViewBag.NavName = ViewBag.Title;

            Member member = bllMember.GetModel(string.Format("AccountId='{0}'", dzuserId));// membersFromCRM.GetMemberModel(string.Format("AccountId='{0}'", guserId));//xrl要调用加密接口
            //解密
            member.ConsigneePhone1 = DESEncryptTools.GetMd5Base32(member.ConsigneePhone1);
            member.EMail = DESEncryptTools.GetMd5Base32(member.EMail);
            //---加**号
            member.ConsigneePhone1 = commonCore.PhoneToNickName(member.ConsigneePhone1);
            member.EMail = commonCore.EMaliToNickName(member.EMail);

            ViewBag.PassWord = account.Password;// "E10ADC3949BA59ABBE56E057F20F883E";
            long sessonid = Utility.CheckSum.ComputeCheckSum("resetpassword:" + Session.SessionID);
            ViewBag.BindCode = sessonid;
            RedisUtil.Set<object>("resetpassword:" + sessonid.ToString(), new { OpenId = account.OpenId, IsScan = 0 }, TimeSpan.FromHours(3));
            return View(member);
        }
        [CheckLogin]
        public ActionResult IsScan(string wxkey)
        {
            if (string.IsNullOrEmpty(wxkey))
            {
                return Content("-1");
            }
            Dictionary<string, string> dic = RedisUtil.Get<Dictionary<string, string>>("resetpassword:" + wxkey);
            if (dic == null)
            {
                return Content("-1");
            }
            string IsScan = dic["IsScan"];
            if (IsScan != "1")//没有扫码
            {
                return Content("-1");
            }
            try
            {
                Account model = AccountBLL.SingleModel.GetModel(dzuserId);
                if (model != null)
                {
                    model.Password = "E10ADC3949BA59ABBE56E057F20F883E";//重置密码
                    AccountBLL.SingleModel.Update(model);
                    RedisUtil.Remove("resetpassword:" + wxkey);
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Content("-1");
            }
            return Content("-1");
        }
    }
}
