using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.ViewModel;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utility.IO;
using User.MiniApp.Comment;
using User.MiniApp.Model;
using Entity.MiniApp.User;
using Entity.MiniApp.Ent;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Plat;
using BLL.MiniApp.Alading;
using Entity.MiniApp.Alading;
using User.MiniApp.Filters;
using Utility;

namespace User.MiniApp.Controllers
{
    public class MiniappConfigController : configController
    {

    }

    [RouteAuthCheck]
    public class configController : baseController
    {
        protected readonly bool? isTrue = true; //是否为true;
        
        public configController()
        {
            
        }

        #region 保存基本设置
        public ActionResult SaveConfig(int id, string datajson)
        {
            if (dzaccount == null)
            {
                return Redirect("~/dzhome/login");
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(id);
            if (umodel == null)
            {
                return Json(new { isok = -1, msg = "您还没授权" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                List<ConfParam> paramslist = ConfParamBLL.SingleModel.GetModelByappid(umodel.AppId);
                if (paramslist != null && paramslist.Count > 0)
                {
                    List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
                    if (data != null && data.Count > 0)
                    {
                        foreach (ConfParam item in data)
                        {
                            ConfParam list = paramslist.FirstOrDefault(f => f.Param == item.Param);
                            if (list != null)
                            {
                                if (ConfParamBLL.SingleModel.UpdateList(item.Value, item.Param, umodel.AppId) <= 0)
                                {
                                    return Json(new { isok = -1, msg = "修改失败_" + item.Param }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                ConfParam model = new ConfParam();
                                model.AppId = umodel.AppId;
                                model.Param = item.Param;
                                model.Value = item.Value;
                                model.State = 0;
                                model.UpdateTime = DateTime.Now;
                                model.AddTime = DateTime.Now;
                                model.RId = id;
                                if (Convert.ToInt32(ConfParamBLL.SingleModel.Add(model)) <= 0)
                                {
                                    return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<ConfParam> data = JsonConvert.DeserializeObject<List<ConfParam>>(datajson);
                    if (data != null && data.Count > 0)
                    {
                        foreach (ConfParam item in data)
                        {
                            ConfParam model = new ConfParam();
                            model.AppId = umodel.AppId;
                            model.Param = item.Param;
                            model.Value = item.Value;
                            model.State = 0;
                            model.UpdateTime = DateTime.Now;
                            model.AddTime = DateTime.Now;
                            model.RId = id;
                            if (Convert.ToInt32(ConfParamBLL.SingleModel.Add(model)) <= 0)
                            {
                                return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = "系统繁忙" + ex.Message }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { isok = 1, msg = "保存成功" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveAppInfo(int aid = 0, string appid = "", string appsr = "",string mcid="",string mckey="",int needPay=0,string name="",int pageType=0)
        {
            Return_Msg returnData = new Return_Msg();
            if (dzaccount == null)
            {
                return Redirect("~/dzhome/login");
            }
            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (umodel == null)
            {
                returnData.Msg = "您还没授权";
                return Json(returnData);
            }

            if (XcxAppAccountRelationBLL.SingleModel.ExitModelByAppIdAndId(aid, appid))
            {
                returnData.Msg = "该appid已绑定其他小程序，请先解绑";
                return Json(returnData);
            }

            umodel.AppId = appid;
            XcxAppAccountRelationBLL.SingleModel.Update(umodel);

            if (!string.IsNullOrEmpty(appid))
            {
                if (UserXcxTemplateBLL.SingleModel.ExitName(name, appid))
                {
                    returnData.Msg = "该名称已被占用，请用别的名称";
                    return Json(returnData);
                }
                UserXcxTemplate userxcxModel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(appid);
                if (userxcxModel != null)
                {
                    userxcxModel.Name = name;
                    userxcxModel.Appsr = appsr;
                    UserXcxTemplateBLL.SingleModel.Update(userxcxModel);
                }
                else
                {
                    userxcxModel = new UserXcxTemplate();
                    userxcxModel.Name = name;
                    userxcxModel.AppId = appid.Trim();
                    userxcxModel.Appsr = appsr.Trim();
                    userxcxModel.AddTime = DateTime.Now;
                    userxcxModel.UpdateTime = DateTime.Now;
                    userxcxModel.TId = umodel.TId;
                    UserXcxTemplateBLL.SingleModel.Add(userxcxModel);
                }

                OpenAuthorizerConfig openConfig = OpenAuthorizerConfigBLL.SingleModel.GetModelByAid(umodel.Id);
                //判断是否权限表跟授权时对不上
                if (openConfig != null && openConfig.appid != appid)
                {
                    openConfig.appid = appid;
                    OpenAuthorizerConfigBLL.SingleModel.Update(openConfig, "appid");
                }

                //清除企业模板绑定的appid
                MiniappBLL.SingleModel.UpdateModelAppId(appid, umodel.Id, pageType);

                if (needPay==1)
                {
                    #region 小程序商户支付配置
                    PayCenterSetting paycentersetting = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(appid);
                    if (paycentersetting == null)
                    {
                        paycentersetting = new PayCenterSetting();
                    }
                    paycentersetting.Appid = appid.Trim();
                    paycentersetting.Mch_id = mcid.Trim();
                    paycentersetting.Key = mckey.Trim();
                    paycentersetting.BindingType = 5;
                    paycentersetting.Status = 0;

                    if (paycentersetting.Id > 0)
                    {
                        if(!PayCenterSettingBLL.SingleModel.Update(paycentersetting))
                        {
                            returnData.Msg = "保存商户号失败";
                            return Json(returnData);
                        }
                    }
                    else
                    {
                        PayCenterSettingBLL.SingleModel.Add(paycentersetting);
                    }
                    #endregion
                }
            }

            returnData.isok = true;
            returnData.Msg = "保存成功";
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeAuthoAppType(int aid = 0,int authoType=0)
        {
            Return_Msg returnData = new Return_Msg();
            if (dzaccount == null)
            {
                return Redirect("~/dzhome/login");
            }
            XcxAppAccountRelation model = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (model == null && model.AccountId!=dzaccount.Id)
            {
                returnData.Msg = "您还没授权";
                return Json(returnData);
            }

            model.AuthoAppType = authoType;
            XcxAppAccountRelationBLL.SingleModel.Update(model);

            //if(!string.IsNullOrEmpty(model.AppId))
            //{
            //    OpenAuthorizerConfig openConfig = _openauthorizerconfigBll.GetModelByAppids(model.AppId);
            //    if (openConfig != null)
            //    {
            //        if (_openauthorizerconfigBll.Delete(openConfig.id) <= 0)
            //        {
            //            returnData.Msg = "解除绑定失败";
            //            return Json(returnData);
            //        }
            //    }
            //}
            
            returnData.isok = true;
            returnData.Msg = "保存成功";
            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
        
        #endregion

        #region 小程序授权新

        /// <summary>
        /// 获取授权链接
        /// </summary>
        /// <param name="Id"></param>
        public ActionResult Getauthorierurl()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            int type = Context.GetRequestInt("type", 0);
            string hosturl = Context.GetRequest("hosturl", string.Empty);
            if (dzaccount == null)
            {
                return View("PageError", new Return_Msg() { Msg = "登陆过期，请重新登陆!", code = "500" });
            }
            string returnurl = hosturl;
            string pageurl = "";
            switch (type)
            {
                case (int)TmpType.小未平台: pageurl = $"/plat/admin/Index"; break;
                case (int)TmpType.小未平台子模版: pageurl = $"/PlatChild/Admin/index"; break;
                case (int)TmpType.智慧餐厅: pageurl = $"/dish/admin"; break;
                case (int)TmpType.企业智推版: pageurl = $"/Qiye/Admin/Index"; break;
                default: pageurl = $"/config/MiniAppConfig"; break;
            }
            returnurl += $"{pageurl}?appId={appId}&Id={Id}&type={type}";

            string AuthodUrl = $"{WebSiteConfig.GoToGetAuthoUrl}index?userId={dzaccount.Id.ToString()}&returnurl={AsUrlData(returnurl)}&rid={appId}";

            return Json(new { url = AuthodUrl }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult MiniAppConfig()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            int type = Context.GetRequestInt("type", 0);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);

            if (dzaccount == null)
            {
                return Redirect("dzhome/login");
            }

            if (appId <= 0 && Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (appId <= 0)
            {
                appId = Id;
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxrelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没开通该模板!", code = "403" });
            }

            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            ConfigViewModel viewModel = new ConfigViewModel();
            viewModel.XcxRelationModel = xcxrelation;
            //小程序模板
            viewModel.XcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (viewModel.XcxTemplate == null)
            {
                return View("PageError", new Return_Msg() { Msg = "找不到模板!", code = "500" });
            }
            int versionId = 0;
            if (viewModel.XcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcxrelation.VersionId;
            }

            //手动上传代码
            if (xcxrelation.AuthoAppType == 0)
            {
                return Redirect($"/config/PersonMiniAppConfig?appId={appId}&Id={Id}&type={type}&SouceFrom={souceFrom}");
            }

            CertInstall certInstallModel = CertInstallBLL.SingleModel.GetModelByAidAndAppId(xcxrelation.Id, xcxrelation.AppId);

            ViewBag.SouceFrom = souceFrom;
            ViewBag.PageType = type;
            ViewBag.appId = appId;
            ViewBag.rappId = appId;
            ViewBag.Id = appId;

            viewModel.CerInstallInfo = certInstallModel;
            List<object> imgurl = new List<object>();
            ViewBag.LogoImgList = imgurl;
            ViewBag.LogoImg = "";

            try
            {
                #region 获取底部logo图
                ConfParam imginfo = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", xcxrelation.AppId);
                if (imginfo != null)
                {
                    imgurl.Add(new { id = imginfo.Id, url = imginfo.Value });
                    ViewBag.LogoImg = imginfo.Value;
                    ViewBag.LogoImgList = imgurl;
                }
                #endregion

                PlatStatisticalFlowConfigBLL.SingleModel.AddAppConfig(xcxrelation.Id, xcxrelation.AppId, viewModel.XcxTemplate.Type);
                ViewBag.versionId = versionId;
                OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                viewModel.Openconfig = XUserList;
                ViewBag.isshouquan = 0;
                ViewBag.PageType = viewModel.XcxTemplate.Type;

                if (XUserList != null)
                {
                    ViewBag.isshouquan = 1;
                    //小程序上传记录
                    viewModel.UserXcxTemplate = UserXcxTemplateBLL.SingleModel.GetModelByAppId(XUserList.appid);
                    if (viewModel.UserXcxTemplate != null)
                    {
                        XcxTemplate tempmodel = XcxTemplateBLL.SingleModel.GetModel(viewModel.UserXcxTemplate.TId);
                        if (tempmodel != null)
                        {
                            viewModel.UserXcxTemplate.TName = tempmodel.TName;
                        }
                    }
                }

                //商户账号信息
                PayCenterSetting paycenter = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(xcxrelation.AppId);
                viewModel.paycenter = paycenter;

                //小程序二维码
                string token = "";
                if (XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
                {

                    qrcodeclass result = CommondHelper.GetMiniAppQrcode(token, viewModel.XcxTemplate.Address);
                    if (result != null)
                    {
                        if (result.isok > 0)
                        {
                            viewModel.miniappqrcode = result.url;
                        }
                        else
                        {
                            viewModel.miniappqrcode = "";
                        }
                    }
                }

                //清除重复的appid
                MiniappBLL.SingleModel.UpdateModelAppid(xcxrelation.AppId, XUserList != null ? XUserList.RId : 0, type);
            }
            catch (Exception ex)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!" + ex.Message, code = "500" });
            }
            return View(viewModel);
        }

        public ActionResult PersonMiniAppConfig()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            int type = Context.GetRequestInt("type", 0);
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);

            if (dzaccount == null)
            {
                return Redirect("dzhome/login");
            }

            if (appId <= 0 && Id <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            if (appId <= 0)
            {
                appId = Id;
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxrelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没开通该模板!", code = "403" });
            }

            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            //手动上传代码
            if (xcxrelation.AuthoAppType == 1)
            {
                return Redirect($"/config/MiniAppConfig?appId={appId}&Id={Id}&type={type}&SouceFrom={souceFrom}");
            }

            ViewBag.SouceFrom = souceFrom;
            ViewBag.PageType = type;
            ViewBag.appId = appId;
            ViewBag.rappId = appId;
            ViewBag.Id = appId;
            ViewBag.versionId = 0;

            ConfigViewModel viewmodel = new ConfigViewModel();
            List<object> imgurl = new List<object>();
            ViewBag.LogoImgList = imgurl;
            ViewBag.LogoImg = "";

            //小程序模板
            viewmodel.XcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            if (viewmodel.XcxTemplate == null)
            {
                return View("PageError", new Return_Msg() { Msg = "找不到模板!", code = "500" });
            }
            ViewBag.PageType = viewmodel.XcxTemplate.Type;
            int versionId = 0;
            if (viewmodel.XcxTemplate.Type == (int)TmpType.小程序专业模板)
            {
                versionId = xcxrelation.VersionId;
            }
            ViewBag.versionId = versionId;

            try
            {
                if (!string.IsNullOrEmpty(xcxrelation.AppId))
                {
                    //证书
                    CertInstall certInstallModel = CertInstallBLL.SingleModel.GetModelByAidAndAppId(xcxrelation.Id, xcxrelation.AppId);
                    viewmodel.CerInstallInfo = certInstallModel;

                    //商户账号信息
                    PayCenterSetting paycenter = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(xcxrelation.AppId);
                    if (paycenter == null)
                        paycenter = new PayCenterSetting();
                    viewmodel.paycenter = paycenter;

                    #region 获取底部logo图
                    ConfParam imginfo = ConfParamBLL.SingleModel.GetModelByParamappid("logoimg", xcxrelation.AppId);
                    if (imginfo != null)
                    {
                        imgurl.Add(new { id = imginfo.Id, url = imginfo.Value });
                        ViewBag.LogoImg = imginfo.Value;
                        ViewBag.LogoImgList = imgurl;
                    }
                    #endregion

                    PlatStatisticalFlowConfigBLL.SingleModel.AddAppConfig(xcxrelation.Id, xcxrelation.AppId, viewmodel.XcxTemplate.Type);

                    //小程序上传记录
                    viewmodel.UserXcxTemplate = UserXcxTemplateBLL.SingleModel.GetModelByAppId(xcxrelation.AppId);
                    if (viewmodel.UserXcxTemplate != null)
                    {
                        XcxTemplate tempmodel = XcxTemplateBLL.SingleModel.GetModel(viewmodel.UserXcxTemplate.TId);
                        if (tempmodel != null)
                        {
                            viewmodel.UserXcxTemplate.TName = tempmodel.TName;
                        }
                        if(string.IsNullOrEmpty(viewmodel.UserXcxTemplate.Name))
                        {
                            OpenAuthorizerConfig openConfig = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(viewmodel.UserXcxTemplate.AppId);
                            if(openConfig!=null && !string.IsNullOrEmpty(openConfig.nick_name))
                            {
                                viewmodel.UserXcxTemplate.Name = openConfig.nick_name;
                                UserXcxTemplateBLL.SingleModel.Update(viewmodel.UserXcxTemplate,"name");
                            }
                        }
                    }
                    else {
                        UserXcxTemplate userxcxModel = new UserXcxTemplate();
                        userxcxModel.AppId = xcxrelation.AppId.Trim();
                        userxcxModel.AddTime = DateTime.Now;
                        userxcxModel.UpdateTime = DateTime.Now;
                        userxcxModel.TId = xcxrelation.TId;
                        UserXcxTemplateBLL.SingleModel.Add(userxcxModel);
                        viewmodel.UserXcxTemplate = userxcxModel;
                    }

                    //小程序二维码
                    string token = "";
                    if (XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
                    {
                        
                        qrcodeclass result = CommondHelper.GetMiniAppQrcode(token, viewmodel.XcxTemplate.Address);

                        viewmodel.miniappqrcode = "";
                        if (result != null && result.isok > 0)
                        {
                            viewmodel.miniappqrcode = result.url;
                        }
                    }
                }
                else
                {
                    viewmodel.CerInstallInfo = new CertInstall();
                    viewmodel.paycenter = new PayCenterSetting();
                    viewmodel.UserXcxTemplate = new UserXcxTemplate();
                }
            }
            catch (Exception ex)
            {
                return View("PageError", new Return_Msg() { Msg = "系统繁忙!" + ex.Message, code = "500" });
            }
            return View(viewmodel);
        }

        private string AsUrlData(string data)
        {
            return Uri.EscapeDataString(data);
        }

        /// <summary>
        /// 解绑体验者
        /// </summary>
        /// <param name="tester"></param>
        /// <returns></returns>
        public ActionResult UnBindWxTester(int Id, string tester)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    string url = XcxApiBLL.SingleModel.UnSettester(model.user_name, tester);
                    
                    string result = HttpHelper.GetData(url);
                    XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                    if (data.isok <= 0)
                    {
                        return Json(new { isok = data.isok, msg = data.msg }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { isok = 1, msg = "解除成功" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "解除失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 设置体验者
        /// </summary>
        /// <param name="tester"></param>
        /// <returns></returns>
        public ActionResult SetWxTester(int Id, string tester)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }

                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    string[] testers = tester.Split(' ');
                    if (tester != null && testers.Length > 0)
                    {
                        foreach (string item in testers)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                string url = XcxApiBLL.SingleModel.Settester(model.user_name, tester);
                                string result = HttpHelper.GetData(url);
                                XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                                if (data.isok <= 0)
                                {
                                    return Json(new { isok = data.isok, msg = data.msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }

                    return Json(new { isok = 1, msg = "设置成功" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "设置失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取体验者列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMemberAuth()
        {
            int Id = Context.GetRequestInt("Id", 0);
            Return_Msg rdata = new Return_Msg();
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(Id);
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    string url = XcxApiBLL.SingleModel.Memberauth(model.user_name);
                    string result = HttpHelper.GetData(url);
                    if (result == null)
                    {
                        rdata.Msg = "";
                        return Json(rdata);
                    }
                    rdata.Msg = result;
                    return Json(rdata);
                }
                else
                {
                    rdata.Msg = "还没授权";
                    return Json(rdata);
                }
            }
            catch (Exception ex)
            {
                rdata.Msg = ex.Message;
                return Json(rdata);
            }
        }

        /// <summary>
        /// 设置默认服务器地址
        /// </summary>
        private void SetServerHost(int Id)
        {
            string requesthost = WebSiteConfig.requesthost;
            string uploadFilehost = WebSiteConfig.uploadFilehost;
            string downloadFilehost = WebSiteConfig.downloadFilehost;
            string sokethost = WebSiteConfig.sokethost;
            string webviewHost = WebSiteConfig.WebviewHost;
            UpdateServerHost(Id, requesthost, sokethost, uploadFilehost, downloadFilehost, webviewHost);
        }
        
        /// <summary>
        /// 小程序服务器地址管理
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <param name="tester">微信图</param>
        /// <returns></returns>
        public ActionResult UpdateServerHost(int Id, string requesthost, string sockethost, string uploadFilehost, string downloadFilehost,string webviewHost)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }

                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                List<string> requesthostlist = new List<string>();
                if (!string.IsNullOrEmpty(requesthost))
                {
                    requesthostlist = new List<string>(requesthost.Split(','));
                }
                List<string> sockethostlist = new List<string>();
                if (!string.IsNullOrEmpty(sockethost))
                {
                    sockethostlist = new List<string>(sockethost.Split(','));
                }
                List<string> uploadFilehostlist = new List<string>();
                if (!string.IsNullOrEmpty(uploadFilehost))
                {
                    uploadFilehostlist = new List<string>(uploadFilehost.Split(','));
                }
                List<string> downloadFilehostlist = new List<string>();
                if (!string.IsNullOrEmpty(downloadFilehost))
                {
                    downloadFilehostlist = new List<string>(downloadFilehost.Split(','));
                }
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    var data = new
                    {
                        action = "set",
                        requestdomain = requesthostlist,
                        wsrequestdomain = sockethostlist,
                        uploaddomain = uploadFilehostlist,
                        downloaddomain = downloadFilehostlist,
                    };
                    string url = XcxApiBLL.SingleModel.UpdateServerHost(model.user_name, data);
                    string result = HttpHelper.GetData(url);
                    //设置业务域名
                    string urlWebview = XcxApiBLL.SingleModel.UpdateWebviewDomain(model.user_name, "add", webviewHost);
                    string webresult = HttpHelper.GetData(urlWebview);
                    XcxApiRequestJson<object> re = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);

                    return Json(new { isok = re.isok, msg = re.msg, data = data, webresult = webresult }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "设置失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// 获取小程序服务器地址
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <param name="tester">微信图</param>
        /// <returns></returns>
        public ActionResult GetServerHostAddress(int Id)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    var data = new
                    {
                        action = "get"
                    };
                    string url = XcxApiBLL.SingleModel.UpdateServerHost(model.user_name, data);
                    string result = HttpHelper.GetData(url);
                    XcxApiRequestJson<object> re = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);

                    return Json(new { isok = re.isok, msg = re.msg, obj = result, data = re }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, msg = "设置失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 保存小程序基本信息
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <param name="appstr">小程序秘钥</param>
        /// <param name="uid">userxcxtemplate表id</param>
        /// <returns></returns>
        public ActionResult SaveMiniappBaseInfo(int Id, string appstr, int uid, string mc_id, string mc_key, int needpay = 1)
        {
            try
            {
                if (dzaccount == null)
                {
                    return Json(new { isok = -1, msg = "非法请求" }, JsonRequestBehavior.AllowGet);
                }
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
                }

                if (needpay == 1)
                {
                    if (string.IsNullOrEmpty(mc_id))
                    {
                        return Json(new { isok = -1, msg = "小程序商户号不能为空" }, JsonRequestBehavior.AllowGet);
                    }
                    if (string.IsNullOrEmpty(mc_key))
                    {
                        return Json(new { isok = -1, msg = "商户号秘钥不能为空" }, JsonRequestBehavior.AllowGet);
                    }
                }


                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                if(string.IsNullOrEmpty(xcxrelation.AppId))
                {
                    return Json(new { isok = -1, msg = "请先保存小程序appid" }, JsonRequestBehavior.AllowGet);
                }
                UserXcxTemplate xcxmodel = UserXcxTemplateBLL.SingleModel.GetModel(uid);
                if (xcxmodel == null)
                {
                    return Json(new { isok = -1, msg = "保存失败，没有上传记录" }, JsonRequestBehavior.AllowGet);
                }
                if (!UserXcxTemplateBLL.SingleModel.Update(xcxmodel))
                {
                    return Json(new { isok = -1, msg = "保存失败" }, JsonRequestBehavior.AllowGet);
                }
                if (needpay == 1)
                {
                    #region 小程序商户支付配置
                    PayCenterSetting paycentersetting = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(xcxrelation.AppId);
                    if (paycentersetting == null)
                    {
                        paycentersetting = new PayCenterSetting();
                    }
                    paycentersetting.Appid = xcxrelation.AppId.Trim();
                    paycentersetting.Mch_id = mc_id.Trim();
                    paycentersetting.Key = mc_key.Trim();
                    paycentersetting.BindingType = 5;
                    paycentersetting.Status = 0;

                    if (paycentersetting.Id > 0)
                    {
                        if (Convert.ToInt32(PayCenterSettingBLL.SingleModel.Update(paycentersetting)) <= 0)
                        {
                            return Json(new { isok = -1, msg = "修改小程序支付商户配置失败" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(PayCenterSettingBLL.SingleModel.Add(paycentersetting)) <= 0)
                        {
                            return Json(new { isok = -1, msg = "添加小程序支付商户配置失败" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    #endregion
                }

                return Json(new { isok = 1, msg = "保存成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 上传小程序
        /// </summary>
        /// <param name="openConfigId"></param>
        /// <param name="tid"></param>
        /// <param name="rid"></param>
        /// <param name="appsr"></param>
        /// <returns></returns>
        public XcxApiRequestJson<object> UploadCodefunction(int openConfigId, int tid, int rid, string appsr, string mc_id, string mc_key, int needpay = 1, int isshenhe = 1)
        {
            XcxApiRequestJson<object> result = new XcxApiRequestJson<object>();
            if (openConfigId <= 0)
            {
                result.isok = -1;
                result.msg = "请选择小程序账号";
                return result;
            }
            XcxAppAccountRelation rlmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(rid);
            if (rlmodel == null)
            {
                result.isok = -1;
                result.msg = "模板无效";
                return result;
            }
            XcxApiBLL.SingleModel._openType = rlmodel.ThirdOpenType;

            if (needpay == 1)
            {
                if (string.IsNullOrEmpty(mc_id))
                {
                    result.isok = -1;
                    result.msg = "请输入小程序商户号";
                    return result;
                }
                if (string.IsNullOrEmpty(mc_key))
                {
                    result.isok = -1;
                    result.msg = "请输入商户号秘钥";
                    return result;
                }
            }

            XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(tid);
            if (xcxtemplate == null)
            {
                result.isok = -1;
                result.msg = "系统繁忙auth_null";
                return result;
            }
            
            try
            {
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModel(openConfigId);

                if (model != null)
                {
                    #region 上传小程序代码

                    #region 小程序上传记录
                    
                    UserXcxTemplate userxcxtemplate = UserXcxTemplateBLL.SingleModel.GetModelByAppId(model.appid);
                    if (userxcxtemplate == null)
                    {
                        userxcxtemplate = new UserXcxTemplate();
                    }
                    userxcxtemplate.TId = tid;
                    //判断是否需要审核，不需要审核不改变上传记录的提交时间和状态，版本
                    if (isshenhe == 1)
                    {
                        userxcxtemplate.Version = xcxtemplate.Version;
                        userxcxtemplate.State = -1;
                        userxcxtemplate.Reason = "";
                        userxcxtemplate.UpdateTime = DateTime.Now;
                        userxcxtemplate.AddTime = DateTime.Now;
                    }
                    userxcxtemplate.AppId = model.appid;
                    userxcxtemplate.TuserId = model.user_name;
                    string columnstr = "TId,AppId,AreaCode,TuserId,Version,State,UpdateTime,Reason,AddTime";
                    if (userxcxtemplate.Id > 0)
                    {
                        if (!UserXcxTemplateBLL.SingleModel.Update(userxcxtemplate, columnstr))
                        {
                            result.isok = -1;
                            result.msg = "小程序上传记录";
                            return result;
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(UserXcxTemplateBLL.SingleModel.Add(userxcxtemplate)) <= 0)
                        {
                            result.isok = -1;
                            result.msg = "小程序上传记录";
                            return result;
                        }
                    }
                    #endregion
                    if (needpay == 1)
                    {
                        #region 小程序商户支付配置
                        PayCenterSetting paycentersetting = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(model.appid);
                        if (paycentersetting == null)
                        {
                            paycentersetting = new PayCenterSetting();
                        }
                        paycentersetting.Appid = model.appid.Trim();
                        paycentersetting.Mch_id = mc_id.Trim();
                        paycentersetting.Key = mc_key.Trim();
                        paycentersetting.BindingType = 5;
                        paycentersetting.Status = 0;

                        if (paycentersetting.Id > 0)
                        {
                            if (Convert.ToInt32(PayCenterSettingBLL.SingleModel.Update(paycentersetting)) <= 0)
                            {
                                result.isok = -1;
                                result.msg = "修改小程序支付商户配置失败";
                                return result;
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(PayCenterSettingBLL.SingleModel.Add(paycentersetting)) <= 0)
                            {
                                result.isok = -1;
                                result.msg = "添加小程序支付商户配置失败";
                                return result;
                            }
                        }
                        #endregion
                    }
                    OpenAuthorizerConfig openauthorizerconfig = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(model.appid);
                    if (openauthorizerconfig != null)
                    {
                        openauthorizerconfig.RId = rid;
                        OpenAuthorizerConfigBLL.SingleModel.Update(openauthorizerconfig, "RId");
                        rlmodel.AppId = model.appid;
                        XcxAppAccountRelationBLL.SingleModel.Update(rlmodel, "appid");
                    }

                    //提交小程序准备信息
                    var datajson = GetCommitJson(openauthorizerconfig);

                    string url = "";
                    //设置默认地址
                    SetServerHost(rid);
                    if (isshenhe == 1)
                    {
                        if (WebSiteConfig.ServiceShenHe == "1")
                        {
                            userxcxtemplate.State = (int)XcxTypeEnum.待审核;
                            UserXcxTemplateBLL.SingleModel.Update(userxcxtemplate, "state");
                            result.msg = "提交成功，请等待审核";
                            result.isok = 1;
                            return result;
                        }

                        url = XcxApiBLL.SingleModel.Commit(model.user_name, datajson);
                    }
                    else
                    {
                        //提交代码，不审核
                        url = XcxApiBLL.SingleModel.CommitCode(model.user_name, datajson);
                    }
                    string result2 = HttpHelper.GetData(url);
                    XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result2);
                    if (data != null)
                    {
                        if (data.isok == 1)
                        {
                            result.isok = 1;
                            result.msg = isshenhe == 1 ? "已提交审核" : "已提交代码";
                            return result;
                        }
                        else
                        {
                            result.isok = -2;
                            result.msg = data.msg;
                            return result;
                        }
                    }

                    result.isok = -2;
                    result.msg = "提交失败";
                    return result;
                    #endregion
                }
                else
                {
                    result.isok = -2;
                    result.msg = "请重新授权";
                    return result;
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteInfo(typeof(configController), ex.Message);
                result.isok = -1;
                result.msg = "提交失败，请检查您小程序服务类目";
                return result;
            }

        }

        public object GetCommitJson(OpenAuthorizerConfig openauthorizerconfig)
        {
            object datajson = new object();
            if (!WebSiteConfig.UseALaDing)
            {
                //提交小程序准备信息
                datajson = new
                {
                    extEnable = true,
                    extAppid = openauthorizerconfig.appid,
                    ext = new
                    {
                        appid = openauthorizerconfig.appid,
                    }
                };
                return datajson;
            }

            #region 阿拉丁配置
            AlaDingAppInfo alaDingConfig = new AlaDingAppInfo();
            alaDingConfig.AppId = openauthorizerconfig.appid;
            alaDingConfig.Name = openauthorizerconfig.nick_name;
            alaDingConfig.Logo = openauthorizerconfig.head_img;
            string alaDingAppKey = AlaDingAppInfoBLL.SingleModel.RegisterApp(alaDingConfig);
            //提交小程序准备信息
            datajson = new
            {
                extEnable = true,
                extAppid = openauthorizerconfig.appid,
                ext = new
                {
                    appid = openauthorizerconfig.appid,
                    ald_config = new
                    {
                        app_key = alaDingAppKey,
                        getLocation = 0,
                        getUserinfo = 0
                    }
                }
            };
            #endregion

            return datajson;
        }
        
        /// <summary>
        /// 获取体验二维码提交审核
        /// </summary>
        /// <param name="tester"></param>
        /// <returns></returns>
        public ActionResult GettestQrcode_needshenhe(int Id, int tid, string appsr, string mc_id, string mc_key, int needpay = 1)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
                }

                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    XcxTemplate templateinfo = XcxTemplateBLL.SingleModel.GetModel(tid);
                    if (templateinfo == null)
                    {
                        return Json(new { isok = -1, msg = "模板null" }, JsonRequestBehavior.AllowGet);
                    }
                    UserXcxTemplate usertempmodel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(model.appid);
                    if (usertempmodel == null || templateinfo.Version != usertempmodel.Version)
                    {
                        XcxApiRequestJson<object> upresult = UploadCodefunction(model.id, tid, Id, appsr, mc_id, mc_key, needpay);
                        if (upresult.isok == 1)
                        {
                            string url = XcxApiBLL.SingleModel.GetQrcode(model.user_name);
                            string result = HttpHelper.GetData(url);
                            XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                            return Json(new { isok = data.isok, msg = data.msg, imgdata = data.src }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { isok = upresult.isok, msg = upresult.msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        string url = XcxApiBLL.SingleModel.GetQrcode(model.user_name);
                        string result = HttpHelper.GetData(url);
                        XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                        return Json(new { isok = data.isok, msg = data.msg, imgdata = data.src }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    return Json(new { isok = -1, msg = "设置失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 获取体验二维码
        /// </summary>
        /// <param name="tester"></param>
        /// <returns></returns>
        public ActionResult GettestQrcode(int Id, int tid, string appsr, string mc_id, string mc_key, int needpay = 1)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
                }

                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    XcxTemplate templateinfo = XcxTemplateBLL.SingleModel.GetModel(tid);
                    if (templateinfo == null)
                    {
                        return Json(new { isok = -1, msg = "模板null" }, JsonRequestBehavior.AllowGet);
                    }
                    XcxApiRequestJson<object> upresult = UploadCodefunction(model.id, tid, Id, appsr, mc_id, mc_key, needpay, 0);
                    if (upresult.isok == 1)
                    {
                        string url = XcxApiBLL.SingleModel.GetQrcode(model.user_name);
                        string result = HttpHelper.GetData(url);
                        XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                        return Json(new { isok = data.isok, msg = data.msg, imgdata = data.src }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { isok = upresult.isok, msg = upresult.msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { isok = -1, msg = "设置失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 撤回审核
        /// 单个帐号每天审核撤回次数最多不超过1次，一个月不超过10次。
        /// </summary>
        /// <param name="tester"></param>
        /// <returns></returns>
        public ActionResult UndocodeAudit()
        {
            Return_Msg data = new Return_Msg();
            int rid = Context.GetRequestInt("rid", 0);
            if (rid <= 0)
            {
                data.Msg = "系统繁忙id";
                return Json(data);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(rid, dzaccount.Id.ToString());
            if (xcxrelation == null)
            {
                data.Msg = "系统繁忙xcxrelation_null";
                return Json(data);
            }
            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;

            OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
            if (model == null)
            {
                data.Msg = "您还没授权";
                return Json(data);
            }

            string url = XcxApiBLL.SingleModel.UndocodeAuditUrl(model.user_name);
            string result = HttpHelper.GetData(url);
            if (string.IsNullOrEmpty(result))
            {
                data.Msg = "撤销审核繁忙";
                return Json(data);
            }
            XcxApiRequestJson<object> resultdata = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);

            if (resultdata != null && resultdata.isok == 1)
            {
                data.Msg = resultdata.msg;
                data.isok = true;
                UserXcxTemplate uploadmodel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(xcxrelation.AppId);
                if (uploadmodel != null)
                {
                    uploadmodel.Auditid = 0;
                    uploadmodel.State = (int)XcxTypeEnum.未发布;
                    uploadmodel.UpdateTime = DateTime.Now;
                    uploadmodel.Reason = "";
                    UserXcxTemplateBLL.SingleModel.Update(uploadmodel);
                }
                return Json(data);
            }

            data.Msg = resultdata.msg;
            return Json(data);
        }

        /// <summary>
        /// 发布小程序
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="tid"></param>
        /// <param name="appsr"></param>
        /// <param name="mc_id"></param>
        /// <param name="mc_key"></param>
        /// <param name="needpay"></param>
        /// <param name="isshenhe"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadCodenew(int Id, int tid, string appsr, string mc_id, string mc_key, int needpay = 1, int isshenhe = 1)
        {
            try
            {
                if (Id <= 0)
                {
                    return View("PageError", new Return_Msg() { Msg = "参数出错!", code = "500" });
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
                if (xcxrelation == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "没开通该模板权限!", code = "403" });
                }
                XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    XcxTemplate templateinfo = XcxTemplateBLL.SingleModel.GetModel(tid);
                    if (templateinfo == null)
                    {
                        return Json(new { isok = -1, msg = "模板null" }, JsonRequestBehavior.AllowGet);
                    }
                    UserXcxTemplate usertemplatemodel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(xcxrelation.AppId);
                    if (usertemplatemodel != null && usertemplatemodel.State == (int)XcxTypeEnum.审核中)
                    {
                        string url = XcxApiBLL.SingleModel.ReleaseCode(model.user_name);
                        string result = HttpHelper.GetData(url);
                        XcxApiRequestJson<object> data = JsonConvert.DeserializeObject<XcxApiRequestJson<object>>(result);
                        if (data.isok == 1 || data.msg == "已发布")
                        {
                            usertemplatemodel.UpdateTime = DateTime.Now;
                            usertemplatemodel.State = (int)XcxTypeEnum.发布成功;
                            usertemplatemodel.Reason = "发布成功";
                            UserXcxTemplateBLL.SingleModel.Update(usertemplatemodel);
                        }
                        else if (data.msg == "正在审核中")
                        {
                            usertemplatemodel.State = (int)XcxTypeEnum.审核中;
                            usertemplatemodel.Reason = data.msg;
                            UserXcxTemplateBLL.SingleModel.Update(usertemplatemodel);
                        }
                        else
                        {
                            usertemplatemodel.UpdateTime = DateTime.Now;
                            usertemplatemodel.State = (int)XcxTypeEnum.发布失败;
                            usertemplatemodel.Reason = data.msg + "，请重新发布";
                            UserXcxTemplateBLL.SingleModel.Update(usertemplatemodel);
                        }
                        return Json(new { isok = data.isok, msg = data.msg, imgdata = data.src }, JsonRequestBehavior.AllowGet);
                    }
                    else if (usertemplatemodel != null && (usertemplatemodel.State == (int)XcxTypeEnum.待审核 || usertemplatemodel.State == (int)XcxTypeEnum.通过审核))
                    {
                        return Json(new { isok = 1, msg = "正在审核中" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        XcxApiRequestJson<object> upresult = UploadCodefunction(model.id, tid, Id, appsr, mc_id, mc_key, needpay, isshenhe);
                        return Json(new { isok = upresult.isok, msg = upresult.msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { isok = -1, msg = "设置失败，还没授权" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { isok = -1, msg = "系统繁忙" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 解绑
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult DelBangding(int Id)
        {
            Return_Msg returndata = new Return_Msg();
            //string dmsg = "";
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }

            if (Id <= 0)
            {
                returndata.Msg = "系统繁忙id";
                return Json(returndata);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(Id, dzaccount.Id.ToString());
            if (xcxrelation == null)
            {
                returndata.Msg = "系统繁忙xcxrelation_null";
                return Json(returndata);
            }

            try
            {
                //判断企业版小程序是否有绑定
                Miniapp gwmodel = MiniappBLL.SingleModel.GetModelByAppId(xcxrelation.AppId, xcxrelation.Id);
                if (gwmodel != null)
                {
                    gwmodel.ModelId = "";
                    MiniappBLL.SingleModel.Update(gwmodel, "ModelId");
                }

                OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
                if (model != null)
                {
                    if (OpenAuthorizerConfigBLL.SingleModel.Delete(model.id) <= 0)
                    {
                        returndata.Msg = "解除绑定失败";
                        return Json(returndata);
                    }
                }
                if (XcxAppAccountRelationBLL.SingleModel.UpdateModelByAppId(xcxrelation.AppId) <= 0)
                {
                    returndata.Msg = "解除绑定失败";
                    return Json(returndata);
                }
                //清除小程序记录
                if (!string.IsNullOrEmpty(xcxrelation.AppId))
                {
                    UserXcxTemplate usermodel = UserXcxTemplateBLL.SingleModel.GetModelByAppId(xcxrelation.AppId);
                    if (usermodel != null)
                    {
                        usermodel.Auditid = 0;
                        usermodel.TId = 0;
                        usermodel.Desc = "";
                        usermodel.Reason = "";
                        usermodel.State = (int)XcxTypeEnum.未发布;
                        usermodel.Version = "";

                        UserXcxTemplateBLL.SingleModel.Update(usermodel);
                    }

                }
            }
            catch (Exception)
            {
                returndata.Msg = "系统繁忙auth_null";
                return Json(returndata);
            }
            returndata.isok = true;
            returndata.Msg = "解除绑定成功";
            return Json(returndata);
        }

        /// <summary>
        /// 获取小程序服务类目
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCategoryTyleList()
        {
            int rid = Context.GetRequestInt("rid", 0);
            if (rid <= 0)
            {
                return Json(new { isok = -1, msg = "系统繁忙id" }, JsonRequestBehavior.AllowGet);
            }
            if (dzaccount == null)
            {
                return Json(new { isok = -1, msg = "登录过期" }, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(rid, dzaccount.Id.ToString());
            if (xcxrelation == null)
            {
                return Json(new { isok = -1, msg = "系统繁忙xcxrelation_null" }, JsonRequestBehavior.AllowGet);
            }

            XcxApiBLL.SingleModel._openType = xcxrelation.ThirdOpenType;
            OpenAuthorizerConfig model = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(xcxrelation.AppId);
            if (model == null)
            {
                return Json(new { isok = -1, msg = "您还没授权" }, JsonRequestBehavior.AllowGet);
            }

            string url = XcxApiBLL.SingleModel.GetCategoryTyleListUrl(model.user_name);
            string result = HttpHelper.GetData(url);
            if (string.IsNullOrEmpty(result))
            {
                return Json(new { isok = -1, msg = "获取服务类目失败" }, JsonRequestBehavior.AllowGet);
            }
            XcxApiRequestJson<List<CategoryList>> data = JsonConvert.DeserializeObject<XcxApiRequestJson<List<CategoryList>>>(result);

            if (data != null)
            {
                if (data.data != null && data.data.Count > 0)
                {
                    return Json(new { isok = data.isok, msg = data.msg, data = data.data }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { isok = data.isok, msg = data.msg }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 检测商户号是否正确
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckMerchanInfo()
        {
            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("aid", 0);
            string mc_id = Context.GetRequest("mc_id", string.Empty);
            string mc_key = Context.GetRequest("mc_key", string.Empty);
            if (aid <= 0)
            {
                returndata.Msg = "参数不能为0";
                return Json(returndata);
            }
            if (string.IsNullOrEmpty(mc_id))
            {
                returndata.Msg = "请输入小程序商户号";
                return Json(returndata);
            }
            if (string.IsNullOrEmpty(mc_key))
            {
                returndata.Msg = "请输入商户号秘钥";
                return Json(returndata);
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null || string.IsNullOrEmpty(xcxrelation.AppId) || xcxrelation.AccountId != dzaccount.Id)
            {
                returndata.Msg = "权限不足或没有绑定小程序";
                return Json(returndata);
            }

            #region 小程序商户支付配置
            PayCenterSetting paycentersetting = PayCenterSettingBLL.SingleModel.GetPayCenterSettingByappid(xcxrelation.AppId);
            if (paycentersetting == null)
            {
                paycentersetting = new PayCenterSetting();
            }
            paycentersetting.Appid = xcxrelation.AppId.Trim();
            paycentersetting.Mch_id = mc_id.Trim();
            paycentersetting.Key = mc_key.Trim();
            paycentersetting.BindingType = 5;
            paycentersetting.Status = 0;

            if (paycentersetting.Id > 0)
            {
                if (Convert.ToInt32(PayCenterSettingBLL.SingleModel.Update(paycentersetting)) <= 0)
                {
                    returndata.Msg = "修改小程序支付商户配置失败";
                    return Json(returndata);
                }
            }
            else
            {
                if (Convert.ToInt32(PayCenterSettingBLL.SingleModel.Add(paycentersetting)) <= 0)
                {
                    returndata.Msg = "添加小程序支付商户配置失败";
                    return Json(returndata);
                }
            }
            #endregion
            try
            {
                C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModelByAppId(xcxrelation.AppId);
                if (userinfo.OpenId == null || userinfo.OpenId.Length <= 0)
                {
                    returndata.Msg = "请扫描进入小程序，授权成功后再试试";
                    return Json(returndata);
                }
                string no = WxPayApi.GenerateOutTradeNo();
                CityMorders order = new CityMorders()
                {
                    trade_no = no,
                    orderno = no,
                    ShowNote = "",
                    OrderType = 0,
                    payment_free = 1,
                };
                JsApiPay jsApiPay = new JsApiPay(HttpContext)
                {
                    total_fee = order.payment_free,
                    openid = userinfo.OpenId
                };

                string msg = "";
                //统一下单，获得预支付码
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(paycentersetting, order, WebConfigBLL.citynotify_url, ref msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    returndata.Msg = msg;
                    return Json(returndata);
                }
                jsApiPay.GetJsApiParametersnew(paycentersetting);
                returndata.Msg = "商户号配置正确";
            }
            catch (Exception ex)
            {
                returndata.Msg = ex.Message;
            }

            return Json(returndata);
        }
        #endregion

        #region 新版小程序管理
        [RouteAuthCheck]
        public ActionResult FunctionList()
        {
            int appId = Context.GetRequestInt("appId", 0);
            int type = Context.GetRequestInt("type", 0);
            if (dzaccount == null)
            {
                return Redirect("dzhome/login");
            }

            if (appId <= 0)
            {
                return View("PageError", new Return_Msg() { Msg = "参数错误!", code = "500" });
            }
            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (xcxrelation == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没开通该模板!", code = "403" });
            }

            ViewBag.layId = "";
            CustomModelUserRelation crelation = CustomModelUserRelationBLL.SingleModel.GetModelByAId(xcxrelation.Id);
            //默认选择从零开始装修模板
            if (crelation != null || type == 26 || type == 12)
            {
                ViewBag.layId = "PersonRenovation";
            }
            ViewBag.versionId = xcxrelation.VersionId;
            ViewBag.appId = appId;
            ViewBag.Id = appId;
            ViewBag.PageType = type;
            return View();
        }
        public ActionResult ChooseModelList()
        {
            int appId = Context.GetRequestInt("appId", 0);

            ViewBag.appId = appId;

            return View();
        }
        public ActionResult GetCustomModelList()
        {
            Return_Msg returndata = new Return_Msg();
            try
            {
                int appId = Context.GetRequestInt("appId", 0);
                int pageSize = Context.GetRequestInt("pageSize", 20);
                int pageIndex = Context.GetRequestInt("pageIndex", 1);
                if (dzaccount == null)
                {
                    return Redirect("dzhome/login");
                }
                if (appId <= 0)
                {
                    returndata.Msg = "参数不能为0";
                    return Json(returndata);
                }
                XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(appId);
                if (xcxrelation == null || xcxrelation.AccountId != dzaccount.Id)
                {
                    return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "400" });
                }
                #region 判断是否是代理商
                int agentid = 0;
                Agentinfo agentmodel = AgentinfoBLL.SingleModel.GetModelByAccoundId(dzaccount.Id.ToString());
                if (agentmodel != null && agentmodel.id == xcxrelation.agentId)
                {
                    agentid = agentmodel.id;
                }
                //判断是否是代理登陆
                else if (agentinfo != null && agentinfo.id == xcxrelation.agentId)
                {
                    agentid = agentinfo.id;
                }
                #endregion

                CustomModelUserRelation crelation = CustomModelUserRelationBLL.SingleModel.GetModelByAId(xcxrelation.Id);
                //默认选择从零开始装修模板
                if (crelation == null)
                {
                    CustomModelRelation tempmodel = CustomModelRelationBLL.SingleModel.GetModelByDataType();
                    if (tempmodel != null)
                    {
                        crelation = new CustomModelUserRelation();
                        crelation.UpdateTime = DateTime.Now;
                        crelation.PreviousId = 0;
                        crelation.AddTime = DateTime.Now;
                        crelation.AId = xcxrelation.Id;
                        crelation.CustomModelId = tempmodel.Id;
                        crelation.Id = Convert.ToInt32(CustomModelUserRelationBLL.SingleModel.Add(crelation));
                        CustomModelRelationBLL.SingleModel.RemoveCache();
                    }
                }

                int count = 0;
                ViewModel<CustomModelRelation> viewmodel = new ViewModel<CustomModelRelation>();
                viewmodel.DataList = CustomModelRelationBLL.SingleModel.GetCustomModelRelationList(xcxrelation.Id, pageSize, pageIndex, ref count);
                viewmodel.TotalCount = count;
                viewmodel.PageIndex = pageIndex;
                viewmodel.PageSize = pageSize;
                viewmodel.VersionId = xcxrelation.VersionId;
                viewmodel.AgentId = agentid;
                viewmodel.AId = xcxrelation.Id;
                viewmodel.ModelTemplateId = crelation != null ? crelation.CustomModelId : 0;
                returndata.isok = true;
                returndata.dataObj = viewmodel;
            }
            catch (Exception ex)
            {
                returndata.Msg = ex.Message;
            }

            return Json(returndata);
        }
        public ActionResult UpGrade()
        {
            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("AId", 0);
            int agentId = Context.GetRequestInt("AgentId", 0);
            CustomModelRelationBLL.SingleModel.SaveUpGradeAid(aid, agentId);
            return Json(returndata);
        }

        /// <summary>
        /// 复制装修模板数据
        /// </summary>
        /// <returns></returns>
        public ActionResult CopyTemplate()
        {
            Return_Msg returndata = new Return_Msg();
            int appId = Context.GetRequestInt("appId", 0);
            int templateid = Context.GetRequestInt("templateid", 0);
            if (appId <= 0)
            {
                returndata.Msg = "参数不能为0";
                return Json(returndata);
            }
            if (templateid <= 0)
            {
                returndata.Msg = "模板ID不能为0";
                return Json(returndata);
            }
            XcxAppAccountRelation relation = XcxAppAccountRelationBLL.SingleModel.GetModelById(appId);
            if (relation == null || relation.AccountId != dzaccount.Id)
            {
                return View("PageError", new Return_Msg() { Msg = "权限不足!", code = "400" });
            }
            EntSetting pageSetting = EntSettingBLL.SingleModel.GetModel(relation.Id);
            if (pageSetting == null)
            {
                returndata.Msg = "小程序编辑页数据不能为空";
                return Json(returndata);
            }

            CustomModelRelation customrelation = CustomModelRelationBLL.SingleModel.GetModel(templateid);
            if (customrelation == null)
            {
                returndata.Msg = "装修模板已下架，请刷新重试！";
                return Json(returndata);
            }
            XcxAppAccountRelation trelation = XcxAppAccountRelationBLL.SingleModel.GetModel(customrelation.AId);
            if (trelation == null)
            {
                returndata.Msg = "模板数据不能为空";
                return Json(returndata);
            }

            returndata.isok = true;
            returndata.dataObj = customrelation.AId;
            return Json(returndata);

            //EntSetting templateSetting = EntSettingBLL.SingleModel.GetModel(trelation.Id);
            //if (templateSetting == null)
            //{
            //    returndata.Msg = "装修数据为空，请刷新重试！";
            //    return Json(returndata);
            //}


            ////保存旧的配置数据
            //CustomModelEntSetting oldentsetting = new CustomModelEntSetting();
            //oldentsetting.AddTime = DateTime.Now;
            //oldentsetting.AId = pageSetting.aid;
            //oldentsetting.ConfigJson = pageSetting.configJson;
            //oldentsetting.Pages = pageSetting.pages;
            //oldentsetting.Syncmainsite = pageSetting.syncmainsite;
            //oldentsetting.UpdateTime = DateTime.Now;
            //oldentsetting.Id =Convert.ToInt32(_customModelEntSettingBLL.Add(oldentsetting));

            //CustomModelUserRelation crelation = _customModelUserRelationBLL.GetModelByAId(relation.Id);
            //if (crelation == null)
            //{
            //    crelation = new CustomModelUserRelation();
            //    crelation.AId = relation.Id;
            //    crelation.PreviousId = 0;
            //    crelation.AddTime = DateTime.Now;
            //    crelation.CustomModelId = customrelation.Id;
            //    crelation.UpdateTime = DateTime.Now;
            //    _customModelUserRelationBLL.Add(crelation);
            //}
            //else
            //{
            //    crelation.SettingId = oldentsetting.Id;
            //    crelation.PreviousId = crelation.CustomModelId;
            //    crelation.CustomModelId = customrelation.Id;
            //    crelation.UpdateTime = DateTime.Now;
            //    _customModelUserRelationBLL.Update(crelation, "PreviousId,CustomModelId,UpdateTime");
            //}
            //_customModelRelationBLL.RemoveCache();

            //pageSetting.pages = templateSetting.pages;
            //pageSetting.configJson = templateSetting.configJson;
            //pageSetting.syncmainsite = templateSetting.syncmainsite;
            //pageSetting.updatetime = DateTime.Now;


            //returndata.isok = EntSettingBLL.SingleModel.Update(pageSetting, "pages,configJson,syncmainsite,updatetime");
            //returndata.Msg = returndata.isok ? "复制成功" : "复制失败";

            //return Json(returndata);
        }

        /// <summary>
        /// 上传安装证书
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="name"></param>
        /// <param name="mc_id"></param>
        /// <returns></returns>
        public ActionResult AddAutoInstallCert(int aid = 0, string name = "", string mc_id = "")
        {
            Return_Msg returnData = new Return_Msg();
            XcxAppAccountRelation xcxrelationModel = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelationModel == null || string.IsNullOrEmpty(xcxrelationModel.AppId))
            {
                returnData.Msg = "请先授权";
                return Json(returnData);
            }

            CertInstall model = CertInstallBLL.SingleModel.GetModelByAidAndAppId(aid, xcxrelationModel.AppId);
            if (model == null)
            {
                model = new CertInstall();
            }

            model.UpdateTime = DateTime.Now;
            model.State = 0;
            model.Name = name;
            model.Password = mc_id;
            model.ErrorMsg = "";
            model.Mc_Id = mc_id;
            model.ProjectType = (int)ProjectType.小程序;
            model.AppId = xcxrelationModel.AppId;
            if (model.Id <= 0)
            {
                model.Aid = aid;
                model.AddTime = DateTime.Now;
                CertInstallBLL.SingleModel.Add(model);
            }
            else
            {
                CertInstallBLL.SingleModel.Update(model);
            }

            returnData.isok = true;
            return Json(returnData);
        }
        #endregion

        public ActionResult BackTemplate()
        {
            Return_Msg returndata = new Return_Msg();
            int aid = Context.GetRequestInt("AId", 0);

            if (dzaccount == null)
            {
                return Redirect("dzhome/login");
            }

            XcxAppAccountRelation xcxrelation = XcxAppAccountRelationBLL.SingleModel.GetModel(aid);
            if (xcxrelation == null)
            {
                returndata.Msg = "权限已过期，请刷新重试！";
                return Json(returndata);
            }
            if (xcxrelation.AccountId != dzaccount.Id)
            {
                returndata.Msg = "权限已失效，请刷新重试！";
                return Json(returndata);
            }

            CustomModelUserRelation crelation = CustomModelUserRelationBLL.SingleModel.GetModelByAId(aid);
            if (crelation == null)
            {
                returndata.Msg = "装修数据为空，请刷新重试！";
                return Json(returndata);
            }

            if (crelation.SettingId > 0)
            {
                CustomModelEntSetting customModelEntSetting = CustomModelEntSettingBLL.SingleModel.GetModel(crelation.SettingId);
                if (customModelEntSetting == null)
                {
                    returndata.Msg = "装修数据为空，请刷新重试！";
                    return Json(returndata);
                }

                EntSetting pageSetting = EntSettingBLL.SingleModel.GetModel(aid);
                if (pageSetting == null)
                {
                    returndata.Msg = "小程序编辑页数据不能为空";
                    return Json(returndata);
                }
                pageSetting.pages = customModelEntSetting.Pages;
                pageSetting.configJson = customModelEntSetting.ConfigJson;
                pageSetting.syncmainsite = customModelEntSetting.Syncmainsite;
                pageSetting.updatetime = DateTime.Now;
                EntSettingBLL.SingleModel.Update(pageSetting, "pages,configJson,syncmainsite,updatetime");

                crelation.CustomModelId = crelation.PreviousId;
                crelation.UpdateTime = DateTime.Now;
                CustomModelUserRelationBLL.SingleModel.Update(crelation, "PreviousId,CustomModelId,UpdateTime");

                returndata.isok = true;
            }

            returndata.Msg = returndata.isok ? "成功" : "暂无还原数据";
            return Json(returndata);
        }
    }
}