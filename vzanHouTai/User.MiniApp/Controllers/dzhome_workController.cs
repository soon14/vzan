using BLL.MiniApp;
using Entity.MiniApp.User;
using Entity.MiniApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Entity.MiniApp.Conf;
using Utility;
using Utility.IO;
using BLL.MiniApp.Conf;
using User.MiniApp.Filters;

namespace User.MiniApp.Controllers
{
    public partial class dzhomeController : baseController
    {
        #region 新版
        public ActionResult CaseTemplate()
        {
            int opensinglepagefree = Context.GetRequestInt("opensinglepagefree", 0);
            ViewBag.testxcxType = _testxcxtype;
            ViewBag.freexcxType = _freexcxtype;
            ViewBag.Phone = "未绑定";
            ViewBag.BindPhone = 0;

            Return_Msg msg = new Return_Msg();
            ViewModelMyWorkbench model = GetUserInfo();
            if (opensinglepagefree > 0)
            {
                TestTemplate();
            }

            if (model._Member == null || model._Account == null)
            {
                CookieHelper.Remove("dz_UserCookieNew");
                return Redirect("/dzhome/Login");
            }

            //判断是否子帐号登陆
            AuthRole role = RouteAuthCheck.GetAdminAuth();
            if (role != null)
            {
                int pageType = XcxAppAccountRelationBLL.SingleModel.GetXcxTemplateType(role.AId);
                string redirectUrl = string.Empty;
                Dictionary<int, string> pageTypeIndex = new Dictionary<int, string>
                {
                    { (int)TmpType.小程序专业模板, $"/SubAccount/Welcome?appId={role.AId}&pageType={pageType}" },
                    { (int)TmpType.小未平台,  $"/Plat/admin/Index?Id={role.AId}&appId={role.AId}&pageType={pageType}"},
                    { (int)TmpType.拼享惠,  $"/Pin/main/Index?Id={role.AId}&appId={role.AId}&pageType={pageType}"},
                };
                //禁止专业版子帐号，进入小程序管理列表页
                if(pageTypeIndex.TryGetValue(pageType,out redirectUrl) && !string.IsNullOrWhiteSpace(redirectUrl))
                {
                    return Redirect(redirectUrl);
                }
            }

            //判断是否是代理商
            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(model._Account.Id.ToString());//GetModel($"useraccountid='{model._Account.Id.ToString()}'");
            if (agentmodel != null)
            {
                //赠送一年免费的平台版
                if (agentmodel.userLevel == 0 && agentmodel.addtime <= DateTime.Parse("2018-06-21"))
                {
                    XcxAppAccountRelationBLL.SingleModel.AddTemplate(model._Account, agentmodel);
                }
                return View(model);
            }

            //判断是否是代理商开通的用户
            AgentCustomerRelation agentcutomer = AgentCustomerRelationBLL.SingleModel.GetModel($"useraccountid='{model._Account.Id.ToString()}'");
            if (agentcutomer != null)
            {
                return View(model);
            }

            //判断是否有用户未绑定手机号码
            if (string.IsNullOrEmpty(model._Account.ConsigneePhone))
            {
                ViewBag.BindPhone = 1;
            }
            else
            {
                //用户已绑定手机号，判断是否有单页版
                XcxAppAccountRelation usertemplate = XcxAppAccountRelationBLL.SingleModel.GetModel($"accountid='{model._Account.Id}'");
                if (usertemplate == null)
                {
                    //免费开通单页版
                    XcxAppAccountRelationBLL.SingleModel.AddFreeTemplate(model._Account);
                }
            }

            return View(model);
        }
        public readonly string _customerLoginId = WebSiteConfig.CustomerLoginId ?? "";

