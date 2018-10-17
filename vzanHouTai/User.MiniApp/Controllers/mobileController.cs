using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Home;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Home;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{
    public class mobileController : Controller
    {


        
        protected readonly AgentDistributionRelationBLL _agentDistributionRelationBLL = new AgentDistributionRelationBLL();
        //修改密码发送验证码缓存
        private readonly string _resetPasswordkey = "dz_phone_{0}_bindvalidatecode_resetpwd";
        private Return_Msg _msg = new Return_Msg();
        /// <summary>
        /// 移动端：代理招商
        /// </summary>
        /// <returns></returns>
        public ActionResult mAgent()
        {
            return View();
        }
        /// <summary>
        /// 移动端：免费领取小程序
        /// </summary>
        /// <returns></returns>
        public ActionResult freeTrial()
        {
            return View();
        }
        /// <summary>
        /// 移动端：领取小程序
        /// </summary>
        /// <returns></returns>
        public ActionResult mHome()
        {
            return View();
        }

        public ActionResult mobileHome()
        {
            return View();
        }
        public ActionResult mobileReg()
        {
            //代理分销，判断是否已开通过代理，开通过代理就不给他开通
            int agentqrcodeid = Context.GetRequestInt("agentqrcodeid", 0);
            if(agentqrcodeid>0)
            {
                AgentQrCode agentqrmodel = AgentQrCodeBLL.SingleModel.GetModelById(agentqrcodeid);
                if (agentqrmodel == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "二维码已失效!", code = "403" });
                }
            }
            ViewBag.AgentQrCodeId = agentqrcodeid;

            return View();
        }
        public ActionResult mobileCoreds()
        {
            return View();
        }

        public ActionResult mobileCoreyy()
        {
            return View();
        }
        public ActionResult mobileCorefl()
        {
            return View();
        }
        public ActionResult mobileCorezb()
        {
            return View();
        }
        public ActionResult mobileCorezx()
        {
            return View();
        }
        public ActionResult mobileCorept()
        {
            return View();
        }
        public ActionResult mobileCorekj()
        {
            return View();
        }
        public ActionResult mobileCoresp()
        {
            return View();
        }
        public ActionResult mobileMarketingvip()
        {
            return View();
        }
        public ActionResult mobileMarketingcz()
        {
            return View();
        }
        public ActionResult mobileMarketingkj()
        {
            return View();
        }
        public ActionResult mobileMarketingpt()
        {
            return View();
        }
        public ActionResult mobileOtherdt()
        {
            return View();
        }
        public ActionResult mobileOtherbm()
        {
            return View();
        }
        public ActionResult mobileOtherkf()
        {
            return View();
        }
        public ActionResult mobileOtherfx()
        {
            return View();
        }
        public ActionResult mobileOtheryy()
        {
            return View();
        }
        public ActionResult mobileOtherfj()
        {
            return View();
        }
        public ActionResult mobileOthertz()
        {
            return View();
        }
        public ActionResult mobileOtherdd()
        {
            return View();
        }
        public ActionResult mobileVideoList()
        {
            return View();
        }
        /// <summary>
        /// 保存注册信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveUserInfo()
        {
            string password = Utility.IO.Context.GetRequest("password", string.Empty);
            string phone = Utility.IO.Context.GetRequest("phone", string.Empty);
            string code = Utility.IO.Context.GetRequest("code", string.Empty);
            string address = Utility.IO.Context.GetRequest("address", string.Empty);
            string sourcefrom = Utility.IO.Context.GetRequest("sourcefrom", "");
            int agentqrcodeid = Utility.IO.Context.GetRequestInt("agentqrcodeid",0);
            int opentype = Utility.IO.Context.GetRequestInt("opentype", 0);
            string username = Utility.IO.Context.GetRequest("username", "");
            _msg.isok = false;
            //if (agentqrcodeid>0 && string.IsNullOrEmpty(username))
            //{
            //    _msg.Msg = "请输入姓名";
            //    return Json(_msg, JsonRequestBehavior.AllowGet);
            //}
            if (string.IsNullOrEmpty(phone))
            {
                _msg.Msg = "请输入手机号";
                return Json(_msg, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(code))
            {
                _msg.Msg = "请输入验证码";
                return Json(_msg, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(password))
            {
                _msg.Msg = "请输入密码";
                return Json(_msg, JsonRequestBehavior.AllowGet);
            }

            if (sourcefrom.Length > 20)
            {
                _msg.Msg = "无效来源";
                return Json(_msg, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(phone))
            {
                Account tempaccout = AccountBLL.SingleModel.GetModelByPhone(phone);
                if (tempaccout != null)
                {
                    _msg.Msg = "该手机号已被注册";
                    return Json(_msg, JsonRequestBehavior.AllowGet);
                }
            }


            //是否校验手机号,0检验，1不校验
            //校验验证码
            _msg = CommondHelper.CheckVaildCode(phone, code);
            if (!_msg.isok)
            {
                return Json(_msg, JsonRequestBehavior.AllowGet);
            }
            Account account = null;
            //如果是代理分销扫描注册，测判断绑定的手机号是否已经注册过账号，如果没有则注册一个账号
            if (agentqrcodeid>0)
            {
                sourcefrom = "代理分销推广";
                account = AccountBLL.SingleModel.GetModelByPhone(phone);
            }
            if(account==null)
            {
                account = AccountBLL.SingleModel.WeiXinRegister("", 0, "", true, address, phone, sourcefrom, password);
            }
            else
            {
                //修改已经注册过的用户信息
                AccountBLL.SingleModel.UpdateUserInfo(account.Id.ToString(),phone,password,address);
            }

            if(account!=null)
            {
                //用户已绑定手机号，判断是否有单页版
                XcxAppAccountRelation usertemplate = XcxAppAccountRelationBLL.SingleModel.GetModelByaccound(account.Id.ToString());
                if (usertemplate == null)
                {
                    //免费开通单页版
                    XcxAppAccountRelationBLL.SingleModel.AddFreeTemplate(account);
                }

                //如果是扫分销代理码注册，则开通代理，
                if (agentqrcodeid > 0)
                {
                    //判断是否是代理商
                    Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(account.Id.ToString(), -1);
                    if (agentinfo != null)
                    {
                        _msg.isok = true;
                        _msg.Msg = "注册失败，该号码已绑定了代理商账号";
                        return Json(_msg, JsonRequestBehavior.AllowGet);
                    }

                    _msg.Msg = _agentDistributionRelationBLL.CreateDistributionAgent(account.Id.ToString(), agentqrcodeid, opentype,username);
                    if (_msg.Msg != "")
                    {
                        return Json(_msg, JsonRequestBehavior.AllowGet);
                    }
                }

                _msg.isok = true;
                _msg.Msg = "注册成功";
            }
            else
            {
                _msg.isok = false;
                _msg.Msg = "注册失败";
            }
            string key = string.Format(_resetPasswordkey, phone);
            RedisUtil.Remove(key);

            return Json(_msg, JsonRequestBehavior.AllowGet);
        }

        //代理商建站首页
        public ActionResult indexDl()
        {
            string domian = System.Web.HttpContext.Current.Request.Url.Host;
            if (string.IsNullOrEmpty(domian))
            {
                return View("PageError", new Return_Msg() { Msg = "请求异常(域名不合法)!", code = "500" });
            }

            if (domian.Contains(WebSiteConfig.DzWebSiteDomainExt))
            {
                //表示二级域名
                domian = domian.Replace(WebSiteConfig.DzWebSiteDomainExt, "");
            }
            if (domian.Contains("xn--"))//表示中文域名,会进行punycode编码 例如:www.九美.com   System.Web.HttpContext.Current.Request.Url.Host;实际拿到的是 www.xn--sjq221j.com
            {
                System.Globalization.IdnMapping idn = new System.Globalization.IdnMapping();

                domian = idn.GetUnicode(domian);
            }
            AgentWebSiteInfo agentWebSiteInfo = AgentWebSiteInfoBLL.SingleModel.GetModelByDomian(domian);
            if (agentWebSiteInfo == null)
            {
                return View("PageError", new Return_Msg() { Msg = "请求异常(没有数据)!", code = "500" });
            }
            if (agentWebSiteInfo.webState == -1)
            {
                return View("PageError", new Return_Msg() { Msg = "网站已被停用(请联系客户)!", code = "403" });
            }

            if (!string.IsNullOrEmpty(agentWebSiteInfo.seoConfig))
            {
                agentWebSiteInfo.seoConfigModel = JsonConvert.DeserializeObject<SeoConfigModel>(agentWebSiteInfo.seoConfig);
            }
            if (!string.IsNullOrEmpty(agentWebSiteInfo.pageMsgConfig))
            {
                agentWebSiteInfo.pageMsgConfigModel = JsonConvert.DeserializeObject<PageMsgConfigModel>(agentWebSiteInfo.pageMsgConfig);
            }



            return View(agentWebSiteInfo);
        }



    }
}