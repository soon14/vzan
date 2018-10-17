using Api.MiniApp.Filters;
using BLL.MiniApp;
using BLL.MiniApp.Conf;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using log4net;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;
using Utility.IO;

namespace Api.MiniApp.Controllers
{
    public class apiDistributionController : InheritController
    {
    }
    public class apiMiniAppDistributionController : apiDistributionController
    {
        private readonly static object _dadaLock = new object();
        private readonly static object _uuLock = new object();
        
        /// <summary>
        /// 实例化对象
        /// </summary>
        public apiMiniAppDistributionController()
        {
           
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetDadaFreight()
        {
            #region 达达下单参数
            string cityname = Context.GetRequest("cityname", string.Empty);
            string appid = Context.GetRequest("appid", string.Empty);
            string openid = Context.GetRequest("openid", string.Empty);
            string lat = Context.GetRequest("lat", string.Empty);
            string lnt = Context.GetRequest("lnt", string.Empty);
            string acceptername = Context.GetRequest("acceptername", string.Empty);
            string accepterphone = Context.GetRequest("accepterphone", string.Empty);
            string address = Context.GetRequest("address", string.Empty);
            #endregion
            string msg = "";
            returnObj = new Return_Msg_APP() { isok = false };
            int getway = (int)miniAppOrderGetWay.达达配送;

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if(xcxrelation==null)
            {
                returnObj.Msg = "请先授权 umodel_null";
                return Json(returnObj);
            }

            int storeId = 0;
            int tmpType = _xcxAppAccountRelationBLL.GetXcxTemplateType(xcxrelation.Id);
            switch(tmpType)
            {
                case (int)TmpType.小程序专业模板:break;
                case (int)TmpType.小程序餐饮模板:
                    Food food = FoodBLL.SingleModel.GetModelByAppId(xcxrelation.Id);
                    if(food==null)
                    {
                        returnObj.Msg = "找不到餐饮店铺";
                        return Json(returnObj);
                    }
                    getway=food.DistributionWay;
                    break;
            }

            ResultReponseModel result = new ResultReponseModel();
            result.deliverFeeInt = DistributionApiConfigBLL.SingleModel.Getpeisongfei(cityname,appid,openid, lat, lnt,acceptername,accepterphone,address, ref msg, getway,storeId,xcxrelation.Id,0);
            result.deliverFee = result.deliverFeeInt * 0.01;
            returnObj.isok = !(msg.Length > 0);
            returnObj.Msg = msg;
            returnObj.dataObj = result;
            return Json(returnObj);
        }

        /// <summary>
        /// 达达订单状态改变回调接口
        /// </summary>
        /// <param name="orderform"></param>
        /// <returns></returns>
        public ActionResult Dadanotis(OrderReponseModel orderform)
        {
            if (orderform == null)
            {
                LogHelper.WriteInfo(this.GetType(), "达达订单回调请求参数为空");
                return Content("fail");
            }

            DadaOrder order = _dadaOrderBLL.GetModelByOrderNo(orderform.order_id);
            if (order == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"达达订单回调:找不到订单【{orderform.order_id}】");
            }
            //如果状态一样，说明已发送过一次
            if (order.state == orderform.order_status)
            {
                return Content("success");
            }

            order.state = orderform.order_status;
            order.update_time = _dadaApi.GetDateTimeByStamp(orderform.update_time);
            order.cancel_from = orderform.cancel_from;
            order.cancel_reason = orderform.cancel_reason;
            order.dm_id = orderform.dm_id;
            order.dm_mobile = orderform.dm_mobile;
            order.dm_name = orderform.dm_name;

            DadaOrderRelation oRelationModel = DadaOrderRelationBLL.SingleModel.GetModelUOrderNo(order.origin_id);
            if (oRelationModel == null)
            {
                LogHelper.WriteInfo(this.GetType(), "达达配送回调查询达达订单管理数据出错：" + order.origin_id);
                return Content("fail");
            }

            XcxAppAccountRelation xcxrelation = _xcxAppAccountRelationBLL.GetModel(oRelationModel.dataid);
            if (xcxrelation == null)
            {
                LogHelper.WriteInfo(this.GetType(), "达达配送回调查询权限表：没有找到ID【" + oRelationModel.dataid + "】数据");
                return Content("fail");
            }

            bool isSuccess = false;

            try
            {
                lock (_dadaLock)
                {
                    switch (oRelationModel.ordertype)
                    {
                        case (int)TmpType.小程序餐饮模板:
                            isSuccess = _dadaOrderBLL.DadaToFoodReturn(oRelationModel.orderid, order, xcxrelation.AppId);
                            break;
                        case (int)TmpType.智慧餐厅:
                            isSuccess = _dadaOrderBLL.DadaToMutilFoodReturn(oRelationModel.orderid, order, xcxrelation.AppId);
                            break;

                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteInfo(this.GetType(), "达达配送回调异常:" + JsonConvert.SerializeObject(orderform) + ex.Message);
            }

            return Content("success");
        }

        /// <summary>
        /// 蜂鸟查询配送服务
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult QueryDelivery()
        {
            string lat = Context.GetRequest("lat", string.Empty);
            string appid = Context.GetRequest("appid", string.Empty);
            string lnt = Context.GetRequest("lnt", string.Empty);

            XcxAppAccountRelation umodel = _xcxAppAccountRelationBLL.GetModelByAppid(appid);
            if (umodel == null)
            {
                returnObj.Msg = "请先授权 umodel_null";
                return Json(returnObj);
            }

            FNStoreRelation fstorer = FNStoreRelationBLL.SingleModel.GetModelByRid(umodel.Id);
            if (fstorer == null)
            {
                returnObj.Msg = "请先绑定蜂鸟门店";
                return Json(returnObj);
            }

            FNStore fstore = FNStoreBLL.SingleModel.GetModel(fstorer.fnstoreid);
            if (fstore == null)
            {
                returnObj.Msg = "找不到蜂鸟门店数据";
                return Json(returnObj);
            }

            object data = new
            {
                chain_store_code = fstore.chain_store_code,//门店编号(支持数字、字母的组合)
                position_source = 1,//收货点坐标属性（0:未知, 1:腾讯地图, 2:百度地图, 3:高德地图）
                receiver_longitude = lnt,
                receiver_latitude = lat
            };

            string url = FNApi._querydeliveryapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);

            returnObj.isok = true;
            returnObj.Msg = "在配送范围内";

            return Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 蜂鸟订单状态改变回调接口
        /// </summary>
        /// <param name="returnModel"></param>
        /// <returns></returns>
        public ActionResult FNnotis(FNApiReponseModel<string> returnModel)
        {
            if (returnModel == null || returnModel.data == null || returnModel.data.Length == 0)
            {
                LogHelper.WriteInfo(this.GetType(), "蜂鸟回调请求参数为空");
                return Content("");
            }

            string datajson = HttpUtility.UrlDecode(returnModel.data);
            FNAccepterOrderModel data = JsonConvert.DeserializeObject<FNAccepterOrderModel>(datajson);
            string urlencodedata = returnModel.data;
            DistributionApiConfig model = DistributionApiConfigBLL.SingleModel.GetModelByRedis(FNApi._fnapi_appid);
            string signature = FNApi.SingleModel.GetSign(model.access_token, urlencodedata, returnModel.salt);

            if (returnModel.signature != signature || data == null)
            {
                LogHelper.WriteInfo(this.GetType(), "蜂鸟配送回调签名错误json:" + JsonConvert.SerializeObject(returnModel) + "，解码后：" + datajson);
                return Content("");
            }

            FNOrder order = FNOrderBLL.SingleModel.GetModelByOrderNo(data.partner_order_code);
            if (order == null)
            {
                log4net.LogHelper.WriteInfo(this.GetType(), $"蜂鸟订单回调:找不到订单【{data.partner_order_code}】");
                return Content("");
            }

            order.state = data.order_status;
            order.updatetime = DateTime.Now;
            order.carrier_driver_name = data.carrier_driver_name;
            order.carrier_driver_phone = data.carrier_driver_phone;
            order.description = data.description;
            bool updatebool = FNOrderBLL.SingleModel.Update(order, "state,updatetime,carrier_driver_name,carrier_driver_phone,description");

            if (updatebool && data.order_status == (int)FNOrderEnum.异常)
            {
                FNOrderRelation orelationmodel = FNOrderRelationBLL.SingleModel.GetModelUOrderNo(order.partner_order_code);
                if (orelationmodel == null)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "蜂鸟配送取消订单回调发起退款出错：找不到关联该订单的数据");
                    return Content("success");
                }

                bool isSuccess = false;
                switch (orelationmodel.ordertype)
                {
                    case (int)TmpType.小程序餐饮模板:
                        FoodGoodsOrder foodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel($" Id = {orelationmodel.orderid} ");// and  OrderType ={(int)miniAppFoodOrderType.店内点餐 } ");
                        if (foodGoodOrder == null)
                        {
                            LogHelper.WriteInfo(this.GetType(), "蜂鸟配送：找不到订单");
                            return Content("success");
                        }
                        //退款接口 abel
                        if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                        {
                            isSuccess = FoodGoodsOrderBLL.SingleModel.outOrder(foodGoodOrder, foodGoodOrder.State);
                        }
                        else if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                        {
                            SaveMoneySetUser userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(foodGoodOrder.UserId) ?? new SaveMoneySetUser();
                            isSuccess = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(foodGoodOrder, userSaveMoney, foodGoodOrder.State);
                        }
                        break;
                    case (int)TmpType.小程序专业模板:
                        EntGoodsOrder entorder = EntGoodsOrderBLL.SingleModel.GetModel(orelationmodel.orderid);
                        if (entorder == null)
                        {
                            LogHelper.WriteInfo(this.GetType(), "蜂鸟配送取消订单回调发起退款出错：找不到小程序订单数据");
                            return Content("success");
                        }

                        isSuccess = EntGoodsOrderBLL.SingleModel.outOrder(entorder, entorder.State, entorder.BuyMode);
                        if (!isSuccess)
                        {
                            LogHelper.WriteInfo(this.GetType(), "蜂鸟配送取消订单回调发起退款出错：执行退款服务出错");
                            return Content("success");
                        }

                        EntGoodsOrderLogBLL.SingleModel.AddLog(entorder.Id, 0, $"蜂鸟：将订单状态改为：{Enum.GetName(typeof(MiniAppEntOrderState), entorder.State)}");
                        break;
                }
            }

            return Content("");
        }
        
