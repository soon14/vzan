using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using Core.MiniApp;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 平台版小程序 店铺
    /// </summary>
    public partial class apiPlatController : InheritController
    {
        /// <summary>
        /// 店铺入驻
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddStore()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("myCardId", 0);
            int agentInfoId = Context.GetRequestInt("agentInfoId", 0);

            if (string.IsNullOrEmpty(appId) || userId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            Agentinfo agentinfo = AgentinfoBLL.SingleModel.GetModelByAccoundId(r.AccountId.ToString());
            //if (agentinfo == null)//本平台代理信息 
            //{
            //    returnObj.Msg = "入驻异常";
            //    return Json(returnObj, JsonRequestBehavior.AllowGet);
            //}

            string banners = Context.GetRequest("banners", string.Empty);
            if (string.IsNullOrEmpty(banners) || banners.Split(',').Length <= 0)
            {
                returnObj.Msg = "轮播图至少一张";
                return Json(returnObj);
            }
            string storeName = Context.GetRequest("storeName", string.Empty);
            if (string.IsNullOrEmpty(storeName) || storeName.Length >= 20)
            {
                returnObj.Msg = "店铺名称不能为空并且不能超过20字符";
                return Json(returnObj);
            }

            string lngStr = Context.GetRequest("lng", string.Empty);
            string latStr = Context.GetRequest("lat", string.Empty);
            double lng = 0.00;
            double lat = 0.00;
            if (!double.TryParse(lngStr, out lng) || !double.TryParse(latStr, out lat))
            {
                returnObj.Msg = "地址坐标错误";
                return Json(returnObj);
            }

            string location = Context.GetRequest("location", string.Empty);
            if (string.IsNullOrEmpty(location))
            {
                returnObj.Msg = "地址不能为空";
                return Json(returnObj);
            }

            string storeService = Context.GetRequest("storeService", string.Empty);
            //if (string.IsNullOrEmpty(storeService) || storeService.Split(',').Length <= 0)
            //{
            //    returnObj.Msg = "提供服务不能为空";
            //    return Json(returnObj);
            //}

            string openTime = Context.GetRequest("openTime", string.Empty);
            //if (string.IsNullOrEmpty(openTime))
            //{
            //    returnObj.Msg = "营业时间不能为空";
            //    return Json(returnObj);
            //}
            //TODO 需要验证手机+电话
            string phone = Context.GetRequest("phone", string.Empty);
            if (string.IsNullOrEmpty(phone))
            {
                returnObj.Msg = "请填写正确的号码";
                return Json(returnObj);
            }

            string businessDescription = Context.GetRequest("businessDescription", string.Empty);
            if (string.IsNullOrEmpty(businessDescription))
            {
                returnObj.Msg = "业务简述不能为空";
                return Json(returnObj);
            }

            string storeDescription = Context.GetRequest("storeDescription", string.Empty);
            string storeImgs = Context.GetRequest("storeImgs", string.Empty);
            int category = Context.GetRequestInt("category", 0);//店铺类别
            if (PlatStoreCategoryBLL.SingleModel.GetModel(category) == null)
            {
                returnObj.Msg = "类别选择错误!";
                return Json(returnObj);
            }

            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(userId, 1);
            if (platStore == null)
            {
                platStore = new PlatStore();
            }

            AddressApi addressinfo = AddressHelper.GetAddressByApi(lngStr, latStr);
            if (addressinfo == null || addressinfo.result == null || addressinfo.result.address_component == null)
            {
                returnObj.Msg = "地址出错!";
                return Json(returnObj);
            }

            if (!storeService.Contains("ServiceState"))
            {
                //表示旧版本的数据,设施服务给初始化新值
                List<StoreServiceModel> list = new List<StoreServiceModel>();
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "WIFI" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "停车位" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "支付宝支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "微信支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "刷卡支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "空调雅座" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "付费停车" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "接送服务" });
                storeService = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }


            string storeRemark = Context.GetRequest("storeRemark", string.Empty);
            platStore.StoreRemark = storeRemark;

            string provinceName = addressinfo.result.address_component.province;
            string cityName = addressinfo.result.address_component.city;
            string countryName = addressinfo.result.address_component.district;
            platStore.ProvinceCode = C_AreaBLL.SingleModel.GetCodeByName(provinceName);
            platStore.CityCode = C_AreaBLL.SingleModel.GetCodeByName(cityName);
            platStore.CountryCode = C_AreaBLL.SingleModel.GetCodeByName(countryName);
            platStore.AddTime = DateTime.Now;
            platStore.Banners = banners;
            platStore.BindPlatAid = r.Id;
            platStore.BusinessDescription = businessDescription;
            platStore.Category = category;
            platStore.Lat = lat;
            platStore.Lng = lng;
            platStore.Location = location;
            platStore.MyCardId = userId;
            platStore.Name = storeName;
            platStore.OpenTime = openTime;
            platStore.Phone = phone;
            platStore.StoreDescription = storeDescription;
            platStore.StoreImgs = storeImgs;
            platStore.StoreService = storeService;
            platStore.UpdateTime = DateTime.Now;
            TransactionModel TranModel = new TransactionModel();
            if (platStore.Id > 0)
            {
                //表示更新

                TranModel.Add(PlatStoreBLL.SingleModel.BuildUpdateSql(platStore));
                PlatStoreRelation platStoreRelation = PlatStoreRelationBLL.SingleModel.GetPlatStoreRelationOwner(r.Id, platStore.Id);
                if (platStoreRelation == null)
                {
                    returnObj.Msg = "更新异常(店铺关系找不到数据)";
                    return Json(returnObj);
                }
                platStoreRelation.Category = platStore.Category;
                platStoreRelation.UpdateTime = DateTime.Now;
                TranModel.Add(PlatStoreRelationBLL.SingleModel.BuildUpdateSql(platStoreRelation, "Category,UpdateTime"));

                //2.查找上级代理
                #region 同步代理商店铺 暂时先屏蔽
                //AgentDistributionRelation agentDistributionRelationFirst = new AgentDistributionRelationBLL().GetModel(agentinfo.id);
                //if (agentDistributionRelationFirst != null)
                //{
                //    Agentinfo agentinfoFirst = _agentinfoBll.GetModel(agentDistributionRelationFirst.ParentAgentId);
                //    if (agentinfoFirst != null)
                //    {
                //        XcxAppAccountRelation xcxAppAccountRelationFirst = _xcxappaccountrelationBll.GetModelByaccountidAndTid(agentinfoFirst.useraccountid, (int)TmpType.小未平台);
                //        if (xcxAppAccountRelationFirst != null)
                //        {
                //            PlatStoreRelation platStoreRelationFist = _platStoreRelationBLL.GetPlatStoreRelationOwner(xcxAppAccountRelationFirst.Id, platStore.Id, agentinfo.id);
                //            if (platStoreRelationFist != null)
                //            {
                //                platStoreRelationFist.Category = platStore.Category;
                //                platStoreRelationFist.UpdateTime = DateTime.Now;
                //                TranModel.Add(_platStoreRelationBLL.BuildUpdateSql(platStoreRelationFist, "Category,UpdateTime"));
                //            }


                //            //3.查找上级的上级代理
                //            AgentDistributionRelation agentDistributionRelationSecond = new AgentDistributionRelationBLL().GetModel(agentinfoFirst.id);
                //            if (agentDistributionRelationSecond != null)
                //            {
                //                Agentinfo agentinfoSecond = _agentinfoBll.GetModel(agentDistributionRelationSecond.ParentAgentId);
                //                if (agentinfoSecond != null)
                //                {
                //                    XcxAppAccountRelation xcxAppAccountRelationSecond = _xcxappaccountrelationBll.GetModelByaccountidAndTid(agentinfoSecond.useraccountid, (int)TmpType.小未平台);
                //                    if (xcxAppAccountRelationSecond != null)
                //                    {
                //                        PlatStoreRelation platStoreRelationSecond = _platStoreRelationBLL.GetPlatStoreRelationOwner(xcxAppAccountRelationSecond.Id, platStore.Id, agentinfo.id);

                //                        platStoreRelationSecond.Category = platStore.Category;
                //                        platStoreRelationSecond.UpdateTime = DateTime.Now;
                //                        TranModel.Add(_platStoreRelationBLL.BuildUpdateSql(platStoreRelationSecond, "Category,UpdateTime"));
                //                    }
                //                }
                //            }
                //        }
                //    }
                //} 
                #endregion


                if (TranModel.sqlArray != null && TranModel.sqlArray.Length > 0 && PlatStoreRelationBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray))
                {
                    returnObj.isok = true;
                    returnObj.Msg = "操作成功";
                    return Json(returnObj);
                }
                else
                {
                    returnObj.Msg = "更新失败";
                    return Json(returnObj);
                }
            }
            else
            {
                //平台入驻模式
                PlatStoreAddSetting platStoreAddSetting = PlatStoreAddSettingBLL.SingleModel.GetPlatStoreAddSetting(r.Id);
                int addWay = platStoreAddSetting != null ? platStoreAddSetting.AddWay : 0;
                PlatStoreAddRules rule = new PlatStoreAddRules();
                if (addWay == 1)
                {
                    platStore.State = -1;
                    int ruleId = Context.GetRequestInt("ruleId", 0);
                    rule = PlatStoreAddRulesBLL.SingleModel.getRule(r.Id, ruleId);
                    if (rule == null)
                    {
                        returnObj.Msg = "入驻套餐不存在!";
                        return Json(returnObj);
                    }

                    platStore.YearCount = rule.YearCount;
                    platStore.CostPrice = rule.CostPrice;

                }

                //表示入驻平台 需要将该入驻店铺同步到平台的1 2级代理商
                int storeId = Convert.ToInt32(PlatStoreBLL.SingleModel.Add(platStore));

                if (storeId > 0)
                {
                    #region 数据同步

                    //插入关系表  才算成功 店铺数据都是从关系表获取才算有效的
                    //1.先将入驻到平台的插入 
                    PlatStoreRelation platStoreRelation = new PlatStoreRelation();
                    platStoreRelation.StoreId = storeId;
                    platStoreRelation.State = 1;
                    platStoreRelation.FromType = 0;
                    platStoreRelation.Aid = r.Id;
                    platStoreRelation.Category = platStore.Category;
                    platStoreRelation.AgentId = agentinfo != null ? agentinfo.id : agentInfoId;
                    platStoreRelation.AddTime = DateTime.Now;
                    platStoreRelation.UpdateTime = DateTime.Now;
                    TranModel.Add(PlatStoreRelationBLL.SingleModel.BuildAddSql(platStoreRelation));
                    //2.查找上级代理
                    #region 暂时先屏蔽
                    //AgentDistributionRelation agentDistributionRelationFirst = new AgentDistributionRelationBLL().GetModel(agentinfo.id);
                    //if (agentDistributionRelationFirst != null)
                    //{
                    //    Agentinfo agentinfoFirst = _agentinfoBll.GetModel(agentDistributionRelationFirst.ParentAgentId);
                    //    if (agentinfoFirst != null)
                    //    {
                    //        XcxAppAccountRelation xcxAppAccountRelationFirst = _xcxappaccountrelationBll.GetModelByaccountidAndTid(agentinfoFirst.useraccountid, (int)TmpType.小未平台);
                    //        if (xcxAppAccountRelationFirst != null)
                    //        {
                    //            PlatStoreRelation platStoreRelationFist = new PlatStoreRelation();
                    //            platStoreRelationFist.StoreId = storeId;
                    //            platStoreRelationFist.State = 0;
                    //            platStoreRelationFist.FromType = 1;
                    //            platStoreRelationFist.Aid = xcxAppAccountRelationFirst.Id;
                    //            platStoreRelationFist.Category = platStore.Category;
                    //            platStoreRelationFist.AgentId = agentinfo.id;
                    //            platStoreRelationFist.AddTime = DateTime.Now;
                    //            platStoreRelationFist.UpdateTime = DateTime.Now;
                    //            TranModel.Add(_platStoreRelationBLL.BuildAddSql(platStoreRelationFist));

                    //            //3.查找上级的上级代理
                    //            AgentDistributionRelation agentDistributionRelationSecond = new AgentDistributionRelationBLL().GetModel(agentinfoFirst.id);
                    //            if (agentDistributionRelationSecond != null)
                    //            {
                    //                Agentinfo agentinfoSecond = _agentinfoBll.GetModel(agentDistributionRelationSecond.ParentAgentId);
                    //                if (agentinfoSecond != null)
                    //                {
                    //                    XcxAppAccountRelation xcxAppAccountRelationSecond = _xcxappaccountrelationBll.GetModelByaccountidAndTid(agentinfoSecond.useraccountid, (int)TmpType.小未平台);
                    //                    if (xcxAppAccountRelationSecond != null)
                    //                    {
                    //                        PlatStoreRelation platStoreRelationSecond = new PlatStoreRelation();
                    //                        platStoreRelationSecond.StoreId = storeId;
                    //                        platStoreRelationSecond.State = 0;
                    //                        platStoreRelationSecond.FromType = 1;
                    //                        platStoreRelationSecond.Aid = xcxAppAccountRelationSecond.Id;
                    //                        platStoreRelationSecond.Category = platStore.Category;
                    //                        platStoreRelationSecond.AgentId = agentinfo.id;
                    //                        platStoreRelationSecond.AddTime = DateTime.Now;
                    //                        platStoreRelationSecond.UpdateTime = DateTime.Now;
                    //                        TranModel.Add(_platStoreRelationBLL.BuildAddSql(platStoreRelationSecond));
                    //                    }

                    //                }
                    //            }


                    //        }
                    //    }
                    //} 
                    #endregion





                    #endregion
                    int orderid = 0;
                    if (addWay == 1)
                    {
                        CityMorders order = new CityMorders()
                        {
                            FuserId = storeId,
                            Fusername = storeName,
                            TuserId = 0,
                            OrderType = (int)ArticleTypeEnum.PlatAddStorePay,
                            ActionType = (int)miniAppBuyMode.微信支付,
                            Addtime = DateTime.Now,
                            Percent = 99,//不收取服务费
                            userip = WebHelper.GetIP(),
                            payment_status = 0,
                            Status = 0,
                            CitySubId = 0,//无分销,默认为0
                            PayRate = 1,
                            appid = appId,
                            Articleid = rule.Id,
                            CommentId = platStore.YearCount,
                            MinisnsId = r.Id,
                            payment_free = platStore.CostPrice,
                            ShowNote = $"平台版店铺入驻收费付款{platStore.CostPrice * 0.01}元"

                        };
                        string no = WxPayApi.GenerateOutTradeNo();
                        order.orderno = no;
                        order.trade_no = no;

                        orderid = Convert.ToInt32(_cityMordersBLL.Add(order));
                    }





                    if (TranModel.sqlArray != null && TranModel.sqlArray.Length > 0)
                    {
                        if (PlatStoreRelationBLL.SingleModel.ExecuteTransactionDataCorect(TranModel.sqlArray))
                        {
                            if (orderid <= 0 && addWay == 1)
                            {
                                returnObj.Msg = "入驻失败(订单生成失败)";
                                return Json(returnObj);
                            }

                            returnObj.dataObj = new { storeId = storeId, orderid = orderid };
                            returnObj.isok = true;
                            returnObj.Msg = "入驻成功";
                            return Json(returnObj);
                        }
                        else
                        {
                            platStore.State = -1;
                            platStore.UpdateTime = DateTime.Now;
                            PlatStoreBLL.SingleModel.Update(platStore, "State,UpdateTime");
                            returnObj.Msg = "入驻失败";
                            return Json(returnObj);
                        }
                    }
                    else
                    {
                        returnObj.dataObj = storeId;
                        returnObj.isok = true;
                        returnObj.Msg = "入驻成功";
                        return Json(returnObj);
                    }
                }
                else
                {

                    returnObj.Msg = "入驻失败(请联系客服)";
                    return Json(returnObj);
                }
            }
        }

        /// <summary>
        /// 获取平台店铺类别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreCategory()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 200);
            int isFirstType = Context.GetRequestInt("isFirstType", 1);
            int parentId = Context.GetRequestInt("parentId", 0);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<PlatStoreCategory> listPlatStoreCategory = PlatStoreCategoryBLL.SingleModel.getListByaid(r.Id, out totalCount, isFirstType, pageSize, pageIndex, "sortNumber desc,addTime desc", parentId);


            returnObj.dataObj = new
            {
                totalCount = totalCount,
                list = listPlatStoreCategory

            };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取平台指定店铺详情
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreDetail()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int userId = Context.GetRequestInt("userId", 0);
            int storeId = Context.GetRequestInt("storeId", 0);
            int type = Context.GetRequestInt("type", 0);
            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            List<string> listPlatMyCardIds = new List<string>();
           int myCardId = Context.GetRequestInt("myCardId", 0);
            int isStoreID = Context.GetRequestInt("isStoreID", 0);//如果是1 表示myCardId为店铺Id 这里是轮播图处理兼容旧数据
            XcxAppAccountRelation  r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                    returnObj.Msg = "小程序未授权";
                    return Json(returnObj, JsonRequestBehavior.AllowGet);
           
            }
            PlatStore platStore = new PlatStore();
            if (type == 1 && isStoreID == 0)
            {
                //表示从我的店铺名片进来
                if (myCardId <= 0)
                {
                    platStore = null;
                }
                else
                {
                    platStore = PlatStoreBLL.SingleModel.GetPlatStore(myCardId, type);
                }
            }
            else if (type == 2)
            {
                //表示独立小程序进来的
                platStore = PlatStoreBLL.SingleModel.GetPlatStore(r.Id, type);
                r = _xcxAppAccountRelationBLL.GetModel(platStore.BindPlatAid);//查找所属平台
                if (r!= null)
                {
                    listPlatMyCardIds = PlatMyCardBLL.SingleModel.GetCardIds(r.Id, r.AppId).Split(',').ToList();
                    if (!listPlatMyCardIds.Contains(platStore.MyCardId.ToString()))
                    {
                        returnObj.Msg = "店铺不存在(平台换绑了小程序,请重新入驻)";
                        return Json(returnObj, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            else
            {
                //平台轮播图 置顶商家 进入店铺会带上isStoreID

                if (isStoreID > 0)
                {
                    type = 0;
                    platStore = PlatStoreBLL.SingleModel.GetPlatStore(myCardId, type);
                }
                else
                {
                    platStore = PlatStoreBLL.SingleModel.GetPlatStore(storeId, type);
                }
            }

            if (platStore == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            if (platStore.State == -1)
            {
                returnObj.Msg = "店铺无效(未支付)";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            

            if (type != 2)
            {
                //这里的appID是平台的
                listPlatMyCardIds = PlatMyCardBLL.SingleModel.GetCardIds(r.Id, appId).Split(',').ToList();
                if (!listPlatMyCardIds.Contains(platStore.MyCardId.ToString()))
                {
                    returnObj.Msg = "店铺不存在(平台换绑了小程序,请重新入驻绑定)";
                    return Json(returnObj, JsonRequestBehavior.AllowGet);
                }
            }

            


            //店铺关联的权限表数据
            XcxAppAccountRelation storexcxrelation = _xcxAppAccountRelationBLL.GetModel(platStore.Aid);
            if (storexcxrelation != null)
            {
                platStore.AppId = storexcxrelation.AppId;
            }

            //TODO 后续流量大了再放入redis然后定时去更新
            platStore.StorePV++;
            //访客人数
            platStore.StoreUV = PlatUserFavoriteMsgBLL.SingleModel.GetVisitorCount(platStore.MyCardId, platStore.BindPlatAid, (int)PointsDataType.店铺, "", "");
            platStore.FavoriteCount = PlatUserFavoriteMsgBLL.SingleModel.GetStoreFavoriteCount(r.Id, platStore.Id);

            PlatUserFavoriteMsg platUserFavoriteMsg = PlatUserFavoriteMsgBLL.SingleModel.GetUserFavoriteMsg(r.Id, platStore.Id, userId, (int)PointsActionType.收藏, (int)PointsDataType.店铺);
            platStore.Favorited = (platUserFavoriteMsg != null && platUserFavoriteMsg.State != -1 ? 1 : 0);

            platStore.CategoryName = PlatStoreCategoryBLL.SingleModel.GetModel(platStore.Category).Name;

            int tjCount = 0;

            List<PlatChildGoods> listTjGoods = listTjGoods = PlatChildGoodsBLL.SingleModel.GetListByRedis(platStore.Aid, ref tjCount, string.Empty, 0, 0, 1, 1, 4, "TopState desc,VirtualSalesCount+SalesCount desc,sort desc");

            //if (type == 0)
            //{

            //    listTjGoods = _platChildGoodsBLL.GetListByRedis(platStore.Aid, ref tjCount, string.Empty, 0, 0, 1, 1, 4, "TopState desc,VirtualSalesCount+SalesCount desc,sort desc");
            //}
            //else
            //{
            //    listTjGoods = _platChildGoodsBLL.GetListByRedis(r.Id, ref tjCount, string.Empty, 0, 0, 1, 1, 4, "TopState desc,VirtualSalesCount+SalesCount desc,sort desc");
            //}
            if (listTjGoods != null && listTjGoods.Count > 0)
            {
                listTjGoods.ForEach(x =>
                {
                    platStore.TjGoods.Add(new TjGoods()
                    {
                        Aid = x.AId,
                        Id = x.Id,
                        Name = x.Name,
                        Img = x.Img,
                        PriceStr = (x.PriceFen * 0.01).ToString("0.00"),
                        SaleCount = (x.SalesCount + x.VirtualSalesCount),
                        TopState = x.TopState
                    });

                });

            }

            PlatApplyApp platApplyApp = PlatApplyAppBLL.SingleModel.GetPlatApplyAppByStoreId(platStore.Id);
            if (platApplyApp != null)
            {
                platStore.AppState = platApplyApp.OpenState;
            }

            if (!PlatStoreBLL.SingleModel.Update(platStore, "StorePV"))
            {
                returnObj.Msg = "更新店铺浏览量异常";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            //PlatMsgviewFavoriteShare msgview = _platMsgviewFavoriteShareBLL.GetModelByMsgId(platStore.BindPlatAid, platStore.Id,(int)PointsDataType.店铺);
            //if(msgview!=null)
            //{
            //    platStore.StorePV = msgview.ViewCount;
            //}

            PlatMyCard platMyCard = PlatMyCardBLL.SingleModel.GetModel(platStore.MyCardId);
            if (platMyCard != null)
            {
                platStore.storeOwner = new StoreOwner()
                {
                    UserId = platMyCard.UserId,
                    Name = platMyCard.Name,
                    Avatar = platMyCard.ImgUrl,
                    State = platMyCard.State,
                };
            }

            if (!string.IsNullOrEmpty(platStore.StoreService))
            {
                platStore.StoreServiceModelList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StoreServiceModel>>(platStore.StoreService);
            }
            else
            {
                List<StoreServiceModel> list = new List<StoreServiceModel>();
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "WIFI" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "停车位" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "支付宝支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "微信支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "刷卡支付" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "空调雅座" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "付费停车" });
                list.Add(new StoreServiceModel() { ServiceState = true, ServiceName = "接送服务" });
                platStore.StoreServiceModelList = list;
            }

            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                platStore.SwitchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
            }
            else
            {
                platStore.SwitchModel = new PlatStoreSwitchModel();
            }

            platStore.StorePV += platStore.StoreVirtualPV;
            returnObj.dataObj = new
            {
                platStore = platStore

            };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取平台店铺类别配置
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreCategoryLevel()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            returnObj.dataObj = PlatStoreCategoryConfigBLL.SingleModel.GetModelByAid(r.Id);
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取店铺列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreList(double lat = 0.00, double lng = 0.00)
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }


            string keyMsg = Utility.IO.Context.GetRequest("keyMsg", string.Empty);
            int orderType = Utility.IO.Context.GetRequestInt("orderType", 0);//默认为0  1表示按照距离由近到远 2表示默认按照时间降序
            int categoryId = Utility.IO.Context.GetRequestInt("categoryId", 0);//默认为0 表示获取所有 其它表示获取某个类别
            int pageSize = Utility.IO.Context.GetRequestInt("pageSize", 10);
            int pageIndex = Utility.IO.Context.GetRequestInt("pageIndex", 1);

            int isBigType = Utility.IO.Context.GetRequestInt("isBigType", 0);//点击的是否为大类,如果为1则表示点击大类获取其下的小类
            int cityCode = Utility.IO.Context.GetRequestInt("cityCode", 0);//哪个地区 默认为0 获取全部地区

            

            //表示按照距离最近的
            //表示没有传坐标 通过客户端IP获取经纬度
            
            if (lat == 0 || lng == 0)
            {
                string IP = Utility.WebHelper.GetIP();

                IPToPoint iPToPoint = CommondHelper.GetLoctionByIP(IP);
                if (iPToPoint != null)
                {

                    lat = iPToPoint.result.location.lat;
                    lng = iPToPoint.result.location.lng;
                    //log4net.LogHelper.WriteInfo(this.GetType(), $"IP={IP};{lat},{lng}");
                }
            }



            int totalCount = 0;
            List<PlatStore> list = PlatStoreBLL.SingleModel.GetListStore(r.Id, out totalCount, categoryId, keyMsg, lat, lng, cityCode, orderType, pageSize, pageIndex, isBigType, r.AppId);

            returnObj.dataObj = new { totalCount = totalCount, list = list };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }

        /// <summary>
        /// 申请独立小程序
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyStoreApp()
        {
            returnObj = new Return_Msg_APP();
            int userid = Context.GetRequestInt("userid", 0);
            if (userid <= 0)
            {
                returnObj.Msg = "userid不能为0";
            }
            PlatMyCard mycard = PlatMyCardBLL.SingleModel.GetModelByUserId(userid);
            if (mycard == null)
            {
                returnObj.Msg = "名片已过期";
                return Json(returnObj);
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModelBycardid(mycard.AId, mycard.Id);
            if (store == null)
            {
                returnObj.Msg = "店铺已过期";
                return Json(returnObj);
            }

            PlatApplyApp model = new PlatApplyApp();
            model.AddTime = DateTime.Now;
            model.BindAId = mycard.AId;
            model.CustomerName = mycard.Name;
            model.MycardId = mycard.Id;
            model.OpenState = 0;
            model.StoreId = store.Id;
            model.UpdateTime = DateTime.Now;
            model.Id = Convert.ToInt32(PlatApplyAppBLL.SingleModel.Add(model));
            returnObj.isok = model.Id > 0;
            returnObj.Msg = returnObj.isok ? "提交成功，正在审核中" : "提交失败";

            return Json(returnObj);
        }

        public ActionResult GetStoreGoods()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);

            if (string.IsNullOrEmpty(appId))
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int pageSize = Context.GetRequestInt("pageSize", 10);
            int isFirstType = Context.GetRequestInt("isFirstType", 1);
            int categoryId = Context.GetRequestInt("categoryId", 0);
            string goodsName = Utility.IO.Context.GetRequest("goodsName", string.Empty);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<PlatChildGoods> listGoods = PlatStoreBLL.SingleModel.GetSyncGoods(r.Id, out totalCount, string.Empty, goodsName, 1, string.Empty, pageSize, pageIndex, isFirstType, categoryId,appId:r.AppId);

            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            returnObj.dataObj = new { totalCount = totalCount, list = listGoods };
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取店铺小程序二维码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStoreCodeImg()
        {
            returnObj = new Controllers.Return_Msg_APP();
            int storeid = Context.GetRequestInt("storeid", 0);
            string pageurl = Context.GetRequest("pageurl", "");
            if (storeid <= 0)
            {
                returnObj.Msg = "无效店铺ID";
                return Json(returnObj);
            }

            string msg = "";
            if (string.IsNullOrEmpty(pageurl))
            {
                //子模版店铺二维码
                returnObj.dataObj = PlatStoreBLL.SingleModel.GetStoreCode(storeid, ref msg);
                if (msg.Length > 0)
                {
                    returnObj.Msg = msg;
                    return Json(returnObj);
                }
            }
            else
            {
                //平台店铺二维码
                returnObj.dataObj = PlatStoreBLL.SingleModel.GetStoreCode(storeid, pageurl, ref msg);
                if (msg.Length > 0)
                {
                    returnObj.Msg = msg;
                    return Json(returnObj);
                }
            }


            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 店铺续费
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddStoreTime()
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string appId = Context.GetRequest("appId", string.Empty);
            int storeId = Context.GetRequestInt("storeId", 0);
            int ruleId = Context.GetRequestInt("ruleId", 0);

            if (string.IsNullOrEmpty(appId) || storeId <= 0 || ruleId <= 0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }

            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            PlatStore platStore = PlatStoreBLL.SingleModel.GetModel(storeId);
            if (platStore == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            if (platStore.State == -1)
            {
                returnObj.Msg = "店铺不可用";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            PlatStoreAddRules rule = PlatStoreAddRulesBLL.SingleModel.getRule(r.Id, ruleId);
            if (rule == null)
            {
                returnObj.Msg = "续费套餐不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            CityMorders order = new CityMorders()
            {
                FuserId = platStore.Id,
                Fusername = platStore.Name,
                TuserId = 0,
                OrderType = (int)ArticleTypeEnum.PlatStoreAddTimePay,
                ActionType = (int)miniAppBuyMode.微信支付,
                Addtime = DateTime.Now,
                Percent = 99,//不收取服务费
                userip = WebHelper.GetIP(),
                payment_status = 0,
                Status = 0,
                CitySubId = 0,//无分销,默认为0
                PayRate = 1,
                appid = appId,
                Articleid = rule.Id,
                CommentId = rule.YearCount,
                MinisnsId = r.Id,
                payment_free = rule.CostPrice,
                ShowNote = $"平台版店铺续期{rule.YearCount}月收费付款{rule.CostPrice * 0.01}元"

            };
            string no = WxPayApi.GenerateOutTradeNo();
            order.orderno = no;
            order.trade_no = no;

            int orderid = Convert.ToInt32(_cityMordersBLL.Add(order));
            if (orderid <= 0)
            {
                returnObj.Msg = "续费失败(生成订单失败)";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            returnObj.dataObj = orderid;
            returnObj.isok = true;
            returnObj.Msg = "续费成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);

        }



    }
}