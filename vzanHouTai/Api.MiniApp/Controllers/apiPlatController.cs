using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Im;
using BLL.MiniApp.Plat;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Plat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    [AuthCheckLoginSessionKey]
    public partial class apiPlatController : InheritController
    {
        protected readonly object _lockobj = new object();
        public apiPlatController()
        {
        }

        /// <summary>
        /// 创建名片
        /// </summary>
        /// <returns></returns>
        public ActionResult AddMyCard()
        {
            returnObj = new Return_Msg_APP();
            string imgUrl = Context.GetRequest("imgurl", "");
            string companyName = Context.GetRequest("companyname", "");
            int userId = Context.GetRequestInt("userid", 0);
            int jobType = Context.GetRequestInt("jobtype", 0);
            string lat = Context.GetRequest("lat", "0");
            string lng = Context.GetRequest("lng", "0");
            string name = Context.GetRequest("name", "");
            string address = Context.GetRequest("address", "");
            string job = Context.GetRequest("job", "");
            string department = Context.GetRequest("department", "");
            string desc = Context.GetRequest("desc", "");
            int industryIid = Context.GetRequestInt("industryid", 0);
            int hiddenPhone = Context.GetRequestInt("hiddenphone", 0);
            int type = Context.GetRequestInt("type", 0);

            if (userId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }
            if (type == 1)
            {
                if (string.IsNullOrEmpty(lat))
                {
                    returnObj.Msg = "lat不能小于0";
                    return Json(returnObj);
                }
                if (string.IsNullOrEmpty(lng))
                {
                    returnObj.Msg = "lng不能为空";
                    return Json(returnObj);
                }
                if (string.IsNullOrEmpty(address))
                {
                    returnObj.Msg = "地址不能为空";
                    return Json(returnObj);
                }
                if (string.IsNullOrEmpty(name))
                {
                    returnObj.Msg = "姓名不能为空";
                    return Json(returnObj);
                }
                if (industryIid <= 0)
                {
                    returnObj.Msg = "请选择行业";
                    return Json(returnObj);
                }
            }

            C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userinfo == null)
            {
                returnObj.Msg = "用户已失效";
                return Json(returnObj);
            }
            XcxAppAccountRelation xcxrelatiion = _xcxAppAccountRelationBLL.GetModelByAppid(userinfo.appId);
            if (xcxrelatiion == null)
            {
                returnObj.Msg = "模板已失效";
                return Json(returnObj);
            }
            int aid = xcxrelatiion.Id;

            PlatMyCard platMyCardModel = PlatMyCardBLL.SingleModel.GetModelByUserId(userId, aid);

            //地址
            if (!string.IsNullOrEmpty(lng) && lng != "0" && !string.IsNullOrEmpty(lat) && lat != "0")
            {
                AddressApi addressinfo = AddressHelper.GetAddressByApi(lng, lat);
                if (addressinfo != null && addressinfo.result != null && addressinfo.result.address_component != null)
                {
                    string provinceName = addressinfo.result.address_component.province;
                    string cityName = addressinfo.result.address_component.city;
                    string countryName = addressinfo.result.address_component.district;
                    Dictionary<string, int> dic = C_AreaBLL.SingleModel.GetAddressCode(provinceName, cityName, countryName);
                    if (platMyCardModel == null)
                    {
                        platMyCardModel = new PlatMyCard();
                    }
                    platMyCardModel.ProvinceCode = dic["provincename"];
                    platMyCardModel.ProvinceCode = dic["cityname"];
                    platMyCardModel.ProvinceCode = dic["countryName"];
                }
            }
            imgUrl = string.IsNullOrEmpty(imgUrl) ? userinfo.HeadImgUrl : imgUrl;
            //获取服务器商的图片路径
            bool isreload = false;
            imgUrl = WxUploadHelper.GetMyServerImgUrl(imgUrl,ref isreload);

            if (platMyCardModel == null || platMyCardModel.Id <= 0)
            {
                platMyCardModel = new PlatMyCard();
                platMyCardModel.AddTime = DateTime.Now;
                platMyCardModel.Address = address;
                platMyCardModel.AId = aid;
                platMyCardModel.CompanyName = companyName;
                platMyCardModel.Desc = desc;
                platMyCardModel.ImgUrl = imgUrl;
                platMyCardModel.JobType = jobType;
                platMyCardModel.Lat = Convert.ToDouble(lat);
                platMyCardModel.Lng = Convert.ToDouble(lng);
                platMyCardModel.Name = name;
                platMyCardModel.NickName = userinfo.NickName;
                platMyCardModel.Phone = userinfo.TelePhone;
                platMyCardModel.UpdateTime = DateTime.Now;
                platMyCardModel.UserId = userId;
                platMyCardModel.Job = job;
                platMyCardModel.Department = department;
                platMyCardModel.IndustryId = industryIid;
                platMyCardModel.HiddenPhone = hiddenPhone;
                platMyCardModel.AppId = userinfo.appId;
                platMyCardModel.Id = Convert.ToInt32(PlatMyCardBLL.SingleModel.Add(platMyCardModel));
                returnObj.isok = platMyCardModel.Id > 0;
            }
            else
            {
                platMyCardModel.AddTime = DateTime.Now;
                platMyCardModel.Address = address;
                platMyCardModel.AId = aid;
                platMyCardModel.CompanyName = companyName;
                platMyCardModel.Desc = "";
                platMyCardModel.ImgUrl = imgUrl;
                platMyCardModel.JobType = jobType;
                platMyCardModel.Lat = Convert.ToDouble(lat);
                platMyCardModel.Lng = Convert.ToDouble(lng);
                platMyCardModel.Name = name;
                platMyCardModel.NickName = userinfo.NickName;
                platMyCardModel.Phone = userinfo.TelePhone;
                platMyCardModel.UpdateTime = DateTime.Now;
                platMyCardModel.UserId = userId;
                platMyCardModel.IndustryId = industryIid;
                platMyCardModel.HiddenPhone = hiddenPhone;
                if (!string.IsNullOrEmpty(job))
                {
                    platMyCardModel.Job = job;
                }
                if (!string.IsNullOrEmpty(department))
                {
                    platMyCardModel.Department = department;
                }
                if (!string.IsNullOrEmpty(companyName))
                {
                    platMyCardModel.CompanyName = companyName;
                }
                if (!string.IsNullOrEmpty(desc))
                {
                    platMyCardModel.Desc  = desc;
                }
                returnObj.isok = PlatMyCardBLL.SingleModel.Update(platMyCardModel);
            }
            returnObj.Msg = returnObj.isok ? "保存成功" : "保存失败";
            returnObj.dataObj = platMyCardModel;
            return Json(returnObj);
        }

        /// <summary>
        /// 行业数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIndustryList()
        {
            returnObj = new Return_Msg_APP();
            List<PlatIndustry> list = PlatIndustryBLL.SingleModel.GetListData();

            if (list == null || list.Count <= 0)
            {
                return Json(returnObj);
            }

            List<PlatIndustry> pList = list.Where(w => w.ParentId == 0).ToList();
            List<PlatIndustry> cList = list.Where(w => w.ParentId > 0).ToList();
            if (pList == null || pList.Count <= 0)
            {
                return Json(returnObj);
            }

            List<object> dataList = new List<object>();
            foreach (PlatIndustry pitem in pList)
            {
                List<PlatIndustry> itemlist = cList?.Where(w => w.ParentId == pitem.Id).ToList();
                if (itemlist != null && itemlist.Count > 0)
                {
                    object cdata = itemlist.Select(s => new { s.Id, s.Name, Children = new List<object>() });
                    object data = new { pitem.Id, pitem.Name, Children = cdata };
                    dataList.Add(data);
                }
                else
                {
                    object data = new { pitem.Id, pitem.Name, Children = new List<object>() };
                    dataList.Add(data);
                }
            }
            returnObj.dataObj = dataList;
            returnObj.isok = true;
            returnObj.Msg = returnObj.isok ? "成功" : "失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 区域数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAreaList()
        {
            returnObj = new Return_Msg_APP();
            List<C_Area> list = C_AreaBLL.SingleModel.GetAreaRedisData();

            if (list == null || list.Count <= 0)
            {
                return Json(returnObj);
            }

            List<C_Area> pList = list.Where(w => w.Level == 1).ToList();
            List<C_Area> cList = list.Where(w => w.Level == 2).ToList();
            if (pList == null || pList.Count <= 0)
            {
                return Json(returnObj);
            }

            List<object> dataList = new List<object>();
            foreach (C_Area pitem in pList)
            {
                List<C_Area> itemList = cList?.Where(w => w.ParentCode == pitem.Code).ToList();
                if (itemList != null && itemList.Count > 0)
                {
                    object cData = itemList.Select(s => new { s.Code, s.Name, Children = new List<object>() });
                    object data = new { pitem.Code, pitem.Name, Children = cData };
                    dataList.Add(data);
                }
                else
                {
                    object data = new { pitem.Code, pitem.Name, Children = new List<object>() };
                    dataList.Add(data);
                }
            }
            returnObj.dataObj = dataList;
            returnObj.isok = true;
            returnObj.Msg = returnObj.isok ? "成功" : "失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 获取名片
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMyCard()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int myuserId = Context.GetRequestInt("myuserid", 0);
            int aid = Context.GetRequestInt("aid", 0);

            if (userId <= 0 || myuserId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "aid不能小于0";
                return Json(returnObj);
            }

            PlatMyCard platMyCardModel = PlatMyCardBLL.SingleModel.GetMyCardData(userId, myuserId, aid, (int)PointsDataType.名片);
            if (platMyCardModel == null || platMyCardModel.Id<=0)
            {
                returnObj.Msg = "您还没有创建名片";
                returnObj.dataObj = platMyCardModel;
                return Json(returnObj);
            }
            //判断是否已保存appid
            if(string.IsNullOrEmpty(platMyCardModel.AppId))
            {
                C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(platMyCardModel.UserId);
                platMyCardModel.AppId = userinfo.appId;
                PlatMyCardBLL.SingleModel.Update(platMyCardModel,"appid");
            }

            bool isReload = false;
            //获取服务器商的图片路径
            platMyCardModel.ImgUrl = WxUploadHelper.GetMyServerImgUrl(platMyCardModel.ImgUrl,ref isReload);
            if(isReload)
            {
                PlatMyCardBLL.SingleModel.Update(platMyCardModel, "ImgUrl");
            }
            
            returnObj.dataObj = platMyCardModel;
            returnObj.isok = platMyCardModel.Id > 0;
            returnObj.Msg = returnObj.isok ? "成功" : "失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 点赞、关注、浏览、私信、收藏量
        /// </summary>
        /// <returns></returns>
        public ActionResult AddFavorite()
        {
            returnObj = new Return_Msg_APP();
            int actionType = Context.GetRequestInt("actiontype", (int)PointsActionType.收藏);
            int userId = Context.GetRequestInt("userid", 0);
            int otherCardId = Context.GetRequestInt("othercardid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            int dataType = Context.GetRequestInt("datatype", (int)PointsDataType.名片);
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }
            if (otherCardId <= 0)
            {
                returnObj.Msg = "名片ID不能小于0";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "aid不能小于0";
                return Json(returnObj);
            }
            int curState = 0;
            lock (_lockobj)
            {
                switch (dataType)
                {
                    case (int)PointsDataType.名片:
                        if (actionType == (int)PointsActionType.看过)
                        {
                            PlatMyCard platMycard = PlatMyCardBLL.SingleModel.GetModel(otherCardId);
                            if (platMycard == null)
                            {
                                returnObj.Msg = "名片已失效";
                                return Json(returnObj);
                            }
                            if (platMycard.UserId == userId)
                            {
                                returnObj.isok = true;
                                returnObj.Msg = "屏蔽自己的浏览";
                                return Json(returnObj);
                            }
                        }

                        break;
                }

                returnObj.Msg = PlatUserFavoriteMsgBLL.SingleModel.CommondFavoriteMsg(aid, otherCardId, userId, actionType, dataType, ref curState);
            }
            returnObj.isok = returnObj.Msg.ToString() == "";

            if ((int)PointsActionType.点赞 == actionType && dataType == 0)
            {
                //表示帖子的点赞需要返回该帖子的点赞列表
                int count = 0;
                returnObj.dataObj = new { curState = curState, dzList = PlatUserFavoriteMsgBLL.SingleModel.GetMsgUserFavoriteList(0, aid, otherCardId, (int)PointsActionType.点赞, (int)PointsDataType.帖子, 1, 1000, ref count) };
            }
            else
            {
                returnObj.dataObj = new { curState = curState };
            }
            return Json(returnObj);

        }

        /// <summary>
        /// 谁看过我，谁关注我，谁收藏我，谁点赞我列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOtherFavoriteList()
        {
            returnObj = new Return_Msg_APP();
            int actionType = Context.GetRequestInt("actiontype", (int)PointsActionType.收藏);
            int myCardId = Context.GetRequestInt("mycardid", 0);
            int userId = Context.GetRequestInt("userid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            int dataType = Context.GetRequestInt("datatype", (int)PointsDataType.名片);
            int pageIndex = Context.GetRequestInt("pageindex", 1);
            int pageSize = Context.GetRequestInt("pagesize", 10);
            int type = Context.GetRequestInt("type", 0);
            string lng = Context.GetRequest("lng", "0");
            string lat = Context.GetRequest("lat", "0");
            int ver = Context.GetRequestInt("ver", 0);
            if (myCardId <= 0)
            {
                returnObj.Msg = "我的名片ID不能小于0";
                return Json(returnObj);
            }
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "aid不能小于0";
                return Json(returnObj);
            }
            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModelByUserId(userId, aid);
            if (myCard != null)
            {
                lng = myCard.Lng.ToString();
                lat = myCard.Lat.ToString();
            }

            int count = 0;
            switch (dataType)
            {
                case (int)PointsDataType.名片:
                    if (actionType == (int)PointsActionType.私信)
                    {
                        List<PlatUserFavoriteMsg> myCardList = PlatUserFavoriteMsgBLL.SingleModel.GetMyCardUserFavoriteList(ver,myCard.Id,userId, aid, pageIndex, pageSize, ref count);
                        returnObj.dataObj = new { list = myCardList, count = count };
                    }
                    else
                    {
                        List<PlatUserFavoriteMsg> mycardlist = PlatUserFavoriteMsgBLL.SingleModel.GetMyCardUserFavoriteList(myCard.AppId,lng, lat, userId, type, aid, myCardId, actionType, dataType, pageIndex, pageSize, ref count);
                        returnObj.dataObj = new { list = mycardlist, count = count };
                    }

                    break;
                case (int)PointsDataType.帖子:
                    List<PlatUserFavoriteMsg> msgList = PlatUserFavoriteMsgBLL.SingleModel.GetMsgUserFavoriteList(type, aid, myCardId, actionType, dataType, pageIndex, pageSize, ref count);
                    returnObj.dataObj = new { list = msgList, count = count };
                    break;
                case (int)PointsDataType.店铺:
                    List<PlatStore> storeList = PlatUserFavoriteMsgBLL.SingleModel.GetListMyFavoriteStore(aid, userId, out count, pageSize, pageIndex);

                    
                    //表示没有传坐标 通过客户端IP获取经纬度
                    IPToPoint iPToPoint = new IPToPoint();
                    if (lng == "0" || lat == "0")
                    {
                        string IP = WebHelper.GetIP();

                        iPToPoint = CommondHelper.GetLoctionByIP(IP);
                        if (iPToPoint != null && iPToPoint.result != null)
                        {
                            lat = iPToPoint.result.location.lat.ToString();
                            lng = iPToPoint.result.location.lng.ToString();
                        }
                    }
                    double distance = 0.00;
                    storeList.ForEach(x =>
                    {
                        distance = CommondHelper.GetDistance(x.Lat, x.Lng, Convert.ToDouble(lat), Convert.ToDouble(lng));
                        if (distance < 1)
                        {
                            x.Distance = (distance * 1000).ToString() + "m";
                        }
                        else
                        {
                            x.Distance = distance.ToString() + "km";
                        }
                    });

                    returnObj.dataObj = new { list = storeList, count = count };
                    break;

            }
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 人脉圈
        /// </summary>
        /// <returns></returns>
        public ActionResult GetConnectionsList()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            int distance = Context.GetRequestInt("distance", -1);//-1默认，-2：同城，大于0：附近人
            int actionType = Context.GetRequestInt("actiontype", -1);
            int areaCode = Context.GetRequestInt("areacode", 0);
            int industryId = Context.GetRequestInt("industryid", 0);
            int pageIndex = Context.GetRequestInt("pageindex", 1);
            int pageSize = Context.GetRequestInt("pagesize", 10);
            string lng = Context.GetRequest("lng", "0");
            string lat = Context.GetRequest("lat", "0");
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "aid不能小于0";
                return Json(returnObj);
            }

            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if(userInfo==null)
            {
                returnObj.Msg = "用户信息无效";
                return Json(returnObj);
            }

            int cityCode = 0;
            int count = 0;

            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModelByUserId(userId, aid);
            if (myCard != null)
            {
                lng = myCard.Lng.ToString();
                lat = myCard.Lat.ToString();
                cityCode = myCard.CityCode;
            }
            lat = lat == "undefined" ? "0" : lat;
            lng = lng == "undefined" ? "0" : lng;

            List<PlatMyCard> list = PlatMyCardBLL.SingleModel.GetCardApiList(userInfo.appId,industryId, areaCode, lng, lat, cityCode, distance, userId, aid, actionType, ref count, pageSize, pageIndex);

            returnObj.dataObj = new { list = list, count = count };
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 用户活动轨迹
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActivityLog()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int pageIndex = Context.GetRequestInt("pageindex", 1);
            int pageSize = Context.GetRequestInt("pagesize", 10);
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能小于0";
                return Json(returnObj);
            }

            int count = 0;
            List<PlatActivityTrajectory> list = PlatActivityTrajectoryBLL.SingleModel.GetListByMyUserId(userId, pageIndex, pageSize, ref count);

            returnObj.isok = true;
            returnObj.dataObj = new { list = list, count = count };

            return Json(returnObj);
        }

        /// <summary>
        /// 数据雷达
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRadarData()
        {
            PlatRadarReportModel radar = new PlatRadarReportModel();
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            int type = Context.GetRequestInt("type", 0);//-1:统计，0：本月，1：上个月
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能为0";
                return Json(returnObj);
            }
            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModelByUserId(userId);
            if (myCard == null)
            {
                returnObj.Msg = "名片已过期";
                return Json(returnObj);
            }

            string startTime = "";
            string endTime = "";
            switch (type)
            {
                case -1: break;
                case 0:
                    startTime = DateTime.Now.ToString("yyyy-MM") + "-1";
                    endTime = DateTime.Now.AddMonths(1).ToString("yyyy-MM") + "-1";
                    break;
                case 1:
                    startTime = DateTime.Now.AddMonths(1).ToString("yyyy-MM") + "-1";
                    endTime = DateTime.Now.AddMonths(2).ToString("yyyy-MM") + "-1";
                    break;
            }

            #region 名片数据分析
            //浏览量
            //点赞量
            //关注量
            //与多少人私信
            //转发量
            PlatMsgviewFavoriteShare reportData = PlatMsgviewFavoriteShareBLL.SingleModel.GetModelByMsgId(aid, myCard.Id, (int)PointsDataType.名片);
            radar.MyCardViewCount = myCard.FictitiousCount;
            if (reportData!=null)
            {
                radar.MyCardDzCount = reportData.DzCount;
                radar.MyCardViewCount += reportData.ViewCount;
                radar.MyCardFollowCount = reportData.FollowCount;
                radar.MyCardShareCount = reportData.ShareCount;
            }
            radar.MyCardSiXinCount = ImMessageBLL.SingleModel.GetCountByTuesrId(userId);

            //访客人数
            int visitorCount = PlatUserFavoriteMsgBLL.SingleModel.GetVisitorCount(myCard.Id, myCard.AId, (int)PointsDataType.名片, startTime, endTime);
            radar.MyCardVisitorCount = visitorCount;
            #endregion

            #region 人脉关系统计
            //已关注人数
            int myFavoriteCount = PlatUserFavoriteMsgBLL.SingleModel.GetUserMsgCount(aid, 0, (int)PointsActionType.关注, (int)PointsDataType.名片, userId);
            radar.FollowCount = myFavoriteCount;

            //粉丝人数
            int fanCount = PlatUserFavoriteMsgBLL.SingleModel.GetUserMsgCount(aid, myCard.Id, (int)PointsActionType.关注, (int)PointsDataType.名片, 0);
            radar.FanCount = fanCount;

            //相互关注人数
            radar.MutualFollowCount = PlatUserFavoriteMsgBLL.SingleModel.GetMutualFollowCount(aid,userId);
            #endregion

            #region 平台入驻店铺数据统计
            PlatStore storeModel = PlatStoreBLL.SingleModel.GetPlatStore(myCard.Id, 1);
            if (storeModel != null)
            {
                //浏览量
                radar.StoreViewCount = storeModel.StorePV + storeModel.StoreVirtualPV;
                //收藏数
                List<PlatUserFavoriteMsg> storelist = PlatUserFavoriteMsgBLL.SingleModel.GetReportData(storeModel.Id, myCard.AId, (int)PointsDataType.店铺, startTime, endTime);
                if (storelist != null && storelist.Count > 0)
                {
                    PlatUserFavoriteMsg tempmodel = storelist.Where(w => w.ActionType == (int)PointsActionType.收藏).FirstOrDefault();
                    radar.StoreFavoriteCount = tempmodel == null ? 0 : tempmodel.Count;
                }

                //访客人数
                int storeVisitorCount = PlatUserFavoriteMsgBLL.SingleModel.GetVisitorCount(storeModel.Id, myCard.AId, (int)PointsDataType.店铺, startTime, endTime);
                radar.StoreVisitorCount = storeVisitorCount;
            }
            #endregion

            returnObj.isok = true;
            returnObj.dataObj = radar;
            return Json(returnObj);
        }

        /// <summary>
        /// 获取名片码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMyCardCodeUrl()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            string pageUrl = Context.GetRequest("pageurl", "");
            if (userId <= 0)
            {
                returnObj.Msg = "userid不能为空";
                return Json(returnObj);
            }
            if (aid <= 0)
            {
                returnObj.Msg = "aid不能为空";
                return Json(returnObj);
            }
            if (string.IsNullOrEmpty(pageUrl))
            {
                returnObj.Msg = "名片路径不能为空";
                return Json(returnObj);
            }

            PlatMyCard myCard = PlatMyCardBLL.SingleModel.GetModelByUserId(userId);
            if (myCard == null)
            {
                returnObj.Msg = "名片过期";
                return Json(returnObj);
            }
            //判断是否已有名片码
            if (!string.IsNullOrEmpty(myCard.QrCodeImgUrl))
            {
                returnObj.dataObj = myCard.QrCodeImgUrl;
                returnObj.isok = true;
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModel(aid);
            if (xcxrelation == null)
            {
                returnObj.Msg = "模板过期";
                return Json(returnObj);
            }

            //XcxTemplate xcxTemplate = XcxTemplateBLL.SingleModel.GetModel(xcxrelation.TId);
            //if (xcxTemplate == null)
            //{
            //    returnObj.Msg = "无效模板";
            //    return Json(returnObj);
            //}

            string token = "";
            if (!XcxApiBLL.SingleModel.GetToken(xcxrelation, ref token))
            {
                returnObj.Msg = token;
                return Json(returnObj);
            }

            string scen = $"{userId}";
            qrcodeclass qrCodeModel = CommondHelper.GetMiniAppQrcode(token, pageUrl, scen);
            if (qrCodeModel == null || string.IsNullOrEmpty(qrCodeModel.url))
            {
                returnObj.Msg = qrCodeModel==null?"生成名片码失败": qrCodeModel.msg+"失败";
                return Json(returnObj);
            }

            myCard.QrCodeImgUrl = qrCodeModel.url;
            myCard.UpdateTime = DateTime.Now;
            PlatMyCardBLL.SingleModel.Update(myCard, "QrCodeImgUrl,UpdateTime");
            returnObj.dataObj = myCard.QrCodeImgUrl;
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 检测名片是否被禁用
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult CheckCardState(int userId=0)
        {
            returnObj = new Return_Msg_APP();
            if(userId<=0)
            {
                returnObj.Msg = "无效的用户参数";
                return Json(returnObj);
            }

            PlatMyCard model = PlatMyCardBLL.SingleModel.GetModelByUserId(userId);
            if(model==null)
            {
                returnObj.isok = true;
                returnObj.Msg = "无效的名片";
                returnObj.isok = true;
                return Json(returnObj);
            }
            if(model.State==-1)
            {
                returnObj.Msg = "该名片信息违反平台规则，已被删除";
                returnObj.dataObj = false;
                return Json(returnObj);
            }

            returnObj.isok = true;
            returnObj.dataObj = true;
            return Json(returnObj);
        }
    }
}