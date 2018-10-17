using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Fds;
using BLL.MiniApp.FunList;
using BLL.MiniApp.Stores;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Fds;
using Entity.MiniApp.FunctionList;
using Entity.MiniApp.Stores;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using User.MiniApp.Filters;
using Utility;
using Utility.IO;

namespace User.MiniApp.Controllers
{

    public class  MiappTemplateMsgController: templateMsgController
    {

    }

    public class templateMsgController : baseController
    {
        public templateMsgController()
        {

        }

        /// <summary>
        /// 模板消息使用管理
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        //[HttpPost]
        [RouteAuthCheck]
        public ActionResult TemplateMsgManager(int appId=0,int PageType = 8)
        {
            string souceFrom = Context.GetRequest("SouceFrom", string.Empty);
            ViewBag.SouceFrom = souceFrom;
            if (dzaccount == null)
            {
                return Redirect("/dzhome/login");
            }
            XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
            if (app == null)
            {
                return View("PageError", new Return_Msg() { Msg = "没有权限!", code = "403" });
            }
            if (app.AppId.IsNullOrWhiteSpace())
            {
                return Redirect($"/config/MiniAppConfig?appId={appId}&id={appId}&SouceFrom={souceFrom}&type={PageType}");
            }
            ViewBag.appId = app.Id;
            
            //获取当前小程序模板类型
            string typeSql = $" select type from xcxtemplate where id = {app.TId} ";
            int temp = Convert.ToInt32(DAL.Base.SqlMySql.ExecuteScalar(XcxAppAccountRelationBLL.SingleModel.connName, CommandType.Text, typeSql, null));
            #region 专业版 版本控制
            int messageSwtich = 0;//消息开关功能 0表示开启 1表示关闭
            int versionId = 0;
            if (temp == (int)TmpType.小程序专业模板)
            {

                 versionId = app.VersionId;
                FunctionList functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={temp} and VersionId={versionId}");
                if (functionList == null)
                {
                    return View("PageError", new Return_Msg() { Msg = "此功能未开启!", code = "403" });

                }
                if (!string.IsNullOrEmpty(functionList.MessageMgr))
                {
                    MessageMgr messageMgr = JsonConvert.DeserializeObject<MessageMgr>(functionList.MessageMgr);
                    messageSwtich = messageMgr.TemplateMessage;
                }
               

            }
            #endregion
           

            List<TemplateMsg> miniapptemplatemsg = TemplateMsgBLL.SingleModel.GetListByType(PageType) ?? new List<TemplateMsg>();
            string msg = "";
            miniapptemplatemsg.ForEach(x =>
            {
                TemplateMsg_User newUserMsg = TemplateMsg_UserBLL.SingleModel.getModelByAppId(app.AppId, PageType, x.Id);
                if (newUserMsg == null)
                {
                    //往微信appid加模板消息
                    
                    addResultModel _addResult = MsnModelHelper.addMsnToMy(app.AppId, x.TitileId, x.ColNums.Split(','), ref msg);

                    newUserMsg = new TemplateMsg_User();
                    newUserMsg.AppId = app.AppId;
                    newUserMsg.Ttypeid = x.Ttypeid;
                    newUserMsg.TmId = x.Id;
                    newUserMsg.ColNums = x.ColNums;
                    newUserMsg.TitleId = x.TitileId;
                    newUserMsg.State = 0;//启用
                    newUserMsg.CreateDate = DateTime.Now;
                    newUserMsg.TemplateId = _addResult.template_id;//微信公众号内的模板Id
                    newUserMsg.TmgType = x.TmgType;

                    int result = Convert.ToInt32(TemplateMsg_UserBLL.SingleModel.Add(newUserMsg));
                }

                if (newUserMsg != null)
                {
                    if (temp == (int)TmpType.小程序专业模板&& messageSwtich==1)
                    {
                        //表示该专业版级别不能使用模板消息功能
                        newUserMsg.State = 0;
                        x.openState = 0;
                        TemplateMsg_UserBLL.SingleModel.Update(newUserMsg, "State");
                    }
                    else
                    {
                        x.openState = newUserMsg.State;
                    }

                      
                }
            });
            ViewBag.PageType = PageType;
            ViewBag.versionId = versionId;
            ViewBag.messageSwtich = messageSwtich;
            return View(miniapptemplatemsg);
        }

