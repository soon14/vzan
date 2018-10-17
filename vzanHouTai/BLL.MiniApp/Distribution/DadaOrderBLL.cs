using BLL.MiniApp.Conf;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Helper;
using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.CoreHelper;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Utility;

namespace BLL.MiniApp
{
    public class DadaOrderBLL : BaseMySql<DadaOrder>
    {
        
        private DadaApi _dadaapi = new DadaApi();

        
        

        public DadaOrder GetModelByOrderNo(string uoderno)
        {
            return GetModel($"origin_id='{uoderno}'");
        }

        public List<DadaOrder> GetListByQOderId(string qorderids)
        {
            return GetList($"origin_id in ({qorderids})");
        }
        
        /// <summary>
        /// 达达推单
        /// </summary>
        /// <param name="data">订单</param>
        /// <param name="sourceid">商户号编号</param>
        /// <param name="raddorder">是否预发单:0：新增订单，1:重发订单，2：预发单</param>
        /// <returns></returns>
        public DadaApiReponseModel<ResultReponseModel> AddOrder(DadaOrder data,string sourceid,int raddorder)
        {
            DadaApiReponseModel<ResultReponseModel> reposemodel = new DadaApiReponseModel<ResultReponseModel>();
            _dadaapi._sourceid = sourceid;
            _dadaapi._shop_no = data.shop_no;
            
            string json = _dadaapi.PostParamJson(data);
            string url = _dadaapi._addorderapi;
            if (raddorder == 1)
            {
                url = _dadaapi._readdorderapi;
            }
            else if (raddorder == 2)
            {
                url = _dadaapi._querydeliverfeeorderapi;
            }

            string result = HttpHelper.DoPostJson(url, json);
            if (!string.IsNullOrEmpty(result))
            {
                reposemodel = JsonConvert.DeserializeObject<DadaApiReponseModel<ResultReponseModel>>(result);
                //if (raddorder == 2)
                //{
                //    RedisUtil.Set(string.Format("_dadaRaddorderkey", data.origin_id), reposemodel.result.deliveryNo, TimeSpan.FromMinutes(3));
                //}
            }

            return reposemodel;
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <returns></returns>
        public DadaApiReponseModel<ResultReponseModel> UpdateOrderState(string order_id,int state,string sourceid)
        {
            _dadaapi._sourceid = sourceid;
            DadaApiReponseModel<ResultReponseModel> reposemodel = new DadaApiReponseModel<ResultReponseModel>();
            reposemodel.status = "fail";
            reposemodel.msg = "执行出错";

            if (state == -1)
            {
                reposemodel.msg = "订单状态出错";
                return reposemodel;
            }

            object data = new object();
            string url = string.Empty;
            switch (state)
            {
                case 0://取消订单
                    data = new { order_id = order_id, cancel_reason_id = 1, cancel_reason = "" };
                    url = _dadaapi._cancelorderapi;
                    break;
                case 1://查询订单
                    data = new { order_id = order_id };
                    url = _dadaapi._orderqueryapi;
                    break;
                case 2://接收订单
                    data = new { order_id = order_id };
                    url = _dadaapi._acceptorderapi;
                    break;
                case 3://完成取货
                    data = new { order_id = order_id };
                    url = _dadaapi._fetchgoodapi;
                    break;
                case 4://完成订单
                    data = new { order_id = order_id };
                    url = _dadaapi._finishorderapi;
                    break;
                case 5://订单过期
                    data = new { order_id = order_id };
                    url = _dadaapi._expireorderapi;
                    break;
                case 6://取消追加订单
                    data = new { order_id = order_id };
                    url = _dadaapi._cancelappendorderapi;
                    break;
                case 7://执行预发单后发单
                    string deliveryNo = RedisUtil.Get<string>(string.Format("_dadaRaddorderkey", order_id));
                    data = new { deliveryNo = deliveryNo };
                    url = _dadaapi._addafterqueryorderapi;
                    break;
            }

            string json = _dadaapi.PostParamJson(data);
            string result = HttpHelper.DoPostJson(url, json);
            if(!string.IsNullOrEmpty(result))
            {
                reposemodel = JsonConvert.DeserializeObject<DadaApiReponseModel<ResultReponseModel>>(result);
            }

            return reposemodel;
        }

        /// <summary>
        /// 添加达达订单
        /// </summary>
        /// <param name="rid">权限表ID</param>
        /// <param name="userid">用户ID</param>
        /// <param name="orderid">订单ID</param>
        /// <param name="cityname">订单所在城市名</param>
        /// <param name="price">价格（分）</param>
        /// <param name="receivername">收货人姓名</param>
        /// <param name="receivertel">收货人电话</param>
        /// <param name="address">收货地址</param>
        /// <param name="lat">收货地址纬度</param>
        /// <param name="lnt">收货地址经度</param>
        /// <param name="desc">备注</param>
        /// <param name="ordertype">看枚举TmpType</param>
        /// <returns></returns>
        public string AddDadaOrder(DistributionApiModel model)
        {
            string msg = "";
            DadaRelation relation = DadaRelationBLL.SingleModel.GetModelByRid(model.aid);
            if(relation !=null)
            {
                DadaMerchant merchant = DadaMerchantBLL.SingleModel.GetModel(relation.merchantid);
                if(merchant!=null)
                {
                    DadaCity city = DadaCityBLL.SingleModel.GetModelName(model.cityname);
                    if(city==null)
                    {
                        msg = "物流到不了城市" + model.cityname;
                        return msg;
                    }

                    DadaShop shop = DadaShopBLL.SingleModel.GetModelByMId(merchant.id);
                    if (shop == null)
                    {
                        msg = "物流到不了城市" + model.cityname;
                        return msg;
                    }

                    TransactionModel tran = new TransactionModel();
                    string timestamp = _dadaapi.GetTimeStamp();
                    string callback = _dadaapi._ordercallback;
                    string order_id = model.userid + DateTime.Now.ToString("yyyyMMddHHmmss");
                    string shopno = shop.origin_shop_id;
                    float buyprice = model.buyprice / 100.0f;

                    DadaOrder data = new DadaOrder(order_id, city.cityCode, shopno, buyprice, model.accepterName, model.accepterTelePhone, model.address, model.lat, model.lnt, model.remark, timestamp, callback);
                    //data.id = Convert.ToInt32(base.Add(data));
                    tran.Add(base.BuildAddSql(data));

                    DadaOrderRelation orderrelation = new DadaOrderRelation();
                    orderrelation.dataid = model.aid;
                    orderrelation.orderid = model.orderid;
                    orderrelation.ordertype = model.temptype;
                    orderrelation.uniqueorderno = order_id;

                    //orderrelation.id = Convert.ToInt32(DadaOrderRelationBLL.SingleModel.Add(orderrelation));
                    tran.Add(DadaOrderRelationBLL.SingleModel.BuildAddSql(orderrelation));

                    if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
                    {
                        msg = "添加达达物流出错";
                    }
                }
                else
                {
                    msg = "找不到商户号";
                }
            }
            else
            {
                msg = "找不到管理表";
            }
            
            return msg;
        }

        /// <summary>
        /// 获取达达订单对象
        /// </summary>
        /// <param name="merchantid">商户表ID</param>
        /// <param name="userid">用户ID</param>
        /// <param name="order_id">订单</param>
        /// <param name="cityname">订单所在城市名</param>
        /// <param name="price">价格（元）</param>
        /// <param name="receivername">收货人姓名</param>
        /// <param name="receivertel">收货人电话</param>
        /// <param name="address">收货地址</param>
        /// <param name="lat">收货地址纬度</param>
        /// <param name="lnt">收货地址经度</param>
        /// <param name="desc">备注</param>
        /// <returns></returns>
        public DadaOrder GetDadaOrderModel(int merchantid,int userid, int orderid, string cityname, float price, string receivername, string receivertel, string address, double lat, double lnt, string desc,ref string msg)
        {
            DadaOrder data = new DadaOrder();
            DadaCity city = DadaCityBLL.SingleModel.GetModelName(cityname);
            if (city == null)
            {
                msg = "物流到不了城市" + cityname;
                return new DadaOrder();
            }

            DadaShop shop = DadaShopBLL.SingleModel.GetModelByMId(merchantid);
            if (shop == null)
            {
                msg = "没有找到店铺:" + merchantid;
                return new DadaOrder();
            }
            TransactionModel tran = new TransactionModel();
            string timestamp = _dadaapi.GetTimeStamp();
            string callback = _dadaapi._ordercallback;
            string order_id = userid + DateTime.Now.ToString("yyyyMMddHHmmss");
            string shopno = shop.origin_shop_id;
            data = new DadaOrder(order_id, city.cityCode, shopno, price, receivername, receivertel, address,lat, lnt, desc,timestamp, callback );
            return data;
        }

        /// <summary>
        /// 获取修改达达订单状态sql
        /// </summary>
        /// <param name="orderid">小程序订单表ID</param>
        /// <param name="rid"></param>
        /// <param name="ordertype">看枚举TmpType</param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public string GetDadaOrderUpdateSql(int orderid, int rid,int ordertype,ref TransactionModel tran,bool gettransql=false)
        {
            DadaOrderRelation model = DadaOrderRelationBLL.SingleModel.GetModelOrder(rid,orderid, ordertype);
            if(model==null)
            {
                return "达达配送：没有找到订单关联数据";
            }

            DadaOrder order = GetModelByOrderNo(model.uniqueorderno);
            if(order==null)
            {
                return "达达配送：订单不存在";
            }

            DadaMerchant merchant = DadaMerchantBLL.SingleModel.GetModelByRId(rid);
            if (merchant == null || merchant.id <= 0)
            {
                return "达达配送：找不到商户数据";
            }


            DadaApiReponseModel<ResultReponseModel> result = AddOrder(order, merchant.sourceid, 0);
            if(result==null)
            {
                return "达达配送：新增订单接口异常";
            }
            //log4net.LogHelper.WriteInfo(this.GetType(),JsonConvert.SerializeObject(result));
            if(result.status== "success")
            {
                order.state = (int)DadaOrderEnum.推单中;
                if (gettransql)
                {
                    return base.ExecuteNonQuery($"update dadaorder set state={order.state} where id={order.id}")>0?"":"修改达达订单状态出错";
                }
                else
                {
                    tran.Add($"update dadaorder set state={order.state} where id={order.id}");
                }
                
                return "";
            }

            return result.msg;
        }

        /// <summary>
        /// 获取专业版达达订单状态
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public void GetDadaOrderState(ref List<EntGoodsOrder> list,int rid)
        {
            if (list == null || list.Count <= 0)
                return;

            string orderis = string.Join(",",list.Where(w=>w.GetWay == (int)miniAppOrderGetWay.达达配送).Select(s=>s.Id));
            if (!string.IsNullOrEmpty(orderis))
            {
                List<DadaOrderRelation> relist = DadaOrderRelationBLL.SingleModel.GetListByOrderIds(orderis,(int)TmpType.小程序专业模板,rid);
                if (relist == null || relist.Count <= 0)
                    return;
                
                DadaRelation merchantrelation = DadaRelationBLL.SingleModel.GetModelByRid(rid);
                if(merchantrelation==null)
                    return;
                
                DadaMerchant merchantmodel = DadaMerchantBLL.SingleModel.GetModel(merchantrelation.merchantid);
                if (merchantmodel == null)
                    return;

                string dadaqids = "'" + string.Join("','",relist.Select(s=>s.uniqueorderno)) + "'";
                if (string.IsNullOrEmpty(dadaqids))
                    return;
                
                List<DadaOrder> dadaorderlist = GetListByQOderId(dadaqids);
                if (dadaorderlist == null || dadaorderlist.Count <= 0)
                    return;

                foreach (EntGoodsOrder orderitem in list)
                {
                    DadaOrderRelation dadarelation = relist.Where(w => w.orderid == orderitem.Id).FirstOrDefault();
                    if(dadarelation!=null)
                    {
                        DadaOrder dadaorder = dadaorderlist.Where(w => w.origin_id == dadarelation.uniqueorderno).FirstOrDefault();
                        if(dadaorder!=null)
                        {
                            orderitem.State = dadaorder.state;
                            orderitem.DadaOrderStateStr = Enum.GetName(typeof(DadaOrderEnum), orderitem.State);
                            orderitem.sourceid = merchantmodel.sourceid;
                            orderitem.dadaorderid = dadarelation.uniqueorderno;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取餐饮版达达订单状态
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public void GetFoodDadaOrderState(ref List<FoodAdminGoodsOrder> list, int rid)
        {
            if (list == null || list.Count <= 0)
                return;

            string orderis = string.Join(",", list.Where(w => w.GetWay == (int)miniAppOrderGetWay.达达配送).Select(s => s.Id));
            if (!string.IsNullOrEmpty(orderis))
            {
                List<DadaOrderRelation> relist = DadaOrderRelationBLL.SingleModel.GetListByOrderIds(orderis,(int)TmpType.小程序餐饮模板, rid);
                if (relist == null || relist.Count <= 0)
                    return;

                DadaRelation merchantrelation = DadaRelationBLL.SingleModel.GetModelByRid(rid);
                if (merchantrelation == null)
                    return;

                DadaMerchant merchantmodel = DadaMerchantBLL.SingleModel.GetModel(merchantrelation.merchantid);
                if (merchantmodel == null)
                    return;

                string dadaqids = "'" + string.Join("','", relist.Select(s => s.uniqueorderno)) + "'";
                if (string.IsNullOrEmpty(dadaqids))
                    return;

                List<DadaOrder> dadaorderlist = GetListByQOderId(dadaqids);
                if (dadaorderlist == null || dadaorderlist.Count <= 0)
                    return;

                foreach (FoodAdminGoodsOrder orderitem in list)
                {
                    DadaOrderRelation dadarelation = relist.Where(w => w.orderid == orderitem.Id).FirstOrDefault();
                    if (dadarelation != null)
                    {
                        DadaOrder dadaorder = dadaorderlist.Where(w => w.origin_id == dadarelation.uniqueorderno).FirstOrDefault();
                        if (dadaorder != null)
                        {
                            orderitem.State = dadaorder.state;
                            orderitem.DadaOrderStateStr = Enum.GetName(typeof(DadaOrderEnum), orderitem.State);
                            orderitem.sourceid = merchantmodel.sourceid;
                            orderitem.dadaorderid = dadarelation.uniqueorderno;
                        }
                    }
                }
            }
        }

        public void GetFoodDadaOrderState<T>(ref List<T> list, int rid,int ordertype = (int)TmpType.小程序餐饮模板)
        {
            if (list == null || list.Count <= 0)
                return;
            List<string> orderids = new List<string>();
            foreach (T item in list)
            {
                int sendWay = Convert.ToInt32(item.GetType().GetProperty("GetWay").GetValue(item));
                if(sendWay==(int)miniAppOrderGetWay.达达配送)
                {
                    string orderid =item.GetType().GetProperty("Id").GetValue(item).ToString();
                    orderids.Add(orderid);
                }
            }
            string orderis = string.Join(",", orderids);
            if (!string.IsNullOrEmpty(orderis))
            {
                List<DadaOrderRelation> relist = DadaOrderRelationBLL.SingleModel.GetListByOrderIds(orderis, ordertype, rid);
                if (relist == null || relist.Count <= 0)
                    return;

                DadaRelation merchantrelation = DadaRelationBLL.SingleModel.GetModelByRid(rid);
                if (merchantrelation == null)
                    return;

                DadaMerchant merchantmodel = DadaMerchantBLL.SingleModel.GetModel(merchantrelation.merchantid);
                if (merchantmodel == null)
                    return;

                string dadaqids = "'" + string.Join("','", relist.Select(s => s.uniqueorderno)) + "'";
                if (string.IsNullOrEmpty(dadaqids))
                    return;

                List<DadaOrder> dadaorderlist = GetListByQOderId(dadaqids);
                if (dadaorderlist == null || dadaorderlist.Count <= 0)
                    return;

                foreach (T orderitem in list)
                {
                    int orderid = Convert.ToInt32(orderitem.GetType().GetProperty("Id").GetValue(orderitem).ToString());
                    DadaOrderRelation dadarelation = relist.Where(w => w.orderid == orderid).FirstOrDefault();
                    if (dadarelation != null)
                    {
                        DadaOrder dadaorder = dadaorderlist.Where(w => w.origin_id == dadarelation.uniqueorderno).FirstOrDefault();
                        if (dadaorder != null)
                        {
                            orderitem.GetType().GetProperty("dadastate").SetValue(orderitem, dadaorder.state);
                            orderitem.GetType().GetProperty("DadaOrderStateStr").SetValue(orderitem, Enum.GetName(typeof(DadaOrderEnum), dadaorder.state));
                            orderitem.GetType().GetProperty("sourceid").SetValue(orderitem, merchantmodel.sourceid);
                            orderitem.GetType().GetProperty("dadaorderid").SetValue(orderitem, dadarelation.uniqueorderno);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 达达运费
        /// </summary>
        /// <param name="cityName"></param>
        /// <param name="appid"></param>
        /// <param name="openid"></param>
        /// <param name="lat"></param>
        /// <param name="lnt"></param>
        /// <param name="accepterName"></param>
        /// <param name="accepterPhone"></param>
        /// <param name="address"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int GetDadaFee(string cityName, string appid, string openid, string accepterName, string accepterPhone, string address,string lat,string lnt, ref string msg)
        {
            if (string.IsNullOrEmpty(appid))
            {
                msg = "appid不能为空";
                return 0;
            }
            if(!string.IsNullOrEmpty(address))
            {
                //再请求一次腾讯地图，获取准确的坐标
                AddressApi addressModel = AddressHelper.GetLngAndLatByAddress(address);
                if (addressModel != null && addressModel.result != null && addressModel.result.location != null)
                {
                    lnt = addressModel.result.location.lng.ToString();
                    lat = addressModel.result.location.lat.ToString();
                }
                else
                {
                    msg = "达达配送：获取腾讯地址失败";
                    return 0;
                }
            }

            XcxAppAccountRelation umodel = XcxAppAccountRelationBLL.SingleModel.GetModelByAppid(appid);
            if (umodel == null)
            {
                msg = "达达配送：权限模板不能为空";
                return 0;
            }
            C_UserInfo userInfo = C_UserInfoBLL.SingleModel.GetModelByAppId_OpenId(appid, openid);
            if (userInfo == null)
            {
                msg = "达达配送：用户不存在";
                return 0;
            }
            if (accepterName == null || accepterName.Length == 0)
            {
                msg = "达达配送：收货人姓名不能为空";
                return 0;
            }
            if (accepterPhone == null || accepterPhone.Length == 0)
            {
                msg = "达达配送：收货人手机号不能为空";
                return 0;
            }
            if (address == null || address.Length == 0)
            {
                msg = "达达配送：收货人地址不能为空";
                return 0;
            }

            DadaMerchant merchant = DadaMerchantBLL.SingleModel.GetModelByRId(umodel.Id);
            if (merchant == null || merchant.id <= 0)
            {
                msg = "达达配送：商户还没开通达达配送";
                return 0;
            }

            try
            {
                DadaOrder dadaOrderModel = GetDadaOrderModel(merchant.id, userInfo.Id, 0, cityName, 0, accepterName, accepterPhone, address, Convert.ToDouble(lat), Convert.ToDouble(lnt), "", ref msg);
                if (dadaOrderModel == null || !string.IsNullOrEmpty(msg))
                {
                    msg = $"达达配送：获取物流运费失败：" + msg;
                    return 0;
                }

                DadaApiReponseModel<ResultReponseModel> postData = AddOrder(dadaOrderModel, merchant.sourceid, 2);
                if (postData != null && postData.result != null)
                {
                    return Convert.ToInt32(postData.result.deliverFee * 100);
                }

                log4net.LogHelper.WriteInfo(this.GetType(), "达达运费出错：" + JsonConvert.SerializeObject(postData) + ",请求参数=" + JsonConvert.SerializeObject(dadaOrderModel));

                msg = "获取运费出错";
            }
            catch (Exception ex)
            {
                msg = ex.Message + ",msg" + msg;
            }

            return 0;
        }

        /// <summary>
        /// 系统单个订单查询达达订单状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="order"></param>
        /// <param name="rid"></param>
        public void GetFoodDadaOrderDetailState<T>(ref T order,int rid)
        {
            int sendWay = Convert.ToInt32(order.GetType().GetProperty("GetWay").GetValue(order));
            
            if (sendWay != (int)miniAppOrderGetWay.达达配送)
            {
                return;
            }
            object objId = order.GetType().GetProperty("Id").GetValue(order);
            if (objId == null || DBNull.Value == objId)
            {
                return;
            }

            int orderId = Convert.ToInt32(objId);
            DadaOrderRelation remodel = DadaOrderRelationBLL.SingleModel.GetModelOrder(rid,orderId, (int)TmpType.小程序餐饮模板);
            if (remodel == null)
                return;
            
            DadaOrder dadaOrder = GetModelByOrderNo(remodel.uniqueorderno);
            if (dadaOrder != null)
            {
                order.GetType().GetProperty("dadastate").SetValue(order, dadaOrder.state);
                order.GetType().GetProperty("DadaOrderStateStr").SetValue(order, Enum.GetName(typeof(DadaOrderEnum), dadaOrder.state));
                order.GetType().GetProperty("dadaorderid").SetValue(order, remodel.uniqueorderno);
            }
        }
        
        /// <summary>
        /// 处理餐饮达达配送回调
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool DadaToFoodReturn(int orderid,DadaOrder order,string appid)
        {
            bool isSuccess=false;
            string updatesql = "state,update_time";
            FoodGoodsOrder foodGoodOrder = FoodGoodsOrderBLL.SingleModel.GetModel(orderid);// and  OrderType ={(int)miniAppFoodOrderType.店内点餐 } ");
            if (foodGoodOrder == null)
            {
                LogHelper.WriteInfo(this.GetType(), "达达配送：找不到订单");
                return isSuccess;
            }

            string msg = string.Empty;
            switch (order.state)
            {
                case (int)DadaOrderEnum.已取消:
                    if (order.state == (int)DadaOrderEnum.已取消)
                    {
                        updatesql += ",cancel_from,cancel_reason";
                    }
                    
                    isSuccess = base.Update(order, updatesql);
                    if(!isSuccess)
                    {
                        LogHelper.WriteInfo(this.GetType(), "达达配送回调修改系统订单状态出错:" + JsonConvert.SerializeObject(order));
                        return isSuccess;
                    }

                    //退款接口 abel
                    //判断是否是取消订单，取消订单则要执行退款
                    if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.微信支付)
                    {
                        isSuccess = FoodGoodsOrderBLL.SingleModel.outOrder(foodGoodOrder, foodGoodOrder.State);
                    }
                    else if (foodGoodOrder.BuyMode == (int)miniAppBuyMode.储值支付)
                    {
                        var userSaveMoney = SaveMoneySetUserBLL.SingleModel.getModelByUserId(foodGoodOrder.UserId) ?? new SaveMoneySetUser();
                        isSuccess = FoodGoodsOrderBLL.SingleModel.outOrderBySaveMoneyUser(foodGoodOrder, userSaveMoney, foodGoodOrder.State);
                    }

                    #region 餐饮退款成功通知 模板消息
                    if(isSuccess)
                    {
                        var postData2 = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(foodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮退款成功通知);
                        TemplateMsg_Miniapp.SendTemplateMessage(foodGoodOrder.UserId, SendTemplateMessageTypeEnum.餐饮退款成功通知, (int)TmpType.小程序餐饮模板, postData2);
                    }
                    
                    #endregion
                    return isSuccess;
                case (int)DadaOrderEnum.待接单:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待接单;
                    break;
                case (int)DadaOrderEnum.待取货:
                    updatesql += ",dm_id,dm_name,dm_mobile";
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待送餐;
                    //object groupData = TemplateMsg_Miniapp.DadaGetTemplateMessageData(order,foodGoodOrder.PayDate,foodGoodOrder.BuyMode,"", SendTemplateMessageTypeEnum.达达配送接单通知);
                    #region 发送餐饮订单配送通知 模板消息
                    C_UserInfo userinfo = C_UserInfoBLL.SingleModel.GetModel(foodGoodOrder.UserId) ?? new C_UserInfo();
                    object postData = FoodGoodsOrderBLL.SingleModel.getTemplateMessageData(foodGoodOrder.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知);
                    TemplateMsg_Miniapp.SendTemplateMessage(userinfo.Id, SendTemplateMessageTypeEnum.餐饮订单配送通知, (int)TmpType.小程序餐饮模板, postData);
                    #endregion
                    break;
                case (int)DadaOrderEnum.配送中:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.待确认送达;
                    break;
                case (int)DadaOrderEnum.已完成:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.已完成;
                    break;
                case (int)DadaOrderEnum.已过期:
                case (int)DadaOrderEnum.系统故障订单发布失败:
                case (int)DadaOrderEnum.妥投异常之物品返回完成:
                    foodGoodOrder.State = (int)miniAppFoodOrderState.退款审核中;
                    updatesql += ",cancel_from,cancel_reason";
                    break;
            }
            isSuccess = base.Update(order, updatesql);
            if (!isSuccess)
            {
                LogHelper.WriteInfo(this.GetType(), "达达配送回调修改系统订单状态出错:" + JsonConvert.SerializeObject(order));
                return isSuccess;
            }
            isSuccess = FoodGoodsOrderBLL.SingleModel.Update(foodGoodOrder, "State");

            return isSuccess;
        }

        /// <summary>
        /// 处理智慧餐厅达达配送回调
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool DadaToMutilFoodReturn(int orderid, DadaOrder order, string appid)
        {
            bool isSuccess = false;
            string updatesql = "state,update_time";
            DishOrder dishOrder = DishOrderBLL.SingleModel.GetModel(orderid);// and  OrderType ={(int)miniAppFoodOrderType.店内点餐 } ");
            if (dishOrder == null)
            {
                LogHelper.WriteInfo(this.GetType(), "智慧餐厅达达配送：找不到订单");
                return isSuccess;
            }

            string msg = string.Empty;
            switch (order.state)
            {
                case (int)DadaOrderEnum.已取消:
                    if (order.state == (int)DadaOrderEnum.已取消)
                    {
                        updatesql += ",cancel_from,cancel_reason";
                    }

                    isSuccess = base.Update(order, updatesql);
                    if (!isSuccess)
                    {
                        LogHelper.WriteInfo(this.GetType(), "智慧餐厅达达配送回调修改系统订单状态出错:" + JsonConvert.SerializeObject(order));
                        return isSuccess;
                    }

                    //退款接口 abel
                    //判断是否是取消订单，取消订单则要执行退款
                    if (dishOrder.pay_status == (int)DishEnums.PayState.已付款)
                    {
                        DishReturnMsg result = new DishReturnMsg();
                        DishOrderBLL.SingleModel.RefundOrderById(orderid, result);
                        isSuccess = result.code == 1;
                        if(isSuccess)
                        {
                            LogHelper.WriteInfo(this.GetType(), result.msg);
                        }
                    }
                    
                    return isSuccess;
                case (int)DadaOrderEnum.待接单:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.待商家确认;
                    break;
                case (int)DadaOrderEnum.待取货:
                    updatesql += ",dm_id,dm_name,dm_mobile";
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.待取货;
                    dishOrder.peisong_open = 1;
                    dishOrder.peisong_user_name = order.dm_name;
                    dishOrder.peisong_user_phone = order.dm_mobile;
                    #region 发送餐饮订单配送通知 模板消息

                    //C_UserInfo userinfo = _userInfoBLL.GetModel(dishOrder.user_id) ?? new C_UserInfo();
                    //TemplateMsg_UserParam tmgup = _templateMsg_UserParamBLL.getParamByAppIdOpenId(appid, userinfo.OpenId) ?? new TemplateMsg_UserParam();
                    //TemplateMsg_User tmgu = _templateMsg_UserBLL.getModelByAppIdTypeId(tmgup.AppId, (int)TmpType.小程序餐饮模板, (int)SendTemplateMessageTypeEnum.餐饮订单配送通知) ?? new Entity.MiniApp.Conf.TemplateMsg_User();
                    //if (tmgu.State == 1)
                    //{
                    //    object postData = _foodGoodsOrderBLL.getTemplateMessageData(dishOrder.id, SendTemplateMessageTypeEnum.餐饮订单配送通知);
                    //    _msnModelHelper.sendMyMsn(userinfo.appId, userinfo.OpenId, tmgu.TemplateId, tmgu.PageUrl, tmgup.Form_Id, postData, string.Empty, string.Empty, ref msg);
                    //    //参数使用次数增加(默认是1)
                    //    _templateMsg_UserParamBLL.addUsingCount(tmgup);
                    //}
                    #endregion
                    break;
                case (int)DadaOrderEnum.配送中:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.配送中;
                    break;
                case (int)DadaOrderEnum.已完成:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.已完成;
                    break;
                case (int)DadaOrderEnum.已过期:
                case (int)DadaOrderEnum.系统故障订单发布失败:
                case (int)DadaOrderEnum.妥投异常之物品返回完成:
                    dishOrder.peisong_status = (int)DishEnums.DeliveryState.已取消;
                    updatesql += ",cancel_from,cancel_reason";
                    break;
            }
            isSuccess = base.Update(order, updatesql);
            if (!isSuccess)
            {
                LogHelper.WriteInfo(this.GetType(), "达达配送回调修改系统订单状态出错:" + JsonConvert.SerializeObject(order));
                return isSuccess;
            }
            isSuccess = DishOrderBLL.SingleModel.Update(dishOrder, "peisong_status,peisong_open,peisong_user_name,peisong_user_phone");

            return isSuccess;
        }
    }
}