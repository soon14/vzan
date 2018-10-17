using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Home;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Home;
using Entity.MiniApp.User;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiGwController : InheritController
    {
    }
        [ExceptionLog]
    public class apiMiniAppGwController : apiGwController
    {
        protected static readonly AgentDistributionRelationBLL _agentDistributionRelationBLL=new AgentDistributionRelationBLL();

        //修改密码发送验证码缓存
        private readonly string _resetPasswordkey = "dz_phone_{0}_bindvalidatecode_resetpwd";
        /// <summary>
        /// 实例化对象
        /// </summary>
        public apiMiniAppGwController()
        {
            
        } 

        /// <summary>
        /// 小程序商店
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Index( int pageIndex = 1, int pageSize = 100)
        {
            string strWhere =$"Qrcode not like '%default_qrcode%'";
            string AppTitle = Utility.IO.Context.GetRequest("keys", string.Empty);
            if (!string.IsNullOrEmpty(AppTitle))
                strWhere += $" and Tag like '%{AppTitle}%'";
            List<Gw> list = GwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex);
            return Json(new { isok = true, msg = "请求成功", obj = list }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取排行榜
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult RangeApp(int pageIndex = 1, int pageSize = 10,int type = 0)
        {
            string strWhere = $"Qrcode not like '%default_qrcode%' ";
            string order = "cast(viewNumbers as SIGNED INTEGER) desc";
           
            if (type == 1)
            {
                //本周上升
                order = "(cast(viewNumbers as SIGNED INTEGER)- cast(LastViewNumbers as SIGNED INTEGER)) desc";
            }

            if (type == 2)
            {
                //本周新增
                strWhere += $" and addtime>'{DateTime.Now.AddDays(-3)}' and addtime<'{DateTime.Now.AddDays(3)}'";
            }
          
            List<RangeGw> list = RangeGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", order);
            int TotalCount = RangeGwBLL.SingleModel.GetCount(strWhere);
            if (TotalCount == 0 && type == 2)
            {
                
                order = "cast(viewNumbers as SIGNED INTEGER) desc";
                list = RangeGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex, "*", order);
            }

            return Json(new { isok = true, msg = "请求成功", obj = list }, JsonRequestBehavior.AllowGet);

        }
        
        /// <summary>
        /// 新闻资讯或者深度观点列表
        /// </summary>
        /// <param name="Type">0→资讯  1 →深度观点  2拼享惠资讯</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult NewsList(int Type = 0, int pageIndex = 1, int pageSize = 20)
        {
            string strWhere = $"State>0 and Type={Type}";
            string sortWhere = "addtime desc";
           
            List<NewsGw> list = NewsGwBLL.SingleModel.GetList(strWhere, pageSize, pageIndex,"*", sortWhere);
            return Json(new { isok = true, msg = "请求成功", obj = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendUserAdvisory()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            string name = Utility.IO.Context.GetRequest("username", string.Empty);
            string phone = Utility.IO.Context.GetRequest("phone", string.Empty);
            int datasource = Utility.IO.Context.GetRequestInt("source", 0);
            int type = Utility.IO.Context.GetRequestInt("type", 5);
            if (string.IsNullOrEmpty(name))
            {
                result.Msg = "请输入您的称呼";
                return Json(result);
            }
            if (string.IsNullOrEmpty(phone))
            {
                result.Msg = "请输入您的手机号码";
                return Json(result);
            }
            if (!Regex.IsMatch(phone, @"^[1]+[3-9]+\d{9}$"))
            {
                result.Msg = "手机格式不正确";
                return Json(result);
            }
            Hfeedback model = new Hfeedback()
            {
                name = name,
                phone = phone,
                datasource = datasource,
                type = type,
                addtime = DateTime.Now
            };
            result.isok = Convert.ToInt32(HfeedbackBLL.SingleModel.Add(model)) > 0;
            result.Msg = result.isok ? "发送成功" : "发送失败";
            return Json(result);
        }

        public ActionResult SaveUserInfo()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            string password = Utility.IO.Context.GetRequest("password", string.Empty);
            string phone = Utility.IO.Context.GetRequest("phone", string.Empty);
            string code = Utility.IO.Context.GetRequest("code", string.Empty);
            string address = Utility.IO.Context.GetRequest("address", string.Empty);
            string sourcefrom = Utility.IO.Context.GetRequest("sourcefrom", "");
            int agentqrcodeid = Utility.IO.Context.GetRequestInt("agentqrcodeid", 0);
            result.isok = false;
            if (string.IsNullOrEmpty(phone))
            {
                result.Msg = "请输入手机号";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(code))
            {
                result.Msg = "请输入验证码";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(password))
            {
                result.Msg = "请输入密码";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (sourcefrom.Length > 20)
            {
                result.Msg = "无效来源";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(phone))
            {
                Account tempaccout = AccountBLL.SingleModel.GetModelByPhone(phone);
                if (tempaccout != null)
                {
                    result.Msg = "该手机号已被注册";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            //是否校验手机号,0检验，1不校验
            //校验验证码
            Return_Msg _msg = CommondHelper.CheckVaildCode(phone, code);
            result.isok = _msg.isok;
            result.Msg = _msg.Msg;
            result.code = _msg.code;
            result.dataObj = _msg.dataObj;
            if (!result.isok)
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            Account account = null;
            //如果是代理分销扫描注册，测判断绑定的手机号是否已经注册过账号，如果没有则注册一个账号
            if (agentqrcodeid > 0)
            {
                account = AccountBLL.SingleModel.GetModelByPhone(phone);
            }
            if (account == null)
            {
                account = AccountBLL.SingleModel.WeiXinRegister("", 0, "", true, address, phone, sourcefrom, password);
            }
            else
            {
                //修改已经注册过的用户信息
                AccountBLL.SingleModel.UpdateUserInfo(account.Id.ToString(), phone, password, address);
            }

            if (account != null)
            {
                //用户已绑定手机号，判断是否有单页版
                XcxAppAccountRelation usertemplate = _xcxAppAccountRelationBLL.GetModelByStringId(account.Id.ToString());
                if (usertemplate == null)
                {
                    //免费开通单页版
                    _xcxAppAccountRelationBLL.AddFreeTemplate(account);
                }

                //如果是扫分销代理码注册，则开通代理，
                if (agentqrcodeid > 0)
                {
                    result.Msg = _agentDistributionRelationBLL.CreateDistributionAgent(account.Id.ToString(), agentqrcodeid);
                    if (result.Msg.ToString() != "")
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }

                result.isok = true;
                result.Msg = "注册成功";
            }
            else
            {
                result.isok = false;
                result.Msg = "注册失败";
            }
            string key = string.Format(_resetPasswordkey, phone);
            RedisUtil.Remove(key);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 发送验证码修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetVaildCode()
        {
            Return_Msg_APP result = new Return_Msg_APP();
            string phoneNum = Context.GetRequest("phonenum", "");
            //修改密码 type=0  注册type=1
            int type = Context.GetRequestInt("type", 0);

            Account account = AccountBLL.SingleModel.GetModelByPhone(phoneNum);

            //代理分销，判断是否已开通过代理，开通过代理就不给他开通
            int agentqrcodeid = Context.GetRequestInt("agentqrcodeid", 0);
            if (agentqrcodeid > 0 && account != null)
            {
                Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(account.Id.ToString());
                if (agentmodel != null)
                {
                    result.Msg = "该手机号已经绑定了代理商账号";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            Return_Msg _msg = CommondHelper.GetVaildCode(agentqrcodeid, phoneNum, account, type);
            result.isok = _msg.isok;
            result.Msg = _msg.Msg;
            result.code = _msg.code;
            result.dataObj = _msg.dataObj;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}