        /// <summary>
        /// 用户开通的模板
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserTemplatesList()
        {
            int pageindex = Context.GetRequestInt("pageindex", 1);
            int pagesize = Context.GetRequestInt("pagesize", 20);

            try
            {
                Account account = AccountBLL.SingleModel.GetModel(dzuserId);
                List<XcxAppAccountRelation> miniapplist = new List<XcxAppAccountRelation>();
                List<object> mytemplatelist = new List<object>();
                if (account != null)
                {
                    bool ismodel = _customerLoginId.Contains(account.LoginId);
                    #region 小程序
                    //判断是否是代理登陆
                    if (agentaccount != null && agentinfo != null && agentinfo.id > 0 && dzuserId != agentuserId)
                    {
                        miniapplist = XcxAppAccountRelationBLL.SingleModel.GetListByaccountIdandAgentId(account.Id.ToString(), agentinfo.id, 0, pagesize, pageindex);
                    }
                    else
                    {
                        miniapplist = XcxAppAccountRelationBLL.SingleModel.GetListByaccountIdandAgentId(account.Id.ToString(), 0, 0, pagesize, pageindex);
                    }

                    if (miniapplist != null && miniapplist.Count > 0)
                    {
                        List<CustomModelRelation> customModelList = new List<CustomModelRelation>();
                        if (ismodel)
                        {
                            string aids = string.Join(",", miniapplist.Select(s => s.Id));
                            customModelList = CustomModelRelationBLL.SingleModel.GetListByAIds(aids);
                        }

                        string ids = string.Join(",", miniapplist.Select(s => s.TId).Distinct());
                        List<XcxTemplate> xcxtemplatelist = XcxTemplateBLL.SingleModel.GetList("id in (" + ids + ")");
                        if (xcxtemplatelist != null && xcxtemplatelist.Count > 0)
                        {
                            string appIds = $"'{string.Join("','", miniapplist.Where(w=>!string.IsNullOrEmpty(w.AppId))?.Select(s=>s.AppId))}'";
                            List<UserXcxTemplate> userXcxTemplateList = UserXcxTemplateBLL.SingleModel.GetListByAppIds(appIds);
                            foreach (XcxAppAccountRelation item in miniapplist)
                            {
                                List<OpenAuthorizerConfig> authomodellist = OpenAuthorizerConfigBLL.SingleModel.GetListByaccoundidAndRid(item.AccountId.ToString(), item.Id);
                                XcxTemplate xcxtempmodel = xcxtemplatelist.Where(w => w.Id == item.TId).FirstOrDefault();
                                if (item.AuthoAppType == 1)
                                {
                                    if (!string.IsNullOrEmpty(item.AppId))
                                    {
                                        OpenAuthorizerConfig authomodel = authomodellist != null && authomodellist.Count > 0 ? authomodellist[0] : null;
                                        if (authomodel != null)
                                        {
                                            item.XcxName = authomodel.nick_name;

                                            UserXcxTemplate uploadmodel = userXcxTemplateList?.FirstOrDefault(f=>f.AppId==item.AppId);
                                            if (uploadmodel != null)
                                            {
                                                if (uploadmodel.AddTime.ToString("yyyy-MM-dd") == "0001-01-01")
                                                {
                                                    uploadmodel.State = (int)XcxTypeEnum.未发布;
                                                    UserXcxTemplateBLL.SingleModel.Update(uploadmodel, "state");
                                                }
                                                item.UploadState = uploadmodel.State;
                                                item.UploadStateName = Enum.GetName(typeof(XcxTypeEnum), uploadmodel.State);
                                            }
                                            else
                                            {
                                                item.UploadState = (int)XcxTypeEnum.未发布;
                                                item.UploadStateName = XcxTypeEnum.未发布.ToString();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        item.XcxName = "未绑定";
                                        item.UploadState = (int)XcxTypeEnum.未发布;
                                        item.UploadStateName = XcxTypeEnum.未发布.ToString();
                                    }
                                }
                                else
                                {
                                    UserXcxTemplate uploadmodel = userXcxTemplateList?.FirstOrDefault(f => f.AppId == item.AppId);
                                    item.XcxName = uploadmodel?.Name;
                                }

                                if (item.outtime <= DateTime.Now)
                                {
                                    item.TimeLength = 0;
                                }
                                else
                                {
                                    TimeSpan sp = item.outtime.Subtract(DateTime.Now);
                                    item.TimeLength = sp.Days<=0?1:sp.Days;
                                }

                                if (xcxtempmodel != null)
                                {
                                    string bangdinurl = "/Config/MiniAppConfig?appId=" + item.Id + "&id=" + item.Id + "&type=" + xcxtempmodel.Type;

                                    string TName = xcxtempmodel.TName.Replace("专业版","专业");
                                    if (xcxtempmodel.Type == 22)
                                    {
                                        TName += AgentdepositLogBLL.SingleModel.GetVerName(item.VersionId);
                                    }
                                    string modelname = "";
                                    if (ismodel)
                                    {
                                        modelname = customModelList.Where(w => w.AId == item.Id).FirstOrDefault()?.Name;
                                        modelname = string.IsNullOrEmpty(modelname) ? TName : modelname;
                                    }

                                    mytemplatelist.Add(new
                                    {
                                        Id = item.Id,
                                        Title = TName,
                                        Url = "/" + item.Url + "?Id=" + item.Id + "&appId=" + item.Id + "&versionId=" + item.VersionId,//跳转管理路径
                                        item.TimeLength,
                                        outtime = item.outtime.ToString("yyyy-MM-dd"),
                                        XcxName = item.XcxName,
                                        UploadState = item.UploadState,
                                        UploadStateName = item.UploadStateName,
                                        item.State,
                                        item.AppId,
                                        xcxtempmodel.Type,
                                        bangdinurl = bangdinurl,//绑定路径
                                        yulang = $"/config/GettestQrcode?Id={item.Id}&tid={item.TId}&needpay=0",//预览路径
                                        iosimg = "",
                                        logoimg = xcxtempmodel.TImgurl,//小图标
                                        item.IsExperience,
                                        ismodel,
                                        modelstate = customModelList.Where(w => w.AId == item.Id).FirstOrDefault()?.State,
                                        modelname,
                                        modeldesc = customModelList.Where(w => w.AId == item.Id).FirstOrDefault()?.Desc,
                                        modelimgurl = customModelList.Where(w => w.AId == item.Id).FirstOrDefault()?.ImgUrl,
                                    });
                                    }
                                }
                            }
                    }


                        return Json(new { isok = 1, msg = "成功", obj = mytemplatelist, accountid = dzuserId }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "找不到用户信息", obj = new List<object>() }, JsonRequestBehavior.AllowGet);
                }
                #endregion
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
            }
            
            return Json(new { isok = -1, msg = "获取用户已开通模板数据出错",obj=new List<object>() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取模板市场数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTemplatesList()
        {
            Return_Msg msg = new Return_Msg();
            try
            {
                List<XcxTemplate> templatelist = XcxTemplateBLL.SingleModel.GetListByIdsandProjectType((int)ProjectType.小程序);
                if (templatelist != null && templatelist.Count > 0)
                {
                    foreach (XcxTemplate xcxtempmodel in templatelist)
                    {
                        if (!string.IsNullOrEmpty(_freexcxtype) && _freexcxtype.Contains("," + xcxtempmodel.Type.ToString() + ","))
                        {
                            //是否免费使用
                            xcxtempmodel.FreeUse = 1;
                        }
                        else if (!string.IsNullOrEmpty(_testxcxtype) && _testxcxtype.Contains("," + xcxtempmodel.Type.ToString() + ","))
                        {
                            //是否可以试用
                            xcxtempmodel.TestUse = 1;
                        }
                    }
                }

                msg.Msg = "成功";
                msg.isok = true;
                msg.dataObj = templatelist;
            }
            catch (Exception ex)
            {
                msg.Msg = "异常：" + ex.Message;
                msg.isok = false;
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取登陆用户数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserData()
        {
            Return_Msg msg = new Return_Msg();
            ViewModelMyWorkbench model = new ViewModelMyWorkbench();
            model._Member = MemberBLL.SingleModel.GetModel(string.Format("AccountId='{0}'", dzuserId));
            model._Account = AccountBLL.SingleModel.GetModel(dzuserId);
            if (model._Member != null)
            {
                if (model._Account != null)
                {
                    Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(model._Account.Id.ToString());
                    msg.isok = true;
                    msg.Msg = "成功";
                    msg.dataObj = new { userinfo = model._Account, agentinfo = agentinfo, isagent = agentinfo != null ? 1 : 0 };
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    CookieHelper.Remove("dz_UserCookieNew");
                    return RedirectToAction("Login", "dzhome");
                }
            }
            else
            {
                CookieHelper.Remove("dz_UserCookieNew");
                return RedirectToAction("Login", "dzhome");
            }
        }


        /// <summary>
        /// 免费试用
        /// </summary>
        /// <returns></returns>
        [CheckLogin()]
        public void TestTemplate()
        {
            Return_Msg msg = new Return_Msg();
            msg.isok = false;
            msg.Msg = "好可惜，试用失败";

            int tid = Context.GetRequestInt("tid", 0);
            int opensinglepagefree = Context.GetRequestInt("opensinglepagefree", 0);

            if (tid <= 0)
            {
                msg.Msg = "亲，试用模板ID不能为0";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            XcxTemplate xcxtempmodel = XcxTemplateBLL.SingleModel.GetModel(tid);
            if (xcxtempmodel == null)
            {
                msg.Msg = "哎呀模板不见了，请联系我们可爱的客服";

                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            if (dzuserId == null)
            {
                msg.code = "-10";
                msg.Msg = "亲您要先登录";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            Account accountmodel = AccountBLL.SingleModel.GetModel(dzuserId);
            if (accountmodel == null)
            {
                msg.Msg = "亲您要先登录";
                msg.code = "-10";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            if (string.IsNullOrEmpty(accountmodel.ConsigneePhone))
            {
                msg.Msg = "亲请先绑定手机号";
                msg.code = "-1";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            //模板过期时间
            DateTime outtime = DateTime.Now.AddDays(7);
            //0：免费使用，1：试用7天
            int freeuse = 0;

            //判断模板是否免费
            if (!string.IsNullOrEmpty(_freexcxtype) && _freexcxtype.Contains("," + xcxtempmodel.Type.ToString() + ","))
            {
                //配置文件中配置可以免费使用的模板类型
                //免费使用模板100年
                outtime = DateTime.Now.AddYears(100);
            }
            else if (!string.IsNullOrEmpty(_testxcxtype) && _testxcxtype.Contains("," + xcxtempmodel.Type.ToString() + ","))
            {
                freeuse = 1;
                //配置文件中配置可以试用的模板类型
                //限制单页版和官网版小程序可以试用
                msg.Msg = "亲，该模板暂不提供试用";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }
            else
            {
                msg.Msg = "暂不支持此功能，请刷新重试";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            XcxAppAccountRelation xcxrelationmodel = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndTid(accountmodel.Id.ToString(), tid, 1);
            if (xcxrelationmodel != null)
            {
                msg.Msg = "亲，您已经开通该模板了！";
                msg.code = "-2";
                msg.isok = true;
                msg.dataObj = "http://dz.vzan.com/dzhome/casetemplate";
                if (opensinglepagefree <= 0)
                {
                    Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                }
                return;
            }

            xcxrelationmodel = new XcxAppAccountRelation();
            xcxrelationmodel.TId = tid;
            xcxrelationmodel.AccountId = accountmodel.Id;
            xcxrelationmodel.price = xcxtempmodel.Price;
            xcxrelationmodel.Url = xcxtempmodel.Link;
            xcxrelationmodel.Desc = xcxtempmodel.Desc;
            xcxrelationmodel.AddTime = DateTime.Now;
            xcxrelationmodel.outtime = outtime;

            AgentdepositLog pricemodellog = new AgentdepositLog();
            pricemodellog.addtime = DateTime.Now;
            pricemodellog.afterDeposit = 0;
            pricemodellog.agentid = 0;
            pricemodellog.beforeDeposit = 0;
            pricemodellog.cost = 0;
            pricemodellog.costdetail = $"客户{(freeuse == 0 ? "免费使用" : "试用")}小程序模板：" + xcxtempmodel.TName;
            pricemodellog.type = 0;
            pricemodellog.tid = xcxtempmodel.Id;

            if (Convert.ToInt32(XcxAppAccountRelationBLL.SingleModel.Add(xcxrelationmodel)) > 0)
            {
                if (Convert.ToInt32(AgentdepositLogBLL.SingleModel.Add(pricemodellog)) > 0)
                {
                    msg.Msg = "欢迎" + (freeuse == 0 ? "使用" : "试用！");
                    msg.isok = true;
                    msg.dataObj = "http://dz.vzan.com/dzhome/casetemplate";
                    if (opensinglepagefree <= 0)
                    {
                        Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
                    }
                    return;
                }
            }

            if (opensinglepagefree <= 0)
            {
                Context.SuperResponseWrite(JsonConvert.SerializeObject(msg));
            }
            return;
        }

        #endregion

        #region 代理商免费体验
        public ActionResult experience()
        {
            return View();
        }
        public ActionResult GetFreeExperienceTemplate()
        {
            Return_Msg data = new Return_Msg();
            Account account = AccountBLL.SingleModel.GetModel(dzuserId);
            if (account == null)
            {
                return RedirectToAction("login", "dzhome");
            }

            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(dzuserId.ToString());
            if (agentmodel == null)
            {
                data.Msg = "您还不是代理商";
                return Json(data);
            }

            //付费模板
            List<XcxTemplate> xlist = XcxTemplateBLL.SingleModel.GetListByPriceType(1);
            if (xlist == null || xlist.Count == 0)
            {
                return Json(data);
            }

            //已开通过的体验模板
            List<XcxAppAccountRelation> userlist = XcxAppAccountRelationBLL.SingleModel.GetListByAccountId(agentmodel.useraccountid, agentmodel.id, 1);

            xlist.ForEach(f =>
            {
                XcxAppAccountRelation xmodel = userlist?.FirstOrDefault(x => x.TId == f.Id);
                //state字段暂时用来表示该模板是否已被体验过，0：未体验，1：已体验
                f.State = 0;
                f.statename = "未开通";
                f.year = WebSiteConfig.ExperienceDayLength;
                if (xmodel != null)
                {
                    f.State = 1;
                    f.outtime = xmodel.outtime.ToString("yyyy-MM-dd HH:mm:ss");
                    if (xmodel.outtime <= DateTime.Now)
                    {
                        f.State = 0;
                        f.year = 0;
                        f.statename = "已过期";
                    }
                    else
                    {
                        TimeSpan sp = xmodel.outtime.Subtract(DateTime.Now);
                        f.year = sp.Days;
                        f.statename = "已开通";
                    }
                    if (xmodel.State == -1)
                    {
                        f.State = -1;
                        f.statename = "已停用";
                    }
                }
            });
            data.dataObj = xlist;
            return Json(data);
        }

        /// <summary>
        /// 代理商免费体验所有付费模板30天
        /// </summary>
        public ActionResult OpenFreeExperience()
        {
            Return_Msg data = new Return_Msg();
            Account account = AccountBLL.SingleModel.GetModel(dzuserId);
            int daylength = WebSiteConfig.ExperienceDayLength;
            if (account == null)
            {
                return RedirectToAction("login", "dzhome");
            }

            Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(dzuserId.ToString());
            if (agentmodel == null)
            {
                data.Msg = "您还不是代理商";
                return Json(data);
            }

            string tids = Context.GetRequest("tids", "");
            if (string.IsNullOrEmpty(tids))
            {
                data.Msg = "请选择开通的模板";
                return Json(data);
            }

            string errormsg = "开通成功";
            bool isSuccess = XcxAppAccountRelationBLL.SingleModel.OpenExperience(agentmodel, tids, account, ref errormsg, daylength);
            if (!isSuccess)
            {
                errormsg = "开通失败";
            }
            data.isok = isSuccess;
            data.Msg = errormsg;
            return Json(data);
        }
        #endregion

        #region 装修模型
        public ActionResult SaveModelState()
        {
            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("aid",0);
            int state= Context.GetRequestInt("state", 0);
            string desc = Context.GetRequest("desc",string.Empty);
            string imgurl = Context.GetRequest("imgurl", string.Empty);
            string name = Context.GetRequest("name", string.Empty);
            if (aid<=0)
            {
                returndata.Msg = "参数不能为0";
                return Json(returndata);
            }
            Account account = AccountBLL.SingleModel.GetModel(dzuserId);
            if (account == null)
            {
                return RedirectToAction("login", "dzhome");
            }
            bool ismodel = _customerLoginId.Contains(account.LoginId);
            if(!ismodel)
            {
                returndata.Msg = "权限不够";
                return Json(returndata);
            }
            if (string.IsNullOrEmpty(imgurl) && state==1)
            {
                returndata.Msg = "请上传封面";
                return Json(returndata);
            }

            CustomModelRelation customModel = CustomModelRelationBLL.SingleModel.GetModelByAid(aid);
            if(customModel==null)
            {
                customModel = new CustomModelRelation();
                customModel.AddTime = DateTime.Now;
                customModel.UpdateTime = DateTime.Now;
                customModel.State = state;
                customModel.ImgUrl =imgurl;
                customModel.Name = name;
                customModel.AId = aid;
                customModel.Desc = desc;
                returndata.isok = Convert.ToInt32(CustomModelRelationBLL.SingleModel.Add(customModel))>0;
            }
            else
            {
                customModel.UpdateTime = DateTime.Now;
                customModel.State = state;
                string column = "state,updatetime";
                if(state==1)
                {
                    customModel.ImgUrl = imgurl;
                    customModel.Name = name;
                    customModel.Desc = desc;
                    column += ",name,desc,ImgUrl";
                }
                returndata.isok = CustomModelRelationBLL.SingleModel.Update(customModel,column);
            }

            CustomModelRelationBLL.SingleModel.RemoveCache();

            returndata.Msg = returndata.isok ? "保存成功" : "保存失败";
            return Json(returndata);
        }
        #endregion
    }
}