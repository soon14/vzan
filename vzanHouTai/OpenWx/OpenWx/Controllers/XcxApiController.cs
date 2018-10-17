using BLL.OpenWx;
using Core.OpenWx;
using DAL.Base;
using Entity.OpenWx;
using Newtonsoft.Json;
using OpenWx.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace OpenWx.Controllers
{
    public class XcxApiController : BaseController
    {
        private static OpenAuthorizerConfigBLL _openAuthorizerConfigBLL = OpenAuthorizerConfigBLL.SingleModel;
        private static OpenAuthorizerInfoBLL _openAuthorizerInfoBLL = OpenAuthorizerInfoBLL.SingleModel;

        public XcxApiController()
        {

        }

        #region 小程序跳转授权
        /// <summary>
        /// 获取跳转到授权页链接
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult getOpenImg(string userId, string returnurl, string rid = "", string cityinfoid = "", int newtype = 0, int citytype = 0, int storeid = 0)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { isok = -1, src = "", msg = "userId不能为空" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                //opencomponentconfig model = _opencomponentconfigBLL.getCurrentModel();
                OpenPlatConfig model = OpenPlatConfigBLL.SingleModel.getCurrentModel();
                if (model == null)
                {
                    return Json(new { isok = -1, src = "", msg = "获取不到第三方平台信息" }, JsonRequestBehavior.AllowGet);
                }

                var pre_token = WxRequest.GetPreAuthCode(model.component_Appid, model.component_access_token);
                if (pre_token != null && !string.IsNullOrEmpty(pre_token.pre_auth_code))
                {
                    string callbackurl = $"{WxRequest._component_Host}/XcxApi/OpenOAuthCallback?userId={userId}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}&citytype={citytype}&storeid={storeid}&returnurl={WxRequest.AsUrlData(returnurl)}";
                    string url = WxRequest.ReturnUrl(model.component_Appid, pre_token.pre_auth_code, callbackurl);
                    return Json(new { isok = 1, src = url, msg = "成功" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isok = -1, src = "", msg = "获取不到预授权码" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new { isok = -1, src = "", msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Index(string userId, string returnurl, int rid = 0, int cityinfoid = 0, int newtype = 0, int citytype = 0, int storeid = 0)
        {
            if (Request == null || Request.UrlReferrer == null)
            {
                return Content($"系统繁忙auto_null!<a href=\"{returnurl}\">返回</a>");
            }
            string lastUrl = Request.UrlReferrer.Host == null ? "" : Request.UrlReferrer.Host.ToString();
            
            if (newtype > 0)
            {
                if (rid <= 0)
                {
                    return Content($"系统繁忙，参数rid不能小于0!<a href=\"{returnurl}\">返回</a>");
                }
            }
            ViewBag.userId = userId;
            //小程序对应的权限表 
            ViewBag.rid = rid;
            //小程序属于哪个项目。0代表同城
            ViewBag.newtype = newtype;
            ViewBag.cityinfoid = cityinfoid;
            ViewBag.storeid = storeid;
            ViewBag.returnurl = WxRequest.AsUrlData(returnurl);
            //区分小程序类型
            ViewBag.citytype = citytype;
            return View();
        }

        /// <summary>
        /// 授权页回调
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult OpenOAuthCallback(string id)
        {
            string auth_code = Request["auth_code"];
            var userId = Request["userId"];
            var rid = Convert.ToInt32(string.IsNullOrEmpty(Request["rid"]?.ToString()) ? "0" : Request["rid"]);
            var cityinfoid = Convert.ToInt32(string.IsNullOrEmpty(Request["cityinfoid"]?.ToString()) ? "0" : Request["cityinfoid"]);
            var storeid = Convert.ToInt32(string.IsNullOrEmpty(Request["storeid"]?.ToString()) ? "0" : Request["storeid"]);
            var newtype = Convert.ToInt32(string.IsNullOrEmpty(Request["newtype"]?.ToString()) ? "0" : Request["newtype"]);
            var citytype = Convert.ToInt32(string.IsNullOrEmpty(Request["citytype"]?.ToString()) ? "0" : Request["citytype"]);
            var returnurl = Request["returnurl"]?.ToString();
            int expires_in = Convert.ToInt32(Request["expires_in"]);
            
            var currentmodel = OpenPlatConfigBLL.SingleModel.getCurrentModel();

            //使用授权码获取小程序授权信息
            var queryAuthResult = WxRequest.QueryAuth(currentmodel.component_access_token, WxRequest._component_AppId, auth_code);

            try
            {
                var authorizerInfoResult = WxRequest.GetAuthorizerInfo(currentmodel.component_access_token, WxRequest._component_AppId, queryAuthResult.authorization_info.authorizer_appid);

                //同城小程序授权禁止公众号授权
                if (null == authorizerInfoResult.authorizer_info.MiniProgramInfo && newtype != 4)
                {
                    return Redirect($"/XcxApi/NotSmallAppErroView");
                }
                StringBuilder str = new StringBuilder();
                foreach (FuncscopeCategoryItem item in queryAuthResult.authorization_info.func_info)
                {
                    str.Append(item.funcscope_category.id.ToString() + ",");
                }
                string func_info = str.ToString();
                if (func_info.Length > 0)
                {
                    func_info = func_info.Substring(0, func_info.Length - 1);
                }
                
                if (citytype <= 0)
                {
                    #region 服务号授权 用于电商版小程序微信会员卡功能
                    if (newtype == 4)
                    {
                        //表示微信会员卡链接过来的

                        if ((int)authorizerInfoResult.authorizer_info.service_type_info.id != 2)
                        {
                            //表示不是服务号
                            return Content($"该公众号不是服务号,<a href=\"{returnurl}\">返回</a>");
                        }

                        if ((int)authorizerInfoResult.authorizer_info.verify_type_info.id == -1)
                        {
                            //表示未认证
                            return Content($"该公众号未认证,<a href=\"{returnurl}\">返回</a>");

                        }

                        if (authorizerInfoResult.authorizer_info.business_info.open_card == 0)
                        {
                            //表示服务号未开通卡券功能
                            return Content($"服务号未开通卡券功能,请前往公众号平台开通,<a href=\"{returnurl}\">返回</a>");
                        }
                    }
                    else
                    {
                        if (!func_info.Contains("小程序帐号管理权限"))
                        {
                            //表示服务号未开通卡券功能
                            return Content(func_info + $"没有勾选账号管理权限（小程序），请返回重新发起绑定请求，<a href=\"{returnurl}\">返回</a>");
                        }
                    }
                    #endregion
                }

                #region 小程序详细信息
                OpenAuthorizerConfig openconfig = _openAuthorizerConfigBLL.GetModel("user_name='" + authorizerInfoResult.authorizer_info.user_name + "'");
                if (openconfig == null)
                {
                    openconfig = new OpenAuthorizerConfig();
                }
                else
                {
                    //设置解绑时间
                    RedisUtil.Set<string>("JieBan" + authorizerInfoResult.authorizer_info.user_name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), TimeSpan.FromMinutes(10));

                    if (openconfig.minisnsid != userId && newtype != 4)
                    {

                        return Redirect($"/XcxApi/JieBan?username={authorizerInfoResult.authorizer_info.user_name}&userId={userId}&returnurl={WxRequest.AsUrlData(returnurl)}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}");
                        //return Content("授权失败，该小程序已被绑定");
                    }
                    else
                    {
                        if (newtype > 0)
                        {
                            if (openconfig.RId != rid && newtype != openconfig.newtype && newtype != 4)//newtype=4表示服务号 小程序微信卡券功能识别
                            {
                                return Redirect($"/XcxApi/JieBan?username={authorizerInfoResult.authorizer_info.user_name}&userId={userId}&returnurl={WxRequest.AsUrlData(returnurl)}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}");
                                //return Content("授权失败，该小程序已被绑定");
                            }
                            if (cityinfoid > 0)
                            {
                                return Content($"参数错误,<a href=\"{returnurl}\">返回</a>");
                            }
                            cityinfoid = 0;
                        }
                        else
                        {
                            if (cityinfoid > 0)
                            {
                                if (openconfig.type != cityinfoid && cityinfoid != 0 && openconfig.type != 0)
                                {
                                    return Redirect($"/XcxApi/JieBan?username={authorizerInfoResult.authorizer_info.user_name}&userId={userId}&returnurl={WxRequest.AsUrlData(returnurl)}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}");
                                    //return Content("授权失败，该小程序已被绑定");
                                }
                                if (openconfig.RId > 0)
                                {
                                    return Redirect($"/XcxApi/JieBan?username={authorizerInfoResult.authorizer_info.user_name}&userId={userId}&returnurl={WxRequest.AsUrlData(returnurl)}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}");
                                    //return Content("授权失败，该小程序已被绑定");
                                }
                            }
                            if (rid > 0)
                            {
                                if (openconfig.RId != rid && rid != 0 && openconfig.RId != 0)
                                {
                                    return Redirect($"/XcxApi/JieBan?username={authorizerInfoResult.authorizer_info.user_name}&userId={userId}&returnurl={WxRequest.AsUrlData(returnurl)}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}");
                                    //return Content("授权失败，该小程序已被绑定");
                                }
                                if (openconfig.type > 0)
                                {
                                    return Redirect($"/XcxApi/JieBan?username={authorizerInfoResult.authorizer_info.user_name}&userId={userId}&returnurl={WxRequest.AsUrlData(returnurl)}&rid={rid}&cityinfoid={cityinfoid}&newtype={newtype}");
                                    //return Content("授权失败，该小程序已被绑定");
                                }
                            }
                        }
                    }
                }
                openconfig.alias = authorizerInfoResult.authorizer_info.alias;
                openconfig.appid = queryAuthResult.authorization_info.authorizer_appid;
                openconfig.func_info = func_info;
                openconfig.head_img = authorizerInfoResult.authorizer_info.head_img;
                openconfig.nick_name = authorizerInfoResult.authorizer_info.nick_name;
                openconfig.qrcode_url = authorizerInfoResult.authorizer_info.qrcode_url;
                openconfig.service_type_info = (int)authorizerInfoResult.authorizer_info.service_type_info.id;
                openconfig.user_name = authorizerInfoResult.authorizer_info.user_name;
                openconfig.verify_type_info = (int)authorizerInfoResult.authorizer_info.verify_type_info.id;
                openconfig.state = 1;
                openconfig.minisnsid = userId;
                if (citytype > 0)//同城小程序
                {
                    openconfig.RId = citytype;
                    openconfig.newtype = storeid;
                }
                else
                {
                    openconfig.newtype = newtype;
                    openconfig.RId = rid;
                }

                openconfig.type = cityinfoid;
                if (openconfig.id > 0)
                {
                    _openAuthorizerConfigBLL.Update(openconfig);
                }
                else
                {
                    _openAuthorizerConfigBLL.Add(openconfig);
                }

                OpenAuthorizerInfo info = _openAuthorizerInfoBLL.GetModel(string.Format("user_name='{0}'", authorizerInfoResult.authorizer_info.user_name));

                if (info == null)
                {
                    info = new OpenAuthorizerInfo();
                }
                info.addtime = DateTime.Now;
                info.authorizer_access_token = "";//queryAuthResult.authorization_info.authorizer_access_token;
                info.authorizer_appid = authorizerInfoResult.authorization_info.authorizer_appid;
                info.authorizer_refresh_token = queryAuthResult.authorization_info.authorizer_refresh_token;
                info.refreshtime = DateTime.Now;
                info.status = 1;
                info.minisnsid = userId;
                info.user_name = authorizerInfoResult.authorizer_info.user_name;
                if (info.id > 0)
                {
                    _openAuthorizerInfoBLL.UpdateModel(info);
                }
                else
                {
                    _openAuthorizerInfoBLL.Add(info);
                }

                if (rid > 0 && newtype == 0)
                {

                    var xcxmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(rid);
                    if (xcxmodel != null)
                    {
                        //消除其他绑定了该小程序的模板
                        XcxAppAccountRelationBLL.SingleModel.UpdateModelByAppId(openconfig.appid);

                        xcxmodel.AppId = openconfig.appid;
                        xcxmodel.ThirdOpenType = 1;
                        XcxAppAccountRelationBLL.SingleModel.Update(xcxmodel);
                    }
                }

                #endregion

                return Redirect(returnurl + "&rid=" + rid);
            }
            catch (Exception ex)
            {
                //log4net.LogHelper.WriteInfo(this.GetType(), GetIP() + "OpenOAuthCallback：1" + ex.Message);
                return Content(ex.Message);
            }
        }

        public ActionResult NotSmallAppErroView()
        {
            return View();
        }

        /// <summary>
        /// 解除绑定小程序
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userId"></param>
        /// <param name="returnurl"></param>
        /// <param name="rid"></param>
        /// <param name="cityinfoid"></param>
        /// <param name="newtype"></param>
        /// <returns></returns>
        public ActionResult JieBan(string username="", string userId="", string returnurl="", int rid=0, int cityinfoid=0, int newtype = 0)
        {
            var datetime = RedisUtil.Get<string>("JieBan" + username);
            if (string.IsNullOrEmpty(datetime))
            {
                return Content($"非法请求,<a href=\"{returnurl}\">返回</a>");
            }
            if (DateTime.Parse(datetime).AddMinutes(10) < DateTime.Now)
            {
                return Content($"请求已过期，请重新授权,<a href=\"{returnurl}\">返回</a>");
            }

            OpenAuthorizerConfig openconfig = _openAuthorizerConfigBLL.GetModel("user_name='" + username + "'");
            if (openconfig == null)
            {
                return Content($"请求数据失效，请重新授权,<a href=\"{returnurl}\">返回</a>");
            }

            //var xcxrelations = new XcxAppAccountRelationBLL().GetList($"AppId='{openconfig.appid}'");
            JieBangModel model = new Models.JieBangModel();
            model.appId = openconfig.appid;
            model.xcxname = openconfig.nick_name;
            model.userId = userId;
            model.returnurl = WxRequest.AsUrlData(returnurl);
            model.rid = rid;
            model.cityinfoid = cityinfoid;
            model.newtype = newtype;
            model.username = username;
            model.name = "未知对象";
            //同城小程序
            if (openconfig.type > 0 && openconfig.RId == 0 && openconfig.newtype == 0)
            {
                
            }
            else if (openconfig.type == 0 && openconfig.RId > 0 && openconfig.newtype == 0)
            {
                //小程序
                var xcxmodel = XcxAppAccountRelationBLL.SingleModel.GetModel(openconfig.RId);
                if (xcxmodel != null)
                {
                    var tempmodel = XcxTemplateBLL.SingleModel.GetModel(xcxmodel.TId);
                    if (tempmodel != null)
                    {
                        model.name = tempmodel.TName;
                    }

                }
            }
            else if (openconfig.type == 0 && openconfig.RId > 0 && openconfig.newtype == 1)
            {
                //论坛
                model.name = "论坛";
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult SaveJieBan(string userId)
        {
            var datetime = RedisUtil.Get<string>("JieBan" + userId);
            if (string.IsNullOrEmpty(datetime))
            {
                return Json(new { isok = -1, msg = "非法请求" });
            }
            if (DateTime.Parse(datetime).AddMinutes(10) < DateTime.Now)
            {
                return Json(new { isok = -1, msg = "请求已过期，请重新授权" });
            }

            var result = XcxAppAccountRelationBLL.SingleModel.JieBang(userId);
            if (!result)
            {
                return Json(new { isok = -1, msg = "解绑失败" });
            }

            return Json(new { isok = 1, msg = "解绑成功，跳转授权中..." });
        }
        #endregion

        #region 代码管理
        /// <summary>
        /// 上传小程序代码
        /// </summary>
        /// <param name="username">商户小程序原始Id</param>
        /// <param name="datajson">自定义参数</param>
        /// <param name="projectType">项目类型</param>
        /// <returns></returns>
        public ActionResult Commit(string username, string datajson)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo openAuthoInfo = _openAuthorizerInfoBLL.getCurrentModel(username);
                if(openAuthoInfo==null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                //判断是否已上传过代码
                UserXcxTemplate userxcxtemplate = UserXcxTemplateBLL.SingleModel.GetModelByAppId(openAuthoInfo.authorizer_appid);
                if (userxcxtemplate == null)
                {
                    return Json(new { isok = -1, msg = "没有找到商户添加的模板" }, JsonRequestBehavior.AllowGet);
                }
                
                //小程序模板
                XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModelFromMaster(userxcxtemplate.TId);
                if (xcxtemplate == null || xcxtemplate.Id <= 0)
                {
                    return Json(new { isok = -1, msg = "没有找到小程序模板" }, JsonRequestBehavior.AllowGet);
                }

                CommitModel model = new CommitModel();
                model.user_version = xcxtemplate.Version;
                model.user_desc = xcxtemplate.Desc;
                model.template_id = xcxtemplate.TId;
                model.ext_json = datajson;
                OAuthAccessTokenResult result = WxRequest.Commit(openAuthoInfo.authorizer_access_token, model);
                if (result.errcode == 0)
                {
                    return SubmitAuditCommont(openAuthoInfo, xcxtemplate, userxcxtemplate);
                }

                return Json(new { isok = -1, msg = "上传代码失败" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = -2, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 上传小程序代码不审核
        /// </summary>
        /// <param name="username">商户小程序原始Id</param>
        /// <param name="data">自定义参数</param>
        /// <param name="isnewcode">是否重新上传</param>
        /// <returns></returns>
        public ActionResult CommitCode(string username, string datajson)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo openAuthoInfo = _openAuthorizerInfoBLL.getCurrentModel(username);
                if(openAuthoInfo==null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                UserXcxTemplate userXcxLModel = UserXcxTemplateBLL.SingleModel.GetModelByUserName(username);
                if (userXcxLModel == null)
                {
                    return Json(new { isok = -1, msg = "没有找到用户模板信息" }, JsonRequestBehavior.AllowGet);
                }

                //小程序模板
                XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetModel(userXcxLModel.TId);
                if (xcxtemplate == null || xcxtemplate.Id <= 0)
                {
                    return Json(new { isok = -1, msg = "没有找到小程序模板" }, JsonRequestBehavior.AllowGet);
                }
                
                CommitModel model = new CommitModel();
                model.user_version = xcxtemplate.Version;
                model.user_desc = xcxtemplate.Desc;
                model.template_id = xcxtemplate.TId;
                model.ext_json = datajson;

                OAuthAccessTokenResult result = WxRequest.Commit(openAuthoInfo.authorizer_access_token, model);
                if (result.errcode == 0)
                {
                    return Json(new { isok = 1, msg = "上传代码成功" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = -1, msg = "上传代码失败" +result.errcode.ToString()}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = -2, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 提交审核代码
        /// </summary>
        /// <param name="amodel"></param>
        /// <param name="xcxtemplate"></param>
        /// <param name="userxcxtemplate"></param>
        /// <returns></returns>
        private ActionResult SubmitAuditCommont(OpenAuthorizerInfo amodel, XcxTemplate xcxtemplate, UserXcxTemplate userxcxtemplate)
        {
            OAuthAccessTokenResult obj = new OAuthAccessTokenResult();
            int isok = 0;
            string msg = "";

            #region 获取页面路径
            OAuthAccessTokenResult pageResult = WxRequest.GetPage(amodel.authorizer_access_token);
            if (pageResult.errcode != 0 || pageResult.page_list == null || pageResult.page_list.Count <= 0)
            {
                return Json(new { isok = -1, msg = "获取小程序页面路径失败" }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            #region 获取服务类目
            Category cresult = WxRequest.getCategory(amodel.authorizer_access_token);
            if(cresult.errcode != 0)
            {
                return Json(new { isok = -1, msg = "获取小程序服务类目失败" }, JsonRequestBehavior.AllowGet);
            }
            if (cresult.category_list==null || cresult.category_list.Count<=0)
            {
                return Json(new { isok = -1, msg = "您的小程序还没有添加服务类目" }, JsonRequestBehavior.AllowGet);
            }

            string ftclass = string.Empty;
            int ftid = 0;
            string sdclass = string.Empty;
            int sdid = 0;
            string tdclass = string.Empty;
            int tdid = 0;
            SubmitModel data = new SubmitModel();

            List<CategoryList> classdata = cresult.category_list;
            ftclass = classdata[0].first_class;
            ftid = classdata[0].first_id;
            sdclass = classdata[0].second_class;
            sdid = classdata[0].second_id;
            tdclass = classdata[0].third_class;
            tdid = classdata[0].third_id;
            #endregion

            #region 配置页面数据
            data.item_list = new List<ItemListModel>{tdid>=0? new ItemListModel()
                    {
                        address = pageResult.page_list[0],
                        tag=string.IsNullOrEmpty(userxcxtemplate.Tag)?xcxtemplate.Tag:userxcxtemplate.Tag,
                        first_class=ftclass,
                        second_class=sdclass,
                        first_id=ftid,
                        second_id=sdid,
                        title=xcxtemplate.Title,
                    }:new ItemListModel()
                    {
                        address = pageResult.page_list[0],
                        tag=string.IsNullOrEmpty(userxcxtemplate.Tag)?xcxtemplate.Tag:userxcxtemplate.Tag,
                        first_class=ftclass,
                        second_class=sdclass,
                        third_class=tdclass,
                        first_id=ftid,
                        second_id=sdid,
                        third_id=tdid,
                        title=xcxtemplate.Title,
                    }
                };
            #endregion

            OAuthAccessTokenResult result = WxRequest.SubmitAudit(amodel.authorizer_access_token, data);
            if (result.errcode == 0)
            {
                isok = 1;
                msg = "已提交审核";
                obj = result;

                userxcxtemplate.State = 2;
                userxcxtemplate.Auditid = obj.auditid;
                userxcxtemplate.Reason = "正在审核中";
            }
            else
            {
                isok = -1;
                msg = "提交失败:" + result.errcode.ToString();
                obj = result;

                userxcxtemplate.State = -1;
                userxcxtemplate.Reason = msg;
            }
            
            userxcxtemplate.UpdateTime = DateTime.Now;
            if (userxcxtemplate.Id > 0)
            {
                UserXcxTemplateBLL.SingleModel.Update(userxcxtemplate, "State,UpdateTime,Auditid,Reason");
            }
            else
            {
                UserXcxTemplateBLL.SingleModel.Add(userxcxtemplate);
            }
            return Json(new { isok = isok, msg = msg, obj = obj }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交小程序代码审核
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitAudit(string username, int tid)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }
                if (tid <= 0)
                {
                    return Json(new { isok = -1, msg = "tid 不能小于0" }, JsonRequestBehavior.AllowGet);
                }

                //小程序模板
                XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetTModel(tid);
                if (xcxtemplate == null || xcxtemplate.Id <= 0)
                {
                    return Json(new { isok = -1, msg = "没有找到小程序模板" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel == null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                //判断是否已提交审核过代码
                UserXcxTemplate userxcxtemplate = UserXcxTemplateBLL.SingleModel.GetTModel(tid, amodel.user_name, amodel.authorizer_appid);
                if (userxcxtemplate != null)
                {
                    //判断是否已提交审核
                    if (userxcxtemplate.Version == xcxtemplate.Version && userxcxtemplate.State == 1)
                    {
                        return Json(new { isok = 1, msg = "您已提交审核了该小程序模板" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    userxcxtemplate = new UserXcxTemplate();
                }

                return SubmitAuditCommont(amodel, xcxtemplate, userxcxtemplate);
            }
            catch (Exception ex)
            {
                return Json(new{code = -1,msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// 发布小程序代码
        /// </summary>
        /// <returns></returns>
        public ActionResult ReleaseCode(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    OAuthAccessTokenResult cresult = WxRequest.Release(amodel.authorizer_access_token);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { isok = 1, msg = "发布成功", obj = cresult }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { isok = -1, msg = cresult.errcode.ToString(), obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 撤回审核，单个帐号每天审核撤回次数最多不超过1次，一个月不超过10次。
        /// </summary>
        /// <returns></returns>
        public ActionResult UndocodeAudit(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    OAuthAccessTokenResult cresult = WxRequest.UndocodeAudit(amodel.authorizer_access_token);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { isok = 1, msg = "审核撤回成功", obj = cresult }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { isok = -1, msg = cresult.errcode.ToString(), obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 小程序版本回退
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public ActionResult VersionBack(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo openAuthoInfo = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (openAuthoInfo == null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                WxJsonResult result = WxRequest.VersionBack(openAuthoInfo.authorizer_access_token);
                if (result.errcode == 0)
                {
                    return Json(new { isok = 1, msg = "回退版本成功" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = -1, msg = result.errcode.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = -2, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取审核结果
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAuthMsg(string username, int tid)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { isok = -1, msg = "username 不能为空" }, JsonRequestBehavior.AllowGet);
                }
                if (tid <= 0)
                {
                    return Json(new { isok = -1, msg = "tid 不能小于0" }, JsonRequestBehavior.AllowGet);
                }
                //小程序模板
                XcxTemplate xcxtemplate = XcxTemplateBLL.SingleModel.GetTModel(tid);
                if (xcxtemplate == null || xcxtemplate.Id <= 0)
                {
                    return Json(new { isok = -1, msg = "没有找到小程序模板" }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel == null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }
                
                //判断是否已提交审核过代码
                var userxcxtemplate = UserXcxTemplateBLL.SingleModel.GetTModel(tid, amodel.user_name, amodel.authorizer_appid);
                if (userxcxtemplate != null)
                {
                    return Json(new { isok = -1, msg = "还没上传过小程序" }, JsonRequestBehavior.AllowGet);
                }
                if (userxcxtemplate.Auditid <= 0)
                {
                    return Json(new { isok = -1, msg = "还没提交审核过小程序" }, JsonRequestBehavior.AllowGet);
                }

                var cresult = userxcxtemplate.Auditid > 0 ? WxRequest.GetAuditstatus(amodel.authorizer_access_token, userxcxtemplate.Auditid) : WxRequest.GetLatestAuditstatus(amodel.authorizer_access_token);
                if (cresult.status == 1)
                {
                    return Json(new { isok = 1, msg = "审核失败，" + cresult.reason, obj = cresult }, JsonRequestBehavior.AllowGet);
                }
                else if (cresult.status == 2)
                {
                    return Json(new { isok = 1, msg = "审核中", obj = cresult }, JsonRequestBehavior.AllowGet);
                }
                else if (cresult.status == 0)
                {
                    return Json(new { isok = 1, msg = "审核成功", obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = cresult.errcode.ToString(), obj = cresult }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = "操作异常,msg=" + ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        
        #region 用户管理
        /// <summary>
        /// 获取体验者列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMemberAuth(string username)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    string access_token = amodel.authorizer_access_token;
                    OAuthAccessTokenResult cresult = WxRequest.GetMemberauth(access_token);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { msg = "成功", isok = 1, obj = cresult }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { msg = cresult.errcode.ToString(), isok = -1 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 设置小程序体验者
        /// </summary>
        /// <returns></returns>
        public ActionResult Settester(string username, string tester)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    string access_token = amodel.authorizer_access_token;
                    OAuthAccessTokenResult cresult = WxRequest.BindTester(access_token, tester);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { msg = "发送体验者邀请成功", isok = 1 }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { msg = cresult.errcode.ToString(), isok = -1 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 解除小程序体验者
        /// </summary>
        /// <param name="username">小程序原始Id</param>
        /// <param name="tester">微信图</param>
        /// <returns></returns>
        public ActionResult UnSettester(string username, string tester)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    string access_token = amodel.authorizer_access_token;
                    OAuthAccessTokenResult cresult = WxRequest.UnbindTester(access_token, tester);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { msg = "解除体验者成功", isok = 0 }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { msg = cresult.errcode.ToString(), isok = -1 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 服务器设置
        /// <summary>
        /// 小程序服务器地址管理
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateServerHost(string username, string datajson)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel == null)
                {
                    return Json(new { code = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }
                
                string access_token = amodel.authorizer_access_token;
                ServerHost tempdata = JsonConvert.DeserializeObject<ServerHost>(datajson);
                if (tempdata == null)
                {
                    return Json(new { code = -1, msg = "datajson参数为空" }, JsonRequestBehavior.AllowGet);
                }

                if (tempdata.action == "get")
                {
                    var data = new { action = "get" };
                    ServerHost cresult = WxRequest.ModifyDomain(access_token, data);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { msg = "成功", isok = 1, host = cresult }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { msg = cresult.errcode.ToString(), isok = -1 }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var data = new
                    {
                        action = tempdata.action,
                        requestdomain = tempdata.requestdomain,
                        uploaddomain = tempdata.uploaddomain,
                        downloaddomain = tempdata.downloaddomain,
                        wsrequestdomain = tempdata.wsrequestdomain,
                    };
                    ServerHost cresult = WxRequest.ModifyDomain(access_token, data);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { msg = "配置成功", isok = 1 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { msg = cresult.errcode.ToString(), isok = -1 }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Json(new
                {
                    isok = -1,
                    msg = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 小程序业务域名
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateWebviewDomain(string username, string action, string datajson)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel == null)
                {
                    return Json(new { code = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                string access_token = amodel.authorizer_access_token;
                List<string> webviewdomain = new List<string>();
                if (!string.IsNullOrEmpty(datajson))
                {
                    foreach (string item in datajson.Split(';'))
                    {
                        webviewdomain.Add(item);
                    }
                }

                var data = new
                {
                    action = action,
                    webviewdomain = webviewdomain,
                };
                ServerHost cresult = WxRequest.SetWebviewDomain(access_token, data);
                if (cresult.errcode == 0)
                {
                    return Json(new { msg = "配置成功", isok = 1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = cresult.errcode.ToString() + datajson + action, isok = -2 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isok = -1,
                    msg = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        /// <summary>
        /// 获取授权用户acccetoken
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOpenAuthodModel(string username)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    return Json(new { isok = 1, msg = "获取用户授权信息", obj = new { access_token = amodel.authorizer_access_token } }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new{isok = -1,msg = ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// code 换取 session_key
        /// </summary>
        /// <param name="appid">小程序appid</param>
        /// <param name="js_code">登录时获取的 code</param>
        /// <returns></returns>
        public ActionResult GetSessionKey(string appid, string js_code)
        {
            try
            {
                OpenPlatConfig componentmodel = OpenPlatConfigBLL.SingleModel.getCurrentModel();
                if (componentmodel == null)
                {
                    return Json(new { msg = "没有第三方平台", isok = -1 }, JsonRequestBehavior.AllowGet);
                }

                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.GetModelByAppId(appid);
                if (amodel == null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                amodel = _openAuthorizerInfoBLL.getCurrentModel(amodel.user_name);
                if (amodel == null)
                {
                    return Json(new { isok = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
                }

                OAuthAccessTokenResult cresult = WxRequest.GetSessionKey(appid, js_code, componentmodel.component_Appid, componentmodel.component_access_token);
                
                if (cresult== null || string.IsNullOrEmpty(cresult.session_key))
                {
                    return Json(new { msg = "api获取秘钥为空"+cresult?.errcode.ToString(), isok = -1 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { msg = "获取成功", isok = 1, obj = cresult }, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception)
            {
                return Json(new
                {
                    isok = -1,
                    msg = "登陆火爆，请刷新重新"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// 获取小程序体验二维码
        /// </summary>
        /// <returns></returns>
        public ActionResult getqrcode(string username)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel != null)
                {
                    string access_token = amodel.authorizer_access_token;
                    var cresult = WxRequest.GetQrcode(access_token);
                    Stream stream = cresult.GetResponseStream();

                    Image image = Image.FromStream(stream);
                    MemoryStream ms = new MemoryStream();
                    Bitmap bmp = new Bitmap(image);
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length);
                    ms.Close();
                    string baseimg = Convert.ToBase64String(arr);

                    return Json(new { msg = "获取体验二维码", src = baseimg, isok = 1 }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isok = -1,
                    msg = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// 获取小程序服务类目
        /// </summary>
        /// <param name="username">小程序原始ID</param>
        /// <returns></returns>
        public ActionResult GetCategoryTyleList(string username)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel == null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                Category cresult = WxRequest.getCategory(amodel.authorizer_access_token);
                List<CategoryList> classdata = new List<CategoryList>();
                if (cresult.errcode != 0)
                {
                    return Json(new { isok = -1, msg = "获取小程序类目失败:" + cresult.errcode.ToString() }, JsonRequestBehavior.AllowGet);
                }

                if (cresult.category_list.Count > 0)
                {
                    return Json(new { isok = 1, msg = "成功", data = cresult.category_list }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = -1, msg = "您的小程序还没有添加服务类目" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 解除小程序绑定的平台,该平台必须是平台申请的平台
        /// </summary>
        /// <param name="username">小程序原始ID</param>
        /// <returns></returns>
        public ActionResult UnbindXiaoChengXu(string username)
        {
            try
            {
                OpenAuthorizerInfo amodel = _openAuthorizerInfoBLL.getCurrentModel(username);
                if (amodel == null)
                {
                    return Json(new { isok = -1, msg = "请重新授权" }, JsonRequestBehavior.AllowGet);
                }

                OAuthAccessTokenResult cresult = WxRequest.Unbind(amodel.authorizer_access_token, amodel.authorizer_appid, WxRequest._component_AppId);

                if (cresult.errcode != 0)
                {
                    return Json(new { isok = -1, msg = "解绑失败:" + cresult.errcode.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { isok = 1, msg = "解绑成功" }, JsonRequestBehavior.AllowGet);
        }
    }
}