        /// <summary>
        /// 启用模板
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult startTmg(int appId, int TempMsgId,int PageType)
        {
            try
            {
                if (dzaccount == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
                if (app == null)
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet); 
                }
                if (app.AppId.IsNullOrWhiteSpace())
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }

                TemplateMsg miniapptemplatemsg = TemplateMsgBLL.SingleModel.GetModel(TempMsgId);
                if (miniapptemplatemsg == null)
                {
                    return Json(new { isok = false, msg = "系统内置的模板Id错误！" }, JsonRequestBehavior.AllowGet);
                }
                int TmpId = 0;
                //获取当前小程序模板类型
                string typeSql = $" select type from xcxtemplate where id = {app.TId} ";
                int temp = Convert.ToInt32(DAL.Base.SqlMySql.ExecuteScalar(XcxAppAccountRelationBLL.SingleModel.connName, CommandType.Text, typeSql, null));

                if (temp == (int)TmpType.小程序电商模板 || temp == (int)TmpType.小程序电商模板测试)
                {
                    Store store = StoreBLL.SingleModel.GetModelByRid(app.Id) ?? new Store();
                    TmpId = store.Id;
                }
                else if (temp == (int)TmpType.小程序餐饮模板)
                {
                    Food store = FoodBLL.SingleModel.GetModel($" appId = {app.Id} ") ?? new Food();
                    TmpId = store.Id;
                }

                if (temp == (int)TmpType.小程序专业模板)
                {

                    int industr = app.VersionId;
                    FunctionList functionList = FunctionListBLL.SingleModel.GetModel($"TemplateType={temp} and VersionId={industr}");
                    if (functionList == null)
                    {
                        return Json(new { isok = false, msg = "此功能未开启" }, JsonRequestBehavior.AllowGet);
                    
                    }
                    MessageMgr messageMgr = new MessageMgr();
                    if (!string.IsNullOrEmpty(functionList.MessageMgr))
                    {
                         messageMgr = JsonConvert.DeserializeObject<MessageMgr>(functionList.MessageMgr);
                    }
                    if (messageMgr.TemplateMessage == 1)
                    {
                        return Json(new { isok = false, msg = "请升级到更高版本才能开启此功能" }, JsonRequestBehavior.AllowGet);
                    }

                }

