using Core.MiniApp;
using DAL.Base;
using Entity.MiniApp;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Pin;
using Entity.MiniApp.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.MiniApp.Tools
{
    public class DeliveryFeedbackBLL : BaseMySql<DeliveryFeedback>
    {
        #region 单例模式
        private static DeliveryFeedbackBLL _singleModel;
        private static readonly object SynObject = new object();

        private DeliveryFeedbackBLL()
        {

        }

        public static DeliveryFeedbackBLL SingleModel
        {
            get
            {
                if (_singleModel == null)
                {
                    lock (SynObject)
                    {
                        if (_singleModel == null)
                        {
                            _singleModel = new DeliveryFeedbackBLL();
                        }
                    }
                }
                return _singleModel;
            }
        }
        #endregion

        /// <summary>
        /// 单体缓存Key（{0}：orderId,{1}：orderType）
        /// </summary>
        private static readonly string cacheModelKey = "deliveryFeed_{0}_{1}";

        private bool AddDeliveryFeed(int orderId, DeliveryOrderType orderType, string contactName, string contactTel, string address, string deliveryNo, string companyCode, string companyTitle, bool isTrack = true, string mark = null)
        {
            DeliveryFeedback newOrderFeed = new DeliveryFeedback
            {
                CreateDate = DateTime.Now,
                OrderId = orderId,
                OrderType = (int)orderType,
                DeliveryNo = deliveryNo,
                FeedBack = null,
                CompanyCode = companyCode,
                Address = address,
                CompanyTitle = companyTitle,
                ContactName = contactName,
                ContactTel = contactTel,
                Status = (int)DeliveryFeedState.正常,
                IsTrack = isTrack,
                Mark = mark,
            };
            int newId = 0;
            return int.TryParse(Add(newOrderFeed).ToString(), out newId);
        }

        /// <summary>
        /// 新增物流信息（专业版发货）
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <param name="address">收货地址</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司代码</param>
        /// <param name="mark">物流备注</param>
        /// <returns></returns>
        public bool AddEntOrderFeed(int orderId, DeliveryUpdatePost AddInfo)
        {
            return AddDeliveryFeed(orderId: orderId, orderType: DeliveryOrderType.专业版订单商家发货,
                contactName: AddInfo.ContactName, contactTel: AddInfo.ContactTel, address: AddInfo.Address,
                deliveryNo: AddInfo.DeliveryNo, companyCode: AddInfo.CompanyCode, companyTitle: AddInfo.CompanyTitle, isTrack: !AddInfo.SelfDelivery,
                mark: AddInfo.Remark);
        }

        /// <summary>
        /// 新增物流信息（独立小程序版发货）
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <param name="address">收货地址</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司代码</param>
        /// <param name="mark">物流备注</param>
        /// <returns></returns>
        public bool AddPlatOrderFeed(int orderId, DeliveryUpdatePost AddInfo)
        {
            return AddDeliveryFeed(orderId: orderId, orderType: DeliveryOrderType.独立小程序订单商家发货,
                contactName: AddInfo.ContactName, contactTel: AddInfo.ContactTel, address: AddInfo.Address,
                deliveryNo: AddInfo.DeliveryNo, companyCode: AddInfo.CompanyCode, companyTitle: AddInfo.CompanyTitle, isTrack: !AddInfo.SelfDelivery,
                mark: AddInfo.Remark);
        }

        /// <summary>
        /// 新增物流信息（独立小程序版发货）
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <param name="address">收货地址</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司代码</param>
        /// <param name="mark">物流备注</param>
        /// <returns></returns>
        public bool AddOrderFeed(int orderId, DeliveryUpdatePost AddInfo, DeliveryOrderType deliveryOrderType)
        {
            return AddDeliveryFeed(orderId: orderId, orderType: deliveryOrderType,
                contactName: AddInfo.ContactName, contactTel: AddInfo.ContactTel, address: AddInfo.Address,
                deliveryNo: AddInfo.DeliveryNo, companyCode: AddInfo.CompanyCode, companyTitle: AddInfo.CompanyTitle, isTrack: !AddInfo.SelfDelivery,
                mark: AddInfo.Remark);
        }

        /// <summary>
        /// 新增物流信息（专业版换货）
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <param name="address">收货地址</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司代码</param>
        /// <param name="mark">物流备注</param>
        /// <returns></returns>
        public bool AddEntOrderFeedExchange(int orderId, string contactName, string contactTel, string address, string deliveryNo, string companyCode, string companyTitle, string mark = null)
        {
            return AddDeliveryFeed(orderId, DeliveryOrderType.专业版订单商家换货, contactName, contactTel, address, deliveryNo, companyCode, companyTitle, false, mark);
        }

        /// <summary>
        /// 新增物流信息（专业版退货）
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <param name="address">收货地址</param>
        /// <param name="deliveryNo">物流订单号</param>
        /// <param name="companyCode">物流公司代码</param>
        /// <param name="mark">物流备注</param>
        /// <returns></returns>
        public bool AddEntOrderReturnFeed(int orderId, string contactName, string contactTel, string address, string deliveryNo, string companyCode, string companyTitle, string mark = null)
        {
            return AddDeliveryFeed(orderId, DeliveryOrderType.专业版订单用户退货, contactName, contactTel, address, deliveryNo, companyCode, companyTitle, false, mark);
        }

        ///// <summary>
        ///// 物流跟踪接口
        ///// </summary>
        ///// <param name="delivery"></param>
        ///// <returns>接口返回JSON</returns>
        //public string GetDeliveryFeed(DeliveryFeedback delivery)
        //{
        //    //string apiStr = WebConfigBLL.DeliveryAPI;
        //    string paramStr = $"&com={delivery.CompanyCode}&nu={delivery.DeliveryNo}";
        //    string requestUrl = $"{apiStr}{paramStr}";
        //    return Utility.HttpHelper.GetData(requestUrl);
        //}

        public bool IsDelivery(EntGoodsOrder order)
        {
            bool isHasInfo = false;
            switch (order.State)
            {
                case (int)MiniAppEntOrderState.退款中:
                    isHasInfo = IsHasDelivery(order.Id, DeliveryOrderType.专业版订单用户退货);
                    break;
                default:
                    isHasInfo = IsHasDelivery(order.Id, DeliveryOrderType.专业版订单商家发货);
                    break;
            }
            return isHasInfo;
        }

        public int GetOrderFeedCount(int orderId, DeliveryOrderType orderType)
        {
            string whereSql = BuildWhereSql(orderId, orderType);
            return GetCount(whereSql);
        }

        public int GetOrderFeedCount(PinGoodsOrder order)
        {
            return GetOrderFeedCount(order.id, DeliveryOrderType.拼享惠订单商家发货);
        }

        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <returns></returns>
        public DeliveryFeedback GetOrderFeed(int orderId, DeliveryOrderType orderType)
        {
            //从缓存中获取
            //string cahcheKey = GetModelKey(orderId, orderType);
            //DeliveryFeedback feedCache = RedisUtil.Get<DeliveryFeedback>(cahcheKey);
            //if (feedCache != null && feedCache.Id > 0)
            //{
            //    return feedCache;
            //}
            //从数据库中获取，写入缓存
            string whereSql = BuildWhereSql(orderId, orderType);
            //int cacheInterval = WebConfigBLL.DeliveryCheckSpan;
            //获取
            DeliveryFeedback deliveryInfo = GetModel(whereSql);
            //if (deliveryInfo == null)
            //{
            //    return null;
            //}
            //写入
            //if (string.IsNullOrWhiteSpace(deliveryInfo.CompanyCode))
            //{
            //RedisUtil.Set(cacheModelKey, deliveryInfo, TimeSpan.FromMinutes(cacheInterval));
            //return deliveryInfo;
            //}
            //同步物流信息
            //deliveryInfo.FeedBack = GetDeliveryFeed(deliveryInfo);
            //Task.Factory.StartNew(() =>
            //{
            //    Update(deliveryInfo, "FeedBack");
            //});
            //RedisUtil.Set(cacheModelKey, deliveryInfo, TimeSpan.FromMinutes(cacheInterval));
            return deliveryInfo;
        }

        public string GetModelKey(int orderId, DeliveryOrderType orderType)
        {
            return string.Format(cacheModelKey, orderId, (int)orderType);
        }

        /// <summary>
        /// 订单是否有物流信息
        /// </summary>
        /// <param name="orderId">EntGoodsOrderId</param>
        /// <returns></returns>
        public bool IsHasDelivery(int orderId, DeliveryOrderType orderType)
        {
            string whereSql = BuildWhereSql(orderId, orderType);
            return GetCount(whereSql) > 0;
        }

        private string BuildWhereSql(int orderId = 0, DeliveryOrderType? type = null, DeliveryFeedState? status = null, bool getTrackable = false, string compaynCode = null, string deliveryNo = null)
        {
            string whereSql = "orderId > 0";
            if (orderId > 0)
            {
                whereSql = $"{whereSql} AND OrderId = {orderId}";
            }
            if (type.HasValue)
            {
                whereSql = $"{whereSql} AND OrderType = {(int)type.Value}";
            }
            if (status.HasValue)
            {
                whereSql = $"{whereSql} AND Status = {(int)status.Value}";
            }
            if (getTrackable)
            {
                whereSql = $"{whereSql} AND DeliveryNo <> '' AND CompanyCode <> ''";
            }
            if(!string.IsNullOrWhiteSpace(deliveryNo) && !string.IsNullOrWhiteSpace(compaynCode))
            {
                whereSql = $"{whereSql} AND DeliveryNo = '{deliveryNo}' AND CompanyCode = '{compaynCode}'";
            }
            return whereSql;
        }

        public bool IsVaildType(int value)
        {
            return Enum.IsDefined(typeof(DeliveryOrderType), value);
        }

        /// <summary>
        /// 获取待查询物流订单
        /// </summary>
        /// <param name="getCount"></param>
        /// <returns></returns>
        public List<DeliveryFeedback> GetWaitForTrace(int getCount)
        {
            string whereSql = BuildWhereSql(status: DeliveryFeedState.正常, getTrackable: true);
            return GetList(whereSql, getCount, 1);
        }

        /// <summary>
        /// 查询实时物流信息轨迹
        /// </summary>
        /// <param name="feedOrder"></param>
        /// <returns></returns>
        public DeliveryData GetRealTime(DeliveryFeedback feedOrder)
        {
            object result = DeliveryAPI.RequestRealTime(WebConfigBLL.MerchIdForDeliveryAPi, WebConfigBLL.AuthKeyForDeliveryAPI, feedOrder.DeliveryNo, feedOrder.CompanyCode);
            //object result = DeliveryAPI.RequestRealTime("1336595", "dcf8d02b-c3ed-4f48-8964-6e4ee6146a38", feedOrder.DeliveryNo, feedOrder.CompanyCode);
            return JsonConvert.DeserializeObject<DeliveryData>(result.ToString());
        }

        /// <summary>
        /// 获取待订阅物流订单
        /// </summary>
        /// <param name="getCount"></param>
        /// <returns></returns>
        public List<DeliveryFeedback> GetWaitForSubscribe(int getCount)
        {
            string whereSql = BuildWhereSql(status: DeliveryFeedState.等待, getTrackable: true);
            return GetList(whereSql, getCount, 1);
        }

        /// <summary>
        /// 添加物流订阅申请
        /// </summary>
        /// <param name="feedOrder"></param>
        /// <returns></returns>
        public DeliverySubscribeResult AddSubscribe(DeliveryFeedback feedOrder)
        {
            object result = DeliveryAPI.RequestSubscribe(WebConfigBLL.MerchIdForDeliveryAPi, WebConfigBLL.AuthKeyForDeliveryAPI, feedOrder.DeliveryNo, feedOrder.CompanyCode, callBack: feedOrder.Id.ToString());
            return JsonConvert.DeserializeObject<DeliverySubscribeResult>(result.ToString());
        }

        public DeliveryFeedback GetByCodeAndNote(string companyCode, string deliveryNo)
        {
            string whereSql = BuildWhereSql(deliveryNo: deliveryNo, compaynCode: companyCode);
            return GetModel(whereSql);
        }
    }
}
