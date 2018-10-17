using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Plat;
using BLL.MiniApp.PlatChild;
using BLL.MiniApp.Tools;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Plat;
using Entity.MiniApp.PlatChild;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utility;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    /// <summary>
    /// 平台子模版
    /// </summary>
    public partial class apiPlatController : InheritController
    {
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <returns></returns>
        public ActionResult AddGoodsCarData()
        {
            returnObj = new Return_Msg_APP();
            string attrSpacStr = Context.GetRequest("attrspacstr", "");
            //商品规格(格式)：规格1：属性1 规格2：属性2 如:（颜色：白色 尺码：M）
            string specInfo = Context.GetRequest("specinfo", "");
            string specImg = Context.GetRequest("specimg", "");
            int goodId = Context.GetRequestInt("goodid", 0);
            int userId = Context.GetRequestInt("userid", 0);
            int qty = Context.GetRequestInt("qty", 0);
            //立即购买，1：立即购买，0：添加到购物车
            int gotoBuy = Context.GetRequestInt("gotobuy", 2);
            if (qty <= 0)
            {
                returnObj.Msg = "数量必须大于0";
                return Json(returnObj);
            }

            PlatChildGoods good = PlatChildGoodsBLL.SingleModel.GetModel(goodId);
            if (good == null)
            {
                returnObj.Msg = "未找到该商品";
                return Json(returnObj);
            }
            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(good.AId);
            if(store==null)
            {
                returnObj.Msg = "店铺已关闭";
                return Json(returnObj);
            }
            if (!string.IsNullOrWhiteSpace(attrSpacStr))
            {
                //log4net.LogHelper.WriteInfo(this.GetType(),$"{JsonConvert.SerializeObject(good.GASDetailList)},{attrSpacStr}");
                if (!good.GASDetailList.Any(x => x.Id.Equals(attrSpacStr)))
                {
                    returnObj.Msg = "商品已过期";
                    return Json(returnObj);
                }
            }
            if (!(good.State == 1 && good.Tag == 1))
            {
                returnObj.Msg = "无法添加失效商品";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "用户不存在";
                return Json(returnObj);
            }

            PlatChildGoodsCart car = PlatChildGoodsCartBLL.SingleModel.GetModelBySpec(userInfo.Id, goodId, attrSpacStr,0);
            //商品价格
            int price = Convert.ToInt32(good.Price * 100);
            price = Convert.ToInt32(!string.IsNullOrWhiteSpace(attrSpacStr) ? good.GASDetailList.First(x => x.Id.Equals(attrSpacStr)).Price * 100 : good.Price * 100);
            
            if (car == null || gotoBuy==1)
            {
                car = new PlatChildGoodsCart
                {
                    //FoodId = store.Id,
                    StoreId = store.Id,
                    GoodsName = good.Name,
                    GoodsId = good.Id,
                    SpecIds = attrSpacStr,
                    Count = qty,
                    Price = price,
                    SpecInfo = specInfo,
                    SpecImg = specImg,//规格图片
                    UserId = userInfo.Id,
                    AddTime = DateTime.Now,
                    State = 0,
                    GoToBuy = gotoBuy,
                    AId = good.AId,
                };
                
                //加入购物车
                int id = Convert.ToInt32(PlatChildGoodsCartBLL.SingleModel.Add(car));
                if (id > 0)
                {
                    int cartcount = PlatChildGoodsCartBLL.SingleModel.GetCartGoodsCountByUserId(userInfo.Id);
                    returnObj.Msg = "成功";
                    returnObj.dataObj = new { id=id,count= cartcount };
                    returnObj.isok = true;
                    return Json(returnObj);
                }
            }
            else
            {
                car.Count += qty;
                if (PlatChildGoodsCartBLL.SingleModel.Update(car, "Count"))
                {
                    int cartcount = PlatChildGoodsCartBLL.SingleModel.GetCartGoodsCountByUserId(userInfo.Id);
                    returnObj.dataObj = new { id = car.Id, count = cartcount };
                    returnObj.Msg = "成功";
                    returnObj.isok = true;
                    return Json(returnObj);
                }
            }

            returnObj.Msg = "失败";
            return Json(returnObj);
        }

        /// <summary>
        /// 查询购物车
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetGoodsCarList()
        {
            returnObj = new Return_Msg_APP();
            int pageSize = Context.GetRequestInt("pageSize", 6);
            int pageIndex = Context.GetRequestInt("pageIndex", 1);
            int userId = Context.GetRequestInt("userid", 0);

            try
            {
                C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
                if (userInfo == null)
                {
                    returnObj.Msg = "用户不存在";
                    return Json(returnObj);
                }

                XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userInfo.appId);
                if (xcxrelation == null)
                {
                    returnObj.Msg = "未绑定小程序或模板已过期";
                    return Json(returnObj);
                }

                List<PlatChildGoodsCart> carList = PlatChildGoodsCartBLL.SingleModel.GetListByUserId(xcxrelation.Id, userInfo.Id, pageSize, pageIndex);

                if (carList == null || carList.Count <= 0)
                {
                    returnObj.Msg = "购物车为空";
                    return Json(returnObj);
                }

                //获取会员信息
                VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userInfo.Id);
                VipLevel levelInfo = vipInfo != null ? VipLevelBLL.SingleModel.GetModel(vipInfo.levelid) : null;

                //#region 会员打折
                carList.ForEach(g => g.OriginalPrice = g.Price);
                VipLevelBLL.SingleModel.GetVipDiscount(ref carList, vipInfo, levelInfo, userInfo.Id, "Discount", "Price");
                //#endregion 会员打折

                //获取商品详细资料
                List<PlatChildGoods> goods = new List<PlatChildGoods>();
                goods = PlatChildGoodsBLL.SingleModel.GetList($" Id in ({string.Join(",", carList.Select(x => x.GoodsId))}) ");

                if (goods == null || goods.Count<=0)
                {
                    returnObj.Msg = "商品已过期";
                    returnObj.dataObj = carList;
                    return Json(returnObj);
                }

                PlatChildGoods curGood = new PlatChildGoods();
                carList.ForEach(c =>
                {
                    curGood = goods.FirstOrDefault(g => g.Id == c.GoodsId);
                    if (curGood != null && curGood.Id > 0)
                    {
                        //多规格处理
                        if (curGood.GASDetailList != null && curGood.GASDetailList.Count > 0)
                        {
                            List<GoodsSpecDetail> detaillist = curGood.GASDetailList.ToList();
                            detaillist?.ForEach(g =>
                            {
                                g.OriginalPrice = g.Price;
                                g.Discount = c.Discount;
                                float discountPrice = g.Price * (c.Discount * 0.01F);
                                g.DiscountPrice = discountPrice < 0.01 ? 0.01F : discountPrice;
                            });
                            curGood.SpecDetail = JsonConvert.SerializeObject(detaillist);
                        }
                        curGood.Description = string.Empty;
                        c.GoodsInfo = curGood;
                    }
                });

                returnObj.dataObj = new { carlist= carList,count= carList.Sum(s=>s.Count) } ;
                returnObj.isok = true;
                return Json(returnObj);
            }
            catch (Exception ex)
            {
                returnObj.dataObj = ex;
                return Json(returnObj);
            }
        }

        /// <summary>
        /// 从购物车 删除商品
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="goodsCarId"></param>
        /// <param name="function">0为更新,-1为删除</param>
        /// <returns></returns>
        public ActionResult UpdateOrDeleteGoodsCarData()
        {
            returnObj = new Return_Msg_APP();
            string cartStr = Context.GetRequest("cartstr", "");
            int userId = Context.GetRequestInt("userid", 0);
            int type = Context.GetRequestInt("type", 0);
            if (string.IsNullOrEmpty(cartStr))
            {
                returnObj.Msg = "数据不能为空";
                return Json(returnObj);
            }
            List<PlatChildGoodsCart> goodsCarModel = JsonConvert.DeserializeObject<List<PlatChildGoodsCart>>(cartStr);
            if(userId<=0)
            {
                returnObj.Msg = "userid不能为空";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if(userInfo == null)
            {
                returnObj.Msg = "找不到用户";
                return Json(returnObj);
            }
            if (goodsCarModel == null || goodsCarModel.Count <= 0)
            {
                returnObj.Msg = "找不到购物车数据";
                return Json(returnObj);
            }

            string column = "";
            foreach (PlatChildGoodsCart item in goodsCarModel)
            {
                column = "State";
                if (type == -1)
                {
                    item.State = -1;
                }
                else if (type == 0)//根据传入参数更新购物车内容
                {
                    //价格因更改规格随之改变
                    PlatChildGoods carGoods = PlatChildGoodsBLL.SingleModel.GetModel(item.GoodsId);
                    if (carGoods == null)
                    {
                        item.State = 2;
                    }
                    else
                    {
                        column += ",Count";
                        if (!string.IsNullOrWhiteSpace(carGoods.SpecDetail))
                        {
                            float? price = carGoods.GASDetailList.Where(x => x.Id.Equals(item.SpecIds))?.FirstOrDefault()?.Price;
                            if (price != null)
                            {
                                column += ",Price";
                                item.Price = Convert.ToInt32(price * 100);
                            }
                        }
                    }
                }

                bool success = PlatChildGoodsCartBLL.SingleModel.Update(item, column);
                if (!success)
                {
                    returnObj.Msg = "失败";
                    return Json(returnObj);
                }
            }
            returnObj.Msg = "操作成功";
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 获取运费信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFreightFee(string appId = null, string openId = null)
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int storeId = Context.GetRequestInt("storeid", 0);
            string province = Context.GetRequest("province","");
            string city = Context.GetRequest("city", "");
            string goodCartIds = Context.GetRequest("goodcartids", "");

            if (userId<=0)
            {
                returnObj.Msg = "参数错误";
                return Json(returnObj);
            }
            if(string.IsNullOrWhiteSpace(goodCartIds))
            {
                returnObj.Msg = "购物车参数出错";
                return Json(returnObj);
            }
            C_UserInfo usrInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if(usrInfo == null)
            {
                returnObj.Msg = "无效用户";
                return Json(returnObj);
            }
            XcxAppAccountRelation model = _xcxAppAccountRelationBLL.GetModelByAppid(usrInfo.appId);
            if (model == null)
            {
                returnObj.Msg = "无效模板";
                return Json(returnObj);
            }

            PlatStore platStore = new PlatStore();
            if(storeId>0)
            {
                platStore = PlatStoreBLL.SingleModel.GetModel(storeId);
            }
            else
            {
                platStore = PlatStoreBLL.SingleModel.GetModelByAId(model.Id);
            }
            if (platStore == null)
            {
                returnObj.Msg = "没有找到店铺";
                return Json(returnObj);
            }
            string errorMsg = "";
            DeliveryFeeResult deliueryResult  = DeliveryTemplateBLL.SingleModel.GetPlatFee(goodCartIds, platStore.Aid, province, city, ref errorMsg);
            if (errorMsg.Length > 0)
            {
                returnObj.Msg = errorMsg;
                return Json(returnObj);
            }

            returnObj.Msg = "获取成功";
            returnObj.isok = true;
            returnObj.dataObj = new { deliueryResult= deliueryResult,storeaddress= platStore.Location} ;
            return Json(returnObj);
        }

        /// <summary>
        /// 获取订单信息列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderList()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid",0);
            int state = Context.GetRequestInt("state", 0);
            int pageIndex = Context.GetRequestInt("pageindex", 1);
            int pageSize = Context.GetRequestInt("pagesize", 10);
            if (userId<=0)
            {
                returnObj.Msg = "用户ID不能为0";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if(userInfo==null)
            {
                returnObj.Msg = "找不到用户";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userInfo.appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "无效模板";
                return Json(returnObj);
            }

            PlatStore store = PlatStoreBLL.SingleModel.GetModelByAId(xcxrelation.Id);
            if(store==null)
            {
                returnObj.Msg = "无效店铺";
                return Json(returnObj);
            }

            int platUserId = 0;
            XcxAppAccountRelation platXcxRelation = _xcxAppAccountRelationBLL.GetModel(store.BindPlatAid);
            if (platXcxRelation != null)
            {
                //平台上的用户ID
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByAppIds(userInfo.TelePhone, $"'{platXcxRelation.AppId}'");
                if(userInfoList!=null && userInfoList.Count>0)
                {
                    platUserId = userInfoList[0].Id;
                }
            }
            
            int count = 0;
            List<PlatChildGoodsOrder> list = PlatChildGoodsOrderBLL.SingleModel.GetList_Api(state,xcxrelation.Id,userId,pageSize,pageIndex,ref count, store.Id, store.BindPlatAid, platUserId);

            returnObj.isok = true;
            returnObj.dataObj = new { count = count, list = list };
            return Json(returnObj);
        }

        /// <summary>
        /// 平台上获取订单信息列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPlatOrderList()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int state = Context.GetRequestInt("state", 0);
            int pageIndex = Context.GetRequestInt("pageindex", 1);
            int pageSize = Context.GetRequestInt("pagesize", 10);
            if (userId <= 0)
            {
                returnObj.Msg = "用户ID不能为0";
                return Json(returnObj);
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModel(userId);
            if (userInfo == null)
            {
                returnObj.Msg = "找不到用户";
                return Json(returnObj);
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(userInfo.appId);
            if (xcxrelation == null)
            {
                returnObj.Msg = "无效模板";
                return Json(returnObj);
            }

            string userIds = userId.ToString();
            List<PlatStore> storeList = PlatStoreBLL.SingleModel.GetXcxRelationAppids(xcxrelation.Id);
            if (storeList != null && storeList.Count > 0)
            {
                string appids = "'" + string.Join("','", storeList.Select(s => s.AppId)) + "'";
                List<C_UserInfo> userInfoList = C_UserInfoBLL.SingleModel.GetListByAppIds(userInfo.TelePhone, appids);
                if (userInfoList != null && userInfoList.Count > 0)
                {
                    userIds += "," + string.Join(",", userInfoList.Select(s => s.Id));
                }
            }
            
            int count = 0;
            List<PlatChildGoodsOrder> list = PlatChildGoodsOrderBLL.SingleModel.GetList_Api(state, 0, 0, pageSize, pageIndex, ref count, 0, 0,0,userIds);
            if(count>0)
            {
                string storeIds = string.Join(",",list.Select(s=>s.StoreId).Distinct());
                List<PlatStore> allStores = PlatStoreBLL.SingleModel.GetListByIds(storeIds);
                foreach (PlatChildGoodsOrder item in list)
                {
                    PlatStore storeModel = allStores?.FirstOrDefault(f=>f.Id == item.StoreId);
                    item.StoreName = storeModel?.Name;
                }
            }

            returnObj.isok = true;
            returnObj.dataObj = new { count = count, list = list };
            return Json(returnObj);
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrderInfo()
        {
            returnObj = new Return_Msg_APP();
            int userId = Context.GetRequestInt("userid", 0);
            int id = Context.GetRequestInt("id", 0);
            if (userId <= 0)
            {
                returnObj.Msg = "用户ID不能为0";
                return Json(returnObj);
            }

            PlatChildGoodsOrder model = PlatChildGoodsOrderBLL.SingleModel.GetModel_Api(id);
            if(model!=null)
            {
                PlatStore store = PlatStoreBLL.SingleModel.GetModel(model.StoreId);
                if(store!=null)
                {
                    model.StorePhone = store.Phone;
                }
                model.DeliveryInfo = DeliveryFeedbackBLL.SingleModel.GetOrderFeed(orderId: model.Id, orderType: DeliveryOrderType.独立小程序订单商家发货);
            }
            
            returnObj.dataObj = model;
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateOrderState()
        {
            returnObj = new Return_Msg_APP();
            int id = Context.GetRequestInt("id",0);
            //int userId = Context.GetRequestInt("userid", 0);
            int state = Context.GetRequestInt("state", 0);
            if (id<=0)
            {
                returnObj.Msg = "订单ID不能为空";
                return Json(returnObj);
            }
            PlatChildGoodsOrder order = PlatChildGoodsOrderBLL.SingleModel.GetModel(id);
            if(order == null)
            {
                returnObj.Msg = "订单已失效";
                return Json(returnObj);
            }
            //if (order.UserId != userId)
            //{
            //    returnObj.Msg = "无效用户";
            //    return Json(returnObj);
            //}

            string msg = "";
            switch (state)
            {
                case (int)PlatChildOrderState.已取消:
                    if(order.State!= (int)PlatChildOrderState.待付款)
                    {
                        returnObj.Msg = "取消订单：无效订单状态";
                        return Json(returnObj);
                    }
                    PlatChildGoodsOrderBLL.SingleModel.CancelOrder(order, ref msg);
                    if (msg.Length > 0)
                    {
                        returnObj.Msg = msg;
                        return Json(returnObj);
                    }
                    break;
                case (int)PlatChildOrderState.已完成:
                    if (order.State != (int)PlatChildOrderState.待收货)
                    {
                        returnObj.Msg = "确认收货：无效订单状态";
                        return Json(returnObj);
                    }
                    PlatChildGoodsOrderBLL.SingleModel.ReceiptGoods(order, ref msg);
                    if (msg.Length > 0)
                    {
                        returnObj.Msg = msg;
                        return Json(returnObj);
                    }
                    break;
                default:
                    returnObj.Msg = "无效订单状态";
                    return Json(returnObj);
            }
            
            returnObj.Msg = "操作成功";
            returnObj.isok = true;
            return Json(returnObj);
        }

        /// <summary>
        /// 获取店铺产品类别配置 来进行样式的显示
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsCategoryLevel()
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

            PlatStore platStore = PlatStoreBLL.SingleModel.GetPlatStore(r.Id,2);

            if (platStore == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);

            }
            PlatStoreSwitchModel switchModel = new PlatStoreSwitchModel();
            int productCategoryLevel = 1;
            if (!string.IsNullOrEmpty(platStore.SwitchConfig))
            {
                switchModel = Newtonsoft.Json.JsonConvert.DeserializeObject<PlatStoreSwitchModel>(platStore.SwitchConfig);
                productCategoryLevel = switchModel.ProductCategoryLevel;
            }

            returnObj.dataObj = new {Level= productCategoryLevel };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 获取店铺产品类别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsCategory()
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
            int parentId = Context.GetRequestInt("parentId", 0);
            XcxAppAccountRelation r = _xcxAppAccountRelationBLL.GetModelByAppid(appId);
            if (r == null)
            {
                returnObj.Msg = "小程序未授权";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            int totalCount = 0;
            List<PlatChildGoodsCategory> list = PlatChildGoodsCategoryBLL.SingleModel.getListByaid(r.Id, out totalCount, isFirstType, pageSize, pageIndex, "sortNumber desc,addTime desc", parentId);


            returnObj.dataObj = new
            {
                totalCount = totalCount,
                list = list

            };
            returnObj.isok = true;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 产品列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="typeid"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetGoodsList(int aid, string typeid = "", int pageindex = 1, int pagesize = 10, int userId = 0, int isFirstType = -1)
        {
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";
            string goodsName = Context.GetRequest("goodsName", "");
            string priceSort = Context.GetRequest("pricesort", "");
            string entGoodTypeIds = string.Empty;

            if (!string.IsNullOrEmpty(typeid))
            {
                typeid = EncodeHelper.ReplaceSqlKey(typeid);
                typeid = Server.UrlDecode(typeid);
            }
            List<PlatChildGoods> goodslist = PlatChildGoodsBLL.SingleModel.GetListGoods(aid, goodsName, typeid, priceSort, pagesize, pageindex, isFirstType);
            //log4net.LogHelper.WriteInfo(this.GetType(), JsonConvert.SerializeObject(goodslist));
            if (goodslist != null)
            {


                goodslist.ForEach((Action<PlatChildGoods>)(goodModel =>
                {

                    if (!string.IsNullOrEmpty(goodModel.Categorys))
                    {
                        goodModel.CategorysStr = PlatChildGoodsCategoryBLL.SingleModel.GetPlatChildGoodsCategoryName(goodModel.Categorys);
                    }

                    if (!string.IsNullOrEmpty(goodModel.Plabels))
                    {
                        goodModel.PlabelStr = PlatChildGoodsLabelBLL.SingleModel.GetGoodsLabel(goodModel.Plabels);
                        goodModel.PlabelStr_Arry = goodModel.PlabelStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    List<GoodsSpecDetail> listGoodsSpecDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GoodsSpecDetail>>(goodModel.SpecDetail);
                    if (listGoodsSpecDetail != null)
                    {


                        listGoodsSpecDetail.ForEach(x =>
                        {
                            if (x.Discount == 100)
                            {
                                x.DiscountPrice = x.Price;
                            }

                        });


                        goodModel.SpecDetail = JsonConvert.SerializeObject(listGoodsSpecDetail);
                    }

                }));

                //获取会员信息
                VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userId);
                VipLevel levelinfo = vipInfo != null ? VipLevelBLL.SingleModel.GetModel(vipInfo.levelid) : null;

                //#region 会员打折

                VipLevelBLL.SingleModel.GetVipDiscount(ref goodslist, vipInfo, levelinfo, userId, "Discount", "Price");

            }
            var postdata = new
            {
                goodslist = goodslist.Select(g => new
                {
                    Id = g.Id,
                    Img = g.Img,
                    Name = g.Name,
                    Plabelstr_array = g.PlabelStr_Arry,
                    PriceFen = g.PriceFen,
                    DiscountPricestr = g.DiscountPricestr,
                    Discount = g.Discount,
                    Unit = g.Unit,
                    VirtualSalesCount = g.VirtualSalesCount,
                    SalesCount = g.SalesCount,
                    Price = g.Price
                }),
            };

            returnObj.isok = true;
            returnObj.dataObj = postdata;
            return Json(returnObj, JsonRequestBehavior.AllowGet);


        }

        public ActionResult GetGoodInfo()
        {
            int pid = Context.GetRequestInt("pid", 0);
            int userId = Context.GetRequestInt("userId",0);
            returnObj = new Return_Msg_APP();
            returnObj.code = "200";

            if (pid == 0)
            {
                returnObj.Msg = "请选择产品";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }

            PlatChildGoods goodModel = PlatChildGoodsBLL.SingleModel.GetModel(pid);
            if (goodModel == null || goodModel.State == 0)
            {
                returnObj.Msg = "产品不存在或已删除";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
             
            }

            PlatStore platStore = PlatStoreBLL.SingleModel.GetModelByAId(goodModel.AId);
            if (platStore == null)
            {
                returnObj.Msg = "店铺不存在";
                return Json(returnObj, JsonRequestBehavior.AllowGet);
            }
            
            if (!string.IsNullOrEmpty(goodModel.Plabels))
            {
                //goodModel.plabelstr = DAL.Base.SqlMySql.ExecuteScalar(Utility.dbEnum.MINIAPP.ToString(), CommandType.Text, $"SELECT group_concat(name order by sort desc) from entgoodlabel where id in ({goodModel.plabels})").ToString();
                goodModel.PlabelStr = PlatChildGoodsLabelBLL.SingleModel.GetEntGoodsLabelStr(goodModel.Plabels);
                goodModel.PlabelStr_Arry = goodModel.PlabelStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            
            #region 会员折扣显示
            //获取会员信息
            VipRelation vipInfo = VipRelationBLL.SingleModel.GetModelByUserid(userId);
            VipLevel levelinfo = vipInfo != null ? VipLevelBLL.SingleModel.GetModel(vipInfo.levelid) : null;

            List<PlatChildGoods> list = new List<PlatChildGoods>();
            list.Add(goodModel);
          //  _miniappVipLevelBll.GetVipDiscount(ref list, vipInfo, levelinfo, userId, "Discount", "Price");
            goodModel = list.FirstOrDefault();

            #endregion 会员折扣显示

            //#region 会员打折
            List<PlatChildGoodsCart> carlist = new List<PlatChildGoodsCart>() { new PlatChildGoodsCart() { GoodsId = goodModel.Id } };
            carlist.ForEach(g => g.OriginalPrice = g.Price);
            VipLevelBLL.SingleModel.GetVipDiscount(ref carlist, vipInfo, levelinfo, userId, "Discount", "Price");
            goodModel.Discount = carlist[0].Discount;
            //#endregion 会员打折
            
            if (!string.IsNullOrEmpty(goodModel.Img))
            {
                goodModel.Img = goodModel.Img.Replace("http://vzan-img.oss-cn-hangzhou.aliyuncs.com", "https://i.vzan.cc/");
            }

          List<GoodsSpecDetail> listGoodsSpecDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GoodsSpecDetail>>(goodModel.SpecDetail);
            listGoodsSpecDetail.ForEach(x =>
            {
                if (x.Discount == 100)
                {
                    x.DiscountPrice = x.Price;
                }
            });
            
            goodModel.SpecDetail = JsonConvert.SerializeObject(listGoodsSpecDetail);
            goodModel.storeModel = new StoreModel()
            {
                StoreId=platStore.Id,
                Name = platStore.Name,
                Img = platStore.StoreHeaderImg,
                Loction = platStore.Location,
                Lng = platStore.Lng,
                Lat = platStore.Lat
            };
            
            PlatMyCard platMyCard = PlatMyCardBLL.SingleModel.GetModel(platStore.MyCardId);
            if (platMyCard != null)
            {
                goodModel.storeOwner = new StoreOwner()
                {
                    UserId = platMyCard.UserId,
                    Name = platMyCard.Name,
                    Avatar = platMyCard.ImgUrl
                };

                PlatUserCash userCash = PlatUserCashBLL.SingleModel.GetModelByUserId(platMyCard.AId,platMyCard.UserId);
                if(userCash!=null)
                {
                    goodModel.storeOwner.IsOpenDistribution = userCash.IsOpenDistribution;
                }
            }

            returnObj.isok = true;
            returnObj.dataObj = goodModel;
            returnObj.Msg = "获取成功";
            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 商品一物一码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGoodsCodeUrl(int goodsId = 0, int type=0,string pageUrl = "")
        {
            returnObj = new Return_Msg_APP();
            if (goodsId <= 0)
            {
                returnObj.Msg = "无效商品ID";
                return Json(returnObj);
            }

            string msg = "";
            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = "pages/selected/goods-details/index";
            }

            //商品二维码
            returnObj.dataObj = PlatChildGoodsBLL.SingleModel.GetGoodsCodeUrl(goodsId, pageUrl, type,ref msg);
            if (msg.Length > 0)
            {
                returnObj.Msg = msg;
                return Json(returnObj);
            }

            returnObj.isok = true;
            return Json(returnObj);
        }
    }
}