        /// <summary>
        /// 获取快跑者运费
        /// </summary>
        /// <returns></returns>
        [AuthCheckLoginSessionKey]
        public ActionResult GetKPZFreight()
        {
            int storeid = Context.GetRequestInt("storeid", 0);
            int aid = Context.GetRequestInt("aid", 0);
            string lat = Context.GetRequest("lat", string.Empty);
            string lnt = Context.GetRequest("lnt", string.Empty);
            int orderprice = Context.GetRequestInt("orderprice", 0);
            returnObj = new Return_Msg_APP();

            string msg = "";
            
            returnObj.dataObj =  DistributionApiConfigBLL.SingleModel.Getpeisongfei("","","",lat,lnt,"","","", ref msg,(int)miniAppOrderGetWay.快跑者配送, storeid, aid, orderprice);
            
            returnObj.isok = !(msg.Length>0);
            returnObj.Msg = msg;

            return Json(returnObj);
        }

        /// <summary>
        /// 快跑者订单状态改变回调接口
        /// </summary>
        /// <param name="returnModel"></param>
        /// <returns></returns>
        public ActionResult KPZnotis(KPZApiReponseModel returnModel)
        {
            if (returnModel == null)
            {
                LogHelper.WriteInfo(this.GetType(), "快跑者配送订单回调参数出错");
            }
            LogHelper.WriteInfo(this.GetType(), "快跑者配送订单回调" + JsonConvert.SerializeObject(returnModel));

            
            KPZOrder order = KPZOrderBLL.SingleModel.GetModelByOrderNo(returnModel.trade_no);
            if (order == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"快跑者订单回调:找不到订单【{returnModel.trade_no}】");
            }
            //如果状态一样，说明已发送过一次
            if (order.status == returnModel.state)
            {
                return Content("success");
            }

