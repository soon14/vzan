using BLL.MiniSNS;
using BLL.MiniSNS.VZANCity;
using BLL.OpenWx;
using Entity.MiniSNS.VZANCity;
using Entity.OpenWx;
using Entity.XcxApis.OpenWx;
using Newtonsoft.Json;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.HttpUtility;
using Senparc.Weixin.Open.ComponentAPIs;
using Senparc.Weixin.Open.XcxApis;
using Senparc.Weixin.Open.XcxApis.XcxApiJson;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Utility.AliOss;

namespace OpenWx.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index(string value = "")
        {
            //    string sqlwhere = "XcxAppid = '" + value+ "' or WxAppid='" + value+"'";
            //    var modellist = new PayAppidRelationBLL().GetList(sqlwhere);

            return View();
        }

        public ActionResult GetData(int typeid, string value)
        {
            string sqlwhere = string.Empty;
            try
            {
                switch (typeid)
                {
                    //case 1:
                    //    sqlwhere = "XcxAppid = '" + value + "' or WxAppid='" + value + "'";
                    //    var modellist1 = new PayAppidRelationBLL().GetList(sqlwhere);

                    //    return Json(new { code = 1, modellist = modellist1 }, JsonRequestBehavior.AllowGet);
                    //    break;
                    //case 2:
                    //    sqlwhere = "openId = '" + value + "' or Id='" + value + "'";
                    //    var modellist2 = new C_UserInfoBLL().GetList(sqlwhere);

                    //    return Json(new { code = 1, modellist = modellist2 }, JsonRequestBehavior.AllowGet);
                    //    break;
                    //case 3:
                    //    sqlwhere = "UserID = '" + value + "'";
                    //    var modellist3 = new CityUserCashLogBLL().GetList(sqlwhere, 10, 1, "", "addtime desc");

                    //    return Json(new { code = 1, modellist = modellist3 }, JsonRequestBehavior.AllowGet);
                    //    break;
                    //case 4:
                    //    var TableName = new C_UserCashBLL().GetTableName(value);
                    //    var modellist4 = new C_UserCashBLL().GetListBySql($"select * from {TableName} where UserID={value}");

                    //    return Json(new { code = 1, modellist = modellist4 }, JsonRequestBehavior.AllowGet);
                    //    break;
                    case 5:
                        sqlwhere = "BindingId = '" + value + "'";
                        if (value.Length > 10)
                        {
                            sqlwhere = "Appid = '" + value + "'";
                        }
                        var modellist5 = new PayCenterSettingBLL().GetList(sqlwhere);

                        return Json(new { code = 1, modellist = modellist5 }, JsonRequestBehavior.AllowGet);
                    case 7:

                        var modellist7 = OpenAuthorizerConfigBLL.SingleModel.GetList();
                        return Json(new { code = 1, modellist = modellist7 }, JsonRequestBehavior.AllowGet);
                        break;
                    case 8:

                        var modellist8 = OpenAuthorizerInfoBLL.SingleModel.GetList();
                        return Json(new { code = 1, modellist = modellist8 }, JsonRequestBehavior.AllowGet);
                        break;
                    case 9:

                        var modellist9 = UserXcxTemplateBLL.SingleModel.GetList();
                        return Json(new { code = 1, modellist = modellist9 }, JsonRequestBehavior.AllowGet);
                        break;
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = -1, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //if (typeid == 1)
            //{
            //    string sqlwhere = "XcxAppid = '" + value + "' or WxAppid='" + value + "'";
            //    var modellist = new PayAppidRelationBLL().GetList(sqlwhere);

            //    return Json(new { code = -1, modellist = modellist }, JsonRequestBehavior.AllowGet);
            //}
            //else if (typeid == 2)
            //{
            //    string sqlwhere = "openId = '" + value + "' or Id='" + value+"'";
            //    var modellist = new C_UserInfoBLL().GetList(sqlwhere);

            //    return Json(new { code = -1, modellist = modellist }, JsonRequestBehavior.AllowGet);
            //}
            //else if (typeid == 4)
            //{
            //    var TableName = new C_UserCashBLL().GetTableName(value);
            //    var modellist = new C_UserCashBLL().GetListBySql($"select * from {TableName} where UserID={value}");

            //    return Json(new { code = -1, modellist = modellist }, JsonRequestBehavior.AllowGet);
            //}
            //else if (typeid == 3)
            //{
            //    string sqlwhere = "UserID = '" + value + "'";
            //    var modellist = new CityUserCashLogBLL().GetList(sqlwhere,10,1,"","addtime desc");

            //    return Json(new { code = -1, modellist = modellist }, JsonRequestBehavior.AllowGet);
            //}
            //else if (typeid == 5)
            //{
            //    string sqlwhere = "BindingId = '" + value + "'";
            //    if(value.Length>10)
            //    {
            //        sqlwhere ="Appid = '" + value + "'";
            //    }
            //    var modellist = new PayCenterSettingBLL().GetList(sqlwhere);

            //    return Json(new { code = -1, modellist = modellist }, JsonRequestBehavior.AllowGet);
            //}
            //else if (typeid == 6)
            //{
            //    //var modellist = new opencomponentconfigBLL().getCurrentModel();

            //    return Json(new { code = -1, modellist = "" }, JsonRequestBehavior.AllowGet);
            //}

            return Json(new { code = -1, msg = "参数异常" }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult AddRelation(string wxappid, string xcxappid, int typeid = 0)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(wxappid) || string.IsNullOrEmpty(xcxappid))
        //        {
        //            return Json(new { code = -1, msg = "appid 不能为空" });
        //        }

        //        if (typeid == 0)
        //        {
        //            var model = new PayAppidRelationBLL().GetModel($"WxAppid='{wxappid}' and XcxAppid='{xcxappid}'");
        //            if (model != null)
        //            {
        //                return Json(new { code = -1, msg = "不能重复添加" });
        //            }
        //            model = new PayAppidRelation();
        //            model.AddTime = DateTime.Now;
        //            model.WxAppid = wxappid;
        //            model.XcxAppid = xcxappid;
        //            var result = new PayAppidRelationBLL().Add(model);
        //            if (Convert.ToInt32(result) > 0)
        //            {
        //                return Json(new { code = 1, msg = "添加成功" });
        //            }
        //            return Json(new { code = -1, msg = "添加失败" });
        //        }
        //        else
        //        {
        //            var model = new PayAppidRelationBLL().GetModel($"WxAppid='{wxappid}' and XcxAppid='{xcxappid}'");
        //            if (model == null)
        //            {
        //                return Json(new { code = -1, msg = "请先添加" });
        //            }

        //            model.AddTime = DateTime.Now;
        //            model.WxAppid = wxappid;
        //            model.XcxAppid = xcxappid;
        //            var result = new PayAppidRelationBLL().Update(model);
        //            if (Convert.ToInt32(result) > 0)
        //            {
        //                return Json(new { code = 1, msg = "修改成功" });
        //            }
        //            return Json(new { code = -1, msg = "修改失败" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { code = 0, msg = "操作异常,msg=" + ex.Message });
        //    }
        //}
        public ActionResult AddShanghu(string appid, string mc_id, string mc_key, int typeid = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(appid))
                {
                    return Json(new { code = -1, msg = "appid 不能为空" });
                }
                if (string.IsNullOrEmpty(mc_id))
                {
                    return Json(new { code = -1, msg = "mc_id 不能为空" });
                }
                if (string.IsNullOrEmpty(mc_key))
                {
                    return Json(new { code = -1, msg = "mc_key 不能为空" });
                }

                if (typeid == 0)
                {
                    var model = new PayCenterSettingBLL().GetModel($"Appid='{appid}'");
                    if (model != null)
                    {
                        return Json(new { code = -1, msg = "不能重复添加" });
                    }
                    model = new Entity.MiniSNS.PayCenterSetting();
                    model.Appid = appid;
                    model.Mch_id = mc_id;
                    model.Key = mc_key;
                    model.BindingType = 5;
                    model.Status = 0;
                    var result = new PayCenterSettingBLL().Add(model);
                    if (Convert.ToInt32(result) > 0)
                    {
                        return Json(new { code = 1, msg = "添加成功" });
                    }
                    return Json(new { code = -1, msg = "添加失败" });
                }
                else
                {
                    var model = new PayCenterSettingBLL().GetModel($"Appid='{appid}'");
                    if (model == null)
                    {
                        return Json(new { code = -1, msg = "请先添加" });
                    }

                    model.Appid = appid;
                    model.Mch_id = mc_id;
                    model.Key = mc_key;
                    model.BindingType = 5;
                    model.Status = 0;
                    var result = new PayCenterSettingBLL().Update(model);
                    if (Convert.ToInt32(result) > 0)
                    {
                        return Json(new { code = 1, msg = "修改成功" });
                    }
                    return Json(new { code = -1, msg = "修改失败" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { code = 0, msg = "操作异常,msg=" + ex.Message });
            }
        }

        /// <summary>
        /// 获取跳转到授权页链接
        /// </summary>
        /// <returns></returns>
        public ActionResult getOpenImg()
        {
            var msg = string.Empty;
            try
            {
                var model = opencomponentconfigBLL.SingleModel.getCurrentModel();
                if (model != null)
                {
                    var pre_token = ComponentApi.GetPreAuthCode(model.component_Appid, model.component_access_token);
                    if (pre_token != null)
                    {
                        var returnurl = "http://chx.vzan.com/XcxManage/Index?cityInfoId=695";
                        var callbackurl = "http://openapp.vzan.com/Test/OpenOAuthCallback?AreaCode=110228&returnurl=" + returnurl;
                        var url = XcxApis.XcxApis_tiaozhuan(model.component_Appid, pre_token.pre_auth_code, callbackurl);
                        msg = url;
                    }
                }
                //return Redirect(msg);
                return Json(new { code = 1, src = msg, msg = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 0, src = "", msg = ex.Message + "," + msg }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 授权页回调
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult OpenOAuthCallback(string id)
        {
            var reurl = "http://openapp.vzan.com/test/index";
            var appid = "wx9a6ab00a752e10e8";
            string auth_code = Request["auth_code"];
            int areacode = int.Parse(Request["AreaCode"]?.ToString());
            int expires_in = Convert.ToInt32(Request["expires_in"]);

            var currentmodel = opencomponentconfigBLL.SingleModel.getCurrentModel();
            string token = currentmodel.component_access_token;

            //使用授权码获取小程序授权信息
            var queryAuthResult = ComponentApi.QueryAuth(token, appid, auth_code);

            try
            {
                var authorizerInfoResult = ComponentApi.GetAuthorizerInfo(token, appid, queryAuthResult.authorization_info.authorizer_appid);
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

                #region 公众号详细信息
                OpenAuthorizerConfig openconfig = OpenAuthorizerConfigBLL.SingleModel.GetModel("user_name='" + authorizerInfoResult.authorizer_info.user_name + "'");
                if (openconfig == null)
                {
                    openconfig = new OpenAuthorizerConfig();
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
                //openconfig.minisnsid = areacode;
                if (openconfig.id > 0)
                {
                    OpenAuthorizerConfigBLL.SingleModel.Update(openconfig);
                }
                else
                {
                    OpenAuthorizerConfigBLL.SingleModel.Add(openconfig);
                }
                
                OpenAuthorizerInfo info = OpenAuthorizerInfoBLL.SingleModel.GetModel(string.Format("user_name='{0}'", authorizerInfoResult.authorizer_info.user_name));

                if (info == null)
                {
                    info = new OpenAuthorizerInfo();
                }
                info.addtime = DateTime.Now;
                info.authorizer_access_token = queryAuthResult.authorization_info.authorizer_access_token;
                info.authorizer_appid = authorizerInfoResult.authorization_info.authorizer_appid;
                info.authorizer_refresh_token = queryAuthResult.authorization_info.authorizer_refresh_token;
                info.refreshtime = DateTime.Now;
                info.status = 1;
                //info.minisnsid = areacode;
                info.user_name = authorizerInfoResult.authorizer_info.user_name;
                if (info.id > 0)
                {
                    OpenAuthorizerInfoBLL.SingleModel.Update(info);
                }
                else
                {
                    OpenAuthorizerInfoBLL.SingleModel.Add(info);
                }

                #endregion
                return Redirect(reurl);
            }
            catch (ErrorJsonResultException ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return Content(ex.Message);
            }
        }
        /// <summary>
        /// 上传小程序代码
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="mc_id"></param>
        /// <param name="mc_key"></param>
        /// <param name="typeid"></param>
        /// <returns></returns>
        public ActionResult Commit(string appid, string mc_id, string mc_key, int areacode)
        {
            try
            {
                //if (string.IsNullOrEmpty(appid))
                //{
                //    return Json(new { code = -1, msg = "appid 不能为空" });
                //}
                //if (string.IsNullOrEmpty(mc_id))
                //{
                //    return Json(new { code = -1, msg = "mc_id 不能为空" });
                //}
                //if (string.IsNullOrEmpty(mc_key))
                //{
                //    return Json(new { code = -1, msg = "mc_key 不能为空" });
                //}
                //if (areacode<=0)
                //{
                //    return Json(new { code = -1, msg = "areacode不能小于0" });
                //}

                //var model = new PayCenterSettingBLL().GetModel($"Appid='{appid}'");
                //if (model != null)
                //{
                //    return Json(new { code = -1, msg = "不能重复添加" });
                //}

                var com_appid = "wx9cb1d8be83da075b";
                var amodel = OpenAuthorizerInfoBLL.SingleModel.GetModel("authorizer_appid='" + com_appid + "'");
                if (amodel != null)
                {
                    var data = new
                    {
                        extEnable = true,
                        extAppid = amodel.authorizer_appid,
                        ext = new
                        {
                            areaCode = 110228,
                            appid = amodel.authorizer_appid,
                            appsr = "2de3680a7314ac1254c98294d1eabf64"
                        },
                        window = new
                        {
                            navigationBarTitleText = "Q逗"
                        }
                    };

                    CommitModel model = new CommitModel();
                    model.user_version = "v1.0.0";
                    model.user_desc = "提供给同城用户的移动端经营管理工具,可以实现帖子添加、编辑、删除、置顶、刷新等管理功能";
                    model.template_id = 0;
                    model.ext_json = JsonConvert.SerializeObject(data);

                    var dd = XcxApis.XcxApis_Commit(amodel.authorizer_access_token, model);

                    return Json(new { code = 1, msg = dd });
                }

                return Json(new { code = -1, msg = "还未授权" });

            }
            catch (Exception ex)
            {
                return Json(new { code = 0, msg = "操作异常,msg=" + ex.Message });
            }
        }

        /// <summary>
        /// 刷新用户授权信息
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="mc_id"></param>
        /// <param name="mc_key"></param>
        /// <param name="typeid"></param>
        /// <returns></returns>
        public ActionResult refreshAccessToken(string appid)
        {
            try
            {
                var cmodel = opencomponentconfigBLL.SingleModel.getCurrentModel();
                var amodel = OpenAuthorizerInfoBLL.SingleModel.GetModel($"authorizer_appid='{appid}'");
                if (amodel != null && cmodel != null)
                {
                    var result = XcxApis.XcxApis_refreshauthorizer_token(cmodel.component_access_token, amodel.authorizer_refresh_token, cmodel.component_Appid, amodel.authorizer_appid,true);
                    if (result != null)
                    {
                        amodel.authorizer_refresh_token = result.authorizer_refresh_token;
                        amodel.authorizer_access_token = result.authorizer_access_token;
                        amodel.refreshtime = DateTime.Now;
                        if (OpenAuthorizerInfoBLL.SingleModel.Update(amodel))
                        {
                            return Json(new { code = 1, msg = "刷新成功" });
                        }
                        else
                        {
                            return Json(new { code = -1, msg = "刷新失败", obj = result });
                        }
                    }
                    else
                    {
                        return Json(new { code = -1, msg = "刷新失败", obj = result });
                    }
                }

                return Json(new { code = -1, msg = "请先授权" });

            }
            catch (Exception ex)
            {
                return Json(new { code = 0, msg = "操作异常,msg=" + ex.Message });
            }
        }


        /// <summary>
        /// 获取类目
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCategoryData()
        {
            try
            {
                var amodel = OpenAuthorizerInfoBLL.SingleModel.GetModel("authorizer_appid='wx9cb1d8be83da075b'");
                if (amodel != null)
                {
                    var result = XcxApis.XcxApis_get_category(amodel.authorizer_access_token);
                    if (result != null)
                    {
                        if (result.category_list.Count > 0)
                        {
                            var classdata = result.category_list.Where(w => w.first_class == "生活服务" && w.second_class == "综合生活服务平台").ToList();
                            if (classdata != null && classdata.Count > 0)
                            {
                                var ftclass = classdata[0].first_class;
                                var ftid = classdata[0].first_id;
                                var sdclass = classdata[0].second_class;
                                var sdid = classdata[0].second_id;
                            }
                        }
                        return Json(new { code = 1, msg = "获取成功", obj = result }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { code = -1, msg = "获取失败", obj = result }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { code = 0, msg = "操作异常,msg=" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 提交小程序代码审核
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitAudit()
        {
            try
            {
                var amodel = OpenAuthorizerInfoBLL.SingleModel.GetModel("authorizer_appid='wx9cb1d8be83da075b'");
                if (amodel != null)
                {
                    var cresult = XcxApis.XcxApis_get_category(amodel.authorizer_access_token);
                    if (cresult.errcode == 0)
                    {
                        if (cresult.category_list.Count > 0)
                        {
                            var classdata = cresult.category_list.Where(w => w.first_class == "生活服务" && w.second_class == "综合生活服务平台").ToList();
                            if (classdata != null && classdata.Count > 0)
                            {
                                var ftclass = classdata[0].first_class;
                                var ftid = classdata[0].first_id;
                                var sdclass = classdata[0].second_class;
                                var sdid = classdata[0].second_id;

                                SubmitModel data = new SubmitModel();
                                data.item_list = new List<ItemListModel>{ new ItemListModel()
                    {
                        address = "pages/launch/launch",
                        tag="同城 114 分类信息 便民 本地 生活",
                        first_class=ftclass,
                        second_class=sdclass,
                        first_id=ftid,
                        second_id=sdid,
                        title="招聘求职、房产租售、二手车/物品",
                    }
                };
                                var result = XcxApis.XcxApis_submit_audit(amodel.authorizer_access_token, data);
                                if (result.errcode == 0)
                                {
                                    return Json(new { code = 1, msg = "提交成功", obj = result }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { code = -1, msg = "提交失败", obj = result }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { code = -1, msg = "没有【生活服务》综合生活服务平台】服务分类", obj = classdata }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new { code = -1, msg = "没有添加服务分类", obj = cresult }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { code = -1, msg = cresult.errmsg, obj = cresult }, JsonRequestBehavior.AllowGet);
                    }
                }


                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取审核结果
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAuthMsg(string username, int auditid = 0)
        {
            try
            {
                //gh_b3c2409834fa
                var amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(username);
                if (amodel != null)
                {
                    var cresult = auditid > 0 ? XcxApis.XcxApis_get_auditstatus(amodel.authorizer_access_token, auditid) : XcxApis.XcxApis_get_latest_auditstatus(amodel.authorizer_access_token);
                    if (cresult.status == 1)
                    {
                        return Json(new { code = cresult.status, msg = "审核失败，" + cresult.reason, obj = cresult }, JsonRequestBehavior.AllowGet);
                    }
                    else if (cresult.status == 2)
                    {
                        return Json(new { code = cresult.status, msg = "审核中", obj = cresult }, JsonRequestBehavior.AllowGet);
                    }
                    else if (cresult.status == 0)
                    {
                        return Json(new { code = cresult.status, msg = "审核成功", obj = cresult }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { code = -1, obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// 获取获取小程序体验二维码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMiniappCode(string username)
        {
            string msg = "0";
            try
            {

                var amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(username);
                if (amodel != null)
                {
                    var access_token =amodel.authorizer_access_token;
                    msg += 1;
                    var cresult = XcxApis.XcxApis_grcode(access_token);
                    Stream stream = cresult.GetResponseStream();
                    Image image = Image.FromStream(stream);
                    MemoryStream ms = new MemoryStream();
                    Bitmap bmp = new Bitmap(image);
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length);
                    ms.Close();
                    var baseimg = Convert.ToBase64String(arr);
                    
                    return Json(new { msg = "获取体验二维码", obj = baseimg }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message+ ","+ msg
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 设置小程序体验者
        /// </summary>
        /// <returns></returns>
        public ActionResult Settester(string username,string tester)
        {
            string msg = "0";
            try
            {

                var amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(username);
                if (amodel != null)
                {
                    var access_token = amodel.authorizer_access_token;
                    var cresult = XcxApis.XcxApis_settester(access_token,tester);
                    return Json(new { msg = "获取体验二维码", obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message + "," + msg
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 解除小程序体验者
        /// </summary>
        /// <returns></returns>
        public ActionResult UnSettester(string username, string tester)
        {
            string msg = "0";
            try
            {

                var amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(username);
                if (amodel != null)
                {
                    var access_token = amodel.authorizer_access_token;
                    var cresult = XcxApis.XcxApis_unsettester(access_token, tester);
                    return Json(new { msg = "获取体验二维码", obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message + "," + msg
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 修改服务器地址
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateServerHost(string username)
        {
            string msg = "0";
            try
            {

                var amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(username);
                if (amodel != null)
                {
                    var access_token = amodel.authorizer_access_token;
                    //var data = new
                    //{
                    //    action = "get"
                    //};
                    var data = new
                    {
                        action = "add",
                        requestdomain = new List<string> { "https://txiaowei.vzan.com", "https://cityapi.vzan.com" },
                        uploaddomain = new List<string> { "https://txiaowei.vzan.com", "https://cityapi.vzan.com" },
                        downloaddomain = new List<string> { "https://txiaowei.vzan.com", "https://cityapi.vzan.com" },
                    };
                    var cresult = XcxApis.XcxApis_serverhost(access_token, data);
                    return Json(new { msg = "获取体验二维码", obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message + "," + msg
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 发布小程序代码
        /// </summary>
        /// <returns></returns>
        public ActionResult ReleaseCode(string name)
        {
            try
            {
                //var amodel = new OpenAuthorizerInfoBLL().GetModel("authorizer_appid='wx9cb1d8be83da075b'");
                var amodel = OpenAuthorizerInfoBLL.SingleModel.getCurrentModel(name);
                if (amodel != null)
                {
                    var cresult = XcxApis.XcxApis_release(amodel.authorizer_access_token);
                    if (cresult.errcode == 0)
                    {
                        return Json(new { code = 0, msg = "发布成功", obj = cresult }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { code = -1, msg = "发布失败", obj = cresult }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { code = -1, msg = "请先授权" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = 0,
                    msg = "操作异常,msg=" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult editoradd(XcxTemplate data)
        {
            try
            {
                if (data.Id > 0)
                {
                    data.UpdateTime = DateTime.Now;
                    var model = XcxTemplateBLL.SingleModel.GetModel(data.Id);
                    model.TName = data.TName;
                    model.TImgurl = data.TImgurl;
                    model.Version = data.Version;
                    model.Desc = data.Desc;
                    model.Address = data.Address;
                    model.Tag = data.Tag;
                    model.First_class = data.First_class;
                    model.Second_class = data.Second_class;
                    model.Third_class = data.Third_class;
                    model.Title = data.Title;
                    model.UpdateTime = data.UpdateTime;
                    string upstr = "TName,TImgurl,Version,Price,Desc,Address,Tag,First_class,Second_class,Third_class,Title,UpdateTime";
                    if (XcxTemplateBLL.SingleModel.Update(data,upstr))
                    {
                        return Json(new { isok = 1, msg = "修改成功", obj= data }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { isok = -1, msg = "修改失败" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var model = XcxTemplateBLL.SingleModel.GetModel("TId=" + data.TId);
                    if (model != null)
                    {
                        return Json(new { isok = -1, msg = "请不要重复添加" }, JsonRequestBehavior.AllowGet);
                    }
                    data.AddTime = DateTime.Now;
                    data.UpdateTime = DateTime.Now;
                    //data.State = 0;
                    if (Convert.ToInt32(XcxTemplateBLL.SingleModel.Add(data)) > 0)
                    {
                        return Json(new { isok = 1, msg = "添加成功",obj=data }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { isok = -1, msg = "添加失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
           

            return Json(new { isok = -1, msg = "登录信息过期，刷新试试" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getxcxt(int tid)
        {
            try
            {
                var model = XcxTemplateBLL.SingleModel.GetModel("TId=" + tid);
                if (model != null)
                {
                    return Json(new { isok = 1, msg = "成功", obj = model }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { isok = -1, msg = "没数据", obj = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isok = -1, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
           
        }
    }
}