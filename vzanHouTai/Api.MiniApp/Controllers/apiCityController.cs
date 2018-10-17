using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.cityminiapp;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.cityminiapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 小程序-同城模板
    /// </summary>
    public class apiCityController : InheritController
    {
        private readonly string CITY_BINDPHONE = "CITY_BINDPHONE_{0}";//同城短信验证码绑定手机
        
        /// <summary>
        /// 获取同城店铺 分类以及轮播图配置
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getCitySetting()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int groupPageSize = Context.GetRequestInt("groupPageSize", 8);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            #region 轮播图
            CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(r.Id);
            if (storebanner == null)
            {
                //新增一条 初始化
                storebanner = new CityStoreBanner()
                {
                    aid = r.Id,
                    addTime = DateTime.Now,
                    updateTime = DateTime.Now,
                    ReviewSetting = 0
                };

                int id = Convert.ToInt32(CityStoreBannerBLL.SingleModel.Add(storebanner));
                if (id <= 0)
                {
                    returnObj.Msg = "初始化数据失败";
                    return Json(returnObj, JsonRequestBehavior.AllowGet);
                }
            }
            
            List<string> listBanners = null;
            List<string> listMsgs = null;
            if (!string.IsNullOrEmpty(storebanner.banners))
            {
                listBanners = storebanner.banners.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                listMsgs = storebanner.MsgIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        
            #endregion

            //公告
            var remarkObj = new { haveRemark = false, remark = string.Empty, remarkOpenFrm = false };
            if (!string.IsNullOrEmpty(storebanner.Remark))
            {
                remarkObj = new { haveRemark = true, remark = storebanner.Remark, remarkOpenFrm = storebanner.RemarkOpenFrm == 0 };
            }
            
            #region 分类配置
            int totalCount = 0;//总条数
            List<CityStoreMsgType> list = CityStoreMsgTypeBLL.SingleModel.getListByaid(r.Id, out totalCount, 1000, 1, string.Empty);

            int groupCount = Convert.ToInt32(Math.Ceiling((double)totalCount / groupPageSize));

            List<List<CityStoreMsgType>> listCity_StoreMsgTypes = new List<List<CityStoreMsgType>>();
            for (int i = 1; i <= groupCount; i++)
            {
                List<CityStoreMsgType> listItem = new List<CityStoreMsgType>();
                listItem.AddRange(list.Skip((i - 1) * groupPageSize).Take(groupPageSize));
                listCity_StoreMsgTypes.Add(listItem);
            }
            #endregion

            string reportReason = WebSiteConfig.City_ReportReasonTemplate;
            List<string> listReportReason = null;

            if (!string.IsNullOrEmpty(reportReason))
            {
                listReportReason = reportReason.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            storebanner.PV++;
            CityStoreBannerBLL.SingleModel.Update(storebanner, "PV");


            int totalPV = storebanner.VirtualPV + storebanner.PV;
            int totalMsgCout = storebanner.VirtualMsgCount + CityMsgBLL.SingleModel.GetCountByAId(r.Id);


            returnObj.isok = true;
            returnObj.code = "200";
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new
            {
                remarkObj = remarkObj,
                storebanner = listBanners,
                bannerToMsgs=listMsgs,
                listReportReason = listReportReason,
                listCity_StoreMsgTypes = listCity_StoreMsgTypes,
                groupCount = groupCount,
                totalCount = totalCount,
                TotalPV = totalPV > 100000 ? Convert.ToInt32(totalPV * 0.0001).ToString("0.0") + "万+" : totalPV.ToString(),
                TotalMsgCount = totalMsgCout > 100000 ? Convert.ToInt32(totalMsgCout * 0.0001).ToString("0.0") + "万+" : totalMsgCout.ToString()
            };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 获取商家审核设置模式 小程序发帖时请求
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getCityReviewSetting()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            
            #region 轮播图
            CityStoreBanner storebanner = CityStoreBannerBLL.SingleModel.getModelByaid(r.Id);

            if (storebanner == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "商家未进行配置";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            #endregion
            
            returnObj.isok = true;
            returnObj.code = "200";
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { ReviewSetting = storebanner.ReviewSetting };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
   
        /// <summary>
        /// 更新 帖子的 浏览量 点赞 收藏数 分享量 以及新增用户的收藏帖子
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddMsgViewFavoriteShare()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int msgId = Context.GetRequestInt("msgId", 0);
            int userId = Context.GetRequestInt("userId", 0);
            int actionType = Context.GetRequestInt("actionType", 0);//默认为0 表示浏览量 1表示收藏 2 表示分享成功 3表示点赞
            if (string.IsNullOrEmpty(appId) || msgId <= 0)
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }
            C_UserInfo userInfo = new C_UserInfo();
            if (actionType == 3 || actionType == 1)
            {
                if (userId <= 0)
                {
                    returnObj.code = "200";
                    returnObj.Msg = "请先授权";
                    return Json(returnObj);
                }
                userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
                if (userInfo == null)
                {
                    returnObj.code = "200";
                    returnObj.Msg = "登录信息过期";
                    return Json(returnObj);
                }
            }

            CityMsg cityMsg = CityMsgBLL.SingleModel.GetModel(msgId);
            if (cityMsg == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "帖子不存在";
                return Json(returnObj);
            }
            string msgTypeName = string.Empty;
            CityStoreMsgType cityStoreMsgType = CityStoreMsgTypeBLL.SingleModel.GetModel(cityMsg.msgTypeId);
            if (cityStoreMsgType != null)
            {
                msgTypeName = cityStoreMsgType.name;
            }

            CityMsgViewFavoriteShare mode = CityMsgViewFavoriteShareBLL.SingleModel.getModelByMsgId(msgId);

            TransactionModel tranModel = new TransactionModel();
            if (mode == null)
            {

                //新增一条数据
                mode = new CityMsgViewFavoriteShare();
                switch (actionType)
                {
                    case 1://表示收藏

                        mode.FavoriteCount = 1;
                        if (CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, msgId, userId) == null)
                        {
                            tranModel.Add(CityUserFavoriteMsgBLL.SingleModel.BuildAddSql(new CityUserFavoriteMsg()
                            {//用户-帖子收藏记录
                                aid = r.Id,
                                userId = userId,
                                msgId = msgId,
                                addTime = DateTime.Now
                            }));
                        }
                        break;
                    case 2://表示分享成功
                        mode.ShareCount = 1;
                        break;
                    case 3://表示点赞成功
                        if (CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, msgId, userId, 1) == null)
                        {
                            mode.DzCount = 1;
                            CityUserMsg city_UserMsg = new CityUserMsg();
                            city_UserMsg.addTime = DateTime.Now;
                            city_UserMsg.aid = r.Id;
                            city_UserMsg.fromUserId = userId;
                            city_UserMsg.toUserId = cityMsg.userId;
                            city_UserMsg.updateTime = DateTime.Now;
                            city_UserMsg.msgBody = string.Format(WebSiteConfig.City_UserDzMsgTemplate, userInfo.NickName, $"'#{msgTypeName}#{(cityMsg.msgDetail.Length > 10 ? cityMsg.msgDetail.Substring(0, 10) : cityMsg.msgDetail)}'");
                            city_UserMsg.msgType = 1;
                            tranModel.Add(CityUserMsgBLL.SingleModel.BuildAddSql(city_UserMsg));

                            tranModel.Add(CityUserFavoriteMsgBLL.SingleModel.BuildAddSql(new CityUserFavoriteMsg()
                            {//用户-帖子点赞记录
                                aid = r.Id,
                                userId = userId,
                                msgId = msgId,
                                addTime = DateTime.Now,
                                actionType = 1
                            }));
                        }
                        break;
                    default://默认为0 表示浏览量
                        mode.ViewCount = 1;
                        break;
                }

                mode.msgId = msgId;
                mode.addTime = DateTime.Now;
                mode.aid = r.Id;
                tranModel.Add(CityMsgViewFavoriteShareBLL.SingleModel.BuildAddSql(mode));
                
                if (tranModel.sqlArray == null || tranModel.sqlArray.Length <= 0 || !CityMsgViewFavoriteShareBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    returnObj.code = "200";
                    returnObj.Msg = "新增失败";
                    return Json(returnObj);
                }

                returnObj.isok = true;
                returnObj.code = "200";
                returnObj.Msg = "新增成功";
                return Json(returnObj);
            }
            else
            {
                string updateFiled = "ViewCount";
                //更新
                switch (actionType)
                {
                    case 1://表示收藏

                        if (CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, msgId, userId) == null)
                        {
                            mode.FavoriteCount += 1;
                            updateFiled = "FavoriteCount";
                            tranModel.Add(CityUserFavoriteMsgBLL.SingleModel.BuildAddSql(new CityUserFavoriteMsg()
                            {//用户-帖子收藏记录
                                aid = r.Id,
                                userId = userId,
                                msgId = msgId,
                                addTime = DateTime.Now
                            }));
                        }
                        break;
                    case 2://表示分享成功
                        mode.ShareCount += 1;
                        updateFiled = "ShareCount";
                        break;
                    case 3://表示点赞成功

                        if (CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, msgId, userId, 1) == null)
                        {
                            mode.DzCount += 1;
                            updateFiled = "DzCount";
                            //发消息
                            CityUserMsg city_UserMsg = new CityUserMsg();
                            city_UserMsg.addTime = DateTime.Now;
                            city_UserMsg.aid = r.Id;
                            city_UserMsg.fromUserId = userId;
                            city_UserMsg.toUserId = cityMsg.userId;
                            city_UserMsg.updateTime = DateTime.Now;
                            city_UserMsg.msgBody = string.Format(WebSiteConfig.City_UserDzMsgTemplate, userInfo.NickName, $"'#{msgTypeName}#{(cityMsg.msgDetail.Length > 10 ? cityMsg.msgDetail.Substring(0, 10) : cityMsg.msgDetail)}'");
                            city_UserMsg.msgType = 1;
                            tranModel.Add(CityUserMsgBLL.SingleModel.BuildAddSql(city_UserMsg));
                            
                            tranModel.Add(CityUserFavoriteMsgBLL.SingleModel.BuildAddSql(new CityUserFavoriteMsg()
                            {//用户-帖子点赞记录
                                aid = r.Id,
                                userId = userId,
                                msgId = msgId,
                                addTime = DateTime.Now,
                                actionType = 1
                            }));
                        }

                        break;
                    default://默认为0 表示浏览量
                        mode.ViewCount += 1;
                        break;
                }
                
                tranModel.Add(CityMsgViewFavoriteShareBLL.SingleModel.BuildUpdateSql(mode, updateFiled));
                
                if (tranModel.sqlArray == null || tranModel.sqlArray.Length <= 0 || !CityMsgViewFavoriteShareBLL.SingleModel.ExecuteTransactionDataCorect(tranModel.sqlArray))
                {
                    returnObj.code = "200";
                    returnObj.Msg = "更新失败";
                    return Json(returnObj);
                }

                returnObj.isok = true;
                returnObj.code = "200";
                returnObj.Msg = "更新成功";
                return Json(returnObj);
            }
        }


        /// <summary>
        ///用户举报帖子
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult AddReportMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int msgId = Context.GetRequestInt("msgId", 0);
            int userId = Context.GetRequestInt("userId", 0);
            string reportReason = Context.GetRequest("reportReason", string.Empty);
            if (string.IsNullOrEmpty(appId) || msgId <= 0 || userId <= 0 || string.IsNullOrEmpty(reportReason))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }
            CityMsgReport city_MsgReport = new CityMsgReport();
            city_MsgReport.addTime = DateTime.Now;
            city_MsgReport.aid = r.Id;
            city_MsgReport.msgId = msgId;
            city_MsgReport.reportReason = HttpUtility.HtmlEncode(reportReason);//编码保存
            city_MsgReport.reportUserId = userId;

            int id = Convert.ToInt32(CityMsgReportBLL.SingleModel.Add(city_MsgReport));
            if (id <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.Msg = "举报成功";
            returnObj.dataObj = new { id = id };
            return Json(returnObj);
        }
        
        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getMsgCode()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0 || string.IsNullOrEmpty(phone))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            phone = phone.Trim();
            //校验手机号码  
            if (!Regex.IsMatch(phone, @"\d") || phone.Length > 11)
            {
                returnObj.Msg = "请填写合法的手机号码";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            SendMsgHelper sendMsgHelper = new SendMsgHelper();
            string codeKey = string.Format(CITY_BINDPHONE, phone);
            string authCode = RedisUtil.Get<string>(codeKey);
            if (string.IsNullOrEmpty(authCode))
                authCode = Utility.EncodeHelper.CreateRandomCode(4);//生成4位数验证码

            bool senMsgCodeResult = sendMsgHelper.AliSend(phone, "{\"code\":\"" + authCode + "\",\"product\":\" " + Enum.GetName(typeof(SendTypeEnum), 8) + "\"}", "小未科技", 401);
            if (senMsgCodeResult)//表示发送成功
            {
                RedisUtil.Set<string>(codeKey, authCode, TimeSpan.FromMinutes(1));
                returnObj.dataObj = new { authCode = authCode };
                returnObj.isok = true;
                returnObj.Msg = "验证码发送成功！";
            }
            else
            {
                returnObj.Msg = "验证码发送失败,请稍后再试！";
            }

            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 保存或者更新用户的手机号码
        /// 默认为0 表示微信授权获取到号码提交过来保存号码 1表示短信验证码过来绑定号码
        /// 短信验证码绑定号码 需要带上submitAuthCode 验证码来验证 如果是保存则不用提交过来
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult saveUserPhone()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            string phone = Context.GetRequest("phone", string.Empty);
            string submitAuthCode = Context.GetRequest("submitAuthCode", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int actionType = Context.GetRequestInt("actionType", 0);//默认为0 表示微信授权获取到号码提交过来保存号码 1表示短信验证码过来绑定号码
            if (string.IsNullOrEmpty(appId) || userId <= 0 || string.IsNullOrEmpty(phone))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }

            phone = phone.Trim();
            //校验手机号码  
            if (!Regex.IsMatch(phone, @"\d") || phone.Length > 11)
            {
                returnObj.Msg = "请填写合法的手机号码";
                return Json(returnObj);
            }

            if (actionType == 1)
            {
                //表示短信验证码过来绑定号码 需要验证短信验证码是否正确
                string codeKey = string.Format(CITY_BINDPHONE, phone);
                string authCode = RedisUtil.Get<string>(codeKey);//获取缓存里的验证码与提交过来的验证码对比
                if (string.IsNullOrEmpty(authCode))
                {
                    returnObj.Msg = "验证码失效,请重新获取";
                    return Json(returnObj);
                }
                if (string.IsNullOrEmpty(submitAuthCode))
                {
                    returnObj.Msg = "验证码不能为空!";
                    return Json(returnObj);
                }

                if (authCode != submitAuthCode)
                {
                    returnObj.Msg = "验证码错误";
                    return Json(returnObj);
                }
            }

            CityStoreUser _city_StoreUser = CityStoreUserBLL.SingleModel.getCity_StoreUser(r.Id, userId);
            if (_city_StoreUser == null)
            {
                _city_StoreUser = new CityStoreUser();
                //新增一条
                _city_StoreUser.userId = userId;
                _city_StoreUser.addTime = DateTime.Now;
                _city_StoreUser.aid = r.Id;
                _city_StoreUser.phone = phone;
                _city_StoreUser.updateTime = DateTime.Now;
                int id = Convert.ToInt32(CityStoreUserBLL.SingleModel.Add(_city_StoreUser));
                if (id <= 0)
                {
                    returnObj.Msg = "绑定失败";
                    return Json(returnObj);
                }

                returnObj.isok = true;
                returnObj.Msg = "绑定成功";
                return Json(returnObj);
            }
            else
            {
                //更新手机号码
                _city_StoreUser.phone = phone;
                _city_StoreUser.updateTime = DateTime.Now;
                if (!CityStoreUserBLL.SingleModel.Update(_city_StoreUser, "phone,updateTime"))
                {
                    returnObj.Msg = "更新失败";
                    return Json(returnObj);
                }

                returnObj.isok = true;
                returnObj.Msg = "更新成功";
                return Json(returnObj);
            }
        }


        /// <summary>
        /// 获取用户消息 未读数量
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getUnReadUserMsgCount()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int count = CityUserMsgBLL.SingleModel.GetCountByUserId(r.Id, userId, 0);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { count = count };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取用户帖子列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getMsgByUserId()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int orderType = Context.GetRequestInt("orderType", 0);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<CityMsg> list = CityMsgBLL.SingleModel.getListByUserId(r.Id, out totalCount, userId, pageSize, pageIndex, orderType);
            list.ForEach(x =>
            {
                CityUserFavoriteMsg _userFavoriteMsg = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, x.Id, userId, 1);
                x.isDzed = (_userFavoriteMsg != null);
                CityMsgReport city_MsgReport = CityMsgReportBLL.SingleModel.GetCityMsgReport(userId, x.Id);
                x.isReported = (city_MsgReport != null);
            });

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }



        /// <summary>
        /// 获取发布的帖子
        /// orderType默认为0 表示最新发布  1表示距离最近
        /// msgTypeId 默认为0 表示获取所有类别
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getMsgList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            int userId = Context.GetRequestInt("userId", 0);
            string appId = Context.GetRequest("appId", string.Empty);
            string keyMsg = Context.GetRequest("keyMsg", string.Empty);
            int orderType = Context.GetRequestInt("orderType", 0);//默认为0 表示最新发布  1表示距离最近
            int msgTypeId = Context.GetRequestInt("msgTypeId", 0);//默认为0 表示获取所有
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            string latStr = Context.GetRequest("lat", string.Empty);
            string lngStr = Context.GetRequest("lng", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int commentTotalCount = 0;
            int totalCount = 0;

            List<CityMsg> list = new List<CityMsg>();
            if (orderType == 1)
            {

                double lat = 0.00;
                double lng = 0.00;
                //表示按照距离最近的
                //表示没有传坐标 通过客户端IP获取经纬度
                // log4net.LogHelper.WriteInfo(this.GetType(), $"!!!!orderType={orderType};{lat},{lng}");
                if (!double.TryParse(latStr, out lat) || !double.TryParse(lngStr, out lng) || lat == 0 || lng == 0)
                {
                    string IP = WebHelper.GetIP();
                    
                    IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                    if (iPToPoint != null && iPToPoint.result != null)
                    {

                        lat = iPToPoint.result.location.lat;
                        lng = iPToPoint.result.location.lng;
                        //log4net.LogHelper.WriteInfo(this.GetType(), $"IP={IP};{lat},{lng}");
                    }
                }
                //log4net.LogHelper.WriteInfo(this.GetType(),$"orderType={orderType};{lat},{lng}" );
                list = CityMsgBLL.SingleModel.getListCity_Msg(r.Id, out totalCount, pageSize, pageIndex, msgTypeId, keyMsg, 1, lat, lng);
                list.ForEach(x =>
                {
                    x.distance = CityMsgBLL.SingleModel.GetDistance(Convert.ToDouble(x.lat), Convert.ToDouble(x.lng), lat, lng);
                    if (x.distance < 1)
                    {
                        x.distanceStr = (x.distance * 1000).ToString("0.00") + "m";
                    }
                    else
                    {
                        x.distanceStr = x.distance.ToString("0.00") + "km";
                    }
                    CityUserFavoriteMsg _userFavoriteMsg = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, x.Id, userId, 1);
                    x.isDzed = (_userFavoriteMsg != null);

                    CityMsgReport city_MsgReport = CityMsgReportBLL.SingleModel.GetCityMsgReport(userId, x.Id);
                    x.isReported = (city_MsgReport != null);


                    x.Comments = CityMsgCommentBLL.SingleModel.GetCityMsgComment(x.aid, out commentTotalCount, string.Empty, 1000, 1, x.Id);

                });


            }
            else
            {
                list = CityMsgBLL.SingleModel.getListCity_Msg(r.Id, out totalCount, pageSize, pageIndex, msgTypeId, keyMsg);

                list.ForEach(x =>
                {
                    CityUserFavoriteMsg _userFavoriteMsg = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, x.Id, userId, 1);
                    x.isDzed = (_userFavoriteMsg != null);
                    CityMsgReport city_MsgReport = CityMsgReportBLL.SingleModel.GetCityMsgReport(userId, x.Id);
                    x.isReported = (city_MsgReport != null);


                    x.Comments = CityMsgCommentBLL.SingleModel.GetCityMsgComment(x.aid, out commentTotalCount, string.Empty, 1000, 1, x.Id);

                });

            }

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 获取指定的某条帖子
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getMsgDetail()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int msgId = Context.GetRequestInt("msgId", 0);//帖子信息Id
            int userId = Context.GetRequestInt("userId", 0);//用户Id
            string latStr = Context.GetRequest("lat", string.Empty);
            string lngStr = Context.GetRequest("lng", string.Empty);
            if (string.IsNullOrEmpty(appId) || msgId <= 0)
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            CityMsg model = CityMsgBLL.SingleModel.getMsg(r.Id, msgId);
            if (model == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "找不到数据";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            double lat = 0.00;
            double lng = 0.00;
            //表示按照距离最近的
            //表示没有传坐标 通过客户端IP获取经纬度
            // log4net.LogHelper.WriteInfo(this.GetType(), $"!!!!orderType={orderType};{lat},{lng}");
            if (!double.TryParse(latStr, out lat) || !double.TryParse(lngStr, out lng) || lat <= 0 || lng <= 0)
            {
                string IP = WebHelper.GetIP();
                
                IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                if (iPToPoint != null)
                {

                    lat = iPToPoint.result.location.lat;
                    lng = iPToPoint.result.location.lng;
                    // log4net.LogHelper.WriteInfo(this.GetType(), $"IP={IP};{lat},{lng}");
                }
            }
            model.distance = CityMsgBLL.SingleModel.GetDistance(Convert.ToDouble(model.lat), Convert.ToDouble(model.lng), lat, lng);

            CityUserFavoriteMsg _userFavoriteMsg = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, msgId, userId);
            model.isFavorited = (_userFavoriteMsg != null);
            _userFavoriteMsg = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, msgId, userId, 1);
            model.isDzed = (_userFavoriteMsg != null);
            CityMsgReport city_MsgReport = CityMsgReportBLL.SingleModel.GetCityMsgReport(userId, msgId);
            model.isReported = (city_MsgReport != null);
            int commentTotalCount = 0;
            model.Comments = CityMsgCommentBLL.SingleModel.GetCityMsgComment(r.Id, out commentTotalCount, string.Empty, 1000, 1, model.Id);
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { msg = model };
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 获取指定用户的收藏帖子列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getListMyFavoriteMsg()
        {
            returnObj = new Return_Msg_APP();
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);//帖子信息Id
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.code = "200";
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.code = "200";
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<CityMsg> list = CityUserFavoriteMsgBLL.SingleModel.getListMyFavoriteMsg(r.Id, userId, out totalCount, pageSize, pageIndex);
            list.ForEach(x =>
            {
                CityUserFavoriteMsg _userFavoriteMsg = CityUserFavoriteMsgBLL.SingleModel.getCity_UserFavoriteMsg(r.Id, x.Id, userId, 1);
                x.isDzed = (_userFavoriteMsg != null);
                CityMsgReport city_MsgReport = CityMsgReportBLL.SingleModel.GetCityMsgReport(userId, x.Id);
                x.isReported = (city_MsgReport != null);

            });
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list.OrderByDescending(x => x.FavoriteId) };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取小程序 置顶规则
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getRuleList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<CityStoreMsgRules> list = CityStoreMsgRulesBLL.SingleModel.getListByaid(r.Id, out totalCount, pageSize, pageIndex);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取用户消息列表
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult getCityUserMsgList()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            int totalCount = 0;
            List<CityUserMsg> list = CityUserMsgBLL.SingleModel.getListByUserId(r.Id, userId, out totalCount, pageSize, pageIndex, appId);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = list };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除我收藏的帖子或者我发布的帖子
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult delMyFavoriteOrMyMsg()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            int delType = Context.GetRequestInt("delType", 0);//默认为0 表示删除我发布的帖子 1表示删除我收藏的帖子
            if (string.IsNullOrEmpty(appId) || userId <= 0 || Id <= 0)
            {

                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }

            bool delResult = false;
            // log4net.LogHelper.WriteInfo(this.GetType(), "删除" + $"delType={delType} ， userId={userId} ， aid={appId}，Id={Id}");
            if (delType == 0)
            {
                //删除我发布的帖子
                delResult = CityMsgBLL.SingleModel.delMsg(Id, userId, r.Id);
            }
            else
            {
                //  log4net.LogHelper.WriteInfo(this.GetType(), "删除收藏");
                delResult = CityUserFavoriteMsgBLL.SingleModel.delMolde(Id, userId, r.Id);
            }

            if (!delResult)
            {

                returnObj.Msg = "操作异常";
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.Msg = "删除成功";
            return Json(returnObj);
        }

        /// <summary>
        /// 记录进入小程序的用户
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult SaveCityStoreUser()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }
            CityStoreUser model = CityStoreUserBLL.SingleModel.getCity_StoreUser(r.Id, userId);
            int city_storeUserId = 0;
            if (model == null)
            {
                //新增一条记录
                city_storeUserId = Convert.ToInt32(CityStoreUserBLL.SingleModel.Add(new CityStoreUser()
                {
                    aid = r.Id,
                    userId = userId,
                    addTime = DateTime.Now,
                    updateTime = DateTime.Now,
                    state = 0

                }));
                if (city_storeUserId <= 0)
                {
                    returnObj.Msg = "操作异常";
                    return Json(returnObj);
                }
            }
            else
            {
                city_storeUserId = model.Id;
            }

            returnObj.isok = true;
            returnObj.Msg = "操作成功";
            returnObj.dataObj = new { id = city_storeUserId };
            return Json(returnObj);
        }


        /// <summary>
        /// 获取用户手机号码判断是否有绑定过
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetCityStoreUserPhone()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj);
            }
            CityStoreUser model = CityStoreUserBLL.SingleModel.getCity_StoreUser(r.Id, userId);
            string phone = string.Empty;
            if (model != null)
            {
                phone = model.phone;
            }
            returnObj.isok = true;
            returnObj.Msg = "操作成功";
            returnObj.dataObj = new { phone = phone };
            return Json(returnObj);
        }



        /// <summary>
        /// 新增评论
        /// </summary>
        /// <returns></returns>
        public ActionResult AddComment()
        {
            base.returnObj = new Return_Msg_APP();
            base.returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int Id = Context.GetRequestInt("Id", 0);
            string commentDetail = Context.GetRequest("commentDetail", string.Empty);
            if (string.IsNullOrEmpty(appId) || userId <= 0 || Id <= 0)
            {
                base.returnObj.Msg = "参数错误";
                return Json(base.returnObj);
            }

            if (string.IsNullOrEmpty(commentDetail))
            {
                base.returnObj.Msg = "评论详情不能为空";
                return Json(base.returnObj);
            }
            if (commentDetail.Length > 1000)
            {
                base.returnObj.Msg = "评论详情最大1000字符";
                return Json(base.returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                base.returnObj.Msg = "小程序未授权";
                return Json(base.returnObj);
            }

            //表示帖子的评论
            CityMsg cityMsg = CityMsgBLL.SingleModel.GetModel(Id);
            if (cityMsg == null || cityMsg.state == -1)
            {
                base.returnObj.Msg = "帖子不存在!";
                return Json(base.returnObj);
            }


            CityMsgComment cityMsgComment = new CityMsgComment();
            cityMsgComment.AId = r.Id;
            cityMsgComment.FromUserId = userId;
            cityMsgComment.CommentDetail = commentDetail;
            cityMsgComment.AddTime = DateTime.Now;
            cityMsgComment.ToUserId = cityMsg.userId;
            cityMsgComment.MsgId = cityMsg.Id;
            int commentTotalCount = 0;
            int commentId = Convert.ToInt32(CityMsgCommentBLL.SingleModel.Add(cityMsgComment));
            if (commentId > 0)
            {
                base.returnObj.dataObj = new { comments = CityMsgCommentBLL.SingleModel.GetCityMsgComment(r.Id, out commentTotalCount, string.Empty, 1000, 1, cityMsg.Id) };
                base.returnObj.isok = true;
                base.returnObj.Msg = "评论成功";
                return Json(base.returnObj);
            }
            else
            {
                base.returnObj.Msg = "评论失败";
                return Json(base.returnObj);
            }
        }

        /// <summary>
        /// 获取评论
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMsgComment()
        {
            base.returnObj = new Return_Msg_APP();
            base.returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int Id = Context.GetRequestInt("Id", 0);//指定某条帖子的 评论
            int userId = Context.GetRequestInt("userId", 0);//指定用户Id 表示获取我发出去的评论
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            string keyMsg = Context.GetRequest("keyMsg", string.Empty);
            if (string.IsNullOrEmpty(appId))
            {

                base.returnObj.Msg = "参数错误";
                return Json(base.returnObj, JsonRequestBehavior.AllowGet);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {

                base.returnObj.Msg = "小程序未授权";
                return Json(base.returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<CityMsgComment> listComment = CityMsgCommentBLL.SingleModel.GetCityMsgComment(r.Id, out totalCount, keyMsg, pageSize, pageIndex, Id, userId);

            base.returnObj.dataObj = new { totalCount = totalCount, list = listComment };
            base.returnObj.isok = true;
            base.returnObj.Msg = "获取成功";
            return Json(base.returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除某条评论
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteMsgComment()
        {
            base.returnObj = new Return_Msg_APP();
            base.returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int Id = Context.GetRequestInt("Id", 0);
            
            if (string.IsNullOrEmpty(appId) || Id <= 0)
            {
                base.returnObj.Msg = "参数错误";
                return Json(base.returnObj);
            }
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                base.returnObj.Msg = "小程序未授权";
                return Json(base.returnObj);
            }

            CityMsgComment cityMsgComment = CityMsgCommentBLL.SingleModel.GetModel(Id);
            if (cityMsgComment == null || cityMsgComment.AId != r.Id)
            {
                base.returnObj.Msg = "帖子评论不存在";
                return Json(base.returnObj);
            }

            cityMsgComment.State = -1;
            if (CityMsgCommentBLL.SingleModel.Update(cityMsgComment, "State"))
            {
                base.returnObj.isok = true;
                base.returnObj.Msg = "操作成功";
                return Json(base.returnObj);
            }
            else
            {
                base.returnObj.Msg = "操作失败";
                return Json(base.returnObj);
            }
        }
    }
}