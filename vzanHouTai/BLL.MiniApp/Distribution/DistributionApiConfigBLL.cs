using BLL.MiniApp.Dish;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Dish;
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
    public class DistributionApiConfigBLL : BaseMySql<DistributionApiConfig>
    {
        #region 单例模式
        private static DistributionApiConfigBLL _singleModel;
        private static readonly object SynObject = new object();

        private DistributionApiConfigBLL()
        {

        }

        public static DistributionApiConfigBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DistributionApiConfigBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion
        private static readonly string _eleapi_token_key = "eleapi_token_{0}";

        public DistributionApiConfig GetModelByRedis(string appid)
        {
            string key = string.Format(_eleapi_token_key, appid);
            DistributionApiConfig model = RedisUtil.Get<DistributionApiConfig>(key);
            if (model == null)
            {
                model = GetModel($"app_id='{appid}'");
                if (model == null)
                {
                    log4net.LogHelper.WriteInfo(this.GetType(), "蜂鸟配送获取配置出错，找不到配置");
                    return new DistributionApiConfig();
                }
            }

            try
            {
                //判断token是否已失效
                if (model.updatetime.AddMinutes(model.refreshtime) < DateTime.Now || string.IsNullOrEmpty(model.access_token))
                {
                    //获取token接口链接
                    string tokenurl = FNApi.SingleModel.GetTokenApiUrl();
                    string result = HttpHelper.GetData(tokenurl);
                    if (string.IsNullOrEmpty(result))
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "获取蜂鸟配送获token出错，返回值：" + result);
                        return new DistributionApiConfig();
                    }

                    FNApiReponseModel<DistributionApiConfig> eleModel = JsonConvert.DeserializeObject<FNApiReponseModel<DistributionApiConfig>>(result);
                    if (eleModel.code != 200 || eleModel.data==null)
                    {
                        log4net.LogHelper.WriteInfo(this.GetType(), "获取蜂鸟配送获token出错，返回值：" + result);
                        return new DistributionApiConfig();
                    }

                    //保存token
                    DistributionApiConfig tempModel = eleModel.data;
                    if(tempModel != null)
                    {
                        if(model==null || model.id<=0)
                        {
                            tempModel.addtime = DateTime.Now;
                            tempModel.updatetime = DateTime.Now;
                            tempModel.refreshtime = 60;
                            base.Add(tempModel);
                            model = tempModel;
                        }
                        else
                        {
                            model.access_token = tempModel.access_token;
                            model.expire_time = tempModel.expire_time;
                            model.updatetime = DateTime.Now;
                            base.Update(model, "access_token,expire_time,updatetime");
                        }
                        RedisUtil.Set<DistributionApiConfig>(key, model, TimeSpan.FromMinutes(model.refreshtime));
                    }

                }
            }
            catch (Exception ex)
            {
                log4net.LogHelper.WriteError(this.GetType(), ex);
                return new DistributionApiConfig();
            }
            
            return model;
        }

        /// <summary>
        /// 物流平台对接
        /// </summary>
        /// <param name="rid">权限表ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="cityName">城市名（达达）</param>
        /// <param name="lat">纬度</param>
        /// <param name="lnt">经度</param>
        /// <param name="distributionType">2：达达，3：蜂鸟</param>
        /// <param name="order">订单</param>
        /// <param name="carsData">购物车</param>
        /// <param name="orderType">TmpType枚举</param>
        /// <returns></returns>
        public string AddDistributionOrder(int rid, int userId, string cityName, string latStr, string lntStr, int distributionType, object order, object carsData, int tempType,int storeId = 0,int fee=0,int orderType=0)
        {
            if (distributionType <= 1)
            {
                return "";
            }

            string dugmsg = "";
            DistributionApiModel model = new DistributionApiModel();
            model.aid = rid;
            model.userid = userId;
            model.temptype = tempType;
            model.storeid = storeId;
            model.fee = fee;
            model.cityname = cityName;
            model.ordertype = orderType;
            double lat = 0;
            if (!double.TryParse(latStr, out lat))
            {
                dugmsg = "纬度转换错误";
                return dugmsg;
            }
            if (lat <= 0)
            {
                dugmsg = "纬度不能小于0";
                return dugmsg;
            }
            double lnt = 0;
            if (!double.TryParse(lntStr, out lnt))
            {
                dugmsg = "经度转换错误";
                return dugmsg;
            }
            if (lnt <= 0)
            {
                dugmsg = "经度不能小于0";
                return dugmsg;
            }
            model.lat = lat;
            model.lnt = lnt;
            List<elegood> fngoods = new List<elegood>();

            try
            {
                //物流数据提取
                switch (tempType)
                {
                    case (int)TmpType.小程序餐饮模板:
                        FoodGoodsOrder data = (FoodGoodsOrder)order;
                        model.orderid = data.Id;
                        model.buyprice = data.BuyPrice;
                        model.accepterName = data.AccepterName;
                        model.accepterTelePhone = data.AccepterTelePhone;
                        model.address = data.Address;
                        model.remark = data.Message;
                        if(carsData!=null)
                        {
                            List<FoodGoodsCart> goodsCar = (List<FoodGoodsCart>)carsData;
                            //商品信息
                            foreach (FoodGoodsCart good in goodsCar)
                            {
                                model.ordercontent += good.goodsMsg.GoodsName + $"({good.Count});";
                            }
                        }
                        dugmsg = "数据提取成功";
                        break;
                    case (int)TmpType.智慧餐厅:
                        DishOrder disOrder = (DishOrder)order;
                        model.orderid = disOrder.id;
                        model.buyprice = Convert.ToInt32(disOrder.order_amount*100);
                        model.accepterName = disOrder.consignee;
                        model.accepterTelePhone = disOrder.mobile;
                        model.address = disOrder.address;
                        model.remark = disOrder.post_info;
                        List<DishShoppingCart> dishGoodsCartList = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(disOrder.id);
                        if(dishGoodsCartList!=null && dishGoodsCartList.Count>0)
                        {
                            //商品信息
                            foreach (DishShoppingCart good in dishGoodsCartList)
                            {
                                model.ordercontent += good.goods_name + $"({good.goods_number});";
                            }
                        }

                        dugmsg = "数据提取成功";
                        break;
                }
                //添加物流订单
                switch (distributionType)
                {
                    case (int)miniAppOrderGetWay.达达配送://生成达达物流订单
                        DadaOrderBLL dadaOrderBLL = new DadaOrderBLL();
                        dugmsg = dadaOrderBLL.AddDadaOrder(model);
                        if (!string.IsNullOrEmpty(dugmsg))
                        {
                            return $"达达订单生成失败：" + dugmsg;
                        }
                        break;
                    case (int)miniAppOrderGetWay.蜂鸟配送://生成蜂鸟物流订单
                        
                        dugmsg = FNOrderBLL.SingleModel.AddFNOrder(fngoods,model);
                        if (!string.IsNullOrEmpty(dugmsg))
                        {
                            return "蜂鸟订单生成失败：" + dugmsg;
                        }
                        break;
                    case (int)miniAppOrderGetWay.快跑者配送://生成快跑者物流订单
                        
                        dugmsg = KPZOrderBLL.SingleModel.AddKPZOrder(model);
                        if (!string.IsNullOrEmpty(dugmsg))
                        {
                            return "快跑者订单生成失败：" + dugmsg;
                        }
                        break;
                    case (int)miniAppOrderGetWay.UU配送://生成uu物流订单
                        
                        dugmsg = UUOrderBLL.SingleModel.AddUUOrder(model);
                        if (!string.IsNullOrEmpty(dugmsg))
                        {
                            return "UU订单生成失败：" + dugmsg;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                dugmsg += "物流数据转化失败：" + ex.Message;
                log4net.LogHelper.WriteInfo(this.GetType(), dugmsg);
                return dugmsg;
            }

            return dugmsg;
        }

        /// <summary>
        /// 获取配送费
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
        /// <param name="dtype"></param>
        /// <returns></returns>
        public int Getpeisongfei(string cityName,string appid,string openid,string lat,string lnt,string accepterName,string accepterPhone,string address,ref string msg,int getWay,int storeId=0,int aid=0,int orderPrice=0)
        {
            int fee = 0;
            try
            {
                switch (getWay)
                {
                    case (int)miniAppOrderGetWay.达达配送:
                        fee = new DadaOrderBLL().GetDadaFee(cityName, appid, openid, accepterName, accepterPhone, address,lat,lnt, ref msg);
                        break;
                    case (int)miniAppOrderGetWay.快跑者配送:
                        fee = KPZOrderBLL.SingleModel.GetKPZFee(storeId, aid, address, lat, lnt, orderPrice, ref msg);
                        fee = fee * 100;
                        break;
                    case (int)miniAppOrderGetWay.UU配送:
                        UUOrderFee uuResult = UUOrderBLL.SingleModel.GetUUFee(storeId,aid,address,ref msg);
                        fee = uuResult!=null? uuResult.Fee:0;
                        break;
                }
            }
            catch (Exception)
            {
                msg = "无效配送，请检查配送团队是否正常";//快跑者配送团队欠费是获取不到运费的
            }
            
            return fee;
        }

        /// <summary>
        /// 推单
        /// </summary>
        /// <param name="orderid"></param>
        /// <param name="aid"></param>
        /// <param name="tmptype"></param>
        /// <param name="getway"></param>
        /// <param name="tran"></param>
        /// <param name="getsql"></param>
        /// <param name="storeid"></param>
        /// <returns></returns>
        public string UpdatePeiSongOrder(int orderid,int aid,int tmptype,int getway,ref TransactionModel tran,bool getsql,int storeid=0)
        {
            switch (getway)
            {
                case (int)miniAppOrderGetWay.达达配送:
                    return new DadaOrderBLL().GetDadaOrderUpdateSql(orderid,aid, tmptype, ref tran, getsql);
                case (int)miniAppOrderGetWay.蜂鸟配送:
                    return FNOrderBLL.SingleModel.GetFNOrderUpdateSql(orderid,aid, tmptype, ref tran, getsql);
                case (int)miniAppOrderGetWay.快跑者配送:
                    return KPZOrderBLL.SingleModel.GetKPZOrderUpdateSql(orderid, aid,storeid, ref tran, getsql);
                case (int)miniAppOrderGetWay.UU配送:
                    return UUOrderBLL.SingleModel.GetUUOrderUpdateSql(orderid, aid, storeid, ref tran, getsql);
            }

            return "";
        }
    }
}