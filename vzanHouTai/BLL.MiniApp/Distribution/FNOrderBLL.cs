using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Fds;
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
    public class FNOrderBLL : BaseMySql<FNOrder>
    {
        #region 单例模式
        private static FNOrderBLL _singleModel;
        private static readonly object SynObject = new object();

        private FNOrderBLL()
        {

        }

        public static FNOrderBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new FNOrderBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        public FNOrder GetModelByOrderNo(string uoderno)
        {
            return GetModel($"partner_order_code='{uoderno}'");
        }

        public List<FNOrder> GetListByQOderId(string qorderids)
        {
            return GetList($"partner_order_code in ({qorderids})");
        }

        /// <summary>
        /// 生成蜂鸟物流订单
        /// </summary>
        /// <param name="rid">权限表ID</param>
        /// <param name="userinfo">用户信息</param>
        /// <param name="orderid">订单ID</param>
        /// <param name="goodcars">购物车</param>
        /// <param name="price">订单价格</param>
        /// <param name="receviename">收货人姓名</param>
        /// <param name="receviephone">收货人电话</param>
        /// <param name="address">收货人地址</param>
        /// <param name="lat">纬度</param>
        /// <param name="lnt">经度</param>
        /// <param name="ordertype">看枚举TmpType</param>
        /// <returns></returns>
        public string AddFNOrder(List<elegood> fngoods, DistributionApiModel model)
        {
            string msg = "";
            FNStoreRelation relation = FNStoreRelationBLL.SingleModel.GetModelByRid(model.aid);
            if (relation != null)
            {
                msg = "请先配置蜂鸟门店";
                return msg;
            }
            FNStore fnstore = FNStoreBLL.SingleModel.GetModel(relation.fnstoreid);
            if (fnstore != null)
            {
                msg = "没有找到蜂鸟门店信息";
                return msg;
            }
            if (fngoods == null || fngoods.Count <= 0)
            {
                msg = "商品信息不能为空";
                return msg;
            }

            TransactionModel tran = new TransactionModel();
            long timestamp = Convert.ToInt64(FNApi.SingleModel.GetTimeStamp());
            string callback = FNApi._callbackurl;
            string orderno = model.userid + DateTime.Now.ToString("yyyyMMddHHmmss");
            string storecode = fnstore.chain_store_code;
            float buyprice = model.buyprice / 100.0f;
            int goodcount = 0;

            //收货人信息
            FNReceiverInfo reciverifo = new FNReceiverInfo()
            {
                receiver_name = model.accepterName,
                receiver_primary_phone = model.accepterTelePhone,
                receiver_address = model.address,
                receiver_longitude = model.lnt,
                receiver_latitude = model.lat,
            };

            //门店信息
            FNStoreInfo storeinfo = new FNStoreInfo() {
                transport_name = fnstore.chain_store_name,
                transport_address = fnstore.address,
                transport_longitude = Convert.ToDouble(fnstore.longitude),
                transport_latitude = Convert.ToDouble(fnstore.latitude),
                transport_tel = fnstore.contact_phone,
            };
            
            //生成订单实体类对象
            FNOrder data = new FNOrder(storecode, goodcount, callback, buyprice, timestamp, buyprice, orderno, model.remark, fngoods,reciverifo,storeinfo);
            tran.Add(base.BuildAddSql(data));

            //订单与系统订单关联
            FNOrderRelation orderrelation = new FNOrderRelation();
            orderrelation.dataid = model.aid;
            orderrelation.orderid = model.orderid;
            orderrelation.ordertype = model.temptype;
            orderrelation.uniqueorderno = orderno;
            
            tran.Add(FNOrderRelationBLL.SingleModel.BuildAddSql(orderrelation));

            if (!ExecuteTransactionDataCorect(tran.sqlArray, tran.ParameterArray))
            {
                msg = "添加蜂鸟物流出错";
            }
            return msg;
        }

        /// <summary>
        /// 蜂鸟推单
        /// </summary>
        /// <param name="data">订单</param>
        /// <returns></returns>
        public FNApiReponseModel<object> AddOrderToFN(FNOrder data)
        {
            FNApiReponseModel<object> reposemodel = new FNApiReponseModel<object>();

            string url = FNApi._addorderapi;
            string result = FNApi.SingleModel.UseFNApi(data, url);
            
            if (!string.IsNullOrEmpty(result))
            {
                reposemodel = JsonConvert.DeserializeObject<FNApiReponseModel<object>>(result);
            }

            return reposemodel;
        }

        /// <summary>
        /// 获取修改蜂鸟订单状态sql
        /// </summary>
        /// <param name="orderid">小程序订单表ID</param>
        /// <param name="rid"></param>
        /// <param name="ordertype">TmpType枚举</param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public string GetFNOrderUpdateSql(int orderid, int rid, int ordertype, ref TransactionModel tran, bool gettransql = false)
        {
            FNOrderRelation model = FNOrderRelationBLL.SingleModel.GetModelOrder(rid, orderid, ordertype);
            if (model == null)
            {
                return "没有找到蜂鸟订单关联数据";
            }

            FNOrder order = GetModelByOrderNo(model.uniqueorderno);
            if (order == null)
            {
                return "蜂鸟订单不存在";
            }
            
            FNApiReponseModel<object> result = AddOrderToFN(order);
            if (result == null)
            {
                return "蜂鸟新增订单接口异常";
            }

            if (result.code == 200)
            {
                order.state = (int)FNOrderEnum.推单中;

                if (gettransql)
                {
                    return base.ExecuteNonQuery($"update fnorder set state={order.state},updatetime=now() where id={order.id}") > 0 ? "" : "修改蜂鸟订单状态出错";
                }
                else
                {
                    tran.Add($"update fnorder set state={order.state},updatetime=now() where id={order.id}");
                }

                return "";
            }

            return result.msg;
        }

        /// <summary>
        /// 获取餐饮版蜂鸟订单状态
        /// </summary>
        /// <param name="list"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public void GetFoodFNOrderState(ref List<FoodAdminGoodsOrder> list, int rid)
        {
            if (list == null || list.Count <= 0)
                return;

            string orderis = string.Join(",", list.Where(w => w.GetWay == (int)miniAppOrderGetWay.达达配送).Select(s => s.Id));
            if (!string.IsNullOrEmpty(orderis))
            {
                List<FNOrderRelation> relist = FNOrderRelationBLL.SingleModel.GetListByOrderIds(orderis, (int)TmpType.小程序餐饮模板, rid);
                if (relist == null || relist.Count <= 0)
                    return;
                
                string dadaqids = "'" + string.Join("','", relist.Select(s => s.uniqueorderno)) + "'";
                if (string.IsNullOrEmpty(dadaqids))
                    return;

                List<FNOrder> dadaorderlist = GetListByQOderId(dadaqids);
                if (dadaorderlist == null || dadaorderlist.Count <= 0)
                    return;

                foreach (FoodAdminGoodsOrder orderitem in list)
                {
                    FNOrderRelation dadarelation = relist.Where(w => w.orderid == orderitem.Id).FirstOrDefault();
                    if (dadarelation != null)
                    {
                        FNOrder dadaorder = dadaorderlist.Where(w => w.partner_order_code == dadarelation.uniqueorderno).FirstOrDefault();
                        if (dadaorder != null)
                        {
                            orderitem.State = dadaorder.state;
                            orderitem.DadaOrderStateStr = Enum.GetName(typeof(FNOrderEnum), orderitem.State);
                            orderitem.dadaorderid = dadarelation.uniqueorderno;
                        }
                    }
                }
            }
        }
    }
}