                string msg = "";
                TemplateMsg_User newUserMsg = TemplateMsg_UserBLL.SingleModel.getModelByAppId(app.AppId, PageType, miniapptemplatemsg.Id);
                if (newUserMsg == null)
                {
                    //往微信appid加模板消息
                    
                    addResultModel _addResult = MsnModelHelper.addMsnToMy(app.AppId, miniapptemplatemsg.TitileId, miniapptemplatemsg.ColNums.Split(','), ref msg);
                    log4net.LogHelper.WriteInfo(GetType(), JsonConvert.SerializeObject(_addResult));


                    if (_addResult.errcode != 0)
                    {
                        return Json(new { isok = false, msg = _addResult.errmsg , newUserMsg  = newUserMsg }, JsonRequestBehavior.AllowGet);
                    }

                    newUserMsg = new TemplateMsg_User();
                    newUserMsg.AppId = app.AppId;
                    //newUserMsg.TmpId = miAppfood.Id;
                    newUserMsg.TmpId = TmpId;
                    newUserMsg.Ttypeid = miniapptemplatemsg.Ttypeid;
                    newUserMsg.TmId = miniapptemplatemsg.Id;
                    newUserMsg.ColNums = miniapptemplatemsg.ColNums;
                    newUserMsg.TitleId = miniapptemplatemsg.TitileId;
                    newUserMsg.State = 1;//启用
                    newUserMsg.CreateDate = DateTime.Now;
                    newUserMsg.TemplateId = _addResult.template_id;//微信公众号内的模板Id
                    newUserMsg.TmgType = miniapptemplatemsg.TmgType;

                    int result = Convert.ToInt32(TemplateMsg_UserBLL.SingleModel.Add(newUserMsg));
                    //var result = Convert.ToInt32(_miniapptemplatemsg_userBll.startTemplate(newUserMsg));

                    if (result <= 0)
                    {
                        return Json(new { isok = false, msg = "启用失败" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    newUserMsg.State = 1;
                    bool isSuccess = TemplateMsg_UserBLL.SingleModel.Update(newUserMsg, "State");

                    if (!isSuccess)
                    {
                        return Json(new { isok = false, msg = "启用失败" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { isok = true, msg = "启用成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { isok = false, msg = "网络忙,请重试" }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 停用模板
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult stopTmg(int appId, int TempMsgId ,int PageType)
        {
            try
            {
                if (dzaccount == null)
                {
                    return Json(new { isok = false, msg = "系统繁忙auth_null！" }, JsonRequestBehavior.AllowGet);
                }
                XcxAppAccountRelation app = XcxAppAccountRelationBLL.SingleModel.GetModelByaccountidAndAppid(appId, dzaccount.Id.ToString());
                if (app == null)
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }
                if (app.AppId.IsNullOrWhiteSpace())
                {
                    return Json(new { isok = false, msg = "未授权！" }, JsonRequestBehavior.AllowGet);
                }
                Food miAppfood = FoodBLL.SingleModel.GetModel($"appId={appId}");
    
                int TmpId = 0;
                //获取当前小程序模板类型
                string typeSql = $" select type from xcxtemplate where id = {app.TId} ";
                int temp = Convert.ToInt32(DAL.Base.SqlMySql.ExecuteScalar(XcxAppAccountRelationBLL.SingleModel.connName, CommandType.Text, typeSql, null));

                if (temp == (int)TmpType.小程序电商模板 || temp == (int)TmpType.小程序电商模板测试)
                {
                    Store store = StoreBLL.SingleModel.GetModelByRid(app.Id) ?? new Store();
                    TmpId = store.Id;
                }
                else if (temp == (int)TmpType.小程序餐饮模板)
                {
                    Food store = FoodBLL.SingleModel.GetModel($" appId = {app.Id} ") ?? new Food();
                    TmpId = store.Id;
                }
                TemplateMsg miniapptemplatemsg = TemplateMsgBLL.SingleModel.GetModel(TempMsgId);
                if (miniapptemplatemsg == null)
                {
                    return Json(new { isok = false, msg = "系统内置的模板Id错误！" }, JsonRequestBehavior.AllowGet);
                }
                //string msg = "";
                TemplateMsg_User newUserMsg = TemplateMsg_UserBLL.SingleModel.getModelByAppId(app.AppId, PageType, miniapptemplatemsg.Id);
                if (newUserMsg == null)
                {
                     return Json(new { isok = false, msg = "未找到模板" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    newUserMsg.State = 0;//0为停用
                    bool isSuccess = TemplateMsg_UserBLL.SingleModel.Update(newUserMsg, "State");

                    //var isSuccess = _miniapptemplatemsg_userBll.stopTemplate(newUserMsg);
                    if (!isSuccess)
                    {
                        return Json(new { isok = false, msg = "停用失败" }, JsonRequestBehavior.AllowGet);
                    }
                    
                    //var _deleteResult = _msnModelHelper.deleteMyMsn(app.AppId, newUserMsg.TemplateId, ref msg);
                    //if (_deleteResult.errcode != 0)
                    //{
                    //    return Json(new { isok = false, msg = _deleteResult.errmsg }, JsonRequestBehavior.AllowGet);
                    //}
                }

                return Json(new { isok = true, msg = "停用成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception )
            {
                return Json(new { isok = false, msg = "网络忙,请重试" }, JsonRequestBehavior.AllowGet);
            }
        }
        

    }
}