            order.status = returnModel.state;
            order.UpdateTime = returnModel.update_time;
            order.courier = returnModel.courier;
            order.tel = returnModel.tel;

            bool isSuccess = false;
            try
            {
                lock (_dadaLock)
                {
                    switch (order.TemplateType)
                    {
                        case (int)TmpType.小程序餐饮模板:
                            isSuccess = KPZOrderBLL.SingleModel.KPZToFoodReturn(order);
                            break;
                        case (int)TmpType.智慧餐厅:
                            isSuccess = KPZOrderBLL.SingleModel.KPZToMutilFoodReturn(order);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteInfo(this.GetType(), "快跑者配送回调异常:" + JsonConvert.SerializeObject(returnModel) + ex.Message);
            }

            return Content("success");
        }
        
        /// <summary>
        /// UU订单状态改变回调接口
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult UUnotis(string data)
        {
            LogHelper.WriteInfo(this.GetType(), $"UU配送：订单回调{data}" );
            if (string.IsNullOrEmpty(data))
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：参数出错（回调）");
                return Content("fail");
            }

            UUOrderCallBackResult returnModel = JsonConvert.DeserializeObject<UUOrderCallBackResult>(data);
            if (returnModel == null)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送：参数转化失败（回调）");
                return Content("fail");
            }

            UUOrder order = UUOrderBLL.SingleModel.GetModelByOrderCode(returnModel.order_code);
            if (order == null)
            {
                LogHelper.WriteInfo(this.GetType(), $"UU配送：找不到订单【{returnModel.order_code}】（回调）");
                return Content("fail");
            }
            //如果状态一样，说明已发送过一次
            if (order.State == Convert.ToInt32(returnModel.state))
            {
                return Content("success");
            }

            order.State = Convert.ToInt32(returnModel.state);
            if (!string.IsNullOrEmpty(returnModel.driver_name))
            {
                order.driver_name = returnModel.driver_name;
                order.driver_mobile = returnModel.driver_mobile;
                order.driver_jobnum = returnModel.driver_jobnum;
            }

            bool isSuccess = false;
            try
            {
                lock (_uuLock)
                {
                    switch (order.TemplateType)
                    {
                        case (int)TmpType.小程序餐饮模板:
                            isSuccess = UUOrderBLL.SingleModel.UUToFoodReturn(order);
                            break;
                        case (int)TmpType.智慧餐厅:
                            isSuccess = UUOrderBLL.SingleModel.UUToMutilFoodReturn(order);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteInfo(this.GetType(), "UU配送:回调异常" + JsonConvert.SerializeObject(returnModel) + ex.Message);
            }

            return Content("success");
        }
    }
}