using BLL.MiniApp;
using BLL.MiniApp.Im;
using BLL.MiniApp.Plat;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Im;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiImController : InheritController
    {
        public apiImController()
        {
        }
        /// <summary>
        /// 足浴版获取聊天列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetContactList()
        {
            int fuserType = Context.GetRequestInt("fuserType", 0);
            int fuserId = Context.GetRequestInt("fuserId", 0);
            if (fuserId <= 0)
            {
                return Json(new { isok = false, msg = "fuserid 为空" });
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 100);
            List<ImContact> imchatlist = ImContactBLL.SingleModel.GetListByFuserid(fuserId, fuserType, pageSize, pageIndex);
            if (imchatlist != null && imchatlist.Count > 0)
            {
                List<ImMessage> messageList = new List<ImMessage>();
                string fkey = string.Empty;
                string tkey = string.Empty; ;

            }
            imchatlist = imchatlist.OrderByDescending(imchat => imchat.newDate).ToList();
            return Json(new { isok = true, data = imchatlist });
        }

        /// <summary>
        /// 添加联系人
        /// </summary>
        /// <returns></returns>
        public ActionResult AddContact()
        {
            string appid = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid为空" });
            }
            int fuserId = Context.GetRequestInt("fuserId", 0);
            if (fuserId <= 0)
            {
                return Json(new { isok = false, msg = "fuserid 为空" });
            }
            int tuserId = Context.GetRequestInt("tuserId", 0);
            if (tuserId <= 0)
            {
                return Json(new { isok = false, msg = "tuserid为空" });
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" });
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(fuserId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" });
            }
            bool isok = ImContactBLL.SingleModel.CreateChat(fuserId, tuserId, 0, appid) && ImContactBLL.SingleModel.CreateChat(tuserId, fuserId, 1, appid);
            string msg = isok ? "添加成功" : "添加失败";
            return Json(new { isok = isok, msg = msg });
        }
        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetHistory()
        {
            string appId = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                return Json(new { isok = false, msg = "appId为空" });
            }
            int fuserId = Context.GetRequestInt("fuserId", 0);
            if (fuserId <= 0)
            {
                return Json(new { isok = false, msg = "fuserId 为空" });
            }
            int tuserId = Context.GetRequestInt("tuserId", 0);
            if (tuserId <= 0)
            {
                return Json(new { isok = false, msg = "tuserId为空" });
            }
            int fuserType = Context.GetRequestInt("fuserType", 0);
            int ver = Context.GetRequestInt("ver", 0);//版本号：0：技师端，1：通用
            int id = Context.GetRequestInt("id", 0);
            List<ImMessage> messageList = ImMessageBLL.SingleModel.GetHistory(id, fuserId, tuserId, fuserType, ver);

            //设为已读
            ImMessage readmodel = messageList?.FirstOrDefault(w => w.isRead == 0);
            if (readmodel != null)
            {
                ImMessageBLL.SingleModel.UpdateIMReadState(tuserId, fuserId);
            }

            return Json(new { isok = true, data = messageList, msg = messageList.Count });
        }

        public ActionResult HaveSixin(int tuserId = 0, int fuserId = 0)
        {
            Return_Msg_APP result = new Return_Msg_APP();
            //私信记录是否有发新消息
            PlatMyCardBLL.SingleModel.HaveSixin(tuserId, fuserId, true);
            result.isok = true;

            return Json(result);
        }

        /// <summary>
        /// 同步数据到数据库
        /// </summary>
        /// <returns></returns>
        public ActionResult AddMessageRecord()
        {
            string messagekeys = "messagekeys";
            List<string> list = RedisUtil.GetRange<string>(messagekeys);
            if (list.Count <= 0)
            {
                return Json(new { isok = true, msg = "没有缓存数据" });
            }
            List<ImMessage> msgList = new List<ImMessage>();

            foreach (string key in list)
            {
                msgList = new List<ImMessage>();
                msgList = RedisUtil.GetRange<ImMessage>(key);
                if (msgList.Count > 0)
                {


                    StringBuilder sb = new StringBuilder();
                    foreach (var msg in msgList)
                    {

                        msg.msg = msg.msg.Replace("\\", "\\\\");
                        msg.updateDate = msg.createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sb.Append(ImMessageBLL.SingleModel.BuildAddSql(msg));
                    }
                    try
                    {
                        ImMessageBLL.SingleModel.ExecuteNonQuery(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        log4net.LogHelper.WriteInfo(GetType(), $"私信错误：{sb.ToString()}{ex.Message}");
                    }
                    RedisUtil.RemoveItemListAll<string>(key);
                }
            }

            RedisUtil.RemoveItemListAll<string>(messagekeys);
            return Json(new { isok = "ok" });
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserInfo()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appid", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int userType = Context.GetRequestInt("userType", 0);//0 普通用户 1足浴技师  2商家
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation xcxRelatrion = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (xcxRelatrion == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            //Store storeInfo = _storeBll.GetModelByRid(xcxRelatrion.Id);
            //if (storeInfo == null)
            //{
            //    result.Msg = "门店信息错误";
            //    return Json(result);
            //}
            //try
            //{

            //    storeInfo.funJoinModel = JsonConvert.DeserializeObject<StoreConfigModel>(storeInfo.configJson) ?? new StoreConfigModel();//若为 null 则new一个新的配置
            //}
            //catch
            //{
            //    storeInfo.funJoinModel = new StoreConfigModel();
            //}
            if (userType == 0)//普通用户
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_UserId(appId, userId);
                if (userInfo != null)
                {
                    returnObj.isok = true;
                    returnObj.dataObj = userInfo;
                    // result.Msg = storeInfo.funJoinModel.helloWords;
                }
                else
                {
                    returnObj.Msg = "用户不存在";
                }
            }
            else if (userType == 2)//商家
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetKfInfo(xcxRelatrion.AppId);
                returnObj.isok = true;
                returnObj.dataObj = userInfo;
            }
            return Json(returnObj);
        }

        /// <summary>
        /// 专业版获取联系人列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetProContactList()
        {
            int fuserType = Context.GetRequestInt("fuserType", 0);
            int fuserId = Context.GetRequestInt("fuserId", 0);
            if (fuserId <= 0)
            {
                return Json(new { isok = false, msg = "fuserid 为空" });
            }
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 100);
            int ver = Context.GetRequestInt("ver", 0);
            List<ImContact> imchatlist = new List<ImContact>();
            imchatlist = ImContactBLL.SingleModel.GetListByFuserid(fuserId, fuserType, pageSize, pageIndex, ver);

            return Json(new { isok = true, data = imchatlist });
        }

        /// <summary>
        /// 专业版添加联系人
        /// </summary>
        /// <returns></returns>
        public ActionResult AddProContact()
        {
            string appid = Context.GetRequest("appId", string.Empty);
            if (string.IsNullOrEmpty(appid))
            {
                return Json(new { isok = false, msg = "appid为空" });
            }
            int fuserId = Context.GetRequestInt("fuserId", 0);
            if (fuserId <= 0)
            {
                return Json(new { isok = false, msg = "fuserid 为空" });
            }
            int tuserId = Context.GetRequestInt("tuserId", 0);
            if (tuserId <= 0)
            {
                return Json(new { isok = false, msg = "tuserid为空" });
            }
            XcxAppAccountRelation xcxAccountRelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (xcxAccountRelation == null)
            {
                return Json(new { isok = false, msg = "请先授权" });
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(fuserId);
            if (userInfo == null)
            {
                return Json(new { isok = false, msg = "用户不存在" });
            }
            bool isok = ImContactBLL.SingleModel.CreateChat(fuserId, tuserId, 0, appid) && ImContactBLL.SingleModel.CreateChat(tuserId, fuserId, 0, appid);
            string msg = isok ? "添加成功" : "添加失败";
            return Json(new { isok = isok, msg = msg });
        }

        public ActionResult GetConnectInfo()
        {
            returnObj = new Return_Msg_APP();
            int aid = Context.GetRequestInt("aid", 0);
            if (aid <= 0)
            {
                returnObj.Msg = "参数错误 aid error";
                return Json(returnObj);
            }
            XcxAppAccountRelation xcxAppAccountRelation = _xcxAppAccountRelationBLL.GetModelById(aid);
            if (xcxAppAccountRelation == null)
            {
                returnObj.Msg = "小程序不存在";
                return Json(returnObj);
            }
            string phone = Context.GetRequest("phone", string.Empty);
            if (string.IsNullOrEmpty(phone))
            {
                returnObj.Msg = "参数错误 phone error";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByTelephone_appid(phone, xcxAppAccountRelation.AppId);
            if (userInfo == null)
            {
                returnObj.isok = true;
                returnObj.dataObj = null;
                return Json(returnObj);
            }
            returnObj.isok = true;
            returnObj.dataObj = new { aid = xcxAppAccountRelation.Id, appId = xcxAppAccountRelation.AppId, userId = userInfo.Id };
            return Json(returnObj);
        }

    }
}